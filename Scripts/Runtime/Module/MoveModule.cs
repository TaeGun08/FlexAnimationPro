using UnityEngine;
using System.Collections;
using FlexAnimation.Internal;
using UnityEngine.Scripting.APIUpdating;

namespace FlexAnimation
{
    public enum MoveAnimMode { Absolute, Relative, Direction, Target }
    public enum MoveDirection { Left, Right, Up, Down, Forward, Back }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class MoveModule : AnimationModule
    {
        [Header("Movement Mode")]
        public MoveAnimMode mode = MoveAnimMode.Relative;
        public FlexSpace space = FlexSpace.Local;

        [Header("Position Settings")]
        public Vector3 position; // Used for Absolute & Relative
        public MoveDirection direction = MoveDirection.Right;
        public float distance = 100f;
        public Transform targetTransform;

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            RectTransform rect = target as RectTransform;
            Vector3 startPos = GetCurrentPos(target, rect);
            Vector3 destination = CalculateDestination(target, startPos);

            yield return FlexTween.To(
                () => GetCurrentPos(target, rect),
                val => SetPos(target, rect, val),
                destination, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount
            );
        }

        private Vector3 GetCurrentPos(Transform t, RectTransform r)
        {
            if (r) return r.anchoredPosition3D;
            return space == FlexSpace.Local ? t.localPosition : t.position;
        }

        private void SetPos(Transform t, RectTransform r, Vector3 val)
        {
            if (r) r.anchoredPosition3D = val;
            else if (space == FlexSpace.Local) t.localPosition = val;
            else t.position = val;
        }

        private Vector3 CalculateDestination(Transform t, Vector3 start)
        {
            switch (mode)
            {
                case MoveAnimMode.Absolute: 
                    return position;
                case MoveAnimMode.Relative: 
                    return start + position;
                case MoveAnimMode.Direction: 
                    return start + GetDirVector() * distance;
                case MoveAnimMode.Target: 
                    if (targetTransform == null) return start;
                    return (space == FlexSpace.Local && t.parent != null) 
                        ? t.parent.InverseTransformPoint(targetTransform.position) 
                        : targetTransform.position;
                default: 
                    return start;
            }
        }

        private Vector3 GetDirVector()
        {
            switch (direction)
            {
                case MoveDirection.Left: return Vector3.left;
                case MoveDirection.Right: return Vector3.right;
                case MoveDirection.Up: return Vector3.up;
                case MoveDirection.Down: return Vector3.down;
                case MoveDirection.Forward: return Vector3.forward;
                case MoveDirection.Back: return Vector3.back;
                default: return Vector3.zero;
            }
        }
    }
}
