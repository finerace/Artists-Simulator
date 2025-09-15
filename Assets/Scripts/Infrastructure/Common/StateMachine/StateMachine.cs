using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.FSM
{
    
    public abstract class StateMachine
    {
        private Dictionary<Type,(IExitableState,IEnterableState)> states;
        private (IExitableState,IEnterableState) currentState;
        
        protected readonly StatesFactory statesFactory;

        private bool isAwaiting;
        public bool IsAwaiting => isAwaiting;

        private Type currentStateType;
        public Type CurrentStateType => currentStateType;

        protected StateMachine(StatesFactory statesFactory)
        {
            this.statesFactory = statesFactory;
        }
        
        public abstract void Initialize();
        
        public UniTask EnterState<TState>(bool isExitAwaitedFirst = true,
            bool ignoreIfNewStateIsCurrent = false)
        {
            return EnterState(typeof(TState),isExitAwaitedFirst,ignoreIfNewStateIsCurrent);
        }
        
        public async UniTask EnterState(Type stateType,bool isExitAwaitedFirst = true,
            bool isIgnoreIfNewStateIsCurrent = false)
        {
            if (currentStateType == stateType && !isIgnoreIfNewStateIsCurrent)
                return;
            
            IsTypeAState(stateType);

            isAwaiting = true;

            UniTask exitTask = UniTask.CompletedTask;
            if (currentState.Item1 != null)
            {
                Logs.Info($"ExitState: {currentState.Item1.GetType().Name}");
                
                exitTask = currentState.Item1.Exit();
                
                if (isExitAwaitedFirst)
                    await exitTask;
            }
            
            SetCurrentStates(states[stateType]);
            void SetCurrentStates((IExitableState,IEnterableState) state)
            {
                currentState.Item1 = null;
                currentState.Item2 = null;
                
                if (state.Item1 != null)
                    currentState.Item1 = state.Item1;
                
                if (state.Item2 != null)
                    currentState.Item2 = state.Item2;

                currentStateType = stateType;
            }
            
            Logs.Info($"EnterState: {currentState.Item2.GetType().Name}");
            
            if(currentState.Item2 != null)
                await currentState.Item2.Enter();
            
            if(!isExitAwaitedFirst)
                await exitTask;
            
            isAwaiting = false;
        }
        
        public UniTask WaitAwaiting()
        {
            return UniTask.WaitUntil(() => !isAwaiting);
        }
        
        protected void RegisterState<TState>(TState state)
        {
            states ??= new Dictionary<Type, (IExitableState, IEnterableState)>();
            
            IsTypeAState(typeof(TState));
            
            (IExitableState, IEnterableState) newState = (null, null);
            
            if (state != null)
            {
                if (typeof(TState).GetInterface(nameof(IExitableState)) != null)
                    newState.Item1 = (IExitableState)state;
                
                if (typeof(TState).GetInterface(nameof(IEnterableState)) != null)
                    newState.Item2 = (IEnterableState)state;
            }
            
            states[typeof(TState)] = newState;
        }
        
        internal static void IsTypeAState(Type targetType)
        {
            string exitInterfaceName = nameof(IExitableState);
            string enterInterfaceName = nameof(IEnterableState);

            if (targetType.GetInterface(exitInterfaceName) == null &&
                targetType.GetInterface(enterInterfaceName) == null)
            {
                throw new ArgumentException($"Type is not a " +
                                            $"{exitInterfaceName} or {enterInterfaceName}!");
            }
        }
    }
}