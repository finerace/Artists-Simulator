using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using UnityEngine;

namespace Game.Services.Common
{
    
    public class CamerasService
    {
        private Camera mainCamera;
        private Camera canvasCamera;
        
        private Transform mainCameraT;
        private Transform canvasCameraT;

        public Camera MainCamera => mainCamera;

        public Camera CanvasCamera => canvasCamera;

        public Transform MainCameraT => mainCameraT;

        public Transform CanvasCameraT => canvasCameraT;

        public void Initialize() 
        {
            var mainCameraGO = GameObject.Find(ConfigsProxy.AssetsPathsConfig.MainCameraName);
            if (mainCameraGO == null)
            {
                Logs.Error($"Main camera not found: {ConfigsProxy.AssetsPathsConfig.MainCameraName}");
                return;
            }
            
            mainCamera = mainCameraGO.GetComponent<Camera>();
            if (mainCamera == null)
            {
                Logs.Error($"Camera component missing on: {ConfigsProxy.AssetsPathsConfig.MainCameraName}");
                return;
            }
            mainCameraT = mainCamera.transform;
            
            var canvasCameraGO = GameObject.Find(ConfigsProxy.AssetsPathsConfig.CanvasCameraName);
            if (canvasCameraGO == null)
            {
                Logs.Error($"Canvas camera not found: {ConfigsProxy.AssetsPathsConfig.CanvasCameraName}");
                return;
            }
            
            canvasCamera = canvasCameraGO.GetComponent<Camera>();
            if (canvasCamera == null)
            {
                Logs.Error($"Camera component missing on: {ConfigsProxy.AssetsPathsConfig.CanvasCameraName}");
                return;
            }
            canvasCameraT = canvasCamera.transform;
            
            Logs.Info("Cameras initialized successfully");
        }

        public void SetMainCameraToPoint(Transform point)
        {
            if (point == null || mainCamera == null)
            {
                Logs.Warning("Cannot set camera position - null reference");
                return;
            }

            var cameraT = mainCamera.transform;
            cameraT.SetPositionAndRotation(point.position, point.rotation);
        }

        public void SetMainCameraToPoint(Vector3 pos, Quaternion rotation)
        {
            if (mainCamera == null)
            {
                Logs.Warning("Cannot set camera position - camera not initialized");
                return;
            }

            var cameraT = mainCamera.transform;
            cameraT.SetPositionAndRotation(pos, rotation);
        }
        
        public async UniTask MoveMainCameraToPoint(Transform targetPoint)
        {
            if (targetPoint == null || MainCameraT == null)
            {
                Logs.Warning("Cannot move camera - null reference");
                return;
            }

            await UniTask.WhenAll(
                MainCameraT.DOMove(targetPoint.position, ConfigsProxy.AnimationsConfig.CameraAnimationTime)
                    .SetEase(ConfigsProxy.AnimationsConfig.CameraAnimationEase).AsyncWaitForKill().AsUniTask(),
                MainCameraT.DORotateQuaternion(targetPoint.rotation, ConfigsProxy.AnimationsConfig.CameraAnimationTime)
                    .SetEase(ConfigsProxy.AnimationsConfig.CameraAnimationEase).AsyncWaitForKill().AsUniTask()
            );
        }
    }
}