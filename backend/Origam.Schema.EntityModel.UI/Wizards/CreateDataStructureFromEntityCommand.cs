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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Schema.EntityModel.UI.Wizards;

/// <summary>
/// Summary description for CreateDataStructureFromEntityCommand.
/// </summary>
public class CreateDataStructureFromEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    StructureForm structureForm;

    public override bool IsEnabled
    {
        get { return Owner is IDataEntity; }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }

    public override void Run()
    {
        List<string> listdsName = GetListDatastructure(itemTypeConst: DataStructure.CategoryConst);
        IDataEntity entity = Owner as IDataEntity;
        var list = new List<ListViewItem>();
        DataStructure dd = new DataStructure();
        list.Add(
            item: new ListViewItem(
                text: dd.GetType().SchemaItemDescription().Name,
                imageKey: dd.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        if (listdsName.Any(predicate: name => name == entity.Name))
        {
            stackPage.Push(obj: PagesList.StructureNamePage);
        }
        stackPage.Push(obj: PagesList.StartPage);
        structureForm = new StructureForm
        {
            Title = ResourceUtils.GetString(key: "CreateDataStructureFromEntityWizardTitle"),
            Description = ResourceUtils.GetString(
                key: "CreateDataStructureFromEntityWizardDescription"
            ),
            ItemTypeList = list,
            Pages = stackPage,
            StructureList = listdsName,
            NameOfEntity = entity.Name,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wizardscreen = new Wizard(objectForm: structureForm);
        if (wizardscreen.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        DataStructure ds = EntityHelper.CreateDataStructure(
            entity: Owner as IDataEntity,
            name: structureForm.NameOfEntity,
            persist: true
        );
        GeneratedModelElements.Add(item: ds);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text = "";
        richTextBoxSummary.AppendText(text: "");
        richTextBoxSummary.AppendText(text: "Create Data Structure: ");
        richTextBoxSummary.SelectionFont = new Font(
            prototype: richTextBoxSummary.Font,
            newStyle: FontStyle.Bold
        );
        richTextBoxSummary.AppendText(text: structureForm.NameOfEntity);
        richTextBoxSummary.AppendText(text: "");
    }
}
