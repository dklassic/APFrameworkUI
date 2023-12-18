using System.Collections;
using UnityEngine;

public static class CoroutineUtility
{
    public static IEnumerator WaitThenExecute(float time, System.Action callback)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
            yield return null;
        callback.Invoke();
    }
    public static IEnumerator WaitThenExecuteRealtime(float time, System.Action callback)
    {
        for (float i = 0; i < time; i += Time.unscaledDeltaTime)
            yield return null;
        callback.Invoke();
    }
    public static IEnumerator WaitThenExecute(float time, IEnumerator enumerator)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
            yield return null;
        yield return enumerator;
    }
    public static IEnumerator WaitThenExecuteRealtime(float time, IEnumerator enumerator)
    {
        for (float i = 0; i < time; i += Time.unscaledDeltaTime)
            yield return null;
        yield return enumerator;
    }
    public static IEnumerator Wait(float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
            yield return null;
    }
    public static IEnumerator WaitRealtime(float time)
    {
        for (float i = 0; i < time; i += Time.unscaledDeltaTime)
            yield return null;
    }
}