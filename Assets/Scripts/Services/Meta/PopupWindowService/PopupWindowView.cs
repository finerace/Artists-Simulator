using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;

namespace Game.Services.Meta
{
    
    public class PopupWindowView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelT;
        
        [Header("Content")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image stateIcon;
        
        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text confirmButtonText;
        [SerializeField] private Image confirmButtonImage;
        
        [SerializeField] private Button[] closeButtons;
        
        private event Action onConfirmAction;
        
        public event Action onClose;
        
        public CanvasGroup CanvasGroup => canvasGroup;
        public RectTransform PanelT => panelT;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
                
            foreach (var button in closeButtons)
            {
                button.onClick.AddListener(OnCloseClicked);
            }
        }
        
        public void SetupContent(string message, string title = null, Action action = null, string buttonText = null)
        {
            messageText.text = message;
            
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            if (!string.IsNullOrEmpty(title))
                titleText.text = title;
            
            confirmButtonText.text = !string.IsNullOrEmpty(buttonText) ? buttonText : "OK";
            
            bool hasAction = action != null;
            
            if (hasAction)
                onConfirmAction = action;
            else
                onConfirmAction = () => OnCloseClicked();
        }
        
        public void SetupState(PopupState state)
        {
            Sprite iconSprite = state switch
            {
                PopupState.Error => ConfigsProxy.AssetsPathsConfig.PopupErrorIcon,
                PopupState.InsufficientFunds => ConfigsProxy.AssetsPathsConfig.PopupInsufficientFundsIcon,
                PopupState.Information => ConfigsProxy.AssetsPathsConfig.PopupInformationIcon,
                _ => ConfigsProxy.AssetsPathsConfig.PopupInformationIcon
            };
            
            stateIcon.sprite = iconSprite;
            stateIcon.gameObject.SetActive(iconSprite != null);
            
            var animConfig = ConfigsProxy.AnimationsConfig;
            
            switch (state)
            {
                case PopupState.Error:
                    SetPopupColors(animConfig.PopupErrorTitleColor, animConfig.PopupErrorTextColor, animConfig.PopupErrorButtonColor);
                    break;
                    
                case PopupState.InsufficientFunds:
                    SetPopupColors(animConfig.PopupInsufficientFundsTitleColor, animConfig.PopupInsufficientFundsTextColor, animConfig.PopupInsufficientFundsButtonColor);
                    break;
                    
                case PopupState.Information:
                default:
                    SetPopupColors(animConfig.PopupInformationTitleColor, animConfig.PopupInformationTextColor, animConfig.PopupInformationButtonColor);
                    break;
            }
        }
        
        private void SetPopupColors(Color titleColor, Color textColor, Color buttonColor)
        {
            titleText.color = titleColor;
            messageText.color = textColor;
            confirmButtonText.color = Color.white;
            confirmButtonImage.color = buttonColor;
        }
        
        private void OnConfirmClicked()
        {
            onConfirmAction?.Invoke();
            OnCloseClicked();
        }
        
        private void OnCloseClicked()
        {
            onClose?.Invoke();
        }
    }
} 