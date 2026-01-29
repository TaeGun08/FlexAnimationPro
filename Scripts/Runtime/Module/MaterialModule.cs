using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum MaterialPropType { Color, Float, Vector }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class MaterialModule : AnimationModule
    {
        [Header("Target")]
        public string propertyName = "_Color"; // Shader Property Name
        public int materialIndex = 0; // Renderer's material index

        [Header("Values")]
        public MaterialPropType propertyType = MaterialPropType.Color;
        public Color targetColor = Color.white;
        public float targetFloat;
        public Vector4 targetVector;

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            if (target.TryGetComponent(out Renderer rend))
            {
                if (materialIndex < 0 || materialIndex >= rend.materials.Length)
                {
                    Debug.LogWarning($"[FlexAnimation] Material Index {materialIndex} out of range for {target.name}");
                    return null;
                }

                Material mat = rend.materials[materialIndex];
                
                switch (propertyType)
                {
                    case MaterialPropType.Color:
                        return mat.DOColor(targetColor, propertyName, duration);
                    case MaterialPropType.Float:
                        return mat.DOFloat(targetFloat, propertyName, duration);
                    case MaterialPropType.Vector:
                        return mat.DOVector(targetVector, propertyName, duration);
                }
            }
            return null;
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target)
        {
            if (target.TryGetComponent(out Renderer rend))
            {
                if (materialIndex < 0 || materialIndex >= rend.materials.Length) yield break;
                Material mat = rend.materials[materialIndex];

                switch (propertyType)
                {
                    case MaterialPropType.Color:
                        yield return FlexTween.To(
                            () => mat.GetColor(propertyName),
                            val => mat.SetColor(propertyName, val),
                            targetColor, duration, ease);
                        break;
                    case MaterialPropType.Float:
                        yield return FlexTween.To(
                            () => mat.GetFloat(propertyName),
                            val => mat.SetFloat(propertyName, val),
                            targetFloat, duration, ease);
                        break;
                    case MaterialPropType.Vector:
                        Debug.LogWarning("[FlexAnimation] Vector Material Property not fully supported in Native Mode.");
                        break;
                }
            }
        }
    }
}