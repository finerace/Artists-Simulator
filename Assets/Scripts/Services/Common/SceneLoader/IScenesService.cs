using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Services.Common
{
    public interface IScenesService
    {
        public void LoadScene(int id);
        public AsyncOperation LoadSceneAsync(int id, LoadSceneMode loadSceneMode);
        public AsyncOperation UnloadSceneAsync(int id);
        public AsyncOperation GetCurrentLoadingOperation();
        public void MoveGameObjectsToScene(int sceneId, params GameObject[] target);

    }
}