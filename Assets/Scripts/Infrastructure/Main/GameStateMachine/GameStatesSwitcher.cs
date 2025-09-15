using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main
{
    
    public class GameStatesSwitcher : MonoBehaviour, IPointerClickHandler
    {
        private GameStateMachine stateMachine;
        
        [SerializeField] private GameStates targetState;
        [SerializeField] private bool waitingStateMachine;
        private bool isPressed;
        
        [Inject]
        private void Construct(GameStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!stateMachine.IsAwaiting)
                EnterState(targetState);    
        }

        private void EnterState(GameStates state)
        {
            switch (state)
            {
                case GameStates.BootState:
                    EnterState<BootState>();
                    break;
                
                case GameStates.MainMenuState:
                    EnterState<MainMenuState>();
                    break;
                
                case GameStates.LightGamePlayState:
                    EnterState<LightGamePlayState>();
                    break;
                
                case GameStates.HardGamePlayState:
                    EnterState<HardGamePlayState>();
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
        
        private enum GameStates
        {
            BootState = 0,
            MainMenuState = 1,
            LightGamePlayState = 2,
            HardGamePlayState = 3,
        }
    }
}