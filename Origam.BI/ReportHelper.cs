#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Xml;
using System.Collections;
using Origam.Rule;

using Origam.Schema;
using Origam.DA;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.BI
{
	/// <summary>
	/// Summary description for ReportHelper.
	/// </summary>
	public class ReportHelper
	{
		public static void LogInfo(Type type, string message)
		{
			log4net.ILog log = log4net.LogManager.GetLogger(type);

			if(log.IsInfoEnabled)
			{
				log.Info(message);
			}
		}

        public static void ComputeXsltValueParameters(AbstractReport report, 
            Hashtable parameters, TraceTaskInfo traceTaskInfo = null)
        {
            if (parameters == null) return;
            IPersistenceService persistence = ServiceManager.Services
                .GetService(typeof(IPersistenceService)) as IPersistenceService;
            RuleEngine ruleEngine = new RuleEngine(null, null);

            var transformParams = new Hashtable();
            foreach (SchemaItemParameter parameter in report.Parameters)
            {
                // send all ordinary parameters as an input to the Xslt
                if (parameter != null && !(parameter is XsltInitialValueParameter))
                {
                    transformParams.Add(parameter.Name, parameters[parameter.Name]);
                }
            }
            string oldStepName = null;
            if (traceTaskInfo != null)
            {
                oldStepName = traceTaskInfo.TraceStepName;
            }
            foreach (SchemaItemParameter parameter in report.Parameters)
            {
                XsltInitialValueParameter xsltParameter = parameter as XsltInitialValueParameter;
                if (xsltParameter == null)
                {
                    continue;
                }
                // do not recompute parameters if they were sent and they have some value
                if (parameters.ContainsKey(xsltParameter.Name) 
                    && parameters[xsltParameter.Name] != null)
                {
                    continue;
                }
                IXsltEngine transformer = AsTransform.GetXsltEngine(
                    persistence.SchemaProvider,
                    xsltParameter.transformationId);
                IXmlContainer xmlData = new XmlContainer("<ROOT/>");
                if (traceTaskInfo != null)
                {
                    traceTaskInfo.TraceStepName = string.Format(
                        "{0}/ComputeParam_{1}", oldStepName,
                        xsltParameter.Name);
                    transformer.SetTraceTaskInfo(traceTaskInfo);
                }
                IXmlContainer result = transformer.Transform(xmlData,
                    xsltParameter.transformationId,
                    Guid.Empty, transformParams, null, ruleEngine, null, false);
                XmlNode resultNode = result.Xml.SelectSingleNode("/ROOT/value");
                // add a newlu created computed parameter
                if (resultNode == null)
                {
                    parameters.Add(xsltParameter.Name, null);
                }
                else
                {
                    object valueToContext = resultNode.InnerText;
                    RuleEngine.ConvertStringValueToContextValue(
                        xsltParameter.DataType, resultNode.InnerText, ref valueToContext);
                    parameters.Add(xsltParameter.Name, valueToContext);
                }
            }
            if (traceTaskInfo != null)
            {
                traceTaskInfo.TraceStepName = oldStepName;
            }
        }

        public static string BuildFileSystemReportFilePath(
            string filePath, Hashtable parameters)
        {
            foreach (DictionaryEntry entry in parameters)
            {
                string sKey = entry.Key.ToString();
                string sValue = null;
                if (entry.Value != null)
                {
                    sValue = entry.Value.ToString();
                }
                string replacement = "{" + sKey + "}";
                if (filePath.IndexOf(replacement) > -1)
                {
                    if (sValue == null)
                    {
                        throw new Exception(
                            "parametry nesedi");
                    }
                    filePath = filePath.Replace(replacement, sValue);
                }
            }
            return filePath;
        }

        public static string ExpandCurlyBracketPlaceholdersWithParameters(
            string input, Hashtable parameters)
        {
            string output = input;

            foreach (DictionaryEntry entry in parameters)
            {
                string sKey = entry.Key.ToString();
                string sValue = "";
                if (entry.Value != null)
                {
                    sValue = entry.Value.ToString();
                }
                string replacement = "{" + sKey + "}";
                if (output.IndexOf(replacement) > -1)
                {
                    output = output.Replace(replacement, sValue);
                }
            }
            return output;
        }

        public static void LogError(Type type, string message)
		{
			log4net.ILog log = log4net.LogManager.GetLogger(type);

			if (log.IsErrorEnabled)
			{
				log.Error(message);
			}
		}

		public static void PopulateDefaultValues(AbstractReport report, 
            Hashtable parameters)
		{
			IParameterService pms = ServiceManager.Services
                .GetService(typeof(IParameterService)) as IParameterService;

			foreach(object parameter in report.Parameters)
			{
                if (parameter is DefaultValueParameter defaultParam)
                {
                    if (parameters.Contains(defaultParam.Name))
                    {
                        object paramValue = parameters[defaultParam.Name];
                        if (paramValue == null || paramValue == DBNull.Value)
                        {
                            parameters[defaultParam.Name] = 
                                pms.GetParameterValue(defaultParam.DefaultValue.Id);
                        }
                    }
                    else
                    {
                        parameters[defaultParam.Name] =
                            pms.GetParameterValue(defaultParam.DefaultValue.Id);
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
			if (report == null)
			{
				throw new ArgumentOutOfRangeException(
                    "reportId",
                    reportId, 
                    ResourceUtils.GetString("DefinitionNotInModel"));
			}
			return report;
		}

		public static string ResolveLanguage(IXmlContainer doc, AbstractDataReport reportElement)
		{
			if (string.IsNullOrEmpty(reportElement.LocaleXPath)) return null;
			RuleEngine ruleEngine = new RuleEngine(null, null);

			//XmlDocument xPathXMLDoc = ruleEngine.   GetXmlDocumentFromData(updateTask.XPathContextStore);
			string cultureString = (string) ruleEngine.EvaluateContext(reportElement.LocaleXPath, doc, OrigamDataType.String, null);
			//reportElement.localeXPath;
			return cultureString;
		}

		public static IDataDocument LoadOrUseReportData(AbstractDataReport r,
		    IXmlContainer data, Hashtable parameters, string dbTransaction)
		{
			if (data == null && r.DataStructure != null)
			{
				QueryParameterCollection qp = new QueryParameterCollection();
                if (parameters != null)
                {
                    foreach (DictionaryEntry entry in parameters)
                    {
                        qp.Add(new QueryParameter((string)entry.Key, entry.Value));
                    }
                }
				return DataDocumentFactory.New(
                    core.DataService.LoadData(r.DataStructureId,
                    r.DataStructureMethodId, Guid.Empty, 
                    r.DataStructureSortSetId, dbTransaction, qp));
			}
			if (data is IDataDocument)
			{
				return data as IDataDocument;
			}
		    throw new ArgumentOutOfRangeException("data", data, ResourceUtils.GetString("OnlyXmlDocSupported"));	
		}
	}	
}
