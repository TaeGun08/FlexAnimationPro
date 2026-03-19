using System;
using UnityEngine;

namespace FlexAnimation
{
    public enum FlexAnimationNodeType
    {
        Clip,
        Blend,
        Lerp,
        Output,
        FlexPreset,
        OrganicNoise,
        ShaderProperty,
        ShaderEffect,
        Spring,
        PointerInput,
        AudioReactor,
        TimeRewind,
        ParticleControl,
        Wait,
        Compare,
        Sequence,
        GlobalSet,
        GlobalGet,
        Math,
        Custom
    }

    public enum FlexShaderPropertyType { Float, Color }
    public enum FlexShaderEffectType { Blink, Pulse, SineWave, Flash }
    public enum FlexPointerMode { Hover, Distance, Click }
    public enum FlexAudioBand { Bass, Mid, Treble, All }
    public enum FlexParticleControlField { EmissionRate, StartSpeed, StartSize, StartColor }
    public enum FlexCompareType { Greater, Less, Equal, NotEqual }
    public enum FlexMathOp { Add, Multiply, Subtract, Divide, Min, Max }

    [Serializable]
    public class FlexAnimationNodeData
    {
        public string Guid;
        public string Title;
        public FlexAnimationNodeType NodeType;
        public Vector2 Position;
        
        // Data for specific node types
        public UnityEngine.AnimationClip Clip;
        public FlexAnimationPreset Preset;
        
        // Animation Settings
        public float Speed = 1.0f;
        public float Weight = 1.0f;
        public float BlendValue = 0.5f;

        // Creative - Organic Noise Settings
        public float NoiseFrequency = 1.0f;
        public float NoiseAmplitude = 0.5f;
        public Vector3 NoiseAxis = Vector3.up;

        // Creative - Shader Settings
        public Material TargetMaterial;
        public string PropertyName = "_Color";
        public FlexShaderPropertyType PropertyType = FlexShaderPropertyType.Color;
        public float FloatValue = 1.0f;
        public Color ColorValue = Color.white;

        // Creative - v1.3.0 Shader Effects
        public FlexShaderEffectType EffectType = FlexShaderEffectType.Pulse;
        public float EffectFrequency = 1.0f;
        public float EffectIntensity = 1.0f;
        public float EffectMin = 0.0f;
        public float EffectMax = 1.0f;

        // Creative - v1.4.0 Physics & Interaction
        public float Stiffness = 100.0f;
        public float Damping = 10.0f;
        public FlexPointerMode PointerMode = FlexPointerMode.Hover;
        public float InteractionRange = 500.0f;

        // Creative - v1.5.0 Audio Reacting
        public FlexAudioBand AudioBand = FlexAudioBand.Bass;
        public float AudioSensitivity = 2.0f;
        public float AudioThreshold = 0.05f;

        // Creative - v1.6.0 Time Rewind
        public float BufferSeconds = 5.0f;
        public float RewindStrength = 1.0f;

        // Creative - v1.7.0 Particle Control
        public ParticleSystem TargetParticle;
        public FlexParticleControlField ParticleField = FlexParticleControlField.EmissionRate;
        public float ParticleMin = 0.0f;
        public float ParticleMax = 50.0f;
        public Color ParticleColor = Color.white;

        // Creative - v1.8.0 Logic & Flow
        public float Duration = 1.0f;
        public float Threshold = 0.5f;
        public FlexCompareType CompareType = FlexCompareType.Greater;

        // Creative - v1.9.0 Global Sync
        public string GlobalKey = "GlobalValue";

        // Creative - v2.0.0 Modular Hub
        public FlexMathOp MathOp = FlexMathOp.Add;
        public Component CustomComponent;
        public string CustomMethod;
    }
}
