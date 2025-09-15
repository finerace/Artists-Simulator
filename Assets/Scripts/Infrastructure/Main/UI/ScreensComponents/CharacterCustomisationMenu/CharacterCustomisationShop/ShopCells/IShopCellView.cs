using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface IShopCellView<T>
    {
        event Action onClick;
        
        RectTransform CellTransform { get; }
        T CurrentItem { get; }
        
        UniTask SetNewItem(T itemData);
        UniTask StartShowAnimation();
        UniTask StartHideAnimation();
        
        void SetCellState(ShopCellFrameState frameState);
    }
 
    public enum ShopCellFrameState
    {
        Idle,
        Selected,
        SelectedRemoved,
        Locked,
        SelectedLocked,
        SelectedRemovedLocked
    }
} 