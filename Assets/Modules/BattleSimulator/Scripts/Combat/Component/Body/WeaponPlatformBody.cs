﻿using Combat.Component.Body;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Platform
{
    public class WeaponPlatformBody : MonoBehaviour, IWeaponPlatformBody
    {
        public static WeaponPlatformBody Create(IScene scene, IUnit parent, Vector2 position, float rotation, float offset, float maxAngle, float rotationSpeed)
        {
            var gameobject = new GameObject("Body");
            parent.Body.AddChild(gameobject.transform);
            var body = gameobject.AddComponent<WeaponPlatformBody>();
            body.Initialize(scene, parent, position, rotation, offset, maxAngle, rotationSpeed);
            return body;
        }

        public IBody Parent => _parent.Body;
        public Vector2 Position => _position;
        public float Rotation => _rotation;
        public float Offset => _offset;
        public Vector2 Velocity => Vector2.zero;
        public float AngularVelocity => 0f;
        public float Weight => 0.0f;
        public float Scale => _scale;
        public Vector2 VisualPosition => _position;
        public float VisualRotation => _rotation;


        public void Move(Vector2 position) {}
        public void Turn(float rotation) {}
        public void ApplyAcceleration(Vector2 acceleration) {}
        public void ApplyAngularAcceleration(float acceleration) {}
        public void SetSize(float size) {}

        public void ApplyForce(Vector2 position, Vector2 force)
        {
            Parent.ApplyForce(position, force);
        }

        public void SetVelocityLimit(float value) {}

        public void Dispose()
        {
            Destroy(gameObject);
        }

        public float FixedRotation => Parent.WorldRotation() + _initialRotation;
        public float AutoAimingAngle => _maxAngle;

        public void UpdatePhysics(float elapsedTime)
        {
            _timeFromTargetUpdate += elapsedTime;
            _targetingTime += elapsedTime;
        }

        public void UpdateView(float elapsedTime)
        {
            UpdateRotation(elapsedTime);
        }

        public void AddChild(Transform child)
        {
            child.parent = transform;
        }

		public IUnit ActiveTarget 
		{
			get => _target; 
			set
			{
				_target = value;
				_timeFromTargetUpdate = 0f;
			} 
		}

        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect)
        {
            _bulletVelocity = bulletVelocity;
            _weaponRange = weaponRange;
            _relativeEffect = relativeEffect;

            if (!IsValidTarget(_target))
                _target = null;

            if (_timeFromTargetUpdate < _targetFindCooldown) return;
            if (_timeFromTargetUpdate < _targetUpdateCooldown && _target.IsActive()) return;

			ActiveTarget = _scene.Ships.GetEnemy(_parent, EnemyMatchingOptions.EnemyForTurret(), _initialRotation, _maxAngle, _weaponRange);
        }

		private bool IsValidTarget(IUnit target)
		{
			if (target == null) return false;
			if (target.Type.Side.IsAlly(_parent.Type.Side)) return true;
			if (target is not IShip ship) return true;
			if (ship.Features.TargetPriority == TargetPriority.None) return false;
			return true;
		}

        private void Initialize(IScene scene, IUnit parent, Vector2 position, float rotation, float offset, float maxAngle, float rotationSpeed)
        {
            _scene = scene;
            _parent = parent;
            _position = parent.Body.ChildPosition(position);
            _initialRotation = rotation;
            _offset = offset;
            _maxAngle = maxAngle;
            _scale = 1f / parent.Body.Scale;
            _rotationSpeed = rotationSpeed > 0 ? rotationSpeed : 360;

            transform.localPosition = _position;
            transform.localScale = Vector3.one*_scale;
        }

        private void UpdateRotation(float elapsedTime)
        {
            var currentFrame = Time.frameCount;
            if (_updateFrameId == currentFrame)
                return;

            _updateFrameId = currentFrame;
            var targetRotation = _initialRotation;

            if (!_target.IsActive())
            {
                _rotation = Mathf.MoveTowardsAngle(_rotation, targetRotation, _rotationSpeed * elapsedTime);
                transform.localEulerAngles = new Vector3(0, 0, _rotation);
                return;
            }

            var targetPosition = _target.Body.WorldPosition();
            var platformPosition = this.WorldPosition();
            float rotation;

            if (_bulletVelocity > 0)
            {
                var velocity = _target.Body.WorldVelocity() - Parent.WorldVelocity() * _relativeEffect;

                Vector2 target;
                float timeInterval;
                if (!Geometry.GetTargetPosition(
                    targetPosition,
                    velocity,
                    platformPosition,
                    _bulletVelocity,
                    out target,
                    out timeInterval))
                {
                    target = targetPosition;
                }

                rotation = RotationHelpers.Angle(platformPosition.Direction(target)) - Parent.WorldRotation();
            }
            else
            {
                rotation = RotationHelpers.Angle(platformPosition.Direction(targetPosition)) - Parent.WorldRotation();
            }

            var delta = Mathf.DeltaAngle(targetRotation, rotation);
            if (delta > _maxAngle)
                targetRotation += _maxAngle;
            else if (delta < -_maxAngle)
                targetRotation -= _maxAngle;
            else
                targetRotation = rotation;

            _rotation = Mathf.MoveTowardsAngle(_rotation, targetRotation, _rotationSpeed * elapsedTime);

            transform.localEulerAngles = new Vector3(0,0,_rotation);
        }

        private float _bulletVelocity;
        private float _weaponRange;
        private float _relativeEffect = 1;

        private int _updateFrameId;
        private float _timeFromTargetUpdate = _targetUpdateCooldown;
        private IUnit _target;

        private float _rotation;

        private Vector2 _position;
        private float _initialRotation;
        private float _maxAngle;
        private float _offset;
        private float _scale;
        private IUnit _parent;
        private IScene _scene;
        private float _targetingTime;
        private float _rotationSpeed;

        private const float _targetUpdateCooldown = 1.0f;
        private const float _targetFindCooldown = 0.1f;
        private const int _minSpread = 14;
    }
}
