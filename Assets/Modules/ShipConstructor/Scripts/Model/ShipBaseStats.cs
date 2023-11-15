﻿using GameDatabase.DataModel;
using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ShipBaseStats
    {
        public StatMultiplier DamageMultiplier;
        public StatMultiplier ArmorMultiplier;
        public StatMultiplier ShieldMultiplier;
        public StatMultiplier EnergyMultiplier;
        public StatMultiplier ShipWeightMultiplier;
        public StatMultiplier EquipmentWeightMultiplier;
        public StatMultiplier VelocityMultiplier;
        public StatMultiplier TurnRateMultiplier;
        public StatMultiplier EnergyResistanceMultiplier;
        public StatMultiplier HeatResistanceMultiplier;
        public StatMultiplier KineticResistanceMultiplier;
        public float RegenerationRate;
        public bool AutoTargeting;
        public bool IgnoreWeaponClass;
        //public bool UnlimitedRespawn;
        public Layout Layout;
        public ImmutableCollection<Device> BuiltinDevices;
    }
}
