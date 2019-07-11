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
using System.Windows.Forms;
using Origam.UI;
using Origam.Workbench;

namespace Origam.Schema.EntityModel.Wizards
{
	/// <summary>
	/// Summary description for CreateDataStructureFromEntityCommand.
	/// </summary>
	public class CreateDataStructureFromEntityCommand : AbstractMenuCommand
	{
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
            IDataEntity entity = Owner as IDataEntity;
            ArrayList list = new ArrayList();
            DataStructure dd = new DataStructure();
            list.Add(new object[] { dd.ItemType, dd.Icon });
            Wizard wizardscreen = new Wizard();
            wizardscreen.SetDescription("Create Data Structure Wizard");
            wizardscreen.ShowObjcts(list);
            Stack<PagesList> stackPage = new Stack<PagesList>();
            stackPage.Push(PagesList.startPage);
           
            wizardscreen.ShowPages(stackPage);
            if (wizardscreen.ShowDialog() == DialogResult.OK)
            {
                DataStructure ds = EntityHelper.CreateDataStructure(entity, entity.Name, true);
                GeneratedModelElements.Add(ds);
            }
		}
	}
}
