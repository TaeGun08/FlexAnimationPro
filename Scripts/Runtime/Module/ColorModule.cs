using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
using System.Collections;
using System.Collections.Generic;

namespace FlexAnimation
{
    public enum ColorAnimMode { Single, Gradient, Rainbow, Pulse, Flicker, Impact, Relative, Theme }
    public enum ColorTheme { Success, Danger, Warning, Info, Neutral }
    public enum RelativeType { Brighter, Darker, Desaturate, Invert }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class ColorModule : AnimationModule
    {
        [Header("Primary Target")]
        public bool applyToChildren = false;
        public ColorAnimMode mode = ColorAnimMode.Single;

        [Header("Main Settings")]
        public Color targetColor = Color.white;
        public ColorTheme theme = ColorTheme.Info;
        public RelativeType relativeType = RelativeType.Brighter;
        [Range(0f, 1f)] public float amount = 0.2f;

        [Header("Impact Settings (Expert)")]
        public int flashCount = 1;
        public bool returnToInitial = true;

        [Header("Advanced (Expert)")]
        public Gradient gradient = new Gradient();
        [Range(0.1f, 20f)] public float speed = 1f;
        public string materialProperty = "_Color";

        // Runtime State
        private List<Graphic> _graphics = new List<Graphic>();
        private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
        private List<Material> _materials = new List<Material>();
        private Dictionary<object, Color> _initialColors = new Dictionary<object, Color>();

        public override IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            CollectTargets(target);
            SaveCurrentStates();

            float elapsed = 0f;
            while (elapsed < duration || loop != LoopMode.None)
            {
                float dt = (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * globalTimeScale;
                
                var owner = target.GetComponent<FlexAnimation>();
                if (owner != null && owner.IsPaused) dt = 0;
                
                elapsed += dt;
                float t = (duration > 0) ? Mathf.Clamp01(elapsed / duration) : 1f;
                float easedT = FlexTween.EvaluateEase(t, ease);

                ApplyToAll(easedT, elapsed);

                if (duration > 0 && t >= 1f && loop == LoopMode.None) break;
                yield return null;
            }

            if (loop == LoopMode.None) ApplyToAll(1f, elapsed);
        }

        private void CollectTargets(Transform target)
        {
            _graphics.Clear(); _sprites.Clear(); _materials.Clear();
            if (applyToChildren)
            {
                _graphics.AddRange(target.GetComponentsInChildren<Graphic>(true));
                _sprites.AddRange(target.GetComponentsInChildren<SpriteRenderer>(true));
                foreach (var r in target.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.material != null) _materials.Add(r.material);
                }
            }
            else
            {
                var g = target.GetComponent<Graphic>(); if (g) _graphics.Add(g);
                var s = target.GetComponent<SpriteRenderer>(); if (s) _sprites.Add(s);
                if (target.TryGetComponent(out Renderer r) && r.material != null) _materials.Add(r.material);
            }
        }

        private void SaveCurrentStates()
        {
            _initialColors.Clear();
            foreach (var g in _graphics) _initialColors[g] = g.color;
            foreach (var s in _sprites) _initialColors[s] = s.color;
            foreach (var m in _materials) if (m.HasProperty(materialProperty)) _initialColors[m] = m.GetColor(materialProperty);
        }

        private void ApplyToAll(float t, float elapsed)
        {
            foreach (var g in _graphics) g.color = GetResultColor(_initialColors.ContainsKey(g) ? _initialColors[g] : Color.white, t, elapsed);
            foreach (var s in _sprites) s.color = GetResultColor(_initialColors.ContainsKey(s) ? _initialColors[s] : Color.white, t, elapsed);
            foreach (var m in _materials)
            {
                if (m.HasProperty(materialProperty))
                    m.SetColor(materialProperty, GetResultColor(_initialColors.ContainsKey(m) ? _initialColors[m] : Color.white, t, elapsed));
            }
        }

        private Color GetResultColor(Color initial, float t, float elapsed)
        {
            switch (mode)
            {
                case ColorAnimMode.Impact:
                    float f = Mathf.PingPong(t * flashCount * 2f, 1f);
                    return Color.Lerp(initial, targetColor, f);

                case ColorAnimMode.Relative:
                    return ModifyRelative(initial, relativeType, t * amount);

                case ColorAnimMode.Theme:
                    return Color.Lerp(initial, GetThemeColor(theme), t);

                case ColorAnimMode.Gradient:
                    return gradient.Evaluate(t);

                case ColorAnimMode.Rainbow:
                    return Color.HSVToRGB((elapsed * speed * 0.1f) % 1f, 0.7f, 1f);

                case ColorAnimMode.Pulse:
                    float p = (Mathf.Sin(elapsed * speed * Mathf.PI) + 1f) * 0.5f;
                    return Color.Lerp(initial, targetColor, p);

                case ColorAnimMode.Flicker:
                    float n = Mathf.PerlinNoise(elapsed * speed * 2f, 0f);
                    return Color.Lerp(initial, targetColor, n);

                default:
                    return Color.Lerp(initial, targetColor, t);
            }
        }

        private Color GetThemeColor(ColorTheme t)
        {
            switch (t) {
                case ColorTheme.Success: return new Color(0.15f, 0.68f, 0.37f);
                case ColorTheme.Danger: return new Color(0.75f, 0.22f, 0.16f);
                case ColorTheme.Warning: return new Color(0.95f, 0.6f, 0.07f);
                case ColorTheme.Info: return new Color(0.16f, 0.5f, 0.72f);
                default: return new Color(0.58f, 0.64f, 0.65f);
            }
        }

        private Color ModifyRelative(Color baseCol, RelativeType type, float val)
        {
            float h, s, v;
            Color.RGBToHSV(baseCol, out h, out s, out v);
            switch (type) {
                case RelativeType.Brighter: v += val; break;
                case RelativeType.Darker: v -= val; break;
                case RelativeType.Desaturate: s -= val; break;
                case RelativeType.Invert: return Color.Lerp(baseCol, new Color(1-baseCol.r, 1-baseCol.g, 1-baseCol.b, baseCol.a), val);
            }
            Color res = Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
            res.a = baseCol.a;
            return res;
        }

        public override void OnStop(Transform target)
        {
            // Stop 시 초기 상태로 복구 로직 (필요 시)
        }
    }
}
