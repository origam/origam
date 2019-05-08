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

using System;
using Origam.Schema.WorkflowModel;
using Microsoft.Msagl.Drawing;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.Layered;
using MoreLinq.Extensions;
using Origam.Schema;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.NodeDrawing;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram
{
	public class WorkFlowDiagramFactory : IDiagramFactory<IWorkflowBlock, WorkFlowGraph>
	{
		private static readonly int nodeMargin = 40;
		
		private WorkFlowGraph graph;
		private readonly NodeFactory nodeFactory;

		public WorkFlowDiagramFactory(INodeSelector nodeSelector)
		{
			nodeFactory = new NodeFactory(nodeSelector);
		}

		public WorkFlowGraph Draw(IWorkflowBlock graphParent)
		{
			graph = new WorkFlowGraph();
			graph.TopSubgraph.DrawNodeDelegate =  (node, graphics) => true;
			AddWorkflowDiagram(graphParent, graph.TopSubgraph);
			AddContextStores(graphParent);
			graph.LayoutAlgorithmSettings.ClusterMargin = nodeMargin;
			return graph;
		}

		public void AlignContextStoreSubgraph()
		{
			if(graph.ContextStoreSubgraph == null || graph.MainDrawingSubgraf == null)
			{
				throw new InvalidOperationException();
			}

			MoveSubgraphRight(graph.ContextStoreSubgraph, graph.MainDrawingSubgraf);
		}

		private void MoveSubgraphRight(Subgraph subgraphToMove, Subgraph refSubgraph) {
            
			double dx = refSubgraph.Pos.X - subgraphToMove.Pos.X  +
			            refSubgraph.Width / 2 + subgraphToMove.Width / 2;
            
			double dy = refSubgraph.Pos.Y - subgraphToMove.Pos.Y  +
			            refSubgraph.Height / 2 - subgraphToMove.Height / 2;
            
			subgraphToMove.GeometryNode.Center = new Point(subgraphToMove.Pos.X + dx, subgraphToMove.Pos.Y + dy);
			((Cluster)subgraphToMove.GeometryNode).RectangularBoundary.Rect = 
				new Microsoft.Msagl.Core.Geometry.Rectangle(subgraphToMove.GeometryNode.BoundingBox.Size, subgraphToMove.Pos); 
			foreach (var node in subgraphToMove.Nodes)
			{
				node.GeometryNode.Center = new Point(node.Pos.X + dx, node.Pos.Y+ dy);
			}
		}

		private void AddContextStores(IWorkflowBlock graphParent)
		{
			graph.ContextStoreSubgraph.DrawNodeDelegate = (node, graphics) => true;
			foreach (var childItem in graphParent.ChildItems)
			{
				if (childItem is ContextStore contextStore)
				{
					Node node = nodeFactory.AddNode(graph, contextStore);
					graph.ContextStoreSubgraph.AddNode(node);
				}
			}
		}

		private Subgraph AddSubgraphNode(IWorkflowStep step, Subgraph subGraph)
		{
			Subgraph subgraphNode = nodeFactory.AddSubgraphNode(subGraph, step);
            subgraphNode.UserData = step;
            step.ChildItems.ToEnumerable()
	            .SelectMany(GetItemWithChildren)
	            .ForEach(item => AddNodeItem(item, subgraphNode));
            
            subgraphNode.LayoutSettings = new SugiyamaLayoutSettings();
            subgraphNode.LayoutSettings.PackingMethod = PackingMethod.Columns;
            subgraphNode.LayoutSettings.PackingAspectRatio = 0.01;
            subgraphNode.LayoutSettings.ClusterTopMargin = 30;
            subgraphNode.LayoutSettings.ClusterMargin = 1;
            
            return subgraphNode;
		}

		private IEnumerable<AbstractSchemaItem> GetItemWithChildren(AbstractSchemaItem item)
		{
			yield return item;
			foreach (AbstractSchemaItem childItem in item.ChildItems)
			{
				yield return childItem;
			}
		}

		private void AddNodeItem(AbstractSchemaItem item, Subgraph subGraph)
		{
			Node node = nodeFactory.AddNodeItem(graph, item);
			node.UserData = item;
			subGraph.AddNode(node);
		}

		private Subgraph AddWorkflowDiagram(IWorkflowBlock workFlowBlock, Subgraph parentSubgraph)
		{
			Subgraph subgraph = nodeFactory.AddSubgraph(parentSubgraph, workFlowBlock);

			IDictionary<Key, Node> ht = new Dictionary<Key, Node>();

			foreach(IWorkflowStep step in workFlowBlock.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
				if (!(step is IWorkflowBlock subBlock))
                {
                    Subgraph shape = AddSubgraphNode(step, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
                else
                {
                    Node shape = AddWorkflowDiagram(subBlock, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
			}

			// add connections
			foreach(IWorkflowStep step in workFlowBlock.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
				Node destinationShape = ht[step.PrimaryKey];
				if(destinationShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorDestinationShapeNotFound"));
				int i = 0;
				foreach(WorkflowTaskDependency dependency in step.ChildItemsByType(WorkflowTaskDependency.ItemTypeConst))
				{
					Node sourceShape = ht[dependency.Task.PrimaryKey];
					if(sourceShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorSourceShapeNotFound"));

					Edge edge = graph.AddEdge(sourceShape.Id,destinationShape.Id);
					edge.UserData = dependency;
					i++;
				}
			}
            return subgraph;
        }
	}
}
