using Cysharp.Threading.Tasks;
using Game.Services.Common;

namespace Game.Services.Meta
{
    public interface ISlotItemApplicator
    {
        UniTask ApplyItem(CharacterItemData itemData, IAssetsService assetsService);
    }
} 