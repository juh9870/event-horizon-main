﻿using System.Collections.Generic;
using System.Linq;
using Constructor;
using GameServices.GameManager;
using Session;
using Session.Model;
using Zenject;
using Constructor.Ships;
using Diagnostics;
using GameDatabase;
using GameDatabase.Enums;
using UnityEngine.Assertions;
using CommonComponents.Utils;
using Services.InAppPurchasing;

namespace GameServices.Player
{
    public sealed class PlayerFleet : GameServiceBase
    {
        [Inject]
        public PlayerFleet(
            ISessionData session, 
            SessionDataLoadedSignal dataLoadedSignal, 
            SessionCreatedSignal sessionCreatedSignal, 
            SessionAboutToSaveSignal sessionAboutToSaveSignal,
            SupporterPackPurchasedSignal supporterPackPurchasedSignal,
            IDebugManager debugManager,
            PlayerSkillsResetSignal playerSkillsResetSignal)
            : base(dataLoadedSignal, sessionCreatedSignal)
        {
            _session = session;
            _debugManager = debugManager;

            _sessionAboutToSaveSignal = sessionAboutToSaveSignal;
            _sessionAboutToSaveSignal.Event += OnSessionAboutToSave;
            _playerSkillsResetSignal = playerSkillsResetSignal;
            _playerSkillsResetSignal.Event += OnPlayerSkillsReset;
            _supporterPackPurchasedSignal = supporterPackPurchasedSignal;
            _supporterPackPurchasedSignal.Event += AddSupporterPack;

            _ships.ItemAddedEvent += OnShipAdded;
            _ships.ItemRemovedEvent += OnShipRemoved;
            _ships.EntireCollectionChangedEvent += OnShipCollectionChanged;
        }

        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly PlayerInventory _inventory;
        [Inject] private readonly IDatabase _database;

        public ShipSquad ActiveShipGroup => _activeShips;

        public IShip ExplorationShip
        {
            get => _explorationShip;
            set
            {
				Assert.IsTrue(value == null || value.Model.SizeClass == SizeClass.Frigate && _ships.Contains(value));
				_explorationShip = value;
                DataChanged = true;
            }
        }

        public IEnumerable<IShip> GetAllHangarShips()
        {
            return _activeShips.Ships;
        }

        public ObservableCollection<IShip> Ships => _ships;

        private void OnShipCollectionChanged()
        {
            DataChanged = true;
        }

        private void OnShipAdded(IShip ship)
        {
            DataChanged = true;
            _session.Statistics.UnlockShip(ship.Id);
        }

        private void OnShipRemoved(IShip ship)
        {
            DataChanged = true;
            _activeShips.Remove(ship);
        }

        public float Power { get { return ActiveShipGroup.Ships.Sum(ship => Maths.Threat.GetShipPower(ship)); } }

        protected override void OnSessionDataLoaded()
        {
            Load();
        }

        protected override void OnSessionCreated()
        {
            var components = new List<IntegratedComponent>();
            foreach (var ship in _ships)
            {
                var debug = _debugManager.CreateLog(ship.Name);
                var empty = Enumerable.Empty<IntegratedComponent>();
                if (TryRemoveInvalidComponents(new ShipLayout(ship.Model.Layout, ship.Model.Barrels, empty, debug), ship.Components, components))
	                ship.Components.Assign(components);

                if (ship.FirstSatellite != null)
                {
                    if (TryRemoveInvalidComponents(new ShipLayout(ship.FirstSatellite.Information.Layout, ship.FirstSatellite.Information.Barrels, 
                        empty, debug), ship.FirstSatellite.Components, components))
	                    ship.FirstSatellite.Components.Assign(components);
                }

                if (ship.SecondSatellite != null)
                {
                    if (TryRemoveInvalidComponents(new ShipLayout(ship.SecondSatellite.Information.Layout, ship.SecondSatellite.Information.Barrels,
                        empty, debug), ship.SecondSatellite.Components, components))
	                    ship.SecondSatellite.Components.Assign(components);
                }
            }

            _activeShips.CheckIfValid(_playerSkills, true);
        }

        private bool TryRemoveInvalidComponents(ShipLayout layout, IEnumerable<IntegratedComponent> components, IList<IntegratedComponent> validComponents)
        {
            validComponents.Clear();
            var random = new System.Random(_session.Game.Seed);
			var result = false;

            foreach (var item in components)
            {
                IntegratedComponent component = item;
				if (!component.Info)
				{
					result = true;
					continue;
				}

                if (!item.Info.IsValidModification)
                {
                    component = new IntegratedComponent(ComponentInfo.CreateRandomModification(item.Info.Data, random, item.Info.ModificationQuality, item.Info.ModificationQuality), item.X, item.Y, 
                        item.BarrelId, item.KeyBinding, item.Behaviour, item.Locked);
                }

                var id = layout.InstallComponent(component.Info, component.X, component.Y);
				if (id >= 0)
					validComponents.Add(component);
				else
				{
					result = true;
					_inventory.Components.Add(component.Info);
				}
            }

			return result;
		}

		private void OnPlayerSkillsReset()
        {
            _activeShips.CheckIfValid(_playerSkills, true);
        }

        private void Load()
        {
            Clear();

            var ships = new List<IShip>();

			foreach (var item in _session.Fleet.Ships.Items)
            {
                try
                {
					ships.Add(item.ToShip(_database));
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    ships.Add(null);
                    UnityEngine.Debug.Log("Unknown ship: " + item.Id);
                }
            }

            _ships.Assign(ships.Where(ship => ship != null));

            _activeShips.Clear();
			foreach (var item in _session.Fleet.Hangar.Items)
            {
                UnityEngine.Debug.Log("group:" + item.Index + " ship:" + item.ShipId);
                _activeShips[item.Index] = ships[item.ShipId];
            }

            _explorationShip = _session.Fleet.ExplorationShipId >= 0 ? ships[_session.Fleet.ExplorationShipId] : null;

            UnityEngine.Debug.Log("PlayerFleet.Load: " + _ships.Count + " ships");

            DataChanged = false;
        }

        private void SaveShips()
        {
            UnityEngine.Debug.Log("PlayerFleet.SaveShips - " + _ships.Count);

            _session.Fleet.Ships.Clear();

            var shipIndices = new Dictionary<IShip, int>();
            var index = 0;
            foreach (var ship in _ships)
            {
				var info = _session.Fleet.CreateShipInfo(ship);
                _session.Fleet.Ships.Add(info);
                shipIndices.Add(ship, index++);
            }

            _session.Fleet.Hangar.Clear();
            for (var j = 0; j < _activeShips.Count; ++j)
            {
                var ship = _activeShips[j];
                if (ship == null)
                    continue;

                if (!shipIndices.TryGetValue(ship, out var id))
                    continue;

                _session.Fleet.Hangar.Add(new HangarSlotInfo(j, id));
            }

            if (_explorationShip != null && shipIndices.TryGetValue(_explorationShip, out var explorationShipId))
                _session.Fleet.ExplorationShipId = explorationShipId;
            else
                _session.Fleet.ExplorationShipId = -1;

            DataChanged = false;
        }

        public void AddSupporterPack()
        {
            var falcon = _database.GalaxySettings.SupporterPackShip;
            if (falcon == null) return;

            if (_ships.FindIndex(item => item.Id == falcon.Ship.Id) < 0)
                _ships.Add(new CommonShip(falcon));
        }

        private void OnSessionAboutToSave()
        {
			if (DataChanged)
				SaveShips();
        }

        private void Clear()
        {
            _ships.Clear();
            _activeShips.Clear();
        }

        private bool DataChanged
        {
            get
            {
                return _dataChanged || _activeShips.IsChanged || _ships.Any(ship => ship.DataChanged);
            }
            set
            {
                _dataChanged = value;
				if (_dataChanged)
                    return;

                _activeShips.IsChanged = false;

                foreach (var ship in _ships)
                    ship.DataChanged = false;
            }
        }

        private bool _dataChanged;
        private IShip _explorationShip;
        private readonly ShipSquad _activeShips = new ShipSquad();
        private readonly ObservableCollection<IShip> _ships = new ObservableCollection<IShip>();

        private readonly ISessionData _session;
        private readonly IDebugManager _debugManager;
        private readonly SupporterPackPurchasedSignal _supporterPackPurchasedSignal;
        private readonly SessionAboutToSaveSignal _sessionAboutToSaveSignal;
        private readonly PlayerSkillsResetSignal _playerSkillsResetSignal;
    }
}
