﻿using Combat.Component.Systems.Weapons;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SustainAimNode : INode
	{
		private readonly bool _onlyDirectWeapon;

		public SustainAimNode(bool onlyDirectWeapon)
		{
			_onlyDirectWeapon = onlyDirectWeapon;
		}

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;

			for (int i = 0; i < context.SelectedWeapons.Count; ++i)
			{
				var weapon = context.SelectedWeapons.GetWeaponByIndex(i);
				if (!ShouldTrack(weapon)) continue;

				if (context.TargetShip != null)
					if (AttackHelpers.TryGetTarget(weapon, ship, context.TargetShip, out var target))
					{
						context.Controls.Course = Helpers.TargetCourse(ship, target, weapon.Platform);
						return NodeState.Running;
					}

				for (int j = 0; j < context.SecondaryTargets.Count; ++i)
				{
					var enemy = context.SecondaryTargets[j];
					if (AttackHelpers.TryGetTarget(weapon, ship, enemy, out var target))
					{
						context.Controls.Course = Helpers.TargetCourse(ship, target, weapon.Platform);
						return NodeState.Running;
					}
				}
			}

			return NodeState.Success;
		}

		private bool ShouldTrack(IWeapon weapon)
		{
			var type = weapon.Info.WeaponType;
			switch (type)
			{
				case WeaponType.Common:
				case WeaponType.Continuous:
					if (!weapon.Active) return false;
					break;
				case WeaponType.Manageable:
				case WeaponType.RequiredCharging:
					return false;
			}

			var bulletType = weapon.Info.BulletType;
			switch (bulletType)
			{
				case BulletType.Direct:
					return true;
				case BulletType.Projectile:
					return !_onlyDirectWeapon;
				default:
					return false;
			}
		}
	}
}
