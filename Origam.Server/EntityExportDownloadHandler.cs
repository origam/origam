#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Web;
using System.Web.SessionState;
using log4net;
using NPOI.SS.UserModel;
using Origam.Excel;
using Origam.ServerCommon;

namespace Origam.Server
{
    public class EntityExportDownloadHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog = LogManager.GetLogger(
            "Performance");
        private static readonly ILog log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExcelEntityExporter excelEntityExporter = new ExcelEntityExporter();

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if(perfLog.IsInfoEnabled)
            {
                perfLog.Info("ExcelExport");
            }
            string requestId = context.Request.Params.Get("id");
            try
            {
                EntityExportInfo info 
                    = (EntityExportInfo)context.Application[requestId];
                if(info == null)
                {
                    context.Response.Write(
                        Properties.Resources.ErrorExportNotAvailable);
                    return;
                }
                SetupResponseHeader(context);
                IWorkbook workbook = excelEntityExporter.FillWorkBook(info);
                workbook.Write(context.Response.OutputStream);
                context.Response.End();
            }
            catch (Exception ex)
            {
                if(log.IsErrorEnabled)
                {
                    log.Error(ex.Message, ex);
                }
                throw;
            }
            finally
            {
                context.Application.Remove(requestId);
            }
        }

        private void SetupResponseHeader(HttpContext context)
        {
            if (excelEntityExporter.ExportFormat == ExcelFormat.XLS)
            {
                context.Response.ContentType = "application/vnd.ms-excel";
                context.Response.AppendHeader(
                    "content-disposition", "attachment; filename=export.xls");
            }
            else
            {
                context.Response.ContentType 
                    = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                context.Response.AppendHeader(
                    "content-disposition", "attachment; filename=export.xlsx");
            }
        }
        #endregion
    }
}
