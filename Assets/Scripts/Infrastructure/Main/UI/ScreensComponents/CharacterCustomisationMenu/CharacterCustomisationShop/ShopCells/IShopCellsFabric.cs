using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Services.Meta;

namespace Game.Infrastructure.Main.UI
{
    public interface IShopCellsFabric
    {
        UniTask<IShopCellView<CharacterItemData>> CreateCell(CharacterItemData itemData, Transform parent);
        void DestroyCell(IShopCellView<CharacterItemData> cellView);
    }
} 