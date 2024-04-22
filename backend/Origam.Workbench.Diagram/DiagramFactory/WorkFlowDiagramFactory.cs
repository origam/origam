#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using MoreLinq;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Diagram.NodeDrawing;
using Origam.Workbench.Services;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram;

public class WorkFlowDiagramFactory : IDiagramFactory<IWorkflowBlock, WorkFlowGraph>
{
	private readonly INodeSelector nodeSelector;
	private readonly GViewer gViewer;
	private readonly WorkbenchSchemaService schemaService;
	private List<string> expandedSubgraphNodeIds = new List<string>();
	private WorkFlowGraph graph;
	private  NodeFactory nodeFactory;

	public WorkFlowDiagramFactory(INodeSelector nodeSelector,
		GViewer gViewer, WorkbenchSchemaService schemaService)
	{
			this.nodeSelector = nodeSelector;
			this.gViewer = gViewer;
			this.schemaService = schemaService;
		}

	public WorkFlowGraph Draw(IWorkflowBlock graphParent)
	{
			return Draw(graphParent, new List<string>());
		}

	public WorkFlowGraph Draw(IWorkflowBlock graphParent, List<string> expandedSubgraphNodeIds)
	{
			this.expandedSubgraphNodeIds = expandedSubgraphNodeIds;
			graph = new WorkFlowGraph();
			nodeFactory = new NodeFactory(nodeSelector, gViewer, schemaService, graph);
			nodeFactory.AddSubgraph(graph.RootSubgraph, graphParent);
			graph.MainDrawingSubgraf.LayoutSettings = new SugiyamaLayoutSettings
			{
				PackingMethod = PackingMethod.Columns,
				PackingAspectRatio = 1000,
				SelfMarginsOverride = new Margins{Right = 0.1}
			};
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
					Node node = nodeFactory.AddNode(contextStore);
					blockSubGraph.AddContextStore(node);
				}
			}
		}

	private Subgraph AddSubgraphNode(IWorkflowStep step, Subgraph subGraph)
	{
			Subgraph subgraphNode = nodeFactory.AddSubgraphNode(subGraph, step);
			if (expandedSubgraphNodeIds.Contains(subgraphNode.Id))
            {
	            AddNodeItems(step, subgraphNode);
            }
			AddActionNodes(step, subgraphNode);

            subgraphNode.LayoutSettings = new SugiyamaLayoutSettings
            {
	            PackingMethod = PackingMethod.Columns,
	            PackingAspectRatio = 0.01,
	            AdditionalClusterTopMargin = 30,
	            ClusterMargin = 1
            };

            return subgraphNode;
		}

	private void AddActionNodes(IWorkflowStep step, Subgraph subgraphNode)
	{
			if (!(step is UIFormTask formTask)) return;
			
			foreach (DataStructureEntity entity in formTask.Screen.DataStructure
				.Entities)
			{
				var actions = GetActions(entity, formTask.ScreenId);
				if (actions.Length == 0) continue;
				var actionSubgraph =
					nodeFactory.AddActionSubgraph(subgraphNode, entity);
				foreach (var action in actions)
				{
					nodeFactory.AddActionNode(actionSubgraph, action);
				}
			} 
			
			AddNodeItem(subgraphNode, new NodeItemLabel("", 5));
		}

	private EntityUIAction[] GetActions(DataStructureEntity entity,
		Guid screenId)
	{
			var actions = entity.Entity.ChildItems
				.ToGeneric()
				.OfType<EntityUIAction>()
				.Where(action => ShouldBeShownOnScreen(action, screenId))
				.ToArray();

			var entityDropdownActions = actions
				.OfType<EntityDropdownAction>()
				.ToArray();

			if (entityDropdownActions.Length > 0)
			{
				var actionsFromDropDowns = entityDropdownActions
					.SelectMany(dropDown => dropDown.ChildItems.ToGeneric())
					.Cast<EntityUIAction>();
				actions = actions
					.Except(entityDropdownActions)
					.Concat(actionsFromDropDowns)
					.Where(action => ShouldBeShownOnScreen(action, screenId))
					.ToArray();
			}

			return actions;
		}

	private bool ShouldBeShownOnScreen(EntityUIAction action, Guid screenId)
	{
			return !action.ScreenIds.Any()|| action.ScreenIds.Contains(screenId);
		}

	private void AddNodeItems(IWorkflowStep step, Subgraph subgraphNode)
	{
			step.ChildItems.ToGeneric()
				.Where(x => !(x is WorkflowTaskDependency))
				.OrderByDescending(x => x.Name)
				.ForEach(stepChild =>
				{
					stepChild.ChildItems.ToGeneric()
						.OrderByDescending(x => x.Name)
						.ForEach(innerChild =>
						{
							AddNodeItem(innerChild, subgraphNode, 30);
						});
					AddNodeItem(stepChild, subgraphNode, 15);
				});
		}

	private void AddNodeItem(ISchemaItem item, Subgraph subGraph, int leftMargin)
	{
			var nodeData = new NodeItemData(item, leftMargin, schemaService);
			Node node = nodeFactory.AddNodeItem(nodeData);
			subGraph.AddNode(node);
		}
		
	private void AddNodeItem(Subgraph subGraph, INodeData nodeData)
	{
			Node node = nodeFactory.AddNodeItem(nodeData);
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
			IDictionary<Key, Node> nodes = new Dictionary<Key, Node>();

			foreach (IWorkflowStep step in workFlowBlock.ChildItemsByType(
				AbstractWorkflowStep.CategoryConst))
			{
				Node shape = step is IWorkflowBlock subBlock
					? AddWorkflowDiagram(subBlock, subgraph)
					: AddSubgraphNode(step, subgraph);
				nodes.Add(step.PrimaryKey, shape);
			}

			// add connections
			foreach (IWorkflowStep step in workFlowBlock.ChildItemsByType(
				AbstractWorkflowStep.CategoryConst))
			{
				Node destinationShape = nodes[step.PrimaryKey];
				if (destinationShape == null)
					throw new NullReferenceException(Strings.WorkFlowDiagramFactory_DestinationShape_not_found);
				int i = 0;
				foreach (WorkflowTaskDependency dependency in step.ChildItemsByType(
					WorkflowTaskDependency.CategoryConst))
				{
					Node sourceShape = nodes[dependency.Task.PrimaryKey];
					if (sourceShape == null)
						throw new NullReferenceException(Strings.WorkFlowDiagramFactory_SourceShape_not_found);

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
					Node startBalloon = nodeFactory.AddStarBalloon();
					graph.MainDrawingSubgraf.AddNode(startBalloon);
					graph.AddEdge(startBalloon.Id, subgraph.Id);
				}
				if (!subgraph.OutEdges.Any())
				{
					Node endBalloon = nodeFactory.AddEndBalloon();
					graph.MainDrawingSubgraf.AddNode(endBalloon);
					graph.AddEdge(subgraph.Id, endBalloon.Id);
				}
			}
		}
}