﻿using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.Serializable;

namespace GameDatabase.DataModel
{
    public partial class Component
    {
        public static Component Empty = new Component();

        partial void OnDataDeserialized(ComponentSerializable serializable, Database.Loader loader)
        {
            CellType = string.IsNullOrEmpty(serializable.CellType) ? CellType.Empty : (CellType)serializable.CellType.First();
            WeaponSlotType = string.IsNullOrEmpty(serializable.WeaponSlotType) ? WeaponSlotType.Default : (WeaponSlotType)serializable.WeaponSlotType.First();
        }

        private Component() { Id = ItemId<Component>.Empty; }

        public CellType CellType { get; private set; }
        public WeaponSlotType WeaponSlotType { get; private set; }
    }
}
