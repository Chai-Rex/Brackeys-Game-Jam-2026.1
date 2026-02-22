using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Command interface for game commands.
/// Provides type-safe, explicit API for all game actions.
/// </summary>
[CreateAssetMenu(fileName = "GameCommandsManager", menuName = "ScriptableObjects/Managers/GameCommandsManager")]
public class GameCommandsManager : ScriptableObject, IInitializable, ICleanable, IPersistentManager {

    public string _ManagerName => "GameCommandsManager";

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Reference to GameBootstrap (set at initialization)
    [SerializeField] private GameBootstrap _gameBootstrap;

    ////////////////////////////////////////////////////////////
    /// Initialization
    ////////////////////////////////////////////////////////////

    public Task Initialize() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Register GameBootstrap so commands can be executed
    /// </summary>
    public void RegisterBootstrap(GameBootstrap bootstrap) {
        _gameBootstrap = bootstrap;
        Log("GameBootstrap registered");
    }

    public void CleanUp() {
        _gameBootstrap = null;
    }

    ////////////////////////////////////////////////////////////
    /// Public Commands (Called by UI)
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Start the gameplay scene
    /// </summary>
    public void BeginGame() {
        Log("Command: StartGame");

        if (_gameBootstrap == null) {
            _gameBootstrap = Object.FindFirstObjectByType<GameBootstrap>();

        }

        _gameBootstrap.BeginGame();
    }

    /// <summary>
    /// Load a specific level by name
    /// </summary>
    public void LoadLevel(string levelName) {
        Log($"Command: LoadLevel({levelName})");

        if (_gameBootstrap == null) {
            _gameBootstrap = Object.FindFirstObjectByType<GameBootstrap>();

        }

        _gameBootstrap.LoadScene(levelName);
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame() {
        Log("Command: PauseGame");

        if (_gameBootstrap == null) {

            _gameBootstrap = Object.FindFirstObjectByType<GameBootstrap>();

        }

        _gameBootstrap.PauseGame();
    }

    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame() {
        Log("Command: ResumeGame");

        if (_gameBootstrap == null) {
            _gameBootstrap = Object.FindFirstObjectByType<GameBootstrap>();

        }

        _gameBootstrap.ResumeGame();
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void QuitToMainMenu() {
        Log("Command: QuitToMainMenu");

        if (_gameBootstrap == null) {
            _gameBootstrap = Object.FindFirstObjectByType<GameBootstrap>();

        }

        _gameBootstrap.ReturnToMainMenu();
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitApplication() {
        Log("Command: QuitApplication");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    ////////////////////////////////////////////////////////////
    /// Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_enableDebugLogs) {
            Debug.Log($"[GameCommands] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[GameCommands] {i_message}");
    }
}