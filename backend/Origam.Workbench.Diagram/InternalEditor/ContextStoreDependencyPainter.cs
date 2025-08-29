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
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Diagram.NodeDrawing;
using Origam.Workbench.Services;

namespace Origam.Workbench.Diagram.InternalEditor;
class ContextStoreDependencyPainter
{
    private readonly Func<ISchemaItem> graphParentItemGetter;
    private readonly GViewer gViewer;
    private readonly List<IArrowPainter> arrowPainters = new List<IArrowPainter>();
    public ContextStoreDependencyPainter(GViewer gViewer,
        Func<ISchemaItem> graphParentItemGetter)
    {
        this.gViewer = gViewer;
        this.graphParentItemGetter = graphParentItemGetter;
    }
    public IContextStore CurrentContextStore { get; private set; }
    public bool DidDrawSomeEdges => arrowPainters.Count > 0;
    public void DeActivate()
    {
        CurrentContextStore = null;
        RemoveEdges();
    }
    public void Activate(IContextStore contextStore)
    {
        CurrentContextStore = contextStore;
    }
    public List<string> GetNodesToExpand()
    {
        RemoveEdges();
        if (CurrentContextStore != null)
        {
            PreparePainters(CurrentContextStore);
            return FindTasksToExpand();
        }
        return new List<string>();
    }
    public void Draw()
    {
        if (CurrentContextStore != null)
        {
            DrawEdges(CurrentContextStore.NodeId);
        }
    }
    private void PreparePainters(IContextStore contextStore)
    {
        var allChildren =
            graphParentItemGetter.Invoke()
                .ChildrenRecursive;
        foreach (var schemaItem in allChildren)
        {
            bool isTargetOfFromArrow =
                IsInputContextStore(schemaItem, contextStore);
            bool isSourceOfToArrow = IsOutpuContextStore(schemaItem, contextStore);
            if (isTargetOfFromArrow && isSourceOfToArrow)
            {
                arrowPainters.Add(
                    new BidirectionalArrowPainter(
                        gViewer,
                        schemaItem)
                );
            }
            else if (isTargetOfFromArrow)
            {
                arrowPainters.Add(
                    new FromArrowPainter(
                        gViewer,
                        schemaItem)
                );
            }
            else if (isSourceOfToArrow)
            {
                arrowPainters.Add(
                    new ToArrowPainter(
                        gViewer,
                        schemaItem)
                );
            }
        }
    }
    private List<string> FindTasksToExpand()
    {
        List<string> tasksToExpand = arrowPainters
            .Select(painter => painter.SchemaItem)
            .Where(item => !(item is IWorkflowTask))
            .Select(item => item.FirstParentOfType<IWorkflowTask>()?.Id)
            .Where(id => id != null)
            .Select(id => IdTranslator.SchemaToFirstNode(id.ToString()))
            .ToList();
        return tasksToExpand;
    }
    private void DrawEdges(string contextStoreId)
    {
        Node contextStoreNode = gViewer.Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(contextStoreId));
        foreach (IArrowPainter painter in arrowPainters)
        {
            painter.Draw(contextStoreNode);
        }
    }
    
    private void RemoveEdges()
    {
        foreach (IArrowPainter painter in arrowPainters)
        {
            gViewer.RemoveEdge(painter.Edge);
        }
        arrowPainters.Clear();
    }
    private bool IsOutpuContextStore(ISchemaItem item,  IContextStore contextStore)
    {
        if (item is WorkflowTask workflowTask)
        {
           if (workflowTask.OutputMethod == ServiceOutputMethod.Ignore)
            {
                return false;
            }

            if (workflowTask.OutputContextStore == contextStore)
            {
                return true;
            }
        }
        if (item is ContextStoreLink link)
        {
            return  link.CallerContextStore == contextStore &&
                    link.Direction == ContextStoreLinkDirection.Output;
        }
        return false;
    }
    private bool IsInputContextStore(ISchemaItem item,  IContextStore contextStore)
    {
        if (item is WorkflowTask workflowTask)
        {
            if (workflowTask.OutputContextStore == contextStore ||
                workflowTask.OutputMethod == ServiceOutputMethod.Ignore)
            {
                return false;
            }
        }
        if (item is ServiceMethodCallTask callTask)
        {
            return callTask.ValidationRuleContextStore == contextStore ||
                   callTask.StartConditionRuleContextStore == contextStore;
        }
        if (item is ContextStoreLink link &&
            link.CallerContextStore == contextStore &&
            link.Direction == ContextStoreLinkDirection.Input)
        {
            return true;
        }
        return item.GetDependencies(true).Contains(contextStore);
    }
}

interface IArrowPainter
{
    void Draw(Node contextStoreNode);
    Edge Edge { get; }
    ISchemaItem SchemaItem { get; }
}
abstract class ArrowPainter: IArrowPainter
{
    protected readonly GViewer gViewer;
    public Edge Edge { get; protected set; }
    public ISchemaItem SchemaItem { get; }
    public ArrowPainter(GViewer gViewer, ISchemaItem schemaItem)
    {
        this.gViewer = gViewer;
        SchemaItem = schemaItem;
    }
    
    public abstract void Draw(Node contextStoreNode);
}
class ToArrowPainter: ArrowPainter
{
    public ToArrowPainter(GViewer gViewer, ISchemaItem sourceItem)
        : base(gViewer, sourceItem)
    {
    }
    public override void Draw(Node contextStoreNode)
    {
        var sourceNode = gViewer.Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(SchemaItem.NodeId));
        if (sourceNode != null)
        {
            Edge = gViewer.AddEdge(sourceNode, contextStoreNode, false);
            Edge.Attr.Color = Color.Red;
        } 
    }
}
class FromArrowPainter: ArrowPainter
{
    public FromArrowPainter(GViewer gViewer,
        ISchemaItem targetItem) : base(gViewer,  targetItem)
    {
    }
    public override void Draw(Node contextStoreNode)
    {
        var targetNode = gViewer.Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(SchemaItem.NodeId));
        if (targetNode != null)
        {
            Edge = gViewer.AddEdge(contextStoreNode, targetNode, false);
            Edge.Attr.Color = Color.Blue;
        }
    }
}
class BidirectionalArrowPainter: ArrowPainter
{
    public BidirectionalArrowPainter(GViewer gViewer, 
        ISchemaItem targetItem) : base(gViewer,  targetItem)
    {
    }
    public override void Draw(Node contextStoreNode)
    {
        var sourceNode = gViewer.Graph.FindNodeOrSubgraph(IdTranslator.SchemaToFirstNode(SchemaItem.NodeId));
        if (sourceNode != null)
        {
            Edge = gViewer.AddEdge(sourceNode, contextStoreNode, false);
            Edge.Attr.ArrowheadAtSource = ArrowStyle.None;
            Edge.Attr.ArrowheadLength = 0;
            Edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
            Edge.Attr.Color = Color.Green;
        }
    }
}
