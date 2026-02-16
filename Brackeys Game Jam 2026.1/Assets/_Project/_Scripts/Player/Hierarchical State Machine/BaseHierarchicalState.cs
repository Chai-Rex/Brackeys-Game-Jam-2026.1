using UnityEngine;
using UnityEngine.Playables;

public abstract class BaseHierarchicalState {

    protected SceneContainerSO _sceneContainer;
    protected HierarchicalStateFactory _factory;
    protected HierarchicalStateMachine _machine; 

    protected bool _isRootState = false;

    private BaseHierarchicalState _currentSuperState;
    private BaseHierarchicalState _currentSubState;

    public BaseHierarchicalState(HierarchicalStateMachine currentContext) { 
        _sceneContainer = currentContext.SceneContainer;
        _factory = currentContext.Factory;
        _machine = currentContext; 
    }

    public abstract void EnterState();

    public abstract void InitializeSubState();

    public abstract void Update();

    public abstract void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

    public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

    public abstract void CheckSwitchStates();

    public abstract void ExitState();

    public void UpdateStates() {
        Update();
        if (_currentSubState != null) {
            _currentSubState.UpdateStates();
        }
    }

    public void UpdateRotationStates(ref Quaternion currentRotation, float deltaTime) {
        UpdateRotation(ref currentRotation, deltaTime);
        if (_currentSubState != null) {
            _currentSubState.UpdateRotationStates(ref currentRotation, deltaTime);
        }
    }

    public void UpdateVelocityStates(ref Vector3 currentVelocity, float deltaTime) {
        UpdateVelocity(ref currentVelocity, deltaTime);
        if (_currentSubState != null) {
            _currentSubState.UpdateVelocityStates(ref currentVelocity, deltaTime);
        }
    }

    public void ExitStates() {
        ExitState();
        if (_currentSubState != null) {
            _currentSubState.ExitState();
        }
    }

    protected void SwitchState(BaseHierarchicalState newState) {
        ExitStates();
        if (_isRootState) {
            newState.EnterState();
            _machine.SetState(newState);
        } else if (_currentSuperState != null) {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(BaseHierarchicalState newSuperState) {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(BaseHierarchicalState newSubState) {
        _currentSubState = newSubState;
        newSubState.EnterState();
        newSubState.SetSuperState(this);
    }

    public BaseHierarchicalState GetSubState() {
        return _currentSubState;
    }
}
