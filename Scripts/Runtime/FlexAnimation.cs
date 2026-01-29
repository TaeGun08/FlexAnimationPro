using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

        private List<Coroutine> _activeCoroutines = new List<Coroutine>();

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayAll();
            }
        }

        private void OnDisable()
        {
            StopAndReset();
        }

        public void PlayAll()
        {
            StopAndReset();
            
            var targetModules = modules;
            if ((targetModules == null || targetModules.Count == 0) && preset != null)
            {
                targetModules = preset.modules;
            }

            if (targetModules == null || targetModules.Count == 0) return;

            OnPlay?.Invoke();

            float cursor = 0f;
            float maxEndTimeInBlock = 0f;
            float maxTotalDuration = 0f;

            foreach (var module in targetModules)
            {
                if (!module.enabled) continue;

                if (module.linkType == FlexLinkType.Append)
                {
                    cursor = maxEndTimeInBlock;
                }
                // Join keeps the current cursor

                float startTime = cursor + module.delay;
                float duration = module.duration;
                // If looping, duration is technically infinite or multiplied, 
                // but for scheduling next blocks we usually just count one cycle or the explicit duration.
                // Assuming standard sequencing behavior where duration is the single cycle.
                
                float endTime = startTime + duration;
                if (endTime > maxEndTimeInBlock)
                {
                    maxEndTimeInBlock = endTime;
                }

                if (endTime > maxTotalDuration)
                {
                    maxTotalDuration = endTime;
                }

                Coroutine c = StartCoroutine(RunModuleRoutine(module, startTime));
                _activeCoroutines.Add(c);
            }

            // Schedule OnComplete
            Coroutine completeRoutine = StartCoroutine(WaitAndComplete(maxTotalDuration));
            _activeCoroutines.Add(completeRoutine);
        }

        private IEnumerator RunModuleRoutine(AnimationModule module, float delay)
        {
            float effectiveDelay = delay;
            // If we want the delay to also respect the local timeScale:
            if (timeScale > 0.0001f) effectiveDelay /= timeScale;

            if (effectiveDelay > 0)
            {
                if (ignoreTimeScale)
                    yield return new WaitForSecondsRealtime(effectiveDelay);
                else
                    yield return new WaitForSeconds(effectiveDelay);
            }

            yield return module.CreateRoutine(transform, ignoreTimeScale, timeScale);
        }

        private IEnumerator WaitAndComplete(float duration)
        {
            float effectiveDuration = duration;
            if (timeScale > 0.0001f) effectiveDuration /= timeScale;

            if (effectiveDuration > 0)
            {
                if (ignoreTimeScale)
                    yield return new WaitForSecondsRealtime(effectiveDuration);
                else
                    yield return new WaitForSeconds(effectiveDuration);
            }
            OnComplete?.Invoke();
        }

        public void PauseAll()
        {
            // Simple pause not implemented with coroutines easily without custom wrapper.
            // For now, Stop is safer.
            StopAndReset(); 
        }

        public void StopAndReset()
        {
            foreach (var c in _activeCoroutines)
            {
                if (c != null) StopCoroutine(c);
            }
            _activeCoroutines.Clear();
            
            // Note: Resetting values to start state requires storing them, 
            // which is not currently implemented in the modules.
        }

        public void EditorPreviewUpdate(float deltaTime) { }
    }
}
