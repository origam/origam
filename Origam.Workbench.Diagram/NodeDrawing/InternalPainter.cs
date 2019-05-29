using System.Drawing;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Diagram.Graphs;

namespace Origam.Workbench.Diagram.NodeDrawing
{
    internal class InternalPainter
    {
        public readonly int LabelTopMargin = 8;
        public readonly double LabelSideMargin = 20;

        public readonly int ImageTopMargin = 8;
        public readonly int ImageRightMargin = 3;
        public readonly int ImageLeftMargin = 5;

        public readonly int HeadingBackgroundHeight = 30;

        public readonly int Margin = 3;
        public readonly int TextSideMargin = 15;
        public readonly Font Font = new Font("Arial", 10);

        public readonly StringFormat DrawFormat = new StringFormat();
        private readonly Graphics measurementGraphics = new Control().CreateGraphics();
        private Pen BoldBlackPen = new Pen(System.Drawing.Color.Black, 2);
        public Pen BlackPen { get; } = new Pen(System.Drawing.Color.Black, 1);

        public readonly SolidBrush BlackBrush = new SolidBrush(System.Drawing.Color.Black);
        public readonly SolidBrush GreyBrush = new SolidBrush(System.Drawing.Color.LightGray);
        public readonly SolidBrush GreenBrush = new SolidBrush(System.Drawing.Color.LimeGreen);
        public readonly SolidBrush RedBrush = new SolidBrush(System.Drawing.Color.Red);
        public readonly Brush WhiteBrush  = new SolidBrush(System.Drawing.Color.White);

        public readonly int NodeHeaderHeight = 25;

        private readonly GViewer gViewer;

        internal INodeSelector NodeSelector { get; }

        public InternalPainter(INodeSelector nodeSelector, GViewer gViewer)
        {
            NodeSelector = nodeSelector;
            this.gViewer = gViewer;
        }

        private Image GetImage(string iconId)
        {
            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as
                    IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            return imageList.Images[schemaBrowser.ImageIndex(iconId)];
        }
        
        internal NodeImages GetImages(Node node)
        {
            var schemaItem = (ISchemaItem) node.UserData;
            
            Image primaryImage = GetImage(schemaItem.Icon);

            Image secondaryImage = null;
            if (schemaItem is AbstractWorkflowStep workflowStep
                && workflowStep.StartConditionRule != null)
            {
                secondaryImage = GetImage(workflowStep.StartConditionRule.Icon);
            }

            return new NodeImages
            {
                Primary = primaryImage,
                Secondary = secondaryImage
            };
        }
        
        internal Pen GetActiveBorderPen(Node node)
        {
            bool markAsSelected =
                Equals(NodeSelector.Selected, node) ||
                node is IWorkflowSubgraph subgraph && 
                NodeSelector.Selected is IWorkflowSubgraph selectedSubgraph &&
                Equals(subgraph.WorkflowItemId, selectedSubgraph.WorkflowItemId);
            return markAsSelected
                ? BoldBlackPen
                : BlackPen;
        }

        internal Size CalculateMinHeaderBorder(Node node)
        {
            SizeF stringSize =
                measurementGraphics.MeasureString(node.LabelText, Font);

            int totalWidth = (int) (Margin + NodeHeaderHeight + TextSideMargin +
                                    stringSize.Width + TextSideMargin);
            if(GetImages(node).Secondary != null) totalWidth += NodeHeaderHeight;
            return new Size(totalWidth, NodeHeaderHeight);
        }

        internal SizeF MeasureString(string nodeLabelText)
        {
            return  measurementGraphics.MeasureString(nodeLabelText, Font);
        }

        internal float GetLabelWidth(Node node)
        {
            Image image = GetImages(node).Primary;
            SizeF stringSize = MeasureString(node.LabelText);
            var labelWidth = stringSize.Width + ImageRightMargin + image.Width;
            return labelWidth;
        }
    }

    class NodeImages
    {
        public Image Primary { get; set; }
        public Image Secondary { get; set; }
    }
}