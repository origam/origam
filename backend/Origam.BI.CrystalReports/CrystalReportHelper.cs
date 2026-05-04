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
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using log4net.Core;
using Origam.CrystalReportsService.Models;
using Origam.Schema.GuiModel;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for CrystalReportHelper.
/// </summary>
public class CrystalReportHelper
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public CrystalReportHelper() { }

    #region Public Functions
    private void TraceReportData(DataSet data, string reportName)
    {
        try
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            if (data != null)
            {
                data.WriteXml(
                    fileName: System.IO.Path.Combine(
                        path1: settings.ReportsFolder(),
                        path2: reportName + ".xml"
                    ),
                    mode: XmlWriteMode.WriteSchema
                );
            }
        }
        catch (Exception e)
        {
            ReportHelper.LogError(
                type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                message: string.Format(
                    format: "Can't write Crystal Report debug info: {0}",
                    arg0: e.Message
                )
            );
        }
    }

    public byte[] CreateReport(Guid reportId, DataSet data, Hashtable parameters, string format)
    {
        // get report model element
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        parameters = PrepareParameters(data: data, parameters: parameters, report: report);
        // get report
        string paramString = $"&format={format}";
        string reportFileName = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
            input: report.ReportFileName,
            parameters: parameters
        );
        object result = SendReportRequest(
            method: "Report",
            fileName: reportFileName,
            data: data,
            parameters: parameters,
            reportElement: report,
            paramString: paramString
        );
        if (result is byte[] bytes)
        {
            return bytes;
        }
        throw new Exception(message: "Invalid data returned. Expected byte array.");
    }

    public string PrepareReport(
        CrystalReport report,
        DataSet data,
        Hashtable parameters,
        string format
    )
    {
        // get report model element
        parameters = PrepareParameters(data: data, parameters: parameters, report: report);
        // get report
        object result = SendReportRequest(
            method: "Report/PrepareViewer",
            fileName: report.ReportFileName,
            data: data,
            parameters: parameters,
            reportElement: report,
            paramString: ""
        );
        if (result is Origam.Service.Core.XmlContainer xml)
        {
            var innerXml = xml.Xml.InnerXml;
            var settings = ConfigurationManager.GetActiveConfiguration();
            string baseUrl = ParseConnectionString(
                connectionString: settings.ReportConnectionString,
                timeout: out int? timeout
            );
            string id = xml.Xml[name: "ROOT"][name: "ViewerUrl"].InnerText;
            string url = baseUrl + $"ViewReport.aspx?Id={id}";
            return url;
        }
        throw new Exception(message: "Invalid data returned. Expected byte array.");
    }

    public void PrintReport(
        Guid reportId,
        DataSet data,
        Hashtable parameters,
        string printerName,
        int copies
    )
    {
        // get report model element
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        parameters = PrepareParameters(data: data, parameters: parameters, report: report);
        // get report
        string paramString = $"&printerName={printerName}&copies={copies}";
        SendReportRequest(
            method: "Print",
            fileName: report.ReportFileName,
            data: data,
            parameters: parameters,
            reportElement: report,
            paramString: paramString
        );
    }

    private Hashtable PrepareParameters(DataSet data, Hashtable parameters, CrystalReport report)
    {
        if (parameters == null)
        {
            parameters = new Hashtable();
        }

        TraceReportData(data: data, reportName: report.Name);
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(report: report, parameters: parameters);
        return parameters;
    }
    #endregion
    public object SendReportRequest(
        string method,
        string fileName,
        DataSet data,
        Hashtable parameters,
        CrystalReport reportElement,
        string paramString
    )
    {
        if (log.IsInfoEnabled)
        {
            WriteInfoLog(reportElement: reportElement, message: "Generating report started");
        }
        if (parameters == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "CreateReport: Parameters cannot be null.")
            );
        }

        var settings = ConfigurationManager.GetActiveConfiguration();
        string baseUrl = ParseConnectionString(
            connectionString: settings.ReportConnectionString,
            timeout: out int? timeout
        );
        var request = new ReportRequest { Dataset = data };
        foreach (DictionaryEntry item in parameters)
        {
            request.Parameters.Add(
                item: new Parameter { Key = item.Key.ToString(), Value = item.Value?.ToString() }
            );
        }
        var stringBuilder = new StringBuilder();
        using (
            var stringWriter = new EncodingStringWriter(
                builder: stringBuilder,
                encoding: Encoding.UTF8
            )
        )
        {
            using (
                var xmlWriter = XmlWriter.Create(
                    output: stringWriter,
                    settings: new XmlWriterSettings { Encoding = Encoding.UTF8 }
                )
            )
            {
                DataContractSerializer ser = new DataContractSerializer(
                    type: typeof(ReportRequest)
                );
                ser.WriteObject(writer: xmlWriter, graph: request);
            }
        }
        var result = HttpTools
            .Instance.SendRequest(
                request: new Request(
                    url: baseUrl + $"api/{method}?report={fileName}{paramString}",
                    method: "POST",
                    content: stringBuilder
                        .ToString()
                        .Replace(
                            oldValue: " xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"",
                            newValue: ""
                        ),
                    contentType: "application/xml",
                    timeout: timeout
                )
            )
            .Content;
        if (log.IsInfoEnabled)
        {
            WriteInfoLog(reportElement: reportElement, message: "Generating report finished");
        }
        return result;
    }

    private void WriteInfoLog(CrystalReport reportElement, string message)
    {
        LoggingEvent loggingEvent = new LoggingEvent(
            callerStackBoundaryDeclaringType: this.GetType(),
            repository: log.Logger.Repository,
            loggerName: log.Logger.Name,
            level: Level.Info,
            message: string.Format(format: "{0}: {1}", arg0: message, arg1: reportElement.Name),
            exception: null
        );
        loggingEvent.Properties[key: "Caption"] = reportElement.Caption;
        log.Logger.Log(logEvent: loggingEvent);
    }

    private static string ParseConnectionString(string connectionString, out int? timeout)
    {
        string url = null;
        timeout = null;
        string[] parts = connectionString.Split(
            separator: ";".ToCharArray(),
            options: StringSplitOptions.RemoveEmptyEntries
        );
        if (parts.Length == 0)
        {
            throw new Exception(message: "Crystal Reports connection string is empty.");
        }
        foreach (var item in parts)
        {
            string[] pair = item.Split(separator: "=".ToCharArray());
            if (pair.Length == 1)
            {
                throw new Exception(
                    message: "Error while parsing Crystal Reports "
                        + "connection string. '=' expected in '"
                        + pair[0]
                        + "'"
                );
            }
            string identifier = pair[0].ToLower().Trim();
            switch (identifier)
            {
                case "url":
                {
                    url = pair[1];
                    break;
                }

                case "timeout":
                {
                    int timeoutInternal;
                    if (int.TryParse(s: pair[1], result: out timeoutInternal))
                    {
                        timeout = timeoutInternal;
                    }
                    else
                    {
                        throw new Exception(
                            message: "Error occured while trying to "
                                + "parse Crystal Reports connection string timeout"
                                + " value '"
                                + pair[1]
                                + "'."
                        );
                    }
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Unknown Crystal"
                            + " Reports connection string identifier '"
                            + identifier
                            + "'"
                    );
                }
            }
            if (string.IsNullOrEmpty(value: url))
            {
                throw new Exception(
                    message: "Crystal Reports connection string "
                        + "does not contain 'url' setting."
                );
            }
        }
        return url;
    }
}
