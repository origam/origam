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
using System.IO;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace OrigamArchitect.Commands;

public class GenerateXamlCommand : AbstractMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;
    public override bool IsEnabled
    {
        get { return _schema.ActiveNode is Origam.Schema.MenuModel.Menu; }
        set
        {
            throw new ArgumentException(
                message: "Cannot set this property",
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        SaveFileDialog dialog = new SaveFileDialog();
        dialog.DefaultExt = "xml";
        dialog.FileName = "menu.xml";
        if (
            dialog.ShowDialog(owner: WorkbenchSingleton.Workbench as IWin32Window)
            == DialogResult.OK
        )
        {
            Origam.Schema.MenuModel.Menu item = _schema.ActiveNode as Origam.Schema.MenuModel.Menu;
            Origam
                .OrigamEngine.ModelXmlBuilders.MenuXmlBuilder.GetXml(menu: item)
                .Save(filename: dialog.FileName);
            string path = Path.GetDirectoryName(path: dialog.FileName);
            foreach (ISchemaItem child in item.ChildItemsRecursive)
            {
                FormReferenceMenuItem formMenu = child as FormReferenceMenuItem;
                if (formMenu != null)
                {
                    FormControlSet form = formMenu.Screen;
                    string formPath = Path.Combine(
                        path1: path,
                        path2: formMenu.Name + "_" + formMenu.Id.ToString() + ".xml"
                    );
                    Origam
                        .OrigamEngine.ModelXmlBuilders.FormXmlBuilder.GetXml(
                            item: form,
                            name: formMenu.DisplayName,
                            isPreloaded: formMenu.ListDataStructure == null,
                            menuId: formMenu.Id,
                            structure: form.DataStructure,
                            forceReadOnly: formMenu.ReadOnlyAccess,
                            confirmSelectionChangeEntity: formMenu.SelectionChangeEntity
                        )
                        .Document.Save(filename: formPath);
                }
            }
        }
    }
}
