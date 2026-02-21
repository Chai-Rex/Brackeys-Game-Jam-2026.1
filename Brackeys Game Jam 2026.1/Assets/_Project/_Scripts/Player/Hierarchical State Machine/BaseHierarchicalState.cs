using UnityEngine;

/// <summary>
/// Base class for all hierarchical states.
/// States can have a superstate (parent) and a substate (child).
/// Root states communicate directly with the state machine context.
/// </summary>
public abstract class BaseHierarchicalState {
    protected object _context;
    protected bool _isRootState = false;

    private BaseHierarchicalState _currentSuperState;
    private BaseHierarchicalState _currentSubState;

    // KEY FIX: Prevents substates from firing transitions after a root switch
    // has already occurred this frame.
    private bool _hasTransitionedThisFrame = false;

    protected BaseHierarchicalState(object context) {
        _context = context;
    }

    // --- Abstract Interface ---------------------------------------------------

    public abstract void EnterState();
    public abstract void InitializeSubState();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void CheckSwitchStates();
    public abstract void ExitState();

    // --- Update Loop ----------------------------------------------------------

    public void UpdateStates() {
        _hasTransitionedThisFrame = false;

        Update();
        CheckSwitchStates();

        // Only propagate to substate if THIS state hasn't already switched away.
        // This prevents orphaned substates from firing stale transitions.
        if (!_hasTransitionedThisFrame && _currentSubState != null) {
            _currentSubState.UpdateStates();
        }
    }

    public void FixedUpdateStates() {
        FixedUpdate();

        if (_currentSubState != null) {
            _currentSubState.FixedUpdateStates();
        }
    }

    // --- Exit -----------------------------------------------------------------

    public void ExitStates() {
        ExitState();

        if (_currentSubState != null) {
            _currentSubState.ExitStates();
        }
    }

    // --- State Switching ------------------------------------------------------

    protected void SwitchState(BaseHierarchicalState newState) {
        _hasTransitionedThisFrame = true;

        ExitStates();

        if (_isRootState) {
            if (_context is IStateMachineContext ctx) {
                ctx.SetState(newState);
            }

            newState.EnterState();
            newState.InitializeSubState();
        } else if (_currentSuperState != null) {
            _currentSuperState.SetSubState(newState);
        }
    }

    // --- Superstate / Substate Management ------------------------------------

    protected void SetSuperState(BaseHierarchicalState newSuperState) {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(BaseHierarchicalState newSubState) {
        // Mark that a transition occurred so the update loop knows to stop
        _hasTransitionedThisFrame = true;

        if (_currentSubState != null) {
            _currentSubState.ExitStates();
        }

        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
        newSubState.EnterState();
        newSubState.InitializeSubState();
    }

    // --- Accessors ------------------------------------------------------------

    public BaseHierarchicalState GetCurrentSubState() => _currentSubState;
    public BaseHierarchicalState GetCurrentSuperState() => _currentSuperState;
    public bool IsRootState() => _isRootState;
}

/// <summary>
/// Interface allowing states to communicate state changes back to the state machine.
/// </summary>
public interface IStateMachineContext {
    void SetState(BaseHierarchicalState state);
}
