using System;
using DG.Tweening;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Cosmetic.UI
{
    
    public class LineRendererProgressLine : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        
        [Space]
           
        [SerializeField] private Ease ease;
        [SerializeField] private float duration;
        
        [Space] 
        
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform movePoint;
        [SerializeField] private Transform endPoint;
        
        private void Start()
        {
            lineRenderer.positionCount = 2;
        }

        private void Update()
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, movePoint.position);
        }

        public Tween SetProgress(float progress)
        {
            var targetPos = Vector3.Lerp(startPoint.position, endPoint.position, progress);
            
            return movePoint.DOMove(targetPos, duration).SetEase(ease);
        }
        
#if UNITY_EDITOR
        
        [SerializeField] private float progress;

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            
            lineRenderer.positionCount = 2;
            
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, Vector3.Lerp(startPoint.position, endPoint.position, progress));
        }

#endif
        
    }
}