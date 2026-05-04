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
public static class AuditLogDA
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
                Guid guidId = new Guid(g: stringId);
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
        if (!IsRecordIdValid(recordId: recordId))
        {
            return null;
        }

        IBusinessServicesService services =
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService;
        IServiceAgent dataServiceAgent = services.GetAgent(
            serviceType: "DataService",
            ruleEngine: null,
            workflowEngine: null
        );
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: "530eba45-40db-470d-8e53-8b98ace758ad"),
            methodId: new Guid(g: "e0602afa-e86d-4f03-bcb8-aee19f976961")
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "AuditRecord_parRefParentRecordId",
                value: recordId
            )
        );

        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        DataSet result = dataServiceAgent.Result as DataSet;
        return result;
    }

    public static DataSet RetrieveLogTransformed(Guid entityId, object recordId)
    {
        if (!IsRecordIdValid(recordId: recordId))
        {
            return null;
        }
        var sourceDataSet = RetrieveLog(entityId: entityId, recordId: recordId);
        var dataSet = new DataSet(dataSetName: "ROOT");
        var table = new OrigamDataTable(tableName: "AuditLog");
        dataSet.Tables.Add(table: table);
        table.Columns.Add(columnName: "Id", type: typeof(Guid));
        table.Columns.Add(columnName: "Timestamp", type: typeof(DateTime));
        table.Columns.Add(columnName: "User", type: typeof(string));
        table.Columns.Add(columnName: "Property", type: typeof(string));
        table.Columns.Add(columnName: "OldValue", type: typeof(string));
        table.Columns.Add(columnName: "NewValue", type: typeof(string));
        table.Columns.Add(columnName: "ActionType", type: typeof(int));
        table.Columns.Add(columnName: "ParentRecordId", type: typeof(Guid));
        table.Columns.Add(columnName: "Entity", type: typeof(string));
        foreach (DataRow row in sourceDataSet.Tables[index: 0].Rows)
        {
            string user;
            try
            {
                var profileProvider = SecurityManager.GetProfileProvider();
                var profile =
                    profileProvider.GetProfile(profileId: (Guid)row[columnName: "RecordCreatedBy"])
                    as UserProfile;
                user = profile.FullName;
            }
            catch (Exception ex)
            {
                user = ex.Message;
            }
            var entity = GetEntity(row: row);
            var property = GetProperty(row: row);
            table.LoadDataRow(
                values: new[]
                {
                    row[columnName: "Id"],
                    row[columnName: "RecordCreated"],
                    user,
                    property,
                    row[columnName: "OldValue"],
                    row[columnName: "NewValue"],
                    row[columnName: "ActionType"],
                    row[columnName: "refParentRecordId"],
                    entity,
                },
                fAcceptChanges: true
            );
        }
        return dataSet;
    }

    private static string GetProperty(DataRow row)
    {
        var columnId = (Guid)row[columnName: "refColumnId"];
        try
        {
            var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
            if (
                !(
                    persistenceService.SchemaProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: columnId)
                    )
                    is ISchemaItem schemaItem
                )
            )
            {
                return columnId.ToString();
            }
            if (schemaItem is ICaptionSchemaItem captionSchemaItem)
            {
                return string.IsNullOrEmpty(value: captionSchemaItem.Caption)
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
        if ((DataRowState)row[columnName: "ActionType"] != DataRowState.Deleted)
        {
            return null;
        }
        var entityId = (Guid)row[columnName: "refParentRecordEntityId"];
        try
        {
            var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
            if (
                !(
                    persistenceService.SchemaProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: entityId)
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
