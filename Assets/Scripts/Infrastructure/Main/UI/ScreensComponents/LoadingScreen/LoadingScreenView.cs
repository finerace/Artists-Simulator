using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class LoadingScreenView : MonoBehaviour, IUIScreen
    {
        private ICoroutineRunner coroutineRunner;
        
        [SerializeField] private Transform rotationCircle;
        //[SerializeField] private RectTransform loadingProgressLine;
        
        //[SerializeField] private TMP_Text loadingPercentsLabel;
        [SerializeField] private TMP_Text loadingTextLabel;

        [SerializeField] private CanvasGroup canvasGroup;
        
        private float showHideAnimationTime;

        //private AsyncOperation currentSceneLoading;
        
        [Inject]
        private void Construct(ICoroutineRunner coroutineRunner)
        {
            this.coroutineRunner = coroutineRunner;
        }

        public void Start()
        {
            //currentSceneLoading = 
            //_scenesService.GetCurrentLoadingOperation();

            DontDestroyOnLoad(gameObject);
            showHideAnimationTime = ConfigsProxy.AnimationsConfig.LoadingMenuShowHideAnimationSpeed;

            Show().Forget();
        }

        /*private void Update()
        {
            /* UpdateLoadingPercentLabel();
             void UpdateLoadingPercentLabel()
             {
                 loadingPercentsLabel.text = 
                     currentSceneLoading.progress.ConvertFloatToPercent() + "%";
              }#1#
            
            /*UpdateLoadingProgressLine();
            void UpdateLoadingProgressLine()
            {
                const float progressLineWidthMin = 210f;
                const float progressLineWidthMax = 1444f;

                var resultWidth = 
                    Mathf.Lerp(progressLineWidthMin, progressLineWidthMax,currentSceneLoading.progress);

                var resultSizeDelta = loadingProgressLine.sizeDelta;
                resultSizeDelta.x = resultWidth;
            
                loadingProgressLine.sizeDelta = resultSizeDelta;
            }#1#
        }*/

        public async UniTask Show()
        {
            gameObject.SetActive(true);
            
            LoadingCircleAnimation();
            void LoadingCircleAnimation()
            {
                const float duration = 0.5f;
                
                rotationCircle.DOLocalRotate
                    (new Vector3(0, 0, -360),duration,RotateMode.FastBeyond360)
                    .SetLoops(-1).SetEase(Ease.Linear);
            }

            StartLoadingLabelAnimation();
            void StartLoadingLabelAnimation()
            {
                const int animationDelay = 1;

                coroutineRunner.StartCoroutine(LoadingTextAnimation());
                IEnumerator LoadingTextAnimation()
                {
                    var wait = new WaitForSeconds(animationDelay);

                    while (loadingTextLabel != null)
                    {
                        loadingTextLabel.text = "Loading";
                        yield return wait;

                        loadingTextLabel.text = "Loading.";
                        yield return wait;

                        loadingTextLabel.text = "Loading..";
                        yield return wait;

                        loadingTextLabel.text = "Loading...";
                        yield return wait;
                    }
                }
            }
            
            await canvasGroup.DOFade(1,showHideAnimationTime).AsyncWaitForKill().AsUniTask();
        }

        public async UniTask Hide()
        {
            await canvasGroup.DOFade(0,showHideAnimationTime).AsyncWaitForKill().AsUniTask();
            
            gameObject.SetActive(false);
        }
    }
}
