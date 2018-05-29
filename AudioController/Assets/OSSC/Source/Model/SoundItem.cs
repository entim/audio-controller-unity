using System;
using UnityEngine;

namespace OSSC.Model {
    /// <summary>
    /// Used by CategoryItem to store sounds data.
    /// </summary>
    [Serializable]
    public class SoundItem {
        public string id;
        /// <summary>
        /// SoundItem Name
        /// </summary>
        public string name;
        /// <summary>
        /// List of Audioclips
        /// </summary>
        public AudioClip[] clips;
        public PlayMode playMode = PlayMode.random;
        public bool overridePriority;
        public int priority = 128;
        public float minTimeBetweenPlay = 0.1f;
        /// <summary>
        /// Is SoundItem using Random Pitch?
        /// </summary>
        public bool isRandomPitch;
        /// <summary>
        /// Range of the Random pitch.
        /// </summary>
        public CustomRange pitchRange = new CustomRange();
        /// <summary>
        /// Is SoundItem using Random Volume?
        /// </summary>
        public bool isRandomVolume;
        /// <summary>
        /// Range of the Random Volume.
        /// </summary>
        public CustomRange volumeRange = new CustomRange();

        /// <summary>
        /// Standard volume of the SoundItem
        /// </summary>
        [Range(0f, 1f)]
        public float volume = 1f;

        public SoundItem() {
            id = Guid.NewGuid().ToString();
        }

        public SoundItem(SoundItem other) {
            id = Guid.NewGuid().ToString();
            name = other.name;
            clips = new AudioClip[other.clips.Length];
            Array.Copy(other.clips, clips, other.clips.Length);
            playMode = other.playMode;
            overridePriority = other.overridePriority;
            priority = other.priority;
            minTimeBetweenPlay = other.minTimeBetweenPlay;
            isRandomPitch = other.isRandomPitch;
            pitchRange = other.pitchRange;
            isRandomVolume = other.isRandomVolume;
            volumeRange = other.volumeRange;
        }

    }

    public enum PlayMode {
        sequence,
        random,
        loopOneClip,
        loopSequence,
        introLoopOutroSequence
    }

    /// <summary>
    /// Used by SoundItem to store Random Ranges.
    /// </summary>
    [Serializable]
    public class CustomRange {
        /// <summary>
        /// Minimum limit
        /// </summary>
        public float min = 1f;
        /// <summary>
        /// Maximum limit
        /// </summary>
        public float max = 1f;

        /// <summary>
        /// Gets a random value from it's Minimum and Maximum limits.
        /// </summary>
        /// <returns></returns>
        public float GetRandomRange() {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
