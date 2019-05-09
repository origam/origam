using System.Drawing;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Origam.Schema;

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

        public readonly SolidBrush DrawBrush = new SolidBrush(System.Drawing.Color.Black);
        public readonly StringFormat DrawFormat = new StringFormat();
        private readonly Graphics measurementGraphics = new Control().CreateGraphics();
        private Pen BoldBlackPen = new Pen(System.Drawing.Color.Black, 2);
        private Pen BlackPen = new Pen(System.Drawing.Color.Black, 1);

        public readonly SolidBrush GreyBrush = new SolidBrush(System.Drawing.Color.LightGray);

        public readonly int NodeHeight = 25;

        internal INodeSelector NodeSelector { get; }

        public InternalPainter(INodeSelector nodeSelector)
        {
            this.NodeSelector = nodeSelector;
        }

        internal Image GetImage(Node node)
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
        
        internal Pen GetActiveBorderPen(Node node)
        {
            return NodeSelector.Selected == node
                ? BoldBlackPen
                : BlackPen;
        }

        internal Size CalculateBorderSize(Node node)
        {
            SizeF stringSize =
                measurementGraphics.MeasureString(node.LabelText, Font);

            int totalWidth = (int) (Margin + NodeHeight + TextSideMargin +
                                    stringSize.Width + TextSideMargin);
            return new Size(totalWidth, NodeHeight);
        }

        internal SizeF MeasureString(string nodeLabelText)
        {
            return  measurementGraphics.MeasureString(nodeLabelText, Font);
        }

        internal float GetLabelWidth(Node node)
        {
            Image image = GetImage(node);
            SizeF stringSize = MeasureString(node.LabelText);
            var labelWidth = stringSize.Width + ImageRightMargin + image.Width;
            return labelWidth;
        }
    }
}