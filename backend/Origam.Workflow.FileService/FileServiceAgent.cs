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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private IXmlContainer ReadDelimitedFile(
        TextReader reader,
        XmlContainer optionsXml,
        string entity
    )
    {
        DataSet data = CreateEmptyOutputData();
        DataTable dt = data.Tables[entity];
        TextReaderOptions options = TextReaderOptions.Deserialize(optionsXml.Xml);
        var typeExtender = new TypeExtender(entity);
        typeExtender.AddAttribute<IgnoreFirstAttribute>(new object[] { options.IgnoreFirst });
        typeExtender.AddAttribute<IgnoreLastAttribute>(new object[] { options.IgnoreLast });
        typeExtender.AddAttribute<DelimitedRecordAttribute>(new object[] { options.Separator, "" });

        foreach (TextReaderOptionsField trof in options.FieldOptions)
        {
            var attributeParameters = new Dictionary<Type, List<object>>();
            DataColumn col = null;
            Type dataType = typeof(string);
            if (!trof.IsIgnored)
            {
                col = dt.Columns[trof.Name];
                dataType = col.DataType;
                if (col == null)
                {
                    throw new ArgumentOutOfRangeException(
                        "columnName",
                        trof.Name,
                        "Column specified in options was not found in the data structure."
                    );
                }
                if (col.AllowDBNull)
                {
                    // Convert standard types to nullable types for the engine.
                    // The later conversion to DataTable actually converts them
                    // back to value types internally.
                    if (col.DataType.IsValueType)
                    {
                        dataType = typeof(Nullable<>).MakeGenericType(col.DataType);
                    }
                }
            }

            if (dataType == typeof(DateTime))
            {
                attributeParameters.Add(typeof(FieldConverterAttribute), new List<object>());
                if (trof.AlternativeFormats == null)
                {
                    attributeParameters[typeof(FieldConverterAttribute)].Add(ConverterKind.Date);
                    attributeParameters[typeof(FieldConverterAttribute)].Add(trof.Format);
                }
                else
                {
                    attributeParameters[typeof(FieldConverterAttribute)]
                        .Add(ConverterKind.DateMultiFormat);
                    attributeParameters[typeof(FieldConverterAttribute)].Add(trof.Format);
                    if (trof.AlternativeFormats.Length > 0)
                    {
                        attributeParameters[typeof(FieldConverterAttribute)]
                            .Add(trof.AlternativeFormats[0]);
                    }
                    if (trof.AlternativeFormats.Length > 1)
                    {
                        attributeParameters[typeof(FieldConverterAttribute)]
                            .Add(trof.AlternativeFormats[1]);
                    }
                    if (trof.AlternativeFormats.Length > 2)
                    {
                        throw new ArgumentException(
                            "Maximum 2 alternative date formats can be used by this function."
                        );
                    }
                }
            }
            else if (dataType == typeof(decimal))
            {
                attributeParameters.Add(
                    typeof(FieldConverterAttribute),
                    new List<object> { ConverterKind.Decimal, trof.DecimalSeparator }
                );
            }
            if (trof.IsOptional)
            {
                attributeParameters.Add(typeof(FieldOptionalAttribute), new List<object>());
            }
            if (trof.IsQuoted)
            {
                if (!string.IsNullOrEmpty(trof.QuoteChar))
                {
                    attributeParameters.Add(
                        typeof(FieldQuotedAttribute),
                        new List<object>
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
                        typeof(FieldQuotedAttribute),
                        new List<object> { QuoteMode.OptionalForRead, MultilineMode.AllowForRead }
                    );
                }
            }
            if (trof.NullValue != null && col != null)
            {
                var nullValue = Convert.ChangeType(trof.NullValue, col.DataType);
                attributeParameters.Add(
                    typeof(FieldNullValueAttribute),
                    new List<object> { nullValue }
                );
            }
            typeExtender.AddField(trof.Name, dataType, attributeParameters);
        }
        UserProfile profile = SecurityManager.CurrentUserProfile();
        FileHelperEngine engine = new FileHelperEngine(typeExtender.FetchType());
        DataTable newDt = engine.ReadStreamAsDT(reader);
        if (log.IsInfoEnabled)
        {
            log.InfoFormat("Read {0} records from a file.", newDt?.Rows.Count);
        }
        dt.TableNewRow += new DataTableNewRowEventHandler(dt_TableNewRow);
        MergeParams mergeParams = new MergeParams();
        mergeParams.SourceIsFragment = true;
        mergeParams.ProfileId = profile.Id;
        DatasetTools.MergeDataTable(dt, newDt, null, null, mergeParams);
        dt.TableNewRow -= new DataTableNewRowEventHandler(dt_TableNewRow);
        return DataDocumentFactory.New(data);
    }

    void dt_TableNewRow(object sender, DataTableNewRowEventArgs e)
    {
        DatasetTools.ApplyPrimaryKey(e.Row);
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
        Encoding encoding = FileSystemServiceAgent.GetEncoding(encodingName);
        if (stringFile != null)
        {
            using (StringReader sr = new StringReader(stringFile))
            {
                return ReadDelimitedFile(sr, optionsXml, entity);
            }
        }
        else if (blobFile != null)
        {
            MemoryStream ms = new MemoryStream(blobFile);
            using (TextReader tr = new StreamReader(ms, encoding))
            {
                return ReadDelimitedFile(tr, optionsXml, entity);
            }
        }
        else
        {
            using (TextReader tr = new StreamReader(fileName, encoding))
            {
                return ReadDelimitedFile(tr, optionsXml, entity);
            }
        }
    }

    private Type ConvertType(Type t)
    {
        if (t == typeof(int))
        {
            return typeof(int?);
        }
        else if (t == typeof(decimal))
        {
            return typeof(decimal?);
        }
        else if (t == typeof(Guid))
        {
            return typeof(Guid?);
        }
        else if (t == typeof(DateTime))
        {
            return typeof(DateTime?);
        }
        else
        {
            return t;
        }
    }

    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        if (log.IsDebugEnabled)
        {
            log.RunHandled(() =>
            {
                log.DebugFormat("Executing {0}", this.MethodName);
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

                    log.DebugFormat("Parameter {0}, Value {1}", item.Key, value);
                }
            });
        }
        switch (this.MethodName)
        {
            case "ReadTextFile":
                // Check input parameters
                if (!(this.Parameters["Entity"] is string || this.Parameters["Entity"] == null))
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorViewNameNotString")
                    );
                _result = this.ReadFile(
                    this.Parameters["FileName"] as String,
                    this.Parameters["Options"] as XmlContainer,
                    this.Parameters["Entity"] as String,
                    this.Parameters["File"],
                    this.Parameters["Encoding"] as string
                );
                break;
        }
    }
}
