//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class InstalledComponentSerializable
	{
		public int ComponentId;
		public ComponentModType Modification;
		public ModificationQuality Quality;
		public int X;
		public int Y;
		public int BarrelId;
		public int Behaviour;
		public int KeyBinding;
	}
}
