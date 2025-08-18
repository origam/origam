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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Origam.BI;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Server.Extensions;
using Origam.Server.Model.Report;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Origam.Workflow;

namespace Origam.Server.Controller;
[AllowAnonymous]
[Controller]
[Route("internalApi/[controller]")]
public class ReportController : AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly CoreHttpTools httpTools = new();
    public ReportController(
        SessionObjects sessionObjects, 
        IStringLocalizer<SharedResources> localizer,
        ILogger<ReportController> log, IWebHostEnvironment environment) 
        : base(log, sessionObjects, environment)
    {
        this.localizer = localizer;
    }
    [HttpGet("{reportRequestId:guid}")]
    public IActionResult Get(Guid reportRequestId)
    {
        try
        {
            var (reportRequest, report) = GetReport(reportRequestId);
            if (report == null)
            {
                return NotFound(localizer["ErrorReportNotAvailable"].ToString());
            }
            switch (report)
            {
                case WebReport webReport:
                {
                    return HandleWebReport(reportRequest, webReport);
                }
                case FileSystemReport fileSystemReport:
                {
                    return HandleFileSystemReport(
                        reportRequest, fileSystemReport);
                }
                default:
                {
                    if (reportRequest.DataReportExportFormatType
                        == DataReportExportFormatType.ExternalViewer)
                    {
                        return HandleReportWithExternalViewer(
                            new Guid(reportRequest.ReportId),
                            report, reportRequest.Parameters);
                    }
                    // handle all other report by running
                    // report service agent's GetReport method
                    return HandleReport(reportRequest, report.Name);
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
        finally
        {
            RemoveRequest(reportRequestId);
        }
    }
    private IActionResult HandleReportWithExternalViewer(
        Guid reportId,
        AbstractReport report, 
        Hashtable parameters)
    {
        var reportService = ReportServiceAgent.GetService(report);
        string url = reportService.PrepareExternalReportViewer(
            reportId,
            data: null, 
            DataReportExportFormatType.ExternalViewer.ToString(),
            parameters, 
            dbTransaction: null);
        return Redirect(url);
    }
    [HttpGet("[action]")]
    public IActionResult GetReportInfo(Guid reportRequestId)
    {
        return RunWithErrorHandler(() =>
        {
            var (_, report) = GetReport(reportRequestId);
            return report == null
                ? NotFound(localizer["ErrorReportNotAvailable"].ToString())
                : Ok(new ReportInfo { IsWebReport = report is WebReport });
        });
    }
    private (ReportRequest, AbstractReport) GetReport(Guid reportRequestId)
    {
        ReportRequest reportRequest = sessionObjects.SessionManager
            .GetReportRequest(reportRequestId);
        if (reportRequest == null)
        {
            return (null, null);
        }
        // log in as the user originally requesting the report
        // so row level security can be applied 
        SecurityManager.SetCustomIdentity(
            reportRequest.UserName, HttpContext);
        reportRequest.TimesRequested++;
        // get report model data
        var persistenceService = ServiceManager
            .Services.GetService<IPersistenceService>();
        var report = persistenceService.SchemaProvider.RetrieveInstance(
                typeof(AbstractReport), 
                new ModelElementKey(new Guid(reportRequest.ReportId))) 
            as AbstractReport;
        return (reportRequest, report);
    }
    private IActionResult HandleReport(
        ReportRequest reportRequest, string reportName)
    {
        byte[] report = ReportService.GetReport(
            new Guid(reportRequest.ReportId),
            data: null,
            reportRequest.DataReportExportFormatType.GetString(),
            reportRequest.Parameters,
            transactionId: null);
        Response.Headers.Append(
            HeaderNames.ContentDisposition,
            "filename=\"" + reportName + "."
            + reportRequest.DataReportExportFormatType.GetExtension() 
            + "\"");
        return File(report, 
            reportRequest.DataReportExportFormatType.GetContentType());
    }
    private IActionResult HandleWebReport(
        ReportRequest reportRequest, 
        WebReport webReport)
    {
        ReportHelper.PopulateDefaultValues(
            webReport, reportRequest.Parameters);
        string url = HttpTools.Instance.BuildUrl(
            webReport.Url, reportRequest.Parameters, 
            webReport.ForceExternalUrl,
            webReport.ExternalUrlScheme, webReport.IsUrlEscaped);
        return Redirect(url);
    }
    private IActionResult HandleFileSystemReport(
        ReportRequest reportRequest, 
        FileSystemReport report)
    {
        ReportHelper.PopulateDefaultValues(
            report, reportRequest.Parameters);
        string filePath = ReportHelper.BuildFileSystemReportFilePath(
            report.ReportPath, reportRequest.Parameters);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }
        string mimeType = HttpTools.Instance.GetMimeType(filePath);
        string fileName = Path.GetFileName(filePath);
        Response.Headers.Append(
            HeaderNames.ContentDisposition,
            httpTools.GetFileDisposition(
                Request.GetUserAgent(), 
                fileName));
        var stream = new FileStream(filePath, FileMode.Open);
        // specifying filename forces content-disposition attachment;
        return File(stream, mimeType, fileName);
    }
    private void RemoveRequest(Guid reportRequestId)
    {
        var reportRequest = sessionObjects.SessionManager
            .GetReportRequest(reportRequestId);
        if (reportRequest is not { TimesRequested: < 2 }
            || (Request.Headers.ContainsKey(HeaderNames.UserAgent)
                && (Request.Headers[HeaderNames.UserAgent].ToString()
                    .IndexOf("Edge", StringComparison.Ordinal) == -1)))
        {
            sessionObjects.SessionManager.RemoveReportRequest(
                reportRequestId);
        }
    }
}
