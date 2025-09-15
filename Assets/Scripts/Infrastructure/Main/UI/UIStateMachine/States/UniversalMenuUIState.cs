using Cysharp.Threading.Tasks;
using Game.Additional;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Infrastructure.Main.UI.States
{
    
    public abstract class UniversalMenuUIState : IEnterableState, IExitableState
    {
        protected readonly IAssetsService assetsService;
        protected readonly MainScreenProxy mainScreenProxy;
        protected UniversalMenuView universalMenuView;
        protected CanvasGroup menuCanvasGroup;
        
        protected UniversalMenuUIState(
            IAssetsService assetsService,
            MainScreenProxy mainScreenProxy)
        {
            this.assetsService = assetsService;
            this.mainScreenProxy = mainScreenProxy;
        }
        
        public virtual async UniTask Enter()
        {
            menuCanvasGroup = universalMenuView.CanvasGroup;
               
            MoveCurrencyPanels();
            void MoveCurrencyPanels()
            {
                mainScreenProxy.MainScreen.MoveStatsPanels(universalMenuView.CoinsCurrencyPoint,
                    universalMenuView.DiamondsCurrencyPoint,universalMenuView.PlayerPanelPoint).Forget();
            }

            var scaleAnimationTask = 
                universalMenuView.PanelT
                    .ShowScaleAnimation(ConfigsProxy.AnimationsConfig.CommonAnimationScaleDifference);

            menuCanvasGroup.alpha = 0;
            mainScreenProxy.MainScreen.ToMainCanvas(menuCanvasGroup.GetComponent<RectTransform>());
            
            await menuCanvasGroup.ChangeCanvasAlpha(1);
            await scaleAnimationTask;
        }
        
        public virtual async UniTask Exit()
        {
            var panelT = universalMenuView.PanelT;
            
            var scaleAnimationTask = 
                panelT.HideScaleAnimation(ConfigsProxy.AnimationsConfig.CommonAnimationScaleDifference);
            
            await menuCanvasGroup.ChangeCanvasAlpha(0);
            await scaleAnimationTask;
            
            assetsService.ReleaseAsset(universalMenuView.gameObject);
        }
        
    }
}