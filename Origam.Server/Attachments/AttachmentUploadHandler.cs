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
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using core = Origam.Workbench.Services.CoreServices;
using log4net;
using System.Security.Principal;

namespace Origam.Server
{

    public class AttachmentUploadHandler : IHttpHandler, IRequiresSessionState
    {
        protected static readonly ILog perfLog = LogManager.GetLogger("Performance");
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (perfLog.IsInfoEnabled)
                {
                    perfLog.Info("AttachmentUpload");
                }

                string requestId = null;

                try
                {
                    requestId = context.Request.Params.Get("id");
                }
                catch (HttpException ex)
                {
                    if (ex.ErrorCode == -2147467259)
                    {
                        HttpRuntimeSection runTime = (HttpRuntimeSection)WebConfigurationManager.GetSection("system.web/httpRuntime");
                        //Approx 100 Kb(for page content) size has been deducted because the maxRequestLength proprty is the page size, not only the file upload size
                        int maxRequestLength = (runTime.MaxRequestLength - 100);

                        context.Response.Write(String.Format(Properties.Resources.BlobMaxSizeError, maxRequestLength));

                        if (log.IsErrorEnabled) log.Error(string.Format(Properties.Resources.ErrorAttachmentMaximumSize, maxRequestLength.ToString()), ex);
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }

                try
                {
                    AttachmentUploadRequest ar = (AttachmentUploadRequest)context.Application[requestId];

                    if (ar == null)
                    {
                        if (log.IsErrorEnabled) log.Error(Properties.Resources.ErrorAttachmentNotAvailable);
                        context.Response.Write(Properties.Resources.ErrorAttachmentNotAvailable);
                        return;
                    }
                    else
                    {
                        if (context.Request.Files.Count == 0)
                        {
                            context.Response.Write(Properties.Resources.ErrorAttachmentNoFile);
                            if (log.IsErrorEnabled) log.Error(Properties.Resources.ErrorAttachmentNoFile);
                            return;
                        }

                        // bug in Flash Player - it does not send the cookie for file uploads. 
                        // We must create a temporary principal here.
                        System.Threading.Thread.CurrentPrincipal = 
                                new GenericPrincipal(new GenericIdentity(ar.UserName), new string[] {});
                        foreach (string fileKey in context.Request.Files)
                        {
                            HttpPostedFile file = context.Request.Files[fileKey];
                            InternalHandlers.GetAttachmentHandler().StoreFile(context, ar, file);
                        }
                    }
                }
                finally
                {
                    context.Application.Remove(requestId);
                }

                context.Response.Write("OK");
            }
            catch (Exception ex)
            {
                if(log.IsErrorEnabled) log.Error(ex.Message, ex);
                throw;
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
