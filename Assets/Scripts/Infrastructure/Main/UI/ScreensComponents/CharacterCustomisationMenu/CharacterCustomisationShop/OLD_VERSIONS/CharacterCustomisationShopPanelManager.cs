/*using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Additional;
using Game.Services.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI
{
    
    public class CharacterCustomisationShopPanelManager : MonoBehaviour
    {
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
        private RectTransform buyButtonT;
        [SerializeField] private Button backButton;
        private RectTransform backButtonT;
        
        [Header("Color Price Panel")]
        [SerializeField] private RectTransform colorPricePanelT;
        [SerializeField] private RectTransform colorPricePanelShowPointT;
        [SerializeField] private RectTransform colorPricePanelHidePointT;
        [SerializeField] private TMP_Text colorPriceCoinsLabel;
        
        [Header("Buy Color Button")]
        [SerializeField] private RectTransform buyColorButtonT;
        
        private AnimationsConfig animConfig;
        
        public void Awake()
        {
            animConfig = ConfigsProxy.AnimationsConfig;
            
            buyButtonT = buyButton.GetComponent<RectTransform>();
            backButtonT = backButton.GetComponent<RectTransform>();
            
            colorPricePanelT.gameObject.SetActive(false);
            UpdateColorPricePanel();
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
            descPanelT.SetPanelShow(isShow, descPanelShowPointT, descPanelHidePointT,
                animConfig.PanelsAnimationsTime, animConfig.PanelsBigAnimationsScaleDifference, 
                animConfig.PanelsAnimationsEase).Forget();
        }

        public void SetColorPricePanelShow(bool isShow)
        {
            colorPricePanelT.SetPanelShow(isShow, colorPricePanelShowPointT, colorPricePanelHidePointT,
                animConfig.PanelsAnimationsTime, animConfig.PanelsBigAnimationsScaleDifference,
                animConfig.PanelsAnimationsEase).Forget();
        }

        public void SetBuyColorButtonShow(bool isShow)
        {
            buyColorButtonT.SetUIObjectShowAlpha(isShow, Vector3.one);
        }
        
        public void UpdatePricePanel(CharacterItemData itemData)
        {
            if (itemData == null)
                return;
                
            priceCoins.text = itemData.CoinsPrice.ToString();
            priceCrystals.text = itemData.CrystalsPrice.ToString();
            
            SetPricePanelShow(true);
        }

        public void UpdateDescPanel(CharacterItemData itemData)
        {
            if (itemData == null)
                return;
                
            itemName.text = itemData.ItemNameId.ToString();
            
            SetDescPanelShow(true);
        }
        
        public void HideAllPanels()
        {
            SetDescPanelShow(false);
            SetPricePanelShow(false);
            SetBuyButtonShow(false);
            SetColorPricePanelShow(false);
            SetBuyColorButtonShow(false);
        }

        private void UpdateColorPricePanel()
        {
            colorPriceCoinsLabel.text = ConfigsProxy.EconomicConfig.ColorizePrice.ToString();
        }
        
        public Button GetBuyButton() => buyButton;
        public Button GetBackButton() => backButton;
    }
}
*/