using System;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface IColorSelectWidget
    {
        event Action<Color> onColorUpdate;
        event Action<Color> onColorBuy;
        
        Color SelectedColor { get; }
        
        void SetCurrentColor(Color color);
        void SetPanelShow(bool isShow);
        void SetBuyColorPanelsShow(bool isShow);
        void SetColorPrice(int price);
    }
} 