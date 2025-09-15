using System;
using Game.Additional;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Infrastructure.Main.UI
{
    [RequireComponent(typeof(Image))]
    public class ColorSelectorImage : MonoBehaviour, IPointerMoveHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform colorSelectorT;
        [SerializeField] private Image targetImage;

        private bool isPressed;
        
        public event Action<Vector3,Color> onColorSelect;

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!isPressed)
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(colorSelectorT, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            Rect rect = colorSelectorT.rect;
            float x = Mathf.Clamp01((localCursor.x - rect.x) / rect.width);
            float y = Mathf.Clamp01((localCursor.y - rect.y) / rect.height);

            Color color = 
                targetImage.sprite.texture.GetPixelBilinear(x,y);
            
            onColorSelect?.Invoke(eventData.pointerCurrentRaycast.worldPosition, color);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
        }
    }
}