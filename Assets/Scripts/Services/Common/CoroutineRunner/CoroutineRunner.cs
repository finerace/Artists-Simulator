using System.Collections;
using UnityEngine;

namespace Game.Services.Meta
{
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        Coroutine ICoroutineRunner.StartCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }
    }
}