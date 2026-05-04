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
    private NodeFactory nodeFactory;

    public WorkFlowDiagramFactory(
        INodeSelector nodeSelector,
        GViewer gViewer,
        WorkbenchSchemaService schemaService
    )
    {
        this.nodeSelector = nodeSelector;
        this.gViewer = gViewer;
        this.schemaService = schemaService;
    }

    public WorkFlowGraph Draw(IWorkflowBlock graphParent)
    {
        return Draw(graphParent: graphParent, expandedSubgraphNodeIds: new List<string>());
    }

    public WorkFlowGraph Draw(IWorkflowBlock graphParent, List<string> expandedSubgraphNodeIds)
    {
        this.expandedSubgraphNodeIds = expandedSubgraphNodeIds;
        graph = new WorkFlowGraph();
        nodeFactory = new NodeFactory(
            nodeSelector: nodeSelector,
            gViewer: gViewer,
            schemaService: schemaService,
            graph: graph
        );
        nodeFactory.AddSubgraph(parentSbubgraph: graph.RootSubgraph, schemaItem: graphParent);
        graph.MainDrawingSubgraf.LayoutSettings = new SugiyamaLayoutSettings
        {
            PackingMethod = PackingMethod.Columns,
            PackingAspectRatio = 1000,
            SelfMarginsOverride = new Margins { Right = 0.1 },
        };
        AddToSubgraph(workFlowBlock: graphParent, subgraph: graph.MainDrawingSubgraf);
        AddContextStores(block: graphParent, blockSubGraph: graph.TopSubgraph);
        AddBalloons();
        return graph;
    }

    private void AddContextStores(IWorkflowBlock block, BlockSubGraph blockSubGraph)
    {
        foreach (var childItem in block.ChildItems)
        {
            if (childItem is ContextStore contextStore)
            {
                Node node = nodeFactory.AddNode(schemaItem: contextStore);
                blockSubGraph.AddContextStore(node: node);
            }
        }
    }

    private Subgraph AddSubgraphNode(IWorkflowStep step, Subgraph subGraph)
    {
        Subgraph subgraphNode = nodeFactory.AddSubgraphNode(
            parentSbubgraph: subGraph,
            schemaItem: step
        );
        if (expandedSubgraphNodeIds.Contains(item: subgraphNode.Id))
        {
            AddNodeItems(step: step, subgraphNode: subgraphNode);
        }
        AddActionNodes(step: step, subgraphNode: subgraphNode);
        subgraphNode.LayoutSettings = new SugiyamaLayoutSettings
        {
            PackingMethod = PackingMethod.Columns,
            PackingAspectRatio = 0.01,
            AdditionalClusterTopMargin = 30,
            ClusterMargin = 1,
        };
        return subgraphNode;
    }

    private void AddActionNodes(IWorkflowStep step, Subgraph subgraphNode)
    {
        if (!(step is UIFormTask formTask))
        {
            return;
        }

        foreach (DataStructureEntity entity in formTask.Screen.DataStructure.Entities)
        {
            var actions = GetActions(entity: entity, screenId: formTask.ScreenId);
            if (actions.Length == 0)
            {
                continue;
            }

            var actionSubgraph = nodeFactory.AddActionSubgraph(
                parentSbubgraph: subgraphNode,
                schemaItem: entity
            );
            foreach (var action in actions)
            {
                nodeFactory.AddActionNode(actionSubgraph: actionSubgraph, action: action);
            }
        }

        AddNodeItem(subGraph: subgraphNode, nodeData: new NodeItemLabel(text: "", leftMargin: 5));
    }

    private EntityUIAction[] GetActions(DataStructureEntity entity, Guid screenId)
    {
        var actions = entity
            .Entity.ChildItems.OfType<EntityUIAction>()
            .Where(predicate: action => ShouldBeShownOnScreen(action: action, screenId: screenId))
            .ToArray();
        var entityDropdownActions = actions.OfType<EntityDropdownAction>().ToArray();
        if (entityDropdownActions.Length > 0)
        {
            var actionsFromDropDowns = entityDropdownActions
                .SelectMany(selector: dropDown => dropDown.ChildItems)
                .Cast<EntityUIAction>();
            actions = actions
                .Except(second: entityDropdownActions)
                .Concat(second: actionsFromDropDowns)
                .Where(predicate: action =>
                    ShouldBeShownOnScreen(action: action, screenId: screenId)
                )
                .ToArray();
        }
        return actions;
    }

    private bool ShouldBeShownOnScreen(EntityUIAction action, Guid screenId)
    {
        return !action.ScreenIds.Any() || action.ScreenIds.Contains(value: screenId);
    }

    private void AddNodeItems(IWorkflowStep step, Subgraph subgraphNode)
    {
        step.ChildItems.Where(predicate: x => !(x is WorkflowTaskDependency))
            .OrderByDescending(keySelector: x => x.Name)
            .ForEach(action: stepChild =>
            {
                stepChild
                    .ChildItems.OrderByDescending(keySelector: x => x.Name)
                    .ForEach(action: innerChild =>
                    {
                        AddNodeItem(item: innerChild, subGraph: subgraphNode, leftMargin: 30);
                    });
                AddNodeItem(item: stepChild, subGraph: subgraphNode, leftMargin: 15);
            });
    }

    private void AddNodeItem(ISchemaItem item, Subgraph subGraph, int leftMargin)
    {
        var nodeData = new NodeItemData(
            schemaItem: item,
            leftMargin: leftMargin,
            schemaService: schemaService
        );
        Node node = nodeFactory.AddNodeItem(nodeData: nodeData);
        subGraph.AddNode(node: node);
    }

    private void AddNodeItem(Subgraph subGraph, INodeData nodeData)
    {
        Node node = nodeFactory.AddNodeItem(nodeData: nodeData);
        subGraph.AddNode(node: node);
    }

    private Subgraph AddWorkflowDiagram(IWorkflowBlock workFlowBlock, Subgraph parentSubgraph)
    {
        BlockSubGraph subgraph = nodeFactory.AddSubgraph(
            parentSbubgraph: parentSubgraph,
            schemaItem: workFlowBlock
        );
        AddContextStores(block: workFlowBlock, blockSubGraph: subgraph);
        return AddToSubgraph(workFlowBlock: workFlowBlock, subgraph: subgraph);
    }

    private Subgraph AddToSubgraph(IWorkflowBlock workFlowBlock, Subgraph subgraph)
    {
        IDictionary<Key, Node> nodes = new Dictionary<Key, Node>();
        foreach (
            IWorkflowStep step in workFlowBlock.ChildItemsByType<AbstractWorkflowStep>(
                itemType: AbstractWorkflowStep.CategoryConst
            )
        )
        {
            Node shape = step is IWorkflowBlock subBlock
                ? AddWorkflowDiagram(workFlowBlock: subBlock, parentSubgraph: subgraph)
                : AddSubgraphNode(step: step, subGraph: subgraph);
            nodes.Add(key: step.PrimaryKey, value: shape);
        }
        // add connections
        foreach (
            IWorkflowStep step in workFlowBlock.ChildItemsByType<AbstractWorkflowStep>(
                itemType: AbstractWorkflowStep.CategoryConst
            )
        )
        {
            Node destinationShape = nodes[key: step.PrimaryKey];
            if (destinationShape == null)
            {
                throw new NullReferenceException(
                    message: Strings.WorkFlowDiagramFactory_DestinationShape_not_found
                );
            }

            int i = 0;
            foreach (
                WorkflowTaskDependency dependency in step.ChildItemsByType<WorkflowTaskDependency>(
                    itemType: WorkflowTaskDependency.CategoryConst
                )
            )
            {
                Node sourceShape = nodes[key: dependency.Task.PrimaryKey];
                if (sourceShape == null)
                {
                    throw new NullReferenceException(
                        message: Strings.WorkFlowDiagramFactory_SourceShape_not_found
                    );
                }

                Edge edge = graph.AddEdge(source: sourceShape.Id, target: destinationShape.Id);
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
                graph.MainDrawingSubgraf.AddNode(node: startBalloon);
                graph.AddEdge(source: startBalloon.Id, target: subgraph.Id);
            }
            if (!subgraph.OutEdges.Any())
            {
                Node endBalloon = nodeFactory.AddEndBalloon();
                graph.MainDrawingSubgraf.AddNode(node: endBalloon);
                graph.AddEdge(source: subgraph.Id, target: endBalloon.Id);
            }
        }
    }
}
