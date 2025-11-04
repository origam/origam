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

using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.Workbench.Diagram.Extensions;

namespace Origam.Workbench.Diagram.InternalEditor;

class NodePositionTracker
{
    private readonly GViewer gViewer;
    private readonly PlaneTransformation originalTransformation;
    private readonly string nodeId;
    private readonly Point pointOnScreen;
    private readonly Node originalNode;
    private Node updatedNode;
    private Point CurrentSourcePoint => updatedNode.GeometryNode.Center;
    public bool NodeExists => !string.IsNullOrWhiteSpace(nodeId) && updatedNode != null;

    public PlaneTransformation UpdatedTransformation
    {
        get
        {
            if (originalTransformation == null)
            {
                return null;
            }

            return new PlaneTransformation(
                originalTransformation[0, 0],
                originalTransformation[0, 1],
                pointOnScreen.X - (CurrentSourcePoint.X * originalTransformation[0, 0]),
                originalTransformation[1, 0],
                originalTransformation[1, 1],
                pointOnScreen.Y + (CurrentSourcePoint.Y * originalTransformation[0, 0])
            );
        }
    }

    public NodePositionTracker(GViewer gViewer, string nodeId)
    {
        this.gViewer = gViewer;
        this.nodeId = nodeId;
        originalNode = gViewer.Graph.FindNodeOrSubgraph(nodeId);
        if (originalNode == null)
        {
            return;
        }

        originalTransformation = gViewer.Transform;
        if (string.IsNullOrWhiteSpace(this.nodeId))
        {
            return;
        }

        pointOnScreen = gViewer.Transform * originalNode.GeometryNode.Center;
    }

    public void LoadUpdatedState()
    {
        updatedNode = gViewer.Graph.FindNodeOrSubgraph(nodeId);
    }
}
