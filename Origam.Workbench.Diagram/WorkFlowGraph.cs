using System.Linq;
using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram
{
    public class WorkFlowGraph: Graph
    {
        private readonly string contextStoreSubgraphId = "contextStores";
        private readonly string topSubgraphId = "topSubGraph";

        public Subgraph TopSubgraph => RootSubgraph.Subgraphs.FirstOrDefault();

        public Subgraph ContextStoreSubgraph
        {
            get => TopSubgraph.Subgraphs.SingleOrDefault(x=>x.Id == contextStoreSubgraphId);
        }

        public Subgraph MainDrawingSubgraf
        {
            get => TopSubgraph.Subgraphs.SingleOrDefault(x=>x.Id != contextStoreSubgraphId);
            set => TopSubgraph.AddSubgraph(value);
        }

        public WorkFlowGraph()
        {
            RootSubgraph.AddSubgraph(new Subgraph(topSubgraphId));
            TopSubgraph.AddSubgraph(new Subgraph(contextStoreSubgraphId));
        }

        public bool IsWorkFlowItemSubGraph(Node node)
        {
            if (!(node is Subgraph)) return false;
            if (node == TopSubgraph) return false;
            if (node == ContextStoreSubgraph) return false;
            if (node == MainDrawingSubgraf) return false;
            return true;
        }
        public bool IsInfrastructureSubGraph(Node node)
        {
            if (!(node is Subgraph)) return false;
            if (node == TopSubgraph) return true;
            if (node == ContextStoreSubgraph) return true;
            return false;
        }
    }
}