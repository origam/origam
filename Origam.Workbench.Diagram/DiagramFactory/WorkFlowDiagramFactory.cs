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
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using MoreLinq.Extensions;
using Origam.Schema;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.Extensions;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram
{
	public class WorkFlowDiagramFactory : IDiagramFactory<IWorkflowBlock, WorkFlowGraph>
	{
		private static readonly int labelTopMargin = 8;
		private static readonly double labelSideMargin = 20;
		
		private static readonly int imageTopMargin = 8;
		private static readonly int imageRightMargin = 3;
		private static readonly int imageLeftMargin = 5;
		
		private static readonly int headingBackgroundHeight = 30;
		private static readonly int nodeMargin = 40;

		private readonly int emptySubgraphWidth = 200;
		private readonly int emptySubgraphHeight = 80;
		
		private static readonly Font font = new Font("Arial", 10);
		private static readonly Pen blackPen = new Pen(System.Drawing.Color.Black, 1);
		private static readonly Pen boldBlackPen = new Pen(System.Drawing.Color.Black, 2);
		private static readonly SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
		private static readonly StringFormat drawFormat = new StringFormat();
		private static readonly SolidBrush greyBrush = new SolidBrush(System.Drawing.Color.LightGray);
		
		private readonly Graphics graphics;
		private readonly INodeSelector nodeSelector;
		private WorkFlowGraph graph;
		private readonly NodeFactory nodeFactory;

		public WorkFlowDiagramFactory(INodeSelector nodeSelector, Graphics graphics)
		{
			this.nodeSelector = nodeSelector;
			this.graphics = graphics;
			nodeFactory = new NodeFactory(nodeSelector);
		}

		public WorkFlowGraph Draw(IWorkflowBlock graphParent)
		{
			graph = new WorkFlowGraph();
			graph.TopSubgraph.DrawNodeDelegate = DrawHiddenSubgraph;
			AddWorkflowDiagram(graphParent, graph.TopSubgraph);
			AddContextStores(graphParent);
			graph.LayoutAlgorithmSettings.ClusterMargin = nodeMargin;
			return graph;
		}

		public void AlignContextStoreSubgraph()
		{
			if(graph.ContextStoreSubgraph == null || graph.MainDrawingSubgraf == null)
			{
				throw new InvalidOperationException();
			}

			MoveSubgraphRight(graph.ContextStoreSubgraph, graph.MainDrawingSubgraf);
		}

		private void MoveSubgraphRight(Subgraph subgraphToMove, Subgraph refSubgraph) {
            
			double dx = refSubgraph.Pos.X - subgraphToMove.Pos.X  +
			            refSubgraph.Width / 2 + subgraphToMove.Width / 2;
            
			double dy = refSubgraph.Pos.Y - subgraphToMove.Pos.Y  +
			            refSubgraph.Height / 2 - subgraphToMove.Height / 2;
            
			subgraphToMove.GeometryNode.Center = new Point(subgraphToMove.Pos.X + dx, subgraphToMove.Pos.Y + dy);
			((Cluster)subgraphToMove.GeometryNode).RectangularBoundary.Rect = 
				new Microsoft.Msagl.Core.Geometry.Rectangle(subgraphToMove.GeometryNode.BoundingBox.Size, subgraphToMove.Pos); 
			foreach (var node in subgraphToMove.Nodes)
			{
				node.GeometryNode.Center = new Point(node.Pos.X + dx, node.Pos.Y+ dy);
			}
		}

		private void AddContextStores(IWorkflowBlock graphParent)
		{
			graph.ContextStoreSubgraph.DrawNodeDelegate = DrawHiddenSubgraph;
			foreach (var childItem in graphParent.ChildItems)
			{
				if (childItem is ContextStore contextStore)
				{
					Node node = nodeFactory.AddNode(graph, contextStore);
					graph.ContextStoreSubgraph.AddNode(node);
				}
			}
		}

		private Node AddNode(IWorkflowStep step, Subgraph subGraph)
		{
//			Node node = nodeFactory.AddNode(graph, step);
			Node node = nodeFactory.AddSubgraphNode(subGraph, step);
            node.UserData = step;
           // subGraph.AddNode(node);
            return node;
		}

		private Subgraph AddWorkflowDiagram(IWorkflowBlock workFlowBlock, Subgraph parentSubgraph)
		{
            Subgraph subgraph = new Subgraph(workFlowBlock.NodeId);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            subgraph.DrawNodeDelegate = DrawSubgraph;
            subgraph.NodeBoundaryDelegate = GetSubgraphBoundary;
            subgraph.UserData = workFlowBlock;
            subgraph.LabelText = workFlowBlock.Name;
  
            parentSubgraph.AddSubgraph(subgraph);
            
            IDictionary<Key, Node> ht = new Dictionary<Key, Node>();

			foreach(IWorkflowStep step in workFlowBlock.ChildItemsByType(AbstractWorkflowStep.ItemTypeConst))
			{
				if (!(step is IWorkflowBlock subBlock))
                {
                    Node shape = AddNode(step, subgraph);
                    ht.Add(step.PrimaryKey, shape);
                }
                else
                {
                    Node shape = AddWorkflowDiagram(subBlock, subgraph);
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
		
		private ICurve GetSubgraphBoundary(Node node) 
		{
			Subgraph subgraph = (Subgraph) node;
			if (!subgraph.Nodes.Any())
			{
				return CurveFactory.CreateRectangle(emptySubgraphWidth, emptySubgraphHeight, new Point());
			}
			
			var clusterBoundary = ((Cluster) node.GeometryNode).RectangularBoundary;

			var height = clusterBoundary.TopMargin;
			var labelWidth = GetLabelWidth(node);

			var width = clusterBoundary.MinWidth > labelWidth
				? clusterBoundary.MinWidth 
				: labelWidth + labelSideMargin * 2;

			return CurveFactory.CreateRectangle(width, height, new Point());
		}
		
		private float GetLabelWidth(Node node)
		{
			Image image = GetImage(node);
			SizeF stringSize = graphics.MeasureString(node.LabelText, font);
			var labelWidth = stringSize.Width + imageRightMargin + image.Width;
			return labelWidth;
		}
		
		private bool DrawHiddenSubgraph(Node node, object graphicsObj)
		{
			return true;
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

			var labelWidth = GetLabelWidth(node);

			double centerX = node.GeometryNode.Center.X;
			double centerY = node.GeometryNode.Center.Y;
			var borderCorner = new System.Drawing.Point(
				(int)centerX - borderSize.Width / 2,
				(int)centerY - borderSize.Height / 2);
			Rectangle border = new Rectangle(borderCorner, borderSize);

			var labelPoint = new PointF(
				(float)(centerX - labelWidth / 2 + imageLeftMargin + image.Width +imageRightMargin),
				(float)centerY - border.Height / 2.0f + labelTopMargin);

			var imagePoint = new PointF(
				(float)(centerX - labelWidth / 2 + imageLeftMargin),
				(float)(centerY - border.Height / 2.0f + imageTopMargin));

			Rectangle imageBackground = new Rectangle(
				borderCorner,
				new Size(border.Width, headingBackgroundHeight));

			var (emptyMessagePoint, emptyGraphMessage) = GetEmptyNodeMessage(node);
			
			editorGraphics.DrawUpSideDown(drawAction: graphics =>
				{
					graphics.FillRectangle(greyBrush, imageBackground);
					graphics.DrawString(node.LabelText, font, drawBrush,
						labelPoint, drawFormat);
					if (!string.IsNullOrWhiteSpace(emptyGraphMessage))
					{
						graphics.DrawString(emptyGraphMessage, font, drawBrush,
							emptyMessagePoint, drawFormat);
					}
					graphics.DrawRectangle(pen, border);
					graphics.DrawImage(image, imagePoint);
				},
				yAxisCoordinate: (float)node.GeometryNode.Center.Y);

			return true;
		}

		private Tuple<PointF, string> GetEmptyNodeMessage(Node node)
		{
			Subgraph subgraph = (Subgraph) node;
			double centerX = node.GeometryNode.Center.X;
			double centerY = node.GeometryNode.Center.Y;
			
			if (subgraph.Nodes.Any() || subgraph.Subgraphs.Any())
			{
				return new Tuple<PointF, string>(new PointF(), "");
			}

			string emptyGraphMessage = "Right click here to add steps";
			SizeF messageSize = graphics.MeasureString(emptyGraphMessage, font);
			var emptyMessagePoint = new PointF(
				(float)centerX -  messageSize.Width / 2,
				(float)centerY + headingBackgroundHeight / 2 - messageSize.Height / 2 );
			
			return new Tuple<PointF, string>(emptyMessagePoint, emptyGraphMessage);
		}
	}
}
