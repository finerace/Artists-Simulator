using System;
using Game.Infrastructure.Main.Locations;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using UnityEngine;
using Zenject;
using Game.Services.Common;

namespace Game.Services.Core
{
    
    public class PaintingService : ITickable
    {
        private readonly PaintInputService paintInputService;
        private readonly PaintExecutionService paintExecutionService;
        private readonly BrushControlService brushControlService;
        
        private Vector3 lastPaintPoint;
        private Vector3 lastPaintNormal;
        private bool isPaintingNow;
        
        public bool IsPaintingNow => isPaintingNow;
        public Vector3 LastPaintPoint => lastPaintPoint;
        public Vector3 LastPaintNormal => lastPaintNormal;

        private bool isActive;
        private bool isInitialized;
        
        public event Action<Color> OnBrushColorChanged
        {
            add => brushControlService.OnBrushColorChanged += value;
            remove => brushControlService.OnBrushColorChanged -= value;
        }
        
        public event Action<bool> OnBrushEffectStateChanged
        {
            add => brushControlService.OnBrushEffectStateChanged += value;
            remove => brushControlService.OnBrushEffectStateChanged -= value;
        }
        
        private PaintingService(
            MainLocationProxy mainLocationProxy,
            CamerasService camerasService)
        {
            paintInputService = new PaintInputService(mainLocationProxy, camerasService);
            brushControlService = new BrushControlService(mainLocationProxy);
            paintExecutionService = new PaintExecutionService(mainLocationProxy, brushControlService);
        }
        
        [LogMethod(LogLevel.Info, LogLevel.Debug)]
        public void Initialize()
        {
            if(isInitialized)
            {
                Logs.Warning("Service is already initialized");
                return;
            }
            
            paintInputService.Initialize();
            brushControlService.Initialize();
            paintExecutionService.Initialize();
            
            isInitialized = true;
        }
        
        public void Tick()
        {
            if(!isInitialized)
                return;
            
            brushControlService.Tick();
            
            if(!isActive)
                return;
            
            if (!paintInputService.IsServiceActive)
                return;

            if (paintInputService.IsInputActive())
            {
                if (paintInputService.TryGetPaintCoordinate(out Vector2 paintCoord, out Vector3 paintPoint, out Vector3 paintNormal))
                {
                    if (!brushControlService.BrushIsOnCanvas && !paintInputService.IsFollowMode)
                        return;
                    
                    lastPaintPoint = paintPoint;
                    lastPaintNormal = paintNormal;
                    
                    brushControlService.MoveBrushToPaintPoint(lastPaintPoint, lastPaintNormal);
                    
                    isPaintingNow = paintExecutionService.ProcessPainting(paintCoord);
                }
                else
                {
                    if (!paintInputService.IsFollowMode)
                    {
                        ClearLastPaintPoint();
                        isPaintingNow = false;
                    }
                }
            }
            else
            {
                ClearLastPaintPoint();
            }
        }
        
        private void ClearLastPaintPoint()
        {
            if(Logs.IsActiveWarning(isActive))
                return;
            
            brushControlService.ClearLastPaintPoint();
        }
        
        public void Paint(Vector2 paintPos, int diameter, Color paintColor)
        {
            if(Logs.IsActiveWarning(isActive))
                return;

            paintExecutionService.Paint(paintPos, diameter, paintColor);
        }
        
        public void SetActive(bool active)
        {
            if(Logs.IsNotInitializedWarning(isInitialized))
                return;

            isActive = active;
            
            paintInputService.SetActive(active);
            brushControlService.SetActive(active);

            Logs.Debug($"PaintingService active: {active}");
        }
        
        public void SetInputActive(bool inputActive)
        {
            if(Logs.IsActiveWarning(isActive))
                return;
            
            paintInputService.SetActive(inputActive);
            
            Logs.Info($"SetInputActive: {inputActive}");
        }
        
        public void SetFollowTarget(Transform target)
        {
            if(Logs.IsActiveWarning(isActive))
                return;
            
            lastPaintPoint = target != null ? target.position : Vector3.zero;
            paintInputService.SetFollowTarget(target);
            brushControlService.ClearLastPaintPoint();
        }
        
        public PaintingSurface GetPaintingSurface()
        {
            return paintExecutionService.GetPaintingSurface();
        }
    
        public void MoveBrushToStartPos()
        {
            if(Logs.IsActiveWarning(isActive))
                return;

            brushControlService.MoveBrushToStartPos();

            Logs.Debug("MoveBrushToStartPos");
        }
    }
}
