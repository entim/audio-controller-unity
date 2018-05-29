using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSSC.Model {
    /// <summary>
    /// Used by the SoundControllerData to store categories.
    /// </summary>
    [Serializable]
    public class CategoryItem {
        public string id;
        /// <summary>
        /// Category name
        /// </summary>
        public string name;
        /// <summary>
        /// Array of SoundItems
        /// </summary>
        public SoundItem[] soundItems;
        /// <summary>
        /// Alternative SoundObject prefab to use, instead of the Default one from SoundController.
        /// </summary>
        public GameObject audioObjectPrefab;
        /// <summary>
        /// Check whether to use alternative SoundObject prefab.
        /// </summary>
        public bool usingDefaultPrefab = true;
        /// <summary>
        /// Mixer group associated with this SoundItem.
        /// </summary>
        public UnityEngine.Audio.AudioMixerGroup mixer;

        /// <summary>
        /// Volume of the category
        /// </summary>
        [Range(0f, 1f)]
        public float categoryVolume = 1f;

        [Range(1, 256)]
        public int categoryPriority = 128;

        /// <summary>
        /// Save the last search name written in editor.
        /// </summary>
        public string soundsSearchName = "";
        /// <summary>
        /// Is Category mute?
        /// </summary>
        public bool isMute = false;

        public CategoryItem() {
            id = Guid.NewGuid().ToString();
        }
    }

}
