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
    public class UIModule : AnimationModule
    {
        [Header("Values")]
        public bool anchorPos;
        public Vector2 anchorPosValue;
        
        public bool sizeDelta;
        public Vector2 sizeDeltaValue;
        
        public bool relative;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            if (!target.TryGetComponent(out RectTransform rect)) return null;

            Sequence seq = DOTween.Sequence();

            if (anchorPos)
            {
                Tween t = rect.DOAnchorPos(anchorPosValue, duration);
                if (relative) t.SetRelative(true);
                seq.Join(t);
            }

            if (sizeDelta)
            {
                Tween t = rect.DOSizeDelta(sizeDeltaValue, duration);
                if (relative) t.SetRelative(true);
                seq.Join(t);
            }

            return seq;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (!target.TryGetComponent(out RectTransform rect)) yield break;

            if (anchorPos)
            {
                Vector2 start = rect.anchoredPosition;
                Vector2 dest = anchorPosValue;
                if (relative) dest += start;
                
                yield return FlexTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, dest, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }

            if (sizeDelta)
            {
                Vector2 start = rect.sizeDelta;
                Vector2 dest = sizeDeltaValue;
                if (relative) dest += start;
                yield return FlexTween.To(() => rect.sizeDelta, x => rect.sizeDelta = x, dest, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
        }
    }
}