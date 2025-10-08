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
using System.Windows.Forms;
using Origam.Schema.EntityModel;

namespace Origam.UI.WizardForm;

public class ChildEntityForm : AbstractWizardForm
{
    public IDataEntity Entity2 { get; set; }
    public IDataEntity Entity1 { get; set; }
    public string EntityName { get; set; }
    public string EnterAllInfo { get; set; }
    public string ChildEntityWiz { get; set; }

    internal void SetUpForm(TextBox txtchildEntityName, ComboBox cboEntity1, ComboBox cboEntity2)
    {
        if (cboEntity1.Items.Count == 0)
        {
            txtchildEntityName.Text = "";
            cboEntity1.Items.Clear();
            cboEntity2.Items.Clear();
            if (this.Entity1 == null)
                return;
            object selectedItem = null;
            foreach (IDataEntity entity in this.Entity1.RootProvider.ChildItems)
            {
                cboEntity1.Items.Add(entity);
                cboEntity2.Items.Add(entity);
                if (entity.PrimaryKey.Equals(this.Entity1.PrimaryKey))
                {
                    selectedItem = entity;
                }
            }
            cboEntity1.SelectedItem = selectedItem;
        }
    }
}
