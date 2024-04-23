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

using System;
using core = Origam.Workbench.Services.CoreServices;
using System.Data;
using Origam.Server;

namespace Origam.Server;

public class AttachmentUtils
{
    public static DataRow LoadAttachmentInfo(object id)
    {
            DataSet result = core.DataService.Instance.LoadData(new Guid("44a25061-750f-4b42-a6de-09f3363f8621"), new Guid("08a7d05e-c3e8-414e-a9a3-11bee9a26025"), Guid.Empty, Guid.Empty, null, "Attachment_parId", id);
            DataTable t = result.Tables["Attachment"];
            if (t.Rows.Count == 0)
            {
                throw new Exception(Resources.ErrorAttachmentNotFoundInDb);
            }
            return t.Rows[0];
        }

    public static void SaveAttachmentInfo(DataRow row)
    {
            core.DataService.Instance.StoreData(new Guid("44a25061-750f-4b42-a6de-09f3363f8621"), row.Table.DataSet, false, null);
        }
}