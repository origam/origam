using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;
using Origam.Workbench.Diagram.Extensions;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.DiagramFactory
{
    class NodeFactory
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

        private static readonly int margin = 3;
        private static readonly int textSideMargin = 15;
        private static readonly Font font = new Font("Arial", 10);

        private static readonly SolidBrush drawBrush =
            new SolidBrush(System.Drawing.Color.Black);

        private static readonly StringFormat drawFormat = new StringFormat();

        private static readonly Graphics measurementGraphics =
            new Control().CreateGraphics();

        private static readonly Pen boldBlackPen =
            new Pen(System.Drawing.Color.Black, 2);

        private static readonly Pen blackPen =
            new Pen(System.Drawing.Color.Black, 1);

        private static readonly SolidBrush greyBrush =
            new SolidBrush(System.Drawing.Color.LightGray);

        private static readonly int nodeHeight = 25;
        private readonly INodeSelector nodeSelector;
        

        public NodeFactory(INodeSelector nodeSelector)
        {
            this.nodeSelector = nodeSelector;
        }

        public Node AddNode(Graph graph, ISchemaItem schemaItem)
        {
            Node node = graph.AddNode(schemaItem.Id.ToString());
            node.Attr.Shape = Shape.DrawFromGeometry;
            node.DrawNodeDelegate = DrawNode;
            node.NodeBoundaryDelegate = GetNodeBoundary;
            node.UserData = schemaItem;
            node.LabelText = schemaItem.Name;
            return node;
        }

        public Node AddNodeItem(Graph graph, ISchemaItem schemaItem)
        {
            Node node = graph.AddNode(schemaItem.Id.ToString());
            node.Attr.Shape = Shape.DrawFromGeometry;
            node.DrawNodeDelegate = DrawNodeItem;
            node.NodeBoundaryDelegate = GetNodeBoundary;
            node.UserData = schemaItem;
            node.LabelText = schemaItem.Name;
            return node;
        }

        public Subgraph AddSubgraphNode(Subgraph parentSbubgraph,
            ISchemaItem schemaItem)
        {
            Subgraph subgraph = new Subgraph(schemaItem.Id.ToString());
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            subgraph.DrawNodeDelegate = DrawSubgraphNode;
            subgraph.NodeBoundaryDelegate = GetSubgraphNodeBoundary;
            subgraph.UserData = schemaItem;
            subgraph.LabelText = schemaItem.Name;
            parentSbubgraph.AddSubgraph(subgraph);
            return subgraph;
        }

        public Subgraph AddSubgraph(Subgraph parentSbubgraph,
            ISchemaItem schemaItem)
        {
            Subgraph subgraph = new Subgraph(schemaItem.NodeId);
            subgraph.Attr.Shape = Shape.DrawFromGeometry;
            subgraph.DrawNodeDelegate = DrawSubgraph;
            subgraph.NodeBoundaryDelegate = GetSubgraphBoundary;
            subgraph.UserData = schemaItem;
            subgraph.LabelText = schemaItem.Name;
            parentSbubgraph.AddSubgraph(subgraph);
            return subgraph;
        }

        private ICurve GetSubgraphNodeBoundary(Node node)
        {
            Subgraph subgraph = (Subgraph) node;
            if (!subgraph.Nodes.Any())
            {
                return GetNodeBoundary(node);
            }

            var clusterBoundary =
                ((Cluster) node.GeometryNode).RectangularBoundary;

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
            SizeF stringSize = measurementGraphics.MeasureString(node.LabelText, font);
            var labelWidth = stringSize.Width + imageRightMargin + image.Width;
            return labelWidth;
        }

        private bool DrawSubgraphNode(Node node, object graphicsObj)
        {
            Subgraph subgraph = (Subgraph) node;
            if (!subgraph.Nodes.Any())
            {
                return DrawNode(node, graphicsObj);
            }

            var borderSize = new Size(
                (int) node.BoundingBox.Width,
                (int) node.BoundingBox.Height);

            Pen pen = nodeSelector.Selected == node
                ? boldBlackPen
                : blackPen;

            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = GetImage(node);

            double centerX = node.GeometryNode.Center.X;
            double centerY = node.GeometryNode.Center.Y;
            var borderCorner = new System.Drawing.Point(
                (int) centerX - borderSize.Width / 2,
                (int) centerY - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                nodeHeight + margin + textSideMargin,
                (float) centerY - border.Height / 2.0f + labelTopMargin);

            Rectangle imageBackground = new Rectangle(borderCorner,
                new Size(nodeHeight, nodeHeight));
            var imageHorizontalBorder =
                (imageBackground.Width - image.Width) / 2;
            var imageVerticalBorder =
                (imageBackground.Height - image.Height) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder),
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));


            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.FillRectangle(greyBrush, imageBackground);
                    graphics.DrawString(node.LabelText, font, drawBrush,
                        labelPoint, drawFormat);
                    graphics.DrawRectangle(pen, border);
                    graphics.DrawImage(image, imagePoint);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }

        private ICurve GetNodeBoundary(Node node)
        {
            var borderSize = CalculateBorderSize(node);
            return CurveFactory.CreateRectangle(borderSize.Width,
                borderSize.Height, new Point());
        }

         private bool DrawNodeItem(Node node, object graphicsObj)
        {
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = GetImage(node);

            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, font);

            var borderSize = CalculateBorderSize(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);
            Rectangle imageBackground = new Rectangle(borderCorner,
                new Size(nodeHeight, nodeHeight));

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                nodeHeight + textSideMargin,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);

            var imageHorizontalBorder =
                (imageBackground.Width - image.Width) / 2;
            var imageVerticalBorder =
                (imageBackground.Height - image.Height) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder),
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, font, drawBrush,
                        labelPoint, drawFormat);
                   // graphics.FillRectangle(greyBrush, imageBackground);
                    graphics.DrawImage(image, imagePoint);
                    //graphics.DrawRectangle(pen, border);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }
        
        private bool DrawNode(Node node, object graphicsObj)
        {
            Graphics editorGraphics = (Graphics) graphicsObj;
            var image = GetImage(node);

            Pen pen = nodeSelector.Selected == node
                ? boldBlackPen
                : blackPen;

            SizeF stringSize =
                editorGraphics.MeasureString(node.LabelText, font);

            var borderSize = CalculateBorderSize(node);
            var borderCorner = new System.Drawing.Point(
                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);
            Rectangle imageBackground = new Rectangle(borderCorner,
                new Size(nodeHeight, nodeHeight));

            var labelPoint = new PointF(
                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
                nodeHeight + margin + textSideMargin,
                (float) node.GeometryNode.Center.Y -
                (int) stringSize.Height / 2);

            var imageHorizontalBorder =
                (imageBackground.Width - image.Width) / 2;
            var imageVerticalBorder =
                (imageBackground.Height - image.Height) / 2;
            var imagePoint = new PointF(
                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
                         imageHorizontalBorder),
                (float) (node.GeometryNode.Center.Y -
                         (float) border.Height / 2 + imageVerticalBorder));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, font, drawBrush,
                        labelPoint, drawFormat);
                    graphics.FillRectangle(greyBrush, imageBackground);
                    graphics.DrawImage(image, imagePoint);
                    graphics.DrawRectangle(pen, border);
                },
                yAxisCoordinate: (float) node.GeometryNode.Center.Y);

            return true;
        }

        private static Image GetImage(Node node)
        {
            var schemaItem = (ISchemaItem) node.UserData;

            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as
                    IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            Image image =
                imageList.Images[schemaBrowser.ImageIndex(schemaItem.Icon)];
            return image;
        }

        private Size CalculateBorderSize(Node node)
        {
            SizeF stringSize =
                measurementGraphics.MeasureString(node.LabelText, font);

            int totalWidth = (int) (margin + nodeHeight + textSideMargin +
                                    stringSize.Width + textSideMargin);
            return new Size(totalWidth, nodeHeight);
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
			SizeF messageSize = measurementGraphics.MeasureString(emptyGraphMessage, font);
			var emptyMessagePoint = new PointF(
				(float)centerX -  messageSize.Width / 2,
				(float)centerY + headingBackgroundHeight / 2 - messageSize.Height / 2 );
			
			return new Tuple<PointF, string>(emptyMessagePoint, emptyGraphMessage);
		}
        
    }

//    internal class InternalPainter
//    {
//        public static readonly int LabelTopMargin = 8;
//        public static readonly double LabelSideMargin = 20;
//
//        public static readonly int ImageTopMargin = 8;
//        public static readonly int ImageRightMargin = 3;
//        public static readonly int ImageLeftMargin = 5;
//
//        public static readonly int HeadingBackgroundHeight = 30;
//        public static readonly int NodeMargin = 40;
//
//        public static readonly int EmptySubgraphWidth = 200;
//        public static readonly int EmptySubgraphHeight = 80;
//
//        public static readonly int margin = 3;
//        public static readonly int textSideMargin = 15;
//        public static readonly Font Font1 = new Font("Arial", 10);
//
//        public static readonly SolidBrush drawBrush =
//            new SolidBrush(System.Drawing.Color.Black);
//
//        public static readonly StringFormat drawFormat = new StringFormat();
//
//        private static readonly Graphics measurementGraphics =
//            new Control().CreateGraphics();
//
//        public static Pen BoldBlackPen { get; } =
//            new Pen(System.Drawing.Color.Black, 2);
//
//        public static Pen BlackPen { get; } =
//            new Pen(System.Drawing.Color.Black, 1);
//
//        public static readonly SolidBrush greyBrush =
//            new SolidBrush(System.Drawing.Color.LightGray);
//
//        public static readonly int nodeHeight = 25;
//        public INodeSelector NodeSelector { get; }
//
//        public InternalPainter(INodeSelector nodeSelector)
//        {
//            NodeSelector = nodeSelector;
//        }
//
//        internal Image GetImage(Node node)
//        {
//            var schemaItem = (ISchemaItem) node.UserData;
//
//            var schemaBrowser =
//                WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as
//                    IBrowserPad;
//            var imageList = schemaBrowser.ImageList;
//            Image image =
//                imageList.Images[schemaBrowser.ImageIndex(schemaItem.Icon)];
//            return image;
//        }
//
//        internal Size CalculateBorderSize(Node node)
//        {
//            SizeF stringSize =
//                measurementGraphics.MeasureString(node.LabelText, Font1);
//
//            int totalWidth = (int) (margin + nodeHeight + textSideMargin +
//                                    stringSize.Width + textSideMargin);
//            return new Size(totalWidth, nodeHeight);
//        }
//    }
//
//    class NodePainter
//    {
//        private readonly bool drawBorder;
//        private readonly InternalPainter internalPainter;
//
//        public NodePainter(INodeSelector nodeSelector, bool drawBorder)
//        {
//            this.drawBorder = drawBorder;
//            internalPainter =  new InternalPainter(nodeSelector);
//        }
//
//        internal bool DrawNode(Node node, object graphicsObj)
//        {
//            Graphics editorGraphics = (Graphics) graphicsObj;
//            var image = internalPainter.GetImage(node);
//
//            Pen pen = internalPainter.NodeSelector.Selected == node
//                ? InternalPainter.BoldBlackPen
//                : InternalPainter.BlackPen;
//
//            SizeF stringSize =
//                editorGraphics.MeasureString(node.LabelText, InternalPainter.Font1);
//
//            var borderSize = internalPainter.CalculateBorderSize(node);
//            var borderCorner = new System.Drawing.Point(
//                (int) node.GeometryNode.Center.X - borderSize.Width / 2,
//                (int) node.GeometryNode.Center.Y - borderSize.Height / 2);
//            Rectangle border = new Rectangle(borderCorner, borderSize);
//            Rectangle imageBackground = new Rectangle(borderCorner,
//                new Size(InternalPainter.nodeHeight, InternalPainter.nodeHeight));
//
//            var labelPoint = new PointF(
//                (float) node.GeometryNode.Center.X - (float) border.Width / 2 +
//                InternalPainter.nodeHeight + InternalPainter.margin + InternalPainter.textSideMargin,
//                (float) node.GeometryNode.Center.Y -
//                (int) stringSize.Height / 2);
//
//            var imageHorizontalBorder =
//                (imageBackground.Width - image.Width) / 2;
//            var imageVerticalBorder =
//                (imageBackground.Height - image.Height) / 2;
//            var imagePoint = new PointF(
//                (float) (node.GeometryNode.Center.X - (float) border.Width / 2 +
//                         imageHorizontalBorder),
//                (float) (node.GeometryNode.Center.Y -
//                         (float) border.Height / 2 + imageVerticalBorder));
//
//            editorGraphics.DrawUpSideDown(drawAction: graphics =>
//                {
//                    graphics.DrawString(node.LabelText, InternalPainter.Font1, InternalPainter.drawBrush,
//                        labelPoint, InternalPainter.drawFormat);
//                    graphics.FillRectangle(InternalPainter.greyBrush, imageBackground);
//                    graphics.DrawImage(image, imagePoint);
//                    if (drawBorder)
//                    {
//                        graphics.DrawRectangle(pen, border);
//                    }
//                },
//                yAxisCoordinate: (float) node.GeometryNode.Center.Y);
//
//            return true;
//        }
//    }
}