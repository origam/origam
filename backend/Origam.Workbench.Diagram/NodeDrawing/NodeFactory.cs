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
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Graphs;
using Origam.Workbench.Services;
using Node = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.NodeDrawing;

class NodeFactory
{
    private readonly InternalPainter internalPainter;
    private readonly WorkbenchSchemaService schemaService;
    private readonly Graph graph;
    private readonly IdTranslator idTranslator;
    private static int balloonNumber = 0;

    public NodeFactory(
        INodeSelector nodeSelector,
        GViewer gViewer,
        WorkbenchSchemaService schemaService,
        Graph graph
    )
    {
        this.schemaService = schemaService;
        this.graph = graph;
        internalPainter = new InternalPainter(nodeSelector: nodeSelector, gViewer: gViewer);
        idTranslator = new IdTranslator();
    }

    public Node AddNode(ISchemaItem schemaItem)
    {
        INodeData nodeData = new NodeData(schemaItem: schemaItem, schemaService: schemaService);
        Node node = graph.AddNode(nodeId: idTranslator.MakeNodeId(schemaItemId: schemaItem.Id));
        node.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new NodePainter(painter: internalPainter);
        node.DrawNodeDelegate = painter.Draw;
        node.NodeBoundaryDelegate = painter.GetBoundary;
        node.UserData = nodeData;
        node.LabelText = nodeData.Text;
        return node;
    }

    public Node AddNodeItem(INodeData nodeData)
    {
        Node node = graph.AddNode(nodeId: idTranslator.MakeNodeId(strId: nodeData.Id));
        node.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new NodeItemPainter(internalPainter: internalPainter);
        node.DrawNodeDelegate = painter.Draw;
        node.NodeBoundaryDelegate = painter.GetBoundary;
        node.UserData = nodeData;
        node.LabelText = nodeData.Text;
        return node;
    }

    public Subgraph AddSubgraphNode(Subgraph parentSbubgraph, ISchemaItem schemaItem)
    {
        INodeData nodeData = new NodeData(schemaItem: schemaItem, schemaService: schemaService);
        Subgraph subgraph = new Subgraph(id: idTranslator.MakeNodeId(schemaItemId: schemaItem.Id));
        subgraph.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new SubgraphNodePainter(internalPainter: internalPainter);
        subgraph.DrawNodeDelegate = painter.Draw;
        subgraph.NodeBoundaryDelegate = painter.GetBoundary;
        subgraph.UserData = nodeData;
        subgraph.LabelText = nodeData.Text;
        if (parentSbubgraph is BlockSubGraph blockSubGraph)
        {
            blockSubGraph.MainDrawingSubgraf.AddSubgraph(subgraph: subgraph);
        }
        else
        {
            parentSbubgraph.AddSubgraph(subgraph: subgraph);
        }
        return subgraph;
    }

    public BlockSubGraph AddSubgraph(Subgraph parentSbubgraph, IWorkflowBlock schemaItem)
    {
        INodeData nodeData = new NodeData(schemaItem: schemaItem, schemaService: schemaService);
        BlockSubGraph subgraph = new BlockSubGraph(
            id: idTranslator.MakeNodeId(schemaItemId: schemaItem.Id)
        );
        subgraph.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new SubgraphPainter(internalPainter: internalPainter);
        subgraph.DrawNodeDelegate = painter.Draw;
        subgraph.NodeBoundaryDelegate = painter.GetBoundary;
        subgraph.UserData = nodeData;
        subgraph.LabelText = nodeData.Text;
        if (parentSbubgraph is BlockSubGraph parentBlockSubGraph)
        {
            parentBlockSubGraph.MainDrawingSubgraf.AddSubgraph(subgraph: subgraph);
        }
        else
        {
            parentSbubgraph.AddSubgraph(subgraph: subgraph);
        }
        return subgraph;
    }

    public Subgraph AddActionSubgraph(Subgraph parentSbubgraph, ISchemaItem schemaItem)
    {
        INodeData nodeData = new NodeItemLabel(text: schemaItem.Name);
        Subgraph subgraph = new Subgraph(id: idTranslator.MakeNodeId(schemaItemId: schemaItem.Id));
        subgraph.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new ActionSubgraphPainter(internalPainter: internalPainter);
        subgraph.DrawNodeDelegate = painter.Draw;
        subgraph.NodeBoundaryDelegate = painter.GetBoundary;
        subgraph.UserData = nodeData;
        subgraph.LayoutSettings = new SugiyamaLayoutSettings
        {
            PackingMethod = PackingMethod.Compact,
            PackingAspectRatio = 1000,
            AdditionalClusterTopMargin = 20,
            ClusterMargin = 10,
        };

        parentSbubgraph.AddSubgraph(subgraph: subgraph);
        return subgraph;
    }

    public void AddActionNode(Subgraph actionSubgraph, EntityUIAction action)
    {
        INodeData nodeData = new NodeData(action: action, schemaService: schemaService);
        Subgraph subgraph = new Subgraph(id: idTranslator.MakeNodeId(strId: nodeData.Id));
        subgraph.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new ActionNodePainter(internalPainter: internalPainter);
        subgraph.DrawNodeDelegate = painter.Draw;
        subgraph.NodeBoundaryDelegate = painter.GetBoundary;
        subgraph.UserData = nodeData;
        actionSubgraph.AddSubgraph(subgraph: subgraph);
    }

    public Node AddStarBalloon()
    {
        return AddBalloon(graph: graph, balloonBrush: internalPainter.GreenBrush, label: "Start");
    }

    public Node AddEndBalloon()
    {
        return AddBalloon(graph: graph, balloonBrush: internalPainter.RedBrush, label: "End");
    }

    private Node AddBalloon(Graph graph, SolidBrush balloonBrush, string label)
    {
        Node node = graph.AddNode(nodeId: $"{label} balloon {balloonNumber++}");
        node.Attr.Shape = Shape.DrawFromGeometry;
        var painter = new BalloonPainter(painter: internalPainter, balloonBrush: balloonBrush);
        node.DrawNodeDelegate = painter.Draw;
        node.NodeBoundaryDelegate = painter.GetBoundary;
        node.LabelText = label;
        return node;
    }
}

class IdTranslator
{
    private static Regex idRegex;
    private readonly HashSet<string> createdNodeIds = new HashSet<string>();

    public string MakeNodeId(string strId)
    {
        if (Guid.TryParse(input: strId, result: out Guid id))
        {
            return MakeNodeId(schemaItemId: id);
        }
        return strId;
    }

    public string MakeNodeId(Guid schemaItemId)
    {
        for (int i = 0; i < 100; i++)
        {
            string nodeId = schemaItemId + "_Instance_" + i;
            if (!createdNodeIds.Contains(item: nodeId))
            {
                createdNodeIds.Add(item: nodeId);
                return nodeId;
            }
        }
        throw new Exception(
            message: "There are too many nodes referencing the same schema item in this diagram"
        );
    }

    public static Guid ToSchemaId(Node node)
    {
        return NodeToSchema(nodeId: node.Id);
    }

    public static Guid NodeToSchema(string nodeId)
    {
        if (nodeId == null)
        {
            return Guid.Empty;
        }
        idRegex = new Regex(pattern: @"(^[0-9A-Fa-f\-]{36})_Instance_\d+");
        Match match = idRegex.Match(input: nodeId);
        if (!match.Success)
        {
            return Guid.Empty;
        }
        return Guid.Parse(input: match.Groups[groupnum: 1].Value);
    }

    public static string SchemaToFirstNode(Guid schemaItemId)
    {
        return SchemaToFirstNode(schemaItemId: schemaItemId.ToString());
    }

    public static string SchemaToFirstNode(string schemaItemId)
    {
        return schemaItemId + "_Instance_" + 0;
    }
}

internal interface INodeData
{
    ISchemaItem SchemaItem { get; }
    string Text { get; }
    Image PrimaryImage { get; }
    Image SecondaryImage { get; }
    bool IsFromActivePackage { get; }
    string Id { get; }
    int LeftMargin { get; }
}

class NodeItemLabel : INodeData
{
    private static int lastId;

    public ISchemaItem SchemaItem { get; }
    public string Text { get; }
    public Image PrimaryImage { get; }
    public Image SecondaryImage { get; }
    public bool IsFromActivePackage { get; } = true;
    public string Id { get; }
    public int LeftMargin { get; }

    public NodeItemLabel(string text)
    {
        Text = text;
        Id = "NodeItemLabel_" + lastId++;
    }

    public NodeItemLabel(string text, int leftMargin)
        : this(text: text)
    {
        LeftMargin = leftMargin;
    }
}

class NodeItemData : NodeData
{
    public NodeItemData(
        ISchemaItem schemaItem,
        int leftMargin,
        WorkbenchSchemaService schemaService
    )
        : base(schemaItem: schemaItem, schemaService: schemaService)
    {
        LeftMargin = leftMargin;
    }
}

class NodeData : INodeData
{
    private readonly WorkbenchSchemaService schemaService;
    private Image primaryImage;
    private Image secondaryImage;

    public virtual Image PrimaryImage
    {
        get
        {
            if (primaryImage == null)
            {
                if (SchemaItem.NodeImage != null)
                {
                    primaryImage = SchemaItem.NodeImage.ToBitmap();
                    return primaryImage;
                }
                primaryImage = GetImage(iconId: SchemaItem.Icon);
            }
            return primaryImage;
        }
    }
    public Image SecondaryImage
    {
        get
        {
            if (
                secondaryImage == null
                && SchemaItem is AbstractWorkflowStep workflowStep
                && workflowStep.StartConditionRule != null
            )
            {
                secondaryImage = GetImage(iconId: workflowStep.StartConditionRule.Icon);
            }
            return secondaryImage;
        }
    }
    public ISchemaItem SchemaItem { get; }
    public string Text { get; }
    public bool IsFromActivePackage =>
        SchemaItem.Package.Id == schemaService.ActiveSchemaExtensionId;
    public string Id => SchemaItem.Id.ToString();
    public int LeftMargin { get; protected set; } = 0;

    public NodeData(ISchemaItem schemaItem, WorkbenchSchemaService schemaService)
    {
        this.schemaService = schemaService;
        SchemaItem = schemaItem;
        Text = SchemaItem.Name;
    }

    public NodeData(EntityUIAction action, WorkbenchSchemaService schemaService)
    {
        this.schemaService = schemaService;
        SchemaItem = action;
        Text = action.Caption;
    }

    private Image GetImage(string iconId)
    {
        var schemaBrowser =
            WorkbenchSingleton.Workbench.GetPad(type: typeof(IBrowserPad)) as IBrowserPad;
        var imageList = schemaBrowser.ImageList;
        return imageList.Images[index: schemaBrowser.ImageIndex(icon: iconId)];
    }
}
