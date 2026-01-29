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
    public class ColorModule : AnimationModule
    {
        [Header("Values")]
        public Color color = Color.white;
        public bool useAlphaOnly;
        public float alpha = 1f;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            Color targetColor = color;

            if (target.TryGetComponent(out Graphic graphic))
            {
                if (useAlphaOnly) return graphic.DOFade(alpha, duration);
                return graphic.DOColor(targetColor, duration);
            }

            if (target.TryGetComponent(out SpriteRenderer sr))
            {
                if (useAlphaOnly) return sr.DOFade(alpha, duration);
                return sr.DOColor(targetColor, duration);
            }

            if (target.TryGetComponent(out CanvasGroup cg))
            {
                 return cg.DOFade(useAlphaOnly ? alpha : targetColor.a, duration);
            }

            return null;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            if (target.TryGetComponent(out Graphic graphic))
            {
                if (useAlphaOnly)
                    yield return FlexTween.To(() => graphic.color.a, x => { var c = graphic.color; c.a = x; graphic.color = c; }, alpha, duration, ease);
                else
                    yield return FlexTween.To(() => graphic.color, x => graphic.color = x, color, duration, ease);
            }
            else if (target.TryGetComponent(out SpriteRenderer sr))
            {
                if (useAlphaOnly)
                    yield return FlexTween.To(() => sr.color.a, x => { var c = sr.color; c.a = x; sr.color = c; }, alpha, duration, ease);
                else
                    yield return FlexTween.To(() => sr.color, x => sr.color = x, color, duration, ease);
            }
            else if (target.TryGetComponent(out CanvasGroup cg))
            {
                float targetAlpha = useAlphaOnly ? alpha : color.a;
                yield return FlexTween.To(() => cg.alpha, x => cg.alpha = x, targetAlpha, duration, ease);
            }
        }
    }
}