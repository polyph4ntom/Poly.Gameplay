using System.Collections.Generic;
using Poly.Common;
using Poly.Graph;
using Poly.Settings;
using UnityEngine;

namespace Arc.Main
{
    [System.Serializable]
    public class FPolyNodeSet
    {
        [field: SerializeField] 
        public List<OPolyNode> AvailableCheckerNodes { get; private set; }
        
        [field: SerializeField] 
        public List<OPolyNode> AvailableGateNodes { get; private set; }
    }

    [System.Serializable]
    public class PolyGraphSettings
    {
        [field: SerializeField, Header("UIData")] 
        public Texture2D EntryNodesTexture { get; private set; }
        
        [field: SerializeField] 
        public Texture2D CheckerNodesTexture { get; private set; }
        
        [field: SerializeField] 
        public Texture2D GateNodesTexture { get; private set; }
        
        [field: SerializeField, Header("Nodes")]  
        public OPolyNode EntryNode { get; private set; }
        
        [field: SerializeField] 
        public OPolyNode ExitNode { get; private set; }
        
        [field: SerializeField] 
        public FPolySerializableDictionary<FPolySceneReference, FPolyNodeSet> AvailableNodes { get; private set; }
    }
    
    [CreateAssetMenu(menuName = "Polyphantom/Settings/Graph Settings")]
    public class OPolyGraphAsset : OPolyDevSettingsAsset<PolyGraphSettings> { }
}
