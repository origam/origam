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
using Origam.Services;
using Origam.UI;
using Origam.UI.Interfaces;
using Origam.UI.WizardForm;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.Wizards
{
    /// <summary>
    /// Summary description for CreateDataStructureFromEntityCommand.
    /// </summary>
    public class CreateDataStructureFromEntityCommand : AbstractMenuCommand
	{
        ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
        SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        StructureForm structureForm;
       

        public override bool IsEnabled
		{
			get
			{
				return Owner is IDataEntity;
			}
			set
			{
				throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
			}
		}

        public override void Run()
        {
            DataStructureSchemaItemProvider dsprovider = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
            List<string> listdsName = dsprovider.ChildItemsByType(DataStructure.ItemTypeConst)
                            .ToArray()
                            .Select(x => { return ((AbstractSchemaItem)x).Name; })
                            .ToList();

            IDataEntity entity = Owner as IDataEntity;

            ArrayList list = new ArrayList();
            DataStructure dd = new DataStructure();
            list.Add(new ListViewItem(dd.ItemType, dd.Icon));

            Stack stackPage = new Stack();

            stackPage.Push(PagesList.finish);
            if (listdsName.Any(name => name == entity.Name))
            {
                stackPage.Push(PagesList.StructureNamePage);
            }

            stackPage.Push(PagesList.startPage);

            structureForm = new StructureForm
            {
                Description = "Create Data Structure Wizard",
                ItemTypeList = list,
                Pages = stackPage,
                StructureList = listdsName,
                NameOfEntity = entity.Name,
                ImageList = _schemaBrowser.EbrSchemaBrowser.imgList,
                Command = this
            };

            Wizard wizardscreen = new Wizard(structureForm);
            if (wizardscreen.ShowDialog() != DialogResult.OK)
            { 
                GeneratedModelElements.Clear();
            }
		}

        public override void Execute()
        {
            DataStructure ds = EntityHelper.CreateDataStructure(Owner as IDataEntity, structureForm.NameOfEntity, true);
            GeneratedModelElements.Add(ds);
        }
	}
}
