using System.Linq;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;

namespace Origam.Workbench.Diagram.Graphs
{
    public class BlockSubGraph : Subgraph, IWorkflowSubgraph
    {
        private readonly string contextStoreSubgraphId;
        private readonly string mainSubgraphId;

        public bool IsEmpty =>
            !ContextStoreSubgraph.Nodes.Any() &&
            !MainDrawingSubgraf.Nodes.Any() &&
            !MainDrawingSubgraf.Subgraphs.Any();

        public string WorkflowItemId => Id;
        public BlockSubGraph(string id) : base(id)
        {
            contextStoreSubgraphId = "contextStores_"+id;
            mainSubgraphId = "mainSubGraph_"+id;
            LayoutSettings = new SugiyamaLayoutSettings
            {
                PackingAspectRatio = 1000,
                AdditionalClusterTopMargin = 30,
                ClusterMargin = 20,
                PackingMethod = PackingMethod.CompactTop
            };
        }

        private Subgraph GetTopSubgraphChild(string childId)
        {
            var child = Subgraphs.SingleOrDefault(x => x.Id == childId);
            if (child == null)
            {
                child = new InfrastructureSubgraph(childId, this);
                AddSubgraph(child);
            }

            return child;
        }

        public InfrastructureSubgraph ContextStoreSubgraph
        {
            get
            {
                InfrastructureSubgraph child = Subgraphs
                    .OfType<InfrastructureSubgraph>()
                    .SingleOrDefault(x => x.Id == contextStoreSubgraphId);
                if (child == null)
                {
                    child = new InfrastructureSubgraph(contextStoreSubgraphId, this);
                    child.LayoutSettings = new SugiyamaLayoutSettings
                    {
                        ClusterMargin = 20
                    };
                    AddSubgraph(child);
                }

                return child;
            }
        }

        public InfrastructureSubgraph MainDrawingSubgraf =>
            (InfrastructureSubgraph)GetTopSubgraphChild(mainSubgraphId);
    }
}