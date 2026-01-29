using UnityEngine;
using System;
using System.Collections;
using FlexAnimation;

namespace FlexAnimation.Internal
{
    public static class FlexTween
    {
        public static IEnumerator To(Func<float> getter, Action<float> setter, float endValue, float duration, AnimEase easeType)
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

        public static IEnumerator To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration, AnimEase easeType)
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

        public static IEnumerator To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, AnimEase easeType)
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

        public static IEnumerator To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, AnimEase easeType)
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

        private static float EvaluateEase(float t, AnimEase ease)
        {
            // Simple Ease Implementations
            switch (ease)
            {
                case AnimEase.InQuad: return t * t;
                case AnimEase.OutQuad: return t * (2 - t);
                case AnimEase.InOutQuad: return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case AnimEase.InCubic: return t * t * t;
                case AnimEase.OutCubic: return (--t) * t * t + 1;
                case AnimEase.InBack: float s = 1.70158f; return t * t * ((s + 1) * t - s);
                case AnimEase.OutBack: float s2 = 1.70158f; return --t * t * ((s2 + 1) * t + s2) + 1;
                default: return t; // Linear
            }
        }
    }
}
