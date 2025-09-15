using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.FSM
{
    public interface IEnterableState
    {
        public UniTask Enter();
    }
}