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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
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
                    System.IO.Path.Combine(settings.ReportsFolder(), reportName + ".xml"),
                    XmlWriteMode.WriteSchema
                );
            }
        }
        catch (Exception e)
        {
            ReportHelper.LogError(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                string.Format("Can't write Crystal Report debug info: {0}", e.Message)
            );
        }
    }

    public byte[] CreateReport(Guid reportId, DataSet data, Hashtable parameters, string format)
    {
        // get report model element
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        parameters = PrepareParameters(data, parameters, report);
        // get report
        string paramString = $"&format={format}";
        string reportFileName = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
            report.ReportFileName,
            parameters
        );
        object result = SendReportRequest(
            "Report",
            reportFileName,
            data,
            parameters,
            report,
            paramString
        );
        if (result is byte[] bytes)
        {
            return bytes;
        }
        throw new Exception("Invalid data returned. Expected byte array.");
    }

    public string PrepareReport(
        CrystalReport report,
        DataSet data,
        Hashtable parameters,
        string format
    )
    {
        // get report model element
        parameters = PrepareParameters(data, parameters, report);
        // get report
        object result = SendReportRequest(
            "Report/PrepareViewer",
            report.ReportFileName,
            data,
            parameters,
            report,
            ""
        );
        if (result is Origam.Service.Core.XmlContainer xml)
        {
            var innerXml = xml.Xml.InnerXml;
            var settings = ConfigurationManager.GetActiveConfiguration();
            string baseUrl = ParseConnectionString(
                settings.ReportConnectionString,
                out int? timeout
            );
            string id = xml.Xml["ROOT"]["ViewerUrl"].InnerText;
            string url = baseUrl + $"ViewReport.aspx?Id={id}";
            return url;
        }
        throw new Exception("Invalid data returned. Expected byte array.");
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
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        parameters = PrepareParameters(data, parameters, report);
        // get report
        string paramString = $"&printerName={printerName}&copies={copies}";
        SendReportRequest("Print", report.ReportFileName, data, parameters, report, paramString);
    }

    private Hashtable PrepareParameters(DataSet data, Hashtable parameters, CrystalReport report)
    {
        if (parameters == null)
            parameters = new Hashtable();
        TraceReportData(data, report.Name);
        ReportHelper.PopulateDefaultValues(report, parameters);
        ReportHelper.ComputeXsltValueParameters(report, parameters);
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
            WriteInfoLog(reportElement, "Generating report started");
        }
        if (parameters == null)
            throw new NullReferenceException(
                ResourceUtils.GetString("CreateReport: Parameters cannot be null.")
            );
        var settings = ConfigurationManager.GetActiveConfiguration();
        string baseUrl = ParseConnectionString(settings.ReportConnectionString, out int? timeout);
        var request = new ReportRequest { Dataset = data };
        foreach (DictionaryEntry item in parameters)
        {
            request.Parameters.Add(
                new Parameter { Key = item.Key.ToString(), Value = item.Value?.ToString() }
            );
        }
        var stringBuilder = new StringBuilder();
        using (var stringWriter = new EncodingStringWriter(stringBuilder, Encoding.UTF8))
        {
            using (
                var xmlWriter = XmlWriter.Create(
                    stringWriter,
                    new XmlWriterSettings { Encoding = Encoding.UTF8 }
                )
            )
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ReportRequest));
                ser.WriteObject(xmlWriter, request);
            }
        }
        var result = HttpTools
            .Instance.SendRequest(
                new Request(
                    url: baseUrl + $"api/{method}?report={fileName}{paramString}",
                    method: "POST",
                    content: stringBuilder
                        .ToString()
                        .Replace(" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"", ""),
                    contentType: "application/xml",
                    timeout: timeout
                )
            )
            .Content;
        if (log.IsInfoEnabled)
        {
            WriteInfoLog(reportElement, "Generating report finished");
        }
        return result;
    }

    private void WriteInfoLog(CrystalReport reportElement, string message)
    {
        LoggingEvent loggingEvent = new LoggingEvent(
            this.GetType(),
            log.Logger.Repository,
            log.Logger.Name,
            Level.Info,
            string.Format("{0}: {1}", message, reportElement.Name),
            null
        );
        loggingEvent.Properties["Caption"] = reportElement.Caption;
        log.Logger.Log(loggingEvent);
    }

    private static string ParseConnectionString(string connectionString, out int? timeout)
    {
        string url = null;
        timeout = null;
        string[] parts = connectionString.Split(
            ";".ToCharArray(),
            StringSplitOptions.RemoveEmptyEntries
        );
        if (parts.Length == 0)
        {
            throw new Exception("Crystal Reports connection string is empty.");
        }
        foreach (var item in parts)
        {
            string[] pair = item.Split("=".ToCharArray());
            if (pair.Length == 1)
            {
                throw new Exception(
                    "Error while parsing Crystal Reports "
                        + "connection string. '=' expected in '"
                        + pair[0]
                        + "'"
                );
            }
            string identifier = pair[0].ToLower().Trim();
            switch (identifier)
            {
                case "url":
                    url = pair[1];
                    break;
                case "timeout":
                    int timeoutInternal;
                    if (int.TryParse(pair[1], out timeoutInternal))
                    {
                        timeout = timeoutInternal;
                    }
                    else
                    {
                        throw new Exception(
                            "Error occured while trying to "
                                + "parse Crystal Reports connection string timeout"
                                + " value '"
                                + pair[1]
                                + "'."
                        );
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown Crystal"
                            + " Reports connection string identifier '"
                            + identifier
                            + "'"
                    );
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(
                    "Crystal Reports connection string " + "does not contain 'url' setting."
                );
            }
        }
        return url;
    }
}
