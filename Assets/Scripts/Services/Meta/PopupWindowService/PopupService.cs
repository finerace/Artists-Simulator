using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Main.UI;
using Game.Additional;
using Cysharp.Threading.Tasks;
using System;
using Zenject;

namespace Game.Services.Meta
{
    
    public class PopupService : IPopupService
    {
        private readonly IAssetsService assetsService;
        private readonly MainScreenProxy mainScreenProxy;
        private PopupWindowView currentPopupView;
        private bool isPopupHidden;

        [Inject]
        public PopupService(IAssetsService assetsService, MainScreenProxy mainScreenProxy)
        {
            this.assetsService = assetsService;
            this.mainScreenProxy = mainScreenProxy;
        }
        
        public async UniTask ShowPopup(string txt, string title = null, Action action = null, string buttonText = null, PopupState state = PopupState.Information)
        {
            if (currentPopupView != null)
                return;
            
            currentPopupView = await assetsService.GetAsset<PopupWindowView>(ConfigsProxy.AssetsPathsConfig.PopupWindowID);
            
            currentPopupView.SetupContent(txt, title, action, buttonText);
            currentPopupView.SetupState(state);
            
            var canvasGroup = currentPopupView.CanvasGroup;
            var panelT = currentPopupView.PanelT;
            
            mainScreenProxy.MainScreen.ToMainCanvas(panelT, true);
            
            canvasGroup.alpha = 0;
            panelT.gameObject.SetActive(true);
            
            var scaleAnimationTask = 
                panelT.ShowScaleAnimation(ConfigsProxy.AnimationsConfig.CommonAnimationScaleDifference);
            
            await canvasGroup.ChangeCanvasAlpha(1);
            await scaleAnimationTask;
            
            currentPopupView.onClose += () => HidePopup().Forget();
            
            async UniTask HidePopup()
            {
                if (isPopupHidden)
                    return;
                
                isPopupHidden = true;

                var hideScaleAnimationTask = 
                    panelT.HideScaleAnimation(ConfigsProxy.AnimationsConfig.CommonAnimationScaleDifference);
            
                await canvasGroup.ChangeCanvasAlpha(0);
                await hideScaleAnimationTask;
                
                assetsService.ReleaseAsset(currentPopupView.gameObject);
                currentPopupView = null;
                isPopupHidden = false;
            }
        }
    }
} 