using UnityEngine;
using System;

namespace FlexAnimation
{
    [Serializable]
    public class AnimationModule
    {
        public bool enabled = true;
        public FlexLinkType linkType;
        public float duration = 1f;
        public float delay = 0f;
        public Ease ease = Ease.OutQuad;
        public bool loop = false;
        public int loopCount = -1;
        public bool relative = false;
        public float randomSpread = 0f;
        public Space space = Space.Self;
    }
}
