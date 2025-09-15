using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Infrastructure.Configs;
using Game.Services.Core;
using Game.Additional.MagicAttributes;
using UnityEngine;


public class PaintPathProgress : MonoBehaviour
{
    [SerializeField] private PaintingSurface paintingSurface;
    [SerializeField] private PaintPath paintPath;
    [SerializeField] private PaintProgressDragger activeDragger;
    [SerializeField] private Transform pathDotT;
    
    [SerializeField] private LineRenderer[] pathRenderersStatic;
    [SerializeField] private LineRenderer[] pathRenderersProgressive;


    private float maxProgress;
    private float currentProgress;
    private int previousPathProgressRendererPositionCount;
    
    private bool isDragging;
    
    public event Action OnDrag; 
    public event Action OnStopDrag;
    public event Action OnProgressComplete;

    public bool IsDragging => isDragging;

    public float CurrentProgress => currentProgress;

    public Transform PathDotT => pathDotT;

    public PaintProgressDragger ActiveDragger => activeDragger;

    private PaintGameplaySystemsConfig paintConfig = ConfigsProxy.PaintGameplaySystemsConfig;

    public void Initialize(PaintingSurface paintingSurface, PaintPath paintPath, PaintProgressDragger progressDragger = null)
    {
        this.paintingSurface = paintingSurface;
        this.paintPath = paintPath;
        
        if (paintPath == null || paintingSurface == null)
            return;
            
        maxProgress = paintPath.GetPathLong();
        currentProgress = 0;

        var fromSurfaceRotation = Quaternion.LookRotation(-paintingSurface.GetNormal());
        
        if(progressDragger != null)
            pathDotT.gameObject.SetActive(false);
        
        activeDragger = progressDragger != null ? progressDragger : pathDotT.GetComponent<PaintProgressDragger>();
        activeDragger.Initialize(this);
        
        if (pathDotT != null)
        {
            pathDotT.position = paintPath.GetPointOnPath(0);
            pathDotT.rotation = fromSurfaceRotation;
        }
        
        SetPathToRenderers(paintPath.GetPath(paintConfig.PathAccuracy), fromSurfaceRotation);
        ResetProgress();
        
        ShowRenderersInstant(false);
    }

    private void SetPathToRenderers(Vector3[] pathDots, Quaternion rotation)
    {
        if (pathDots == null)
            return;
            
        if (pathRenderersStatic != null)
        {
            foreach (var pathRenderer in pathRenderersStatic)
            {
                if (pathRenderer == null)
                    continue;
                    
                pathRenderer.positionCount = pathDots.Length;
                pathRenderer.transform.rotation = rotation;

                for (int i = 0; i < pathDots.Length; i++)
                {
                    pathRenderer.SetPosition(i, pathDots[i]);
                }
            }
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var pathRenderer in pathRenderersProgressive)
            {
                if (pathRenderer == null)
                    continue;
                    
                pathRenderer.positionCount = 2;
                pathRenderer.transform.rotation = rotation;

                pathRenderer.SetPosition(0, pathDots[0]);
                pathRenderer.SetPosition(1, pathDots[0]);
            }
        }
    }

    public void StartDrag()
    {
        isDragging = true;
        OnDrag?.Invoke();
    }

    public void StopDrag()
    {
        isDragging = false;
        OnStopDrag?.Invoke();
    }

    public void UpdateProgress(Vector3 mouseDelta)
    {
        if (paintPath == null)
            return;
            
        if (float.IsNaN(mouseDelta.x) || float.IsNaN(mouseDelta.y) || float.IsNaN(mouseDelta.z) ||
            float.IsInfinity(mouseDelta.x) || float.IsInfinity(mouseDelta.y) || float.IsInfinity(mouseDelta.z))
        {
            return;
        }
        
        var pathDirection = paintPath.GetDirection(currentProgress);
        
        if (float.IsNaN(pathDirection.x) || float.IsNaN(pathDirection.y) || float.IsNaN(pathDirection.z) ||
            float.IsInfinity(pathDirection.x) || float.IsInfinity(pathDirection.y) || float.IsInfinity(pathDirection.z))
        {
            return;
        }
        
        float dotProduct = Vector3.Dot(pathDirection.normalized, mouseDelta.normalized);
        
        Vector3 rotatedDelta = Quaternion.Euler(pathDirection) * mouseDelta;
        
        if (dotProduct <= 0)
        {
            return;
        }
        
        var currentSegmentIndex = (int)currentProgress;
        
        int pathLong = paintPath.GetPathLong();
        if (currentSegmentIndex >= pathLong)
        {
            currentSegmentIndex = Mathf.Max(0, pathLong - 1);
        }
        
        var segmentLength = paintPath.GetPathPartLong(currentSegmentIndex, paintConfig.PathAccuracy);
        if (segmentLength <= 0.0001f)
        {
            segmentLength = 0.0001f;
        }
        
        var pathDirectionDot = Vector3.Dot(pathDirection.normalized, rotatedDelta.normalized);
        
        var deltaMagnitude = rotatedDelta.magnitude;
        
        var progressAdd = pathDirectionDot * deltaMagnitude * paintConfig.ProgressMoveSpeed / segmentLength;
        
        if (float.IsNaN(progressAdd) || float.IsInfinity(progressAdd) || progressAdd <= 0)
        {
            return;
        }
        
        currentProgress += progressAdd;
        
        if (currentProgress < 0)
            currentProgress = 0;
        else if (currentProgress >= maxProgress)
        {
            currentProgress = maxProgress;
            OnProgressComplete?.Invoke();
        }
        
        UpdateProgressRenderers();
    }

    private void UpdateProgressRenderers()
    {
        if (currentProgress >= maxProgress || paintPath == null)
            return;

        try
        {
            var currentPoint = paintPath.GetPointOnPath(currentProgress);
            
            if (float.IsNaN(currentPoint.x) || float.IsNaN(currentPoint.y) || float.IsNaN(currentPoint.z) ||
                float.IsInfinity(currentPoint.x) || float.IsInfinity(currentPoint.y) || float.IsInfinity(currentPoint.z))
            {
                Debug.LogWarning("Invalid currentPoint detected in UpdateProgressRenderers");
                return;
            }
            
            if (pathDotT != null)
            {
                pathDotT.position = currentPoint;
            }

            if (pathRenderersProgressive == null)
                return;
            
            var pointsAccuracy = paintConfig.PathAccuracy;
            var currentProgressRound = (int)currentProgress;

            int newPositionCount = 2 + (int)(pointsAccuracy * (currentProgress - currentProgressRound)) + currentProgressRound * pointsAccuracy;

            if (newPositionCount < 2)
                return;

            foreach (var pathRenderer in pathRenderersProgressive)
            {
                if (pathRenderer == null)
                    continue;
                
                if (newPositionCount != pathRenderer.positionCount)
                {
                    pathRenderer.positionCount = newPositionCount;

                    for (int i = previousPathProgressRendererPositionCount; i < newPositionCount; i++)
                    {
                        Vector3 pathPoint = paintPath.GetPointOnPath((float)i / pointsAccuracy);
                        
                        if (!float.IsNaN(pathPoint.x) && !float.IsNaN(pathPoint.y) && !float.IsNaN(pathPoint.z) &&
                            !float.IsInfinity(pathPoint.x) && !float.IsInfinity(pathPoint.y) && !float.IsInfinity(pathPoint.z))
                        {
                            pathRenderer.SetPosition(i, pathPoint);
                        }
                        else
                        {
                            Debug.LogWarning($"Invalid path point at index {i}");
                            pathRenderer.SetPosition(i, Vector3.zero);
                        }
                    }
                }

                Vector3 lastPoint = paintPath.GetPointOnPath(currentProgress);
                if (!float.IsNaN(lastPoint.x) && !float.IsNaN(lastPoint.y) && !float.IsNaN(lastPoint.z) &&
                    !float.IsInfinity(lastPoint.x) && !float.IsInfinity(lastPoint.y) && !float.IsInfinity(lastPoint.z))
                {
                    pathRenderer.SetPosition(newPositionCount - 1, lastPoint);
                }

                previousPathProgressRendererPositionCount = newPositionCount;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in UpdateProgressRenderers: " + e.Message);
        }
    }

    public void ResetProgress()
    {
        if (paintPath == null || paintingSurface == null)
            return;
            
        maxProgress = paintPath.GetPathLong();
        currentProgress = 0;
        
        var currentPoint = paintPath.GetPointOnPath(currentProgress);
        var fromSurfaceRotation = Quaternion.LookRotation(-paintingSurface.GetNormal());
        
        if (pathDotT != null)
        {
            pathDotT.position = currentPoint;
            pathDotT.rotation = fromSurfaceRotation;
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var pathRenderer in pathRenderersProgressive)
            {
                if (pathRenderer == null)
                    continue;
                    
                pathRenderer.transform.rotation = fromSurfaceRotation;
                pathRenderer.positionCount = 2;
                pathRenderer.SetPosition(0, paintPath.GetPointOnPath(0));
                pathRenderer.SetPosition(1, paintPath.GetPointOnPath(0));
            }
        }
    }
    
    // Этот метод используется самим компонентом, но весь GameObject должен быть выключен
    // через PaintGameplayGenerationService для правильного отключения обработки событий
    public void SetActive(bool isActive)
    {
        if (activeDragger != null)
        {
            activeDragger.gameObject.SetActive(isActive);
        }
        
        if (pathDotT != null)
        {
            pathDotT.gameObject.SetActive(isActive);
        }
        
        if (pathRenderersStatic != null)
        {
            foreach (var pathRenderer in pathRenderersStatic)
            {
                if (pathRenderer != null)
                    pathRenderer.gameObject.SetActive(isActive);
            }
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var pathRenderer in pathRenderersProgressive)
            {
                if (pathRenderer != null)
                    pathRenderer.gameObject.SetActive(isActive);
            }
        }
    }
    
    /// <summary>
    /// Асинхронно показывает все LineRenderer'ы с анимацией
    /// </summary>
    public async UniTask ShowRenderers()
    {
        await AnimateRenderers(true);
    }
    
    /// <summary>
    /// Асинхронно скрывает все LineRenderer'ы с анимацией
    /// </summary>
    public async UniTask HideRenderers()
    {
        await AnimateRenderers(false);
    }
    
    /// <summary>
    /// Мгновенно показывает или скрывает все LineRenderer'ы без анимации
    /// </summary>
    public void ShowRenderersInstant(bool show)
    {
        // Сначала активируем объекты, если нужно показать
        if (show)
        {
            SetRenderersActive(true);
        }
        
        float targetAlpha = show ? 1f : 0f;
        
        if (pathRenderersStatic != null)
        {
            foreach (var renderer in pathRenderersStatic)
            {
                if (renderer == null) continue;
                SetLineRendererAlpha(renderer, targetAlpha);
            }
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var renderer in pathRenderersProgressive)
            {
                if (renderer == null) continue;
                SetLineRendererAlpha(renderer, targetAlpha);
            }
        }
        
        // Деактивируем объекты после скрытия, если нужно скрыть
        if (!show)
        {
            SetRenderersActive(false);
        }
    }
    
    /// <summary>
    /// Асинхронно анимирует все LineRenderer'ы (показывает или скрывает)
    /// </summary>
    private async UniTask AnimateRenderers(bool show)
    {
        // Включаем объекты перед анимацией показа
        if (show)
        {
            SetRenderersActive(true);
        }
        
        float duration = show ? paintConfig.LineRendererFadeInDuration : paintConfig.LineRendererFadeOutDuration;
        Ease ease = show ? paintConfig.LineRendererFadeInEase : paintConfig.LineRendererFadeOutEase;
        
        UniTask[] animationTasks = new UniTask[
            (pathRenderersStatic?.Length ?? 0) + 
            (pathRenderersProgressive?.Length ?? 0)
        ];
        
        int taskIndex = 0;
        
        if (pathRenderersStatic != null)
        {
            foreach (var renderer in pathRenderersStatic)
            {
                if (renderer == null) continue;
                animationTasks[taskIndex++] = AnimateLineRendererAlpha(renderer, show ? 1f : 0f, duration, ease);
            }
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var renderer in pathRenderersProgressive)
            {
                if (renderer == null) continue;
                animationTasks[taskIndex++] = AnimateLineRendererAlpha(renderer, show ? 1f : 0f, duration, ease);
            }
        }
        
        // Ожидаем завершения всех анимаций
        await UniTask.WhenAll(animationTasks);
        
        // Выключаем объекты после анимации скрытия
        if (!show)
        {
            SetRenderersActive(false);
        }
    }
    
    /// <summary>
    /// Устанавливает активность всех LineRenderer'ов
    /// </summary>
    private void SetRenderersActive(bool active)
    {
        if (pathRenderersStatic != null)
        {
            foreach (var renderer in pathRenderersStatic)
            {
                if (renderer != null)
                    renderer.gameObject.SetActive(active);
            }
        }
        
        if (pathRenderersProgressive != null)
        {
            foreach (var renderer in pathRenderersProgressive)
            {
                if (renderer != null)
                    renderer.gameObject.SetActive(active);
            }
        }
    }
    
    /// <summary>
    /// Мгновенно устанавливает прозрачность для LineRenderer'а
    /// </summary>
    private void SetLineRendererAlpha(LineRenderer lineRenderer, float alpha)
    {
        Gradient gradient = lineRenderer.colorGradient;
        GradientColorKey[] colorKeys = gradient.colorKeys;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
        
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = alpha;
        }
        
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }
    
    /// <summary>
    /// Анимирует прозрачность LineRenderer'а с использованием DOTween
    /// </summary>
    private async UniTask AnimateLineRendererAlpha(LineRenderer lineRenderer, float targetAlpha, float duration, Ease ease)
    {
        Gradient originalGradient = lineRenderer.colorGradient;
        GradientColorKey[] colorKeys = originalGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = originalGradient.alphaKeys;
        
        // Получаем начальную прозрачность (берём среднее значение всех ключей)
        float startAlpha = 0;
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            startAlpha += alphaKeys[i].alpha;
        }
        startAlpha /= alphaKeys.Length;
        
        // Создаём промежуточный объект для анимации через DOTween
        float currentAlpha = startAlpha;
        
        // Создаём и запускаем анимацию
        await DOTween.To(
            () => currentAlpha,
            value => {
                currentAlpha = value;
                for (int i = 0; i < alphaKeys.Length; i++)
                {
                    alphaKeys[i].alpha = value;
                }
                originalGradient.SetKeys(colorKeys, alphaKeys);
                lineRenderer.colorGradient = originalGradient;
            },
            targetAlpha,
            duration
        ).SetEase(ease).AsyncWaitForCompletion();
    }
}
