using Zenject;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.FSM
{
    
    public class StatesFactory
    {
        private readonly DiContainer diContainer;

        public StatesFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        public TState GetState<TState>()
        {
            StateMachine.IsTypeAState(typeof(TState));
            
            return diContainer.Resolve<TState>();
        }

        // private bool IsDiContainerHaveState(Type targetState)
        // {
        //     var dependencyContracts 
        //         = diContainer.GetDependencyContracts(targetState);
        //     
        //     
        // }
    }
}