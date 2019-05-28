using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Extensions;
using Origam.Workbench.Services;

namespace Origam.Workbench.Diagram.InternalEditor
{
    class ContextStoreDependencyPainter
    {
        private readonly NodeSelector nodeSelector;
        private readonly IPersistenceProvider persistenceProvider;
        private readonly Func<AbstractSchemaItem> graphParentItemGetter;
        private readonly GViewer gViewer;
        private readonly List<IArrowPainter> arrowPainters = new List<IArrowPainter>();
        private IContextStore lastContextStore;

        public ContextStoreDependencyPainter(NodeSelector nodeSelector,
            IPersistenceProvider persistenceProvider,
            GViewer gViewer, Func<AbstractSchemaItem> graphParentItemGetter)
        {
            this.nodeSelector = nodeSelector;
            this.persistenceProvider = persistenceProvider;
            this.gViewer = gViewer;
            this.graphParentItemGetter = graphParentItemGetter;

        }

        public bool IsActive => lastContextStore != null;

        public void DeActivate()
        {
            lastContextStore = null;
            RemoveEdges();
        }

        public List<string> GetNodesToExpand()
        {
            Guid? id = nodeSelector.SelectedNodeId;
            RemoveEdges();

            var selectedItem = id == null 
                ? null 
                : persistenceProvider.RetrieveInstance(
                    typeof(AbstractSchemaItem), new Key(id.Value)
                ) as AbstractSchemaItem;

            lastContextStore = selectedItem is IContextStore contextStore
                ? contextStore
                : lastContextStore;
            
            if (lastContextStore != null)
            {
                PreparePainters(lastContextStore);
                return FindTasksToExpand();
            }
            return new List<string>();
        }

        public void Draw()
        {
            if (lastContextStore != null)
            {
                DrawEdges(lastContextStore.NodeId);
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
                .Select(id => id.ToString())
                .ToList();
            return tasksToExpand;
        }

        private void DrawEdges(string contextStoreId)
        {
            Node contextStoreNode = gViewer.Graph.FindNodeOrSubgraph(contextStoreId);
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

        private bool IsOutpuContextStore(AbstractSchemaItem item,  IContextStore contextStore)
        {
            if (item is WorkflowTask workflowTask)
            {
               if (workflowTask.OutputMethod == ServiceOutputMethod.Ignore) return false;
               if (workflowTask.OutputContextStore == contextStore) return true;
            }
            if (item is ContextStoreLink link)
            {
                return  link.CallerContextStore == contextStore &&
                        link.Direction == ContextStoreLinkDirection.Output;
            }
            return false;
        }

        private bool IsInputContextStore(AbstractSchemaItem item,  IContextStore contextStore)
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
        AbstractSchemaItem SchemaItem { get; }
    }

    abstract class ArrowPainter: IArrowPainter
    {
        protected readonly GViewer gViewer;
        public Edge Edge { get; protected set; }
        public AbstractSchemaItem SchemaItem { get; }
        public ArrowPainter(GViewer gViewer, AbstractSchemaItem schemaItem)
        {
            this.gViewer = gViewer;
            SchemaItem = schemaItem;
        }
        
        public abstract void Draw(Node contextStoreNode);
    }

    class ToArrowPainter: ArrowPainter
    {
        public ToArrowPainter(GViewer gViewer, AbstractSchemaItem sourceItem)
            : base(gViewer, sourceItem)
        {
        }

        public override void Draw(Node contextStoreNode)
        {
            var sourceNode = gViewer.Graph.FindNodeOrSubgraph(SchemaItem.NodeId);
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
            AbstractSchemaItem targetItem) : base(gViewer,  targetItem)
        {
        }
        public override void Draw(Node contextStoreNode)
        {
            var targetNode = gViewer.Graph.FindNodeOrSubgraph(SchemaItem.NodeId);
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
            AbstractSchemaItem targetItem) : base(gViewer,  targetItem)
        {
        }
        public override void Draw(Node contextStoreNode)
        {
            var sourceNode = gViewer.Graph.FindNodeOrSubgraph(SchemaItem.NodeId);
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
}