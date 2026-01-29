using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum ShakeType { Position, Rotation, Scale }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class ShakeModule : AnimationModule
    {
        [Header("Values")]
        public ShakeType type = ShakeType.Position;
        public float strength = 1f;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            switch (type)
            {
                case ShakeType.Position:
                    return target.DOShakePosition(duration, strength, vibrato, randomness, false, fadeOut);
                case ShakeType.Rotation:
                    return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut);
                case ShakeType.Scale:
                    return target.DOShakeScale(duration, strength, vibrato, randomness, fadeOut);
            }
            return null;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            Debug.LogWarning("[FlexAnimation] Shake is not supported in Native Mode. Please install DOTween for full features.");
            yield break;
        }
    }
}