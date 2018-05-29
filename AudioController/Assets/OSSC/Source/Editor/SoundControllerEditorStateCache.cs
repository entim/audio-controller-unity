using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSSC {

    public class SoundControllerEditorStateCache : ScriptableObject {

        [SerializeField]
        private List<SoundControllerEditorState> states = new List<SoundControllerEditorState>();

        public bool TryGetState(SoundController soundController, out SoundControllerEditorState state) {
            state = states.Find((s) => s.id == soundController.GetInstanceID());
            return state != null;
        }

        public SoundControllerEditorState RegisterSoundController(SoundController soundController) {
            SoundControllerEditorState state = new SoundControllerEditorState {
                id = soundController.GetInstanceID(),
                soundControllerData = soundController._database
            };
            states.Add(state);
            return state;
        }

        public void DeleteState(SoundControllerEditorState state) {
            states.Remove(state);
        }
    }

    [Serializable]
    public class SoundControllerEditorState {
        public int id;
        public int categoryIndex;
        public int soundItemIndex;
        public Model.SoundControllerData soundControllerData;
    }
}
