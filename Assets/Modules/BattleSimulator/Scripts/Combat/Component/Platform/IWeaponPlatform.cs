﻿using System;
using Combat.Component.Body;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit.HitPoints;

namespace Combat.Component.Platform
{
    public interface IWeaponPlatform : IDisposable
    {
        [Obsolete] UnitType Type { get; }
        IBody Body { get; }
        IUnit Owner { get; }
        IResourcePoints EnergyPoints { get; }
        
        IBulletCompositeDisposable Bullets => null;

        float FixedRotation { get; } // TODO: remove

        bool IsReady { get; }
        float Cooldown { get; }
        float AutoAimingAngle { get; }
		IUnit ActiveTarget { get; set; }

		void Aim(float bulletVelocity, float weaponRange, float relativeEffect);
        void OnShot();
        void SetView(IView view, UnityEngine.Color color);

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);
    }
}
