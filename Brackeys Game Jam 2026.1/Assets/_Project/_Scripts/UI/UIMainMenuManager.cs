using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UtilityExtensions;

[CreateAssetMenu(fileName = "UIMainMenuManager", menuName = "ScriptableObjects/Managers/UI/UIMainMenuManager")]
public class UIMainMenuManager : ScriptableObject, IInitializable {


    [Header("Canvas")]
    [SerializeField] private AssetReferenceGameObject _iMainMenuCanvas;

    public MainMenuCanvas _MainMenuCanvas { get; private set; }


    private bool _isInitialized = false;
    public bool _IsInitialized => _isInitialized;

    public string _ManagerName => GetType().Name;

    private GameObject _CanvasParent;

    public async Task Initialize() {

        //_CanvasParent = new GameObject("===== Main Menu Canvas =====");

        //_MainMenuCanvas = await UnityAddressableExtensions.InstantiateAsync<MainMenuCanvas>(_iMainMenuCanvas, _CanvasParent.transform);

        //_MainMenuCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;

        _isInitialized = true;

        await Task.Yield();
    }


}
