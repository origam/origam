#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

        public static void ComputeXsltValueParameters(AbstractReport report, Hashtable parameters, TraceTaskInfo traceTaskInfo = null)
        {
            if (parameters == null) return;
            IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            RuleEngine ruleEngine = new RuleEngine(null, null);

            Hashtable transformParams = new Hashtable();



            foreach (object o in report.Parameters)
            {
                // send all ordinary parameters as an input to the Xslt
                if (!(o is XsltInitialValueParameter))
                {
                    SchemaItemParameter sp = o as SchemaItemParameter;
                    if (sp != null)
                    {
                        transformParams.Add(sp.Name, parameters[sp.Name]);
                    }
                }
            }

            string oldStepName = null;
            if (traceTaskInfo != null)
            {
                oldStepName = traceTaskInfo.TraceStepName;
            }

            foreach (object o in report.Parameters)
            {
                XsltInitialValueParameter xslValueParam = o as XsltInitialValueParameter;
                if (xslValueParam == null) continue;

                // do not recompute parameters if they were sent and they have some value
                if (parameters.ContainsKey(xslValueParam.Name) && parameters[xslValueParam.Name] != null) continue;

                IXsltEngine transformer = AsTransform.GetXsltEngine(persistence.SchemaProvider, xslValueParam.transformationId);

                IDataDocument xmlData = DataDocumentFactory.New();
                xmlData.Xml.LoadXml("<ROOT/>");

                if (traceTaskInfo != null)
                {
                    traceTaskInfo.TraceStepName = String.Format(
                        "{0}/ComputeParam_{1}", oldStepName,
                        xslValueParam.Name);
                    transformer.SetTraceTaskInfo(traceTaskInfo);
                }

                IDataDocument result = transformer.Transform(xmlData,
                    xslValueParam.transformationId,
                    Guid.Empty, transformParams, null, ruleEngine, null, false);

                // xslValueParam.DataType

                XmlNode resultNode = result.Xml.SelectSingleNode("/ROOT/value");
                // add a newlu created computed parameter
                if (resultNode == null)
                {
                    parameters.Add(xslValueParam.Name, null);
                }
                else
                {
                    object valueToContext = resultNode.InnerText;
                    RuleEngine.ConvertStringValueToContextValue(xslValueParam.DataType, resultNode.InnerText, ref valueToContext);
                    parameters.Add(xslValueParam.Name, valueToContext);
                }
            }
            if (traceTaskInfo != null)
            {
                traceTaskInfo.TraceStepName = oldStepName;
            }
        }

        public static string ExpandCurlyBracketPlaceholdersWithParameters(string input, Hashtable parameters)
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

		public static void PopulateDefaultValues(AbstractReport report, Hashtable parameters)
		{
			IParameterService pms = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

			foreach(object o in report.Parameters)
			{
				DefaultValueParameter defaultParam = o as DefaultValueParameter;

				if(defaultParam != null)
				{
					if(parameters.Contains(defaultParam.Name))
					{
						object paramValue = parameters[defaultParam.Name];

						if(paramValue == null || paramValue == DBNull.Value)
						{
							parameters[defaultParam.Name] = pms.GetParameterValue(defaultParam.DefaultValue.Id);
						}
					}
					else
					{
						parameters[defaultParam.Name] = pms.GetParameterValue(defaultParam.DefaultValue.Id);
					}
				}
			}
		}
		public static AbstractDataReport GetReportElement(Guid reportId)
		{
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			AbstractDataReport report = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractReport), new ModelElementKey(reportId)) as AbstractDataReport;

			if (report == null)
			{
				throw new ArgumentOutOfRangeException("reportId", reportId, ResourceUtils.GetString("DefinitionNotInModel"));
			}

			return report;
		}

		public static string ResolveLanguage(IDataDocument doc, AbstractDataReport reportElement)
		{
			if (string.IsNullOrEmpty(reportElement.LocaleXPath)) return null;
			RuleEngine ruleEngine = new RuleEngine(null, null);

			//XmlDocument xPathXMLDoc = ruleEngine.   GetXmlDocumentFromData(updateTask.XPathContextStore);
			string cultureString = (string) ruleEngine.EvaluateContext(reportElement.LocaleXPath, doc, OrigamDataType.String, null);
			//reportElement.localeXPath;
			return cultureString;
		}

		public static IDataDocument LoadOrUseReportData(AbstractDataReport r,
		    IDataDocument data, Hashtable parameters, string dbTransaction)
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
//			else if (data is XmlDataDocument)
//			{
//				return data as XmlDataDocument;
//			}
//			else
//			{
//				throw new ArgumentOutOfRangeException("data", data, ResourceUtils.GetString("OnlyXmlDocSupported"));
//			}		
		    return data;
		}
	}	
}
