using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UtilityExtensions;

[CreateAssetMenu(fileName = "UIGamePlayManager", menuName = "ScriptableObjects/Managers/UI/UIGamePlayManager")]
public class UIGamePlayManager : ScriptableObject, IInitializable {
    [Header("Canvas")]
    [SerializeField] private AssetReferenceGameObject _iPauseCanvas;
    [SerializeField] private AssetReferenceGameObject _iHUDCanvas;
    [SerializeField] private AssetReferenceGameObject _iCreditsCanvas;


    public PauseCanvas _PauseCanvas { get; private set; }
    public MainMenuCanvas _MainMenuCanvas { get; private set; }
    public HUDCanvas _HUDCanvas { get; private set; }
    public LoadingCanvas _LoadingCanvas { get; private set; }

    public string _ManagerName => GetType().Name;

    private GameObject _CanvasParent;

    public async Task Initialize() {

        _CanvasParent = new GameObject("===== Canvas =====");

        await InstantiateCanvas();

        Resume();

    }

    private async Task InstantiateCanvas() {
        await Task.CompletedTask;
    }

    public void CleanUp() {

    }


    public void Pause() {

    }

    public void Resume() {

    }


}
