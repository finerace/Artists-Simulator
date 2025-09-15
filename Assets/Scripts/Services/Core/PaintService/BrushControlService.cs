using System;
using DG.Tweening;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Core
{
    
    public class BrushControlService
    {
        private static PaintGameplaySystemsConfig paintConfig => ConfigsProxy.PaintGameplaySystemsConfig;
        
        private readonly MainLocationProxy mainLocationProxy;
        
        private Transform brushT;
        private Transform brushStartPoint;
        
        private Vector3 lastPaintPos;
        private Vector2 lastPaintTexturePoint;
        private float gradientValue;
        
        private float startPaintCooldownTimer;
        private float brushToStartPosTimer;
        
        private bool brushIsOnCanvas;
        private bool isActive;
        private bool isMoving;

        private Color currentBrushColor;
        
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Vector3 startMovePosition;
        private Quaternion startMoveRotation;
        private float moveProgress;
        private float moveDuration;
        private Ease currentMoveEase;
        
        public bool BrushIsOnCanvas => brushIsOnCanvas;
        public bool IsMoving => isMoving;
        public float BrushRotation => brushT.eulerAngles.z;
        public Vector2 LastPaintTexturePoint => lastPaintTexturePoint;

        public event Action<Color> OnBrushColorChanged;
        public event Action<bool> OnBrushEffectStateChanged;
        
        private UniTaskCompletionSource<bool> movementUTcs;

        public BrushControlService(MainLocationProxy mainLocationProxy)
        {
            this.mainLocationProxy = mainLocationProxy;
        }

        public void Initialize()
        {
            brushT = mainLocationProxy.MainLocation.BrushT;
            brushStartPoint = mainLocationProxy.MainLocation.BrushStartPoint;

            startPaintCooldownTimer = paintConfig.StartPaintCooldown;
            
            brushT.SetPositionAndRotation(brushStartPoint.position, brushStartPoint.rotation);
            
            if (brushT == null)
                Logs.Warning("Brush transform is null after initialization");
            if (brushStartPoint == null)
                Logs.Warning("Brush start point is null after initialization");
                
            Logs.Debug("BrushControlService initialized");
        }
        
        public void Tick()
        {
            if (!isActive && !isMoving)
                return;
                
            UpdateMovement();
                
            if (brushToStartPosTimer > 0)
            {
                brushToStartPosTimer -= Time.deltaTime;

                if (brushToStartPosTimer <= 0)
                {
                    startPaintCooldownTimer = paintConfig.StartPaintCooldown;
                    MoveBrushToStartPos();
                }
            }

            if (startPaintCooldownTimer > 0)
            {
                startPaintCooldownTimer -= Time.deltaTime;

                if (startPaintCooldownTimer <= 0)
                    brushIsOnCanvas = true;
            }
        }
        
        private void UpdateMovement()
        {
            if (!isMoving)
                return;
            
            moveProgress += Time.deltaTime / moveDuration;
            moveProgress = Mathf.Clamp01(moveProgress);
            
            float easedProgress = DOVirtual.EasedValue(0f, 1f, moveProgress, currentMoveEase);

            brushT.position = Vector3.Lerp(startMovePosition, targetPosition, easedProgress);
            brushT.rotation = Quaternion.Lerp(startMoveRotation, targetRotation, easedProgress);

            if (moveProgress >= 1f)
            {
                CompleteMovement();
            }
        }

        private void CompleteMovement()
        {
            isMoving = false;
            moveProgress = 0f;
            
            brushT.position = targetPosition;
            brushT.rotation = targetRotation;
            
            movementUTcs?.TrySetResult(true);
            movementUTcs = null;
        }
        
        public void MoveBrushToPaintPoint(Vector3 pos, Vector3 normal)
        {
            if (!isActive && !isMoving)
                return;
            
            brushToStartPosTimer = paintConfig.BrushBackTime;
            
            if (pos == lastPaintPos)
                return;
            
            var targetRot = GetBrushRotation(pos, normal);
            
            StartMovement(pos, targetRot, paintConfig.BrushMoveAnimationTime, paintConfig.BrushMoveAnimationEase);

            lastPaintPos = pos;
        }

        private void StartMovement(Vector3 newTargetPosition, Quaternion newTargetRotation, float duration, Ease ease)
        {
            StopCurrentMovement();
            
            targetPosition = newTargetPosition;
            targetRotation = newTargetRotation;
            startMovePosition = brushT.position;
            startMoveRotation = brushT.rotation;
            moveDuration = duration;
            currentMoveEase = ease;
            moveProgress = 0f;
            
            isMoving = true;
        }

        private void StopCurrentMovement()
        {
            if (isMoving)
            {
                isMoving = false;
                moveProgress = 0f;
                
                movementUTcs?.TrySetResult(false);
                movementUTcs = null;
            }
        }

        private Quaternion GetBrushRotation(Vector3 pos, Vector3 normal)
        {
            var mainRot = Quaternion.LookRotation(-(pos - lastPaintPos).normalized - normal * paintConfig.RotationNormalPower);
            var lookRot = Quaternion.LookRotation(-(pos - lastPaintPos).normalized).eulerAngles;
            var euler = mainRot.eulerAngles;
            
            euler.z = lookRot.x - 90;
            
            return Quaternion.Euler(euler);
        }
        
        public void MoveBrushToStartPos()
        {
            if(!isActive)
                return;

            brushIsOnCanvas = false;
            startPaintCooldownTimer = paintConfig.StartPaintCooldown;
            
            StartMovement(brushStartPoint.position, brushStartPoint.rotation, paintConfig.BrushDistanceMoveAnimationTime, paintConfig.BrushRotationAnimationEase);
            
            Logs.Debug("Moving brush to start position");
        }

        public async UniTask WaitForBrushMovementAsync()
        {
            if (!isMoving)
                return;
            
            movementUTcs ??= new UniTaskCompletionSource<bool>();
            
            await movementUTcs.Task;
        }
        
        public async UniTask MoveBrushToPaintPointAsync(Vector3 pos, Vector3 normal)
        {
            MoveBrushToPaintPoint(pos, normal);
            await WaitForBrushMovementAsync();
        }
        
        public async UniTask MoveBrushToStartPosAsync()
        {
            MoveBrushToStartPos();
            await WaitForBrushMovementAsync();
        }

        public Color ProcessGradientAndGetColor(Vector2 paintCoord)
        {
            var toLastPointDistance = Vector2.Distance(paintCoord, lastPaintTexturePoint);
            IncreaseGradientValue(toLastPointDistance * paintConfig.GradientChangeSpeed);
            
            var color = paintConfig.CommonColorGradient.Evaluate(gradientValue);
            SetBrushColor(color);
            
            return color;
        }

        private void IncreaseGradientValue(float value)
        {
            gradientValue += value - (int)value;

            if (gradientValue >= 1)
                gradientValue = gradientValue - (int)gradientValue;
        }

        private void SetBrushColor(Color color)
        {
            if (currentBrushColor == color)
                return;
            currentBrushColor = color;
            OnBrushColorChanged?.Invoke(color);
        }

        public void SetLastPaintTexturePoint(Vector2 point)
        {
            lastPaintTexturePoint = point;
        }
        
        public void ClearLastPaintPoint()
        {
            lastPaintTexturePoint = Vector2.zero;
            
            OnBrushEffectStateChanged?.Invoke(false);
        }

        public void NotifyBrushEffectActive()
        {
            OnBrushEffectStateChanged?.Invoke(true);
        }

        public void SetActive(bool active)
        {
            if (isActive != active)
                Logs.Debug($"BrushControlService active state changed: {isActive} → {active}");
                
            isActive = active;
            
            if (!active)
                lastPaintTexturePoint = Vector2.zero;
        }
    }
}