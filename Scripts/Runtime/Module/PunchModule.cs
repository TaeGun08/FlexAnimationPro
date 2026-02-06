using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class PunchModule : AnimationModule
    {
        [Header("Values")]
        public Vector3 punch = new Vector3(0.5f, 0.5f, 0.5f);
        public int vibrato = 10;
        public float elasticity = 1f;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return target.DOPunchScale(punch, duration, vibrato, elasticity);
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 originalScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                elapsed += dt;
                float t = elapsed / duration;

                // Damped Harmonic Motion (Punch Physics)
                // s(t) = A * e^(-decay * t) * cos(freq * t)
                // We map t (0..1) to physical time roughly based on vibrato
                
                float decay = 10f * (1f / elasticity); 
                float freq = vibrato * Mathf.PI * 2f; 
                float damp = Mathf.Exp(-decay * t);
                float wave = Mathf.Sin(freq * t); // Sin starts at 0, punch usually starts at 0 deviation

                // Shape: Rise fast then decay oscillating
                // Using a curve that starts at 0, goes to 1, then oscillates to 0
                
                float strengthFactor = damp * wave;

                target.localScale = originalScale + Vector3.Scale(punch, Vector3.one * strengthFactor);

                yield return null;
            }

            target.localScale = originalScale;
        }
    }
}