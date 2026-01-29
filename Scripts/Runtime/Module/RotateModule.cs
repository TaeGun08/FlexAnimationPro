using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    [MovedFrom(true, "", "Assembly-CSharp", null)]
    [System.Serializable]
    public class RotateModule : AnimationModule
    {
        [Header("Values")]
        public Vector3 endValue;
        public bool relative;
        public FlexSpace space = FlexSpace.Local;

        [Header("Randomness")]
        public Vector3 randomSpread;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            RotateMode mode = loop == LoopMode.None ? RotateMode.Fast : RotateMode.FastBeyond360; 

            Vector3 offset = GetOffset();

            Tween t;
            if (space == FlexSpace.Local)
            {
                t = target.DOLocalRotate(endValue + offset, duration, mode);
            }
            else
            {
                t = target.DORotate(endValue + offset, duration, mode);
            }

            if (relative) t.SetRelative(true);
            return t;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 offset = GetOffset();
            Vector3 targetRot = endValue + offset;

            if (space == FlexSpace.Local)
            {
                Vector3 startRot = target.localEulerAngles;
                if (relative) targetRot += startRot;
                
                yield return FlexTween.To(
                    () => target.localEulerAngles, 
                    val => target.localEulerAngles = val, 
                    targetRot, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
            else
            {
                Vector3 startRot = target.eulerAngles;
                if (relative) targetRot += startRot;

                yield return FlexTween.To(
                    () => target.eulerAngles, 
                    val => target.eulerAngles = val, 
                    targetRot, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
            }
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