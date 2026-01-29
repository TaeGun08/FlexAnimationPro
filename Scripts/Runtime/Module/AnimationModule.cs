using UnityEngine;
using System;
using System.Collections;
#if DOTWEEN_ENABLED
using DG.Tweening;
#else
using FlexAnimation.Internal;
#endif

namespace FlexAnimation
{
    public enum FlexSpace { Local, World }

    [Serializable]
    public abstract class AnimationModule
    {
        [Header("Behavior")]
        public bool enabled = true;
        public FlexLinkType linkType = FlexLinkType.Join;
        public float delay; 

        [Header("Settings")]
        public float duration = 0.5f;
        public Ease ease = Ease.Linear;
        public LoopMode loop = LoopMode.None;
        public int loopCount = -1;

#if DOTWEEN_ENABLED
        public virtual Tween CreateTween(Transform target) { return null; }
        
        public void ApplyCommonSettings(Tween t)
        {
            if (t == null) return;
            t.SetEase(GetEase());

            // DOTween Sequence does not allow infinite loops (-1) for nested tweens.
            // We use int.MaxValue instead to simulate infinite loop without the warning.
            int finalLoopCount = loopCount < 0 ? int.MaxValue : loopCount;

            switch (loop) {
                case LoopMode.Loop: t.SetLoops(finalLoopCount, LoopType.Restart); break;
                case LoopMode.Yoyo: t.SetLoops(finalLoopCount, LoopType.Yoyo); break;
            }
        }
        
        protected DG.Tweening.Ease GetEase() => Enum.TryParse(ease.ToString(), out DG.Tweening.Ease res) ? res : DG.Tweening.Ease.Linear;
#endif

        // Native Fallback
        public virtual IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            yield break; // Override in subclasses
        }
    }
}