using UnityEngine;
using System;
using System.Collections;
using FlexAnimation;

namespace FlexAnimation.Internal
{
    public static class FlexTween
    {
        public static IEnumerator To(Func<float> getter, Action<float> setter, float endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (duration <= 0)
            {
                setter(endValue);
                yield break;
            }

            float startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                dt *= globalTimeScale;
                time += dt;

                float t = Mathf.Clamp01(time / duration);
                float easedT = EvaluateEase(t, easeType);
                
                setter(Mathf.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (duration <= 0)
            {
                setter(endValue);
                yield break;
            }

            Vector3 startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                dt *= globalTimeScale;
                time += dt;

                float t = Mathf.Clamp01(time / duration);
                float easedT = EvaluateEase(t, easeType);

                setter(Vector3.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (duration <= 0)
            {
                setter(endValue);
                yield break;
            }

            Color startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                dt *= globalTimeScale;
                time += dt;

                float t = Mathf.Clamp01(time / duration);
                float easedT = EvaluateEase(t, easeType);

                setter(Color.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        public static IEnumerator To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (duration <= 0)
            {
                setter(endValue);
                yield break;
            }

            Vector2 startValue = getter();
            float time = 0f;

            while (time < duration)
            {
                float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                dt *= globalTimeScale;
                time += dt;

                float t = Mathf.Clamp01(time / duration);
                float easedT = EvaluateEase(t, easeType);

                setter(Vector2.LerpUnclamped(startValue, endValue, easedT));
                yield return null;
            }
            setter(endValue);
        }

        private static float EvaluateEase(float t, Ease ease)
        {
            const float PI = Mathf.PI;
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            const float c3 = c1 + 1;
            const float c4 = (2 * PI) / 3;
            const float c5 = (2 * PI) / 4.5f;

            switch (ease)
            {
                // Sine
                case Ease.InSine: return 1 - Mathf.Cos((t * PI) / 2);
                case Ease.OutSine: return Mathf.Sin((t * PI) / 2);
                case Ease.InOutSine: return -(Mathf.Cos(PI * t) - 1) / 2;

                // Quad
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return 1 - (1 - t) * (1 - t);
                case Ease.InOutQuad: return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

                // Cubic
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return 1 - Mathf.Pow(1 - t, 3);
                case Ease.InOutCubic: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;

                // Quart
                case Ease.InQuart: return t * t * t * t;
                case Ease.OutQuart: return 1 - Mathf.Pow(1 - t, 4);
                case Ease.InOutQuart: return t < 0.5f ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;

                // Quint
                case Ease.InQuint: return t * t * t * t * t;
                case Ease.OutQuint: return 1 - Mathf.Pow(1 - t, 5);
                case Ease.InOutQuint: return t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;

                // Expo
                case Ease.InExpo: return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
                case Ease.OutExpo: return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
                case Ease.InOutExpo:
                    return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? Mathf.Pow(2, 20 * t - 10) / 2 : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;

                // Circ
                case Ease.InCirc: return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
                case Ease.OutCirc: return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
                case Ease.InOutCirc:
                    return t < 0.5f ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;

                // Back
                case Ease.InBack: return c3 * t * t * t - c1 * t * t;
                case Ease.OutBack: return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
                case Ease.InOutBack:
                    return t < 0.5f
                        ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                        : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (2 * t - 2) + c2) + 2) / 2;

                // Elastic
                case Ease.InElastic:
                    return t == 0 ? 0 : t == 1 ? 1 : -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);
                case Ease.OutElastic:
                    return t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
                case Ease.InOutElastic:
                    return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                        ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2
                        : (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 + 1;

                // Bounce
                case Ease.InBounce: return 1 - EvaluateEase(1 - t, Ease.OutBounce);
                case Ease.OutBounce:
                    const float n1 = 7.5625f;
                    const float d1 = 2.75f;
                    if (t < 1 / d1) return n1 * t * t;
                    else if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
                    else if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
                    else return n1 * (t -= 2.625f / d1) * t + 0.984375f;
                case Ease.InOutBounce:
                    return t < 0.5f
                        ? (1 - EvaluateEase(1 - 2 * t, Ease.OutBounce)) / 2
                        : (1 + EvaluateEase(2 * t - 1, Ease.OutBounce)) / 2;

                default: return t;
            }
        }
    }
}