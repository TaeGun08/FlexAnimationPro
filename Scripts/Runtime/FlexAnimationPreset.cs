using UnityEngine;
using System.Collections.Generic;

namespace FlexAnimation
{
    [CreateAssetMenu(fileName = "New Animation Preset", menuName = "FlexAnimation/Preset")]
    public class FlexAnimationPreset : ScriptableObject
    {
        [SerializeReference]
        public List<AnimationModule> modules = new List<AnimationModule>();
    }
}
