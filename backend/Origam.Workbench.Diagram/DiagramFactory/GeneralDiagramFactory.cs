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

using Microsoft.Msagl.Drawing;
using Origam.Schema;

namespace Origam.Workbench.Diagram.DiagramFactory;
class GeneralDiagramFactory: IDiagramFactory<ISchemaItem, Graph>
{
    private Graph graph;
	
    public Graph Draw(ISchemaItem item)
    {
        graph = new Graph();
        DrawUniShape(item, null);
        return graph;
    }
    private void DrawUniShape(ISchemaItem schemaItem, Node parentShape)
    {
        Node shape = this.AddNode(schemaItem.Id.ToString(), schemaItem.Name);
        if(parentShape != null)
        {
            this.graph.AddEdge(shape.Id, parentShape.Id);
        }
        foreach(ISchemaItem child in schemaItem.ChildItems)
        {
            DrawUniShape(child, shape);
        }
    }
    private Node AddNode(string id, string label)
    {
        Node shape = graph.AddNode(id);
        shape.LabelText = label;
        return shape;
    }
}
