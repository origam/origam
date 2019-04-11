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
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Microsoft.Msagl.Drawing;
using System.Collections.Generic;

namespace Origam.Workbench.Diagram
{
	/// <summary>
	/// Summary description for DiagramFactory.
	/// </summary>
	public class DiagramFactory
	{

	    private readonly ISchemaItem graphParent;

        #region Constructors
        public DiagramFactory(ISchemaItem graphParent)
        {
            this.graphParent = graphParent;
        }
		#endregion

		#region Properties

		private Graph graph;

		#endregion

		#region Public Methods
		public Graph Draw()
		{
			graph = new Graph();
			
			if(graphParent is IWorkflowBlock workflowBlock)
			{
				DrawWorkflowDiagram(workflowBlock, null);
			}
			else
			{
				DrawUniSchemaDiagram(graphParent);
			}

			return graph;
		}
		#endregion

		#region Private Methods
        private Node AddNode(string id,string label)
        {
            return AddNode(id, label, null);
        }

		public Node AddNode(string id, string label, Subgraph subGraph)
		{
			Node shape = graph.AddNode(id);
            shape.LabelText = label;
            subGraph?.AddNode(shape);
            return shape;
		}

		#region Workflow Diagram
		private Subgraph DrawWorkflowDiagram(IWorkflowBlock block, Subgraph parentSubgraph)
		{
            Subgraph subgraph = new Subgraph(block.NodeId);
            subgraph.LabelText = block.Name;
            if (parentSubgraph == null)
            {
                this.graph.RootSubgraph.AddSubgraph(subgraph);
            }
            else
            {
                parentSubgraph.AddSubgraph(subgraph);
            }
            IDictionary<Key, Node> ht = new Dictionary<Key, Node>();
			// root shape
			//Node blockShape = this.AddBasicShape(block.Name, subgraph);
			//ht.Add(block.PrimaryKey, blockShape);

			foreach(IWorkflowStep step in block.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
                IWorkflowBlock subBlock = step as IWorkflowBlock;
                if (subBlock == null)
                {
                    Node shape = this.AddNode(step.NodeId, step.Name, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
                else
                {
                    Node shape = DrawWorkflowDiagram(subBlock, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
			}

			// add connections
			foreach(IWorkflowStep step in block.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
				Node destinationShape = ht[step.PrimaryKey];
				if(destinationShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorDestinationShapeNotFound"));
				int i = 0;
				foreach(WorkflowTaskDependency dependency in step.ChildItemsByType(WorkflowTaskDependency.ItemTypeConst))
				{
					Node sourceShape = ht[dependency.Task.PrimaryKey];
					if(sourceShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorSourceShapeNotFound"));

					this.graph.AddEdge(sourceShape.Id,
                        destinationShape.Id);
					i++;
				}

				if(i==0)
				{
					// no connections, we set the connection to the root block
                    //this.Graph.AddEdge(blockShape.Id, destinationShape.Id);
				}
			}
            return subgraph;
        }
		#endregion

		#region Uni Diagram
		private void DrawUniSchemaDiagram(ISchemaItem item)
		{
			DrawUniShape(item, null);
		}

		private void DrawUniShape(ISchemaItem schemaItem, Node parentShape)
		{
			Node shape = this.AddNode(schemaItem.Id.ToString(), schemaItem.Name);
			if(parentShape != null)
			{
				this.graph.AddEdge(shape.Id, parentShape.Id);
			}
			foreach(ISchemaItem child in schemaItem.ChildItems)
			{
				DrawUniShape(child, shape);
			}
		}
		#endregion
		#endregion
	}
}
