using UnityEngine;

public abstract class BaseHierarchicalState {
    protected SceneContainerSO _sceneContainer;
    protected object _context; // Generic context for the state machine

    protected bool _isRootState = false;

    private BaseHierarchicalState _currentSuperState;
    private BaseHierarchicalState _currentSubState;

    public BaseHierarchicalState(object context, SceneContainerSO sceneContainer) {
        _context = context;
        _sceneContainer = sceneContainer;
    }

    public abstract void EnterState();
    public abstract void InitializeSubState();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void CheckSwitchStates();
    public abstract void ExitState();

    public void UpdateStates() {
        Update();
        CheckSwitchStates();

        if (_currentSubState != null) {
            _currentSubState.UpdateStates();
        }
    }

    public void FixedUpdateStates() {
        FixedUpdate();

        if (_currentSubState != null) {
            _currentSubState.FixedUpdateStates();
        }
    }

    public void ExitStates() {
        ExitState();

        if (_currentSubState != null) {
            _currentSubState.ExitStates();
        }
    }

    protected void SwitchState(BaseHierarchicalState newState) {
        ExitStates();

        if (_isRootState) {
            // Update the machine's current state
            if (_context is IStateMachineContext stateMachineContext) {
                stateMachineContext.SetState(newState);
            }

            newState.EnterState();
            newState.InitializeSubState();
        } else if (_currentSuperState != null) {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(BaseHierarchicalState newSuperState) {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(BaseHierarchicalState newSubState) {
        if (_currentSubState != null) {
            _currentSubState.ExitStates();
        }

        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
        newSubState.EnterState();
        newSubState.InitializeSubState();
    }

    public BaseHierarchicalState GetCurrentSubState() {
        return _currentSubState;
    }

    public BaseHierarchicalState GetCurrentSuperState() {
        return _currentSuperState;
    }

    public bool IsRootState() {
        return _isRootState;
    }
}

/// <summary>
/// Interface to allow base state to communicate with the state machine
/// </summary>
public interface IStateMachineContext {
    void SetState(BaseHierarchicalState state);
}