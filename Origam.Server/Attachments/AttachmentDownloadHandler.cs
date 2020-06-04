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

using core = Origam.Workbench.Services.CoreServices;
using System.Web.SessionState;

using log4net;

namespace Origam.Server
{
    public class AttachmentDownloadHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog = LogManager.GetLogger("Performance");
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (perfLog.IsInfoEnabled)
            {
                perfLog.Info("AttachmentDownload");
            }

            string requestId = context.Request.Params.Get("id");

            try
            {
                AttachmentRequest ar = (AttachmentRequest)context.Application[requestId];

                if (ar == null)
                {
                    context.Response.Write(Properties.Resources.ErrorAttachmentNotAvailable);
                }
                else
                {
                    InternalHandlers.GetAttachmentHandler().GetFile(context, ar);
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(ex.Message, ex);
                    context.Response.Write(Properties.Resources.ErrorAttachmentRetrieval);
                }
            }
            finally
            {
                context.Application.Remove(requestId);
            }
        }

        #endregion
    }
}
