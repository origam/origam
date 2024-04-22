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

using System.Collections.Generic;
using System.Data;

namespace Origam.DA;

public class OrigamDataRow : DataRow
{
    private List<DataColumn> _columnsWithValidChange 
        = new List<DataColumn>();
    private bool _hasChangedOnce = false;

    protected internal OrigamDataRow(DataRowBuilder builder) : base(builder)
    {
        }

    public void AddColumnWithValidChange(DataColumn dataColumn)
    {
            if (!_columnsWithValidChange.Contains(dataColumn))
            {
                _columnsWithValidChange.Add(dataColumn);
                _hasChangedOnce = true;
            }
        }

    public bool HasColumnWithValidChange()
    {
            return _columnsWithValidChange.Count > 0;
        }

    public bool IsColumnWithValidChange(DataColumn dataColumn)
    {
            return _columnsWithValidChange.Contains(dataColumn);
        }

    public void ResetColumnsWithValidChange()
    {
            _columnsWithValidChange = new List<DataColumn>();
            if (! _hasChangedOnce && this.RowState == DataRowState.Modified)
            {
                this.RejectChanges();
            }
        }

    public bool HasChangedOnce
    {
        get
        {
                return _hasChangedOnce;
            }
        set
        {
                _hasChangedOnce = value;
            }
    }
}