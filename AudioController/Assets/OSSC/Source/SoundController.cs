using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using OSSC.Model;

namespace OSSC {
    /// <summary>
    /// The main class that is used for Playing and controlling all sounds.
    /// </summary>
    [RequireComponent(typeof(ObjectPool))]
    public class SoundController : MonoBehaviour {

        public static SoundController instance = null;

        #region Serialized Data
        /// <summary>
        /// Default prefab with SoundObject and AudioSource.
        /// It is used by the Soundcontroller to play SoundCues.
        /// </summary>
        [Tooltip("This prefab will be instantiated to play audio clips. The prefab must have the following components: Audio Source, Sound Object.")]
        public GameObject _audioObjectPrefab;
        /// <summary>
        /// Saves all the data that the SoundController uses.
        /// </summary>
        public SoundControllerData _database;

        #endregion

        #region Private fields

        /// <summary>
        /// Gives instances of GameObjects thrown in it.
        /// </summary>
        private ObjectPool _pool;
        /// <summary>
        /// Manages all created SoundCues.
        /// </summary>
        private CueManager _cueManager;
        /// <summary>
        /// Initial pool size of SoundCues for CueManager.
        /// </summary>
        private int _initialCueManagerSize = 10;

        private readonly Dictionary<string, float> _activeSoundItems = new Dictionary<string, float>();

        #endregion

        #region Public methods and properties

        /// <summary>
        /// Set the default Prefab with SoundObject and AudioSource in it.
        /// </summary>
        public GameObject defaultPrefab {
            set {
                _audioObjectPrefab = value;
            }
        }

        /// <summary>
        /// Stop all Playing Sound Cues.
        /// </summary>
        /// <param name="shouldCallOnEndCallback">Control whether to call the OnEnd event, or not.</param>
        public void StopAll(bool shouldCallOnEndCallback = true) {
            _cueManager.StopAllCues(shouldCallOnEndCallback);
        }

        /// <summary>
        /// Set mute a category.
        /// </summary>
        /// <param name="categoryName">Name of the cateogory</param>
        /// <param name="value">True to mute, false to unmute</param>
        public void SetMute(string categoryName, bool value) {
            for (int i = 0; i < _database.items.Length; i++) {
                if (_database.items[i].name == categoryName) {
                    _database.items[i].isMute = value;
                }
            }
        }

        /// <summary>
        /// Creates a SoundCue and plays it.
        /// </summary>
        /// <param name="settings">A struct which contains all data for SoundController to work</param>
        /// <returns>A soundCue interface which can be subscribed to it's events.</returns>
        public ISoundCue Play(PlaySoundSettings settings) {
            if (settings.soundCueProxy != null) {
                return PlaySoundCue(settings);
            }

            if (settings.categoryId == null || settings.soundItemId == null) {
                return null;
            }

            float fadeInTime = settings.fadeInTime;
            float fadeOutTime = settings.fadeOutTime;
            Transform parent = settings.parent;

            CategoryItem category = null;
            GameObject prefab = null;


            category = Array.Find(_database.items, (cat) => cat.id == settings.categoryId);

            // Debug.Log(category);
            if (category == null)
                return null;

            prefab = category.usingDefaultPrefab ? _audioObjectPrefab : category.audioObjectPrefab;

            SoundItem item = Array.Find(category.soundItems, (sItem) => sItem.id == settings.soundItemId);

            if (ItemAlreadyPlaying(item)) return null;

            if (item == null || category.isMute) return null;

            SoundCue cue = _cueManager.GetSoundCue();
            SoundCueData data;
            data.audioPrefab = prefab;
            data.soundItem = item;
            data.category = category;
            data.fadeInTime = fadeInTime;
            data.fadeOutTime = fadeOutTime;
            data.isFadeIn = data.fadeInTime >= 0.1f;
            data.isFadeOut = data.fadeOutTime >= 0.1f;
            cue.AudioObject = _pool.GetFreeObject(prefab).GetComponent<SoundObject>();
            if (parent != null)
                cue.AudioObject.transform.SetParent(parent, false);
            if (settings.overrideParentPosition) cue.AudioObject.transform.position = settings.position;
            cue.OnPlayKilled = (c, pr) => _activeSoundItems.Remove(c.Data.soundItem.id);

            SoundCueProxy proxy = new SoundCueProxy();
            proxy.SoundCue = cue;
            proxy.Play(data);
            _activeSoundItems[item.id] = Time.realtimeSinceStartup;
            return proxy;
        }

        #endregion

        #region Private methods

        private bool ItemAlreadyPlaying(SoundItem item) {
            bool alreadyPlaying = false;
            float startTime;
            if (item.minTimeBetweenPlay > 0 && _activeSoundItems.TryGetValue(item.id, out startTime)) {
                float timeSinceLastPlayStarted = Time.realtimeSinceStartup - startTime;
                if (timeSinceLastPlayStarted < item.minTimeBetweenPlay) {
                    alreadyPlaying = true;
                }
            }
            return alreadyPlaying;
        }

        /// <summary>
        /// This method is called only when PlaySoundSettings has a SoundCue reference in it.
        /// Same as Play(), but much faster.
        /// </summary>
        /// <param name="settings">PlaySoundSettings instance with SoundCue reference in it.</param>
        /// <returns>Same SoundCue from PlaySoundSettings</returns>
        private SoundCueProxy PlaySoundCue(PlaySoundSettings settings) {
            SoundCueProxy cue = settings.soundCueProxy as SoundCueProxy;
            Transform parent = settings.parent;
            float fadeInTime = settings.fadeInTime;
            float fadeOutTime = settings.fadeOutTime;
            var ncue = _cueManager.GetSoundCue();
            ncue.AudioObject = _pool.GetFreeObject(cue.Data.audioPrefab).GetComponent<SoundObject>();
            if (parent != null)
                ncue.AudioObject.transform.SetParent(parent, false);
            if (settings.overrideParentPosition) ncue.AudioObject.transform.position = settings.position;
            SoundCueData data = cue.Data;
            data.fadeInTime = fadeInTime;
            data.fadeOutTime = fadeOutTime;
            data.isFadeIn = data.fadeInTime >= 0.1f;
            data.isFadeOut = data.fadeOutTime >= 0.1f;
            cue.SoundCue = ncue;
            cue.Play(data);
            return cue;
        }

        #endregion

        #region MonoBehaviour methods

        void Awake() {
            print("SoundController awake");
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }

            _pool = GetComponent<ObjectPool>();
            _cueManager = new CueManager(_initialCueManagerSize);
        }

        #endregion
    }

    /// <summary>
    /// Set the settings to play a particular cue with particular preferences.
    /// </summary>
    [System.Serializable]
    public struct PlaySoundSettings {
        public SoundControllerData database;
        public string categoryId;
        public string soundItemId;
        /// <summary>
        /// Attach the Playing sound to a Specific GameObject
        /// </summary>
        public Transform parent;
        public bool overrideParentPosition;
        public Vector3 position;
        /// <summary>
        /// Fade In time of the whole SoundCue
        /// </summary>
        public float fadeInTime;
        /// <summary>
        /// Fade Out time of the whole SoundCue
        /// </summary>
        public float fadeOutTime;
        /// <summary>
        /// Use the same SoundCue to play again the sounds played in that SoundCue
        /// This is recommended to do, because searching by names all the Sounds to play is very expensive.
        /// </summary>
        public ISoundCue soundCueProxy;

        /// <summary>
        /// Initializes the PlaySoundSettings with predefined values. It is required to be called after the creation
        /// of the PlaySoundSettings instance.
        /// </summary>
        public void Init() {
            parent = null;
            overrideParentPosition = false;
            position = Vector3.zero;
            fadeInTime = 0f;
            fadeOutTime = 0f;
            soundCueProxy = null;
        }
    }

}
