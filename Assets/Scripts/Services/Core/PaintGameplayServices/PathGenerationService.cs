using System;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using Game.Services.Common;
using Game.Infrastructure.Additionals;
using Game.Additional.MagicAttributes;
using UnityEngine;
using Random = UnityEngine.Random;
using Game.Services.Meta;
namespace Game.Services.Core
{
    public class PathGenerationService
    {
        private readonly IAssetsService assetsService;
        
        private PaintingSurface paintSurface;
        private PaintPath currentPaintPath;
        private PaintPathProgress currentPathProgress;
        private bool isCurrentPathInGeneration;
        
        public event Action<PaintPath, PaintPathProgress> OnPathGenerated;
        public event Action<PaintPathProgress> OnPathProgressComplete;
        public event Action OnPathDragStarted;
        public event Action OnPathDragStopped;
        
        public PathGenerationService(
            IAssetsService assetsService)
        {
            this.assetsService = assetsService;
        }
        
        [LogMethod(LogLevel.Info)]
        public void Initialize(PaintingSurface surface)
        {
            paintSurface = surface;
        }
        
        [LogMethod(LogLevel.Debug)]
        public async UniTask<(PaintPath path, PaintPathProgress progress)> GenerateNewPath(bool isLightMode = false)
        {
            isCurrentPathInGeneration = true;
            
            await Clear();
            
            var paintPathData = ConfigsProxy.PaintGameplaySystemsConfig.
                PaintPathData[Random.Range(0, ConfigsProxy.PaintGameplaySystemsConfig.PaintPathData.Length)];
            
            var paintPath =
                await assetsService.GetAsset<PaintPath>(paintPathData.PrefabPath);
            var pathProgress =
                await assetsService.GetAsset<PaintPathProgress>(ConfigsProxy.AssetsPathsConfig.PaintPathRendererID);
            
            currentPaintPath = paintPath;
            currentPathProgress = pathProgress;
            
            InsertPaintPathIntoSurface(paintPath, paintSurface);
            ApplySettingsToPath(paintPath, paintPathData);
            
            PaintProgressDragger progressDragger = null;
            
            if (isLightMode)
                progressDragger = paintSurface.GetComponent<PaintProgressDragger>();
            
            pathProgress.Initialize(paintSurface, paintPath, progressDragger);
            
            SubscribeToPathEvents(pathProgress);
            
            OnPathGenerated?.Invoke(paintPath, pathProgress);
            
            isCurrentPathInGeneration = false;
            await UniTask.WaitForEndOfFrame();
            
            return (paintPath, pathProgress);
        }
        
        private void InsertPaintPathIntoSurface(PaintPath paintPath, PaintingSurface paintSurface)
        {
            var surfacePosition = paintSurface.GetCenterPos();
            var forwardDir = paintSurface.GetNormal();
            
            const float offset = 0.05f;
            paintPath.transform.position = surfacePosition + forwardDir * offset;

            var leftLowPoint = paintSurface.LeftLowPoint;
            var rightLowPoint = paintSurface.RightLowPoint;
            var leftUpPoint = paintSurface.GetLeftUpPos();

            Quaternion surfaceRotation = Quaternion.LookRotation(forwardDir);
            paintPath.transform.rotation = surfaceRotation;

            var surfaceWidth = Vector3.Distance(leftLowPoint, rightLowPoint);
            var surfaceHeight = Vector3.Distance(leftLowPoint, leftUpPoint);
            var surfaceSize = new Vector2(surfaceWidth, surfaceHeight);

            var pathSize = new Vector2(1, 1);
            var scaleX = surfaceSize.x / pathSize.x;
            var scaleY = surfaceSize.y / pathSize.y;
            var scaleFactor = Mathf.Min(scaleX, scaleY);

            paintPath.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
        
        private void ApplySettingsToPath(PaintPath paintPath, PaintPathData paintPathData)
        {
            if (paintPath == null || paintPathData == null)
                return;

            Vector3 newScale = paintPath.transform.localScale;
            Quaternion newRotation = paintPath.transform.rotation;

            if (paintPathData.Is45DegreeAllowed && Random.value < 0.5f)
                newRotation *= Quaternion.Euler(0, 0, Random.value < 0.5f ? 45f : -45f);

            if (paintPathData.IsXFlipAllowed && Random.value < 0.5f)
                newScale.x *= -1;
            

            if (paintPathData.IsYFlipAllowed && Random.value < 0.5f)
                newScale.y *= -1;
            
            
            paintPath.transform.localScale = newScale;
            paintPath.transform.rotation = newRotation;
        }
        
        private void SubscribeToPathEvents(PaintPathProgress pathProgress)
        {
            UnsubscribeFromEvents();
            
            pathProgress.OnDrag += OnPathDragStarted;
            pathProgress.OnStopDrag += OnPathDragStopped;
            pathProgress.OnProgressComplete += OnProgressComplete;
            
            pathProgress.ShowRenderers().Forget();
        }
        
        public void UnsubscribeFromEvents()
        {
            if (currentPathProgress == null)
                return;
                
            currentPathProgress.OnDrag -= OnPathDragStarted;
            currentPathProgress.OnStopDrag -= OnPathDragStopped;
            currentPathProgress.OnProgressComplete -= OnProgressComplete;
        }
        
        private void OnProgressComplete()
        {
            OnPathProgressComplete?.Invoke(currentPathProgress);
        }

        [LogMethod(LogLevel.Debug)]
        public async UniTask Clear()
        {
            await ClearInternal();
        }
        
        private async UniTask ClearInternal()
        {
            UnsubscribeFromEvents();
            
            if (currentPathProgress != null)
            {
                await currentPathProgress.HideRenderers();
                
                Logs.Debug($"AssetsService: {assetsService}, currentPathProgress: {currentPathProgress}");

                assetsService.ReleaseAsset(currentPathProgress.gameObject);
                currentPathProgress = null;
            }
            
            if (currentPaintPath != null)
            {
                assetsService.ReleaseAsset(currentPaintPath.gameObject);
                currentPaintPath = null;
            }
            
            paintSurface.ClearTexture(ConfigsProxy.PaintGameplaySystemsConfig.StartTextureColor);
        }
        
        public async UniTask WaitForPathGenerationComplete()
        {
            await UniTask.WaitUntil(() => !isCurrentPathInGeneration);
        }
    }
} 