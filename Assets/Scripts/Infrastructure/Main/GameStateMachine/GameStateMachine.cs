using Game.Infrastructure.FSM;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main
{
    
    public class GameStateMachine : StateMachine
    {
        public GameStateMachine(StatesFactory statesFactory) : base(statesFactory)
        {}

        public override void Initialize()
        {
            RegisterState(statesFactory.GetState<BootState>());
            RegisterState(statesFactory.GetState<MainMenuState>());
            
            RegisterState(statesFactory.GetState<LightGamePlayState>());
            RegisterState(statesFactory.GetState<HardGamePlayState>());
        }
    }
}