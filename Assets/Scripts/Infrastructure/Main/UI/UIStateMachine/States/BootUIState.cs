using Cysharp.Threading.Tasks;
using Game.Infrastructure.FSM;

namespace Game.Infrastructure.Main.UI.States
{
    public class BootUIState : IEnterableState
    {
        private readonly MainScreenProxy mainScreenProxy;

        public BootUIState(
            MainScreenProxy mainScreenProxy)
        {
            this.mainScreenProxy = mainScreenProxy;
        }
        
        public async UniTask Enter()
        {
            await mainScreenProxy.Initialize();
        }
    }
}