//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace GameDatabase.Serializable
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
