using UnityEngine;
using System.Collections;
using FlexAnimation.Internal;
using UnityEngine.Scripting.APIUpdating;

namespace FlexAnimation
{
    public enum ScaleAnimMode { Uniform, Free, Pop, Breathe }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class ScaleModule : AnimationModule
    {
        [Header("Scaling Mode")]
        public ScaleAnimMode mode = ScaleAnimMode.Uniform;

        [Header("Settings")]
        public float uniformScale = 1.2f;
        public Vector3 freeScale = Vector3.one;
        
        [Header("Effect Settings (Expert)")]
        [Range(0.1f, 10f)] public float speed = 2f;
        [Range(0f, 1f)] public float intensity = 0.15f;

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 startScale = target.localScale;
            
            if (mode == ScaleAnimMode.Pop)
            {
                // Instant feedback pop
                yield return FlexTween.To(() => startScale, val => target.localScale = val, startScale * uniformScale, duration * 0.3f, Ease.OutBack, ignoreTimeScale, globalTimeScale);
                yield return FlexTween.To(() => target.localScale, val => target.localScale = val, startScale, duration * 0.7f, Ease.OutBounce, ignoreTimeScale, globalTimeScale);
            }
            else if (mode == ScaleAnimMode.Breathe)
            {
                float elapsed = 0f;
                while (elapsed < duration || loop != LoopMode.None)
                {
                    float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                    
                    var owner = target.GetComponent<FlexAnimation>();
                    if (owner != null && owner.IsPaused) dt = 0;
                    
                    elapsed += dt;
                    float wave = Mathf.Sin(elapsed * speed * Mathf.PI) * intensity;
                    target.localScale = startScale * (1f + wave);
                    
                    if (duration > 0 && elapsed >= duration && loop == LoopMode.None) break;
                    yield return null;
                }
            }
            else
            {
                Vector3 dest = mode == ScaleAnimMode.Uniform ? Vector3.one * uniformScale : freeScale;
                yield return FlexTween.To(() => startScale, val => target.localScale = val, dest, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
        }
    }
}
