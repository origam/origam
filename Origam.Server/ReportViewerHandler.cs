#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.SessionState;
using log4net;
using Origam.BI;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Server.Pages;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Server
{
    public class ReportViewerHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog 
            = LogManager.GetLogger("Performance");
        
        private readonly NetFxHttpTools httpTools= new NetFxHttpTools();

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (perfLog.IsInfoEnabled)
            {
                perfLog.Info("ReportViewer");
            }
            string requestId = context.Request.Params.Get("id");
            try
            {
                ReportRequest reportRequest 
                    = (ReportRequest)context.Application[requestId];
                if (reportRequest == null)
                {
                    context.Response.Write(
                        Properties.Resources.ErrorReportNotAvailable);
                }
                else
                {
                    reportRequest.TimesRequested++;
                    // get report model data
                    IPersistenceService persistenceService 
                        = ServiceManager.Services.GetService(
                        typeof(IPersistenceService)) as IPersistenceService;
                    AbstractReport report 
                        = persistenceService.SchemaProvider.RetrieveInstance(
                        typeof(AbstractReport), 
                        new ModelElementKey(new Guid(reportRequest.ReportId))) 
                        as AbstractReport;
                    if (report is WebReport)
                    {
                        HandleWebReport(
                            context, reportRequest, report as WebReport);
                    }
                    else if (report is FileSystemReport)
                    {
                        HandleFileSystemReport(
                            context, reportRequest, report as FileSystemReport);
                    }
                    else 
                    {
                        HandleReport(context, reportRequest, report.Name);
                    }
                }
            }
            finally
            {
                RemoveRequest(context, requestId);
            }
        }

        private void RemoveRequest(HttpContext context, string requestId)
        {
            ReportRequest reportRequest 
                = (ReportRequest)context.Application[requestId];
            if ((reportRequest == null) 
            || (reportRequest.TimesRequested >= 2)
            || ((context.Request.UserAgent != null) 
                && (context.Request.UserAgent.IndexOf("Edge") == -1)))
            {
                context.Application.Remove(requestId);
            }
        }

        private void HandleFileSystemReport(
            HttpContext context, ReportRequest reportRequest, 
            FileSystemReport report)
        {
            ReportHelper.PopulateDefaultValues(
                report, reportRequest.Parameters);
            string filePath = BuildFileSystemReportFilePath(
                report.ReportPath, reportRequest.Parameters);
            string mimeType = HttpTools.GetMimeType(filePath);
            context.Response.ContentType = mimeType;
            context.Response.AddHeader(
                "content-disposition",
                /*"attachment; " +*/ httpTools.GetFileDisposition(
                    new FxHttpRequestWrapper(context.Request), Path.GetFileName(filePath)));
            context.Response.WriteFile(filePath);
        }

        private string BuildFileSystemReportFilePath(
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
                            Properties.Resources.FilePathPartParameterNull);
                    }
                    filePath = filePath.Replace(replacement, sValue);
                }
            }
            return filePath;
        }

        private void HandleWebReport(
            HttpContext context, ReportRequest reportRequest, 
            WebReport webReport)
        {
            ReportHelper.PopulateDefaultValues(
                webReport, reportRequest.Parameters);
            string url = HttpTools.BuildUrl(
                webReport.Url, reportRequest.Parameters, 
                webReport.ForceExternalUrl,
                webReport.ExternalUrlScheme, webReport.IsUrlEscaped);
            context.Response.Redirect(url);
        }

        private static void HandleReport(HttpContext context, ReportRequest reportRequest,
            string reportName)
        {
            byte[] result = core.ReportService.GetReport(
                new Guid(reportRequest.ReportId),
                null,
                reportRequest.DataReportExportFormatType.GetString(),
                reportRequest.Parameters,
                null);
            HttpResponse response = context.Response;
            response.ContentType
                = reportRequest.DataReportExportFormatType.GetContentType();
            context.Response.AddHeader(
                "content-disposition",
                "filename=\"" + reportName + "."
                + reportRequest.DataReportExportFormatType.GetExtension() + "\"");
            response.OutputStream.Write(result, 0, result.Length);
        }
        #endregion
    }
}
