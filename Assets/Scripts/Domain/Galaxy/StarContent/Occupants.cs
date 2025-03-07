﻿using Combat.Domain;
using Domain.Quests;
using Economy.ItemType;
using Economy.Products;
using GameDatabase;
using GameDatabase.DataModel;
using GameModel;
using GameServices.Random;
using Session;
using Session.Content;
using Model.Military;
using Zenject;
using PlayerFleet = GameServices.Player.PlayerFleet;

namespace Galaxy.StarContent
{
    public class Occupants
    {
		[Inject] private readonly ISessionData _session;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly RegionMap _regionMap;
		[Inject] private readonly IRandom _random;
		[Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly ItemTypeFactory _itemTypeFactory;
        [Inject] private readonly StarContentChangedSignal.Trigger _starContentChangedTrigger;
        [Inject] private readonly CombatModelBuilder.Factory _combatModelBuilderFactory;
        [Inject] private readonly QuestEventSignal.Trigger _questEventTrigger;

        public bool IsExists(int starId)
        {
            return _session.StarMap.GetEnemy(starId) != IStarMapData.Occupant.Empty;
        }

        public bool CanBeAggressive(int starId)
        {
            var enemy = _session.StarMap.GetEnemy(starId);
            return enemy == IStarMapData.Occupant.Agressive || enemy == IStarMapData.Occupant.Unknown;
        }

        public void Suppress(int starId, bool destroy)
        {
            var enemy = _session.StarMap.GetEnemy(starId);
            if (enemy == IStarMapData.Occupant.Empty) return;
            if (enemy == IStarMapData.Occupant.Passive && !destroy) return;

            if (destroy)
            {
                _session.StarMap.SetEnemy(starId, IStarMapData.Occupant.Empty);
                _regionMap.GetStarRegion(starId).OnFleetDefeated();
            }
            else
            {
                _session.StarMap.SetEnemy(starId, IStarMapData.Occupant.Passive);
            }

            _starContentChangedTrigger.Fire(starId);
            _questEventTrigger.Fire(new SimpleEventData(QuestEventType.OccupantsDefeated));
        }

		public bool IsAggressive(int starId)
		{
            if (!CanBeAggressive(starId))
                return false;

            var region = _regionMap.GetStarRegion(starId);
            if (region.Id > Region.PlayerHomeRegionId && !region.IsCaptured)
                return true;

            var enemyFleet = CreateFleet(starId);
            if (Maths.Threat.GetLevel(_playerFleet.Power, enemyFleet.Power) <= Maths.Threat.Level.VeryEasy)
            {
                _session.StarMap.SetEnemy(starId, IStarMapData.Occupant.Passive);
                return false;
            }

            return true;
		}

        public void Attack(int starId)
        {
            _questEventTrigger.Fire(new LocalEncounterEventData(starId, _random.Seed + starId));
        }

        public IFleet CreateFleet(int starId)
        {
            if (!IsExists(starId))
                return Model.Factories.Fleet.Empty;

            var region = _regionMap.GetStarRegion(starId);
            var level = StarLayout.GetStarLevel(starId, _session.Game.Seed);

            if (region.Id > Region.PlayerHomeRegionId)
                return Model.Factories.Fleet.FactionDefenders(region, starId + _random.Seed, _database);
            else
                return Model.Factories.Fleet.Common(level, starId + _random.Seed, _database);
        }

        public CombatModelBuilder CreateCombatModelBuilder(int starId)
		{
		    if (!IsExists(starId))
		        throw new System.InvalidOperationException();

            var region = _regionMap.GetStarRegion(starId);
		    var level = StarLayout.GetStarLevel(starId, _session.Game.Seed);

		    var builder = _combatModelBuilderFactory.Create();
		    builder.PlayerFleet = Model.Factories.Fleet.Player(_playerFleet, _database);
		    builder.EnemyFleet = CreateFleet(starId);

		    if (region.Id > Region.PlayerHomeRegionId)
		    {
		        builder.Rules = Model.Factories.CombatRules.Faction(region.Faction, region.MilitaryPower);

                if (_random.RandomInt(starId + 123, 100) <= 10)
                    builder.AddSpecialReward(new Product(_itemTypeFactory.CreateResearchItem(region.Faction)));
            }
            else
		    {
		        builder.Rules = Model.Factories.CombatRules.Neutral(level);

                if (_random.RandomInt(starId + 123, 100) <= 20)
                    builder.AddSpecialReward(new Product(_itemTypeFactory.CreateResearchItem(Faction.Empty)));
            }

            return builder;
		}

		public struct Facade
		{
			public Facade(Occupants occupants, int starId)
			{
				_occupants = occupants;
				_starId = starId;
			}

			public bool IsExists { get { return _occupants.IsExists (_starId); } }
			public bool CanBeAggressive { get { return _occupants.CanBeAggressive(_starId); } }
			public bool IsAggressive { get { return _occupants.IsAggressive(_starId); } }
		    public IFleet CreateFleet() { return _occupants.CreateFleet(_starId); }
		    public CombatModelBuilder CreateCombatModelBuilder() { return _occupants.CreateCombatModelBuilder(_starId); }
            public void Attack() { _occupants.Attack(_starId); }
		    public void Suppress(bool destroy) { _occupants.Suppress(_starId, destroy); }


            private readonly Occupants _occupants;
			private readonly int _starId;
		}
    }
}
