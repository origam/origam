using System;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace Origam.Workbench.Editors
{
    class EdgeInsertionRule
    {
        private readonly GViewer viewerToImposeOn;
        private readonly Func<Node, Node, bool> predicate;
        private Node nodeWhenMouseDown;

        public EdgeInsertionRule(GViewer viewerToImposeOn, Func<Node, Node, bool> predicate)
        {
            this.viewerToImposeOn = viewerToImposeOn;
            this.predicate = predicate;

            viewerToImposeOn.MouseDown += OnMouseDown;
            viewerToImposeOn.MouseUp += OnMouseUp;
        }

        private void OnMouseUp(object sender, MouseEventArgs args)
        {
            if (viewerToImposeOn.LayoutEditor.InsertingEdge)
            {
                var targetNode = GetNodeUnderMouse(viewerToImposeOn, args);
                var insertForbidden = !predicate(nodeWhenMouseDown, targetNode);
                if (insertForbidden)
                {
                    CancelEdgeInsertion(viewerToImposeOn);
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs args)
        {
            nodeWhenMouseDown = GetNodeUnderMouse(viewerToImposeOn, args);
        }

        private static void CancelEdgeInsertion(GViewer gViewer)
        {
            gViewer.StopDrawingRubberLine();
            gViewer.RemoveSourcePortEdgeRouting();
            gViewer.RemoveTargetPortEdgeRouting();
            gViewer.LayoutEditor.InsertingEdge = false;
        }

        private static Node GetNodeUnderMouse(GViewer gViewer, MouseEventArgs args)
        {
            var point = new System.Drawing.Point(args.X, args.Y);
            return (gViewer.GetObjectAt(point) as DNode)?.Node;
        }
    }
}