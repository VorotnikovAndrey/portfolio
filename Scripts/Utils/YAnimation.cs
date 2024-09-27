using DG.Tweening;
using UnityEngine;

public class YAnimation : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [Space]
    [SerializeField] private Vector3 _enterVector;
    [SerializeField] private float _enterDuration;
    [SerializeField] private Ease _enterEase;
    [SerializeField] private Vector3 _exitVector;
    [SerializeField] private float _exitDuration;
    [SerializeField] private Ease _exitEase;

    private Tweener _tweener;

    public void Enter()
    {
        _tweener?.Kill();
        _tweener = _target.DOLocalMove(_enterVector, _enterDuration).SetEase(_enterEase).OnKill(() => _tweener = null);
    }

    public void Exit()
    {
        _tweener?.Kill();
        _tweener = _target.DOLocalMove(_exitVector, _exitDuration).SetEase(_exitEase).OnKill(() => _tweener = null);
    }
}
