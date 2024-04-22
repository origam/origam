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
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Commands;
using Origam.Workbench.Diagram.DiagramFactory;
using Origam.Workbench.Diagram.NodeDrawing;
using DrawingNode = Microsoft.Msagl.Drawing.Node;

namespace Origam.Workbench.Diagram.InternalEditor;

public class GeneralDiagramEditor<T>: IDiagramEditor where T: ISchemaItem
{
    private readonly IPersistenceProvider persistenceProvider;

    public GeneralDiagramEditor(GViewer gViewer, T schemaItem,
        IDiagramFactory<T, Graph> factory,
        IPersistenceProvider persistenceProvider)
    {
            this.persistenceProvider = persistenceProvider;
            gViewer.Graph = factory.Draw(schemaItem);
            gViewer.EdgeInsertButtonVisible = false;
            gViewer.DoubleClick += GViewerOnDoubleClick;
        }

    private void GViewerOnDoubleClick(object sender, EventArgs e)
    {
            GViewer viewer = sender as GViewer;
            if (viewer.SelectedObject is DrawingNode node)
            {
                Guid schemaId = IdTranslator.ToSchemaId(node);
                if (schemaId == Guid.Empty) return;
                AbstractSchemaItem clickedItem = 
                    (AbstractSchemaItem)persistenceProvider
                        .RetrieveInstance(typeof(AbstractSchemaItem), new Key(schemaId));
                if(clickedItem != null)
                {
                    EditSchemaItem cmd = new EditSchemaItem
                    {
                        ShowDialog = true,
                        Owner = clickedItem
                    };
                    cmd.Run();
                }
            }
        }
        
    public void Dispose()
    {
        }

    public void ReDrawAndKeepFocus()
    {
            
        }
}