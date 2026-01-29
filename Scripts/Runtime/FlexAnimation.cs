using UnityEngine;
using System.Collections.Generic;

namespace FlexAnimation
{
    public class FlexAnimation : MonoBehaviour
    {
        [SerializeReference]
        public List<AnimationModule> modules = new List<AnimationModule>();
        
        public bool playOnEnable = true;
        public FlexAnimationPreset preset;
        public float timeScale = 1f;
        public bool ignoreTimeScale = false;

        // Events
        public UnityEngine.Events.UnityEvent OnPlay;
        public UnityEngine.Events.UnityEvent OnComplete;

        public void PlayAll() { }
        public void PauseAll() { }
        public void StopAndReset() { }
        public void EditorPreviewUpdate(float deltaTime) { }
    }
}
