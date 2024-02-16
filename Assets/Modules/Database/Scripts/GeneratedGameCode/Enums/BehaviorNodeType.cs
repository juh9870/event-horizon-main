//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

namespace GameDatabase.Enums
{
	public enum BehaviorNodeType
	{
		Undefined = 0,
		SubTree = 1,
		Selector = 2,
		Sequence = 3,
		Parallel = 4,
		RandomSelector = 5,
		Invertor = 6,
		Cooldown = 7,
		Execute = 8,
		ParallelSequence = 10,
		PreserveTarget = 11,
		HasEnoughEnergy = 50,
		IsLowOnHp = 51,
		IsControledByPlayer = 52,
		HasIncomingThreat = 53,
		HasAdditionalTargets = 54,
		IsFasterThanTarget = 55,
		HasMainTarget = 56,
		MainTargetIsAlly = 57,
		MainTargetIsEnemy = 58,
		MainTargetLowHp = 59,
		MainTargetWithinAttackRange = 60,
		HasMothership = 61,
		TargetDistance = 62,
		FindEnemy = 100,
		MoveToAttackRange = 101,
		AttackMainTarget = 102,
		SelectWeapon = 103,
		SpawnDrones = 104,
		Ram = 105,
		DetonateShip = 106,
		Vanish = 107,
		MaintainAttackRange = 108,
		Wait = 109,
		LookAtTarget = 110,
		LookForAdditionalTargets = 111,
		LookForThreats = 112,
		MatchVelocityWithTarget = 113,
		ActivateDevice = 114,
		RechargeEnergy = 115,
		SustainAim = 116,
		ChargeWeapons = 117,
		Chase = 118,
		AvoidThreats = 119,
		SlowDown = 120,
		UseRecoil = 121,
		DefendWithFronalShield = 122,
		TrackControllableAmmo = 123,
		KeepDistance = 124,
		ForgetMainTarget = 125,
		EscapeTargetAttackRadius = 126,
		AttackAdditionalTargets = 127,
		TargetAllyStarbase = 128,
		TargetEnemyStarbase = 129,
		EnginePropulsionForce = 150,
		MotherShipRetreated = 200,
		MotherShipDestroyed = 201,
		FlyAroundMothership = 202,
		GoBerserk = 203,
		TargetMothership = 204,
		MothershipLowHp = 205,
		DroneBayRangeExceeded = 206,
		MakeTargetMothership = 207,
		ShowMessage = 300,
		DebugLog = 301,
		SetValue = 302,
		GetValue = 303,
		SendMessage = 304,
		MessageReceived = 305,
		TargetMessageSender = 306,
		SaveTarget = 307,
		LoadTarget = 308,
		HasSavedTarget = 309,
		ForgetSavedTarget = 310,
	}
}
