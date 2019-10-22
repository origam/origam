#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
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
        ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
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

            PanelSchemaItemProvider dsprovider = schema.GetProvider(typeof(PanelSchemaItemProvider)) as PanelSchemaItemProvider;
            List<string> listdsName = dsprovider.ChildItemsByType(PanelControlSet.ItemTypeConst)
                            .ToArray()
                            .Select(x => { return ((AbstractSchemaItem)x).Name; })
                            .ToList();

            ArrayList list = new ArrayList();
            TableMappingItem mappingItem = new TableMappingItem();
            list.Add(new ListViewItem(mappingItem.ItemType, mappingItem.Icon));

            Stack stackPage = new Stack();
            stackPage.Push(PagesList.finish);
            stackPage.Push(PagesList.ScreenForm);
            stackPage.Push(PagesList.startPage);

            wizardForm = new ScreenWizardForm
            {
                ItemTypeList = list,
                Title = "Create Language Translation Entity Wizard",
                PageTitle = "",
                Description = "Create Some Description.",
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
    }
}