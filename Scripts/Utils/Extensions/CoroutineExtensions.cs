using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source
{
    public static class CoroutineExtensions
    {
        public static Coroutine After(this MonoBehaviour beh, float delay, Action action)
        {
            return beh != null && beh.isActiveAndEnabled ? beh.StartCoroutine(DelayRoutine(beh, delay, action)) : null;
        }

        public static Coroutine NextUpdate(this MonoBehaviour beh, Action action)
        {
            return beh != null && beh.isActiveAndEnabled ? beh.StartCoroutine(FrameUpdateCoroutine(beh, action)) : null;
        }

        public static Coroutine NextFixedUpdate(this MonoBehaviour beh, Action action)
        {
            return beh != null && beh.isActiveAndEnabled ? beh.StartCoroutine(FixedUpdateRoutine(beh, action)) : null;
        }

        static IEnumerator FixedUpdateRoutine(Object beh, Action action)
        {
            yield return new WaitForFixedUpdate();
            if (action != null && beh != null)
                action();
        }

        static IEnumerator FrameUpdateCoroutine(MonoBehaviour beh, Action action)
        {
            yield return null;
            if (action != null && beh != null)
                action();
        }

        static IEnumerator DelayRoutine(Object beh, float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            if (action != null && beh != null)
                action();
        }
    }
}
