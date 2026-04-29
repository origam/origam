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
    public bool NodeExists => !string.IsNullOrWhiteSpace(value: nodeId) && updatedNode != null;

    public PlaneTransformation UpdatedTransformation
    {
        get
        {
            if (originalTransformation == null)
            {
                return null;
            }

            return new PlaneTransformation(
                matrixElement00: originalTransformation[rowIndex: 0, columnIndex: 0],
                matrixElement01: originalTransformation[rowIndex: 0, columnIndex: 1],
                matrixElement02: pointOnScreen.X
                    - (CurrentSourcePoint.X * originalTransformation[rowIndex: 0, columnIndex: 0]),
                matrixElement10: originalTransformation[rowIndex: 1, columnIndex: 0],
                matrixElement11: originalTransformation[rowIndex: 1, columnIndex: 1],
                matrixElement12: pointOnScreen.Y
                    + (CurrentSourcePoint.Y * originalTransformation[rowIndex: 0, columnIndex: 0])
            );
        }
    }

    public NodePositionTracker(GViewer gViewer, string nodeId)
    {
        this.gViewer = gViewer;
        this.nodeId = nodeId;
        originalNode = gViewer.Graph.FindNodeOrSubgraph(id: nodeId);
        if (originalNode == null)
        {
            return;
        }

        originalTransformation = gViewer.Transform;
        if (string.IsNullOrWhiteSpace(value: this.nodeId))
        {
            return;
        }

        pointOnScreen = gViewer.Transform * originalNode.GeometryNode.Center;
    }

    public void LoadUpdatedState()
    {
        updatedNode = gViewer.Graph.FindNodeOrSubgraph(id: nodeId);
    }
}
