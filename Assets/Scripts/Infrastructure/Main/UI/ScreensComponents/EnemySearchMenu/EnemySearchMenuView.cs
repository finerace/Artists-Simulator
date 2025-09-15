using System.Collections;
using DG.Tweening;
using Game.Services.Common;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class EnemySearchMenuView : UniversalMenuView
    {
        private ILocalizationService localizationService;
        
        [SerializeField] private RectTransform enemySearchFrameRotateArrows;
        [SerializeField] private TMP_Text animatedDots;

        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }
        
        private void Start()
        {
            StartLoadingCircleAnimation();
            StartSearchLabelAnimation();
        }

        public void StartLoadingCircleAnimation()
        {
            const float duration = 0.5f;
                
            enemySearchFrameRotateArrows.DOLocalRotate
                    (new Vector3(0, 0, -360),duration,RotateMode.FastBeyond360)
                .SetLoops(-1).SetEase(Ease.Linear);
        }

        public void StartSearchLabelAnimation()
        {
            const int animationDelay = 1;

            animatedDots.StartCoroutine(LoadingTextAnimation());
            IEnumerator LoadingTextAnimation()
            {
                var wait = new WaitForSeconds(animationDelay);
                var searchTxt = localizationService.GetText(LocalizationKeys.SEARCHMENU_SEARCH);
                
                while (animatedDots != null || animatedDots.gameObject.activeSelf)
                {
                    animatedDots.text = searchTxt;
                    yield return wait;

                    animatedDots.text = $"{searchTxt}.";
                    yield return wait;

                    animatedDots.text = $"{searchTxt}..";
                    yield return wait;

                    animatedDots.text = $"{searchTxt}...";
                    yield return wait;
                }
            }
        }
    }
}