using System.Collections;
using UnityEngine;

namespace Game.Services.Meta
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator routine);
    }
}