using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.InternalEditor
{
    class ContextStoreDependencyPainter
    {
        private readonly IPersistenceProvider persistenceProvider;
        private readonly Func<AbstractSchemaItem> graphParentItemGetter;
        private readonly GViewer gViewer;
        private readonly List<Edge> displayedEdgeList = new List<Edge>(); 

        public ContextStoreDependencyPainter(NodeSelector nodeSelector,
            IPersistenceProvider persistenceProvider,
            GViewer gViewer, Func<AbstractSchemaItem> graphParentItemGetter)
        {
            this.persistenceProvider = persistenceProvider;
            this.gViewer = gViewer;
            this.graphParentItemGetter = graphParentItemGetter;
            nodeSelector.NodeSelected += OnNodeSelected;
        }

        private void OnNodeSelected(object sender, Guid? id)
        {
            RemoveEdges();
            if (id == null)
            {
                return;
            }

            var selectedItem = persistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new Key(id.Value));

            if (selectedItem is IContextStore contextStore)
            {
                Node contextStoreNode = gViewer.Graph.FindNodeOrSubgraph(contextStore.NodeId);
                var allChildren =
                    graphParentItemGetter.Invoke()
                        .ChildrenRecursive;
				
                foreach (var schemaItem in allChildren)
                {
                    bool isTargetOfFromArrow = IsOutpuContextStore(schemaItem, contextStore);
                    bool isSourceOfToArrow = IsInputContextStore(schemaItem, contextStore);
                    if (isTargetOfFromArrow && isSourceOfToArrow)
                    {
                        DrawBothDirectionConnection(contextStoreNode, schemaItem);
                    }
                    else if (isTargetOfFromArrow)
                    {
                        DrawFromArrow(contextStoreNode, schemaItem);
                    }
                    else if (isSourceOfToArrow)
                    {
                        DrawToArrow(contextStoreNode, schemaItem);
                    }
                }
            }
        }
		
        private void DrawBothDirectionConnection(Node contextStoreNode,  AbstractSchemaItem sourceItem)
        {
            var sourceNode = gViewer.Graph.FindNodeOrSubgraph(sourceItem.NodeId);
            if (sourceNode != null)
            {
                Edge edge = gViewer.AddEdge(sourceNode, contextStoreNode, false);
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
                edge.Attr.ArrowheadLength = 0;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.Attr.Color = Color.Green;
                displayedEdgeList.Add(edge);
            }
        }
		
        private void DrawToArrow(Node contextStoreNode,  AbstractSchemaItem sourceItem)
        {
            var sourceNode = gViewer.Graph.FindNodeOrSubgraph(sourceItem.NodeId);
            if (sourceNode != null)
            {
                Edge edge = gViewer.AddEdge(sourceNode, contextStoreNode, false);
                edge.Attr.Color = Color.Red;
                displayedEdgeList.Add(edge);
            }
        }

        private void DrawFromArrow(Node contextStoreNode,  AbstractSchemaItem targetItem)
        {
            var targetNode = gViewer.Graph.FindNodeOrSubgraph(targetItem.NodeId);
            if (targetNode != null)
            {
                Edge edge = gViewer.AddEdge(contextStoreNode, targetNode, false);
                edge.Attr.Color = Color.Blue;
                displayedEdgeList.Add(edge);
            }
        }

        private void RemoveEdges()
        {
            foreach (Edge edge in displayedEdgeList)
            {
                gViewer.RemoveEdge(edge);
            }
            displayedEdgeList.Clear();
        }

        private bool IsOutpuContextStore(AbstractSchemaItem item,  IContextStore contextStore)
        {
            return item.GetDependencies(true).Contains(contextStore);
        }

        private bool IsInputContextStore(AbstractSchemaItem item,  IContextStore contextStore)
        {
            if (item is ServiceMethodCallTask callTask)
            {
                return callTask.ValidationRuleContextStore == contextStore ||
                       callTask.StartConditionRuleContextStore == contextStore;
            }
//			else if (Work)
//			{
//			}

            return false;
        }
    }
}