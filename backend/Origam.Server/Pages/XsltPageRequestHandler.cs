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
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Newtonsoft.Json;
using Origam.DA;
using Origam.JSON;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Server.Model.UIService;
using Origam.Service.Core;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Pages;

internal class XsltPageRequestHandler : AbstractPageRequestHandler
{
    private const string MimeJson = "application/json";
    private const string MimeHtml = "text/html";
    private readonly IPersistenceService persistenceService =
        ServiceManager.Services.GetService<IPersistenceService>();

    public override void Execute(
        AbstractPage page,
        Dictionary<string, object> parameters,
        IRequestWrapper request,
        IResponseWrapper response
    )
    {
        var xsltPage = page as XsltDataPage;
        var ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
        var transformParams = new Hashtable();
        var queryParameterCollection = new QueryParameterCollection();
        Hashtable preprocessorParams = GetPreprocessorParameters(request: request);
        // convert parameters to QueryParameterCollection for data service and hashtable for transformation service
        foreach (KeyValuePair<string, object> parameter in parameters)
        {
            queryParameterCollection.Add(
                value: new QueryParameter(_parameterName: parameter.Key, value: parameter.Value)
            );
            transformParams.Add(key: parameter.Key, value: parameter.Value);
        }
        // copy also the preprocessor parameters to the transformation parameters
        foreach (DictionaryEntry parameter in preprocessorParams)
        {
            transformParams.Add(key: parameter.Key, value: parameter.Value);
        }
        Validate(
            data: null,
            transformParams: transformParams,
            ruleEngine: ruleEngine,
            validation: xsltPage!.InputValidationRule
        );
        if (xsltPage.DisableConstraintForInputValidation)
        {
            // reenable constraints for context parameter
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                if (parameter.Value is IDataDocument dataDocument)
                {
                    dataDocument.DataSet.EnforceConstraints = true;
                }
            }
        }
        IXmlContainer xmlData;
        DataSet data = null;
        var isProcessed = false;
        bool xpath = !string.IsNullOrEmpty(value: xsltPage.ResultXPath);
        if (xsltPage.DataStructure == null)
        {
            // no data source
            xmlData = new XmlContainer(xmlString: "<ROOT/>");
        }
        else
        {
            if (xsltPage.AllowCustomFilters)
            {
                data = LoadWithFilters(
                    xsltPage: xsltPage,
                    parameters: parameters,
                    request: request
                );
            }
            else
            {
                data = CoreServices.DataService.Instance.LoadData(
                    dataStructureId: xsltPage.DataStructureId,
                    methodId: xsltPage.DataStructureMethodId,
                    defaultSetId: Guid.Empty,
                    sortSetId: xsltPage.DataStructureSortSetId,
                    transactionId: null,
                    parameters: queryParameterCollection
                );
            }

            if ((request.HttpMethod != "DELETE") && (request.HttpMethod != "PUT"))
            {
                if (xsltPage.ProcessReadFieldRowLevelRulesForGetRequests)
                {
                    ProcessReadFieldRuleState(data: data, ruleEngine: ruleEngine);
                }
            }
            if (
                (xsltPage.Transformation == null)
                && !xpath
                && (page.MimeType == MimeJson)
                && (request.HttpMethod != "DELETE")
                && (request.HttpMethod != "PUT")
            )
            {
                // pure dataset > json serialization
                response.WriteToOutput(writeAction: textWriter =>
                    JsonUtils.SerializeToJson(
                        textWriter: textWriter,
                        value: data,
                        omitRootElement: xsltPage.OmitJsonRootElement,
                        omitMainElement: xsltPage.OmitJsonMainElement
                    )
                );
                xmlData = null;
                isProcessed = true;
            }
            else
            {
                xmlData = DataDocumentFactory.New(dataSet: data);
            }
            switch (request.HttpMethod)
            {
                case "DELETE":
                {
                    HandleDelete(
                        xsltPage: xsltPage,
                        data: data,
                        transformParams: transformParams,
                        ruleEngine: ruleEngine
                    );
                    return;
                }
                case "PUT":
                {
                    HandlePut(
                        parameters: parameters,
                        xsltPage: xsltPage,
                        data: (IDataDocument)xmlData,
                        transformParams: transformParams,
                        ruleEngine: ruleEngine
                    );
                    return;
                }
            }
        }
        IXmlContainer result;
        if (!isProcessed)
        {
            if (xsltPage.Transformation == null)
            {
                // no transformation
                result = xmlData;
            }
            else
            {
                var transformer = new CompiledXsltEngine(
                    persistence: persistenceService.SchemaProvider
                );
                result = transformer.Transform(
                    data: xmlData,
                    transformationId: xsltPage.TransformationId,
                    retransformationId: new Guid(g: "5b4f2532-a0e1-4ffc-9486-3f35d766af71"),
                    parameters: transformParams,
                    transactionId: null,
                    retransformationParameters: preprocessorParams,
                    outputStructure: xsltPage.TransformationOutputStructure,
                    validateOnly: false
                );
                // pure dataset > json serialization
                if (
                    (result is IDataDocument resultDataDocument)
                    && !xpath
                    && (page.MimeType == MimeJson)
                )
                {
                    response.WriteToOutput(writeAction: textWriter =>
                        JsonUtils.SerializeToJson(
                            textWriter: textWriter,
                            value: resultDataDocument.DataSet,
                            omitRootElement: xsltPage.OmitJsonRootElement,
                            omitMainElement: xsltPage.OmitJsonMainElement
                        )
                    );
                    isProcessed = true;
                }
            }
            if (!isProcessed)
            {
                if (xpath)
                {
                    // subset of the returned xml - json | html not supported
                    // it is mainly used for extracting pure text out of the result xml
                    // so json | html serialization would have to be produced by the
                    // xslt or stored directly in the resulting data
                    XPathNavigator xPathNavigator = result.Xml.CreateNavigator();
                    xPathNavigator!.Select(xpath: xsltPage.ResultXPath);
                    byte[] bytes = Encoding.UTF8.GetBytes(s: xPathNavigator.Value);
                    response.AddHeader(name: "Content-Length", value: bytes.LongLength.ToString());
                    response.BinaryWrite(bytes: bytes);
                }
                else
                {
                    if (page.MimeType == MimeJson)
                    {
                        response.WriteToOutput(writeAction: textWriter =>
                            JsonUtils.SerializeToJson(
                                textWriter: textWriter,
                                value: result.Xml,
                                omitRootElement: xsltPage.OmitJsonRootElement,
                                omitMainElement: xsltPage.OmitJsonMainElement
                            )
                        );
                    }
                    else
                    {
                        if (page.MimeType == MimeHtml)
                        {
                            response.Write(message: "<!DOCTYPE html>");
                        }
                        response.WriteToOutput(writeAction: textWriter =>
                            textWriter.Write(value: result.Xml.InnerXml)
                        );
                    }
                }
            }
        }
        if (!Analytics.Instance.IsAnalyticsEnabled || (xsltPage.LogTransformation == null))
        {
            return;
        }
        xmlData ??= DataDocumentFactory.New(dataSet: data);
        Type type = GetType();
        IXsltEngine logTransformer = new CompiledXsltEngine(
            persistence: persistenceService.SchemaProvider
        );
        IXmlContainer log = logTransformer.Transform(
            data: xmlData,
            transformationId: xsltPage.LogTransformationId,
            parameters: transformParams,
            transactionId: null,
            outputStructure: null,
            validateOnly: false
        );
        XPathNavigator logXPathNavigator = log.Xml.CreateNavigator();
        XPathNodeIterator xPathNodeIterator = logXPathNavigator!.Select(xpath: "/ROOT/LogContext");
        while (xPathNodeIterator.MoveNext())
        {
            var properties = new Dictionary<string, string>();
            XPathNavigator currentNode = xPathNodeIterator.Current;
            string message = currentNode!.Value == string.Empty ? "DATA_ACCESS" : currentNode.Value;
            currentNode.MoveToFirstAttribute();
            do
            {
                properties[key: currentNode.Name] = currentNode.Value;
            } while (currentNode.MoveToNextAttribute());
            Analytics.Instance.Log(type: type, message: message, properties: properties);
        }
    }

    // Loads the data with custom filters defined in the request
    private DataSet LoadWithFilters(
        XsltDataPage xsltPage,
        Dictionary<string, object> parameters,
        IRequestWrapper request
    )
    {
        DataStructure dataStructure =
            persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(
                instanceId: xsltPage.DataStructureId
            );
        DataStructureEntity entity = dataStructure.Entities.First();

        List<ColumnData> columns = entity
            .Columns.Select(selector: column =>
            {
                IDataEntityColumn field = column.Field;
                return new ColumnData(
                    name: column.Name,
                    isVirtual: (field is DetachedField || column.IsWriteOnly),
                    defaultValue: (field as DetachedField)?.DefaultValue?.Value,
                    hasRelation: (field as DetachedField)?.ArrayRelation != null
                );
            })
            .ToList();

        string body = ReadRequestBody(request: request);
        XsltDataPageFilterInput filterInput = DeserializeFilterInput(body: body);
        List<Ordering> orderings = GetOrderings(filterInput: filterInput);

        var parameterCollection = new QueryParameterCollection();
        foreach (KeyValuePair<string, object> parameter in parameters)
        {
            parameterCollection.Add(
                value: new QueryParameter(_parameterName: parameter.Key, value: parameter.Value)
            );
        }

        var query = new DataStructureQuery
        {
            Entity = entity.Name,
            DataSourceId = xsltPage.DataStructureId,
            RowLimit = GetIntParameterValue(parameters: parameters, parameterName: "_pageSize"),
            RowOffset = GetIntParameterValue(parameters: parameters, parameterName: "_pageNumber"),
            Parameters = parameterCollection,
            CustomFilters = new CustomFilters
            {
                Filters = filterInput?.Filter,
                FilterLookups = filterInput?.FilterLookups ?? [],
            },
            MethodId = xsltPage.DataStructureMethodId,
            SortSetId = xsltPage.DataStructureSortSetId,
            CustomOrderings = new CustomOrderings(orderings: orderings),
            ColumnsInfo = new ColumnsInfo(columns: columns, renderSqlForDetachedFields: true),
            ForceDatabaseCalculation = true,
        };
        IDataService dataService = CoreServices.DataServiceFactory.GetDataService();
        IEnumerable<Dictionary<string, object>> lines = dataService.ExecuteDataReaderReturnPairs(
            query: query
        );

        DataSet data = dataService.GetEmptyDataSet(dataStructureId: xsltPage.DataStructureId);
        DataTable dataTable = data.Tables[name: entity.Name];
        foreach (Dictionary<string, object> line in lines)
        {
            DataRow row = dataTable.NewRow();
            foreach (KeyValuePair<string, object> cell in line)
            {
                if (!dataTable.Columns.Contains(name: cell.Key))
                {
                    continue;
                }
                object value = cell.Value ?? DBNull.Value;
                if (
                    value is ICollection collection
                    && (
                        dataTable.Columns[name: cell.Key].DataType == typeof(long)
                        || dataTable.Columns[name: cell.Key].DataType == typeof(int)
                    )
                )
                {
                    value = collection.Count;
                }
                row[columnName: cell.Key] = value;
            }
            dataTable.Rows.Add(row: row);
        }

        return data;
    }

    private static string ReadRequestBody(IRequestWrapper request)
    {
        if (request.ContentLength == 0)
        {
            return string.Empty;
        }

        Stream inputStream = request.InputStream;
        if (inputStream == null)
        {
            return string.Empty;
        }

        if (inputStream.CanSeek)
        {
            inputStream.Position = 0;
        }

        using var reader = new StreamReader(
            stream: inputStream,
            encoding: request.ContentEncoding ?? Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true
        );
        string body = reader.ReadToEnd();
        if (inputStream.CanSeek)
        {
            inputStream.Position = 0;
        }
        return body;
    }

    private static XsltDataPageFilterInput DeserializeFilterInput(string body)
    {
        if (string.IsNullOrWhiteSpace(value: body))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<XsltDataPageFilterInput>(value: body);
    }

    private static List<Ordering> GetOrderings(XsltDataPageFilterInput filterInput)
    {
        if (filterInput?.Ordering == null)
        {
            return [];
        }

        return filterInput
            .Ordering.Select(
                selector: (ordering, index) =>
                    new Ordering(
                        columnName: ordering.ColumnId,
                        direction: ordering.Direction,
                        lookupId: ordering.LookupId,
                        sortOrder: index
                    )
            )
            .ToList();
    }

    private int GetIntParameterValue(Dictionary<string, object> parameters, string parameterName)
    {
        if (!parameters.TryGetValue(key: parameterName, value: out object objValue))
        {
            return 0;
        }
        if (!int.TryParse(s: objValue.ToString(), result: out int value))
        {
            throw new ArgumentException(
                message: string.Format(
                    format: Resources.ErrorInvalidIntParameterValue,
                    arg0: parameterName,
                    arg1: objValue
                )
            );
        }
        return value;
    }

    private void ProcessReadFieldRuleState(DataSet data, RuleEngine ruleEngine)
    {
        DataTableCollection dataTables = data.Tables;
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        foreach (DataTable dataTable in dataTables)
        {
            var entityId = (Guid)dataTable.ExtendedProperties[key: "EntityId"]!;
            IDataEntity entity = persistence.SchemaProvider.RetrieveInstance<IDataEntity>(
                instanceId: entityId
            );
            if (!entity.HasEntityAFieldDenyReadRule())
            {
                continue;
            }
            // we do this to disable constrains if column is AllowDBNull = false,
            // in order to allow field to be set DBNull if it has Deny Read rule.
            dataTable
                .Columns.Cast<DataColumn>()
                .ToList()
                .ForEach(action: column => column.AllowDBNull = true);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                RowSecurityState rowSecurity =
                    RowSecurityStateBuilder.BuildWithoutRelationsAndActions(
                        ruleEngine: ruleEngine,
                        row: dataRow
                    );
                if (rowSecurity == null)
                {
                    continue;
                }
                List<FieldSecurityState> listState = rowSecurity
                    .Columns.Where(predicate: columnState => !columnState.AllowRead)
                    .ToList();
                foreach (FieldSecurityState securityState in listState)
                {
                    dataRow[columnName: securityState.Name] = DBNull.Value;
                }
            }
        }
    }

    private static void HandlePut(
        Dictionary<string, object> parameters,
        XsltDataPage xsltPage,
        IDataDocument data,
        Hashtable transformParams,
        RuleEngine ruleEngine
    )
    {
        Validate(
            data: data,
            transformParams: transformParams,
            ruleEngine: ruleEngine,
            validation: xsltPage.SaveValidationBeforeMerge
        );
        string bodyKey = parameters.Keys.FirstOrDefault(predicate: key =>
            parameters[key: key] is IDataDocument
        );
        if (bodyKey == null)
        {
            throw new Exception(message: Resources.ErrorPseudoparameterBodyNotDefined);
        }
        UserProfile profile = SecurityManager.CurrentUserProfile();
        DataSet original = (parameters[key: bodyKey] as IDataDocument)?.DataSet;
        var mergeParams = new MergeParams(ProfileId: profile.Id) { TrueDelete = true };
        DatasetTools.MergeDataSet(
            inout_dsTarget: data.DataSet,
            in_dsSource: original,
            changeList: null,
            mergeParams: mergeParams
        );
        Validate(
            data: data,
            transformParams: transformParams,
            ruleEngine: ruleEngine,
            validation: xsltPage.SaveValidationAfterMerge
        );
        CoreServices.DataService.Instance.StoreData(
            dataStructureId: xsltPage.DataStructureId,
            data: data.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }

    private static void HandleDelete(
        XsltDataPage xsltPage,
        DataSet data,
        Hashtable transformParams,
        RuleEngine ruleEngine
    )
    {
        IXmlContainer xmlContainer = new XmlContainer();
        xmlContainer.Xml.LoadXml(xml: data.GetXml());
        Validate(
            data: xmlContainer,
            transformParams: transformParams,
            ruleEngine: ruleEngine,
            validation: xsltPage.SaveValidationBeforeMerge
        );
        foreach (DataTable table in data.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                row.Delete();
            }
        }
        CoreServices.DataService.Instance.StoreData(
            dataStructureId: xsltPage.DataStructureId,
            data: data,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }
}
