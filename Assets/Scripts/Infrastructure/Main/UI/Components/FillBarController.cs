using DG.Tweening;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    
    public class FillBarController : MonoBehaviour
    {
        [SerializeField] private RectTransform fillRect;
        [SerializeField] private float emptyRightValue = 200f;
        [SerializeField] private float fullRightValue = 0f;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;
        [SerializeField] private float initialFillPercent = 0f;
        
        private float currentFillPercent;
        private Tween currentTween;
        private bool isInitialized = false;
        
        private void Awake()
        {
            if (fillRect == null)
                fillRect = GetComponent<RectTransform>();
            
        }
        
        private void Start()
        {
            if (!isInitialized)
                SetFillAmountImmediate(initialFillPercent);
        }
        
        public void SetFillAmount(float fillPercent)
        {
            SetFillAmount(fillPercent, animationDuration);
        }
        
        public void SetFillAmount(float fillPercent, float duration)
        {
            fillPercent = Mathf.Clamp01(fillPercent);
            currentFillPercent = fillPercent;
            
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                currentTween = null;
            }
            
            float targetRightValue = Mathf.Lerp(emptyRightValue, fullRightValue, fillPercent);
            
            Vector2 currentOffsetMax = fillRect.offsetMax;
            currentTween = DOTween.To(
                () => currentOffsetMax.x,
                x => 
                {
                    currentOffsetMax.x = x;
                    fillRect.offsetMax = currentOffsetMax;
                },
                targetRightValue,
                duration)
                .SetEase(animationEase);
                
            isInitialized = true;
        }
        
        public void SetFillAmountImmediate(float fillPercent)
        {
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                currentTween = null;
            }
            
            fillPercent = Mathf.Clamp01(fillPercent);
            currentFillPercent = fillPercent;
            
            float targetRightValue = Mathf.Lerp(emptyRightValue, fullRightValue, fillPercent);
            
            Vector2 offsetMax = fillRect.offsetMax;
            offsetMax.x = targetRightValue;
            fillRect.offsetMax = offsetMax;
            
            isInitialized = true;
        }
        
        public float GetFillAmount()
        {
            return currentFillPercent;
        }
        
        private void OnDestroy()
        {
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
                currentTween = null;
            }
        }
    }
} 