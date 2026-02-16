using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SoundSystem {
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour {

        [Header("References")]
        [SerializeField] private SoundManager _iSoundManager;

        public SoundData _Data { get; private set; }
        public LinkedListNode<SoundEmitter> _Node { get; set; }

        private AudioSource _audioSource;
        private Coroutine _playingCoroutine;

        private void Awake() {
            _audioSource = gameObject.GetComponent<AudioSource>();
        }
        public void Initialize(SoundData i_data) {
            _Data = i_data;
            _audioSource.clip = i_data.Clip;
            _audioSource.outputAudioMixerGroup = i_data.MixerGroup;
            _audioSource.loop = i_data.Loop;
            _audioSource.playOnAwake = i_data.PlayOnAwake;

            _audioSource.mute = i_data.Mute;
            _audioSource.bypassEffects = i_data.BypassEffects;
            _audioSource.bypassListenerEffects = i_data.BypassListenerEffects;
            _audioSource.bypassReverbZones = i_data.BypassReverbZones;

            _audioSource.priority = i_data.Priority;
            _audioSource.volume = i_data.Volume;
            _audioSource.pitch = i_data.Pitch;
            _audioSource.panStereo = i_data.PanStereo;
            _audioSource.spatialBlend = i_data.SpatialBlend;
            _audioSource.reverbZoneMix = i_data.ReverbZoneMix;
            _audioSource.dopplerLevel = i_data.DopplerLevel;
            _audioSource.spread = i_data.Spread;

            _audioSource.minDistance = i_data.MinDistance;
            _audioSource.maxDistance = i_data.MaxDistance;

            _audioSource.ignoreListenerVolume = i_data.IgnoreListenerVolume;
            _audioSource.ignoreListenerPause = i_data.IgnoreListenerPause;

            _audioSource.rolloffMode = i_data.rolloffMode;
        }

        public void Play() {
            if (_playingCoroutine != null) {
                StopCoroutine(_playingCoroutine);
            }

            _audioSource.Play();
            _playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        public void Stop() {
            if (_playingCoroutine != null) {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }

            _audioSource.Stop();
            _iSoundManager.ReturnToPool(this);
        }

        IEnumerator WaitForSoundToEnd() {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            Stop();
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f) {
            _audioSource.pitch += Random.Range(min, max);
        }
    }
}

