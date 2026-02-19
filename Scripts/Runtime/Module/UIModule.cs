using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
using System.Collections;
using System.Collections.Generic;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum UIAnimMode { Concurrent, Sequential }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class UIModule : AnimationModule
    {
        [Header("Execution")]
        public UIAnimMode animMode = UIAnimMode.Concurrent;

        [Header("Transform Settings")]
        public bool usePosition;
        public Vector3 position;
        
        public bool useRotation;
        public Vector3 rotation;
        
        public bool useScale;
        public Vector3 scale = Vector3.one;
        
        public bool useSize;
        public Vector2 sizeDelta;
        
        public bool relative = true;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            if (!target.TryGetComponent(out RectTransform rect)) return null;

            Sequence seq = DOTween.Sequence();
            float stepTime = duration;
            
            // In Sequential mode, we split total duration among active steps? 
            // Or each step takes full duration? Let's assume each step takes 'duration' for clarity.
            
            void AddToSeq(Tween t)
            {
                if (animMode == UIAnimMode.Concurrent) seq.Join(t);
                else seq.Append(t);
            }

            if (usePosition)
            {
                Tween t = DOTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, (Vector2)position, duration).SetEase(GetEase());
                if (relative) t.SetRelative(true);
                AddToSeq(t);
            }
            if (useRotation)
            {
                Tween t = DOTween.To(() => rect.localEulerAngles, x => rect.localEulerAngles = x, rotation, duration).SetEase(GetEase());
                if (relative) t.SetRelative(true);
                AddToSeq(t);
            }
            if (useScale)
            {
                Tween t = rect.transform.DOScale(scale, duration).SetEase(GetEase());
                if (relative) t.SetRelative(true);
                AddToSeq(t);
            }
            if (useSize)
            {
                Tween t = DOTween.To(() => rect.sizeDelta, x => rect.sizeDelta = x, sizeDelta, duration).SetEase(GetEase());
                if (relative) t.SetRelative(true);
                AddToSeq(t);
            }

            return seq;
        }
#endif

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            RectTransform rect = target as RectTransform;
            if (rect == null) yield break;

            if (animMode == UIAnimMode.Sequential)
            {
                if (usePosition) yield return RunPos(rect, ignoreTimeScale, globalTimeScale);
                if (useRotation) yield return RunRot(rect, ignoreTimeScale, globalTimeScale);
                if (useScale) yield return RunScale(rect, ignoreTimeScale, globalTimeScale);
                if (useSize) yield return RunSize(rect, ignoreTimeScale, globalTimeScale);
            }
            else
            {
                // Concurrent: Run all active transforms in one tween loop
                Vector2 startPos = rect.anchoredPosition;
                Vector3 startRot = rect.localEulerAngles;
                Vector3 startScale = rect.localScale;
                Vector2 startSize = rect.sizeDelta;

                Vector2 destPos = relative ? startPos + (Vector2)position : (Vector2)position;
                Vector3 destRot = relative ? startRot + rotation : rotation;
                Vector3 destScale = relative ? startScale + scale : scale;
                Vector2 destSize = relative ? startSize + sizeDelta : sizeDelta;

                yield return FlexTween.To(
                    () => 0f,
                    t => 
                    {
                        if (usePosition) rect.anchoredPosition = Vector2.LerpUnclamped(startPos, destPos, t);
                        if (useRotation) rect.localEulerAngles = Vector3.LerpUnclamped(startRot, destRot, t);
                        if (useScale) rect.localScale = Vector3.LerpUnclamped(startScale, destScale, t);
                        if (useSize) rect.sizeDelta = Vector2.LerpUnclamped(startSize, destSize, t);
                    },
                    1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount
                );
            }
        }

        private IEnumerator Wrap(IEnumerator e) { yield return e; }

        private IEnumerator RunPos(RectTransform rect, bool ignore, float ts)
        {
            Vector2 dest = position;
            if (relative) dest += rect.anchoredPosition;
            yield return FlexTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, dest, duration, ease, ignore, ts, loop, loopCount);
        }

        private IEnumerator RunRot(RectTransform rect, bool ignore, float ts)
        {
            Vector3 dest = rotation;
            if (relative) dest += rect.localEulerAngles;
            yield return FlexTween.To(() => rect.localEulerAngles, x => rect.localEulerAngles = x, dest, duration, ease, ignore, ts, loop, loopCount);
        }

        private IEnumerator RunScale(RectTransform rect, bool ignore, float ts)
        {
            Vector3 dest = scale;
            if (relative) dest += rect.localScale;
            yield return FlexTween.To(() => rect.localScale, x => rect.localScale = x, dest, duration, ease, ignore, ts, loop, loopCount);
        }

        private IEnumerator RunSize(RectTransform rect, bool ignore, float ts)
        {
            Vector2 dest = sizeDelta;
            if (relative) dest += rect.sizeDelta;
            yield return FlexTween.To(() => rect.sizeDelta, x => rect.sizeDelta = x, dest, duration, ease, ignore, ts, loop, loopCount);
        }
    }
}