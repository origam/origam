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
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Origam.DA;
using Origam.JSON;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
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
        Hashtable preprocessorParams = GetPreprocessorParameters(request);
        // convert parameters to QueryParameterCollection for data service and hashtable for transformation service
        foreach (KeyValuePair<string, object> parameter in parameters)
        {
            queryParameterCollection.Add(new QueryParameter(parameter.Key, parameter.Value));
            transformParams.Add(parameter.Key, parameter.Value);
        }
        // copy also the preprocessor parameters to the transformation parameters
        foreach (DictionaryEntry parameter in preprocessorParams)
        {
            transformParams.Add(parameter.Key, parameter.Value);
        }
        Validate(data: null, transformParams, ruleEngine, xsltPage!.InputValidationRule);
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
        bool xpath = !string.IsNullOrEmpty(xsltPage.ResultXPath);
        if (xsltPage.DataStructure == null)
        {
            // no data source
            xmlData = new XmlContainer("<ROOT/>");
        }
        else
        {
            if (
                xsltPage.AllowCustomFilters
                && parameters.ContainsKey(XsltDataPage.FiltersParameterName)
            )
            {
                data = LoadWithFilters(xsltPage, parameters);
            }
            else
            {
                data = CoreServices.DataService.Instance.LoadData(
                    xsltPage.DataStructureId,
                    xsltPage.DataStructureMethodId,
                    defaultSetId: Guid.Empty,
                    xsltPage.DataStructureSortSetId,
                    transactionId: null,
                    queryParameterCollection
                );
            }

            if ((request.HttpMethod != "DELETE") && (request.HttpMethod != "PUT"))
            {
                if (xsltPage.ProcessReadFieldRowLevelRulesForGetRequests)
                {
                    ProcessReadFieldRuleState(data, ruleEngine);
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
                response.WriteToOutput(textWriter =>
                    JsonUtils.SerializeToJson(
                        textWriter,
                        data,
                        xsltPage.OmitJsonRootElement,
                        xsltPage.OmitJsonMainElement
                    )
                );
                xmlData = null;
                isProcessed = true;
            }
            else
            {
                xmlData = DataDocumentFactory.New(data);
            }
            switch (request.HttpMethod)
            {
                case "DELETE":
                {
                    HandleDelete(xsltPage, data, transformParams, ruleEngine);
                    return;
                }
                case "PUT":
                {
                    HandlePut(
                        parameters,
                        xsltPage,
                        (IDataDocument)xmlData,
                        transformParams,
                        ruleEngine
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
                var transformer = new CompiledXsltEngine(persistenceService.SchemaProvider);
                result = transformer.Transform(
                    xmlData,
                    xsltPage.TransformationId,
                    retransformationId: new Guid("5b4f2532-a0e1-4ffc-9486-3f35d766af71"),
                    parameters: transformParams,
                    transactionId: null,
                    retransformationParameters: preprocessorParams,
                    xsltPage.TransformationOutputStructure,
                    validateOnly: false
                );
                // pure dataset > json serialization
                if (
                    (result is IDataDocument resultDataDocument)
                    && !xpath
                    && (page.MimeType == MimeJson)
                )
                {
                    response.WriteToOutput(textWriter =>
                        JsonUtils.SerializeToJson(
                            textWriter,
                            resultDataDocument.DataSet,
                            xsltPage.OmitJsonRootElement,
                            xsltPage.OmitJsonMainElement
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
                    xPathNavigator!.Select(xsltPage.ResultXPath);
                    byte[] bytes = Encoding.UTF8.GetBytes(xPathNavigator.Value);
                    response.AddHeader("Content-Length", bytes.LongLength.ToString());
                    response.BinaryWrite(bytes);
                }
                else
                {
                    if (page.MimeType == MimeJson)
                    {
                        response.WriteToOutput(textWriter =>
                            JsonUtils.SerializeToJson(
                                textWriter,
                                result.Xml,
                                xsltPage.OmitJsonRootElement,
                                xsltPage.OmitJsonMainElement
                            )
                        );
                    }
                    else
                    {
                        if (page.MimeType == MimeHtml)
                        {
                            response.Write("<!DOCTYPE html>");
                        }
                        response.WriteToOutput(textWriter => textWriter.Write(result.Xml.InnerXml));
                    }
                }
            }
        }
        if (!Analytics.Instance.IsAnalyticsEnabled || (xsltPage.LogTransformation == null))
        {
            return;
        }
        xmlData ??= DataDocumentFactory.New(data);
        Type type = GetType();
        IXsltEngine logTransformer = new CompiledXsltEngine(persistenceService.SchemaProvider);
        IXmlContainer log = logTransformer.Transform(
            xmlData,
            xsltPage.LogTransformationId,
            transformParams,
            transactionId: null,
            outputStructure: null,
            validateOnly: false
        );
        XPathNavigator logXPathNavigator = log.Xml.CreateNavigator();
        XPathNodeIterator xPathNodeIterator = logXPathNavigator!.Select("/ROOT/LogContext");
        while (xPathNodeIterator.MoveNext())
        {
            var properties = new Dictionary<string, string>();
            XPathNavigator currentNode = xPathNodeIterator.Current;
            string message = currentNode!.Value == string.Empty ? "DATA_ACCESS" : currentNode.Value;
            currentNode.MoveToFirstAttribute();
            do
            {
                properties[currentNode.Name] = currentNode.Value;
            } while (currentNode.MoveToNextAttribute());
            Analytics.Instance.Log(type, message, properties);
        }
    }

    private DataSet LoadWithFilters(XsltDataPage xsltPage, Dictionary<string, object> parameters)
    {
        DataStructure dataStructure =
            persistenceService.SchemaListProvider.RetrieveInstance<DataStructure>(
                xsltPage.DataStructureId
            );
        DataStructureEntity entity = dataStructure.Entities.First();

        List<ColumnData> columns = entity
            .Columns.Select(column =>
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
        var query = new DataStructureQuery
        {
            Entity = entity.Name,
            DataSourceId = xsltPage.DataStructureId,
            RowLimit = GetIntParameterValue(parameters, "pageSize"),
            RowOffset = GetIntParameterValue(parameters, "pageNumber"),
            CustomFilters = new CustomFilters
            {
                Filters = parameters[XsltDataPage.FiltersParameterName].ToString(),
                FilterLookups = ParseFilterLookups(parameters),
            },
            MethodId = xsltPage.DataStructureMethodId,
            SortSetId = xsltPage.DataStructureSortSetId,
            CustomOrderings = new CustomOrderings(
                [new Ordering(columnName: "Date1", direction: "ASC", sortOrder: 0)]
            ),
            ColumnsInfo = new ColumnsInfo(columns: columns, renderSqlForDetachedFields: true),
            ForceDatabaseCalculation = true,
        };
        IDataService dataService = CoreServices.DataServiceFactory.GetDataService();
        IEnumerable<Dictionary<string, object>> lines = dataService.ExecuteDataReaderReturnPairs(
            query
        );

        DataSet data = dataService.GetEmptyDataSet(xsltPage.DataStructureId);
        DataTable dataTable = data.Tables[entity.Name];
        foreach (Dictionary<string, object> line in lines)
        {
            DataRow row = dataTable.NewRow();
            foreach (KeyValuePair<string, object> cell in line)
            {
                if (!dataTable.Columns.Contains(cell.Key))
                {
                    continue;
                }
                object value = cell.Value ?? DBNull.Value;
                if (
                    value is ICollection collection
                    && (
                        dataTable.Columns[cell.Key].DataType == typeof(long)
                        || dataTable.Columns[cell.Key].DataType == typeof(int)
                    )
                )
                {
                    value = collection.Count;
                }
                row[cell.Key] = value;
            }
            dataTable.Rows.Add(row);
        }

        return data;
    }

    private int GetIntParameterValue(Dictionary<string, object> parameters, string parameterName)
    {
        if (!parameters.TryGetValue(parameterName, out object objValue))
        {
            return 0;
        }
        if (!int.TryParse(objValue.ToString(), out int value))
        {
            throw new ArgumentException(
                $"{parameterName}, value: \"{objValue}\" cannot be parsed to integer"
            );
        }
        return value;
    }

    private static Dictionary<string, Guid> ParseFilterLookups(
        Dictionary<string, object> parameters
    )
    {
        if (!parameters.ContainsKey(XsltDataPage.FilterLookupsParameterName))
        {
            return new Dictionary<string, Guid>();
        }

        if (
            parameters[XsltDataPage.FilterLookupsParameterName]
            is not IEnumerable<string> lookupStrings
        )
        {
            if (parameters[XsltDataPage.FilterLookupsParameterName] is string singleParameter)
            {
                lookupStrings = [singleParameter];
            }
            else
            {
                throw new ArgumentException(
                    nameof(XsltDataPage.FilterLookupsParameterName)
                        + " parsing failed. The value is not a string or an array of strings"
                );
            }
        }

        return lookupStrings
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(lookupString => lookupString.Split(":"))
            .ToDictionary(
                x => x[0],
                x =>
                {
                    if (x.Length != 2)
                    {
                        throw new ArgumentException(
                            $"Error when parsing {nameof(XsltDataPage.FilterLookupsParameterName)}, keys and values have to be separated by \":\" "
                        );
                    }

                    if (!Guid.TryParse(x[1], out Guid lookupId))
                    {
                        throw new ArgumentException(
                            $"Error when parsing {nameof(XsltDataPage.FilterLookupsParameterName)}, key \"{x[0]}\". The value \"{x[1]}\" cannot be parsed to Guid"
                        );
                    }
                    return lookupId;
                }
            );
    }

    private void ProcessReadFieldRuleState(DataSet data, RuleEngine ruleEngine)
    {
        DataTableCollection dataTables = data.Tables;
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        foreach (DataTable dataTable in dataTables)
        {
            var entityId = (Guid)dataTable.ExtendedProperties["EntityId"]!;
            IDataEntity entity = persistence.SchemaProvider.RetrieveInstance<IDataEntity>(entityId);
            if (!entity.HasEntityAFieldDenyReadRule())
            {
                continue;
            }
            // we do this to disable constrains if column is AllowDBNull = false,
            // in order to allow field to be set DBNull if it has Deny Read rule.
            dataTable
                .Columns.Cast<DataColumn>()
                .ToList()
                .ForEach(column => column.AllowDBNull = true);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                RowSecurityState rowSecurity =
                    RowSecurityStateBuilder.BuildWithoutRelationsAndActions(ruleEngine, dataRow);
                if (rowSecurity == null)
                {
                    continue;
                }
                List<FieldSecurityState> listState = rowSecurity
                    .Columns.Where(columnState => !columnState.AllowRead)
                    .ToList();
                foreach (FieldSecurityState securityState in listState)
                {
                    dataRow[securityState.Name] = DBNull.Value;
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
        Validate(data, transformParams, ruleEngine, xsltPage.SaveValidationBeforeMerge);
        string bodyKey = parameters.Keys.FirstOrDefault(key => parameters[key] is IDataDocument);
        if (bodyKey == null)
        {
            throw new Exception(Resources.ErrorPseudoparameterBodyNotDefined);
        }
        UserProfile profile = SecurityManager.CurrentUserProfile();
        DataSet original = (parameters[bodyKey] as IDataDocument)?.DataSet;
        var mergeParams = new MergeParams(profile.Id) { TrueDelete = true };
        DatasetTools.MergeDataSet(
            inout_dsTarget: data.DataSet,
            in_dsSource: original,
            changeList: null,
            mergeParams
        );
        Validate(data, transformParams, ruleEngine, xsltPage.SaveValidationAfterMerge);
        CoreServices.DataService.Instance.StoreData(
            xsltPage.DataStructureId,
            data.DataSet,
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
        xmlContainer.Xml.LoadXml(data.GetXml());
        Validate(xmlContainer, transformParams, ruleEngine, xsltPage.SaveValidationBeforeMerge);
        foreach (DataTable table in data.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                row.Delete();
            }
        }
        CoreServices.DataService.Instance.StoreData(
            xsltPage.DataStructureId,
            data,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }
}
