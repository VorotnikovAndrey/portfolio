using NTC.Global.Cache;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoCache
{
    [SerializeField] private Camera _camera;

    public Camera Camera => _camera;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    protected override void OnEnabled()
    {
        ProjectContext.Instance.Container.BindInstance(this);
    }

    protected override void OnDisabled()
    {
        ProjectContext.Instance.Container.Unbind<MainCamera>();
    }
}
