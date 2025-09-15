using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface ILayoutManager
    {
        Vector2 CalculateItemPosition(int itemIndex, RectTransform containerRect, RectTransform cellRect);
        int CalculateMaxItemsInContainer(RectTransform containerRect, RectTransform cellRect);
    }
} 