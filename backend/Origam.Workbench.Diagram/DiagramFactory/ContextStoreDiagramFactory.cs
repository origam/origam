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

using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.NodeDrawing;
using Origam.Workbench.Services;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.DiagramFactory;

class ContextStoreDiagramFactory: IDiagramFactory<IContextStore, Graph>
{

    private Graph graph;
    private readonly IPersistenceProvider persistenceProvider;
    private readonly INodeSelector nodeSelector;
    private readonly GViewer gViewer;
    private readonly WorkbenchSchemaService schemaService;
    private NodeFactory nodeFactory;

    public ContextStoreDiagramFactory(
        IPersistenceProvider persistenceProvider,
        INodeSelector nodeSelector, GViewer gViewer,
        WorkbenchSchemaService schemaService)
    {
            this.persistenceProvider = persistenceProvider;
            this.nodeSelector = nodeSelector;
            this.gViewer = gViewer;
            this.schemaService = schemaService;
        }

    public Graph Draw(IContextStore contextStore)
    {
            graph = new Graph();
            nodeFactory = new NodeFactory(nodeSelector, gViewer, schemaService, graph );
            
            Node storeNode = nodeFactory.AddNode(contextStore);
            List<IWorkflowStep> steps = persistenceProvider
                .RetrieveList<IWorkflowStep>();
            
            foreach (IWorkflowStep step in steps)
            {
                if (step is WorkflowTask task &&
                    task.OutputContextStoreId == contextStore.Id)
                {
                    Node taskNode = nodeFactory.AddNode(task);
                    graph.AddEdge(storeNode.Id, taskNode.Id);
                }
                else if (step is UpdateContextTask updateTask &&
                          updateTask.XPathContextStore.Id == contextStore.Id)
                {
                    Node taskNode = nodeFactory.AddNode(updateTask);
                    graph.AddEdge(taskNode.Id, storeNode.Id);
                }
            }

            return graph;
        }
}