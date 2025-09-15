using System;
using Cysharp.Threading.Tasks;
using Game.Additional;
using Game.Infrastructure.Configs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI
{
    
    public class ColorSelector : MonoBehaviour, IColorSelectWidget
    {
        [SerializeField] private ColorSelectorImage[] colorSelectorImages;
        
        [Header("Color Selector Panel")]
        [SerializeField] private RectTransform colorSelectorPanelT;
        [SerializeField] private RectTransform colorSelectorPanelShowPointT;
        [SerializeField] private RectTransform colorSelectorPanelHidePointT;

        [Space]
        [SerializeField] private RectTransform selectorDotT;
        [SerializeField] private Image selectorDotImage;
        
        [Space]
        [SerializeField] private Image selectedColorImage;

        [Header("Buy Color Button")]
        [SerializeField] private Button buyColorButton;
        [SerializeField] private RectTransform buyColorButtonT;
        [SerializeField] private Image buyColorButtonImage;
        
        [Header("Color Price Panel")]
        [SerializeField] private RectTransform colorPricePanelT;
        [SerializeField] private RectTransform colorPricePanelShowPointT;
        [SerializeField] private RectTransform colorPricePanelHidePointT;
        [SerializeField] private TMP_Text colorPriceCoinsLabel;

        private AnimationsConfig animConfig;
        private Color selectedColor;
        
        public event Action<Color> onColorUpdate;
        public event Action<Color> onColorBuy;

        public Color SelectedColor => selectedColor;

        // === INITIALIZATION ===
        
        private void Awake()
        {
            animConfig = ConfigsProxy.AnimationsConfig;
            SubscribeToColorSelectEvents();
            
            buyColorButton.onClick.AddListener(OnColorBuyEvent);
            
            colorPricePanelT.gameObject.SetActive(false);
            UpdateColorPricePanel();
        }
        
        // === PUBLIC METHODS ===
        
        public void SetCurrentColor(Color color)
        {
            SetNewSelectedColor(color);
            selectorDotT.position = Vector3.zero;
        }

        public void SetPanelShow(bool isShow)
        {
            colorSelectorPanelT.SetPanelShow(isShow, colorSelectorPanelShowPointT, colorSelectorPanelHidePointT,
                animConfig.PanelsAnimationsTime, animConfig.PanelsBigAnimationsScaleDifference,
                animConfig.PanelsAnimationsEase).Forget();
            
            if(!isShow)
                SetBuyColorPanelsShow(isShow);
        }
        
        public void SetBuyColorPanelsShow(bool isShow)
        {
            colorPricePanelT.SetPanelShow(isShow, colorPricePanelShowPointT, colorPricePanelHidePointT,
                animConfig.PanelsAnimationsTime, animConfig.PanelsBigAnimationsScaleDifference,
                animConfig.PanelsAnimationsEase).Forget();
            
           buyColorButtonT.SetUIObjectShowAlpha(isShow, Vector3.one);
        }
        
        public void SetColorPrice(int price)
        {
            colorPriceCoinsLabel.text = price.ToString();
        }

        // === EVENT HANDLERS ===
        
        private void OnColorUpdateEvent(Vector3 newSelectDotPos, Color newColor)
        {
            selectorDotT.position = newSelectDotPos;
            SetNewSelectedColor(newColor);

            onColorUpdate?.Invoke(newColor);
        }
        
        private void OnColorBuyEvent()
        {
            onColorBuy?.Invoke(selectedColor);
        }

        // === PRIVATE METHODS ===
        
        private void SetNewSelectedColor(Color color)
        {
            selectedColor = color;
            selectedColorImage.color = selectedColor;
            selectorDotImage.color = selectedColor;
            buyColorButtonImage.color = selectedColor;
        }

        private void SubscribeToColorSelectEvents()
        {
            foreach (var item in colorSelectorImages)
                item.onColorSelect += OnColorUpdateEvent;
        }

        private void UpdateColorPricePanel()
        {
            colorPriceCoinsLabel.text = ConfigsProxy.EconomicConfig.ColorizePrice.ToString();
        }
        
        // === CLEANUP ===

        private void OnDestroy()
        {
            buyColorButton.onClick.RemoveAllListeners();
        }
    }
}