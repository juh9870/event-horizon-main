﻿using GameDatabase.DataModel;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;
using Combat.Component.Systems.Weapons;
using Combat.Component.Systems.DroneBays;
using Combat.Component.Unit.Classification;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree
{
	public class RequirementChecker : IBehaviorNodeRequirementFactory<bool>
	{
		private readonly IShip _ship;
		private readonly AiSettings _settings;

		public RequirementChecker(IShip ship, AiSettings settings)
		{
			_ship = ship;
			_settings = settings;
		}

		public bool Create(BehaviorNodeRequirement_Empty content) => true;

		public bool Create(BehaviorNodeRequirement_Any content)
		{
			foreach (var child in content.Requirements)
				if (child.Create(this)) return true;

			return false;
		}

		public bool Create(BehaviorNodeRequirement_All content)
		{
			foreach (var child in content.Requirements)
				if (!child.Create(this)) return false;

			return true;
		}

		public bool Create(BehaviorNodeRequirement_None content)
		{
			foreach (var child in content.Requirements)
				if (child.Create(this)) return false;

			return true;
		}

		public bool Create(BehaviorNodeRequirement_HasDevice content)
		{
			foreach (var item in _ship.Specification.Devices)
				if (item.Device.DeviceClass == content.DeviceClass) return true;

			return false;
		}

		public bool Create(BehaviorNodeRequirement_HasDrones content)
		{
			foreach (var item in _ship.Systems.All)
				if (item is IDroneBay || item is ClonningDevice) return true;

			return false;
		}

		public bool Create(BehaviorNodeRequirement_HasHighRecoilWeapon content)
		{
			foreach (var item in _ship.Systems.All)
				if (item is IWeapon weapon)
					if (weapon.Info.Recoil > 0.1f) // TODO: move to database
						return true;

			return false;
		}

		public bool Create(BehaviorNodeRequirement_HasAnyWeapon content) => _ship.Systems.All.HasWeapon();
		public bool Create(BehaviorNodeRequirement_AiLevel content) => content.DifficultyLevel == _settings.AiLevel;
		public bool Create(BehaviorNodeRequirement_MinAiLevel content) => content.DifficultyLevel <= _settings.AiLevel;
		public bool Create(BehaviorNodeRequirement_IsDrone content) => _ship.Type.Class == UnitClass.Drone;
		public bool Create(BehaviorNodeRequirement_CanRepairAllies content) => _ship.Systems.All.HasWeapon(WeaponCapability.RepairAlly);
		public bool Create(BehaviorNodeRequirement_HasChargeableWeapon content) => _ship.Systems.All.HasWeapon(WeaponType.RequiredCharging);
		public bool Create(BehaviorNodeRequirement_HasRemotelyControlledWeapon content) => _ship.Systems.All.HasWeapon(WeaponType.Manageable);
		public bool Create(BehaviorNodeRequirement_SizeClass content) => _ship.Specification.Stats.ShipModel.SizeClass == content.SizeClass;

		public bool Create(BehaviorNodeRequirement_IsReinforcedForRamming content)
		{
			if (_ship.Stats.RammingDamageMultiplier >= 1.5f)
				return true;
			if (_ship.Stats.Resistance.Kinetic >= 0.5f)
				return true;
			if (_ship.Systems.All.HasDevice(GameDatabase.Enums.DeviceClass.Fortification))
				return true;

			return false;
		}

		public bool Create(BehaviorNodeRequirement_HasHighManeuverability content)
		{
			return _ship.Engine.Propulsion > content.Value;
		}
	}
}
