using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSSC.Model {
    /// <summary>
    /// SoundController's Database.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundControllerData", menuName = "Subject 99/Audio/Sound Controller Data")]
    public class SoundControllerData : ScriptableObject {
        /// <summary>
        /// Stores all created Categories.
        /// </summary>
        public CategoryItem[] items;
        /// <summary>
        /// Database name.
        /// </summary>
        public string assetName;
    }
}