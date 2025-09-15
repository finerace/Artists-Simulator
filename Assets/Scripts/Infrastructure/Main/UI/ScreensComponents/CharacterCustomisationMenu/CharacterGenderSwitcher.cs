using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Zenject;
using System;
using Game.Services.Common.Logging;

namespace Game.Infrastructure.Main.UI
{
    public class CharacterGenderSwitcher : MonoBehaviour
    {
        [SerializeField] private Button switchButton;
        
        private ICharactersServiceFacade charactersService;
        private CharacterShopConfig shopConfig;
        
        private bool inSwapProcess = false;

        [Inject]
        private void Construct(ICharactersServiceFacade charactersService)
        {
            this.charactersService = charactersService;
            shopConfig = ConfigsProxy.CharactersAndShopConfig;
        }
        
        private void Awake()
        {
            if (switchButton != null)
                switchButton.onClick.AddListener(SwapGender);
        }
        
        private void OnDestroy()
        {
            if (switchButton != null)
                switchButton.onClick.RemoveListener(SwapGender);
        }
        
        public async void SwapGender()
        {
            if (shopConfig.MainCharacterId == null)
                return;

            if (inSwapProcess) 
                return;
            
            try
            {
                inSwapProcess = true;
                SetButtonInteractable(false);
                
                await charactersService.SwapGender(shopConfig.MainCharacterId);
            }
            catch (Exception ex)
            {
                Logs.Error(ex.Message);
            }
            finally
            {
                inSwapProcess = false;
                SetButtonInteractable(true);
            }
        }
        
        private void SetButtonInteractable(bool interactable)
        {
            if (switchButton != null)
            {
                switchButton.interactable = interactable;
            }
        }
    }
} 