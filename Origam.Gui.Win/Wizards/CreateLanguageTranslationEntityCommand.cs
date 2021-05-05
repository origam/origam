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
using Origam.Services;
using Origam.UI;
using Origam.UI.WizardForm;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Gui.Win.Wizards
{
    class CreateLanguageTranslationEntityCommand : AbstractMenuCommand
	{
        SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        ScreenWizardForm wizardForm;
        public override bool IsEnabled
		{
			get
			{				
				return Owner is TableMappingItem;
			}
			set
			{
				throw new ArgumentException("Cannot set this property", "IsEnabled");				
			}
		}


		public override void Run()
		{
            ArrayList list = new ArrayList();
            TableMappingItem mappingItem = new TableMappingItem();
            list.Add(new ListViewItem(mappingItem.GetType().SchemaItemDescription().Name, mappingItem.Icon));

            Stack stackPage = new Stack();
            stackPage.Push(PagesList.Finish);
            stackPage.Push(PagesList.SummaryPage);
            stackPage.Push(PagesList.ScreenForm);
            stackPage.Push(PagesList.StartPage);

            wizardForm = new ScreenWizardForm
            {
                ItemTypeList = list,
                Title = ResourceUtils.GetString("CreateLanguageTranslationEntityWizardTitle"),
                PageTitle = "",
                Description = ResourceUtils.GetString("CreateLanguageTranslationEntityWizardDescription"),
                Pages = stackPage,
                Entity = Owner as TableMappingItem,
                IsRoleVisible = false,
                textColumnsOnly = true,
                ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
                Command = this
            };
            Wizard wiz = new Wizard(wizardForm);
			if (wiz.ShowDialog() != DialogResult.OK)
            {
                GeneratedModelElements.Clear();
            }
        }
        public override void Execute()
        {
            List<AbstractSchemaItem> generatedElements = new List<AbstractSchemaItem>();
            var table = EntityHelper.CreateLanguageTranslationChildEntity(
                wizardForm.Entity as TableMappingItem, wizardForm.SelectedFields, generatedElements);
            foreach (var item in generatedElements)
            {
                GeneratedModelElements.Add(item);
            }
            var script = CreateTableScript(
                table.Name, table.Id);
            GeneratedModelElements.Add(script);
        }
        public override int GetImageIndex(string icon)
        {
            return _schemaBrowser.ImageIndex(icon);
        }
        public override void SetSummaryText(object summary)
        {
            RichTextBox richTextBoxSummary = (RichTextBox)summary;
            richTextBoxSummary.Text = ResourceUtils.GetString("CreateLanguageTranslationEntityWizardDescription") + " with this parameters:";
            richTextBoxSummary.AppendText(Environment.NewLine);
            richTextBoxSummary.AppendText(Environment.NewLine);
            richTextBoxSummary.AppendText("Language Entity: \t");
            richTextBoxSummary.AppendText(string.Format("{0}_l10n", (wizardForm.Entity as TableMappingItem).Name));
        }
    }
}