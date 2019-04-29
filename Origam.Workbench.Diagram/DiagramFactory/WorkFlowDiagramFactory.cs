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
using Origam.Schema.WorkflowModel;
using Microsoft.Msagl.Drawing;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Extensions;
using WeifenLuo.WinFormsUI.Docking;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram
{
	public class WorkFlowDiagramFactory : IDiagramFactory<IWorkflowBlock>
	{
		private static readonly int margin = 3;
		private static readonly int marginLeft = 5;
		private static readonly Font font = new Font("Arial", 12);
		private static readonly Pen blackPen = new Pen(System.Drawing.Color.Black, 1);
		private static readonly SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
		private static readonly StringFormat drawFormat = new StringFormat();
		
		private readonly INodeSelector nodeSelector;
		private Graph graph;
		private readonly NodeFactory nodeFactory;
		private readonly Pen boldBlackPen = new Pen(System.Drawing.Color.Black, 2);
		private readonly GViewer viewer;

		public WorkFlowDiagramFactory(GViewer viewer, INodeSelector nodeSelector)
		{
			this.viewer = viewer;
			this.nodeSelector = nodeSelector;
			nodeFactory = new NodeFactory(nodeSelector);
		}

		public Graph Draw(IWorkflowBlock graphParent)
		{
			graph = new Graph();
			DrawWorkflowDiagram(graphParent, null);
			graph.LayoutAlgorithmSettings.ClusterMargin = 20;
			return graph;
		}

		private Node AddNode(IWorkflowStep step, Subgraph subGraph)
		{
			Node node = nodeFactory.AddNode(graph, step);
            node.LabelText = step.Name;
            node.UserData = step;
            subGraph?.AddNode(node);
            return node;
		}

		private Subgraph DrawWorkflowDiagram(IWorkflowBlock workFlowBlock, Subgraph parentSubgraph)
		{
            Subgraph subgraph = new Subgraph(workFlowBlock.NodeId);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            subgraph.DrawNodeDelegate = DrawSubgraph;
            subgraph.UserData = workFlowBlock;
            subgraph.LabelText = workFlowBlock.Name;
            
            if (parentSubgraph == null)
            {
                graph.RootSubgraph.AddSubgraph(subgraph);
            }
            else
            {
                parentSubgraph.AddSubgraph(subgraph);
            }
            IDictionary<Key, Node> ht = new Dictionary<Key, Node>();

			foreach(IWorkflowStep step in workFlowBlock.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
                IWorkflowBlock subBlock = step as IWorkflowBlock;
                if (subBlock == null)
                {
                    Node shape = AddNode(step, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
                else
                {
                    Node shape = DrawWorkflowDiagram(subBlock, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
			}

			// add connections
			foreach(IWorkflowStep step in workFlowBlock.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
				Node destinationShape = ht[step.PrimaryKey];
				if(destinationShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorDestinationShapeNotFound"));
				int i = 0;
				foreach(WorkflowTaskDependency dependency in step.ChildItemsByType(WorkflowTaskDependency.ItemTypeConst))
				{
					Node sourceShape = ht[dependency.Task.PrimaryKey];
					if(sourceShape == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorSourceShapeNotFound"));

					Edge edge = graph.AddEdge(sourceShape.Id,destinationShape.Id);
					edge.UserData = dependency;
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
		
		private static Image GetImage(Node node)
		{
			var schemaItem = (ISchemaItem) node.UserData;

			var schemaBrowser =
				WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as IBrowserPad;
			var imageList = schemaBrowser.ImageList;
			Image image = imageList.Images[schemaBrowser.ImageIndex(schemaItem.Icon)];
			return image;
		}
		
		private bool DrawSubgraph(Node node, object graphicsObj)
		{
			var borderSize = new Size(
				(int)node.BoundingBox.Width, 
				(int)node.BoundingBox.Height);
			
			Pen pen = nodeSelector.Selected == node
				? boldBlackPen 
				: blackPen;
			
			Graphics editorGraphics = (Graphics)graphicsObj;
			var image = GetImage(node);

			SizeF stringSize = editorGraphics.MeasureString(node.LabelText, font);
			var labelWidth = stringSize.Width + margin + image.Width;
			
			var borderCorner = new System.Drawing.Point(
				(int)node.GeometryNode.Center.X - borderSize.Width / 2,
				(int)node.GeometryNode.Center.Y - borderSize.Height / 2);
			Rectangle border = new Rectangle(borderCorner, borderSize);

			var labelPoint = new PointF(
				(float)(node.GeometryNode.Center.X - labelWidth / 2 + marginLeft + margin + image.Width),
				(float)node.GeometryNode.Center.Y - border.Height / 2.0f + margin);

			var imagePoint = new PointF(
				(float)(node.GeometryNode.Center.X - labelWidth / 2 + marginLeft),
				(float)(node.GeometryNode.Center.Y - border.Height / 2.0f + margin ));

			editorGraphics.DrawUpSideDown(drawAction: graphics =>
				{
					graphics.DrawString(node.LabelText, font, drawBrush,
						labelPoint, drawFormat);
					graphics.DrawRectangle(pen, border);
					graphics.DrawImage(image, imagePoint);
				}, 
				yAxisCoordinate: (float)node.GeometryNode.Center.Y);
            
			return true;
		}
	}
}
