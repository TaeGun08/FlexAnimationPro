using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Linq;

namespace FlexAnimation
{
    [RequireComponent(typeof(Animator))]
    public class FlexAnimationPlayer : MonoBehaviour
    {
        public FlexAnimationGraph Graph;
        
        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _playableOutput;
        private Dictionary<string, Playable> _playableMap = new Dictionary<string, Playable>();

        public float GetRuntimeValue(string nodeGuid)
        {
            if (!_playableGraph.IsValid() || !_playableMap.TryGetValue(nodeGuid, out var playable)) return 0f;
            
            // For nodes that act as providers (Output, Math, etc.), we check their propagated weight or first input
            if (playable.GetInputCount() > 0) return playable.GetInputWeight(0);
            return 0f;
        }
        
        private void Start()
        {
            if (Graph == null) return;
            BuildGraph();
        }

        private void BuildGraph()
        {
            _playableGraph = PlayableGraph.Create(gameObject.name + "_FlexAnim");
            _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", GetComponent<Animator>());

            var playableMap = new Dictionary<string, Playable>();

            foreach (var nodeData in Graph.Nodes)
            {
                Playable playable;
                switch (nodeData.NodeType)
                {
                    case FlexAnimationNodeType.Clip:
                        var clipPlayable = AnimationClipPlayable.Create(_playableGraph, nodeData.Clip);
                        clipPlayable.SetSpeed(nodeData.Speed);
                        playable = clipPlayable;
                        break;
                    case FlexAnimationNodeType.FlexPreset:
                        var presetPlayable = ScriptPlayable<FlexPresetPlayableBehaviour>.Create(_playableGraph);
                        var behaviour = presetPlayable.GetBehaviour();
                        behaviour.Initialize(nodeData.Preset, gameObject);
                        playable = presetPlayable;
                        break;
                    case FlexAnimationNodeType.OrganicNoise:
                        var noisePlayable = ScriptPlayable<FlexNoisePlayableBehaviour>.Create(_playableGraph);
                        var noiseBehaviour = noisePlayable.GetBehaviour();
                        noiseBehaviour.Initialize(nodeData.NoiseFrequency, nodeData.NoiseAmplitude, nodeData.NoiseAxis, transform);
                        playable = noisePlayable;
                        break;
                    case FlexAnimationNodeType.ShaderProperty:
                        var shaderPlayable = ScriptPlayable<FlexShaderPlayableBehaviour>.Create(_playableGraph);
                        var shaderBehaviour = shaderPlayable.GetBehaviour();
                        shaderBehaviour.Initialize(nodeData.TargetMaterial, nodeData.PropertyName, nodeData.PropertyType, nodeData.FloatValue, nodeData.ColorValue);
                        playable = shaderPlayable;
                        break;
                    case FlexAnimationNodeType.ShaderEffect:
                        var effectPlayable = ScriptPlayable<FlexShaderEffectBehaviour>.Create(_playableGraph);
                        var effectBehaviour = effectPlayable.GetBehaviour();
                        effectBehaviour.Initialize(nodeData.TargetMaterial, nodeData.PropertyName, nodeData.PropertyType, 
                            nodeData.EffectType, nodeData.EffectFrequency, nodeData.EffectIntensity, 
                            nodeData.EffectMin, nodeData.EffectMax, nodeData.ColorValue, nodeData.FloatValue);
                        playable = effectPlayable;
                        break;
                    case FlexAnimationNodeType.Spring:
                        var springPlayable = ScriptPlayable<FlexSpringPlayableBehaviour>.Create(_playableGraph, 1);
                        springPlayable.GetBehaviour().Initialize(nodeData.Stiffness, nodeData.Damping);
                        playable = springPlayable;
                        break;
                    case FlexAnimationNodeType.PointerInput:
                        var pointerPlayable = ScriptPlayable<FlexPointerPlayableBehaviour>.Create(_playableGraph);
                        pointerPlayable.GetBehaviour().Initialize(nodeData.PointerMode, nodeData.InteractionRange, transform);
                        playable = pointerPlayable;
                        break;
                    case FlexAnimationNodeType.AudioReactor:
                        var audioPlayable = ScriptPlayable<FlexAudioPlayableBehaviour>.Create(_playableGraph);
                        audioPlayable.GetBehaviour().Initialize(nodeData.AudioBand, nodeData.AudioSensitivity, nodeData.AudioThreshold);
                        playable = audioPlayable;
                        break;
                    case FlexAnimationNodeType.TimeRewind:
                        var rewindPlayable = ScriptPlayable<FlexTimeRewindBehaviour>.Create(_playableGraph, 1);
                        rewindPlayable.GetBehaviour().Initialize(nodeData.BufferSeconds, transform);
                        playable = rewindPlayable;
                        break;
                    case FlexAnimationNodeType.ParticleControl:
                        var particlePlayable = ScriptPlayable<FlexParticlePlayableBehaviour>.Create(_playableGraph);
                        particlePlayable.GetBehaviour().Initialize(nodeData.TargetParticle, nodeData.ParticleField, nodeData.ParticleMin, nodeData.ParticleMax, nodeData.ParticleColor);
                        playable = particlePlayable;
                        break;
                    case FlexAnimationNodeType.Wait:
                        var waitPlayable = ScriptPlayable<FlexWaitPlayableBehaviour>.Create(_playableGraph, 1);
                        waitPlayable.GetBehaviour().Initialize(nodeData.Duration);
                        playable = waitPlayable;
                        break;
                    case FlexAnimationNodeType.Compare:
                        var comparePlayable = ScriptPlayable<FlexComparePlayableBehaviour>.Create(_playableGraph, 1);
                        comparePlayable.GetBehaviour().Initialize(nodeData.Threshold, nodeData.CompareType);
                        playable = comparePlayable;
                        break;
                    case FlexAnimationNodeType.Sequence:
                        var sequencePlayable = ScriptPlayable<FlexSequencePlayableBehaviour>.Create(_playableGraph, 4); // Up to 4 inputs for sequence
                        sequencePlayable.GetBehaviour().Initialize(nodeData.Duration);
                        playable = sequencePlayable;
                        break;
                    case FlexAnimationNodeType.GlobalSet:
                        var gSetPlayable = ScriptPlayable<FlexGlobalSetBehaviour>.Create(_playableGraph, 1);
                        gSetPlayable.GetBehaviour().Initialize(nodeData.GlobalKey);
                        playable = gSetPlayable;
                        break;
                    case FlexAnimationNodeType.GlobalGet:
                        var gGetPlayable = ScriptPlayable<FlexGlobalGetBehaviour>.Create(_playableGraph);
                        gGetPlayable.GetBehaviour().Initialize(nodeData.GlobalKey);
                        playable = gGetPlayable;
                        break;
                    case FlexAnimationNodeType.Math:
                        var mathPlayable = ScriptPlayable<FlexMathPlayableBehaviour>.Create(_playableGraph, 2); // 2 inputs for math
                        mathPlayable.GetBehaviour().Initialize(nodeData.MathOp);
                        playable = mathPlayable;
                        break;
                    case FlexAnimationNodeType.Custom:
                        var customPlayable = ScriptPlayable<FlexCustomNodeBehaviour>.Create(_playableGraph, 1);
                        customPlayable.GetBehaviour().Initialize(nodeData.CustomComponent, nodeData.CustomMethod);
                        playable = customPlayable;
                        break;
                    case FlexAnimationNodeType.Blend:
                    case FlexAnimationNodeType.Lerp:
                        playable = AnimationMixerPlayable.Create(_playableGraph, 2);
                        break;
                    case FlexAnimationNodeType.Output:
                        playable = AnimationMixerPlayable.Create(_playableGraph, 1);
                        break;
                    default:
                        playable = Playable.Null;
                        break;
                }
                playableMap[nodeData.Guid] = playable;
            }

            _playableMap = playableMap;

            foreach (var edge in Graph.Edges)
            {
                if (playableMap.TryGetValue(edge.OutputNodeGuid, out var source) &&
                    playableMap.TryGetValue(edge.InputNodeGuid, out var target))
                {
                    int inputIndex = 0;
                    if (edge.InputPortName == "B") inputIndex = 1;
                    
                    _playableGraph.Connect(source, 0, target, inputIndex);
                    target.SetInputWeight(inputIndex, 1.0f);
                }
            }

            var outputNodeData = Graph.Nodes.FirstOrDefault(x => x.NodeType == FlexAnimationNodeType.Output);
            if (outputNodeData != null && playableMap.TryGetValue(outputNodeData.Guid, out var finalPlayable))
            {
                _playableOutput.SetSourcePlayable(finalPlayable);
            }

            _playableGraph.Play();
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
        }
    }
}
