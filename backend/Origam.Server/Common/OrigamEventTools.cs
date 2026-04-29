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

using System;
using System.Buffers;
using System.Collections;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Common;

public static class OrigamEventTools
{
    private static readonly JsonEncodedText ObjectId = JsonEncodedText.Encode(value: "objectId");
    private static readonly JsonEncodedText Title = JsonEncodedText.Encode(value: "title");
    private static readonly JsonEncodedText EntityName = JsonEncodedText.Encode(
        value: "entityName"
    );
    private static readonly JsonEncodedText Fields = JsonEncodedText.Encode(value: "fields");
    private static readonly JsonEncodedText Field = JsonEncodedText.Encode(value: "field");
    private static readonly JsonEncodedText Caption = JsonEncodedText.Encode(value: "caption");
    private static readonly JsonEncodedText NumberOfRows = JsonEncodedText.Encode(
        value: "numberOfRows"
    );
    private static readonly JsonEncodedText Parameters = JsonEncodedText.Encode(
        value: "parameters"
    );

    public static void RecordSignInEvent()
    {
        RecordEvent(eventId: OrigamEvent.SignIn.EventId, details: null);
    }

    public static void RecordSignOutEvent()
    {
        RecordEvent(eventId: OrigamEvent.SignOut.EventId, details: null);
    }

    public static void RecordOpenScreen(SessionStore sessionStore)
    {
        RecordEvent(
            eventId: OrigamEvent.OpenScreen.EventId,
            details: CreateOpenScreenDetails(sessionStore: sessionStore)
        );
    }

    public static void RecordExportToExcel(EntityExportInfo entityExportInfo, int numberOfRows)
    {
        RecordEvent(
            eventId: OrigamEvent.ExportToExcel.EventId,
            details: CreateExportToExcelDetails(
                entityExportInfo: entityExportInfo,
                numberOfRows: numberOfRows
            )
        );
    }

    private static string CreateExportToExcelDetails(
        EntityExportInfo entityExportInfo,
        int numberOfRows
    )
    {
        string formLabel = ResolveDynamicFormLabel(sessionStore: entityExportInfo.Store);
        return BuildJson(write: w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(propertyName: ObjectId);
            w.WriteStringValue(value: entityExportInfo.Store.Request.ObjectId);
            w.WritePropertyName(propertyName: Title);
            w.WriteStringValue(value: entityExportInfo.Store.Title);
            w.WritePropertyName(propertyName: Caption);
            w.WriteStringValue(
                value: !string.IsNullOrEmpty(value: formLabel)
                    ? formLabel
                    : entityExportInfo.Store.Request.Caption
            );
            w.WritePropertyName(propertyName: EntityName);
            w.WriteStringValue(value: entityExportInfo.Entity);
            w.WritePropertyName(propertyName: Fields);
            w.WriteStartArray();
            foreach (EntityExportField field in entityExportInfo.Fields)
            {
                w.WriteStartObject();
                w.WritePropertyName(propertyName: Field);
                w.WriteStringValue(value: field.FieldName);
                w.WritePropertyName(propertyName: Caption);
                w.WriteStringValue(value: field.Caption);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WritePropertyName(propertyName: NumberOfRows);
            w.WriteNumberValue(value: numberOfRows);
            w.WriteEndObject();
        });
    }

    private static string CreateOpenScreenDetails(SessionStore sessionStore)
    {
        string formLabel = ResolveDynamicFormLabel(sessionStore: sessionStore);
        return BuildJson(write: w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(propertyName: ObjectId);
            w.WriteStringValue(value: sessionStore.Request.ObjectId);
            w.WritePropertyName(propertyName: Title);
            w.WriteStringValue(value: sessionStore.Title);
            w.WritePropertyName(propertyName: Caption);
            w.WriteStringValue(
                value: !string.IsNullOrEmpty(value: formLabel)
                    ? formLabel
                    : sessionStore.Request.Caption
            );
            w.WritePropertyName(propertyName: Parameters);
            w.WriteStartObject();
            foreach (DictionaryEntry requestParameter in sessionStore.Request.Parameters)
            {
                if (requestParameter.Key.ToString() == null)
                {
                    continue;
                }
                w.WritePropertyName(propertyName: requestParameter.Key.ToString()!);
                w.WriteStringValue(
                    value: requestParameter.Value != null ? requestParameter.Value.ToString() : ""
                );
            }
            w.WriteEndObject();
            w.WriteEndObject();
        });
    }

    private static string BuildJson(Action<Utf8JsonWriter> write)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(
            bufferWriter: buffer,
            options: new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }
        );
        write(obj: writer);
        writer.Flush();
        return Encoding.UTF8.GetString(bytes: buffer.WrittenSpan);
    }

    private static string ResolveDynamicFormLabel(SessionStore sessionStore)
    {
        if (
            sessionStore is not FormSessionStore formSessionStore
            || string.IsNullOrEmpty(value: formSessionStore.MenuItem.DynamicFormLabelField)
            || (formSessionStore.MenuItem.MethodId == Guid.Empty)
        )
        {
            return null;
        }
        string labelEntitySource = formSessionStore
            .MenuItem
            .DynamicFormLabelEntity
            .EntityDefinition
            .Name;
        if (formSessionStore.Data.Tables[name: labelEntitySource]?.Rows.Count > 0)
        {
            return formSessionStore
                .Data.Tables[name: labelEntitySource]
                .Rows[index: 0][columnName: formSessionStore.MenuItem.DynamicFormLabelField]
                .ToString();
        }
        return null;
    }

    private static void RecordEvent(Guid eventId, string details)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var dataService = DataServiceFactory.GetDataService();
        DataSet origamEventDataSet = dataService.GetEmptyDataSet(
            dataStructureId: OrigamEvent.DataStructureId
        );
        DataRow origamEventRecord = origamEventDataSet.Tables[index: 0].NewRow();
        DatasetTools.ApplyPrimaryKey(row: origamEventRecord);
        origamEventRecord[columnName: "Timestamp"] = DateTime.Now;
        origamEventRecord[columnName: "Instance"] = settings.Name;
        origamEventRecord[columnName: "refOrigamEventTypeId"] = eventId;
        origamEventRecord[columnName: "refBusinessPartnerId"] = SecurityManager
            .CurrentUserProfile()
            .Id;
        if (!string.IsNullOrEmpty(value: details))
        {
            origamEventRecord[columnName: "Details"] = details;
        }
        origamEventDataSet.Tables[index: 0].Rows.Add(row: origamEventRecord);
        DataService.Instance.StoreData(
            dataStructureId: OrigamEvent.DataStructureId,
            data: origamEventDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }
}
