using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
using System.Collections;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class FadeModule : AnimationModule
    {
        [Header("Alpha Settings")]
        public bool useCurrentAlpha = true;
        [Range(0f, 1f)] public float startAlpha = 0f;
        [Range(0f, 1f)] public float endAlpha = 1f;

        [Header("Sequence")]
        public bool pingPong = false;
        public float pauseTime = 0.5f;

        [Header("Stylized")]
        [Tooltip("0 for smooth. Higher values create a stepped/quantized alpha effect.")]
        [Range(0, 32)] public int alphaSteps = 0;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            float from = useCurrentAlpha ? GetCurrentAlpha(target) : startAlpha;
            SetAlpha(target, from);

            var seq = DOTween.Sequence();
            
            // Phase 1: To End
            // Use a virtual tween for alphaSteps to ensure rounding works perfectly with DOTween
            float virtualAlpha = from;
            var t1 = DOTween.To(() => virtualAlpha, x => {
                virtualAlpha = x;
                SetAlpha(target, x);
            }, endAlpha, duration).SetEase(GetEase());
            
            seq.Append(t1);

            if (pingPong)
            {
                if (pauseTime > 0) seq.AppendInterval(pauseTime);
                
                var t2 = DOTween.To(() => virtualAlpha, x => {
                    virtualAlpha = x;
                    SetAlpha(target, x);
                }, from, duration).SetEase(GetEase());
                seq.Append(t2);
            }

            return seq;
        }
#endif

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            float from = useCurrentAlpha ? GetCurrentAlpha(target) : startAlpha;
            
            // Phase 1: To End
            yield return RunFade(target, from, endAlpha, duration, ignoreTimeScale, globalTimeScale);

            // Phase 2: PingPong
            if (pingPong)
            {
                if (pauseTime > 0)
                {
                    // IMPORTANT: Custom RoutineRunner uses float for wait time, not WaitForSeconds
                    yield return pauseTime;
                }
                
                yield return RunFade(target, endAlpha, from, duration, ignoreTimeScale, globalTimeScale);
            }
        }

        private IEnumerator RunFade(Transform target, float from, float to, float dur, bool ignore, float ts)
        {
            yield return FlexTween.To(
                () => from,
                x => SetAlpha(target, x),
                to, dur, ease, ignore, ts, loop, loopCount);
        }

        private float GetCurrentAlpha(Transform target)
        {
            if (target.TryGetComponent(out CanvasGroup cg)) return cg.alpha;
            if (target.TryGetComponent(out Graphic gr)) return gr.color.a;
            if (target.TryGetComponent(out SpriteRenderer sr)) return sr.color.a;
            
            if (target.TryGetComponent(out Renderer rend))
            {
                // Try to get from PropertyBlock first (for Editor consistency)
                MaterialPropertyBlock pb = new MaterialPropertyBlock();
                rend.GetPropertyBlock(pb);
                
                Material mat = Application.isPlaying ? rend.material : rend.sharedMaterial;
                string colProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                
                Color c = pb.GetColor(colProp);
                if (c.a > 0 || pb.HasProperty(colProp)) return c.a; // If PB has value, use it
                
                return mat.HasProperty(colProp) ? mat.GetColor(colProp).a : 1f;
            }
            return 1f;
        }

        private void SetAlpha(Transform target, float alpha)
        {
            // Apply Stepping
            float finalAlpha = alphaSteps > 0 ? Mathf.Round(alpha * alphaSteps) / (float)alphaSteps : alpha;

            if (target.TryGetComponent(out CanvasGroup cg)) 
                cg.alpha = finalAlpha;
            else if (target.TryGetComponent(out Graphic gr))
            {
                Color c = gr.color;
                c.a = finalAlpha;
                gr.color = c;
            }
            else if (target.TryGetComponent(out SpriteRenderer sr))
            {
                Color c = sr.color;
                c.a = finalAlpha;
                sr.color = c;
            }
            else if (target.TryGetComponent(out Renderer rend))
            {
                bool useInstance = Application.isPlaying;
                Material mat = useInstance ? rend.material : rend.sharedMaterial;
                string colProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                
                if (useInstance)
                {
                    Color c = mat.GetColor(colProp);
                    c.a = finalAlpha;
                    mat.SetColor(colProp, c);
                }
                else
                {
                    MaterialPropertyBlock pb = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(pb);
                    Color c = mat.GetColor(colProp); // Get original to keep RGB
                    c.a = finalAlpha;
                    pb.SetColor(colProp, c);
                    rend.SetPropertyBlock(pb);
                }
            }
        }
    }
}