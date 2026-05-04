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
using System.Net;
using System.Reflection;
using System.Threading;
using log4net;
using Origam.BI.SSRS.SSRSWebReference;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.BI.SSRS;

public class SSRSService : IReportService
{
    private TraceTaskInfo traceTaskInfo = null;
    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );

    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        IPersistenceService persistenceService =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        SSRSReport report =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractReport),
                primaryKey: new ModelElementKey(id: reportId)
            ) as SSRSReport;
        if (report == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "reportId",
                actualValue: reportId,
                message: Strings.DefinitionNotInModel
            );
        }
        if (parameters == null)
        {
            parameters = new Hashtable();
        }
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(
            report: report,
            parameters: parameters,
            traceTaskInfo: traceTaskInfo
        );
        ReportExecutionService reportService = new ReportExecutionService();
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
        reportService.Url = settings.SQLReportServiceUrl;
        if (String.IsNullOrEmpty(value: settings.SQLReportServiceAccount))
        {
            reportService.Credentials = CredentialCache.DefaultCredentials;
        }
        else
        {
            reportService.Credentials = new NetworkCredential(
                userName: settings.SQLReportServiceAccount,
                password: settings.SQLReportServicePassword
            );
        }
        reportService.Timeout = settings.SQLReportServiceTimeout;
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "SSRSService Timeout: {0}", arg0: reportService?.Timeout);
        }
        byte[] result = null;
        string reportPath = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
            input: report.ReportPath,
            parameters: parameters
        );
        string historyID = null;
        string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
        string encoding;
        string mimeType;
        string extension;
        Warning[] warnings = null;
        string[] streamIDs = null;
        ExecutionInfo execInfo = new ExecutionInfo();
        ExecutionHeader execHeader = new ExecutionHeader();
        reportService.ExecutionHeaderValue = execHeader;
        execInfo = reportService.LoadReport(Report: reportPath, HistoryID: historyID);
        if ((parameters != null) && (parameters.Count > 0))
        {
            ParameterValue[] reportParameters = new ParameterValue[parameters.Count];
            int index = 0;
            foreach (string key in parameters.Keys)
            {
                ParameterValue parameterValue = new ParameterValue();
                parameterValue.Name = key;
                parameterValue.Value = parameters[key: key].ToString();
                reportParameters[index] = parameterValue;
                index++;
            }
            reportService.SetExecutionParameters(
                Parameters: reportParameters,
                ParameterLanguage: Thread.CurrentThread.CurrentCulture.IetfLanguageTag
            );
        }
        result = reportService.Render(
            Format: format,
            DeviceInfo: devInfo,
            Extension: out extension,
            MimeType: out encoding,
            Encoding: out mimeType,
            Warnings: out warnings,
            StreamIds: out streamIDs
        );
        execInfo = reportService.GetExecutionInfo();
        return result;
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
        this.traceTaskInfo = traceTaskInfo;
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
