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
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;

namespace Origam.Gui.Win.Wizards;

class CreateLanguageTranslationEntityCommand : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser =
        WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser)) as SchemaBrowser;
    ScreenWizardForm wizardForm;
    public override bool IsEnabled
    {
        get { return Owner is TableMappingItem; }
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
        var list = new List<ListViewItem>();
        TableMappingItem mappingItem = new TableMappingItem();
        list.Add(
            item: new ListViewItem(
                text: mappingItem.GetType().SchemaItemDescription().Name,
                imageKey: mappingItem.Icon
            )
        );
        Stack stackPage = new Stack();
        stackPage.Push(obj: PagesList.Finish);
        stackPage.Push(obj: PagesList.SummaryPage);
        stackPage.Push(obj: PagesList.ScreenForm);
        stackPage.Push(obj: PagesList.StartPage);
        wizardForm = new ScreenWizardForm
        {
            ItemTypeList = list,
            Title = ResourceUtils.GetString(key: "CreateLanguageTranslationEntityWizardTitle"),
            PageTitle = "",
            Description = ResourceUtils.GetString(
                key: "CreateLanguageTranslationEntityWizardDescription"
            ),
            Pages = stackPage,
            Entity = Owner as TableMappingItem,
            IsRoleVisible = false,
            textColumnsOnly = true,
            ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
            Command = this,
        };
        Wizard wiz = new Wizard(objectForm: wizardForm);
        if (wiz.ShowDialog() != DialogResult.OK)
        {
            GeneratedModelElements.Clear();
        }
    }

    public override void Execute()
    {
        List<ISchemaItem> generatedElements = new List<ISchemaItem>();
        var table = EntityHelper.CreateLanguageTranslationChildEntity(
            parentEntity: wizardForm.Entity as TableMappingItem,
            selectedFields: wizardForm.SelectedFields,
            generatedElements: generatedElements
        );
        foreach (var item in generatedElements)
        {
            GeneratedModelElements.Add(item: item);
        }
        var script = CreateTableScript(name: table.Name, guid: table.Id);
        GeneratedModelElements.Add(item: script);
    }

    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon: icon);
    }

    public override void SetSummaryText(object summary)
    {
        RichTextBox richTextBoxSummary = (RichTextBox)summary;
        richTextBoxSummary.Text =
            ResourceUtils.GetString(key: "CreateLanguageTranslationEntityWizardDescription")
            + " with this parameters:";
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: Environment.NewLine);
        richTextBoxSummary.AppendText(text: "Language Entity: \t");
        richTextBoxSummary.AppendText(
            text: string.Format(
                format: "{0}_l10n",
                arg0: (wizardForm.Entity as TableMappingItem).Name
            )
        );
    }
}
