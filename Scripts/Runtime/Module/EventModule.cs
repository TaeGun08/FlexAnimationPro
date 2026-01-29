using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class EventModule : AnimationModule
    {
        public UnityEvent onTrigger;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return DOVirtual.DelayedCall(duration, () => onTrigger?.Invoke());
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            float effectiveDuration = duration;
            if (globalTimeScale > 0.0001f) effectiveDuration /= globalTimeScale;

            if (effectiveDuration > 0)
            {
                yield return effectiveDuration;
            }
            onTrigger?.Invoke();
        }
    }
}