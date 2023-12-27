﻿using GameServices.Player;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;

namespace Economy.ItemType
{
    public class StarsItem : IItemType
    {
        [Inject]
        public StarsItem(PlayerResources playerResources, ILocalization localization)
        {
            _playerResources = playerResources;
            _localization = localization;
        }

        public string Id { get { return "star"; } }
        public string Name { get { return _localization.GetString("$StarCurrency"); } }
        public string Description { get { return string.Empty; } }
        public SpriteId Icon => new("Textures/Currency/star", SpriteId.Type.Default);
        public Color Color { get { return AppConfiguration.ColorTable.PremiumItemColor; } }
        public Price Price { get { return Price.Common(15000); } }
        public ItemQuality Quality { get { return ItemQuality.Perfect; } }

        public void Consume(int amount)
        {
            _playerResources.Stars += amount;
        }

        public void Withdraw(int amount)
        {
            _playerResources.Stars -= amount;
        }

        public int MaxItemsToConsume { get { return int.MaxValue; } }

        public int MaxItemsToWithdraw { get { return (int)_playerResources.Stars; } }

        private readonly PlayerResources _playerResources;
        private readonly ILocalization _localization;
    }
}
