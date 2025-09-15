using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public class MainScreenView : MonoBehaviour, IUIScreen
    {
        [SerializeField] private Transform canvasT;
        
        [Header("References")]
        
        [SerializeField] private RectTransform mainPanelT;
        [SerializeField] private RectTransform mainPanelShowPoint;
        [SerializeField] private RectTransform mainPanelHidePoint;

        [Space]
        
        [SerializeField] private RectTransform leftPanelT;
        [SerializeField] private RectTransform leftPanelShowPoint;
        [SerializeField] private RectTransform leftPanelHidePoint;

        [Space] 
        
        [SerializeField] private RectTransform coinsCurrencyPanelT;
        [SerializeField] private RectTransform diamondsCurrencyPanelT;
        
        [SerializeField] private RectTransform coinsCurrencyPoint;
        [SerializeField] private RectTransform diamondsCurrencyPoint;
        
        [Space]

        [SerializeField] private RectTransform playerPanelT;
        [SerializeField] private RectTransform playerPanelPoint;

        [Space]
        
        [SerializeField] private float animationTime;
        [SerializeField] private Ease animationEase;

        public RectTransform MainPanelT => mainPanelT;

        public RectTransform LeftPanelT => leftPanelT;

        public RectTransform CoinsCurrencyPanelT => coinsCurrencyPanelT;

        public RectTransform DiamondsCurrencyPanelT => diamondsCurrencyPanelT;

        public RectTransform CoinsCurrencyPoint => coinsCurrencyPoint;

        public RectTransform DiamondsCurrencyPoint => diamondsCurrencyPoint;

        public RectTransform PlayerPanelT => playerPanelT;

        public RectTransform PlayerPanelPoint => playerPanelPoint;

        public async UniTask HidePanels()
        {
            var wait1 = mainPanelT.DOAnchorPos(mainPanelHidePoint.anchoredPosition, animationTime).SetEase(animationEase)
                .AsyncWaitForKill().AsUniTask();
            
            var wait2 = leftPanelT.DOAnchorPos(leftPanelHidePoint.anchoredPosition, animationTime).SetEase(animationEase)
                .AsyncWaitForKill().AsUniTask();

            await wait1;
            await wait2;
            
            mainPanelT.gameObject.SetActive(false);
            leftPanelT.gameObject.SetActive(false);
        }

        public async UniTask ShowPanels()
        {
            mainPanelT.gameObject.SetActive(true);
            leftPanelT.gameObject.SetActive(true);

            var wait1 = mainPanelT.DOAnchorPos(mainPanelShowPoint.anchoredPosition, animationTime).SetEase(animationEase)
                .AsyncWaitForKill().AsUniTask();
            
            var wait2 = leftPanelT.DOAnchorPos(leftPanelShowPoint.anchoredPosition, animationTime).SetEase(animationEase)
                .AsyncWaitForKill().AsUniTask();

            await wait1;
            await wait2;
        }

        public void ToMainCanvas(RectTransform target, bool maxSiblingIndex = false)
        {
            target.SetParent(canvasT.transform);
            
            const int minSiblingIndex = 2;
            target.SetSiblingIndex(maxSiblingIndex ? canvasT.transform.childCount - minSiblingIndex : 0);
            
            target.localScale = Vector3.one;
            target.localPosition = Vector3.zero;
            target.sizeDelta = Vector2.zero;
            target.localEulerAngles = Vector3.zero;
        }

        public async UniTask MoveStatsPanels(RectTransform coinsTargetPoint,RectTransform diamondsTargetPoint,RectTransform playerTargetPoint)
        {
            const Ease panelsAnimationEase = Ease.OutCirc;
            const float panelsAnimationTime = 0.175f;
            
            var moveTask1 = coinsCurrencyPanelT.DOAnchorPos(coinsTargetPoint.anchoredPosition, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();
            var scaleTask1 = coinsCurrencyPanelT.DOScale(coinsTargetPoint.localScale, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();
            
            var moveTask2 = diamondsCurrencyPanelT.DOAnchorPos(diamondsTargetPoint.anchoredPosition, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();
            var scaleTask2 = diamondsCurrencyPanelT.DOScale(diamondsTargetPoint.localScale, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();

            var moveTask3 = playerPanelT.DOAnchorPos(playerTargetPoint.anchoredPosition, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();
            var scaleTask3 = playerPanelT.DOScale(playerTargetPoint.localScale, panelsAnimationTime)
                .SetEase(panelsAnimationEase).AsyncWaitForKill().AsUniTask();

            await UniTask.WhenAll
                (moveTask1, moveTask2, moveTask3,scaleTask1,scaleTask2,scaleTask3);
        }
    }
}
