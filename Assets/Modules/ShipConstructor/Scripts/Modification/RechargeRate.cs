﻿using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using Services.Localization;

namespace Constructor.Modification
{
	class RechargeRate : IModification
	{
		public RechargeRate(ModificationQuality quality)
		{
			_multiplier = quality.ToValue(0.5f, 0.7f, 0.8f, 1.1f, 1.25f, 1.5f);
			Quality = quality;
		}

		public string GetDescription(ILocalization localization) { return localization.GetString("$EnergyRechargeMod", Maths.Format.SignedPercent(_multiplier - 1.0f)); }

		public ModificationQuality Quality { get; private set; }

		public void Apply(ref ShipEquipmentStats stats)
		{
			stats.EnergyRecharge *= _multiplier;
            if (stats.ShieldRechargeRate > 0)
                stats.ShieldRechargeRate *= _multiplier;
        }

        public void Apply(ref DeviceStats device) { }
        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition) { }
        public void Apply(ref DroneBayStats droneBay) { }
	    public void Apply(ref WeaponStatModifier statModifier) { }

        private readonly float _multiplier;
	}
}
