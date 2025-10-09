#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Schema.EntityModel;
using System;
using System.Windows.Forms;

namespace Origam.UI.WizardForm;
public class LookupForm : AbstractWizardForm
{
    public IDataEntity Entity { get; set; }
    public string LookupName { get; set; }
    public IDataEntityColumn NameColumn { get; set; }
    
    public IDataEntityColumn IdColumn { get; private set; } = null;
    public EntityFilter IdFilter { get; set; }
    
    public EntityFilter ListFilter { get; set; }
    
    internal void SetUpForm(ComboBox cboIdFilter, ComboBox cboListFilter, ComboBox cboDisplayField, TextBox txtName)
    {
        cboIdFilter.Items.Clear();
        cboListFilter.Items.Clear();
        cboDisplayField.Items.Clear();
        IDataEntityColumn nameColumn = null;
        if (this.Entity == null) return;
        txtName.Text = this.Entity.Name;
        EntityFilter idFilter = null;
        foreach (var filter in Entity.ChildItemsByType<EntityFilter>(EntityFilter.CategoryConst))
        {
            cboListFilter.Items.Add(filter);
            cboIdFilter.Items.Add(filter);
            if (filter.Name == "GetId") idFilter = filter;
        }
        if (idFilter != null) cboIdFilter.SelectedItem = idFilter;
        foreach (IDataEntityColumn column in this.Entity.EntityColumns)
        {
            if (string.IsNullOrEmpty(column.ToString())) continue;
            if (column.Name == "Name") nameColumn = column;
            if (column.IsPrimaryKey && !column.ExcludeFromAllFields) IdColumn = column;
            cboDisplayField.Items.Add(column);
        }
        cboDisplayField.SelectedItem = nameColumn;
        if (IdColumn == null) throw new Exception("Entity has no primary key defined. Cannot create lookup.");
    }
}
