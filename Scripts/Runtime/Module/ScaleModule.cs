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
    public class ScaleModule : AnimationModule
    {
        [Header("Values")]
        public Vector3 endValue = Vector3.one;
        public bool relative = false;

        [Header("Randomness")]
        public Vector3 randomSpread;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            Vector3 offset = GetOffset();

            Tween t = target.DOScale(endValue + offset, duration);
            if (relative) t.SetRelative(true);
            return t;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 offset = GetOffset();
            Vector3 targetScale = endValue + offset;
            Vector3 startScale = target.localScale;

            if (relative) targetScale += startScale;

            yield return FlexTween.To(
                () => target.localScale, 
                val => target.localScale = val, 
                targetScale, duration, ease, ignoreTimeScale, globalTimeScale);
        }

        private Vector3 GetOffset()
        {
            return new Vector3(
                UnityEngine.Random.Range(-randomSpread.x, randomSpread.x),
                UnityEngine.Random.Range(-randomSpread.y, randomSpread.y),
                UnityEngine.Random.Range(-randomSpread.z, randomSpread.z)
            );
        }
    }
}