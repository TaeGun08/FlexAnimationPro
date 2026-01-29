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
    public class PunchModule : AnimationModule
    {
        [Header("Values")]
        public Vector3 punch = new Vector3(0.5f, 0.5f, 0.5f);
        public int vibrato = 10;
        public float elasticity = 1f;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return target.DOPunchScale(punch, duration, vibrato, elasticity);
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 original = target.localScale;
            Vector3 targetScale = original + punch;
            
            yield return FlexTween.To(() => target.localScale, x => target.localScale = x, targetScale, duration * 0.5f, ease, ignoreTimeScale, globalTimeScale);
            yield return FlexTween.To(() => target.localScale, x => target.localScale = x, original, duration * 0.5f, ease, ignoreTimeScale, globalTimeScale);
        }
    }
}