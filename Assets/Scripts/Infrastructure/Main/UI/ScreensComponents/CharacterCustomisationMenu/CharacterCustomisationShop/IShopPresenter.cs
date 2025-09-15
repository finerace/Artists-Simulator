using System;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Main.UI
{
    public interface IShopPresenter : IAsyncDisposable
    {
        UniTask Initialize();
        UniTask ResetShop();
    }
} 