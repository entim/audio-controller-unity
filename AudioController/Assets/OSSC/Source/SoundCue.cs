using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using OSSC.Model;

namespace OSSC {
    /// <summary>
    /// Plays a whole cue of soundItems
    /// </summary>
    public class SoundCue : ISoundCue {
        /// <summary>
        /// Check ISoundCue
        /// </summary>
        public Action<string> OnPlayEnded { get; set; }

        /// <summary>
        /// Check ISoundCue
        /// </summary>
        public Action<SoundCue> OnPlayCueEnded { get; set; }

        /// <summary>
        /// Called whenever the sound cue has finished playing or was stopped
        /// </summary>
        public Action<SoundCue, SoundCueProxy> OnPlayKilled { get; set; }

        /// <summary>
        /// Check ISoundCue
        /// </summary>
        public SoundObject AudioObject { get; set; }

        /// <summary>
        /// Check ISoundCue
        /// </summary>
        public SoundCueData Data { get { return _data; } }

        /// <summary>
        /// Check ISoundCue
        /// </summary>
        public bool IsPlaying {
            get;
            private set;
        }

        /// <summary>
        /// SoundCue's unique ID given by the manager
        /// </summary>
        /// <returns></returns>
        public int ID {
            get;
            private set;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SoundCue() {
        }

        /// <summary>
        /// Custom Constructor
        /// </summary>
        /// <param name="id">Sets the ID of the SoundCue.</param>
        public SoundCue(int id) {
            ID = id;
        }

        /// <summary>
        /// Current index of the clip playing.
        /// </summary>
        private int _currentClip = 0;
        /// <summary>
        /// SoundCue data
        /// </summary>
        private SoundCueData _data;
        /// <summary>
        /// The proxy that the user uses to control the SoundCue.
        /// </summary>
        private SoundCueProxy _currentProxy;

        /// <summary>
        /// Will start playing the cue.
        /// NOTE: It is called from SoundCueProxy that is created by the SoundController.
        /// </summary>
        public void Play(SoundCueData data) {
            _data = data;

            AudioObject.isDespawnOnFinishedPlaying = data.soundItem.playMode == Model.PlayMode.random;

            AudioObject.OnFinishedPlaying = ChooseFinishedPlayingHandler(data.soundItem.playMode);

            if (TryPlayNextClip() == false) {
                return;
            }
            IsPlaying = true;
        }

        /// <summary>
        /// Plays the SoundCue.
        /// </summary>
        /// <param name="data">SoundCue's data</param>
        /// <param name="proxy">Proxy created by SoundController that called this method.</param>
        public void Play(SoundCueData data, SoundCueProxy proxy) {
            Play(data);
            _currentProxy = proxy;
        }

        /// <summary>
        /// Will pause the cue;
        /// </summary>
        public void Pause() {
            UnityEngine.Assertions.Assert.IsTrue(_currentClip > 0, "[AudioCue] Cannot pause when not even started.");
            AudioObject.Pause();
        }

        /// <summary>
        /// Resume the cue from where it was paused.
        /// </summary>
        public void Resume() {
            UnityEngine.Assertions.Assert.IsTrue(_currentClip > 0, "[AudioCue] Cannot resume when not even started.");
            AudioObject.Resume();
        }

        /// <summary>
        /// Stops the SoundCue.
        /// </summary>
        /// <param name="shouldCallOnFinishedCue">Checks whether to call OnEnd events, or not.</param>
        public void Stop(bool shouldCallOnFinishedCue = true) {
            if (IsPlaying == false) return;

            AudioObject.isDespawnOnFinishedPlaying = true;
            AudioObject.OnFinishedPlaying = null;
            AudioObject.Stop();
            AudioObject = null;
            _currentClip = 0;
            IsPlaying = false;

            if (shouldCallOnFinishedCue) {
                if (OnPlayCueEnded != null) {
                    OnPlayCueEnded(this);
                }
            }

            if (OnPlayKilled != null) {
                OnPlayKilled(this, _currentProxy);
                _currentProxy = null;
            }
        }

        public void StopSequence() {
            UnityEngine.Assertions.Assert.IsTrue(_data.soundItem.playMode == Model.PlayMode.introLoopOutroSequence
                || _data.soundItem.playMode == Model.PlayMode.sequence,
                "[AudioCue] Cannot stop cue when it's not in a sequence play mode.");

            if (IsPlaying == false)
                return;

            AudioObject.isDespawnOnFinishedPlaying = true;
            if (_data.soundItem.playMode == Model.PlayMode.introLoopOutroSequence && _currentClip != 3 && !_data.category.isMute) {
                _currentClip = 2;
                AudioObject.source.loop = false;
                PlayCurrentClip();
                _currentClip += 1;
            } else {
                Stop(true);
            }
        }

        public float Volume {
            get { return _data.soundItem.volume != 0 ? AudioObject.Volume / _data.soundItem.volume : 0; }
            set { AudioObject.Volume = value * _data.soundItem.volume; }
        }

        private Action<SoundObject> ChooseFinishedPlayingHandler(Model.PlayMode playMode) {
            Action<SoundObject> onFinishedPlaying_handler;
            if (playMode == Model.PlayMode.sequence) onFinishedPlaying_handler = OnFinishedPlayingSequence_handler;
            else if (playMode == Model.PlayMode.random) onFinishedPlaying_handler = OnFinishedPlayingRandom_handler;
            else if (playMode == Model.PlayMode.introLoopOutroSequence) onFinishedPlaying_handler = OnFinishedPlayingIntroLoopOutroSequence_handler;
            else if (playMode == Model.PlayMode.loopOneClip) onFinishedPlaying_handler = OnFinishedPlayingLoopOneClip_handler;
            else if (playMode == Model.PlayMode.loopSequence) onFinishedPlaying_handler = OnFinishedPlayingLoopSequence_handler;
            else onFinishedPlaying_handler = OnFinishedPlayingSequence_handler;
            return onFinishedPlaying_handler;
        }

        /// <summary>
        /// Internal event handler.
        /// </summary>
        /// <param name="obj"></param>
        private void OnFinishedPlayingSequence_handler(SoundObject obj) {
            string itemName = _data.soundItem.name;
            if (OnPlayEnded != null) OnPlayEnded(itemName);
            
            if (_currentClip < _data.soundItem.clips.Length) {
                if (_currentClip == _data.soundItem.clips.Length - 1) AudioObject.isDespawnOnFinishedPlaying = true;
                if (TryPlayNextClip() == false) Stop(true);
            } else {
                Stop(true);
            }
        }
        
        private void OnFinishedPlayingRandom_handler(SoundObject obj) {
            string itemName = _data.soundItem.name;
            if (OnPlayEnded != null) OnPlayEnded(itemName);
            Stop(true);
        }
            
        private void OnFinishedPlayingIntroLoopOutroSequence_handler(SoundObject obj) {
            string itemName = _data.soundItem.name;
            if (OnPlayEnded != null) OnPlayEnded(itemName);
            
            if (_currentClip == 1) {
                AudioObject.source.loop = true;
                if (TryPlayNextClip() == false) Stop(true);
            } else {
                Stop(true);
            }
        }
            
        private void OnFinishedPlayingLoopSequence_handler(SoundObject obj) {
            string itemName = _data.soundItem.name;
            if (OnPlayEnded != null) OnPlayEnded(itemName);
            
            if (_currentClip < _data.soundItem.clips.Length) {
                if (TryPlayNextClip() == false) Stop(true);
            } else if (_currentClip == _data.soundItem.clips.Length) {
                _currentClip = 0;
                if (TryPlayNextClip() == false) Stop(true);
            } else {
                Stop(true);
            }
        }
            
        private void OnFinishedPlayingLoopOneClip_handler(SoundObject obj) {
            string itemName = _data.soundItem.name;
            if (OnPlayEnded != null) OnPlayEnded(itemName);
            Stop(true);
        }

        /// <summary>
        /// Tries to play the next SoundItem in SoundCue.
        /// </summary>
        /// <returns>True - can play, False - Cannot</returns>
        private bool TryPlayNextClip() {
            bool isPlaying = false;
            if (!_data.category.isMute && AudioObject.isActiveAndEnabled) {
                PlayCurrentClip();
                _currentClip += 1;
                isPlaying = true;
            }
            return isPlaying;
        }

        /// <summary>
        /// Plays the Current SoundItem.
        /// </summary>
        private void PlayCurrentClip() {
            SoundItem item = _data.soundItem;
            CategoryItem category = _data.category;

            if (_currentClip == 0) {
                float clipVolume = item.isRandomVolume ? item.volumeRange.GetRandomRange() : item.volume;
                float pitch = item.isRandomPitch ? item.pitchRange.GetRandomRange() : 1;

                float volume = clipVolume * category.categoryVolume;
                int priority = item.overridePriority ? item.priority : category.categoryPriority;

                AudioObject.Setup(
                    item.name,
                    GetClip(item.playMode, item.clips),
                    item.playMode == Model.PlayMode.loopOneClip,
                    priority,
                    volume,
                    _data.fadeInTime,
                    _data.fadeOutTime,
                    category.mixer,
                    pitch);
            } else {
                AudioObject.SwitchClip(AudioObject.ID, GetClip(item.playMode, item.clips));
            }
            AudioObject.Play();
        }

        private AudioClip GetClip(Model.PlayMode playMode, AudioClip[] clips) {
            if (playMode == Model.PlayMode.random) {
                return GetRandomClip(clips);
            } else {
                return clips[_currentClip];
            }
        }

        /// <summary>
        /// Gets a random AudioClip from and array of AudioClips.
        /// </summary>
        /// <param name="clips">Array of SoundClips</param>
        /// <returns>An AudioClip</returns>
        private AudioClip GetRandomClip(AudioClip[] clips) {
            int index = UnityEngine.Random.Range(0, clips.Length);
            return clips[index];
        }
    }

    /// <summary>
    /// Used for sending data to play to AudioCue
    /// </summary>
    public struct SoundCueData {
        /// <summary>
        /// sound items that played by the SoundCue.
        /// </summary>
        public SoundItem soundItem;
        /// <summary>
        /// category items that correspond with each of SoundItem in sounds.
        /// </summary>
        public CategoryItem category;
        /// <summary>
        /// Prefab with SoundObject to play Sound items.
        /// </summary>
        public GameObject audioPrefab;
        /// <summary>
        /// Fade In time.
        /// </summary>
        public float fadeInTime;
        /// <summary>
        /// Fade Out time.
        /// </summary>
        public float fadeOutTime;
        /// <summary>
        /// Should SoundCue Fade In?
        /// </summary>
        public bool isFadeIn;
        /// <summary>
        /// Should SoundCue Fade Out?
        /// </summary>
        public bool isFadeOut;
    }
}