using System;
using UnityEngine;
using Game.Additional.MagicAttributes;

namespace Game.Services.Core
{
    
    public class PaintingSurface : MonoBehaviour
    {
        [SerializeField] private Collider paintCollider;
        [SerializeField] private Texture2D paintTexture;

        [Header("Check And Border Points")]

        [SerializeField] private Transform normalCheckPoint;

        [SerializeField] private Transform leftLowPoint;
        [SerializeField] private Transform rightTopPoint;
        [SerializeField] private Transform rightLowPoint;

        public Collider PaintCollider => paintCollider;
        public Texture2D PaintTexture => paintTexture;

        public Vector3 LeftLowPoint => leftLowPoint.position;
        public Vector3 RightTopPoint => rightTopPoint.position;
        public Vector3 RightLowPoint => rightLowPoint.position;

        // Добавляем кэшированные значения
        private Vector3 _cachedNormal;
        private bool _normalIsCached = false;
        private float _normalUpdateInterval = 0.5f;
        private float _lastNormalUpdateTime = 0f;

        // Кэширование Ray и RaycastHit для повторного использования
        private Ray _raycastCache = new Ray();
        private RaycastHit _hitCache;

        // Кэширование для GetNormal
        private Ray _normalRayCache = new Ray();

        public void Initialize(Color color, int textureSizeX, int textureSizeY)
        {
            paintTexture.Reinitialize(textureSizeX, textureSizeY);
            ClearTexture(color);
            InvalidateNormalCache();
            paintTexture.Apply();
        }

        public void PaintCircleAdditive(Vector2 paintPos, int diameter, Color paintColor, float rotation, float scaleX, float scaleY)
        {
            var radius = diameter / 2;

            for (var x = 0; x <= diameter; x++)
                for (var y = 0; y <= diameter; y++)
                {
                    if (!IsCoordsInCircle(x, y))
                        continue;

                    var xCentered = x - radius;
                    var yCentered = y - radius;

                    Rotation(ref xCentered, ref yCentered, Mathf.RoundToInt(rotation));

                    var xTrue = (int)paintPos.x + xCentered;
                    var yTrue = (int)paintPos.y + yCentered;

                    if (!IsPixelInTextureFrame(xTrue, yTrue))
                        continue;

                    Color GetNewPixelColor()
                    {
                        var oldPixelColor = paintTexture.GetPixel(xTrue, yTrue);

                        var newColor = Color.Lerp(oldPixelColor, paintColor, paintColor.a);

                        return newColor;
                    }

                    paintTexture.SetPixel(xTrue, yTrue, GetNewPixelColor());
                }

            bool IsPixelInTextureFrame(int x, int y)
            {
                var xResult = x > 0 && x <= paintTexture.width;
                var yResult = y > 0 && y <= paintTexture.height;

                return xResult && yResult;
            }

            bool IsCoordsInCircle(int x, int y)
            {
                const float pow = 2f;
                var powX = scaleX;
                var powY = scaleY;

                var fixedRadius = radius - 0.5f;

                x -= radius;
                y -= radius;

                x = Mathf.Abs(x);
                y = Mathf.Abs(y);

                var xCheck = Mathf.Pow(x, powX) + Mathf.Pow(y, powY) < Mathf.Pow(fixedRadius, pow);

                return xCheck;
            }

            void Rotation(ref int x, ref int y, int angle)
            {
                float centerX = x;
                float centerY = y;

                const float specialNum = Mathf.PI / 180;

                var c = Mathf.Cos(angle * specialNum);
                var s = Mathf.Sin(angle * specialNum);

                x = Mathf.FloorToInt(centerX * c + centerY * s);
                y = Mathf.FloorToInt(-centerX * s + centerY * c);
            }
        }

        public void PaintCircleAdditive(Vector2 paintPos, int diameter, Color paintColor, float scaleX, float scaleY)
        {
            var radius = diameter / 2;

            for (var x = 0; x <= diameter; x++)
                for (var y = 0; y <= diameter; y++)
                {
                    if (!IsCoordsInCircle(x, y))
                        continue;

                    var xCentered = x - radius;
                    var yCentered = y - radius;

                    var xTrue = (int)paintPos.x + xCentered;
                    var yTrue = (int)paintPos.y + yCentered;

                    if (!IsPixelInTextureFrame(xTrue, yTrue))
                        continue;

                    Color GetNewPixelColor()
                    {
                        var oldPixelColor = paintTexture.GetPixel(xTrue, yTrue);

                        var newColor = Color.Lerp(oldPixelColor, paintColor, paintColor.a);

                        return newColor;
                    }

                    paintTexture.SetPixel(xTrue, yTrue, GetNewPixelColor());
                }

            bool IsPixelInTextureFrame(int x, int y)
            {
                var xResult = x > 0 && x <= paintTexture.width;
                var yResult = y > 0 && y <= paintTexture.height;

                return xResult && yResult;
            }

            bool IsCoordsInCircle(int x, int y)
            {
                const float pow = 2f;
                var powX = scaleX;
                var powY = scaleY;

                var fixedRadius = radius - 0.5f;

                x -= radius;
                y -= radius;

                x = Mathf.Abs(x);
                y = Mathf.Abs(y);

                var xCheck = Mathf.Pow(x, powX) + Mathf.Pow(y, powY) < Mathf.Pow(fixedRadius, pow);

                return xCheck;
            }
        }

        public void PaintCircle(Vector2 paintPos, int diameter, Color paintColor, float scaleX, float scaleY)
        {
            var radius = diameter / 2;

            for (var x = 0; x <= diameter; x++)
                for (var y = 0; y <= diameter; y++)
                {
                    if (!IsCoordsInCircle(x, y))
                        continue;

                    var xCentered = x - radius;
                    var yCentered = y - radius;

                    var xTrue = (int)paintPos.x + xCentered;
                    var yTrue = (int)paintPos.y + yCentered;

                    if (!IsPixelInTextureFrame(xTrue, yTrue))
                        continue;

                    paintTexture.SetPixel(xTrue, yTrue, paintColor);
                }

            bool IsPixelInTextureFrame(int x, int y)
            {
                var xResult = x > 0 && x <= paintTexture.width;
                var yResult = y > 0 && y <= paintTexture.height;

                return xResult && yResult;
            }

            bool IsCoordsInCircle(int x, int y)
            {
                const float pow = 2f;
                var powX = scaleX;
                var powY = scaleY;

                var fixedRadius = radius - 0.5f;

                x -= radius;
                y -= radius;

                x = Mathf.Abs(x);
                y = Mathf.Abs(y);

                var xCheck = Mathf.Pow(x, powX) + Mathf.Pow(y, powY) < Mathf.Pow(fixedRadius, pow);

                return xCheck;
            }
        }

        public void ApplyPaint()
        {
            paintTexture.Apply();
        }

        public void ClearTexture(Color color)
        {
            var textureX = paintTexture.width;
            var textureY = paintTexture.height;

            for (int x = 0; x <= textureX; x++)
            {
                for (int y = 0; y <= textureY; y++)
                {
                    paintTexture.SetPixel(x, y, color);
                }
            }

            paintTexture.Apply();
        }

        public Vector2 WorldToTextureCoord(Vector3 worldPosition)
        {
            // Используем кэшированную нормаль или обновляем ее по необходимости
            Vector3 normal = GetCachedNormal();
            if (normal == Vector3.zero)
                return Vector2.zero;

            // Переиспользуем объект Ray вместо создания нового
            _raycastCache.origin = worldPosition;
            _raycastCache.direction = -normal;

            // Используем кэшированный RaycastHit для уменьшения сборки мусора
            if (!paintCollider.Raycast(_raycastCache, out _hitCache, 1000f))
                return Vector2.zero;

            // Сразу преобразуем UV координаты в пиксельные, минуя создание промежуточного Vector2
            return new Vector2(
                _hitCache.textureCoord.x * paintTexture.width,
                _hitCache.textureCoord.y * paintTexture.height
            );
        }

        // Получение кэшированной нормали с периодическим обновлением
        private Vector3 GetCachedNormal()
        {
            if (!_normalIsCached || Time.time - _lastNormalUpdateTime > _normalUpdateInterval)
            {
                _cachedNormal = GetNormal();
                _normalIsCached = true;
                _lastNormalUpdateTime = Time.time;
            }
            return _cachedNormal;
        }

        // Сбрасываем кэш нормали при изменении объекта
        public void InvalidateNormalCache()
        {
            _normalIsCached = false;
        }

        public Vector3 GetNormal()
        {
            // Переиспользуем ray вместо создания нового
            _normalRayCache.origin = normalCheckPoint.position;
            _normalRayCache.direction = normalCheckPoint.forward;

            if (paintCollider.Raycast(_normalRayCache, out var hit, 1000f))
            {
                return hit.normal;
            }

            return Vector3.zero;
        }

        public Vector3 GetLeftUpPos()
        {
            var leftUpPos =
                Vector3.Lerp(-(rightLowPoint.position - leftLowPoint.position),
                    -(rightLowPoint.position - rightTopPoint.position), 0.5f) * 2 + rightLowPoint.position;

            return leftUpPos;
        }

        public Vector3 GetCenterPos()
        {
            var centerPos = Vector3.Lerp(leftLowPoint.position, rightTopPoint.position, 0.5f);
            return centerPos;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftLowPoint.position, 0.1f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(rightTopPoint.position, 0.1f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(GetLeftUpPos(), 0.1f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rightLowPoint.position, 0.1f);
        }
#endif
    }
}