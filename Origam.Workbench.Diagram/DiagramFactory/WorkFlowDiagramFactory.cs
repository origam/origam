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
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using MoreLinq.Extensions;
using Origam.Schema;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Diagram.NodeDrawing;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram
{
	public class WorkFlowDiagramFactory : IDiagramFactory<IWorkflowBlock, WorkFlowGraph>
	{
		private List<string> expandedSubgraphNodeIds = new List<string>();
		private WorkFlowGraph graph;
		private readonly NodeFactory nodeFactory;

		public WorkFlowDiagramFactory(INodeSelector nodeSelector,
			GViewer gViewer)
		{
			nodeFactory = new NodeFactory(nodeSelector, gViewer);
		}

		public WorkFlowGraph Draw(IWorkflowBlock graphParent)
		{
			return Draw(graphParent, new List<string>());
		}

		public WorkFlowGraph Draw(IWorkflowBlock graphParent, List<string> expandedSubgraphNodeIds)
		{
			this.expandedSubgraphNodeIds = expandedSubgraphNodeIds;
			graph = new WorkFlowGraph();
			nodeFactory.AddSubgraph(graph.RootSubgraph, graphParent);
			AddToSubgraph(graphParent, graph.MainDrawingSubgraf);

			AddContextStores(graphParent, graph.TopSubgraph);
			AddBalloons();
			return graph;
		}

		private void AddContextStores(IWorkflowBlock block, BlockSubGraph blockSubGraph)
		{
			foreach (var childItem in block.ChildItems)
			{
				if (childItem is ContextStore contextStore)
				{
					Node node = nodeFactory.AddNode(graph, contextStore);
					blockSubGraph.ContextStoreSubgraph.AddNode(node);
				}
			}
		}

		private Subgraph AddSubgraphNode(IWorkflowStep step, Subgraph subGraph)
		{
			Subgraph subgraphNode = nodeFactory.AddSubgraphNode(subGraph, step);
            subgraphNode.UserData = step;
            if (expandedSubgraphNodeIds.Contains(subgraphNode.Id))
            {
	            AddNodeItems(step, subgraphNode);
            }

            subgraphNode.LayoutSettings = new SugiyamaLayoutSettings
            {
	            PackingMethod = PackingMethod.Columns,
	            PackingAspectRatio = 0.01,
	            AdditionalClusterTopMargin = 30,
	            ClusterMargin = 1
            };

            return subgraphNode;
		}

		private void AddNodeItems(IWorkflowStep step, Subgraph subgraphNode)
		{
			step.ChildItems.ToEnumerable()
				.Where(x => !(x is WorkflowTaskDependency))
				.OrderByDescending(x => x.Name)
				.ForEach(stepChild =>
				{
					stepChild.ChildItems.ToEnumerable()
						.OrderByDescending(x => x.Name)
						.ForEach(innerChild =>
						{
							AddNodeItem(innerChild, subgraphNode, 30);
						});
					AddNodeItem(stepChild, subgraphNode, 15);
				});
		}

		private void AddNodeItem(AbstractSchemaItem item, Subgraph subGraph, int leftMargin)
		{
			Node node = nodeFactory.AddNodeItem(graph, item, leftMargin);
			node.UserData = item;
			subGraph.AddNode(node);
		}

		private Subgraph AddWorkflowDiagram(IWorkflowBlock workFlowBlock, Subgraph parentSubgraph)
		{
			BlockSubGraph subgraph = nodeFactory.AddSubgraph(parentSubgraph, workFlowBlock);
			AddContextStores(workFlowBlock, subgraph);
			return AddToSubgraph(workFlowBlock, subgraph);
		}

		private Subgraph AddToSubgraph(IWorkflowBlock workFlowBlock, Subgraph subgraph)
		{
			IDictionary<Key, Node> ht = new Dictionary<Key, Node>();

			foreach (IWorkflowStep step in workFlowBlock.ChildItemsByType(
				AbstractWorkflowStep.ItemTypeConst))
			{
				if (step is IWorkflowBlock subBlock)
				{
					Node shape = AddWorkflowDiagram(subBlock, subgraph);
					ht.Add(step.PrimaryKey, shape);
				}
				else
				{
					Subgraph shape = AddSubgraphNode(step, subgraph);
					ht.Add(step.PrimaryKey, shape);
				}
			}

			// add connections
			foreach (IWorkflowStep step in workFlowBlock.ChildItemsByType(
				AbstractWorkflowStep.ItemTypeConst))
			{
				Node destinationShape = ht[step.PrimaryKey];
				if (destinationShape == null)
					throw new NullReferenceException(
						ResourceUtils.GetString("ErrorDestinationShapeNotFound"));
				int i = 0;
				foreach (WorkflowTaskDependency dependency in step.ChildItemsByType(
					WorkflowTaskDependency.ItemTypeConst))
				{
					Node sourceShape = ht[dependency.Task.PrimaryKey];
					if (sourceShape == null)
						throw new NullReferenceException(
							ResourceUtils.GetString("ErrorSourceShapeNotFound"));

					Edge edge = graph.AddEdge(sourceShape.Id, destinationShape.Id);
					edge.UserData = dependency;
					i++;
				}
			}

			return subgraph;
		}

		private void AddBalloons()
		{
			foreach (var subgraph in graph.MainDrawingSubgraf.Subgraphs)
			{
				if (!subgraph.InEdges.Any())
				{
					Node startBalloon = nodeFactory.AddStarBalloon(graph);
					graph.MainDrawingSubgraf.AddNode(startBalloon);
					graph.AddEdge(startBalloon.Id, subgraph.Id);
				}
				if (!subgraph.OutEdges.Any())
				{
					Node endBalloon = nodeFactory.AddEndBalloon(graph);
					graph.MainDrawingSubgraf.AddNode(endBalloon);
					graph.AddEdge(subgraph.Id, endBalloon.Id);
				}
			}
		}
	}
}
