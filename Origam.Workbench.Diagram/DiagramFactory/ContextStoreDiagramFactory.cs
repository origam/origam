using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Extensions;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using Rectangle = System.Drawing.Rectangle;
using DrawPoint = System.Drawing.Point;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace Origam.Workbench.Diagram.DiagramFactory
{
    class ContextStoreDiagramFactory: IDiagramFactory<IContextStore>, IDisposable
    {

        private Graph graph;
        private readonly IPersistenceProvider persistenceProvider;

        private readonly int margin = 3;
        private readonly int marginLeft = 5;
        private readonly Font font = new Font("Arial", 12);
        private readonly Pen blackPen = new Pen(System.Drawing.Color.Black, 1);
        private readonly SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
        private readonly StringFormat drawFormat = new StringFormat();
        private readonly Graphics parentGraphics;

        public ContextStoreDiagramFactory(IPersistenceProvider persistenceProvider, Control parentControl)
        {
            this.persistenceProvider = persistenceProvider;
            parentGraphics = parentControl.CreateGraphics();
        }

        public Graph Draw(IContextStore contextStore)
        {
            graph = new Graph();
            
            Node storeNode = AddNode(contextStore);
            List<IWorkflowStep> steps = persistenceProvider
                .RetrieveList<IWorkflowStep>();
            
            foreach (IWorkflowStep step in steps)
            {
                if (step is WorkflowTask task &&
                    task.OutputContextStoreId == contextStore.Id)
                {
                    Node taskNode = AddNode(task);
                    graph.AddEdge(storeNode.Id, taskNode.Id);
                }else if (step is UpdateContextTask updateTask &&
                          updateTask.XPathContextStore.Id == contextStore.Id)
                {
                    Node taskNode = AddNode(updateTask);
                    graph.AddEdge(taskNode.Id, storeNode.Id);
                }
            }

            return graph;
        }

        private Node AddNode(ISchemaItem schemaItem)
        {
            Node node = graph.AddNode(schemaItem.Id.ToString());
            node.Attr.Shape = Shape.DrawFromGeometry;
            node.DrawNodeDelegate = DrawNode;
            node.NodeBoundaryDelegate = GetNodeBoundary;
            node.UserData = schemaItem;
            node.LabelText = schemaItem.Name;
            return node;
        }
 
        private ICurve GetNodeBoundary(Node node) {
            var image = GetImage(node);
            var borderSize = CalculateBorderSize(node, image);
            return CurveFactory.CreateRectangle(borderSize.Width, borderSize.Height, new MsaglPoint());
        }

        private bool DrawNode(Node node, object graphicsObj) {
            Graphics editorGraphics = (Graphics)graphicsObj;
            var image = GetImage(node);

            SizeF stringSize = editorGraphics.MeasureString(node.LabelText, font);

            var borderSize = CalculateBorderSize(node, image);
            var borderCorner = new DrawPoint(
                (int)node.GeometryNode.Center.X - borderSize.Width / 2,
                (int)node.GeometryNode.Center.Y - borderSize.Height / 2);
            Rectangle border = new Rectangle(borderCorner, borderSize);

            var labelPoint = new PointF(
                (float)node.GeometryNode.Center.X - (float)border.Width / 2 + image.Width + margin * 2,
                (float)node.GeometryNode.Center.Y - stringSize.Height / 2);

            var imagePoint = new PointF(
                (float)(node.GeometryNode.Center.X - (float)border.Width / 2 + marginLeft),
                (float)(node.GeometryNode.Center.Y - (float)image.Height / 2));

            editorGraphics.DrawUpSideDown(drawAction: graphics =>
                {
                    graphics.DrawString(node.LabelText, font, drawBrush,
                        labelPoint, drawFormat);
                    graphics.DrawRectangle(blackPen, border);
                    graphics.DrawImage(image, imagePoint);
                }, 
                yAxisCoordinate: (float)node.GeometryNode.Center.Y);
            
            return true;
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

        private Size CalculateBorderSize(Node node, Image image)
        {
            SizeF stringSize = parentGraphics.MeasureString(node.LabelText, font);

            int totalWidth = (int) (margin + stringSize.Width + margin + image.Width + margin);
            int totalHeight = stringSize.Height > image.Height
                ? (int) stringSize.Height + margin * 2
                : image.Height + margin * 2;
           return  new Size(totalWidth, totalHeight);
        }

        public void Dispose()
        {
            font?.Dispose();
            blackPen?.Dispose();
            drawBrush?.Dispose();
            drawFormat?.Dispose();
            parentGraphics?.Dispose();
        }
    }
}