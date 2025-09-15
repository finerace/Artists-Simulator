using System;
using UnityEngine;
using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;

[RequireComponent(typeof(Collider))]

public class PaintProgressDragger : MonoBehaviour
{
    private static PaintGameplaySystemsConfig Config => ConfigsProxy.PaintGameplaySystemsConfig;
    
    private PaintPathProgress paintPathProgress;
    
    private Vector2 smoothedDelta = Vector2.zero;
    
    private float baseSensitivity;
    private float dampeningFactor;
    private float maxAccelerationRate;
    private AnimationCurve speedResistanceCurve;

    Vector2 mouseDelta = Vector2.zero;
    public Vector2 MouseDelta => mouseDelta;

    public void Initialize(PaintPathProgress pathProgress)
    {
        paintPathProgress = pathProgress;
        
        baseSensitivity = Config.DragBaseSensitivity;
        dampeningFactor = Config.DragDampeningFactor;
        maxAccelerationRate = Config.MaxAccelerationRate;
        speedResistanceCurve = Config.SpeedResistanceCurve;
        
        ResetValues();
    }

    private void ResetValues()
    {
        smoothedDelta = Vector2.zero;
        mouseDelta = Vector2.zero;
    }

    private void OnMouseDown()
    {
        if (paintPathProgress == null)
            return;
        
        ResetValues();
        paintPathProgress.StartDrag();
    }

    private void OnMouseDrag()
    {
        if (paintPathProgress == null)
            return;
        
        mouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }

    private void Update()
    {
        if (paintPathProgress == null)
            return;
        
        if (float.IsNaN(mouseDelta.x) || float.IsNaN(mouseDelta.y))
        {
            mouseDelta = Vector2.zero;
        }
        
        mouseDelta *= baseSensitivity;
        
        smoothedDelta = Vector2.Lerp(smoothedDelta, mouseDelta, 1f - dampeningFactor);
        
        if (float.IsNaN(mouseDelta.x) || float.IsNaN(mouseDelta.y))
        {
            mouseDelta = Vector2.zero;
        }
        
        mouseDelta *= baseSensitivity;
        
        smoothedDelta = Vector2.Lerp(smoothedDelta, mouseDelta, 1f - dampeningFactor);
        
        float currentSpeed = smoothedDelta.magnitude;
        
        if (currentSpeed > 0.001f)
        {
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxAccelerationRate);
            
            float resistanceFactor = speedResistanceCurve.Evaluate(normalizedSpeed);
            
            float adjustedSpeed = currentSpeed / (1f + resistanceFactor * normalizedSpeed);
            
            if (currentSpeed > 0.001f) // Избегаем деления на ноль
            {
                smoothedDelta *= (adjustedSpeed / currentSpeed);
            }
        }
        
        if (!float.IsNaN(smoothedDelta.x) && !float.IsNaN(smoothedDelta.y) &&
            !float.IsInfinity(smoothedDelta.x) && !float.IsInfinity(smoothedDelta.y))
        {
            paintPathProgress.UpdateProgress(new Vector3(smoothedDelta.x, smoothedDelta.y, 0f));
        }
    }

    private void OnMouseUp()
    {
        if (paintPathProgress == null)
            return;
        
        paintPathProgress.StopDrag();
        mouseDelta = Vector2.zero;
    }
}