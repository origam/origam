using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.Graphs
{
    public class WorkFlowGraph: Graph
    {
        public BlockSubGraph TopSubgraph => (BlockSubGraph)RootSubgraph.Subgraphs.FirstOrDefault();
        public InfrastructureSubgraph ContextStoreSubgraph => TopSubgraph.ContextStoreSubgraph;
        public InfrastructureSubgraph MainDrawingSubgraf => TopSubgraph.MainDrawingSubgraf;

        public IEnumerable<InfrastructureSubgraph> AllContextStoreSubgraphs =>
            RootSubgraph
                .GetAllSubgraphs()
                .OfType<BlockSubGraph>()
                .Select(x => x.ContextStoreSubgraph);
        
        
        public bool IsWorkFlowItemSubGraph(Node node)
        {
            if (node == null) return false;
            if (!(node is Subgraph)) return false;
            if (node is IWorkflowSubgraph) return false;
            return true;
        }
    }
}
