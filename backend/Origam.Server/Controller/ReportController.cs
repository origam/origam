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
[Route(template: "internalApi/[controller]")]
public class ReportController : AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly CoreHttpTools httpTools = new();

    public ReportController(
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        ILogger<ReportController> log,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment)
    {
        this.localizer = localizer;
    }

    [HttpGet(template: "{reportRequestId:guid}")]
    public IActionResult Get(Guid reportRequestId)
    {
        try
        {
            var (reportRequest, report) = GetReport(reportRequestId: reportRequestId);
            if (report == null)
            {
                return NotFound(value: localizer[name: "ErrorReportNotAvailable"].ToString());
            }
            switch (report)
            {
                case WebReport webReport:
                {
                    return HandleWebReport(reportRequest: reportRequest, webReport: webReport);
                }
                case FileSystemReport fileSystemReport:
                {
                    return HandleFileSystemReport(
                        reportRequest: reportRequest,
                        report: fileSystemReport
                    );
                }
                default:
                {
                    if (
                        reportRequest.DataReportExportFormatType
                        == DataReportExportFormatType.ExternalViewer
                    )
                    {
                        return HandleReportWithExternalViewer(
                            reportId: new Guid(g: reportRequest.ReportId),
                            report: report,
                            parameters: reportRequest.Parameters
                        );
                    }
                    // handle all other report by running
                    // report service agent's GetReport method
                    return HandleReport(reportRequest: reportRequest, reportName: report.Name);
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(statusCode: 500, value: ex);
        }
        finally
        {
            RemoveRequest(reportRequestId: reportRequestId);
        }
    }

    private IActionResult HandleReportWithExternalViewer(
        Guid reportId,
        AbstractReport report,
        Hashtable parameters
    )
    {
        var reportService = ReportServiceAgent.GetService(report: report);
        string url = reportService.PrepareExternalReportViewer(
            reportId: reportId,
            data: null,
            format: DataReportExportFormatType.ExternalViewer.ToString(),
            parameters: parameters,
            dbTransaction: null
        );
        return Redirect(url: url);
    }

    [HttpGet(template: "[action]")]
    public IActionResult GetReportInfo(Guid reportRequestId)
    {
        var (_, report) = GetReport(reportRequestId: reportRequestId);
        return report == null
            ? NotFound(value: localizer[name: "ErrorReportNotAvailable"].ToString())
            : Ok(value: new ReportInfo { IsWebReport = report is WebReport });
    }

    private (ReportRequest, AbstractReport) GetReport(Guid reportRequestId)
    {
        ReportRequest reportRequest = sessionObjects.SessionManager.GetReportRequest(
            key: reportRequestId
        );
        if (reportRequest == null)
        {
            return (null, null);
        }
        // log in as the user originally requesting the report
        // so row level security can be applied
        SecurityManager.SetCustomIdentity(userName: reportRequest.UserName, context: HttpContext);
        reportRequest.TimesRequested++;
        // get report model data
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        var report =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractReport),
                primaryKey: new ModelElementKey(id: new Guid(g: reportRequest.ReportId))
            ) as AbstractReport;
        return (reportRequest, report);
    }

    private IActionResult HandleReport(ReportRequest reportRequest, string reportName)
    {
        byte[] report = ReportService.GetReport(
            reportId: new Guid(g: reportRequest.ReportId),
            data: null,
            format: reportRequest.DataReportExportFormatType.GetString(),
            parameters: reportRequest.Parameters,
            transactionId: null
        );
        Response.Headers.Append(
            key: HeaderNames.ContentDisposition,
            value: "filename=\""
                + reportName
                + "."
                + reportRequest.DataReportExportFormatType.GetExtension()
                + "\""
        );
        return File(
            fileContents: report,
            contentType: reportRequest.DataReportExportFormatType.GetContentType()
        );
    }

    private IActionResult HandleWebReport(ReportRequest reportRequest, WebReport webReport)
    {
        ReportHelper.PopulateDefaultValues(report: webReport, parameters: reportRequest.Parameters);
        string url = HttpTools.Instance.BuildUrl(
            url: webReport.Url,
            parameters: reportRequest.Parameters,
            forceExternal: webReport.ForceExternalUrl,
            externalScheme: webReport.ExternalUrlScheme,
            isUrlEscaped: webReport.IsUrlEscaped
        );
        return Redirect(url: url);
    }

    private IActionResult HandleFileSystemReport(
        ReportRequest reportRequest,
        FileSystemReport report
    )
    {
        ReportHelper.PopulateDefaultValues(report: report, parameters: reportRequest.Parameters);
        string filePath = ReportHelper.BuildFileSystemReportFilePath(
            filePath: report.ReportPath,
            parameters: reportRequest.Parameters
        );
        if (!System.IO.File.Exists(path: filePath))
        {
            return NotFound();
        }
        string mimeType = HttpTools.Instance.GetMimeType(fileName: filePath);
        string fileName = Path.GetFileName(path: filePath);
        Response.Headers.Append(
            key: HeaderNames.ContentDisposition,
            value: httpTools.GetFileDisposition(
                userAgent: Request.GetUserAgent(),
                fileName: fileName
            )
        );
        var stream = new FileStream(path: filePath, mode: FileMode.Open);
        // specifying filename forces content-disposition attachment;
        return File(fileStream: stream, contentType: mimeType, fileDownloadName: fileName);
    }

    private void RemoveRequest(Guid reportRequestId)
    {
        var reportRequest = sessionObjects.SessionManager.GetReportRequest(key: reportRequestId);
        if (
            reportRequest is not { TimesRequested: < 2 }
            || (
                Request.Headers.ContainsKey(key: HeaderNames.UserAgent)
                && (
                    Request
                        .Headers[key: HeaderNames.UserAgent]
                        .ToString()
                        .IndexOf(value: "Edge", comparisonType: StringComparison.Ordinal) == -1
                )
            )
        )
        {
            sessionObjects.SessionManager.RemoveReportRequest(key: reportRequestId);
        }
    }
}
