using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class FadeModule : AnimationModule
    {
        [Header("Values")]
        public float endAlpha = 1f;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            if (target.TryGetComponent(out CanvasGroup cg)) 
                return cg.DOFade(endAlpha, duration);

            if (target.TryGetComponent(out Graphic gr))
                return gr.DOFade(endAlpha, duration);

            if (target.TryGetComponent(out SpriteRenderer sr)) 
                return sr.DOFade(endAlpha, duration);
                
            return null;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (target.TryGetComponent(out CanvasGroup cg))
            {
                yield return FlexTween.To(
                    () => cg.alpha, 
                    x => cg.alpha = x, 
                    endAlpha, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
            else if (target.TryGetComponent(out Graphic gr))
            {
                yield return FlexTween.To(
                    () => gr.color.a, 
                    x => { Color c = gr.color; c.a = x; gr.color = c; }, 
                    endAlpha, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
            else if (target.TryGetComponent(out SpriteRenderer sr))
            {
                yield return FlexTween.To(
                    () => sr.color.a, 
                    x => { Color c = sr.color; c.a = x; sr.color = c; }, 
                    endAlpha, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
            else if (target.TryGetComponent(out Renderer rend))
            {
                // Support for 3D Renderer (Material alpha)
                // Use .material only in Play Mode when persist is enabled
                bool useInstance = Application.isPlaying; 
                Material activeMat = useInstance ? rend.material : rend.sharedMaterial;
                
                string colProp = activeMat.HasProperty("_BaseColor") ? "_BaseColor" : "_BaseColor";
                if (!activeMat.HasProperty(colProp)) colProp = "_Color";

                MaterialPropertyBlock pb = new MaterialPropertyBlock();
                
                yield return FlexTween.To(
                    () => activeMat.HasProperty(colProp) ? activeMat.GetColor(colProp).a : 1f,
                    x => {
                        if (useInstance) {
                            Color c = activeMat.GetColor(colProp);
                            c.a = x;
                            activeMat.SetColor(colProp, c);
                        } else {
                            rend.GetPropertyBlock(pb);
                            Color c = activeMat.GetColor(colProp);
                            c.a = x;
                            pb.SetColor(colProp, c);
                            rend.SetPropertyBlock(pb);
                        }
                    },
                    endAlpha, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
        }
    }
}