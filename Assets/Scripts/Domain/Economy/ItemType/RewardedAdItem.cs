﻿using System;
using Services.Advertisements;
using Services.Localization;
using GameDatabase.Model;
using UnityEngine;
using Zenject;

namespace Economy.ItemType
{
    public class RewardedAdItem : IItemType
    {
        [Inject]
        public RewardedAdItem(IAdsManager adsManager, ILocalization localization)
        {
            _adsManager = adsManager;
            _localization = localization;
        }

        public string Id { get { return "ad"; } }
        public string Name { get { return _localization.GetString("$AdItemDescription"); } }
        public string Description { get { return string.Empty; } }
        public SpriteId Icon => new("Textures/GUI/InAppPurchases/video", SpriteId.Type.Default);
        public Color Color { get { return Color.white; } }
        public Price Price { get { return new Price(0, Currency.None); } }
        public ItemQuality Quality { get { return ItemQuality.Common; } }

        public void Consume(int amount)
        {
            _adsManager.ShowRewardedVideo();
        }

        public void Withdraw(int amount)
        {
            throw new InvalidOperationException();
        }

        public int MaxItemsToConsume { get { return 1; } }

        public int MaxItemsToWithdraw { get { return 0; } }

        private readonly IAdsManager _adsManager;
        private readonly ILocalization _localization;
    }
}
