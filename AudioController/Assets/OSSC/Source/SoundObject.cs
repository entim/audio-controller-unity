using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using MEC;

namespace OSSC {
    /// <summary>
    /// Used by the SoundCue.
    /// Controls the AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundObject : MonoBehaviour, IPoolable {

        /// <summary>
        /// Called when SoundObject finishes playing.
        /// </summary>
        public System.Action<SoundObject> OnFinishedPlaying;

        #region private fields
        /// <summary>
        /// SoundObject's ID.
        /// </summary>
        private string _id;
        /// <summary>
        /// Played AudioClip.
        /// </summary>
        private AudioClip _clip;
        /// <summary>
        /// The thing that playes the sound.
        /// </summary>
        private AudioSource _source;

        /// <summary>
        /// Curoutine used for playing the sound.
        /// </summary>
        private string _playingRoutineTag;
        /// <summary>
        /// Flag to check if SoundObject is paused.
        /// </summary>
        private bool _isPaused;
        /// <summary>
        /// Reference to the pool that this SoundObject belongs to.
        /// </summary>
        private PrefabBasedPool _pool;
        /// <summary>
        /// Fade In time.
        /// </summary>
        private float _fadeInTime;
        /// <summary>
        /// Fade Out time.
        /// </summary>
        private float _fadeOutTime;
        /// <summary>
        /// Volume of the sound.
        /// </summary>
        private float _volume;
        /// <summary>
        /// Pitch of the sound.
        /// </summary>
        private float _pitch;
        private bool _isDespawnOnFinishedPlaying = true;

        private bool _applicationQuitting;

        private readonly string mecTag = Guid.NewGuid().ToString();
        #endregion

        #region Public methods and properties

        void OnDestroy() {
            //if (!_applicationQuitting) Debug.LogError(_id + " - " + GetHashCode() + " sound object is being destroyed while the game is running. This probably indicates a bug.");
            Timing.KillCoroutines(mecTag);
            Timing.KillCoroutines(_playingRoutineTag);
        }

        void OnApplicationQuit() {
            _applicationQuitting = true;
        }

        /// <summary>
        /// Check whether SoundObject should despawn after finishing playing.
        /// </summary>
        public bool isDespawnOnFinishedPlaying {
            get { return _isDespawnOnFinishedPlaying; }
            set { _isDespawnOnFinishedPlaying = value; }
        }

        /// <summary>
        /// AudioClip name played.
        /// </summary>
        public string clipName {
            get {
                return _clip != null ? _clip.name : "NONE";
            }
        }

        /// <summary>
        /// Gets the SoundObject's AudioSource.
        /// </summary>
        public AudioSource source {
            get { return _source; }
        }

        /// <summary>
        /// Gets the SoundObject's ID.
        /// </summary>
        public string ID {
            get { return _id; }
        }

        /// <summary>
        /// Prepares the SoundObject for playing an AudioClip.
        /// </summary>
        /// <param name="id">SoundObject's ID</param>
        /// <param name="clip">AudioClip to play</param>
        /// <param name="volume">volume of the sound.</param>
        /// <param name="fadeInTime">Fade In Time</param>
        /// <param name="fadeOutTime">Fade Out Time</param>
        /// <param name="mixer">Audio Mixer group</param>
        /// <param name="pitch">Pitch of the sound</param>
        public void Setup(string id, AudioClip clip, int priority, float volume, float fadeInTime = 0f, float fadeOutTime = 0f, AudioMixerGroup mixer = null, float pitch = 1f) {
            _id = id;
            _clip = clip;
            gameObject.name = _id;
            if (_source == null)
                _source = GetComponent<AudioSource>();
            _source.volume = 0;
            _source.priority = priority;
            _source.time = 0f;
            _source.outputAudioMixerGroup = mixer;
            _volume = volume;
            _pitch = pitch;
            _fadeInTime = fadeInTime;
            _fadeOutTime = fadeOutTime;
        }

        public void SwitchClip(string id, AudioClip clip) {
            _clip = clip;
            if (_source == null)
                _source = GetComponent<AudioSource>();
            _source.time = 0f;
        }

        /// <summary>
        /// Plays the AudioSource.
        /// </summary>
        public void Play() {
            if (_source == null)
                _source = GetComponent<AudioSource>();
            _source.clip = _clip;
            gameObject.SetActive(true);
            _source.pitch = _pitch;
            _source.enabled = true;
            Timing.RunCoroutine(FadeRoutine(_fadeInTime, _volume), mecTag);
            _source.Play();
            _playingRoutineTag = Guid.NewGuid().ToString();
            Timing.RunCoroutine(PlayingRoutine(), _playingRoutineTag);
        }

        /// <summary>
        /// Pauses the AudioSource.
        /// </summary>
        public void Pause() {
            if (_source == null)
                return;
            _isPaused = true;
            _source.Pause();
        }

        /// <summary>
        /// Resumes from Pause.
        /// </summary>
        public void Resume() {
            if (_source == null)
                return;
            _source.Play();
            _isPaused = false;
        }

        /// <summary>
        /// Stops the SoundObject from playing.
        /// </summary>
        public void Stop() {
            if (_playingRoutineTag == null)
                return;

            Timing.RunCoroutine(StopRoutine(), mecTag);
        }

        public float Volume {
            get { return _source.volume; }
            set { _source.volume = value; }
        }

        /// <summary>
        /// Internal test method
        /// </summary>
        [ContextMenu("Test Play")]
        private void TestPlay() {
            Play();
        }
        #endregion

        /// <summary>
        /// Fades the volume of the AudioSource.
        /// </summary>
        /// <param name="fadeTime">time to fade</param>
        /// <param name="value">target volume to fade to.</param>
        /// <returns></returns>
        private IEnumerator<float> FadeRoutine(float fadeTime, float value) {
            if (fadeTime < 0.1f) {
                _source.volume = value;
                yield break;
            }

            float initVal = _source.volume;
            float fadeSpeed = 1 / fadeTime;
            float fadeStep = Timing.DeltaTime * fadeSpeed;
            for (float t = 0f; t < 1f; t += fadeStep) {
                float val = Mathf.SmoothStep(initVal, value, t);
                _source.volume = val;
                yield return Timing.WaitForOneFrame;
            }
            _source.volume = value;
        }

        /// <summary>
        /// Internal method to Stop the SoundObject.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> StopRoutine() {
            Timing.KillCoroutines(_playingRoutineTag);
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeRoutine(_fadeOutTime, 0f), mecTag));
            _source.Stop();
            _source.clip = null;
            _playingRoutineTag = null;
            _volume = 0f;
            _source.time = 0f;
            _source.pitch = 1f;

            if (isDespawnOnFinishedPlaying)
                _pool.Despawn(gameObject);

            if (OnFinishedPlaying != null) {
                OnFinishedPlaying(this);
            }
        }

        /// <summary>
        /// Internal method to play the SoundObject.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> PlayingRoutine() {
            while (true) {
                yield return Timing.WaitForOneFrame;

                if (_source.clip == null) yield break;

                float fadeOutTrigger = _source.clip.length - _fadeOutTime;
                if (_source.time >= fadeOutTrigger) {
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeRoutine(_fadeOutTime, 0f), mecTag));
                }
                if (!_source.isPlaying && !_isPaused) {
                    break;
                }
            }

            _source.clip = null;
            _playingRoutineTag = null;
            _source.time = 0f;

            if (isDespawnOnFinishedPlaying)
                _pool.Despawn(gameObject);

            if (OnFinishedPlaying != null) {
                OnFinishedPlaying(this);
            }
        }

        #region IPoolable methods
        /// <summary>
        /// Check IPoolable
        /// </summary>
        PrefabBasedPool IPoolable.pool {
            get { return _pool; }
            set { _pool = value; }
        }

        /// <summary>
        /// Check IPoolable
        /// </summary>
        public bool IsFree() {
            return !_pool.IsActive(gameObject);
        }
        #endregion
    }

}
