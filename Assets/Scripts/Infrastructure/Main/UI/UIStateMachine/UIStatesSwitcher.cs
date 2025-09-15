using Cysharp.Threading.Tasks;
using Game.Infrastructure.Main.UI.States;
using Game.Additional.MagicAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    
    public class UIStatesSwitcher : MonoBehaviour, IPointerClickHandler
    {
        private UIStateMachine stateMachine;
        
        [SerializeField] private UIState targetState;
        
        [Inject]
        private void Construct(UIStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!stateMachine.IsAwaiting)
                EnterState(targetState);    
        }

        private void EnterState(UIState state)
        {
            switch (state)
            {
                case UIState.BootState:
                    EnterState<BootUIState>();
                    break;
                
                case UIState.MainMenuState:
                    EnterState<MainMenuUIState>();
                    break;
                
                case UIState.SettingsMenuState:
                    EnterState<SettingsMenuUIState>(false);
                    break;

                case UIState.DonateMenuState:
                    EnterState<DonateMenuUIState>(false);
                    break;
                
                case UIState.LocationsImproveState:
                    EnterState<LocationImproveMenuUIState>(false);
                    break;
                
                case UIState.CharacterCustomisationState:
                    EnterState<CharacterCustomisationUIState>(false);
                    break;
                
                case UIState.ImproveState:
                    EnterState<ImprovementMenuUIState>(false);
                    break;
                
                case UIState.DifficultySwitchState:
                    EnterState<DifficultySwitchMenuUIState>(false);
                    break;
                
                case UIState.LevelBonusesState:
                    EnterState<LevelBonusesMenuUIState>(false);
                    break;
                
                case UIState.GamePlayState:
                    EnterState<GamePlayMenuUIState>(false);
                    break;
            }
        }

        private void EnterState<TState>(bool isExitAwaitedFirst = true)
        {
            StateItselfExitSkipCheck();
            void StateItselfExitSkipCheck()
            {
                if (!isExitAwaitedFirst)
                    isExitAwaitedFirst = stateMachine.CurrentStateType == typeof(TState);
            }
            
            stateMachine.EnterState<TState>(isExitAwaitedFirst).Forget();
        }
        
        private enum UIState
        {
            BootState = 0,
            MainMenuState = 1,
            SettingsMenuState = 2,
            DonateMenuState = 3,
            LocationsImproveState = 4,
            CharacterCustomisationState = 5,
            ImproveState = 6,
            DifficultySwitchState = 7,
            LevelBonusesState = 8,
            GamePlayState = 9,
        }
    }
}