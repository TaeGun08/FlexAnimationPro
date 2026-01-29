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
    public class MoveModule : AnimationModule
    {
        [Header("Values")]
        public bool x, y, z;
        public Vector3 endValue;
        public bool relative = false;
        
        [Header("Space")]
        public FlexSpace space = FlexSpace.Local;
        public bool useAnchoredPosition; 

        [Header("Randomness")]
        public Vector3 randomSpread;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            Vector3 offset = GetOffset();
            
            if (useAnchoredPosition && target.TryGetComponent(out RectTransform rect))
            {
                Vector2 targetAnchor = new Vector2(
                    x ? endValue.x : rect.anchoredPosition.x,
                    y ? endValue.y : rect.anchoredPosition.y
                ) + (Vector2)offset;
                
                Tween t = rect.DOAnchorPos(targetAnchor, duration);
                if (relative) t.SetRelative(true);
                return t;
            }

            if (space == FlexSpace.Local)
            {
                Vector3 startLocal = target.localPosition;
                Vector3 targetLocal = new Vector3(
                    x ? endValue.x : startLocal.x,
                    y ? endValue.y : startLocal.y,
                    z ? endValue.z : startLocal.z
                ) + offset;
                
                Tween t = target.DOLocalMove(targetLocal, duration);
                if (relative) t.SetRelative(true);
                return t;
            }
            
            Vector3 startWorld = target.position;
            Vector3 targetWorld = new Vector3(
                x ? endValue.x : startWorld.x,
                y ? endValue.y : startWorld.y,
                z ? endValue.z : startWorld.z
            ) + offset;

            Tween tWorld = target.DOMove(targetWorld, duration);
            if (relative) tWorld.SetRelative(true);
            return tWorld;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            Vector3 offset = GetOffset();

            if (useAnchoredPosition && target.TryGetComponent(out RectTransform rect))
            {
                Vector2 startPos = rect.anchoredPosition;
                Vector2 targetPos = new Vector2(
                    x ? endValue.x : startPos.x,
                    y ? endValue.y : startPos.y
                ) + (Vector2)offset;

                if (relative) targetPos += startPos;

                yield return FlexTween.To(
                    () => rect.anchoredPosition, 
                    val => rect.anchoredPosition = val, 
                    targetPos, duration, ease);
            }
            else if (space == FlexSpace.Local)
            {
                Vector3 startPos = target.localPosition;
                Vector3 targetPos = new Vector3(
                    x ? endValue.x : startPos.x,
                    y ? endValue.y : startPos.y,
                    z ? endValue.z : startPos.z
                ) + offset;

                if (relative) targetPos += startPos;

                yield return FlexTween.To(
                    () => target.localPosition, 
                    val => target.localPosition = val, 
                    targetPos, duration, ease);
            }
            else
            {
                Vector3 startPos = target.position;
                Vector3 targetPos = new Vector3(
                    x ? endValue.x : startPos.x,
                    y ? endValue.y : startPos.y,
                    z ? endValue.z : startPos.z
                ) + offset;

                if (relative) targetPos += startPos;

                yield return FlexTween.To(
                    () => target.position, 
                    val => target.position = val, 
                    targetPos, duration, ease);
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