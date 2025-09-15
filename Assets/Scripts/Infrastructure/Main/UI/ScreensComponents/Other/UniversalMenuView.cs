using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public class UniversalMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelT;
     
        [Space]
        
        [SerializeField] private RectTransform coinsCurrencyPoint;
        [SerializeField] private RectTransform diamondsCurrencyPoint;
        [SerializeField] private RectTransform playerPanelPoint;

        public CanvasGroup CanvasGroup => canvasGroup;

        public RectTransform PanelT => panelT;

        public RectTransform CoinsCurrencyPoint => coinsCurrencyPoint;

        public RectTransform DiamondsCurrencyPoint => diamondsCurrencyPoint;

        public RectTransform PlayerPanelPoint => playerPanelPoint;
    }
}