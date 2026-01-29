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
    public enum LoopMode { None, Loop, Yoyo }

    public enum FlexLinkType
    {
        Append, // 이전 동작이 끝나고 실행 (순차)
        Join,   // 이전 동작과 함께 실행 (동시)
        Insert  // 특정 시간에 실행
    }

    [Serializable]
    public abstract class AnimationModule
    {
        [Header("Behavior")]
        public bool enabled = true;
        public FlexLinkType linkType = FlexLinkType.Join;
        public float delay; 

        [Header("Settings")]
        public float duration = 0.5f;
        public AnimEase ease = AnimEase.Linear;
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
        
        protected Ease GetEase() => Enum.TryParse(ease.ToString(), out Ease res) ? res : Ease.Linear;
#endif

        // Native Fallback
        public virtual IEnumerator CreateRoutine(Transform target)
        {
            yield break; // Override in subclasses
        }
    }
}