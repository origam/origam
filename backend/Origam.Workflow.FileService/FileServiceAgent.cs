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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using Extender;
using FileHelpers;
using log4net;
using Origam.DA;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Workflow.FileService;

public class FileServiceAgent : AbstractServiceAgent
{
    private object _result;
    private static readonly log4net.ILog log = LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private IXmlContainer ReadDelimitedFile(
        TextReader reader,
        XmlContainer optionsXml,
        string entity
    )
    {
        DataSet data = CreateEmptyOutputData();
        DataTable dt = data.Tables[name: entity];
        TextReaderOptions options = TextReaderOptions.Deserialize(doc: optionsXml.Xml);
        var typeExtender = new TypeExtender(className: entity);
        typeExtender.AddAttribute<IgnoreFirstAttribute>(
            attributeCtorParams: new object[] { options.IgnoreFirst }
        );
        typeExtender.AddAttribute<IgnoreLastAttribute>(
            attributeCtorParams: new object[] { options.IgnoreLast }
        );
        typeExtender.AddAttribute<DelimitedRecordAttribute>(
            attributeCtorParams: new object[] { options.Separator, "" }
        );

        foreach (TextReaderOptionsField trof in options.FieldOptions)
        {
            var attributeParameters = new Dictionary<Type, List<object>>();
            DataColumn col = null;
            Type dataType = typeof(string);
            if (!trof.IsIgnored)
            {
                col = dt.Columns[name: trof.Name];
                dataType = col.DataType;
                if (col == null)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "columnName",
                        actualValue: trof.Name,
                        message: "Column specified in options was not found in the data structure."
                    );
                }
                if (col.AllowDBNull)
                {
                    // Convert standard types to nullable types for the engine.
                    // The later conversion to DataTable actually converts them
                    // back to value types internally.
                    if (col.DataType.IsValueType)
                    {
                        dataType = typeof(Nullable<>).MakeGenericType(typeArguments: col.DataType);
                    }
                }
            }

            if (dataType == typeof(DateTime))
            {
                attributeParameters.Add(
                    key: typeof(FieldConverterAttribute),
                    value: new List<object>()
                );
                if (trof.AlternativeFormats == null)
                {
                    attributeParameters[key: typeof(FieldConverterAttribute)]
                        .Add(item: ConverterKind.Date);
                    attributeParameters[key: typeof(FieldConverterAttribute)]
                        .Add(item: trof.Format);
                }
                else
                {
                    attributeParameters[key: typeof(FieldConverterAttribute)]
                        .Add(item: ConverterKind.DateMultiFormat);
                    attributeParameters[key: typeof(FieldConverterAttribute)]
                        .Add(item: trof.Format);
                    if (trof.AlternativeFormats.Length > 0)
                    {
                        attributeParameters[key: typeof(FieldConverterAttribute)]
                            .Add(item: trof.AlternativeFormats[0]);
                    }
                    if (trof.AlternativeFormats.Length > 1)
                    {
                        attributeParameters[key: typeof(FieldConverterAttribute)]
                            .Add(item: trof.AlternativeFormats[1]);
                    }
                    if (trof.AlternativeFormats.Length > 2)
                    {
                        throw new ArgumentException(
                            message: "Maximum 2 alternative date formats can be used by this function."
                        );
                    }
                }
            }
            else if (dataType == typeof(decimal))
            {
                attributeParameters.Add(
                    key: typeof(FieldConverterAttribute),
                    value: new List<object> { ConverterKind.Decimal, trof.DecimalSeparator }
                );
            }
            if (trof.IsOptional)
            {
                attributeParameters.Add(
                    key: typeof(FieldOptionalAttribute),
                    value: new List<object>()
                );
            }
            if (trof.IsQuoted)
            {
                if (!string.IsNullOrEmpty(value: trof.QuoteChar))
                {
                    attributeParameters.Add(
                        key: typeof(FieldQuotedAttribute),
                        value: new List<object>
                        {
                            trof.QuoteChar,
                            QuoteMode.OptionalForRead,
                            MultilineMode.AllowForRead,
                        }
                    );
                }
                else
                {
                    attributeParameters.Add(
                        key: typeof(FieldQuotedAttribute),
                        value: new List<object>
                        {
                            QuoteMode.OptionalForRead,
                            MultilineMode.AllowForRead,
                        }
                    );
                }
            }
            if (trof.NullValue != null && col != null)
            {
                var nullValue = Convert.ChangeType(
                    value: trof.NullValue,
                    conversionType: col.DataType
                );
                attributeParameters.Add(
                    key: typeof(FieldNullValueAttribute),
                    value: new List<object> { nullValue }
                );
            }
            typeExtender.AddField(
                fieldName: trof.Name,
                fieldType: dataType,
                attributeTypesAndParameters: attributeParameters
            );
        }
        UserProfile profile = SecurityManager.CurrentUserProfile();
        FileHelperEngine engine = new FileHelperEngine(recordType: typeExtender.FetchType());
        DataTable newDt = engine.ReadStreamAsDT(reader: reader);
        if (log.IsInfoEnabled)
        {
            log.InfoFormat(format: "Read {0} records from a file.", arg0: newDt?.Rows.Count);
        }
        dt.TableNewRow += new DataTableNewRowEventHandler(dt_TableNewRow);
        MergeParams mergeParams = new MergeParams();
        mergeParams.SourceIsFragment = true;
        mergeParams.ProfileId = profile.Id;
        DatasetTools.MergeDataTable(
            inout_dtTarget: dt,
            in_dtSource: newDt,
            changeList: null,
            expressions: null,
            mergeParams: mergeParams
        );
        dt.TableNewRow -= new DataTableNewRowEventHandler(dt_TableNewRow);
        return DataDocumentFactory.New(dataSet: data);
    }

    void dt_TableNewRow(object sender, DataTableNewRowEventArgs e)
    {
        DatasetTools.ApplyPrimaryKey(row: e.Row);
    }

    private IXmlContainer ReadFile(
        string fileName,
        XmlContainer optionsXml,
        string entity,
        object file,
        string encodingName
    )
    {
        string stringFile = file as string;
        byte[] blobFile = file as byte[];
        Encoding encoding = FileSystemServiceAgent.GetEncoding(encoding: encodingName);
        if (stringFile != null)
        {
            using (StringReader sr = new StringReader(s: stringFile))
            {
                return ReadDelimitedFile(reader: sr, optionsXml: optionsXml, entity: entity);
            }
        }

        if (blobFile != null)
        {
            MemoryStream ms = new MemoryStream(buffer: blobFile);
            using (TextReader tr = new StreamReader(stream: ms, encoding: encoding))
            {
                return ReadDelimitedFile(reader: tr, optionsXml: optionsXml, entity: entity);
            }
        }

        using (TextReader tr = new StreamReader(path: fileName, encoding: encoding))
        {
            return ReadDelimitedFile(reader: tr, optionsXml: optionsXml, entity: entity);
        }
    }

    private Type ConvertType(Type t)
    {
        if (t == typeof(int))
        {
            return typeof(int?);
        }

        if (t == typeof(decimal))
        {
            return typeof(decimal?);
        }

        if (t == typeof(Guid))
        {
            return typeof(Guid?);
        }

        if (t == typeof(DateTime))
        {
            return typeof(DateTime?);
        }

        return t;
    }

    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        if (log.IsDebugEnabled)
        {
            log.RunHandled(loggingAction: () =>
            {
                log.DebugFormat(format: "Executing {0}", arg0: this.MethodName);
                foreach (DictionaryEntry item in Parameters)
                {
                    string value;
                    if (item.Value == null)
                    {
                        value = null;
                    }
                    else if (item.Value is XmlDocument)
                    {
                        value = (item.Value as XmlDocument).OuterXml;
                    }
                    else
                    {
                        value = item.Value.ToString();
                    }

                    log.DebugFormat(
                        format: "Parameter {0}, Value {1}",
                        arg0: item.Key,
                        arg1: value
                    );
                }
            });
        }
        switch (this.MethodName)
        {
            case "ReadTextFile":
            {
                // Check input parameters
                if (
                    !(
                        this.Parameters[key: "Entity"] is string
                        || this.Parameters[key: "Entity"] == null
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorViewNameNotString")
                    );
                }

                _result = this.ReadFile(
                    fileName: this.Parameters[key: "FileName"] as String,
                    optionsXml: this.Parameters[key: "Options"] as XmlContainer,
                    entity: this.Parameters[key: "Entity"] as String,
                    file: this.Parameters[key: "File"],
                    encodingName: this.Parameters[key: "Encoding"] as string
                );
                break;
            }
        }
    }
}
