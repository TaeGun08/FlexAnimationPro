using UnityEngine;
using System;
using System.Collections;
using FlexAnimation;

namespace FlexAnimation.Internal
{
    public static class FlexTween
    {
        public static IEnumerator To(Func<float> getter, Action<float> setter, float endValue, float duration, Ease easeType)
        {
            float startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float easedT = EvaluateEase(t, easeType);
                
                setter(Mathf.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration, Ease easeType)
        {
            Vector3 startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float easedT = EvaluateEase(t, easeType);

                setter(Vector3.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, Ease easeType)
        {
            Color startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float easedT = EvaluateEase(t, easeType);

                setter(Color.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, Ease easeType)
        {
            Vector2 startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float easedT = EvaluateEase(t, easeType);

                setter(Vector2.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        private static float EvaluateEase(float t, Ease ease)
        {
            // Simple Ease Implementations
            switch (ease)
            {
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return t * (2 - t);
                case Ease.InOutQuad: return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return (--t) * t * t + 1;
                case Ease.InBack: float s = 1.70158f; return t * t * ((s + 1) * t - s);
                case Ease.OutBack: float s2 = 1.70158f; return --t * t * ((s2 + 1) * t + s2) + 1;
                default: return t; // Linear
            }
        }
    }
}
