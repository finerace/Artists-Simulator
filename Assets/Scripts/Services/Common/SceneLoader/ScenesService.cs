using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Services.Common
{
    
    public class ScenesService : IScenesService
    {
        private AsyncOperation currentLoadSceneOperation;

        public void LoadScene(int id)
        {
            if (id < 0)
            {
                Logs.Warning($"Invalid scene id: {id}");
                return;
            }

            Logs.Info($"Loading scene: {id}");
            SceneManager.LoadScene(id);
        }

        public AsyncOperation LoadSceneAsync(int id, LoadSceneMode loadSceneMode)
        {
            if (id < 0)
            {
                Logs.Warning($"Invalid scene id: {id}");
                return null;
            }

            Logs.Info($"Loading scene async: {id}, mode: {loadSceneMode}");
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(id, loadSceneMode);
            
            currentLoadSceneOperation = asyncOperation;
            return asyncOperation;
        }

        public AsyncOperation UnloadSceneAsync(int id)
        {
            if (id < 0)
            {
                Logs.Warning($"Invalid scene id for unload: {id}");
                return null;
            }

            Logs.Info($"Unloading scene: {id}");
            return SceneManager.UnloadSceneAsync(id);
        }

        public AsyncOperation GetCurrentLoadingOperation()
        {
            return currentLoadSceneOperation;
        }

        public void MoveGameObjectsToScene(int sceneId, params GameObject[] target)
        {
            if (target == null || target.Length == 0)
            {
                Logs.Warning("Cannot move objects - target array is null or empty");
                return;
            }

            if (sceneId < 0 || sceneId >= SceneManager.sceneCount)
            {
                Logs.Warning($"Invalid scene id: {sceneId}");
                return;
            }

            var targetScene = SceneManager.GetSceneAt(sceneId);
            if (!targetScene.IsValid())
            {
                Logs.Warning($"Scene at index {sceneId} is not valid");
                return;
            }

            foreach (var obj in target)
            {
                if (obj != null)
                {
                    SceneManager.MoveGameObjectToScene(obj, targetScene);
                }
            }

            Logs.Debug($"Moved {target.Length} objects to scene {sceneId}");
        }
    }
}
