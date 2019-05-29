using Microsoft.Msagl.Drawing;

namespace Origam.Workbench.Diagram.Graphs
{
    public class InfrastructureSubgraph : Subgraph, IWorkflowSubgraph
    {
        private readonly BlockSubGraph parent;
        public InfrastructureSubgraph(string id, BlockSubGraph parent) : base(id)
        {
            this.parent = parent;
            LabelText = "";
            DrawNodeDelegate = (node, graphics) => true;
        }

        public string WorkflowItemId => parent.WorkflowItemId;
    }
}