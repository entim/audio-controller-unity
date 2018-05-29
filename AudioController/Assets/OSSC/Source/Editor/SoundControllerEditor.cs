using System;
using System.IO;
using System.Collections.Generic;
using OSSC.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace OSSC.Editor {
    /// <summary>
    /// Draws the Custom Editor for SoundController
    /// </summary>
    [CustomEditor(typeof(SoundController))]
    public class SoundControllerEditor : UnityEditor.Editor {
        /// <summary>
        /// Max search string length
        /// </summary>
        private const int NAME_ABV_LEN = 50;
        /// <summary>
        /// Max pitch limit range
        /// </summary>
        private const float PITCH_RANGE_MAX = 3f;
        /// <summary>
        /// Min pitch limit range
        /// </summary>
        private const float PITCH_RANGE_MIN = -3f;
        /// <summary>
        /// Reference to SoundController
        /// </summary>
        private SoundController _ac;

        private int categoryIndex = 0;
        private int soundItemIndex = 0;

        private Color labelColor = new Color(1, 0.686f, 0.011f);
        private GUIStyle headerStyle;
        private Texture separatorTexture;
        private Texture playButtonTexture;
        private Texture stopButtonTexture;
        private Comparison<CategoryItem> categoryComparison;
        private Comparison<SoundItem> soundItemComparison;

        private SoundControllerEditorStateCache stateCache;
        private SoundControllerEditorState state;
	    private AudioSource previewAudioSource;

        private bool enabledInPlayerMode;

        public void OnEnable() {
            Initialize();
            LoadState();
        }

	    public void OnDisable() {
		    DestroyImmediate(previewAudioSource.gameObject);
	    }

        private void Initialize() {
            enabledInPlayerMode = Application.isPlaying;

            _ac = target as SoundController;

            separatorTexture = EditorGUIUtility.Load("IN foldout act on@2x") as Texture;
            playButtonTexture = EditorGUIUtility.Load("OSSC/play_button.png") as Texture;
            stopButtonTexture = EditorGUIUtility.Load("OSSC/stop_button.png") as Texture;

            categoryComparison = (a, b) => {
                if (a.name == null) {
                    if (b.name == null) return 0;
                    else return -1;
                } else if (b.name == null) return 1;
                return a.name.CompareTo(b.name);
            };

            soundItemComparison = (a, b) => {
                if (a.name == null) {
                    if (b.name == null) return 0;
                    else return -1;
                } else if (b.name == null) return 1;
                return a.name.CompareTo(b.name);
            };

            previewAudioSource = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        private void LoadState() {
            if (!Directory.Exists("Assets/Editor")) AssetDatabase.CreateFolder("Assets", "Editor");
            stateCache = AssetDatabase.LoadAssetAtPath<SoundControllerEditorStateCache>("Assets/Editor/SoundControllerEditorStateCache.asset");
            if (stateCache == null) {
                stateCache = CreateInstance<SoundControllerEditorStateCache>();
                AssetDatabase.CreateAsset(stateCache, "Assets/Editor/SoundControllerEditorStateCache.asset");
                AssetDatabase.SaveAssets();
            }

            if (!stateCache.TryGetState(_ac, out state)) {
                state = stateCache.RegisterSoundController(_ac);
            }

            categoryIndex = state.categoryIndex;
            soundItemIndex = state.soundItemIndex;
            EditorUtility.SetDirty(stateCache);
        }

        public void OnDestroy() {
            if (!Application.isPlaying && !enabledInPlayerMode && target == null) {
                stateCache.DeleteState(state);
            }
        }

        public override void OnInspectorGUI() {
            hideFlags = HideFlags.HideAndDontSave;
            base.OnInspectorGUI();

            headerStyle = new GUIStyle(EditorStyles.helpBox);
            headerStyle.padding = new RectOffset(3, 3, 3, 3);

            if (_ac._database == null) {
                EditorGUILayout.HelpBox("Create SoundControllerData asset, then throw it here.", MessageType.Info);
            } else {
                DrawMain();
            }

            EditorUtility.SetDirty(_ac);
            if (_ac._database != null) {
                if (_ac._database != state.soundControllerData) {
                    categoryIndex = 0;
                    soundItemIndex = 0;
                    state.soundControllerData = _ac._database;
                }
                EditorUtility.SetDirty(_ac._database);
            }
        }

        private void DrawMain() {
            if (_ac._database == null)
                return;

            var db = _ac._database;

            if (db.items != null && db.items.Length < 1) {
                EditorGUILayout.HelpBox("Add a category", MessageType.Info);
            }

            // Start to listen to changes to register them in the editor undo queue
            EditorGUI.BeginChangeCheck();

            DrawCategorySection(_ac._database);

            if (db.items.Length > 0) {
                GUILayout.Label(separatorTexture, new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            }

            if (db.items.Length > 0 && (db.items[categoryIndex].soundItems == null || db.items[categoryIndex].soundItems.Length < 1)) {
                EditorGUILayout.HelpBox("Add a sound item to the category", MessageType.Info);
            }

            DrawSoundItemSection(db);

            state.categoryIndex = categoryIndex;
            state.soundItemIndex = soundItemIndex;

            if (EditorGUI.EndChangeCheck()) {
                Undo.RegisterFullObjectHierarchyUndo(_ac._database, "Sound Controller Change");
                Undo.RegisterFullObjectHierarchyUndo(stateCache, "Sound Controller Change");
            }
        }

        private void DrawCategorySection(SoundControllerData db) {
            string[] categoryNames = new string[db.items != null ? db.items.Length : 0];
            if (db.items != null && db.items.Length > 0) {
                CategoryItem selectedCategory = db.items[categoryIndex];
                Array.Sort(db.items, categoryComparison);
                if (selectedCategory != null) categoryIndex = Array.IndexOf(db.items, selectedCategory);

                for (int i = 0; i < db.items.Length; i++) {
                    categoryNames[i] = db.items[i].name;
                }
            }

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = labelColor;

            EditorGUILayout.BeginHorizontal(headerStyle);

            GUI.backgroundColor = prevColor;
            int prevCategoryIndex = categoryIndex;
            bool noCategories = db.items.Length < 1;

            EditorGUI.BeginDisabledGroup(noCategories);
            categoryIndex = EditorGUILayout.Popup("Category", categoryIndex, categoryNames);
            EditorGUI.EndDisabledGroup();

            GUI.SetNextControlName("category add");
            if (GUILayout.Button(new GUIContent("+", "Add new category"), EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                var category = new CategoryItem();
                category.name = "??";
                var categories = new CategoryItem[db.items != null ? db.items.Length + 1 : 1];
                if (db.items != null)
                    db.items.CopyTo(categories, 0);
                categories[categories.Length - 1] = category;
                db.items = categories;
                categoryIndex = categories.Length - 1;
                GUI.FocusControl("category add");
            }

            EditorGUI.BeginDisabledGroup(noCategories);
            GUI.SetNextControlName("category del");
            if (GUILayout.Button(new GUIContent("-", "Delete category"), EditorStyles.miniButtonRight, GUILayout.Width(20))) {
                DeleteCategory(categoryIndex);
                GUI.FocusControl("category del");
            }

            GUI.SetNextControlName("category prev");
            if (GUILayout.Button(new GUIContent("<", "Previous category"), EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                categoryIndex = categoryIndex == 0 ? db.items.Length - 1 : categoryIndex - 1;
                GUI.FocusControl("category prev");
            }

            GUI.SetNextControlName("category next");
            if (GUILayout.Button(new GUIContent(">", "Next category"), EditorStyles.miniButtonRight, GUILayout.Width(20))) {
                categoryIndex = categoryIndex == db.items.Length - 1 ? 0 : categoryIndex + 1;
                GUI.FocusControl("category next");
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (prevCategoryIndex != categoryIndex) {
                soundItemIndex = 0;
            }

            if (db.items != null && db.items.Length > 0) {
                DrawCategory(db.items[categoryIndex]);
            }
        }

        private void DeleteCategory(int index) {
            var categories = new CategoryItem[_ac._database.items.Length - 1];
            int catInd = 0;
            for (int i = 0; i < _ac._database.items.Length; i++) {
                if (i == index)
                    continue;

                categories[catInd] = _ac._database.items[i];
                catInd += 1;
            }
            _ac._database.items = categories;
            categoryIndex = categories.Length - 1;
        }

        private void DrawCategory(CategoryItem item) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            item.name = EditorGUILayout.TextField("Name", item.name);

            item.audioObjectPrefab = (GameObject)EditorGUILayout.ObjectField("Audio Object Prefab Override", item.audioObjectPrefab, typeof(GameObject), false);
            item.usingDefaultPrefab = item.audioObjectPrefab == null;
            item.mixer = (AudioMixerGroup)EditorGUILayout.ObjectField("Mixer", item.mixer, typeof(AudioMixerGroup), false);
            item.isMute = EditorGUILayout.Toggle("Disable", item.isMute);
            item.categoryVolume = EditorGUILayout.Slider("Category Volume", item.categoryVolume, 0f, 1f);
            string priorityTooltip = "Sets the priority of the audio source. Unity limits the number of audio sources playing simultaneously, and decides what to play based on the priority.";
            item.categoryPriority = EditorGUILayout.IntSlider(new GUIContent("Category Priority", priorityTooltip), item.categoryPriority, 1, 256);

            EditorGUILayout.EndVertical();
        }

        private void DrawSoundItemSection(SoundControllerData db) {
            if (db.items != null && db.items.Length > 0) {
                CategoryItem item = db.items[categoryIndex];

                string[] soundItemNames;
                if (item.soundItems != null && item.soundItems.Length > 0) {
                    SoundItem selectedSoundItem = item.soundItems[soundItemIndex];
                    Array.Sort(item.soundItems, soundItemComparison);
                    if (selectedSoundItem != null) soundItemIndex = Array.IndexOf(item.soundItems, selectedSoundItem);

                    soundItemNames = new string[item.soundItems.Length];
                    for (int i = 0; i < item.soundItems.Length; i++) {
                        soundItemNames[i] = item.soundItems[i].name;
                    }
                } else {
                    soundItemNames = new string[0];
                }

                Color prevColor = GUI.backgroundColor;
                GUI.backgroundColor = labelColor;
                EditorGUILayout.BeginHorizontal(headerStyle);
                GUI.backgroundColor = prevColor;

                bool noSoundItems = soundItemNames.Length < 1;

                EditorGUI.BeginDisabledGroup(noSoundItems);
                soundItemIndex = EditorGUILayout.Popup("Sound Item", soundItemIndex, soundItemNames);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(noSoundItems);
                GUI.SetNextControlName("sound item copy");
                if (GUILayout.Button(new GUIContent("D", "Duplicate sound item"), EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                    SoundItem soundItem = new SoundItem(item.soundItems[soundItemIndex]);
                    soundItem.name = soundItem.name + " - Copy";
                    AddSoundItem(soundItem, item);
                    GUI.FocusControl("sound item copy");
                }
                EditorGUI.EndDisabledGroup();

                GUI.SetNextControlName("sound item add");
                if (GUILayout.Button(new GUIContent("+", "Add new sound item"), EditorStyles.miniButtonMid, GUILayout.Width(20))) {
                    SoundItem soundItem = new SoundItem();
                    soundItem.name = "??";
                    AddSoundItem(soundItem, item);
                    GUI.FocusControl("sound item add");
                }

                EditorGUI.BeginDisabledGroup(noSoundItems);
                GUI.SetNextControlName("sound item del");
                if (GUILayout.Button(new GUIContent("-", "Delete sound item"), EditorStyles.miniButtonRight, GUILayout.Width(20))) {
                    DeleteSoundItem(soundItemIndex, item);
                    GUI.FocusControl("sound item del");
                }

                GUI.SetNextControlName("sound item prev");
                if (GUILayout.Button(new GUIContent("<", "Previous sound item"), EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
                    soundItemIndex = soundItemIndex == 0 ? item.soundItems.Length - 1 : soundItemIndex - 1;
                    GUI.FocusControl("sound item prev");
                }

                GUI.SetNextControlName("sound item next");
                if (GUILayout.Button(new GUIContent(">", "Next sound item"), EditorStyles.miniButtonRight, GUILayout.Width(20))) {
                    soundItemIndex = soundItemIndex == item.soundItems.Length - 1 ? 0 : soundItemIndex + 1;
                    GUI.FocusControl("sound item next");
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                if (item.soundItems != null && item.soundItems.Length > 0) {
                    DrawSoundItem(item.soundItems[soundItemIndex]);
                }
            }
        }

        private void AddSoundItem(SoundItem soundItem, CategoryItem item) {
            bool isNoSoundItems = item.soundItems == null;
            var soundItems = new SoundItem[!isNoSoundItems ? item.soundItems.Length + 1 : 1];
            if (!isNoSoundItems)
                item.soundItems.CopyTo(soundItems, 0);
            soundItems[soundItems.Length - 1] = soundItem;
            item.soundItems = soundItems;
            soundItemIndex = soundItems.Length - 1;
        }

        private void DeleteSoundItem(int index, CategoryItem category) {
            var soundItems = new SoundItem[category.soundItems.Length - 1];
            int soundInd = 0;
            for (int i = 0; i < category.soundItems.Length; i++) {
                if (i == index)
                    continue;
                soundItems[soundInd] = category.soundItems[i];
                soundInd += 1;
            }
            category.soundItems = soundItems;
            soundItemIndex = soundItems.Length - 1;
        }

        private void DrawSoundItem(SoundItem item) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            item.name = EditorGUILayout.TextField("Name", item.name);


            var db = _ac._database;
            string[] categoryNames = new string[db.items != null ? db.items.Length : 0];
            if (db.items != null) {
                for (int i = 0; i < db.items.Length; i++) {
                    categoryNames[i] = db.items[i].name;
                }
            }
            string categoryTooltip = "Move this sound item to the selected category.";
            int soundItemCategoryIndex = EditorGUILayout.Popup(new GUIContent("Category", categoryTooltip), categoryIndex, categoryNames);
            if (soundItemCategoryIndex != categoryIndex) {
                DeleteSoundItem(soundItemIndex, db.items[categoryIndex]);
                AddSoundItem(item, db.items[soundItemCategoryIndex]);
                categoryIndex = soundItemCategoryIndex;
                soundItemIndex = db.items[soundItemCategoryIndex].soundItems.Length - 1;
            }
            
            string playModeTooltip = "Controls how clips will be selected to play.";
            item.playMode = (Model.PlayMode)EditorGUILayout.EnumPopup(new GUIContent("Play Mode", playModeTooltip), item.playMode);

            EditorGUILayout.BeginVertical(headerStyle);

            EditorGUILayout.Space();

            if (item.clips == null) {
                item.clips = new AudioClip[1];
            }

            if (item.playMode == Model.PlayMode.introLoopOutroSequence) {
                if (item.clips.Length != 3) ChangeClipListSize(item, 3);
                DrawClipSlot(item, 0, "Intro clip");
                DrawClipSlot(item, 1, "Loop clip");
                DrawClipSlot(item, 2, "Outro clip");
            } else if (item.playMode == Model.PlayMode.loopOneClip) {
                if (item.clips.Length != 1) ChangeClipListSize(item, 1);
                DrawClipSlot(item, 0, "Clip");
            } else {
                for (int i = 0; i < item.clips.Length; i++) {
                    DrawClipSlot(item, i, "Clip " + (i + 1));
                }
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(item.playMode == Model.PlayMode.introLoopOutroSequence || item.playMode == Model.PlayMode.loopOneClip);
            if (GUILayout.Button(new GUIContent("+", "Add clip slot"), EditorStyles.miniButton, GUILayout.MaxWidth(20))) {
                int newClipListSize = item.clips.Length + 1;
                ChangeClipListSize(item, newClipListSize);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginHorizontal();
            string minTimeTooltip = "If the same sound item is already started to play within this time frame, then skip this item.";
            item.minTimeBetweenPlay = EditorGUILayout.FloatField(new GUIContent("Min Time Between Play", minTimeTooltip), item.minTimeBetweenPlay);
            EditorGUILayout.LabelField("sec", GUILayout.MaxWidth(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            item.overridePriority = EditorGUILayout.ToggleLeft("Override Priority", item.overridePriority);
            if (item.overridePriority) {
                item.priority = EditorGUILayout.IntSlider("Priority", item.priority, 1, 256);
            }

            item.isRandomVolume = EditorGUILayout.ToggleLeft("Use Random Volume", item.isRandomVolume);
            if (!item.isRandomVolume)
                item.volume = EditorGUILayout.Slider("Volume", item.volume, 0f, 1f);
            else {
                EditorGUILayout.LabelField("Min Volume:", item.volumeRange.min.ToString(), EditorStyles.largeLabel);
                EditorGUILayout.LabelField("Max Volume:", item.volumeRange.max.ToString(), EditorStyles.largeLabel);
                EditorGUILayout.MinMaxSlider("Volume Range", ref item.volumeRange.min, ref item.volumeRange.max, 0f, 1f);
            }

            item.isRandomPitch = EditorGUILayout.ToggleLeft("Use Random Pitch", item.isRandomPitch);
            if (item.isRandomPitch) {
                EditorGUILayout.LabelField("Min Pitch:", item.pitchRange.min.ToString(), EditorStyles.largeLabel);
                EditorGUILayout.LabelField("Max Pitch:", item.pitchRange.max.ToString(), EditorStyles.largeLabel);
                EditorGUILayout.MinMaxSlider("Pitch Range", ref item.pitchRange.min, ref item.pitchRange.max, PITCH_RANGE_MIN, PITCH_RANGE_MAX);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawClipSlot(SoundItem item, int index, string label) {
            if (index != item.clips.Length) {
                EditorGUILayout.BeginHorizontal();

                item.clips[index] = (AudioClip)EditorGUILayout.ObjectField(label, item.clips[index], typeof(AudioClip), false);

                EditorGUI.BeginDisabledGroup(item.clips[index] == null);
                if (GUILayout.Button(new GUIContent(playButtonTexture, "Preview clip"), EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(15))) {
                    previewAudioSource.clip = item.clips[index];
                    previewAudioSource.Play();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!(previewAudioSource.isPlaying && previewAudioSource.clip == item.clips[index]));
                if (GUILayout.Button(new GUIContent(stopButtonTexture, "Stop preview"), EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(15))) {
                    previewAudioSource.Stop();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(item.clips.Length == 1);
                if (GUILayout.Button(new GUIContent("-", "Remove clip"), EditorStyles.miniButton, GUILayout.Width(20))) {
                    RemoveClip(item, index);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
        }

        private void ChangeClipListSize(SoundItem item, int newSize) {
            var newClips = new AudioClip[newSize];
            for (int i = 0; i < item.clips.Length; i++) {
                if (i >= newSize)
                    break;
                newClips[i] = item.clips[i];
            }
            item.clips = newClips;
        }

        private void RemoveClip(SoundItem item, int index) {
            int newSize = item.clips.Length - 1;
            var newClips = new AudioClip[newSize];
            int targetIndex = 0;
            for (int i = 0; i < item.clips.Length; i++) {
                if (i != index) {
                    newClips[targetIndex] = item.clips[i];
                    targetIndex++;
                }
            }
            item.clips = newClips;
        }
    }
}