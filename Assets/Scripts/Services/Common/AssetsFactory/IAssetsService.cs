using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Services.Common
{
    public interface IAssetsService
    {
        UniTask<T> GetAsset<T>(string assetId) where T : Component;
        UniTask<T> GetAssetWithBind<T>(string screenAssetId) where T : MonoBehaviour;
        void ReleaseAsset(object asset);
    }
}