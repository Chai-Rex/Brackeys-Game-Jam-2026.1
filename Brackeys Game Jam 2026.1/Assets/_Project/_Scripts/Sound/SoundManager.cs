using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace SoundSystem {

    [CreateAssetMenu(fileName = "SoundManager", menuName = "ScriptableObjects/Managers/SoundManager")]
    public class SoundManager : ScriptableObject, IInitializable, ICleanable, IPersistentManager {

        [Header("References")]
        [SerializeField] private AudioMixer _iMixer;
        [SerializeField] private SoundEmitter soundEmitterPrefab;

        [Header("Settings")]
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private int maxSoundInstances = 30;

        private IObjectPool<SoundEmitter> _soundEmitterPool;
        private readonly List<SoundEmitter> activeSoundEmitters = new();
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();
        
        public GameObject _SoundsParent { get; private set; }


        public string _ManagerName => GetType().Name;

        public async Task Initialize() {
            _SoundsParent = new GameObject("===== Sounds =====");
            InitializePool();
            await Task.Yield();
        }

        private void InitializePool() {
            _soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize
            );
        }


        public void CleanUp() {

        }

        public bool CanPlaySound(SoundData data) {
            if (!data.FrequentSound) return true;

            if (FrequentSoundEmitters.Count >= maxSoundInstances) {
                try {
                    FrequentSoundEmitters.First.Value.Stop();
                    return true;
                } catch {
                    Debug.Log("SoundEmitter is already released");
                }
                return false;
            }
            return true;
        }

        public SoundEmitter Get() {
            return _soundEmitterPool.Get();
        }

        public void ReturnToPool(SoundEmitter soundEmitter) {
            _soundEmitterPool.Release(soundEmitter);
        }

        private void OnDestroyPoolObject(SoundEmitter soundEmitter) {
            Destroy(soundEmitter.gameObject);
        }

        private void OnReturnedToPool(SoundEmitter soundEmitter) {
            if (soundEmitter._Node != null) {
                FrequentSoundEmitters.Remove(soundEmitter._Node);
                soundEmitter._Node = null;
            }
        }

        private void OnTakeFromPool(SoundEmitter soundEmitter) {
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }

        /// <summary>
        /// Example Usage:
        /// SoundManager.CreateSoundEmitter()
        /// .WithSoundData(soundData.OnCollisionSoundData)
        /// .WithRandomPitch()
        /// .WithPosition(transform.postion)
        /// .Play();
        /// </summary>
        private SoundEmitter CreateSoundEmitter() {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        public void SetMixerFloat(string mixerParam, float value) {
            // Clamp to avoid Log10(0)
            value = Mathf.Clamp(value, 0.0001f, 1f);

            float dB = Mathf.Log10(value) * 20f;
            _iMixer.SetFloat(mixerParam, dB);
        }
    }
}

