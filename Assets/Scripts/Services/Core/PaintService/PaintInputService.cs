using UnityEngine;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Core
{
    
    public class PaintInputService
    {
        private static PaintGameplaySystemsConfig paintConfig => ConfigsProxy.PaintGameplaySystemsConfig;
        
        private readonly MainLocationProxy mainLocationProxy;
        private readonly CamerasService camerasService;

        private Camera camera;
        private PaintingSurface paintingSurface;
        private Transform followTarget;
        private bool isFollowMode;
        private bool isActive;

        public bool IsFollowMode => isFollowMode;
        public bool IsServiceActive => isActive;

        public PaintInputService(
            MainLocationProxy mainLocationProxy,
            CamerasService camerasService)
        {
            this.mainLocationProxy = mainLocationProxy;
            this.camerasService = camerasService;
        }

        public void Initialize()
        {
            camera = camerasService.MainCamera;
            paintingSurface = mainLocationProxy.MainLocation.MainPaintingSurface;
            
            if (camera == null)
                Logs.Warning("Main camera is null after initialization");
            if (paintingSurface == null)
                Logs.Warning("Painting surface is null after initialization");
                
            Logs.Debug("PaintInputService initialized");
        }
        
        public bool IsInputActive()
        {
            return isActive && Input.GetKey(KeyCode.Mouse0);
        }

        public bool TryGetPaintCoordinate(out Vector2 paintCoord, out Vector3 paintPoint, out Vector3 paintNormal)
        {
            if(!isActive)
            {
                paintCoord = Vector2.zero;
                paintPoint = Vector3.zero;
                paintNormal = Vector3.zero;
                return false;
            }

            paintCoord = Vector2.zero;
            paintPoint = Vector3.zero;
            paintNormal = Vector3.zero;

            if (isFollowMode && followTarget != null)
            {
                paintPoint = followTarget.position;
                paintNormal = Quaternion.LookRotation(camera.transform.position - paintPoint).eulerAngles;
                paintCoord = paintingSurface.WorldToTextureCoord(paintPoint);
                return true;
            }

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var rayDistance = paintConfig.PaintRaycastDistance;
            
            if (paintingSurface.PaintCollider.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                paintPoint = hit.point;
                paintNormal = hit.normal;
                paintCoord = hit.textureCoord * new Vector2(paintingSurface.PaintTexture.width, paintingSurface.PaintTexture.height);
                return true;
            }

            return false;
        }

        public void SetActive(bool serviceActive)
        {
            if (isActive != serviceActive)
                Logs.Debug($"PaintInputService active state changed: {isActive} → {serviceActive}");
                
            isActive = serviceActive;
            
            if (!serviceActive)
            {
                isFollowMode = false;
                followTarget = null;
            }
        }

        public void SetFollowTarget(Transform target)
        {
            if (target != null && followTarget != target)
                Logs.Debug($"Follow target set: {target.name}");
            else if (target == null && followTarget != null)
                Logs.Debug("Follow target cleared");
                
            followTarget = target;
            isFollowMode = target != null;
        }
    }
} 