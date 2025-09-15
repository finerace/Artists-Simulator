using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.UI.States;

namespace Game.Infrastructure.Main.UI
{
    public class UIStateMachine : StateMachine
    {
        public UIStateMachine(StatesFactory statesFactory) : base(statesFactory)
        {}

        public override void Initialize()
        {
            RegisterState(statesFactory.GetState<BootUIState>());
            RegisterState(statesFactory.GetState<MainMenuUIState>());
            
            RegisterState(statesFactory.GetState<SettingsMenuUIState>());
            RegisterState(statesFactory.GetState<DonateMenuUIState>());
            RegisterState(statesFactory.GetState<LocationImproveMenuUIState>());
            RegisterState(statesFactory.GetState<CharacterCustomisationUIState>());
            RegisterState(statesFactory.GetState<ImprovementMenuUIState>());
            RegisterState(statesFactory.GetState<LevelBonusesMenuUIState>());
            
            RegisterState(statesFactory.GetState<DifficultySwitchMenuUIState>());
            RegisterState(statesFactory.GetState<EnemySearchMenuUIState>());
            RegisterState(statesFactory.GetState<GamePlayMenuUIState>());
        }

    }
}
