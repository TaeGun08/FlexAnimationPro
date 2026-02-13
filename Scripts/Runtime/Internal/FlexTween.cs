using UnityEngine;
using System;
using System.Collections;

namespace FlexAnimation.Internal
{
    public static class FlexTween
    {
        public static float? OverrideDeltaTime = null;

        public static IEnumerator To(Func<float> getter, Action<float> setter, float end, float duration, Ease ease, bool ignore = false, float ts = 1f, LoopMode loop = LoopMode.None, int count = -1)
        { yield return RunTween(getter, setter, end, duration, ease, ignore, ts, loop, count, Mathf.LerpUnclamped); }

        public static IEnumerator To(Func<Vector3> getter, Action<Vector3> setter, Vector3 end, float duration, Ease ease, bool ignore = false, float ts = 1f, LoopMode loop = LoopMode.None, int count = -1)
        { yield return RunTween(getter, setter, end, duration, ease, ignore, ts, loop, count, Vector3.LerpUnclamped); }

        public static IEnumerator To(Func<Color> getter, Action<Color> setter, Color end, float duration, Ease ease, bool ignore = false, float ts = 1f, LoopMode loop = LoopMode.None, int count = -1)
        { yield return RunTween(getter, setter, end, duration, ease, ignore, ts, loop, count, Color.LerpUnclamped); }

        public static IEnumerator To(Func<Vector2> getter, Action<Vector2> setter, Vector2 end, float duration, Ease ease, bool ignore = false, float ts = 1f, LoopMode loop = LoopMode.None, int count = -1)
        { yield return RunTween(getter, setter, end, duration, ease, ignore, ts, loop, count, Vector2.LerpUnclamped); }

        private static IEnumerator RunTween<T>(Func<T> getter, Action<T> setter, T end, float duration, Ease ease, bool ignore, float ts, LoopMode loop, int count, Func<T, T, float, T> lerp)
        {
            if (duration <= 0) { setter(end); yield break; }
            T start = getter(), origStart = start, origEnd = end;
            int currentLoop = 0; bool forward = true;
            int targetLoops = (loop == LoopMode.None) ? 1 : count;
            
            while (true)
            {
                float time = 0f;
                T from = forward ? origStart : origEnd;
                T to = forward ? origEnd : origStart;

                while (time < duration)
                {
                    float dt = OverrideDeltaTime ?? ((ignore ? Time.unscaledDeltaTime : Time.deltaTime) * ts);
                    time += dt;
                    setter(lerp(from, to, EvaluateEase(Mathf.Clamp01(time / duration), ease)));
                    yield return null;
                }
                
                setter(to);
                currentLoop++;
                if (targetLoops != -1 && currentLoop >= targetLoops) break;

                if (loop == LoopMode.Yoyo) forward = !forward;
                else if (loop == LoopMode.Loop) setter(origStart);
            }
        }

        public static float EvaluateEase(float t, Ease ease)
        {
            switch (ease)
            {
                case Ease.InSine: return 1 - Mathf.Cos(t * 1.570796f);
                case Ease.OutSine: return Mathf.Sin(t * 1.570796f);
                case Ease.InOutSine: return -0.5f * (Mathf.Cos(3.14159f * t) - 1);
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return t * (2 - t);
                case Ease.InOutQuad: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return (--t) * t * t + 1;
                case Ease.InBack: return t * t * (2.70158f * t - 1.70158f);
                case Ease.OutBack: return 1 + (--t) * t * (2.70158f * t + 1.70158f);
                case Ease.OutBounce:
                    if (t < 0.363636f) return 7.5625f * t * t;
                    else if (t < 0.727272f) return 7.5625f * (t -= 0.545454f) * t + 0.75f;
                    else if (t < 0.909090f) return 7.5625f * (t -= 0.818181f) * t + 0.9375f;
                    else return 7.5625f * (t -= 0.954545f) * t + 0.984375f;
                default: return t;
            }
        }
    }
}
