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

using System;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace Origam.Workbench.Diagram.InternalEditor;
class EdgeInsertionRule: IDisposable
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
    public void Dispose()
    {
        viewerToImposeOn.MouseDown -= OnMouseDown;
        viewerToImposeOn.MouseUp -= OnMouseUp;
    }
}
