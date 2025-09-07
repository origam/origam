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
using System.Data;
using System.Text;
using System.Text.Json;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Common;

public static class OrigamEventTools
{
    private static readonly JsonEncodedText FormId
        = JsonEncodedText.Encode("formId");
    private static readonly JsonEncodedText FormName      
        = JsonEncodedText.Encode("formName");
    private static readonly JsonEncodedText EntityName    
        = JsonEncodedText.Encode("entityName");
    private static readonly JsonEncodedText Fields        
        = JsonEncodedText.Encode("fields");
    private static readonly JsonEncodedText Field         
        = JsonEncodedText.Encode("field");
    private static readonly JsonEncodedText Caption       
        = JsonEncodedText.Encode("caption");
    private static readonly JsonEncodedText NumberOfRows  
        = JsonEncodedText.Encode("numberOfRows");
    private static readonly JsonEncodedText FormLabel     
        = JsonEncodedText.Encode("formLabel");
    
    public static void RecordSignInEvent()
    {
        RecordEvent(
            eventId: OrigamEvent.SignIn.EventId, 
            details: null);
    }

    public static void RecordSignOutEvent()
    {
        RecordEvent(
            eventId: OrigamEvent.SignOut.EventId, 
            details: null);
    }

    public static void RecordOpenScreen(SessionStore sessionStore)
    {
        RecordEvent(
            eventId: OrigamEvent.OpenScreen.EventId,
            details: CreateOpenScreenDetails(sessionStore));
    }

    public static void RecordExportToExcel(
        EntityExportInfo entityExportInfo,
        int numberOfRows)
    {
        RecordEvent(
            eventId: OrigamEvent.ExportToExcel.EventId,
            details: CreateExportToExcelDetails(
                entityExportInfo, numberOfRows));
    }

    private static string CreateExportToExcelDetails(
        EntityExportInfo entityExportInfo, 
        int numberOfRows)
    {
        return BuildJson(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(FormId);
            w.WriteStringValue(entityExportInfo.Store.FormId);
            w.WritePropertyName(FormName);
            w.WriteStringValue(entityExportInfo.Store.Name);
            w.WritePropertyName(EntityName);
            w.WriteStringValue(entityExportInfo.Entity);
            w.WritePropertyName(Fields);
            w.WriteStartArray();
            foreach (EntityExportField field in entityExportInfo.Fields)
            {
                w.WriteStartObject();
                w.WritePropertyName(Field);
                w.WriteStringValue(field.FieldName);
                w.WritePropertyName(Caption);
                w.WriteStringValue(field.Caption);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WritePropertyName(NumberOfRows);
            w.WriteNumberValue(numberOfRows);
            w.WriteEndObject();
        });
    }

    private static string CreateOpenScreenDetails(SessionStore sessionStore)
    {
        string formLabel = null;
        if (sessionStore is FormSessionStore formSessionStore
            && !string.IsNullOrEmpty(
                formSessionStore.MenuItem.DynamicFormLabelField)
            && (formSessionStore.MenuItem.MethodId != Guid.Empty))
        {
            string labelEntitySource = formSessionStore.MenuItem
                .DynamicFormLabelEntity.EntityDefinition.Name;
            formLabel = formSessionStore.Data.Tables[labelEntitySource]
                ?.Rows[0][formSessionStore.MenuItem.DynamicFormLabelField]
                .ToString();
        }
        return BuildJson(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(FormId);
            w.WriteStringValue(sessionStore.FormId);
            w.WritePropertyName(FormName);
            w.WriteStringValue(sessionStore.Name);
            if (!string.IsNullOrEmpty(formLabel))
            {
                    w.WritePropertyName(FormLabel);
                    w.WriteStringValue(formLabel);
            }
            w.WriteEndObject();
        });
    }
    
    private static string BuildJson(Action<Utf8JsonWriter> write)
    {
        var buffer = new ArrayBufferWriter<byte>(); 
        using var writer = new Utf8JsonWriter(
            buffer, new JsonWriterOptions {Indented = true});
        write(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static void RecordEvent(Guid eventId, string details)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var dataService  = DataServiceFactory.GetDataService();
        DataSet origamEventDataSet = dataService.GetEmptyDataSet(
            OrigamEvent.DataStructureId);
        DataRow origamEventRecord = origamEventDataSet.Tables[0].NewRow();
        DatasetTools.ApplyPrimaryKey(origamEventRecord);
        origamEventRecord["Timestamp"] = DateTime.Now;
        origamEventRecord["Instance"] = settings.Name;
        origamEventRecord["refOrigamEventTypeId"] = eventId;
        origamEventRecord["refBusinessPartnerId"]
            = SecurityManager.CurrentUserProfile().Id;
        if (!string.IsNullOrEmpty(details))
        {
            origamEventRecord["Details"] = details;
        }
        origamEventDataSet.Tables[0].Rows.Add(origamEventRecord);
        DataService.Instance.StoreData(
            dataStructureId: OrigamEvent.DataStructureId,
            data: origamEventDataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null);
    }
}