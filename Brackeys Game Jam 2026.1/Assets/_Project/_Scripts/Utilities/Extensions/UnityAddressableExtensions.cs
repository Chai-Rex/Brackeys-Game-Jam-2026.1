using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

namespace UtilityExtensions {
    public static class UnityAddressableExtensions {
        public static async Task<T> InstantiateAsync<T>( AssetReferenceGameObject reference, Transform parent = null) where T : Component {

            AsyncOperationHandle<GameObject> handle = reference.InstantiateAsync(parent);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"Failed to instantiate addressable: {reference.RuntimeKey}");
                return null;
            }

            GameObject instance = handle.Result;

            T component = instance.GetComponent<T>();

            if (component == null) {
                Debug.LogError(
                    $"Instantiated object '{instance.name}' does not contain component {typeof(T).Name}");
            }

            return component;
        }
    }
}
