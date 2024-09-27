using System.Collections;
using NTC.Global.Cache;
using UnityEngine;

public class VFXLifetimeMono : MonoCache
{
    [SerializeField] private float _lifeTime;

    private Coroutine _coroutine;

    protected override void OnEnabled()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _coroutine = StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(_lifeTime);

        Destroy(gameObject);

        _coroutine = null;
    }
}