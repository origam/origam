using System;
using System.Linq;
using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram
{
    public class WorkFlowGraph: Graph
    {
        private readonly string contextStoreSubgraphId = "contextStores";
        private readonly string mainSubgraphId = "mainSubGraph";

        public Subgraph TopSubgraph => RootSubgraph.Subgraphs.FirstOrDefault();
        public Subgraph ContextStoreSubgraph => GetTopSubgraphChild(contextStoreSubgraphId);
        public Subgraph MainDrawingSubgraf => GetTopSubgraphChild(mainSubgraphId);
        
        public bool IsWorkFlowItemSubGraph(Node node)
        {
            if (!(node is Subgraph)) return false;
            if (Equals(node, TopSubgraph)) return false;
            if (Equals(node, ContextStoreSubgraph)) return false;
            if (Equals(node, MainDrawingSubgraf)) return false;
            return true;
        }
        public bool IsInfrastructureSubGraph(Node node)
        {
            if (!(node is Subgraph)) return false;
            if (Equals(node, TopSubgraph)) return true;
            if (Equals(node, ContextStoreSubgraph)) return true;
            return false;
        }
        private Subgraph GetTopSubgraphChild(string childId)
        {
            if (TopSubgraph == null)
                throw new InvalidOperationException("TopSubgraph must be set first");
            var mainDrawingSubraph = TopSubgraph.Subgraphs
                .SingleOrDefault(x => x.Id == childId);
            if (mainDrawingSubraph == null)
            {
                mainDrawingSubraph = new Subgraph(childId);
                TopSubgraph.AddSubgraph(mainDrawingSubraph);
            }

            return mainDrawingSubraph;
        }
    }
}
