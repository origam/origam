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
using System.IO;
using Origam.JSON;
using Origam.Rule.Xslt;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.BI.PrintIt;

public class PrintItService : IReportService
{
    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        if (format != DataReportExportFormatType.PDF.ToString())
        {
            throw new ArgumentOutOfRangeException(
                paramName: "format",
                actualValue: format,
                message: ResourceUtils.GetString(key: "FormatNotSupported")
            );
        }
        var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId: reportId);
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(report: report, parameters: parameters);
        IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        using (
            LanguageSwitcher langSwitcher = new LanguageSwitcher(
                langIetf: ReportHelper.ResolveLanguage(doc: xmlDataDoc, reportElement: report)
            )
        )
        {
            // optional xslt transformation
            IXmlContainer result = null;
            if (report.Transformation != null)
            {
                IPersistenceService persistence =
                    ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                    as IPersistenceService;
                IXsltEngine transformer = new CompiledXsltEngine(
                    persistence: persistence.SchemaProvider
                );
                //Hashtable transformParams = new Hashtable();
                //QueryParameterCollection qparams = new QueryParameterCollection();
                //Hashtable preprocessorParams = GetPreprocessorParameters(request);
                result = transformer.Transform(
                    data: xmlDataDoc,
                    transformationId: report.TransformationId,
                    parameters: parameters,
                    transactionId: null,
                    outputStructure: null,
                    validateOnly: false
                );
            }
            else
            {
                result = xmlDataDoc;
            }
            // convert to JSON
            using (MemoryStream stream = new MemoryStream())
            {
                string postData = null;
                using (StreamWriter writer = new StreamWriter(stream: stream))
                {
                    JsonUtils.SerializeToJson(
                        textWriter: writer,
                        value: result,
                        omitRootElement: true
                    );
                    //JsonUtils.SerializeToJson(response, result, xsltPage.OmitJsonRootElement);
                    writer.Flush();
                    stream.Position = 0;
                    StreamReader reader = new StreamReader(stream: stream);
                    string jsonString = reader.ReadToEnd();
                    TraceReportData(
                        data: jsonString,
                        reportName: report.ReportFileName,
                        parameters: parameters
                    );
                    postData = string.Format(
                        format: "jargs={0}",
                        arg0: HttpTools.Instance.EscapeDataStringLong(value: jsonString)
                    );
                }
                OrigamSettings settings =
                    ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
                string serviceUrl = settings.PrintItServiceUrl;
                // send request to PrintIt service
                return HttpTools
                    .Instance.SendRequest(
                        request: new Request(
                            url: serviceUrl,
                            method: "POST",
                            content: postData,
                            contentType: "application/x-www-form-urlencoded"
                        )
                    )
                    .Content;
            }
        }
    }

    private void TraceReportData(string data, string reportName, Hashtable parameters)
    {
        try
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            string path = Path.Combine(
                path1: settings.ReportsFolder(),
                path2: ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
                    input: reportName,
                    parameters: parameters
                ) + ".json"
            );
            if (!IOTools.IsSubPathOf(path: path, basePath: settings.ReportsFolder()))
            {
                throw new Exception(message: Strings.PathNotOnReportPath);
            }
            using (StreamWriter file = new StreamWriter(path: path))
            {
                file.Write(value: data);
                file.Close();
            }
        }
        catch (Exception e)
        {
            ReportHelper.LogError(
                type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                message: string.Format(
                    format: "Can't write PrintIt Report debug info: {0}",
                    arg0: e.Message
                )
            );
        }
    }

    public void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        throw new NotSupportedException();
    }

    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
        // do nothing unless we need to trace something
    }

    public string PrepareExternalReportViewer(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        throw new NotImplementedException();
    }
}
