using Poly.Common;
using UnityEngine;

namespace Poly.Graph
{
    [CreateAssetMenu(fileName = "NodeData", menuName = "Polyphantom/Graph/New Node Data")]
    public class OPolyNode : ScriptableObject
    {
        [field: SerializeField]
        public FPolyTagReference CategoryTag {get ; private set;}
        
        [field: SerializeField, FPolySubclassPicker(typeof(VPolyNodeBase))]
        public FPolyTypeReference NodeVisualClass {get ; private set;}
        
        [field: SerializeField, FPolySubclassPicker(typeof(FPolyBaseNode))]
        public FPolyTypeReference NodeLogicClass {get; private set;}
        
        [field: Header("Scoring"), SerializeField, Tooltip("How long will the evaluator process this node [s]")]
        public float EvaluationTime {get; private set;}
        [field: SerializeField, Min(0)]
        public int LutValue {get; private set;}
        
        [field: Header("Attributes"), SerializeField, Tooltip("Can the player remove this node from the graph?")]
        public bool IsDeletable { get; private set; } = true;
        
        [field: Header("Attributes"), SerializeField, Tooltip("Does this node has tutorial info?")]
        public bool HasTutorial { get; private set; } = true;
        [field: SerializeField, Tooltip("Is the number of nodes in the graph limited? (0 for no limit)"), Min(0)]
        public int CountPerGraph { get; private set; }
        
        [field: Header("UIData"), SerializeField]
        public Sprite Icon { get; private set; }
        
        [field: SerializeField]
        public Sprite NodeTexture { get; private set; }
        
        [field: SerializeField]
        public Color ColorTint { get; private set; }
        
        [field: SerializeField]
        public Color NodeIconColor { get; private set; }
        
        [field: SerializeField]
        public string NodeName { get; private set; }
        
        [field: SerializeField]
        public string NodeInsideName { get; private set; }
        
        [field: SerializeField]
        public string NodeDesc { get; private set; }
    }
}
