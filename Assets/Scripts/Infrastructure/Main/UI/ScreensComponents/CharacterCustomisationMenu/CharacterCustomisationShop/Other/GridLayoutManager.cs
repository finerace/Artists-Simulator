using UnityEngine;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;

namespace Game.Infrastructure.Main.UI
{
    
    public class GridLayoutManager : ILayoutManager
    {
        private AnimationsConfig animationsConfig => ConfigsProxy.AnimationsConfig;
        
        public Vector2 CalculateItemPosition(int itemIndex, RectTransform containerRect, RectTransform cellRect)
        {
            int columns = Mathf.Max(1, Mathf.FloorToInt(containerRect.rect.width / cellRect.rect.width));

            float totalSpacingX = containerRect.rect.width - (columns * cellRect.rect.width);
            float gapX = (columns > 1) ? totalSpacingX / (columns - 1) : 0;

            float gapY = cellRect.rect.height * animationsConfig.CellsVerticalGapMultiplier;
            
            int column = itemIndex % columns;
            int row = itemIndex / columns;

            float xPos = column * (cellRect.rect.width + gapX);
            float yPos = -row * (cellRect.rect.height + gapY);

            xPos -= containerRect.rect.width / 2f - cellRect.rect.width / 2f;
            yPos += containerRect.rect.height / 2f - cellRect.rect.height / 2f;

            return new Vector2(xPos, yPos);
        }
        
        public int CalculateMaxItemsInContainer(RectTransform containerRect, RectTransform cellRect)
        {
            if (cellRect.rect.width <= 0 || cellRect.rect.height <= 0)
                return 0;

            int maxItemsOnWidth = Mathf.FloorToInt(containerRect.rect.width / cellRect.rect.width);

            float gapY = cellRect.rect.height * animationsConfig.CellsVerticalGapMultiplier;
            int maxItemsOnHeight = Mathf.FloorToInt(containerRect.rect.height / (cellRect.rect.height + gapY));

            return Mathf.Max(1, maxItemsOnWidth * maxItemsOnHeight);
        }

    }
}