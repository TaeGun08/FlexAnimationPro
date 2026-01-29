using UnityEngine;
using System.Collections.Generic;

namespace FlexAnimation
{
    public enum FlexValueType
    {
        Constant,
        RandomRange
    }

    [CreateAssetMenu(fileName = "New FlexPreset", menuName = "FlexAnimation/Preset")]
    public class FlexAnimationPreset : ScriptableObject
    {
        [SerializeReference]
        public List<AnimationModule> modules = new List<AnimationModule>();
    }
}