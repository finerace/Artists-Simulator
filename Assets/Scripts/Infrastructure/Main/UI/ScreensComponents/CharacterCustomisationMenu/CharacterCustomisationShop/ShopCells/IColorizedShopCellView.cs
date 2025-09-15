using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface IColorizedShopCellView
    {
        void SetCellColorizeColor(Color color);
        void SetColorizeIconActive(bool isActive);
    }
} 