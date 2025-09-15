using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace Game.Services.Common
{
    
    public class AssetsService : IAssetsService
    {
        private readonly DiContainer diContainer;

        public AssetsService(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }
        
        public async UniTask<T> GetAsset<T>(string assetId)
            where T : Component
        {
            if (string.IsNullOrEmpty(assetId))
            {
                Logs.Warning("Cannot get asset - assetId is null or empty");
                return null;
            }

            var handle = Addressables.InstantiateAsync(assetId);
            T asset = null;
            
            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    asset = operationHandle.Result.GetComponent<T>();
                    if (asset != null)
                    {
                        diContainer.InjectGameObject(operationHandle.Result);
                    }
                }
            };
            
            await handle.Task;
            
            //Logs.Debug($"Asset instantiated: {assetId}");
            return asset;
        }
        
        public async UniTask<T> GetAssetWithBind<T>(string screenAssetId)
            where T : MonoBehaviour
        {
            if (string.IsNullOrEmpty(screenAssetId))
            {
                Logs.Warning("Cannot get asset with bind - screenAssetId is null or empty");
                return null;
            }

            var handle = Addressables.InstantiateAsync(screenAssetId);
            T screen = null;
            
            handle.Completed += operationHandle =>
            {
                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    screen = operationHandle.Result.GetComponent<T>();
                    if (screen != null)
                    {
                        diContainer.Inject(screen);
                        diContainer.Bind<T>().FromInstance(screen).AsSingle();
                        
                        screen.destroyCancellationToken.Register(() =>
                        {
                            diContainer.Unbind<T>();
                        });
                    }
                }
            };
            
            await handle.Task;
            
            Logs.Debug($"Asset with bind instantiated: {screenAssetId}");
            return screen;
        }
        
        public void ReleaseAsset(object asset)
        {
            if (asset == null)
            {
                Logs.Warning("Cannot release asset - asset is null");
                return;
            }

            if (asset is Behaviour behaviour)
            {
                asset = behaviour.gameObject;
                ((GameObject)asset).SetActive(false);
            }
            
            if (asset is GameObject gameObject)
                //Logs.Debug($"Asset releas: {gameObject.name}");
            
            try
            {
                Addressables.Release(asset);
            }
            catch (System.Exception)
            {
                Logs.Error($"Cannot release asset: {asset.GetType().Name}");
            }
        }
    }
}
