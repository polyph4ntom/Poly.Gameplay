using System;
using Poly.Common;
using Poly.Log;

namespace Poly.Graph
{
    public enum EPolyNodeEvalResult
    {
        Unknown,
        True,
        False
    }

    public abstract class FPolyBaseNode
    {
        protected readonly FPolyBaseNode[] connectedNodes = new FPolyBaseNode[2];

        private object storedData;
        private int lutValue;
        private float evaluationTime;

        public void Initialize(int nodeLutValue, float nodeEvaluationTime)
        {
            this.lutValue = nodeLutValue;
            this.evaluationTime = nodeEvaluationTime;
        }

        public void SetValue(object value)
        {
            this.storedData = value;
        }

        public T GetValue<T>()
        {
            return (T) storedData;
        }

        public virtual void ConnectNode(int idx, FPolyBaseNode nodeToConnect)
        {
            if (idx < 0 || idx >= connectedNodes.Length)
            {
                FPolyLog.Error("Poly.Graph", $"Node {idx} is out of range.");
                return;
            }
            
            connectedNodes[idx] = nodeToConnect;
        }

        public virtual void DisconnectNode(int idx)
        {
            if (idx < 0 || idx >= connectedNodes.Length)
            {
                FPolyLog.Error("Poly.Graph", $"Node {idx} is out of range.");
                return;
            }

            connectedNodes[idx] = null;
        }
        
        public abstract EPolyNodeEvalResult Eval();

        protected static bool EvalToBool(EPolyNodeEvalResult result)
        {
            return result switch
            {
                EPolyNodeEvalResult.True => true,
                EPolyNodeEvalResult.False => false,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
    }
}