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
using Origam.Rule;

using Origam.Schema;
using Origam.DA;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.BI;

public static class ReportHelper
{
    public static void LogInfo(Type type, string message)
    {
			var log = log4net.LogManager.GetLogger(type);
			if(log.IsInfoEnabled)
			{
				log.Info(message);
			}
		}

    public static void ComputeXsltValueParameters(
        AbstractReport report, 
        Hashtable parameters, 
        TraceTaskInfo traceTaskInfo = null)
    {
            if(parameters == null)
            {
                return;
            }
            var persistence = ServiceManager.Services
                .GetService<IPersistenceService>();
            var transformParams = new Hashtable();
            foreach(SchemaItemParameter parameter in report.Parameters)
            {
                // send all ordinary parameters as an input to the Xslt
                if((parameter != null) 
                && !(parameter is XsltInitialValueParameter))
                {
                    transformParams.Add(
                        parameter.Name, parameters[parameter.Name]);
                }
            }
            string oldStepName = null;
            if(traceTaskInfo != null)
            {
                oldStepName = traceTaskInfo.TraceStepName;
            }
            foreach(SchemaItemParameter parameter in report.Parameters)
            {
                var xsltParameter = parameter as XsltInitialValueParameter;
                if(xsltParameter == null)
                {
                    continue;
                }
                // do not recompute parameters if they were sent and they have some value
                if(parameters.ContainsKey(xsltParameter.Name) 
                && (parameters[xsltParameter.Name] != null))
                {
                    continue;
                }
                var transformer = AsTransform.GetXsltEngine(
                    persistence.SchemaProvider,
                    xsltParameter.transformationId);
                IXmlContainer xmlData = new XmlContainer("<ROOT/>");
                if(traceTaskInfo != null)
                {
                    traceTaskInfo.TraceStepName =
                        $"{oldStepName}/ComputeParam_{xsltParameter.Name}";
                    transformer.SetTraceTaskInfo(traceTaskInfo);
                }
                var result = transformer.Transform(
                    xmlData,
                    xsltParameter.transformationId,
                    Guid.Empty, 
                    transformParams, 
                    null,
                    null,
                    null, 
                    false);
                var resultNode = result.Xml.SelectSingleNode("/ROOT/value");
                // add a newly created computed parameter
                if(resultNode == null)
                {
                    parameters.Add(xsltParameter.Name, null);
                }
                else
                {
                    object valueToContext = resultNode.InnerText;
                    RuleEngine.ConvertStringValueToContextValue(
                        xsltParameter.DataType, resultNode.InnerText, 
                        ref valueToContext);
                    parameters.Add(xsltParameter.Name, valueToContext);
                }
            }
            if(traceTaskInfo != null)
            {
                traceTaskInfo.TraceStepName = oldStepName;
            }
        }

    public static string BuildFileSystemReportFilePath(
        string filePath, Hashtable parameters)
    {
            foreach(DictionaryEntry entry in parameters)
            {
                var key = entry.Key.ToString();
                string value = null;
                if(entry.Value != null)
                {
                    value = entry.Value.ToString();
                }
                var replacement = "{" + key + "}";
                if(filePath.IndexOf(replacement, StringComparison.Ordinal) > -1)
                {
                    if(value == null)
                    {
                        throw new Exception(ResourceUtils.GetString(
                            "ParametersDontMatch"));
                    }
                    filePath = filePath.Replace(replacement, value);
                }
            }
            return filePath;
        }

    public static string ExpandCurlyBracketPlaceholdersWithParameters(
        string input, Hashtable parameters)
    {
            var output = input;
            foreach(DictionaryEntry entry in parameters)
            {
                var key = entry.Key.ToString();
                var value = "";
                if(entry.Value != null)
                {
                    value = entry.Value.ToString();
                }
                var replacement = "{" + key + "}";
                if(output.IndexOf(replacement, StringComparison.Ordinal) > -1)
                {
                    output = output.Replace(replacement, value);
                }
            }
            return output;
        }

    public static void LogError(Type type, string message)
    {
			var log = log4net.LogManager.GetLogger(type);
			if(log.IsErrorEnabled)
			{
				log.Error(message);
			}
		}

    public static void PopulateDefaultValues(AbstractReport report, 
        Hashtable parameters)
    {
            var parameterService = ServiceManager.Services
                .GetService<IParameterService>();
			foreach(var parameter in report.Parameters)
			{
                if(parameter is DefaultValueParameter defaultParam)
                {
                    if(parameters.Contains(defaultParam.Name))
                    {
                        var paramValue = parameters[defaultParam.Name];
                        if((paramValue == null) || (paramValue == DBNull.Value))
                        {
                            parameters[defaultParam.Name] = 
                                parameterService.GetParameterValue(
                                    defaultParam.DefaultValue.Id);
                        }
                    }
                    else
                    {
                        parameters[defaultParam.Name] =
                            parameterService.GetParameterValue(
                                defaultParam.DefaultValue.Id);
                    }
                }
            }
		}
    public static T GetReportElement<T>(Guid reportId)
    {
			var persistence = ServiceManager.Services
                .GetService<IPersistenceService>();
			var report = persistence.SchemaProvider
                .RetrieveInstance<T>(reportId);
			if(report == null)
			{
				throw new ArgumentException(
                    @"reportId",
                    ResourceUtils.GetString("DefinitionNotInModel"));
			}
			return report;
		}

    public static string ResolveLanguage(
        IXmlContainer doc, AbstractDataReport reportElement)
    {
            if(string.IsNullOrEmpty(reportElement.LocaleXPath))
            {
                return null;
            }
			var ruleEngine = RuleEngine.Create(null, null);
			var cultureString = (string)ruleEngine.EvaluateContext(
                reportElement.LocaleXPath, doc, OrigamDataType.String, null);
			return cultureString;
		}

    public static IDataDocument LoadOrUseReportData(
        AbstractDataReport report,
        IXmlContainer data, 
        Hashtable parameters, 
        string dbTransaction)
    {
			switch(data)
            {
                case null when report.DataStructure != null:
                {
                    var queryParameterCollection 
                        = new QueryParameterCollection();
                    if(parameters != null)
                    {
                        foreach (DictionaryEntry entry in parameters)
                        {
                            queryParameterCollection.Add(
                                new QueryParameter(
                                    (string)entry.Key, entry.Value));
                        }
                    }
                    return DataDocumentFactory.New(
                        core.DataService.Instance.LoadData(
                            report.DataStructureId,
                            report.DataStructureMethodId, 
                            Guid.Empty, 
                            report.DataStructureSortSetId, 
                            dbTransaction, 
                            queryParameterCollection));
                }
                case IDataDocument document:
                    return document;
                default:
                    throw new ArgumentException(
                        @"data", 
                        ResourceUtils.GetString("OnlyXmlDocSupported"));
            }
        }
}