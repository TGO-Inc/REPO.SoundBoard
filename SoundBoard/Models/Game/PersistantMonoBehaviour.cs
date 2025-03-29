using UnityEngine;

namespace SoundBoard.Models.Game;

internal sealed class PersistantMonoBehaviour : MonoBehaviour
{
    private Action _onDestroy = null!;
    private Action? _onAwake;
    private Action? _onStart;
    private Action? _onUpdate;
    private Action? _onFixedUpdate;
    
    public void Set(Action onDestroy, Action? awake = null, Action? start = null, Action? update = null, Action? fixedUpdate = null)
    {
        _onDestroy = onDestroy;
        _onAwake = awake;
        _onStart = start;
        _onUpdate = update;
        _onFixedUpdate = fixedUpdate;
    }
    private void OnDestroy() => _onDestroy.Invoke();
    private void Awake() => _onAwake?.Invoke();
    private void Start() => _onStart?.Invoke();
    private void Update() => _onUpdate?.Invoke();
    private void FixedUpdate() => _onFixedUpdate?.Invoke();
}