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

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            Vector3 initialPos = target.localPosition;
            Quaternion initialRot = target.localRotation;
            Vector3 initialScale = target.localScale;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                elapsed += dt;

                float percent = elapsed / duration;
                // Decay strength over time if fadeOut is true
                float currentStrength = fadeOut ? strength * (1f - percent) : strength;
                
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ) * currentStrength;

                switch (type)
                {
                    case ShakeType.Position:
                        target.localPosition = initialPos + randomOffset;
                        break;
                    case ShakeType.Rotation:
                        target.localRotation = initialRot * Quaternion.Euler(randomOffset * 10f);
                        break;
                    case ShakeType.Scale:
                        target.localScale = initialScale + randomOffset * 0.2f;
                        break;
                }

                yield return null;
            }

            // Restore initial state
            target.localPosition = initialPos;
            target.localRotation = initialRot;
            target.localScale = initialScale;
        }
    }
}