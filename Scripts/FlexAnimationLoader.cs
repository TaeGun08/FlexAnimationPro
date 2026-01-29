using UnityEngine;
using System.Collections.Generic;

namespace FlexAnimation
{
    public static class FlexAnimationLoader
    {
        /// <summary>
        /// Creates a runtime preset from a list of generic data objects.
        /// This is useful for loading animations from CSV or JSON.
        /// </summary>
        public static FlexAnimationPreset CreatePresetFromData(List<AnimationModule> modules)
        {
            var preset = ScriptableObject.CreateInstance<FlexAnimationPreset>();
            preset.modules = new List<AnimationModule>(modules);
            return preset;
        }

        // Note: Deep serialization of polymorphic lists from JSON is tricky in Unity.
        // If you need full JSON support, consider using Newtonsoft.Json or a custom wrapper.
    }
}