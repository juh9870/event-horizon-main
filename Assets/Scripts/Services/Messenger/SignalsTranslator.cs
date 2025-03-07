﻿using System;
using Combat.Scene;
using Galaxy;
using GameDatabase;
using GameModel;
using GameServices.Multiplayer;
using GameServices.Player;
using GameServices.Quests;
using Session;
using Services.Account;
using Services.Advertisements;
using Services.Gui;
using Services.Storage;
using Services.InAppPurchasing;
using Services.Localization;
using CommonComponents;

namespace Services.Messenger
{
    public class SignalsTranslator : IDisposable
    {
        private readonly IMessengerContext _messenger;
        private readonly IDatabase _database;
        private readonly ISessionData _session;
        private readonly SessionCreatedSignal _sessionCreatedSignal;
        private readonly AccountStatusChangedSignal _accountStatusChangedSignal;
        private readonly CloudLoadingCompletedSignal _cloudLoadingCompletedSignal;
        private readonly PlayerPositionChangedSignal _playerPositionChangedSignal;
        private readonly FuelValueChangedSignal _fuelValueChangedSignal;
        private readonly MoneyValueChangedSignal _moneyValueChangedSignal;
        private readonly StarsValueChangedSignal _starsValueChangedSignal;
        private readonly TokensValueChangedSignal _tokensValueChangedSignal;
        private readonly CloudSavedGamesReceivedSignal _cloudSavedGamesReceivedSignal;
        private readonly CloudStorageStatusChangedSignal _cloudStorageStatusChangedSignal;
        private readonly MultiplayerStatusChangedSignal _multiplayerStatusChangedSignal;
        private readonly EnemyFleetLoadedSignal _enemyFleetLoadedSignal;
        private readonly EnemyFoundSignal _enemyFoundSignal;
        private readonly EscapeKeyPressedSignal _escapeKeyPressedSignal;
        private readonly ShipCreatedSignal _shipCreatedSignal;
        private readonly ShipDestroyedSignal _shipDestroyedSignal;
        private readonly AdStatusChangedSignal _adStatusChangedSignal;
        private readonly RewardedVideoCompletedSignal _rewardedVideoCompletedSignal;
        private readonly ResourcesChangedSignal _resourcesChangedSignal;
        private readonly StarContentChangedSignal _starContentChangedSignal;
        private readonly QuestListChangedSignal _questListChangedSignal;
        private readonly BaseCapturedSignal _baseCapturedSignal;
        private readonly SupplyShipActivatedSignal _supplyShipActivatedSignal;
        private readonly InAppItemListUpdatedSignal _inAppItemListUpdatedSignal;
        private readonly LocalizationChangedSignal _localizationChangedSignal;

        public SignalsTranslator(
            IMessengerContext messenger,
            ISessionData session,
            IDatabase database,
            SessionCreatedSignal sessionCreatedSignal,
            AccountStatusChangedSignal accountStatusChangedSignal,
            CloudLoadingCompletedSignal cloudLoadingCompletedSignal,
            PlayerPositionChangedSignal playerPositionChangedSignal,
            FuelValueChangedSignal fuelValueChangedSignal,
            MoneyValueChangedSignal moneyValueChangedSignal,
            StarsValueChangedSignal starsValueChangedSignal,
            TokensValueChangedSignal tokensValueChangedSignal,
            ResourcesChangedSignal resourcesChangedSignal,
            CloudSavedGamesReceivedSignal cloudSavedGamesReceivedSignal,
            CloudStorageStatusChangedSignal cloudStorageStatusChangedSignal,
            MultiplayerStatusChangedSignal multiplayerStatusChangedSignal,
            EnemyFleetLoadedSignal enemyFleetLoadedSignal,
            EnemyFoundSignal enemyFoundSignal,
            EscapeKeyPressedSignal escapeKeyPressedSignal,
            ShipCreatedSignal shipCreatedSignal,
            ShipDestroyedSignal shipDestroyedSignal,
            AdStatusChangedSignal adStatusChangedSignal,
            RewardedVideoCompletedSignal rewardedVideoCompletedSignal,
            StarContentChangedSignal starContentChangedSignal,
            QuestListChangedSignal questListChangedSignal,
            BaseCapturedSignal baseCapturedSignal,
            SupplyShipActivatedSignal supplyShipActivatedSignal,
            InAppItemListUpdatedSignal inAppItemListUpdatedSignal,
            LocalizationChangedSignal localizationChangedSignal)
        {
            _messenger = messenger;
            _session = session;

            _database = database;
            _database.DatabaseLoaded += OnDatabaseLoaded;

            _sessionCreatedSignal = sessionCreatedSignal;
            _sessionCreatedSignal.Event += OnSessionCreated;
            _cloudLoadingCompletedSignal = cloudLoadingCompletedSignal;
            _cloudLoadingCompletedSignal.Event += OnCloudLoadingCompleted;
            _accountStatusChangedSignal = accountStatusChangedSignal;
            _accountStatusChangedSignal.Event += OnAccountStatusChanged;
            _playerPositionChangedSignal = playerPositionChangedSignal;
            _playerPositionChangedSignal.Event += OnPlayerPositionChanged;
            _fuelValueChangedSignal = fuelValueChangedSignal;
            _fuelValueChangedSignal.Event += OnFuelValueChanged;
            _moneyValueChangedSignal = moneyValueChangedSignal;
            _moneyValueChangedSignal.Event += OnMoneyValueChanged;
            _starsValueChangedSignal = starsValueChangedSignal;
            _starsValueChangedSignal.Event += OnStarsValueChanged;
            _cloudSavedGamesReceivedSignal = cloudSavedGamesReceivedSignal;
            _cloudSavedGamesReceivedSignal.Event += OnCloudSaveGamesReceived;
            _cloudStorageStatusChangedSignal = cloudStorageStatusChangedSignal;
            _cloudStorageStatusChangedSignal.Event += OnCloudStorageStatusChanged;
            _multiplayerStatusChangedSignal = multiplayerStatusChangedSignal;
            _multiplayerStatusChangedSignal.Event += OnMultiplayerStatusChanged;
            _enemyFleetLoadedSignal = enemyFleetLoadedSignal;
            _enemyFleetLoadedSignal.Event += OnEnemyFleetLoaded;
            _enemyFoundSignal = enemyFoundSignal;
            _enemyFoundSignal.Event += OnArenaEnemyFound;
            _tokensValueChangedSignal = tokensValueChangedSignal;
            _tokensValueChangedSignal.Event += OnTokensValueChanged;
            _escapeKeyPressedSignal = escapeKeyPressedSignal;
            _escapeKeyPressedSignal.Event += OnEscapeKeyPressed;
            _shipCreatedSignal = shipCreatedSignal;
            _shipCreatedSignal.Event += OnCombatShipCreated;
            _shipDestroyedSignal = shipDestroyedSignal;
            _shipDestroyedSignal.Event += OnCombatShipDestroyed;
            _adStatusChangedSignal = adStatusChangedSignal;
            _adStatusChangedSignal.Event += OnAdStatusChanged;
            _rewardedVideoCompletedSignal = rewardedVideoCompletedSignal;
            _rewardedVideoCompletedSignal.Event += OnRewardedVideoCompleted;
            _resourcesChangedSignal = resourcesChangedSignal;
            _resourcesChangedSignal.Event += OnResourcesChanged;
            _starContentChangedSignal = starContentChangedSignal;
            _starContentChangedSignal.Event += OnStarContentChanged;
            _questListChangedSignal = questListChangedSignal;
            _questListChangedSignal.Event += OnQuestListChanged;
            _baseCapturedSignal = baseCapturedSignal;
            _baseCapturedSignal.Event += OnBaseCaptured;
            _supplyShipActivatedSignal = supplyShipActivatedSignal;
            _supplyShipActivatedSignal.Event += OnSupplyShipActivated;
            _inAppItemListUpdatedSignal = inAppItemListUpdatedSignal;
            _inAppItemListUpdatedSignal.Event += OnInAppItemListUpdated;
            _localizationChangedSignal = localizationChangedSignal;
            _localizationChangedSignal.Event += OnLocalizationChanged;
        }

        public void Dispose()
        {
            _database.DatabaseLoaded -= OnDatabaseLoaded;
        }

        private void OnSessionCreated()
        {
            _messenger.Broadcast(EventType.SessionCreated);
        }

        private void OnCloudLoadingCompleted()
        {
            _messenger.Broadcast(EventType.CloudDataLoaded);
        }

        private void OnAccountStatusChanged(Account.Status status)
        {
            _messenger.Broadcast<Account.Status>(EventType.AccountStatusChanged, status);
        }

        private void OnPlayerPositionChanged(int position)
        {
            _messenger.Broadcast<int>(EventType.PlayerPositionChanged, position);
        }

        private void OnFuelValueChanged(int value)
        {
            _messenger.Broadcast<int>(EventType.FuelValueChanged, value);
        }

        private void OnMoneyValueChanged(long value)
        {
            _messenger.Broadcast<Money>(EventType.MoneyValueChanged, value);
        }

		private void OnStarsValueChanged(long value)
        {
            _messenger.Broadcast<Money>(EventType.StarsValueChanged, value);
        }

        private void OnTokensValueChanged(int value)
        {
            _messenger.Broadcast<int>(EventType.TokensValueChanged, value);
        }

        private void OnResourcesChanged()
        {
            _messenger.Broadcast(EventType.SpecialResourcesChanged);
        }

        private void OnCloudSaveGamesReceived()
        {
            _messenger.Broadcast(EventType.CloudGamesReceived);
        }

        private void OnCloudStorageStatusChanged(Services.Storage.CloudStorageStatus status)
        {
            _messenger.Broadcast<Services.Storage.CloudStorageStatus>(EventType.CloudStorageStatusChanged, status);
        }

        private void OnMultiplayerStatusChanged(GameServices.Multiplayer.Status status)
        {
            _messenger.Broadcast<GameServices.Multiplayer.Status>(EventType.MultiplayerStatusChanged, status);
        }

        private void OnEnemyFleetLoaded(IPlayerInfo enemy)
        {
            _messenger.Broadcast<IPlayerInfo>(EventType.EnemyFleetLoaded, enemy);
        }

        private void OnArenaEnemyFound(IPlayerInfo enemy)
        {
            _messenger.Broadcast<IPlayerInfo>(EventType.ArenaEnemyFound, enemy);
        }

        private void OnEscapeKeyPressed()
        {
            _messenger.Broadcast(EventType.EscapeKeyPressed);
        }

        private void OnDatabaseLoaded()
        {
            _messenger.Broadcast(EventType.DatabaseLoaded);
        }

        private void OnCombatShipCreated(Combat.Component.Ship.IShip ship)
        {
            _messenger.Broadcast<Combat.Component.Ship.IShip>(EventType.CombatShipCreated, ship);
        }

        private void OnCombatShipDestroyed(Combat.Component.Ship.IShip ship)
        {
            _messenger.Broadcast<Combat.Component.Ship.IShip>(EventType.CombatShipDestroyed, ship);
        }

        private void OnAdStatusChanged(AdStatus status)
        {
            _messenger.Broadcast(EventType.AdStatusChanged, status);
        }

        private void OnRewardedVideoCompleted()
        {
            _messenger.Broadcast(EventType.RewardedVideoCompleted);
        }

        private void OnSocialShareCompleted()
        {
            _messenger.Broadcast(EventType.SocialShareCompleted);
        }

        private void OnStarContentChanged(int starId)
        {
            _messenger.Broadcast(EventType.StarContentChanged, starId);
        }

        private void OnQuestListChanged()
        {
            _messenger.Broadcast(EventType.QuestListChanged);
        }

        private void OnBaseCaptured(Region region)
        {
            _messenger.Broadcast(EventType.StarMapContentChanged);
        }

        private void OnSupplyShipActivated(bool active)
        {
            _messenger.Broadcast(EventType.SupplyShipActivated, active);
        }

        private void OnInAppItemListUpdated()
        {
            _messenger.Broadcast(EventType.IapItemsRefreshed);
        }

        private void OnLocalizationChanged(string language)
        {
            _messenger.Broadcast<string>(EventType.ArrivedToPlanet, language);
        }
    }
}
