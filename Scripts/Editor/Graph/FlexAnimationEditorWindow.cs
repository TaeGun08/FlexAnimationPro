using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FlexAnimation;

namespace FlexAnimation.Editor
{
    public class FlexAnimationEditorWindow : EditorWindow
    {
        private FlexAnimationGraphView _graphView;
        private FlexAnimationGraph _currentGraph;

        [MenuItem("Window/Flex Animation Editor")]
        public static void Open()
        {
            var window = GetWindow<FlexAnimationEditorWindow>("Flex Animation Editor");
            window.Show();
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new FlexAnimationGraphView(this)
            {
                name = "Flex Animation Graph"
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var assetField = new ObjectField("Graph Asset")
            {
                objectType = typeof(FlexAnimationGraph),
                allowSceneObjects = false
            };
            assetField.RegisterValueChangedCallback(evt =>
            {
                _currentGraph = evt.newValue as FlexAnimationGraph;
                if (_currentGraph != null)
                {
                    _graphView.Load(_currentGraph);
                }
            });
            toolbar.Add(assetField);

            toolbar.Add(new Button(() => _graphView.Save()) { text = "Save" });

            rootVisualElement.Add(toolbar);
        }
    }
}
