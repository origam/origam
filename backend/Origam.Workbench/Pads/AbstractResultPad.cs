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
using Origam.Schema;
using Origam.Workbench.Commands;
using Origam.Workbench.Services;
using System;
using System.Windows.Forms;

namespace Origam.Workbench.Pads;
public class AbstractResultPad : AbstractPadContent
{
    public bool OpenParentPackage(Guid SchemaExtensionId)
    {
        Guid SchemaExtensionIdItem = SchemaExtensionId;
        TreeNode treenode = (WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser).EbrSchemaBrowser.GetFirstNode();
        if (treenode != null)
        {
            if (!((Package)treenode.Tag).Id.Equals(SchemaExtensionIdItem))
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to change the Package?", "Package change", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        LoadSchema(treenode,SchemaExtensionIdItem);
        return true;
    }
    private void LoadSchema(TreeNode treenode, Guid SchemaExtensionIdItem)
    {
        SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
        if (treenode != null)
        {
            schema.UnloadSchema();
        }
        if (treenode == null || !((Package)treenode.Tag).Id.Equals(SchemaExtensionIdItem))
        {
            foreach (Package sch in schema.AllPackages)
            {
                if (sch.Id.Equals(SchemaExtensionIdItem))
                {
                    schema.LoadSchema(sch.Id);
                    ViewSchemaBrowserPad cmd = new ViewSchemaBrowserPad();
                    cmd.Run();
                }
            }
        }
    }
}
