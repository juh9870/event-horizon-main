﻿using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class SpawnEffectAction : IAction
    {
        public SpawnEffectAction(IUnit unit, EffectFactory effectFactory, float cooldown, VisualEffect effectData, Color color, float size, float lifetime, ConditionType condition)
        {
            _effectFactory = effectFactory;
            _unit = unit;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            _lifetime = lifetime;
            Condition = condition;
            Cooldown = cooldown;
        }

        public ConditionType Condition { get; private set; }
        public float Cooldown { get; set; }

        public void Dispose() {}

        public CollisionEffect Invoke()
        {
            var time = Time.fixedTime;

            if (time < _nextActivation)
                return CollisionEffect.None;
            
            var effect = CompositeEffect.Create(_visualEffect, _effectFactory, null);
            effect.Position = _unit.Body.WorldPosition();
            effect.Rotation = _unit.Body.WorldRotation();
            effect.Color = _color;
            effect.Size = _size;
            effect.Run(_lifetime, Vector2.zero, 0);

            _nextActivation = time + Cooldown;
            
            return CollisionEffect.None;
        }

        private readonly IUnit _unit;

        private float _nextActivation;
        private readonly float _lifetime;
        private readonly Color _color;
        private readonly float _size;
        private readonly EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;
    }
}
