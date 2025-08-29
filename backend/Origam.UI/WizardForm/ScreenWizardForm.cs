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
using System.Collections;
using System.Windows.Forms;

namespace Origam.UI.WizardForm;
public class ScreenWizardForm : AbstractWizardForm
{
    public IDataEntity Entity { get; set; }
    public bool IsRoleVisible { get; set; }
    public bool textColumnsOnly { get; set; }
    private CheckedListBox _lstFields;
    public bool CheckOnClick { get; set; } = false;
    public string Caption { get; set; }
    public void SetUpForm(CheckedListBox lstField)
    {
        if (_lstFields == null)
        {
            _lstFields = lstField;
            if (this.Entity == null)
            {
                return;
            }

            foreach (IDataEntityColumn column in this.Entity.EntityColumns)
            {
                if (string.IsNullOrEmpty(column.ToString()))
                {
                    continue;
                }

                if (!this.textColumnsOnly
                    || (column.DataType == Origam.Schema.OrigamDataType.String
                    || column.DataType == Origam.Schema.OrigamDataType.Memo))
                {
                    _lstFields.Items.Add(column);
                }
            }
        }
    }
    public Hashtable SelectedFieldNames
    {
        get
        {
            Hashtable result = new Hashtable();
            foreach (IDataEntityColumn column in _lstFields.CheckedItems)
            {
                result.Add(column.Name, null);
            }
            return result;
        }
    }
    public ICollection SelectedFields { get; set; }
    public string Role { get; set; }
}
