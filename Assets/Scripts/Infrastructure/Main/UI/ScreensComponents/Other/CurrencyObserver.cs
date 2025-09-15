using System;
using DG.Tweening;
using Game.Services.Meta;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class CurrencyObserver : MonoBehaviour
    {
        private ICurrenciesService currenciesService;
        
        [SerializeField] private TMP_Text coinsLabel;
        [SerializeField] private TMP_Text crystalsLabel;
        
        [Header("Анимация валюты")]
        [SerializeField] private float currencyChangeDuration = 0.5f;
        [SerializeField] private Ease currencyChangeEase = Ease.OutQuad;
        [SerializeField] private bool useCurrencyAnimation = true;
        
        private int displayedCoins;
        private int displayedCrystals;
        private Tween coinsTween;
        private Tween crystalsTween;

        [Inject]
        private void Construct(ICurrenciesService currenciesService)
        {
            this.currenciesService = currenciesService;
        }

        private void Initialize()
        {
            currenciesService.OnCoinsChange += OnCoinsChanged;
            currenciesService.OnCrystalsChange += OnCrystalsChanged;
            
            displayedCoins = currenciesService.Coins;
            displayedCrystals = currenciesService.Crystals;
            
            UpdateCoinsText(displayedCoins, false);
            UpdateCrystalsText(displayedCrystals, false);
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            if (currenciesService != null)
            {
                currenciesService.OnCoinsChange -= OnCoinsChanged;
                currenciesService.OnCrystalsChange -= OnCrystalsChanged;
            }
            
            coinsTween?.Kill();
            crystalsTween?.Kill();
        }

        private void OnCoinsChanged(int coins)
        {
            if (!useCurrencyAnimation)
            {
                UpdateCoinsText(coins, false);
                return;
            }
            
            AnimateCoinsChange(coins);
        }
        
        private void OnCrystalsChanged(int crystals)
        {
            if (!useCurrencyAnimation)
            {
                UpdateCrystalsText(crystals, false);
                return;
            }
            
            AnimateCrystalsChange(crystals);
        }
        
        private void AnimateCoinsChange(int targetValue)
        {
            if (displayedCoins == targetValue)
                return;
                
            coinsTween?.Kill();
            
            int startValue = displayedCoins;
            float elapsedTime = 0f;
            
            coinsTween = DOTween.To(
                () => 0f,
                x => {
                    elapsedTime = x;
                    float progress = x / currencyChangeDuration;
                    int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
                    
                    if (currentValue != displayedCoins)
                    {
                        displayedCoins = currentValue;
                        UpdateCoinsText(displayedCoins, true);
                    }
                },
                1f,
                currencyChangeDuration)
                .SetEase(currencyChangeEase)
                .OnComplete(() => {
                    displayedCoins = targetValue;
                    UpdateCoinsText(displayedCoins, false);
                });
        }
        
        private void AnimateCrystalsChange(int targetValue)
        {
            if (displayedCrystals == targetValue)
                return;
                
            crystalsTween?.Kill();
            
            int startValue = displayedCrystals;
            float elapsedTime = 0f;
            
            crystalsTween = DOTween.To(
                () => 0f,
                x => {
                    elapsedTime = x;
                    float progress = x / currencyChangeDuration;
                    int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
                    
                    if (currentValue != displayedCrystals)
                    {
                        displayedCrystals = currentValue;
                        UpdateCrystalsText(displayedCrystals, true);
                    }
                },
                1f,
                currencyChangeDuration)
                .SetEase(currencyChangeEase)
                .OnComplete(() => {
                    displayedCrystals = targetValue;
                    UpdateCrystalsText(displayedCrystals, false);
                });
        }
        
        private void UpdateCoinsText(int value, bool animate)
        {
            if (coinsLabel != null)
            {
                if (animate)
                {
                    coinsLabel.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
                        coinsLabel.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
                    });
                }
                
                coinsLabel.text = value.ToString();
            }
        }
        
        private void UpdateCrystalsText(int value, bool animate)
        {
            if (crystalsLabel != null)
            {
                if (animate)
                {
                    crystalsLabel.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
                        crystalsLabel.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
                    });
                }
                
                crystalsLabel.text = value.ToString();
            }
        }
    }
}