using System;

/// <summary>
/// Interface for state factories to allow generic state machine to work with any state enum
/// </summary>
public interface IStateFactory<TStateEnum> where TStateEnum : Enum {
    void SetState(TStateEnum state);
    BaseHierarchicalState GetState(TStateEnum state);
    void InitializeStates();
}