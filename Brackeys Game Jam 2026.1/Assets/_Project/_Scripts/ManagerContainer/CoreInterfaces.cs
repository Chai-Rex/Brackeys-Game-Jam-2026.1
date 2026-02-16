using System.Threading.Tasks;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Core Interfaces
////////////////////////////////////////////////////////////

public interface IManager {
    string _ManagerName { get; }
}

public interface IInitializable : IManager {
    bool _IsInitialized { get; }
    Task Initialize();
}

public interface ICleanable : IManager {
    void CleanUp();
}

public interface IUpdateable : IManager {
    void OnUpdate();
}

public interface IFixedUpdateable : IManager {
    void OnFixedUpdate();
}

public interface IPausable : IManager {
    void OnPause();
    void OnResume();
}

////////////////////////////////////////////////////////////
/// MARKER INTERFACES (Design Pattern: Type Tags)
////////////////////////////////////////////////////////////

/// <summary>
/// Marker interface for managers that persist across all scenes.
/// Initialized by GameBootstrap, never cleaned up by scene containers.
/// </summary>
public interface IPersistentManager : IManager { }
