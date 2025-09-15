using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.FSM
{
    public interface IExitableState
    {
        public UniTask Exit();
    }
}
