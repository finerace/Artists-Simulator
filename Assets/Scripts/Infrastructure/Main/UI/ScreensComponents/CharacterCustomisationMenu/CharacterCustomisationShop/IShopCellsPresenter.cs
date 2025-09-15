using System;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Main.UI
{
    public interface IShopCellsPresenter<TItem> : IAsyncDisposable
    {
        event Action<TItem> OnCellClicked;
        
        UniTask BuildCells(TItem[] items);
        UniTask RebuildCells(TItem[] items);
        UniTask DestroyCells();
        
        void UpdateCellsState();
            
        void UpdateCellState(IShopCellView<TItem> cell);
    }
} 