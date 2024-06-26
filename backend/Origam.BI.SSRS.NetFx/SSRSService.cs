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
using System.Xml;
using Origam;
using Origam.BI;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using log4net;
using Origam.BI.SSRS.SSRSWebReference;
using Origam.Service.Core;

namespace Origam.BI.SSRS;
public class SSRSService : IReportService
{
    private TraceTaskInfo traceTaskInfo = null;
    private static readonly ILog log = LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType);
	public object GetReport(Guid reportId, IXmlContainer data, string format, 
        Hashtable parameters, string dbTransaction)
    {
		IPersistenceService persistenceService = ServiceManager.Services
            .GetService(typeof(IPersistenceService)) as IPersistenceService;
		SSRSReport report = persistenceService.SchemaProvider
            .RetrieveInstance(typeof(AbstractReport), 
            new ModelElementKey(reportId)) as SSRSReport;
		if (report == null)
		{
			throw new ArgumentOutOfRangeException("reportId", reportId, 
                Strings.DefinitionNotInModel);
		}
        if (parameters == null)
        {
            parameters = new Hashtable();
        }
        ReportHelper.PopulateDefaultValues(report, parameters);
        ReportHelper.ComputeXsltValueParameters(report, parameters, traceTaskInfo);
        ReportExecutionService reportService = new ReportExecutionService();
        OrigamSettings settings 
            = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
        reportService.Url = settings.SQLReportServiceUrl;
        if (String.IsNullOrEmpty(settings.SQLReportServiceAccount))
        {
            reportService.Credentials = CredentialCache.DefaultCredentials;
        }
        else
        {
            reportService.Credentials = new NetworkCredential(
                settings.SQLReportServiceAccount,
                settings.SQLReportServicePassword);
        }
        reportService.Timeout = settings.SQLReportServiceTimeout;
        if (log.IsDebugEnabled)
        {
            log.DebugFormat("SSRSService Timeout: {0}", 
                reportService?.Timeout);
        }
        byte[] result = null;
        string reportPath = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(report.ReportPath, parameters);
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
        execInfo = reportService.LoadReport(reportPath, historyID);
        if ((parameters != null) && (parameters.Count > 0))
        {
            ParameterValue[] reportParameters 
                = new ParameterValue[parameters.Count];
            int index = 0;
            foreach (string key in parameters.Keys)
            {
                ParameterValue parameterValue = new ParameterValue();
                parameterValue.Name = key;
                parameterValue.Value = parameters[key].ToString();
                reportParameters[index] = parameterValue;
                index++;
            }
            reportService.SetExecutionParameters(reportParameters,
                Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
        }
        result = reportService.Render(format, devInfo, out extension, 
            out encoding, out mimeType, out warnings, out streamIDs);
        execInfo = reportService.GetExecutionInfo();
        return result;
    }
	public void PrintReport(Guid reportId, IXmlContainer data, string printerName, 
        int copies, Hashtable parameters)
	{
		throw new NotSupportedException();
	}
    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
        this.traceTaskInfo = traceTaskInfo;
    }
    public string PrepareExternalReportViewer(Guid reportId,
        IXmlContainer data, string format,
        Hashtable parameters, string dbTransaction)
    {
        throw new NotImplementedException();
    }
}
