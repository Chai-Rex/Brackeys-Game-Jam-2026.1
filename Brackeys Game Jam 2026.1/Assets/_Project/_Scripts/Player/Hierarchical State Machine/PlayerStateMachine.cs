using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateMachine", menuName = "ScriptableObjects/Managers/Player/PlayerStateMachine")]
public class PlayerStateMachineSO : ScriptableObject, IInitializable, IUpdateable, IFixedUpdateable, IStateMachineContext {
    [Header("Debug")]
    public bool debugStates = false;

    [Header("References")]
    public SceneContainerSO SceneContainer;

    private PlayerStateFactory _factory;
    private BaseHierarchicalState _currentState;

    public string _ManagerName => GetType().Name;

    public async Task Initialize() {
        if (SceneContainer == null) {
            Debug.LogError($"[{_ManagerName}] SceneContainer is not assigned!");
        }

        _factory = new PlayerStateFactory(this, SceneContainer);
        _factory.InitializeStates();
        _factory.SetState(PlayerStateFactory.PlayerStates.Idling);

        await Task.Yield();
    }

    public void SetState(BaseHierarchicalState state) {
        if (debugStates && _currentState != null) {
            Debug.Log($"[{_ManagerName}] State Change: {_currentState.GetType().Name} -> {state.GetType().Name}");
        }
        _currentState = state;
    }

    public BaseHierarchicalState GetCurrentState() {
        return _currentState;
    }

    public PlayerStateFactory GetFactory() {
        return _factory;
    }

    public void OnUpdate() {
        _currentState?.UpdateStates();
    }

    public void OnFixedUpdate() {
        _currentState?.FixedUpdateStates();
    }
}