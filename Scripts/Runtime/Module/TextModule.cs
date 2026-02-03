using UnityEngine;
using TMPro;
using UnityEngine.Scripting.APIUpdating;
using FlexAnimation.Internal;
#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace FlexAnimation
{
    // Simplified and Polished Modes
    public enum TextProcessMode
    {
        Sequential, // Cascading (Waterfall) style
        Concurrent, // All at once with randomness
        Instant     // Immediate change
    }

    public enum TransitionStyle
    {
        None,
        Fade,       // Pure Opacity
        Flip,       // 3D Card Flip (X-Axis)
        Spin,       // 2D Rotation (Z-Axis)
        Slide,      // Directional Movement
        Zoom,       // Scale Up/Down
        Stretch,    // Elastic Squash & Stretch
        Vortex,     // Swirling warping
    }

    public enum TextEffect
    {
        None = 0,
        Glitch = 1 << 0,  // Digital Noise & Char Swap
        Wave = 1 << 1,    // Sine Movement
        Shake = 1 << 2    // Random Vibration
    }

    public enum ScrambleMode
    {
        None,
        Random,      // Matrix style decoding
        Numeric,     // Number counting
        Binary       // 0/1 decoding
    }

    [MovedFrom(true, null, "Assembly-CSharp", null)]
    [System.Serializable]
    public class TextModule : AnimationModule
    {
        [Header("Content")]
        public string customText; 
        
        [Header("Animation Settings")]
        public TextProcessMode processMode = TextProcessMode.Sequential;
        [Range(0f, 1f)] public float overlap = 0.5f; // New: Controls smoothness of sequence
        
        public TransitionStyle transition = TransitionStyle.Fade;
        public Vector2 slideDirection = Vector2.up; // Unified Slide Direction
        
        public TextEffect effects = TextEffect.None;
        public ScrambleMode scrambleMode = ScrambleMode.None;

        [Header("Detail Tuning")]
        public string scrambleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        public float waveFrequency = 2f;
        public float waveSpeed = 5f;
        public float effectStrength = 10f; // Shared strength for Wave/Shake/Glitch

#if DOTWEEN_ENABLED
        public override Tween CreateTween(Transform target)
        {
            return null; 
        }
#endif

        public override System.Collections.IEnumerator CreateRoutine(Transform target, bool ignoreTimeScale = false, float globalTimeScale = 1f)
        {
            if (target.TryGetComponent(out TMP_Text tmp))
            {
                string startText = tmp.text;
                string endText = string.IsNullOrEmpty(customText) ? tmp.text : customText;
                
                // Pre-calc Numeric/Binary Expansion
                if (scrambleMode == ScrambleMode.Binary)
                {
                    endText = StringToBinary(startText); // Expansion logic
                    if(tmp.text.Length < endText.Length) startText = startText.PadRight(endText.Length);
                }
                
                int maxLen = Mathf.Max(startText.Length, endText.Length);
                string paddedStart = startText.PadRight(maxLen);
                string paddedEnd = endText.PadRight(maxLen);

                // Random offsets for Concurrent/Glitch
                float[] randomSeeds = new float[maxLen];
                for(int i=0; i<maxLen; i++) randomSeeds[i] = UnityEngine.Random.value;

                // Set initial text
                tmp.text = paddedStart;
                tmp.ForceMeshUpdate();

                yield return FlexTween.To(
                    () => 0f,
                    val => 
                    {
                        // 1. Resolve Text Content
                        char[] resultChars = new char[maxLen];
                        
                        // Numeric special case (Global)
                        if (scrambleMode == ScrambleMode.Numeric)
                        {
                            long sVal=0, eVal=0;
                            long.TryParse(System.Text.RegularExpressions.Regex.Replace(startText, "[^0-9]", ""), out sVal);
                            long.TryParse(System.Text.RegularExpressions.Regex.Replace(endText, "[^0-9]", ""), out eVal);
                            long curVal = (long)Mathf.Lerp(sVal, eVal, val);
                            tmp.text = curVal.ToString();
                        }
                        else
                        {
                            for (int i = 0; i < maxLen; i++)
                            {
                                float p = GetCharProgress(val, i, maxLen, randomSeeds[i]);
                                
                                // Content Swap Logic
                                bool isSwapped = (p >= 0.5f);
                                
                                // Glitch Effect on Content
                                if ((effects & TextEffect.Glitch) != 0 && p > 0 && p < 1)
                                {
                                    // Random char flicker during animation
                                    if (UnityEngine.Random.value < 0.2f) 
                                        resultChars[i] = scrambleChars[UnityEngine.Random.Range(0, scrambleChars.Length)];
                                    else 
                                        resultChars[i] = isSwapped ? paddedEnd[i] : paddedStart[i];
                                }
                                else if (scrambleMode == ScrambleMode.Random)
                                {
                                    if (!isSwapped) resultChars[i] = scrambleChars[UnityEngine.Random.Range(0, scrambleChars.Length)];
                                    else resultChars[i] = paddedEnd[i];
                                }
                                else
                                {
                                    resultChars[i] = isSwapped ? paddedEnd[i] : paddedStart[i];
                                }
                            }
                            // Only update text if not numeric
                            tmp.text = new string(resultChars).TrimEnd();
                        }
                        
                        tmp.ForceMeshUpdate();

                        // 2. Animate Geometry
                        TMP_TextInfo textInfo = tmp.textInfo;
                        int count = textInfo.characterCount;
                        
                        for (int i = 0; i < count; i++)
                        {
                            if (!textInfo.characterInfo[i].isVisible) continue;

                            int matIdx = textInfo.characterInfo[i].materialReferenceIndex;
                            int vertIdx = textInfo.characterInfo[i].vertexIndex;
                            Vector3[] verts = textInfo.meshInfo[matIdx].vertices;
                            Color32[] colors = textInfo.meshInfo[matIdx].colors32;

                            float p = GetCharProgress(val, i, maxLen, randomSeeds[i]);
                            
                            // Apply Transition (Movement/Scale/Rot)
                            ApplyTransition(transition, p, verts, vertIdx);
                            
                            // Apply Continuous Effects
                            float time = val * duration;
                            ApplyContinuousEffects(effects, p, verts, vertIdx, i, time);
                            
                            // Handle Colors (Fade)
                            ApplyColor(transition, p, colors, vertIdx);
                        }
                        
                        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
#if UNITY_EDITOR
                        if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(tmp);
#endif
                    },
                    1f, duration, ease, ignoreTimeScale, globalTimeScale, loop, loopCount
                );
                
                tmp.text = endText;
            }
        }

        // --- Core Math ---

        private float GetCharProgress(float globalProgress, int index, int total, float randomSeed)
        {
            if (processMode == TextProcessMode.Instant) return 1f;
            
            if (processMode == TextProcessMode.Concurrent)
            {
                // Scramble style: strictly based on threshold? Or smooth concurrent?
                // Let's make it smooth concurrent but with random start times.
                // Map global 0..1 to random start..end windows? 
                // Simple approach: globalProgress * speed + offset
                return Mathf.Clamp01(globalProgress * 1.5f - (randomSeed * 0.5f)); 
            }
            else // Sequential
            {
                // Smooth Cascading Logic
                // Global 0 -> 1 maps to sequential windows.
                // Overlap 0: [0-0.1], [0.1-0.2]...
                // Overlap 1: All start near same time but slightly delayed.
                
                if (total <= 1) return globalProgress;

                float step = 1f / total;
                float visibleWindow = step + (overlap * 0.5f); // Increase window size by overlap
                
                // Formula to map Global(0..1) to Local(0..1) for Index(i)
                // Start time for char i: i * step * (1 - overlap)
                float effectiveTotal = 1f + (visibleWindow * total * overlap); // Adjust total duration scale
                
                // Cleaner Cascading Math:
                // We want the whole sequence to fit in 0..1.
                // Let 'delay' be the spacing between starts.
                float delay = (1f - overlap) / total; 
                // Actually simpler:
                // Start = index / total * (1 - overlap_factor)
                // Duration of one char = something relative.
                
                // Let's use a robust "Waterfall" formula
                float totalDelay = 1f - 0.2f; // Reserve 20% for individual animation time at least
                float myStart = (float)index / total * (1f - overlap);
                float myEnd = myStart + 0.2f + (overlap * 0.8f); // Width varies by overlap
                
                // Remap globalProgress(0..1) to (myStart..myEnd) -> 0..1
                // Standard mapping: (val - start) / (end - start)
                float localP = (globalProgress - myStart) / (myEnd - myStart);
                return Mathf.Clamp01(localP);
            }
        }

        private void ApplyTransition(TransitionStyle style, float p, Vector3[] verts, int idx)
        {
            // p: 0 (Start/Old) -> 1 (End/New)
            if (p <= 0 || p >= 1) return;

            Vector3 center = (verts[idx] + verts[idx+2]) * 0.5f;
            bool isOld = p < 0.5f;
            float t = isOld ? p * 2f : (p - 0.5f) * 2f; // 0->1 per phase

            switch (style)
            {
                case TransitionStyle.Flip:
                    // 0->90 (Old), 270->360 (New)
                    // Add slight Scale dip to simulate distance
                    float angle = isOld ? Mathf.Lerp(0, 90, t) : Mathf.Lerp(270, 360, t);
                    float scaleF = 1f - Mathf.Sin(t * Mathf.PI) * 0.3f; // Shrink slightly at peak
                    Matrix4x4 matF = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(angle, 0, 0), Vector3.one * scaleF);
                    ApplyMatrix(verts, idx, matF, center);
                    break;

                case TransitionStyle.Spin:
                    // Full spin with scale
                    float spin = Mathf.Lerp(0, 360, p);
                    float scaleS = p < 0.5f ? Mathf.Lerp(1, 0, t) : Mathf.Lerp(0, 1, t); // Zoom out/in
                    ApplyMatrix(verts, idx, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, spin), Vector3.one * scaleS), center);
                    break;

                case TransitionStyle.Slide:
                    // Use slideDirection
                    Vector3 offsetDir = new Vector3(slideDirection.x, slideDirection.y, 0) * 20f;
                    // Old: 0 -> move away. New: move from away -> 0.
                    Vector3 move = isOld ? Vector3.Lerp(Vector3.zero, offsetDir, t) : Vector3.Lerp(-offsetDir, Vector3.zero, t);
                    ApplyOffset(verts, idx, move);
                    break;

                case TransitionStyle.Zoom:
                    float scaleZ = isOld ? Mathf.Lerp(1, 0, t) : Mathf.Lerp(0, 1, t);
                    ApplyMatrix(verts, idx, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scaleZ), center);
                    break;
                    
                case TransitionStyle.Stretch:
                    // Elastic scale: X shrinks, Y grows (Squash) then swap.
                    float sy = isOld ? Mathf.Lerp(1, 0, t) : Mathf.Lerp(0, 1, t);
                    float sx = isOld ? Mathf.Lerp(1, 2, t) : Mathf.Lerp(2, 1, t);
                    ApplyMatrix(verts, idx, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sx, sy, 1)), center);
                    break;
                    
                case TransitionStyle.Vortex:
                    float rotV = isOld ? Mathf.Lerp(0, 90, t) : Mathf.Lerp(-90, 0, t);
                    float sclV = isOld ? Mathf.Lerp(1, 0, t) : Mathf.Lerp(0, 1, t);
                    ApplyMatrix(verts, idx, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotV), Vector3.one * sclV), center);
                    break;
            }
        }

        private void ApplyColor(TransitionStyle style, float p, Color32[] colors, int idx)
        {
            // Calculate Alpha
            byte alpha = 255;
            
            if (style == TransitionStyle.None)
            {
                // Just appear (0->1)
                alpha = (byte)(Mathf.Clamp01(p) * 255);
            }
            else
            {
                // Cross fade (1->0 then 0->1)
                bool isOld = p < 0.5f;
                float t = isOld ? p * 2f : (p - 0.5f) * 2f;
                float aVal = isOld ? Mathf.Lerp(1, 0, t) : Mathf.Lerp(0, 1, t);
                alpha = (byte)(aVal * 255);
            }
            
            SetCharAlpha(colors, idx, alpha);
        }

        private void ApplyContinuousEffects(TextEffect effect, float p, Vector3[] verts, int idx, int charIdx, float time)
        {
            if (p < 0 || p > 1) return; // Only apply active effects during transition? Or always?
            // "Instant" mode keeps p=1. So this works.
            
            if ((effect & TextEffect.Wave) != 0)
            {
                float y = Mathf.Sin(time * waveSpeed + charIdx * waveFrequency) * (effectStrength * 0.5f);
                ApplyOffset(verts, idx, new Vector3(0, y, 0));
            }
            if ((effect & TextEffect.Shake) != 0)
            {
                Vector3 rnd = UnityEngine.Random.insideUnitSphere * (effectStrength * 0.1f);
                ApplyOffset(verts, idx, rnd);
            }
            if ((effect & TextEffect.Glitch) != 0)
            {
                if (UnityEngine.Random.value > 0.9f)
                {
                    float offset = effectStrength * 0.5f;
                    // Jitter vertices independently for "broken" look
                    verts[idx+0] += (Vector3)UnityEngine.Random.insideUnitCircle * offset;
                    verts[idx+1] += (Vector3)UnityEngine.Random.insideUnitCircle * offset;
                    verts[idx+2] += (Vector3)UnityEngine.Random.insideUnitCircle * offset;
                    verts[idx+3] += (Vector3)UnityEngine.Random.insideUnitCircle * offset;
                }
            }
        }

        // --- Utils ---
        
        private string StringToBinary(string s)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(char c in s)
            {
                if(char.IsWhiteSpace(c)) sb.Append(" ");
                else sb.Append(System.Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        private void ApplyOffset(Vector3[] verts, int idx, Vector3 offset)
        {
            for(int i=0; i<4; i++) verts[idx+i] += offset;
        }

        private void ApplyMatrix(Vector3[] verts, int idx, Matrix4x4 m, Vector3 c)
        {
            for(int i=0; i<4; i++) verts[idx+i] = m.MultiplyPoint3x4(verts[idx+i] - c) + c;
        }

        private void SetCharAlpha(Color32[] cols, int idx, byte a)
        {
            for(int i=0; i<4; i++) cols[idx+i].a = a;
        }
    }
}