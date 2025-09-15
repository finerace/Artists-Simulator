using UnityEngine;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Core
{
    
    public class PaintExecutionService
    {
        private static PaintGameplaySystemsConfig paintConfig => ConfigsProxy.PaintGameplaySystemsConfig;

        private readonly MainLocationProxy mainLocationProxy;
        private readonly BrushControlService brushControlService;

        private PaintingSurface paintingSurface;

        public PaintExecutionService(
            MainLocationProxy mainLocationProxy,
            BrushControlService brushControlService)
        {
            this.mainLocationProxy = mainLocationProxy;
            this.brushControlService = brushControlService;
        }

        public void Initialize()
        {
            paintingSurface = mainLocationProxy.MainLocation.MainPaintingSurface;
            
            var textureX = paintConfig.TextureSizeX;
            var textureY = paintConfig.TextureSizeY;
            
            paintingSurface.Initialize(paintConfig.StartTextureColor, textureX, textureY);
            
            if (paintingSurface == null)
                Logs.Warning("PaintingSurface is null after initialization");
            
            Logs.Debug($"PaintExecutionService initialized with texture size: {textureX}x{textureY}");
        }

        public bool ProcessPainting(Vector2 paintCoord)
        {
            var lastPoint = brushControlService.LastPaintTexturePoint;
            var toLastPointDistance = Vector2.Distance(paintCoord, lastPoint);
            
            var color = brushControlService.ProcessGradientAndGetColor(paintCoord);
            
            if (toLastPointDistance > paintConfig.MinPaintMove)
            {
                Paint(paintCoord, paintConfig.PaintDiameter, color);
                FastPaintingFix(paintCoord, toLastPointDistance, lastPoint);

                brushControlService.SetLastPaintTexturePoint(paintCoord);
                
                paintingSurface.ApplyPaint();
                brushControlService.NotifyBrushEffectActive();

                return true;
            }

            return false;
        }

        private void FastPaintingFix(Vector2 paintCoord, float toLastPointDistance, Vector2 lastPaintTexturePoint)
        {
            if (lastPaintTexturePoint == Vector2.zero)
                return;

            var paintRateCof = paintConfig.PaintRateCof;
            var localLastPaintPoint = lastPaintTexturePoint;
            
            if (toLastPointDistance < paintRateCof)
                return;

            for (float i = paintRateCof; i <= toLastPointDistance; i += paintRateCof)
            {
                var fixPaintCoord = Vector2.Lerp(lastPaintTexturePoint, paintCoord, i / toLastPointDistance);
                var color = brushControlService.ProcessGradientAndGetColor(fixPaintCoord);
                Paint(fixPaintCoord, paintConfig.PaintDiameter, color);
                localLastPaintPoint = fixPaintCoord;
            }
        }

        public void Paint(Vector2 paintPos, int diameter, Color paintColor)
        {
            if (diameter <= 0)
            {
                Logs.Warning($"Invalid paint diameter: {diameter}");
                return;
            }
            
            paintingSurface.PaintCircleAdditive(paintPos, diameter, paintColor, -brushControlService.BrushRotation, paintConfig.ScaleXCof, paintConfig.ScaleYCof);
        }

        public PaintingSurface GetPaintingSurface()
        {
            return paintingSurface;
        }
    }
} 