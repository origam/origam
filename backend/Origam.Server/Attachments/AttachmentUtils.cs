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
using System.Data;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class AttachmentUtils
{
    public static DataRow LoadAttachmentInfo(object id)
    {
        DataSet result = CoreServices.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "44a25061-750f-4b42-a6de-09f3363f8621"),
            methodId: new Guid(g: "08a7d05e-c3e8-414e-a9a3-11bee9a26025"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "Attachment_parId",
            paramValue1: id
        );
        DataTable t = result.Tables[name: "Attachment"];
        if (t.Rows.Count == 0)
        {
            throw new Exception(message: Resources.ErrorAttachmentNotFoundInDb);
        }
        return t.Rows[index: 0];
    }

    public static void SaveAttachmentInfo(DataRow row)
    {
        CoreServices.DataService.Instance.StoreData(
            dataStructureId: new Guid(g: "44a25061-750f-4b42-a6de-09f3363f8621"),
            data: row.Table.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }
}
