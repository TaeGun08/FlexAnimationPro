using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    public enum MaterialPropType { Color, Float, Vector, TextureOffset }
    public enum MaterialMode { Property, Blend, Effect }
    public enum MaterialEffect { None, Flash, Glitch, Pulse }
    public enum ShaderPipeline { Standard, URP, Custom }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class MaterialModule : AnimationModule
    {
        [Header("Mode")]
        public MaterialMode mode = MaterialMode.Property;
        public int materialIndex = 0; 
        public bool persist = true; // Maintain changes after animation ends

        [Header("Pipeline Settings")]
        public ShaderPipeline pipeline = ShaderPipeline.URP;

        [Header("Property Mode")]
        public string propertyName = "_BaseColor";
        public MaterialPropType propertyType = MaterialPropType.Color;
        public Color targetColor = Color.white;
        public float targetFloat;
        public Vector4 targetVector;
        public Vector2 targetOffset;

        [Header("Blend Mode")]
        public Material targetMaterial; // Single target (Legacy/Simple)
        public List<Material> sequenceMaterials = new List<Material>(); 
        public float interval = 0f; // Delay between steps

        [Header("Effect Mode")]
        public MaterialEffect effect = MaterialEffect.None;
        public Color effectColor = Color.white;
        public float effectIntensity = 1f;

        private static MaterialPropertyBlock _reusableBlock;
        private static MaterialPropertyBlock PropBlock => _reusableBlock ??= new MaterialPropertyBlock();

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return null; 
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (target.TryGetComponent(out Renderer rend))
            {
                if (materialIndex < 0 || materialIndex >= rend.sharedMaterials.Length) yield break;

                // Use .material at runtime for persistence, .sharedMaterial for editor safety
                Material activeMat = (persist && Application.isPlaying) ? rend.materials[materialIndex] : rend.sharedMaterials[materialIndex];
                if (activeMat == null) yield break;

                if (mode == MaterialMode.Property)
                {
                    switch (propertyType)
                    {
                        case MaterialPropType.Color:
                            Color startCol = activeMat.HasProperty(propertyName) ? activeMat.GetColor(propertyName) : Color.white;
                            yield return FlexTween.To(() => 0f, t => {
                                Color lerped = Color.LerpUnclamped(startCol, targetColor, t);
                                ApplyColor(rend, activeMat, propertyName, lerped);
                            }, 1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
                            break;

                        case MaterialPropType.Float:
                            float startF = activeMat.HasProperty(propertyName) ? activeMat.GetFloat(propertyName) : 0f;
                            yield return FlexTween.To(() => 0f, t => {
                                float lerped = Mathf.LerpUnclamped(startF, targetFloat, t);
                                ApplyFloat(rend, activeMat, propertyName, lerped);
                            }, 1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
                            break;

                        case MaterialPropType.TextureOffset:
                            Vector2 startO = activeMat.HasProperty(propertyName) ? activeMat.GetTextureOffset(propertyName) : Vector2.zero;
                            yield return FlexTween.To(() => 0f, t => {
                                Vector2 curOff = Vector2.LerpUnclamped(startO, targetOffset, t);
                                ApplyOffset(rend, activeMat, propertyName, curOff);
                            }, 1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
                            break;
                    }
                }
                else if (mode == MaterialMode.Blend)
                {
                    // Consolidate targets
                    List<Material> targets = new List<Material>();
                    if (sequenceMaterials != null && sequenceMaterials.Count > 0) targets.AddRange(sequenceMaterials);
                    else if (targetMaterial != null) targets.Add(targetMaterial);
                    
                    if (targets.Count > 0)
                    {
                        string colProp = (pipeline == ShaderPipeline.Custom) ? propertyName : ((pipeline == ShaderPipeline.Standard) ? "_Color" : "_BaseColor");
                        float stepDuration = duration / targets.Count;

                        // Track current color manually because PropertyBlock values cannot be read back easily from Material.GetColor
                        Color currentDisplayColor = activeMat.HasProperty(colProp) ? activeMat.GetColor(colProp) : Color.white;
                        
                        foreach (var nextMat in targets)
                        {
                            if (nextMat == null) continue;
                            
                            Color nextColor = nextMat.HasProperty(colProp) ? nextMat.GetColor(colProp) : Color.white;
                            Color startColor = currentDisplayColor;

                            yield return FlexTween.To(() => 0f, t => {
                                currentDisplayColor = Color.Lerp(startColor, nextColor, t);
                                ApplyColor(rend, activeMat, colProp, currentDisplayColor);
                            }, 1f, stepDuration, ease, ignoreTimeScale, globalTimeScale); 
                            
                            // Ensure final value is set before waiting
                            currentDisplayColor = nextColor;
                            ApplyColor(rend, activeMat, colProp, nextColor);
                            
                            if (interval > 0)
                            {
                                float wait = 0;
                                while(wait < interval)
                                {
                                    wait += (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                                    yield return null;
                                }
                            }
                        }
                    }
                }
                else if (mode == MaterialMode.Effect)
                {
                    string colProp = (pipeline == ShaderPipeline.Custom) ? propertyName : ((pipeline == ShaderPipeline.Standard) ? "_Color" : "_BaseColor");
                    Color originalCol = activeMat.HasProperty(colProp) ? activeMat.GetColor(colProp) : Color.white;
                    
                    yield return FlexTween.To(() => 0f, t => {
                        float fxTime = t * duration * 10f; 
                        
                        if (effect == MaterialEffect.Flash) {
                            float flash = Mathf.PingPong(fxTime, 1f);
                            ApplyColor(rend, activeMat, colProp, Color.Lerp(originalCol, effectColor, flash * effectIntensity));
                        }
                        else if (effect == MaterialEffect.Glitch) {
                            // Determine texture property name
                            string texProp = (pipeline == ShaderPipeline.Custom) ? propertyName : ((pipeline == ShaderPipeline.Standard) ? "_MainTex" : "_BaseMap");
                            
                            if (UnityEngine.Random.value > 0.8f) {
                                float strength = effectIntensity * 0.2f; // Scale intensity
                                float rx = UnityEngine.Random.Range(-strength, strength);
                                float ry = UnityEngine.Random.Range(-strength, strength);
                                ApplyOffsetST(rend, activeMat, texProp, new Vector4(1, 1, rx, ry));
                            } else {
                                ApplyOffsetST(rend, activeMat, texProp, new Vector4(1, 1, 0, 0));
                            }
                        }
                        else if (effect == MaterialEffect.Pulse) {
                            float pulse = (Mathf.Sin(fxTime) + 1f) * 0.5f; 
                            Color c = Color.Lerp(originalCol, effectColor, pulse * effectIntensity);
                            ApplyColor(rend, activeMat, colProp, c);
                            if(activeMat.HasProperty("_EmissionColor")) ApplyColor(rend, activeMat, "_EmissionColor", c);
                        }
                    }, 1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount);
                }
            }
        }

        // --- Helpers ---
        private void ApplyColor(Renderer rend, Material mat, string name, Color val) {
            if (persist && Application.isPlaying) mat.SetColor(name, val);
            else {
                rend.GetPropertyBlock(PropBlock, materialIndex);
                PropBlock.SetColor(name, val);
                rend.SetPropertyBlock(PropBlock, materialIndex);
            }
        }
        private void ApplyFloat(Renderer rend, Material mat, string name, float val) {
            if (persist && Application.isPlaying) mat.SetFloat(name, val);
            else {
                rend.GetPropertyBlock(PropBlock, materialIndex);
                PropBlock.SetFloat(name, val);
                rend.SetPropertyBlock(PropBlock, materialIndex);
            }
        }
        private void ApplyOffset(Renderer rend, Material mat, string name, Vector2 val) {
            if (persist && Application.isPlaying) mat.SetTextureOffset(name, val);
            else {
                rend.GetPropertyBlock(PropBlock, materialIndex);
                PropBlock.SetVector(name + "_ST", new Vector4(1, 1, val.x, val.y));
                rend.SetPropertyBlock(PropBlock, materialIndex);
            }
        }
        private void ApplyOffsetST(Renderer rend, Material mat, string name, Vector4 st) {
            if (persist && Application.isPlaying) {
                mat.SetVector(name + "_ST", st);
            }
            else {
                rend.GetPropertyBlock(PropBlock, materialIndex);
                PropBlock.SetVector(name + "_ST", st);
                rend.SetPropertyBlock(PropBlock, materialIndex);
            }
        }
    }
}