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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System.Data;

namespace Origam.Server;
class DeletedRowInfo
{
    public DeletedRowInfo(DataRow row)
    {
        _rowData = GetRowData(row, DataRowVersion.Default);
        if (row.RowState == DataRowState.Modified)
        {
            _originalRowData = GetRowData(row, DataRowVersion.Original);
        }
        
        _state = row.RowState;
    }
    public void ImportData(DataTable table)
    {
        if (_originalRowData != null)
        {
            // change
            DataRow newRow = table.Rows.Add(_originalRowData);
            newRow.AcceptChanges();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn col = table.Columns[i];
                if (!col.ReadOnly && (col.Expression == "" || col.Expression == null))
                {
                    newRow[col] = _rowData[i]; 
                }
            }
        }
        else
        {
            // new
            DataRow newRow = table.Rows.Add(_rowData);
            // or unchanged
            if (_state == DataRowState.Unchanged)
            {
                newRow.AcceptChanges();
            }
        }
    }
    private object[] GetRowData(DataRow row, DataRowVersion version)
    {
        int count = row.Table.Columns.Count;
        object[] result = new object[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = row[i, version];
        }
        return result;
    }
    private object[] _originalRowData = null;
    private object[] _rowData = null;
    private DataRowState _state;
}
