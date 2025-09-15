using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;
using Game.Additional;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Main.UI
{
    
    public class CharacterItemsShopView : UniversalMenuView, ICharacterItemsShopView
    {
        [SerializeField] private RectTransform mainShopFrame;
        
        [Header("Price Panel")]
        [SerializeField] private TMP_Text priceCoins;
        [SerializeField] private TMP_Text priceCrystals;
        [SerializeField] private RectTransform pricePanelT;
        [SerializeField] private RectTransform pricePanelShowPointT;
        [SerializeField] private RectTransform pricePanelHidePointT;
        
        [Header("Item Name and Desc panel")]
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private RectTransform descPanelT;
        [SerializeField] private RectTransform descPanelShowPointT;
        [SerializeField] private RectTransform descPanelHidePointT;

        [Header("Shop Buttons")] 
        [SerializeField] private Button buyButton;
        [SerializeField] private Button backButton;
        
        private AnimationsConfig animConfig;
        private RectTransform buyButtonT;
        private RectTransform backButtonT;
        
        public event Action onBuyButtonClick;
        public event Action onBackButtonClick;
        
        public RectTransform ShopContainer => mainShopFrame;
        
        private void Awake()
        {
            animConfig = ConfigsProxy.AnimationsConfig;
            
            buyButtonT = buyButton.GetComponent<RectTransform>();
            backButtonT = backButton.GetComponent<RectTransform>();
            
            buyButton.onClick.AddListener(() => onBuyButtonClick?.Invoke());
            backButton.onClick.AddListener(() => onBackButtonClick?.Invoke());
        }
        
        private void OnDestroy()
        {
            buyButton.onClick.RemoveAllListeners();
            backButton.onClick.RemoveAllListeners();
        }
        
        public void SetBuyButtonShow(bool isShow)
        {
            buyButtonT.SetUIObjectShowAlpha(isShow, Vector3.one);
        }
        
        public void SetBackButtonShow(bool isShow)
        {
            backButtonT.SetUIObjectShowAlpha(isShow, Vector3.one);
        }
        
        public void SetPricePanelShow(bool isShow)
        {
            pricePanelT.SetPanelShow(isShow, pricePanelShowPointT, pricePanelHidePointT,
                animConfig.PanelsAnimationsTime, animConfig.PanelsSmallAnimationsScaleDifference, 
                animConfig.PanelsAnimationsEase).Forget();
        }
        
        public void SetDescPanelShow(bool isShow)
        {
            descPanelT.gameObject.SetActive(false);
        }
        
        public void HideAllPanels()
        {
            SetDescPanelShow(false);
            SetPricePanelShow(false);
            SetBuyButtonShow(false);
        }
        
        public void SetCoinsPrice(int price)
        {
            priceCoins.text = price.ToString();
        }
        
        public void SetCrystalsPrice(int price)
        {
            priceCrystals.text = price.ToString();
        }
        
        public void SetItemName(string itemName)
        {
            this.itemName.text = itemName;
        }
    }
} 