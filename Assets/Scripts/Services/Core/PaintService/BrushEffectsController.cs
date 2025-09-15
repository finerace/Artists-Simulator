using UnityEngine;
using Zenject;
using System;
using DG.Tweening;
using Game.Additional.MagicAttributes;

namespace Game.Services.Core
{
    
    public class BrushEffectsController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem brushParticles;
        [SerializeField] private Renderer brushRenderer;
        [SerializeField] private string brushMaterialColorProperty = "_Color";
        [SerializeField] private float colorChangeDuration = 0.3f;
        [SerializeField] private Ease colorChangeEase = Ease.Linear;
        [SerializeField] private float particleStopDelay = 0.2f;
        private float stopTimer;
        private bool stopPending;
        private Tween colorTween;

        private PaintingService paintingService;
        private bool particlesActive;
        
        [Inject]
        private void Construct(PaintingService paintingService)
        {
            this.paintingService = paintingService;
            paintingService.OnBrushColorChanged += SetBrushColor;
        }

        private void OnDestroy()
        {
            if (paintingService == null) 
                return;
            
            paintingService.OnBrushColorChanged -= SetBrushColor;
        }

        private void SetBrushColor(Color color)
        {
            if (brushRenderer != null)
            {
                colorTween?.Kill();
                var mat = brushRenderer.material;
                Color startColor = mat.GetColor(brushMaterialColorProperty);
                colorTween = DOTween.To(() => startColor, c => mat.SetColor(brushMaterialColorProperty, c), color, colorChangeDuration)
                    .SetEase(colorChangeEase);
            }
            if (brushParticles != null)
            {
                var main = brushParticles.main;
                main.startColor = color;
            }
        }

        private void Update()
        {
            if (paintingService == null || brushParticles == null)
                return;

            if (paintingService.IsPaintingNow)
            {
                if (!particlesActive)
                {
                    stopPending = false;
                    brushParticles.Play();
                    particlesActive = true;
                }
            }
            else
            {
                if (particlesActive && !stopPending)
                {
                    stopTimer = particleStopDelay;
                    stopPending = true;
                }
                if (stopPending)
                {
                    stopTimer -= Time.deltaTime;
                    if (stopTimer <= 0f)
                    {
                        brushParticles.Stop();
                        stopPending = false;
                        particlesActive = false;
                    }
                }
            }
        }
    }
} 