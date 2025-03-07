﻿using Combat.Component.Body;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using UnityEngine;
using WeaponCapability = Combat.Component.Systems.Weapons.WeaponCapability;

namespace Combat.Factory
{
    public interface IBulletStats
    {
        Component.Systems.Weapons.BulletType Type { get; }
        [System.Obsolete] Component.Systems.Weapons.BulletEffectType EffectType { get; }

		WeaponCapability Capability { get; }

        float FlashSize { get; }
        Color FlashColor { get; }
        float FlashTime { get; }

		float EnergyCost { get; }
		float ActivationCost { get; }
		float BulletSpeed { get; }
        float BulletHitRange { get; }
        float Recoil { get; }
        bool IgnoresShipSpeed { get; }
		bool IsBoundToCannon { get; }

		float PowerLevel { get; set; }
        float RandomFactor { get; set; }
        float HitPointsMultiplier { get; set; }
    }

    public class BulletStats : IBulletStats
    {
        public BulletStats(Ammunition ammunition, WeaponStatModifier statModifier)
        {
            _ammunition = ammunition;
            _statModifier = statModifier;

            PowerLevel = 1.0f;
            RandomFactor = 0.0f;
            HitPointsMultiplier = 1.0f;
        }

        public Component.Systems.Weapons.BulletType Type
        {
            get
            {
                switch (_ammunition.Body.Type)
                {
                    case BulletType.Projectile:
                    case GameDatabase.Enums.BulletType.Static:
                        return Component.Systems.Weapons.BulletType.Projectile;
                    case GameDatabase.Enums.BulletType.Homing:
                    case GameDatabase.Enums.BulletType.Magnetic:
                        return Component.Systems.Weapons.BulletType.Homing;
                    case GameDatabase.Enums.BulletType.Continuous:
                    default:
                        return Component.Systems.Weapons.BulletType.Direct;
                }
            }
        }

		public WeaponCapability Capability
		{
			get
			{
				WeaponCapability capability = 0;
				for (int i = 0; i < _ammunition.Effects.Count; ++i)
				{
					switch (_ammunition.Effects[i].Type)
					{
						case ImpactEffectType.Damage:
						case ImpactEffectType.SiphonHitPoints:
							capability |= WeaponCapability.DamageEnemy;
							break;
						case ImpactEffectType.Repair:
							capability |= WeaponCapability.RepairAlly;
							break;
						case ImpactEffectType.CaptureDrones:
							capability |= WeaponCapability.CaptureDrone;
							break;
					}
				}

				return capability;
			}
		}

        public Component.Systems.Weapons.BulletEffectType EffectType
        {
            get
            {
				switch (_ammunition.ImpactType)
                {
                    case BulletImpactType.HitFirstTarget:
                    case BulletImpactType.HitAllTargets:
                        return Component.Systems.Weapons.BulletEffectType.Common;
                    case BulletImpactType.DamageOverTime:
                        return Component.Systems.Weapons.BulletEffectType.DamageOverTime;
                    default:
                        return Component.Systems.Weapons.BulletEffectType.Special;
                }
            }
        }

        public float FlashSize { get { return _ammunition.Body.Size * SizeMultiplier; } }
        public Color FlashColor { get { return Color; } }
        public float FlashTime { get { return _ammunition.Body.Type == BulletType.Continuous ? Mathf.Max(0.2f, _ammunition.Body.Lifetime * LifetimeMultiplier) : 0.2f; } }

        public float BulletHitRange { get { return _ammunition.Body.Type == BulletType.Continuous ? Range : Range + BodySize; } }
        public float BulletSpeed { get { return _ammunition.Body.Velocity * _statModifier.VelocityMultiplier.Value; } }
        public float EnergyCost { get { return _ammunition.Body.EnergyCost * _statModifier.EnergyCostMultiplier.Value; } }
		public bool IgnoresShipSpeed { get { return _ammunition.Body.Type == BulletType.Static; } }
		public float ActivationCost => _ammunition.Body.Type == BulletType.Continuous ? EnergyCost * _ammunition.Body.Lifetime : EnergyCost;

		public float Recoil
        {
            get
            {
                if (_ammunition.Body.Type == BulletType.Projectile)
                    return Weight * BulletSpeed;
                if (_ammunition.Body.Type == BulletType.Homing)
                    return Weight * BulletSpeed * 0.1f;
                return 0f;
            }
        }

        public Color Color { get { return _statModifier.Color ?? _ammunition.Body.Color; } }
        public float Weight { get { return _ammunition.Body.Weight * SizeMultiplier * _statModifier.WeightMultiplier.Value; } }

        public float BodySize
        {
            get
            {
                if (IsAoe)
                    return _ammunition.Body.Size * SizeMultiplier * _statModifier.AoeRadiusMultiplier.Value;
                else
                    return _ammunition.Body.Size * SizeMultiplier;
            } 
        }

		public float Length => _ammunition.Body.Length * SizeMultiplier;
		public float Range { get { return _ammunition.Body.Range * RangeMultiplier; } }
        public float HitPoints { get { return _ammunition.Body.HitPoints * HitPointsMultiplier * _statModifier.HitPointsMultiplier.Value; } }
        public float DamageMultiplier { get { return PowerLevel * _statModifier.DamageMultiplier.Value; } }


        public float GetBulletSpeed()
        {
            var velocity = _ammunition.Body.Velocity * VelocityMultiplier;
            return RandomFactor > 0 ? velocity * (1f + (Random.value - 0.5f) * RandomFactor) : velocity;
        }

        public float GetBulletLifetime()
        {
            if (_ammunition.Body.Lifetime > 0)
            {
                return RandomFactor > 0 ? _ammunition.Body.Lifetime * (1f + (Random.value - 0.5f) * RandomFactor) : _ammunition.Body.Lifetime;
            }
            else if (_ammunition.Body.Velocity > 0 && _ammunition.Body.Range > 0)
            {
                var range = _ammunition.Body.Range * RangeMultiplier;
                var velocity = _ammunition.Body.Velocity * VelocityMultiplier;
                var lifetime = range / velocity;
                return RandomFactor > 0 ? lifetime * (1f + (Random.value - 0.5f) * RandomFactor) : lifetime;
            }
            else
            {
                return 0f;
            }
        }

        public float PowerLevel { get; set; }
        public float RandomFactor { get; set; }
        public float HitPointsMultiplier { get; set; }
		public bool IsBoundToCannon => _ammunition.Body.Type == BulletType.Continuous;

		private bool IsAoe { get { return (_ammunition.ImpactType == BulletImpactType.DamageOverTime || _ammunition.ImpactType == BulletImpactType.HitAllTargets) && _ammunition.Effects.Count > 0; } }

		private float VelocityMultiplier { get { return _statModifier.VelocityMultiplier.Value; } }
        private float RangeMultiplier { get { return PowerLevel > 0.1f ? PowerLevel * _statModifier.RangeMultiplier.Value : 0f; } }
        private float LifetimeMultiplier { get { return 0.5f + PowerLevel * 0.5f; } }
        private float SizeMultiplier { get { return 0.5f + PowerLevel * 0.5f; } }

        private readonly Ammunition _ammunition;
        private readonly WeaponStatModifier _statModifier;
    }

    public class BulletStatsObsolete : IBulletStats
    {
        public BulletStatsObsolete(AmmunitionObsoleteStats ammunition)
        {
            _stats = ammunition;
            Type = ammunition.AmmunitionClass.GetBulletType();
            EffectType = ammunition.AmmunitionClass.GetEffectType();

            PowerLevel = 1.0f;
            RandomFactor = 0.0f;
            HitPointsMultiplier = 1.0f;
        }

        public Component.Systems.Weapons.BulletType Type { get; private set; }
        public Component.Systems.Weapons.BulletEffectType EffectType { get; private set; }

		public WeaponCapability Capability
		{
			get
			{
				switch (EffectType)
				{
					case Component.Systems.Weapons.BulletEffectType.Common:
					case Component.Systems.Weapons.BulletEffectType.DamageOverTime:
						return WeaponCapability.DamageEnemy;
					case Component.Systems.Weapons.BulletEffectType.Repair:
						return WeaponCapability.DamageEnemy | WeaponCapability.RepairAlly;
					case Component.Systems.Weapons.BulletEffectType.ForDronesOnly:
						return WeaponCapability.CaptureDrone;
					default:
						return WeaponCapability.None;
				}
			}
		}

		public float FlashSize { get { return _stats.Size * SizeMultiplier; } }
        public Color FlashColor { get { return Color; } }
        public float FlashTime { get { return _stats.AmmunitionClass.IsBeam() ? Mathf.Max(0.2f, _stats.LifeTime * LifetimeMultiplier) : 0.2f; } }

        public float Range { get { return _stats.Range * RangeMultiplier; } }
        public float Damage { get { return _stats.Damage * DamageMultiplier; } }
        public float Size { get { return _stats.Size * SizeMultiplier; } }
        public Color Color { get { return _stats.Color; } }
        public float Lifetime { get { return _stats.LifeTime * LifetimeMultiplier; } }
        public float Impulse { get { return _stats.Impulse * SizeMultiplier; } }
        public float Recoil { get { return _stats.Recoil * SizeMultiplier; } }
        public float AreaOfEffect { get { return _stats.AreaOfEffect * SizeMultiplier; } }
        public float Velocity { get { return _stats.Velocity; } }
		public float EnergyCost { get { return _stats.EnergyCost; } }
		public bool IgnoresShipSpeed { get { return _stats.IgnoresShipVelocity; } }
		public float ActivationCost => _stats.AmmunitionClass.IsBeam() ? EnergyCost * _stats.LifeTime : EnergyCost;

		public float BulletSpeed { get { return Velocity; } }
        public float BulletHitRange { get { return Mathf.Max(Range, AreaOfEffect); } }

        private float RangeMultiplier { get { return PowerLevel > 0.1f ? PowerLevel : 0f; } }
        private float LifetimeMultiplier { get { return 0.5f + PowerLevel * 0.5f; } }
        private float DamageMultiplier { get { return PowerLevel; } }
        private float SizeMultiplier { get { return 0.5f + PowerLevel * 0.5f; } }

        public float PowerLevel { get; set; }
        public float RandomFactor { get; set; }
        public float HitPointsMultiplier { get; set; }
		public bool IsBoundToCannon => _stats.AmmunitionClass.IsBoundToCannon();

		private readonly AmmunitionObsoleteStats _stats;
	}
}
