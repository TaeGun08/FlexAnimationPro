using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using FlexAnimation;

namespace FlexAnimation.Editor
{
    public class FlexAnimationGraphView : GraphView
    {
        public FlexAnimationGraph GraphData;
        private FlexAnimationSearchWindow _searchWindow;
        public EditorWindow window;

        public FlexAnimationGraphView(EditorWindow editorWindow)
        {
            window = editorWindow;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();

            // Start live update loop
            schedule.Execute(UpdateRuntimeVisuals).Every(50);
        }

        private void UpdateRuntimeVisuals()
        {
            if (!Application.isPlaying) return;

            var activeGO = UnityEditor.Selection.activeGameObject;
            if (activeGO == null) return;

            var player = activeGO.GetComponent<FlexAnimationPlayer>();
            if (player == null) return;

            foreach (var node in nodes.ToList().Cast<FlexAnimationNode>())
            {
                float val = player.GetRuntimeValue(node.Guid);
                node.UpdateRuntimeValue(val);
            }

            foreach (var edge in edges.ToList())
            {
                if (edge.output.node is FlexAnimationNode sourceNode)
                {
                    float val = player.GetRuntimeValue(sourceNode.Guid);
                    edge.edgeControl.inputColor = Color.Lerp(new Color(0.3f, 0.3f, 0.3f, 0.5f), new Color(0.2f, 1f, 1f, 1f), val);
                    edge.edgeControl.outputColor = edge.edgeControl.inputColor;
                }
            }
        }

        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<FlexAnimationSearchWindow>();
            _searchWindow.Init(this);

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void CreateNode(string title, FlexAnimationNodeType type, Vector2 position)
        {
            var data = new FlexAnimationNodeData
            {
                Guid = System.Guid.NewGuid().ToString(),
                Title = title,
                NodeType = type,
                Position = position
            };

            var node = new FlexAnimationNode(data);
            AddElement(node);
        }

        public void Save()
        {
            if (GraphData == null) return;

            GraphData.Nodes.Clear();
            GraphData.Edges.Clear();

            var nodes = this.nodes.ToList().Cast<FlexAnimationNode>();
            foreach (var node in nodes)
            {
                node.Data.Position = node.GetPosition().position;
                GraphData.Nodes.Add(node.Data);
            }

            var edges = this.edges.ToList();
            foreach (var edge in edges)
            {
                var outputNode = edge.output.node as FlexAnimationNode;
                var inputNode = edge.input.node as FlexAnimationNode;

                GraphData.Edges.Add(new FlexAnimationEdgeData
                {
                    OutputNodeGuid = outputNode.Guid,
                    OutputPortName = edge.output.portName,
                    InputNodeGuid = inputNode.Guid,
                    InputPortName = edge.input.portName
                });
            }

            EditorUtility.SetDirty(GraphData);
            AssetDatabase.SaveAssets();
        }

        public void Load(FlexAnimationGraph graph)
        {
            GraphData = graph;
            DeleteElements(graphElements.ToList());

            foreach (var nodeData in graph.Nodes)
            {
                var node = new FlexAnimationNode(nodeData);
                AddElement(node);
            }

            var nodes = this.nodes.ToList().Cast<FlexAnimationNode>();
            foreach (var edgeData in graph.Edges)
            {
                var outputNode = nodes.FirstOrDefault(x => x.Guid == edgeData.OutputNodeGuid);
                var inputNode = nodes.FirstOrDefault(x => x.Guid == edgeData.InputNodeGuid);

                if (outputNode != null && inputNode != null)
                {
                    var outputPort = outputNode.outputContainer.Query<Port>().Where(x => x.portName == edgeData.OutputPortName).First();
                    var inputPort = inputNode.inputContainer.Query<Port>().Where(x => x.portName == edgeData.InputPortName).First();

                    var edge = outputPort.ConnectTo(inputPort);
                    AddElement(edge);
                }
            }
        }
    }
}
