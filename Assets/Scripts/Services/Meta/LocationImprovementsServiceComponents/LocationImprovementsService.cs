using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using Game.Services.Common.Logging;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Services.Meta
{
    
    public class LocationImprovementsService : ILocationImprovementsService
    {
        private readonly MainLocationProxy mainLocationProxy;
        private readonly ICurrenciesService currenciesService;
        private readonly IAssetsService assetsService;

        private List<(LocationImprovementView,string)> builtLocImprove; 
        private List<string> unlockedImprovements;
        
        public MainLocationView mainLocationView;

        public event Action<string> OnLocImproveUnlocked;
        
        public LocationImprovementsService(
            MainLocationProxy mainLocationProxy,
            ICurrenciesService currenciesService, 
            IAssetsService assetsService)
        {
            this.mainLocationProxy = mainLocationProxy;
            this.currenciesService = currenciesService;
            this.assetsService = assetsService;
        }

        public async UniTask Initialize(List<string> unlockedImprovements)
        {
            this.unlockedImprovements = new List<string>();
            builtLocImprove = new List<(LocationImprovementView, string)>();
            
            this.unlockedImprovements = unlockedImprovements;
            mainLocationView = mainLocationProxy.MainLocation;

            await BuildAllLocationImproves(true);
            Logs.Debug($"LocationImprovements initialized with {unlockedImprovements.Count} unlocked");
        }
        
        public async UniTask BuildAllLocationImproves(bool buildUnlockedOnly = false)
        {
            var allLocImproves = 
                ConfigsProxy.LocationImprovementsConfig.LocationImprovementDatas;

            if (allLocImproves == null || allLocImproves.Length == 0)
                throw new InvalidOperationException("No location improvements found in config");

            foreach (var locImproveData in allLocImproves)
            {
                if(buildUnlockedOnly && !IsLocationImproveUnlocked(locImproveData.Id))
                    continue;
                
                await CreateLocationImprovement(locImproveData.Id);
            }
            
            Logs.Debug($"Built {allLocImproves.Length} location improvements");
        }
        
        public async UniTask DestroyLocationImproves(bool destroyUnlocked = false)
        {
            var improvementsToDestroy = new List<(LocationImprovementView, string)>(builtLocImprove);
            
            foreach (var locImprove in improvementsToDestroy)
            {
                if (destroyUnlocked || !IsLocationImproveUnlocked(locImprove.Item2))
                {
                    await DestroyLocationImprovement(locImprove.Item2);
                }
            }
        }

        public async UniTask CreateLocationImprovement(string locImproveId)
        {
            if (string.IsNullOrEmpty(locImproveId))
                throw new ArgumentException("Location improvement ID cannot be null or empty", nameof(locImproveId));
            
            var locImproveItemData = GetLocationImprovementData(locImproveId);
            
            if (locImproveItemData == null)
                throw new ArgumentException($"Location improvement data not found for id: {locImproveId}", nameof(locImproveId));
            
            await BuildLocationImprovement(locImproveItemData);
        }

        public UniTask DestroyLocationImprovement(string locImproveId)
        {
            if (string.IsNullOrEmpty(locImproveId))
                throw new ArgumentException("Location improvement ID cannot be null or empty", nameof(locImproveId));
            
            bool found = false;
            for (var i = 0; i < builtLocImprove.Count; i++)
            {
                var locImprove = builtLocImprove[i];
                
                if (locImprove.Item2 == locImproveId)
                {
                    assetsService.ReleaseAsset(locImprove.Item1.gameObject);
                    builtLocImprove.RemoveAt(i);
                    Logs.Debug($"Destroyed improvement {locImproveId}");
                    found = true;
                    break;
                }
            }
            
            if (!found)
                throw new ArgumentException($"Location improvement with id {locImproveId} not found in built improvements", nameof(locImproveId));
         
            return UniTask.CompletedTask;
        }

        public bool TryBuyLocationImprove(LocationImprovementItemData locImproveItemData)
        {
            if (string.IsNullOrEmpty(locImproveItemData.Id))
                throw new ArgumentException("Location improvement ID cannot be null or empty", nameof(locImproveItemData));
            
            if (locImproveItemData.Price <= 0)
                throw new ArgumentException($"Invalid price for improvement {locImproveItemData.Id}: {locImproveItemData.Price}", nameof(locImproveItemData));
            
            if (unlockedImprovements.Contains(locImproveItemData.Id))
                throw new InvalidOperationException($"Improvement {locImproveItemData.Id} is already unlocked");
            
            var coinsSpendTry = false;
            
            if(locImproveItemData.CurrencyType == CurrencyType.Coins)
                coinsSpendTry = currenciesService.TrySpendCoins(locImproveItemData.Price);
            else if (locImproveItemData.CurrencyType == CurrencyType.Crystals)
                coinsSpendTry = currenciesService.TrySpendCrystals(locImproveItemData.Price);
            
            if(!coinsSpendTry)
            {
                Logs.Debug($"Insufficient funds for improvement {locImproveItemData.Id}");
                return false;
            }
            
            unlockedImprovements.Add(locImproveItemData.Id);

            foreach (var item in builtLocImprove)
            {
                if(item.Item1.locationImprovementItemData.Id == locImproveItemData.Id)
                    item.Item1.Show();
            }
            
            Logs.Info($"Unlocked improvement {locImproveItemData.Id} for {locImproveItemData.Price} {locImproveItemData.CurrencyType}");
            OnLocImproveUnlocked?.Invoke(locImproveItemData.Id);
            
            return true;
        }
        
        private async UniTask BuildLocationImprovement(LocationImprovementItemData locImproveItemData)
        {
            Transform spawnPoint = null;

            for (int i = 0; i < mainLocationView.LocImproveIds.Length; i++)
            {
                var item = mainLocationView.LocImproveIds[i];

                if (locImproveItemData.Id == item.Id)
                {
                    spawnPoint = mainLocationView.LocImprovePoints[i];
                    break;
                }
            }

            var locImproveView = await assetsService.GetAsset<LocationImprovementView>(locImproveItemData.PrefabPath);

            locImproveView.locationImprovementItemData = locImproveItemData;
            locImproveView.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            builtLocImprove.Add((locImproveView, locImproveItemData.Id));
            
            if(!IsLocationImproveUnlocked(locImproveItemData.Id))
                locImproveView.Hide();
        }
        
        private LocationImprovementItemData GetLocationImprovementData(string locImproveId)
        {
            var allLocImproves = ConfigsProxy.LocationImprovementsConfig.LocationImprovementDatas;
            
            foreach (var locImproveData in allLocImproves)
            {
                if (locImproveData.Id == locImproveId)
                    return locImproveData;
            }
            
            return null;
        }
        
        public bool IsLocationImproveUnlocked(string locImproveId)
        {
            return unlockedImprovements.Contains(locImproveId);
        }
    }
}