using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
using System.Collections;

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
        public bool persist = true; 

        [Header("Pipeline Settings")]
        public ShaderPipeline pipeline = ShaderPipeline.URP;

        [Header("Property Mode")]
        public string propertyName = "_BaseColor";
        public MaterialPropType propertyType = MaterialPropType.Color;
        public Color targetColor = Color.white;
        public float targetFloat;
        public Vector2 targetOffset;

        [Header("Blend Mode")]
        public Material targetMaterial; 
        public List<Material> sequenceMaterials = new List<Material>(); 
        public float interval = 0f; 

        [Header("Effect Mode")]
        public MaterialEffect effect = MaterialEffect.None;
        public Color effectColor = Color.white;
        public float effectIntensity = 1f;

        private static MaterialPropertyBlock _reusableBlock;
        private static MaterialPropertyBlock PropBlock => _reusableBlock ??= new MaterialPropertyBlock();

        // Optimized Cache
        private int _activePropId;
        private int _texSTId;
        private int _emissionId;

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (!target.TryGetComponent(out Renderer rend)) yield break;
            Material[] mats = rend.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= mats.Length) yield break;

            Material activeMat = (persist && Application.isPlaying) ? rend.materials[materialIndex] : mats[materialIndex];
            if (activeMat == null) yield break;

            InitializeIds();

            if (mode == MaterialMode.Property)
            {
                yield return RunPropertyMode(rend, activeMat, ignoreTimeScale, globalTimeScale);
            }
            else if (mode == MaterialMode.Blend)
            {
                yield return RunBlendMode(rend, activeMat, ignoreTimeScale, globalTimeScale);
            }
            else if (mode == MaterialMode.Effect)
            {
                yield return RunEffectMode(rend, activeMat, ignoreTimeScale, globalTimeScale);
            }
        }

        private void InitializeIds()
        {
            string colName = (pipeline == ShaderPipeline.Custom) ? propertyName : ((pipeline == ShaderPipeline.Standard) ? "_Color" : "_BaseColor");
            _activePropId = Shader.PropertyToID(colName);
            _emissionId = Shader.PropertyToID("_EmissionColor");
            
            string texName = (pipeline == ShaderPipeline.Custom) ? propertyName : ((pipeline == ShaderPipeline.Standard) ? "_MainTex" : "_BaseMap");
            _texSTId = Shader.PropertyToID(texName + "_ST");
        }

        private IEnumerator RunPropertyMode(Renderer rend, Material mat, bool ignore, float ts)
        {
            if (propertyType == MaterialPropType.Color)
            {
                Color start = mat.HasProperty(_activePropId) ? mat.GetColor(_activePropId) : Color.white;
                yield return FlexTween.To(() => 0f, t => ApplyColor(rend, mat, _activePropId, Color.LerpUnclamped(start, targetColor, t)), 1f, duration, ease, ignore, ts, loop, loopCount);
            }
            else if (propertyType == MaterialPropType.Float)
            {
                float start = mat.HasProperty(_activePropId) ? mat.GetFloat(_activePropId) : 0f;
                yield return FlexTween.To(() => 0f, t => ApplyFloat(rend, mat, _activePropId, Mathf.LerpUnclamped(start, targetFloat, t)), 1f, duration, ease, ignore, ts, loop, loopCount);
            }
            else if (propertyType == MaterialPropType.TextureOffset)
            {
                Vector2 start = mat.HasProperty(_activePropId) ? mat.GetTextureOffset(_activePropId) : Vector2.zero;
                yield return FlexTween.To(() => 0f, t => ApplyST(rend, mat, _activePropId, new Vector4(1, 1, Mathf.LerpUnclamped(start.x, targetOffset.x, t), Mathf.LerpUnclamped(start.y, targetOffset.y, t))), 1f, duration, ease, ignore, ts, loop, loopCount);
            }
        }

        private IEnumerator RunBlendMode(Renderer rend, Material mat, bool ignore, float ts)
        {
            List<Material> targets = new List<Material>();
            if (sequenceMaterials != null && sequenceMaterials.Count > 0) targets.AddRange(sequenceMaterials);
            else if (targetMaterial != null) targets.Add(targetMaterial);
            if (targets.Count == 0) yield break;

            float stepDuration = duration / targets.Count;
            Color current = mat.HasProperty(_activePropId) ? mat.GetColor(_activePropId) : Color.white;

            foreach (var next in targets)
            {
                if (next == null) continue;
                Color start = current;
                Color target = next.HasProperty(_activePropId) ? next.GetColor(_activePropId) : Color.white;

                yield return FlexTween.To(() => 0f, t => {
                    current = Color.LerpUnclamped(start, target, t);
                    ApplyColor(rend, mat, _activePropId, current);
                }, 1f, stepDuration, ease, ignore, ts);

                if (interval > 0) yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator RunEffectMode(Renderer rend, Material mat, bool ignore, float ts)
        {
            Color original = mat.HasProperty(_activePropId) ? mat.GetColor(_activePropId) : Color.white;
            
            yield return FlexTween.To(() => 0f, t => {
                float time = t * duration * 10f;
                if (effect == MaterialEffect.Flash)
                    ApplyColor(rend, mat, _activePropId, Color.LerpUnclamped(original, effectColor, Mathf.PingPong(time, 1f) * effectIntensity));
                else if (effect == MaterialEffect.Pulse)
                {
                    Color c = Color.LerpUnclamped(original, effectColor, (Mathf.Sin(time) + 1f) * 0.5f * effectIntensity);
                    ApplyColor(rend, mat, _activePropId, c);
                    if (mat.HasProperty(_emissionId)) ApplyColor(rend, mat, _emissionId, c);
                }
                else if (effect == MaterialEffect.Glitch && Random.value > 0.8f)
                    ApplyST(rend, mat, _activePropId, new Vector4(1, 1, Random.Range(-0.1f, 0.1f) * effectIntensity, Random.Range(-0.1f, 0.1f) * effectIntensity));
            }, 1f, duration, ease, ignore, ts, loop, loopCount);
        }

        private void ApplyColor(Renderer rend, Material mat, int id, Color val) {
            if (persist && Application.isPlaying) mat.SetColor(id, val);
            else { rend.GetPropertyBlock(PropBlock, materialIndex); PropBlock.SetColor(id, val); rend.SetPropertyBlock(PropBlock, materialIndex); }
        }
        private void ApplyFloat(Renderer rend, Material mat, int id, float val) {
            if (persist && Application.isPlaying) mat.SetFloat(id, val);
            else { rend.GetPropertyBlock(PropBlock, materialIndex); PropBlock.SetFloat(id, val); rend.SetPropertyBlock(PropBlock, materialIndex); }
        }
        private void ApplyST(Renderer rend, Material mat, int id, Vector4 st) {
            if (persist && Application.isPlaying) mat.SetVector(_texSTId, st);
            else { rend.GetPropertyBlock(PropBlock, materialIndex); PropBlock.SetVector(_texSTId, st); rend.SetPropertyBlock(PropBlock, materialIndex); }
        }
    }
}
