using Game.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Infrastructure.Main.UI.GameplayMenu
{
    public class AccuracyObserver : MonoBehaviour
    {
        private PaintAccuracyService paintAccuracyService;
        
        [SerializeField] private FillBarController fillBarController;
        [SerializeField] private Image barImage;
        [SerializeField] private Gradient textGradient;
        private float maxScore;

        [Inject]
        private void Construct(PaintAccuracyService paintAccuracyService)
        {
            this.paintAccuracyService = paintAccuracyService;
        }

        private void Start()
        {
            paintAccuracyService.OnScoreUpdate += OnScoreUpdate;
            paintAccuracyService.OnNewPath += OnNewPath;

            OnScoreUpdate(paintAccuracyService.CurrentScore);
        }

        private void OnScoreUpdate(float accuracyScore)
        {
            if (fillBarController == null || barImage == null) return;
            
            float fillPercent = maxScore != 0 ? accuracyScore / maxScore : 0f;
            fillBarController.SetFillAmount(fillPercent);
            
            Color barColor = textGradient.Evaluate(fillPercent);
            barImage.color = barColor;
        }
        
        private void OnNewPath(PaintPath path)
        {
            if (fillBarController == null) return;
            
            maxScore = paintAccuracyService.CalculateMaxScore();
            OnScoreUpdate(0);
        }
        
        private void OnDestroy()
        {
            if (paintAccuracyService != null)
            {
                paintAccuracyService.OnScoreUpdate -= OnScoreUpdate;
                paintAccuracyService.OnNewPath -= OnNewPath;
            }
        }
    }
}