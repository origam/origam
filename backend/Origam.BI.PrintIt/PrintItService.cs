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
using System.Xml;
using System.Collections;
using Origam.JSON;
using Origam.Rule;

using Origam.Schema.GuiModel;
using System.IO;
using Origam.Rule.Xslt;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.BI.PrintIt
{
	public class PrintItService : IReportService
	{		
		public object GetReport(Guid reportId, IXmlContainer data, string format, Hashtable parameters, string dbTransaction)
		{
			
			if (format != DataReportExportFormatType.PDF.ToString())
			{
				throw new ArgumentOutOfRangeException("format", format, ResourceUtils.GetString("FormatNotSupported"));
			}

			var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId);
            ReportHelper.PopulateDefaultValues(report, parameters);
            ReportHelper.ComputeXsltValueParameters(report, parameters);

            IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);

			using (LanguageSwitcher langSwitcher = new LanguageSwitcher(ReportHelper.ResolveLanguage(xmlDataDoc, report)))
			{

                // optional xslt transformation
			    IXmlContainer result = null;

				if (report.Transformation != null)
				{
					IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
					IXsltEngine transformer = AsTransform.GetXsltEngine(
                        persistence.SchemaProvider, report.TransformationId);
					//Hashtable transformParams = new Hashtable();
					//QueryParameterCollection qparams = new QueryParameterCollection();
					//Hashtable preprocessorParams = GetPreprocessorParameters(request);

					result = transformer.Transform(xmlDataDoc, report.TransformationId, parameters, null,  null, false);
				}
				else
				{
					result = xmlDataDoc;
				}

				// convert to JSON
				using (MemoryStream stream = new MemoryStream())
				{
					string postData = null;
					using (StreamWriter writer = new StreamWriter(stream))
					{
						JsonUtils.SerializeToJson(writer, result, true);
						//JsonUtils.SerializeToJson(response, result, xsltPage.OmitJsonRootElement);

						writer.Flush();
						stream.Position = 0;
						StreamReader reader = new StreamReader(stream);
						string jsonString = reader.ReadToEnd();
						TraceReportData(jsonString, report.ReportFileName);
						postData = string.Format("jargs={0}", HttpTools.Instance.EscapeDataStringLong(jsonString));
					}

					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
					string serviceUrl = settings.PrintItServiceUrl;
					// send request to PrintIt service
					return HttpTools.Instance.SendRequest(
						new Request(
							url: serviceUrl,
							method: "POST",
							content: postData, 
							contentType: "application/x-www-form-urlencoded")
						).Content;
				}
			}
		}

		private void TraceReportData(string data, string reportName)
		{
			try
			{
				OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;

				string path = System.IO.Path.Combine(settings.ReportsFolder(), reportName + ".json");

				using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
				{
					file.Write(data);
					file.Close();
				}
				
			}
			catch (Exception e)
			{
				ReportHelper.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
					string.Format("Can't write PrintIt Report debug info: {0}", e.Message));								
			}
		}
		
		public void PrintReport(Guid reportId, IXmlContainer data, string printerName, int copies, Hashtable parameters)
		{
			throw new NotSupportedException();
		}
        public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
        {
            // do nothing unless we need to trace something
        }

        public string PrepareExternalReportViewer(Guid reportId,
			IXmlContainer data, string format,
			Hashtable parameters, string dbTransaction)
        {
            throw new NotImplementedException();
        }
    }
}
