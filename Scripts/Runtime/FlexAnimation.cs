using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum AnimEase
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InElastic, OutElastic, InOutElastic,
        InBack, OutBack, InOutBack,
        InBounce, OutBounce, InOutBounce
    }

    [MovedFrom(true, null, "Assembly-CSharp", "DOAnimation")]
    public class FlexAnimation : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private FlexAnimationPreset preset;

        [Header("Override")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private bool ignoreTimeScale = false;

        [Header("Events")]
        public UnityEvent OnPlay;
        public UnityEvent OnComplete;

        [SerializeReference]
        public List<AnimationModule> modules = new List<AnimationModule>();

#if DOTWEEN_ENABLED
        private Sequence currentSequence;
#endif

        private void OnEnable()
        {
            if (playOnEnable)
                PlayAll();
        }

        private void OnDisable()
        {
            StopAll();
        }

        public void PlayAll()
        {
#if DOTWEEN_ENABLED
            // Resume if paused
            if (currentSequence != null && currentSequence.IsActive())
            {
                if (!currentSequence.IsPlaying()) currentSequence.Play();
                return;
            }
#endif
            OnPlay?.Invoke();

            List<AnimationModule> targetModules = (preset != null) ? preset.modules : modules;
            if (targetModules == null || targetModules.Count == 0) return;

#if DOTWEEN_ENABLED
            currentSequence = DOTween.Sequence();
            currentSequence.SetUpdate(ignoreTimeScale);
            currentSequence.timeScale = timeScale;

            foreach (var module in targetModules)
            {
                if (!module.enabled) continue;

                Tween t = module.CreateTween(transform);
                if (t == null) continue;

                module.ApplyCommonSettings(t);

                switch (module.linkType)
                {
                    case FlexLinkType.Append:
                        if (module.delay > 0) currentSequence.AppendInterval(module.delay);
                        currentSequence.Append(t);
                        break;
                    case FlexLinkType.Join:
                        if (module.delay > 0) t.SetDelay(module.delay);
                        currentSequence.Join(t);
                        break;
                    case FlexLinkType.Insert:
                        currentSequence.Insert(module.delay, t);
                        break;
                }
            }

            currentSequence.OnComplete(() => OnComplete?.Invoke());
            currentSequence.Play();
#else
            // Native Mode
            StopAllCoroutines();
            StartCoroutine(NativeSequenceRoutine(targetModules));
#endif
        }

        private System.Collections.IEnumerator NativeSequenceRoutine(List<AnimationModule> targetModules)
        {
            foreach (var module in targetModules)
            {
                if (!module.enabled) continue;

                if (module.linkType == FlexLinkType.Join || module.linkType == FlexLinkType.Insert)
                {
                    StartCoroutine(RunModuleRoutine(module));
                }
                else
                {
                    if (module.delay > 0) yield return new WaitForSeconds(module.delay);
                    yield return StartCoroutine(RunModuleRoutine(module));
                }
            }
            OnComplete?.Invoke();
        }

        private System.Collections.IEnumerator RunModuleRoutine(AnimationModule module)
        {
            if (module.delay > 0 && module.linkType != FlexLinkType.Append) 
                yield return new WaitForSeconds(module.delay);

            yield return module.CreateRoutine(transform);
        }

        public void PauseAll()
        {
#if DOTWEEN_ENABLED
            if (currentSequence != null && currentSequence.IsActive()) currentSequence.Pause();
#else
            Debug.LogWarning("[FlexAnimation] Pause not supported in Native mode.");
#endif
        }

        public void StopAll()
        {
#if DOTWEEN_ENABLED
            if (currentSequence != null && currentSequence.IsActive()) currentSequence.Kill();
            currentSequence = null;
#else
            StopAllCoroutines();
#endif
        }

        public void StopAndReset()
        {
#if DOTWEEN_ENABLED
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Rewind();
                currentSequence.Kill();
            }
            currentSequence = null;
#else
            StopAllCoroutines();
#endif
        }

        public void EditorPreviewUpdate(float deltaTime)
        {
#if DOTWEEN_ENABLED
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.ManualUpdate(deltaTime, deltaTime);
            }
#endif
        }

        // ========================================================================
        // Runtime Builder API
        // ========================================================================

        public FlexAnimation Clear()
        {
            StopAll();
            preset = null;
            modules.Clear();
            return this;
        }

        public void SetPreset(FlexAnimationPreset newPreset)
        {
            preset = newPreset;
        }

        public FlexAnimation AddModule<T>(FlexLinkType linkType, float duration, System.Action<T> initializer = null) where T : AnimationModule, new()
        {
            T module = new T();
            module.enabled = true;
            module.linkType = linkType;
            module.duration = duration;
            initializer?.Invoke(module);
            modules.Add(module);
            return this;
        }

        public FlexAnimation AppendMove(Vector3 endValue, float duration, bool isLocal = true)
        {
            return AddModule<MoveModule>(FlexLinkType.Append, duration, m => 
            {
                m.endValue = endValue;
                m.x = m.y = m.z = true;
                m.space = isLocal ? FlexSpace.Local : FlexSpace.World;
            });
        }

        public FlexAnimation JoinMove(Vector3 endValue, float duration, bool isLocal = true)
        {
            return AddModule<MoveModule>(FlexLinkType.Join, duration, m => 
            {
                m.endValue = endValue;
                m.x = m.y = m.z = true;
                m.space = isLocal ? FlexSpace.Local : FlexSpace.World;
            });
        }

        public FlexAnimation AppendFade(float endAlpha, float duration)
        {
            return AddModule<FadeModule>(FlexLinkType.Append, duration, m => m.endAlpha = endAlpha);
        }
        
        public FlexAnimation JoinFade(float endAlpha, float duration)
        {
            return AddModule<FadeModule>(FlexLinkType.Join, duration, m => m.endAlpha = endAlpha);
        }

        public FlexAnimation AppendScale(Vector3 endScale, float duration)
        {
            return AddModule<ScaleModule>(FlexLinkType.Append, duration, m => m.endValue = endScale);
        }

        public FlexAnimation JoinScale(Vector3 endScale, float duration)
        {
            return AddModule<ScaleModule>(FlexLinkType.Join, duration, m => m.endValue = endScale);
        }
    }
}