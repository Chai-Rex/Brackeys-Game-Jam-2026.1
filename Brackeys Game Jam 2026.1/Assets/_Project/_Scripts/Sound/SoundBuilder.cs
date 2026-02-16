using UnityEngine;

namespace SoundSystem {
    public class SoundBuilder {
        private readonly SoundManager _soundManager;
        private Vector3 _position = Vector3.zero;
        private bool _randomPitch;

        private float minPitchMod = -0.05f;
        private float maxPitchMod = 0.05f;

        public SoundBuilder(SoundManager i_soundManager) {
            this._soundManager = i_soundManager;
        }

        public SoundBuilder ResetFields() {
            _position = Vector3.zero;
            _randomPitch = false;
            return this;
        }


        public SoundBuilder WithPosition(Vector3 i_position) {
            this._position = i_position;
            return this;
        }

        public SoundBuilder WithRandomPitch(float i_min = -0.05f, float i_max = 0.05f) {
            this._randomPitch = true;
            this.minPitchMod = i_min;
            this.maxPitchMod = i_max;
            return this;
        }

        // TO DO 
        // With Debug = Text that shows: Name, Distance, 
        // With OnComplete Callback
        // no localization

        public void Play(SoundData i_soundData) {
            if (i_soundData == null) {
                Debug.LogError("SoundData is null");
                return;
            }

            if (!_soundManager.CanPlaySound(i_soundData)) return;

            SoundEmitter i_soundEmitter = _soundManager.Get();
            i_soundEmitter.Initialize(i_soundData);
            i_soundEmitter.transform.position = _position;
            i_soundEmitter.transform.SetParent(_soundManager._SoundsParent.transform);

            if (_randomPitch) {
                i_soundEmitter.WithRandomPitch();
            }

            if (i_soundData.FrequentSound) {
                i_soundEmitter._Node = _soundManager.FrequentSoundEmitters.AddLast(i_soundEmitter);
            }

            i_soundEmitter.Play();
        }
    }
}