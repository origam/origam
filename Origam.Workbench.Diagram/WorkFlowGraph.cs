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
            get => ContextStoreSubgraph.Subgraphs.SingleOrDefault(x=>x.Id == contextStoreSubgraphId);
        }

        public Subgraph MainDrawingSubgraf
        {
            get => ContextStoreSubgraph.Subgraphs.SingleOrDefault(x=>x.Id != contextStoreSubgraphId);
            set => TopSubgraph.AddSubgraph(value);
        }

        public WorkFlowGraph()
        {
            RootSubgraph.AddSubgraph(new Subgraph(topSubgraphId));
            TopSubgraph.AddSubgraph(new Subgraph(contextStoreSubgraphId));
        }
    }
}