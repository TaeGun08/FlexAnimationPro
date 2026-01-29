using UnityEngine;
using System;
using System.Collections;
using FlexAnimation;

namespace FlexAnimation.Internal
{
    public static class FlexTween
    {
        public static IEnumerator To(Func<float> getter, Action<float> setter, float endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f, LoopMode loopMode = LoopMode.None, int loopCount = -1)
        {
            yield return RunTween(getter, setter, endValue, duration, easeType, ignoreTimeScale, globalTimeScale, loopMode, loopCount, Mathf.LerpUnclamped);
        }

        public static IEnumerator To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f, LoopMode loopMode = LoopMode.None, int loopCount = -1)
        {
            yield return RunTween(getter, setter, endValue, duration, easeType, ignoreTimeScale, globalTimeScale, loopMode, loopCount, Vector3.LerpUnclamped);
        }

        public static IEnumerator To(Func<Color> getter, Action<Color> setter, Color endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f, LoopMode loopMode = LoopMode.None, int loopCount = -1)
        {
            yield return RunTween(getter, setter, endValue, duration, easeType, ignoreTimeScale, globalTimeScale, loopMode, loopCount, Color.LerpUnclamped);
        }

        public static IEnumerator To(Func<Vector2> getter, Action<Vector2> setter, Vector2 endValue, float duration, Ease easeType, bool ignoreTimeScale = false, float globalTimeScale = 1f, LoopMode loopMode = LoopMode.None, int loopCount = -1)
        {
            yield return RunTween(getter, setter, endValue, duration, easeType, ignoreTimeScale, globalTimeScale, loopMode, loopCount, Vector2.LerpUnclamped);
        }

        private static IEnumerator RunTween<T>(Func<T> getter, Action<T> setter, T endValue, float duration, Ease easeType, bool ignoreTimeScale, float globalTimeScale, LoopMode loopMode, int loopCount, Func<T, T, float, T> lerpFunc)
        {
             if (duration <= 0)
            {
                setter(endValue);
                yield break;
            }

            T startValue = getter();
            T originalStart = startValue;
            T originalEnd = endValue;

            int currentLoop = 0;
            bool isPlayingForward = true;

            // -1 means infinite.
            // Loop count logic: 
            // 0 or 1 means play once? Usually 0 means once, 1 means 1 loop (play twice)?
            // DOTween: SetLoops(3) means play 3 times.
            // Standard interpretation: loopCount is total cycles? Or repeats?
            // "loopCount = -1" -> Infinite.
            // Let's assume loopCount is TOTAL iterations. If 1 (default?), it runs once.
            // If user enters 2, it runs twice.
            // If user enters -1, infinite.
            
            // Fix: If loopMode is None, treat as count = 1.
            int targetLoops = (loopMode == LoopMode.None) ? 1 : loopCount;
            
            while (true)
            {
                float time = 0f;
                T from = isPlayingForward ? originalStart : originalEnd;
                T to = isPlayingForward ? originalEnd : originalStart;

                while (time < duration)
                {
                    float dt = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                    dt *= globalTimeScale;
                    time += dt;

                    float t = Mathf.Clamp01(time / duration);
                    float easedT = EvaluateEase(t, easeType);

                    setter(lerpFunc(from, to, easedT));
                    yield return null;
                }
                
                // Ensure finish exact value
                setter(to);

                currentLoop++;
                if (targetLoops != -1 && currentLoop >= targetLoops) break;

                if (loopMode == LoopMode.Yoyo)
                {
                    isPlayingForward = !isPlayingForward;
                }
                else if (loopMode == LoopMode.Loop)
                {
                    // Restart: Reset to original start for next frame (or just keep from/to as is for next iteration logic)
                    setter(originalStart);
                }
            }
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