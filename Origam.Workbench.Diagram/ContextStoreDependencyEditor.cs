using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Extensions;


namespace Origam.Workbench.Diagram
{
    public class ContextStoreDependencyEditor: IDiagramEditor
    {
        private DiagramFactory _factory;
        private readonly GViewer gViewer;
        private readonly IContextStore contextStore;
        private readonly IPersistenceProvider persistenceProvider;

        public ContextStoreDependencyEditor(GViewer gViewer, IContextStore contextStore, IPersistenceProvider persistenceProvider)
        {
            this.gViewer = gViewer;
            this.contextStore = contextStore;
            this.persistenceProvider = persistenceProvider;

           Draw();
        }

        private void Draw()
        {
            gViewer.Graph = new Graph();
            Node storeNode = AddNode(contextStore.Id.ToString(), contextStore.Name);
            List<IWorkflowStep> steps = persistenceProvider
                .RetrieveList<IWorkflowStep>();
            
            foreach (IWorkflowStep step in steps)
            {
                if (step is WorkflowTask task &&
                    task.OutputContextStoreId == contextStore.Id)
                {
                    Node taskNode = AddNode(task.Id.ToString(), task.Name);
                    gViewer.Graph.AddEdge(storeNode.Id, taskNode.Id);
                }else if (step is UpdateContextTask updateTask &&
                          updateTask.XPathContextStore.Id == contextStore.Id)
                {
                    Node taskNode = AddNode(updateTask.Id.ToString(), updateTask.Name);
                    gViewer.Graph.AddEdge(taskNode.Id, storeNode.Id);
                }
            }
            gViewer.Redraw();
        }
        
        private Node AddNode(string id,string label)
        {
            return AddNode(id, label, null);
        }

        public Node AddNode(string id, string label, Subgraph subGraph)
        {
            Node shape = gViewer.Graph.AddNode(id);
            shape.LabelText = label;
            subGraph?.AddNode(shape);
            return shape;
        }

        public void Dispose()
        {
        }
    }
}