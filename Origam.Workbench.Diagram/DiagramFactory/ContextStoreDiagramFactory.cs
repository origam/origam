using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;

namespace Origam.Workbench.Diagram.DiagramFactory
{
    class ContextStoreDiagramFactory: IDiagramFactory<IContextStore>
    {
        private Graph graph;
        private readonly IPersistenceProvider persistenceProvider;

        public ContextStoreDiagramFactory(IPersistenceProvider persistenceProvider)
        {
            this.persistenceProvider = persistenceProvider;
        }

        public Graph Draw(IContextStore contextStore)
        {
            graph = new Graph();
            
            Node storeNode = AddNode(contextStore.Id.ToString(), contextStore.Name);
            List<IWorkflowStep> steps = persistenceProvider
                .RetrieveList<IWorkflowStep>();
            
            foreach (IWorkflowStep step in steps)
            {
                if (step is WorkflowTask task &&
                    task.OutputContextStoreId == contextStore.Id)
                {
                    Node taskNode = AddNode(task.Id.ToString(), task.Name);
                    graph.AddEdge(storeNode.Id, taskNode.Id);
                }else if (step is UpdateContextTask updateTask &&
                          updateTask.XPathContextStore.Id == contextStore.Id)
                {
                    Node taskNode = AddNode(updateTask.Id.ToString(), updateTask.Name);
                    graph.AddEdge(taskNode.Id, storeNode.Id);
                }
            }

            return graph;
        }

        private Node AddNode(string id, string label)
        {
            Node shape = graph.AddNode(id);
            shape.LabelText = label;
            return shape;
        }
    }
}