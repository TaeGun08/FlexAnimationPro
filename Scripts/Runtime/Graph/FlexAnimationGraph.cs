using System.Collections.Generic;
using UnityEngine;

namespace FlexAnimation
{
    [CreateAssetMenu(fileName = "New FlexAnimation Graph", menuName = "FlexAnimation/Graph")]
    public class FlexAnimationGraph : ScriptableObject
    {
        public List<FlexAnimationNodeData> Nodes = new List<FlexAnimationNodeData>();
        public List<FlexAnimationEdgeData> Edges = new List<FlexAnimationEdgeData>();
    }
}
