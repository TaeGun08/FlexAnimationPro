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

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            if (duration > 0) yield return new WaitForSeconds(duration);
            onTrigger?.Invoke();
        }
    }
}