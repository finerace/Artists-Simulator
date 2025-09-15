using System;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface ICharacterItemsShopView
    {
        event Action onBuyButtonClick;
        event Action onBackButtonClick;
        
        RectTransform ShopContainer { get; }
        
        void SetBuyButtonShow(bool isShow);
        void SetBackButtonShow(bool isShow);
        void SetPricePanelShow(bool isShow);
        void SetDescPanelShow(bool isShow);
        
        void HideAllPanels();
        
        void SetCoinsPrice(int price);
        void SetCrystalsPrice(int price);
        void SetItemName(string itemName);
    }
} 