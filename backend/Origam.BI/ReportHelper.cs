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
using Origam.DA;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.BI;

public static class ReportHelper
{
    public static void LogInfo(Type type, string message)
    {
        var log = log4net.LogManager.GetLogger(type: type);
        if (log.IsInfoEnabled)
        {
            log.Info(message: message);
        }
    }

    public static void ComputeXsltValueParameters(
        AbstractReport report,
        Hashtable parameters,
        TraceTaskInfo traceTaskInfo = null
    )
    {
        if (parameters == null)
        {
            return;
        }
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var transformParams = new Hashtable();
        foreach (SchemaItemParameter parameter in report.Parameters)
        {
            // send all ordinary parameters as an input to the Xslt
            if (parameter is not null and not XsltInitialValueParameter)
            {
                transformParams.Add(key: parameter.Name, value: parameters[key: parameter.Name]);
            }
        }
        string oldStepName = null;
        if (traceTaskInfo != null)
        {
            oldStepName = traceTaskInfo.TraceStepName;
        }
        foreach (SchemaItemParameter parameter in report.Parameters)
        {
            if (parameter is not XsltInitialValueParameter xsltParameter)
            {
                continue;
            }
            // do not recompute parameters if they were sent and they have some value
            if (
                parameters.ContainsKey(key: xsltParameter.Name)
                && (parameters[key: xsltParameter.Name] != null)
            )
            {
                continue;
            }
            IXsltEngine transformer = new CompiledXsltEngine(
                persistence: persistence.SchemaProvider
            );
            IXmlContainer xmlData = new XmlContainer(xmlString: "<ROOT/>");
            if (traceTaskInfo != null)
            {
                traceTaskInfo.TraceStepName = $"{oldStepName}/ComputeParam_{xsltParameter.Name}";
                transformer.SetTraceTaskInfo(traceTaskInfo: traceTaskInfo);
            }
            IXmlContainer result = transformer.Transform(
                data: xmlData,
                transformationId: xsltParameter.transformationId,
                retransformationId: Guid.Empty,
                parameters: transformParams,
                transactionId: null,
                retransformationParameters: null,
                outputStructure: null,
                validateOnly: false
            );
            var resultNode = result.Xml.SelectSingleNode(xpath: "/ROOT/value");
            // add a newly created computed parameter
            if (resultNode == null)
            {
                parameters.Add(key: xsltParameter.Name, value: null);
            }
            else
            {
                object valueToContext = resultNode.InnerText;
                RuleEngine.ConvertStringValueToContextValue(
                    origamDataType: xsltParameter.DataType,
                    inputString: resultNode.InnerText,
                    contextValue: ref valueToContext
                );
                parameters.Add(key: xsltParameter.Name, value: valueToContext);
            }
        }
        if (traceTaskInfo != null)
        {
            traceTaskInfo.TraceStepName = oldStepName;
        }
    }

    public static string BuildFileSystemReportFilePath(string filePath, Hashtable parameters)
    {
        foreach (DictionaryEntry entry in parameters)
        {
            var key = entry.Key.ToString();
            string value = null;
            if (entry.Value != null)
            {
                value = entry.Value.ToString();
            }
            var replacement = "{" + key + "}";
            if (
                filePath.IndexOf(value: replacement, comparisonType: StringComparison.Ordinal) <= -1
            )
            {
                continue;
            }
            if (value == null)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ParametersDontMatch"));
            }
            filePath = filePath.Replace(oldValue: replacement, newValue: value);
        }
        return filePath;
    }

    public static string ExpandCurlyBracketPlaceholdersWithParameters(
        string input,
        Hashtable parameters
    )
    {
        string output = input;
        foreach (DictionaryEntry entry in parameters)
        {
            var key = entry.Key.ToString();
            var value = "";
            if (entry.Value != null)
            {
                value = entry.Value.ToString();
            }
            var replacement = "{" + key + "}";
            if (output.IndexOf(value: replacement, comparisonType: StringComparison.Ordinal) > -1)
            {
                output = output.Replace(oldValue: replacement, newValue: value);
            }
        }
        return output;
    }

    public static void LogError(Type type, string message)
    {
        var log = log4net.LogManager.GetLogger(type: type);
        if (log.IsErrorEnabled)
        {
            log.Error(message: message);
        }
    }

    public static void PopulateDefaultValues(AbstractReport report, Hashtable parameters)
    {
        var parameterService = ServiceManager.Services.GetService<IParameterService>();
        foreach (var parameter in report.Parameters)
        {
            if (parameter is not DefaultValueParameter defaultParam)
            {
                continue;
            }
            if (parameters.Contains(key: defaultParam.Name))
            {
                object paramValue = parameters[key: defaultParam.Name];
                if ((paramValue == null) || (paramValue == DBNull.Value))
                {
                    parameters[key: defaultParam.Name] = parameterService.GetParameterValue(
                        id: defaultParam.DefaultValue.Id
                    );
                }
            }
            else
            {
                parameters[key: defaultParam.Name] = parameterService.GetParameterValue(
                    id: defaultParam.DefaultValue.Id
                );
            }
        }
    }

    public static T GetReportElement<T>(Guid reportId)
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var report = persistence.SchemaProvider.RetrieveInstance<T>(instanceId: reportId);
        if (report == null)
        {
            throw new ArgumentException(
                message: nameof(reportId),
                paramName: ResourceUtils.GetString(key: "DefinitionNotInModel")
            );
        }
        return report;
    }

    public static string ResolveLanguage(IXmlContainer doc, AbstractDataReport reportElement)
    {
        if (string.IsNullOrEmpty(value: reportElement.LocaleXPath))
        {
            return null;
        }
        RuleEngine ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
        var cultureString = (string)
            ruleEngine.EvaluateContext(
                xpath: reportElement.LocaleXPath,
                context: doc,
                dataType: OrigamDataType.String,
                targetStructure: null
            );
        return cultureString;
    }

    public static IDataDocument LoadOrUseReportData(
        AbstractDataReport report,
        IXmlContainer data,
        Hashtable parameters,
        string dbTransaction
    )
    {
        switch (data)
        {
            case null when report.DataStructure != null:
            {
                var queryParameterCollection = new QueryParameterCollection();
                if (parameters != null)
                {
                    foreach (DictionaryEntry entry in parameters)
                    {
                        queryParameterCollection.Add(
                            value: new QueryParameter(
                                _parameterName: (string)entry.Key,
                                value: entry.Value
                            )
                        );
                    }
                }
                return DataDocumentFactory.New(
                    dataSet: core.DataService.Instance.LoadData(
                        dataStructureId: report.DataStructureId,
                        methodId: report.DataStructureMethodId,
                        defaultSetId: Guid.Empty,
                        sortSetId: report.DataStructureSortSetId,
                        transactionId: dbTransaction,
                        parameters: queryParameterCollection
                    )
                );
            }
            case IDataDocument document:
            {
                return document;
            }
            default:
            {
                throw new ArgumentException(
                    message: nameof(data),
                    paramName: ResourceUtils.GetString(key: "OnlyXmlDocSupported")
                );
            }
        }
    }
}
