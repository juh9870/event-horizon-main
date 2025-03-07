﻿using Session.Extensions;

namespace Session
{
	public partial class SessionLoader
	{
		partial void Upgrage_v1_0_to_v2_0(v1.SaveGameData oldData, Model.SaveGameData newData)
		{
			GameDiagnostics.Trace.LogWarning("Upgrading savegame from v1.0 to v2.0");

			foreach(var item in oldData.Bosses.Bosses.Items)
			{
				var defeatTime = GameTimeExtensions.TicksToGameTime(item.Value.LastDefeatTime, oldData.Game.StartTime, TimeUnits.Hours);
				newData.Bosses.Bosses.Add(item.Key, new Model.BossInfo(item.Value.DefeatCount, defeatTime));
			}

			foreach (var item in oldData.Events.CompletedTime.Items)
			{
				var time = GameTimeExtensions.TicksToGameTime(item.Value, oldData.Game.StartTime, TimeUnits.Hours);
				newData.Events.CompletedTime.Add(item.Key, time);
			}

			foreach (var item in oldData.StarMap.StarData.Items)
			{
				newData.StarMap.DiscoveredStars.Add((uint)item.Key);

				if (item.Value == 1 || item.Value == 2)
					newData.StarMap.SecuredStars.Add((uint)item.Key);
				if (item.Value == 2 || item.Value == 3)
					newData.StarMap.EnemiesOnStars.Add((uint)item.Key);
			}

			foreach (var item in oldData.Shop.Purchases.Items)
			{
				var map = new Model.PurchasesMap(newData);
				foreach (var purchase in item.Value.Purchases.Items)
					map.Purchases.Add(purchase.Key, new Model.PurchaseInfo(purchase.Value.Quantity, 
						GameTimeExtensions.TicksToGameTime(purchase.Value.Time, oldData.Game.StartTime, TimeUnits.Hours)));

				newData.Shop.Purchases.Add(item.Key, map);
			}

			foreach (var item in oldData.Inventory.Components.Items)
				newData.Inventory.Components.Add(InventoryComponentFromInt64(item.Key), item.Value);
		}

		private static Model.InventoryComponentInfo InventoryComponentFromInt64(long data)
		{
			data >>= 8;
			byte level = (byte)data;
			data >>= 8;
			byte modification = (byte)data;
			data >>= 8;
			byte quality = (byte)data;
			data >>= 8;
			uint componentId = data >= 0 ? (uint)data : 0;

			return new Model.InventoryComponentInfo(componentId, modification, quality, level);
		}
	}
}
