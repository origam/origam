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

using System.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace Origam.Workbench.Diagram.Extensions;

public static class GViewerExtensions
{
    public static IViewerNode FindViewerNode(this GViewer gViewer, Node node)
    {
        return gViewer
            .Entities.OfType<IViewerNode>()
            .SingleOrDefault(viewerNode => Equals(viewerNode.Node, node));
    }

    public static void Redraw(this GViewer gViewer)
    {
        var graph = gViewer.Graph;
        gViewer.Graph = null;
        gViewer.Graph = graph;
    }
}
