using UnityEngine;

namespace SoundBoard.Models.Game;

internal class PersistantMonoBehaviourProxy
{
    private bool _hasCalledAwake;
    private bool _hasCalledStart;
    private readonly IPersistantMonoBehaviour _proxy;

    private PersistantMonoBehaviour _shortLivedMonoBehaviour = null!;

    internal PersistantMonoBehaviourProxy(IPersistantMonoBehaviour proxy)
    {
        this._proxy = proxy;
    }
    
    private GameObject _shortLivedObject = null!;
    private GameObject NewProxyObj => _shortLivedObject = new GameObject("PersistantMonoBehaviourProxy");
    
    private void EnsureMonoBehaviour()
    {
        this._shortLivedMonoBehaviour = this.NewProxyObj.AddComponent<PersistantMonoBehaviour>();
        this._shortLivedMonoBehaviour.Set(OnDestroy, Awake, Start, Update, FixedUpdate);
    }
    
    public void Begin() => this.EnsureMonoBehaviour();

    private void OnDestroy() => this.EnsureMonoBehaviour();
    
    private void Update() => _proxy.Update();
    
    private void FixedUpdate() => _proxy.FixedUpdate();
    
    private void Awake()
    {
        if (_hasCalledAwake) return;
        _hasCalledAwake = true;

        _proxy.Awake();
    }
    
    private void Start()
    {
        if (_hasCalledStart) return;
        _hasCalledStart = true;

        _proxy.Start();
    }
}