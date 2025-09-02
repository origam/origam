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
using Origam.DA;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Workbench;

/// <summary>
/// Summary description for AuditLogDA.
/// </summary>
public class AuditLogDA
{
    public static bool IsRecordIdValid(object recordId)
    {
        if (recordId is Guid)
        {
            return true;
        }

        string stringId = recordId as string;
        if (stringId != null)
        {
            try
            {
                Guid guidId = new Guid(stringId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public static DataSet RetrieveLog(Guid entityId, object recordId)
    {
        if (!IsRecordIdValid(recordId))
        {
            return null;
        }

        IBusinessServicesService services =
            ServiceManager.Services.GetService(typeof(IBusinessServicesService))
            as IBusinessServicesService;
        IServiceAgent dataServiceAgent = services.GetAgent("DataService", null, null);
        DataStructureQuery query = new DataStructureQuery(
            new Guid("530eba45-40db-470d-8e53-8b98ace758ad"),
            new Guid("e0602afa-e86d-4f03-bcb8-aee19f976961")
        );
        query.Parameters.Add(new QueryParameter("AuditRecord_parRefParentRecordId", recordId));

        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add("Query", query);
        dataServiceAgent.Run();
        DataSet result = dataServiceAgent.Result as DataSet;
        return result;
    }

    public static DataSet RetrieveLogTransformed(Guid entityId, object recordId)
    {
        if (!IsRecordIdValid(recordId))
        {
            return null;
        }
        var sourceDataSet = RetrieveLog(entityId, recordId);
        var dataSet = new DataSet("ROOT");
        var table = new OrigamDataTable("AuditLog");
        dataSet.Tables.Add(table);
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("Timestamp", typeof(DateTime));
        table.Columns.Add("User", typeof(string));
        table.Columns.Add("Property", typeof(string));
        table.Columns.Add("OldValue", typeof(string));
        table.Columns.Add("NewValue", typeof(string));
        table.Columns.Add("ActionType", typeof(int));
        table.Columns.Add("ParentRecordId", typeof(Guid));
        table.Columns.Add("Entity", typeof(string));
        foreach (DataRow row in sourceDataSet.Tables[0].Rows)
        {
            string user;
            try
            {
                var profileProvider = SecurityManager.GetProfileProvider();
                var profile =
                    profileProvider.GetProfile((Guid)row["RecordCreatedBy"]) as UserProfile;
                user = profile.FullName;
            }
            catch (Exception ex)
            {
                user = ex.Message;
            }
            var entity = GetEntity(row);
            var property = GetProperty(row);
            table.LoadDataRow(
                new[]
                {
                    row["Id"],
                    row["RecordCreated"],
                    user,
                    property,
                    row["OldValue"],
                    row["NewValue"],
                    row["ActionType"],
                    row["refParentRecordId"],
                    entity,
                },
                true
            );
        }
        return dataSet;
    }

    private static string GetProperty(DataRow row)
    {
        var columnId = (Guid)row["refColumnId"];
        try
        {
            var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
            if (
                !(
                    persistenceService.SchemaProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(columnId)
                    )
                    is ISchemaItem schemaItem
                )
            )
            {
                return columnId.ToString();
            }
            if (schemaItem is ICaptionSchemaItem captionSchemaItem)
            {
                return string.IsNullOrEmpty(captionSchemaItem.Caption)
                    ? schemaItem.Name
                    : captionSchemaItem.Caption;
            }
            return schemaItem.Name;
        }
        catch
        {
            return columnId.ToString();
        }
    }

    private static string GetEntity(DataRow row)
    {
        if ((DataRowState)row["ActionType"] != DataRowState.Deleted)
        {
            return null;
        }
        var entityId = (Guid)row["refParentRecordEntityId"];
        try
        {
            var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
            if (
                !(
                    persistenceService.SchemaProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(entityId)
                    )
                    is ISchemaItem schemaItem
                )
            )
            {
                return entityId.ToString();
            }
            return schemaItem.Name;
        }
        catch
        {
            return entityId.ToString();
        }
    }
}
