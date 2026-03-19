using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using FlexAnimation;

namespace FlexAnimation.Editor
{
    public class FlexAnimationSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private FlexAnimationGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Init(FlexAnimationGraphView graphView)
        {
            _graphView = graphView;
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Animation"), 1),
                new SearchTreeEntry(new GUIContent("Clip Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Clip
                },
                new SearchTreeEntry(new GUIContent("Flex Preset Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.FlexPreset
                },
                new SearchTreeEntry(new GUIContent("Blend Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Blend
                },
                new SearchTreeGroupEntry(new GUIContent("Logic & Flow"), 1),
                new SearchTreeEntry(new GUIContent("Wait Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Wait
                },
                new SearchTreeEntry(new GUIContent("Compare Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Compare
                },
                new SearchTreeEntry(new GUIContent("Sequence Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Sequence
                },
                new SearchTreeGroupEntry(new GUIContent("Global Sync"), 1),
                new SearchTreeEntry(new GUIContent("Global Set", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.GlobalSet
                },
                new SearchTreeEntry(new GUIContent("Global Get", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.GlobalGet
                },
                new SearchTreeGroupEntry(new GUIContent("Modular Hub"), 1),
                new SearchTreeEntry(new GUIContent("Math Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Math
                },
                new SearchTreeEntry(new GUIContent("Custom Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Custom
                },
                new SearchTreeEntry(new GUIContent("Lerp Node", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Lerp
                },
                new SearchTreeGroupEntry(new GUIContent("Procedural"), 1),
                new SearchTreeEntry(new GUIContent("Organic Noise", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.OrganicNoise
                },
                new SearchTreeEntry(new GUIContent("Time Rewind", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.TimeRewind
                },
                new SearchTreeGroupEntry(new GUIContent("Shader"), 1),
                new SearchTreeEntry(new GUIContent("Shader Property", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.ShaderProperty
                },
                new SearchTreeEntry(new GUIContent("Shader Effect", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.ShaderEffect
                },
                new SearchTreeGroupEntry(new GUIContent("Input & Sensors"), 1),
                new SearchTreeEntry(new GUIContent("Pointer Input", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.PointerInput
                },
                new SearchTreeEntry(new GUIContent("Audio Reactor", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.AudioReactor
                },
                new SearchTreeGroupEntry(new GUIContent("VFX"), 1),
                new SearchTreeEntry(new GUIContent("Particle Control", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.ParticleControl
                },
                new SearchTreeGroupEntry(new GUIContent("Physics"), 1),
                new SearchTreeEntry(new GUIContent("Spring Physics", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Spring
                },
                new SearchTreeGroupEntry(new GUIContent("Output"), 1),
                new SearchTreeEntry(new GUIContent("Final Result", _indentationIcon))
                {
                    level = 2,
                    userData = FlexAnimationNodeType.Output
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var mousePosition = _graphView.ChangeCoordinatesTo(_graphView, context.screenMousePosition - _graphView.window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            var type = (FlexAnimationNodeType)searchTreeEntry.userData;
            _graphView.CreateNode(searchTreeEntry.name, type, graphMousePosition);
            return true;
        }
    }
}
