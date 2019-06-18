#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
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