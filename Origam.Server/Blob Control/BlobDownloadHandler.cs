#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

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
using System.Text;
using System.Web;

using core = Origam.Workbench.Services.CoreServices;
using Origam;
using Origam.DA;
using Origam.Workbench.Services;
using System.Web.SessionState;
using System.IO;

using log4net;

namespace Origam.Server
{
    public class BlobDownloadHandler : IHttpHandler, IRequiresSessionState
    {
        private static readonly ILog perfLog = LogManager.GetLogger("Performance");

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

            Stream resultStream = null;
            MemoryStream ms = null;

            try
            {
                BlobDownloadRequest br = (BlobDownloadRequest)context.Application[requestId];

                if (br == null)
                {
                    context.Response.Write(Properties.Resources.ErrorAttachmentNotAvailable);
                    return;
                }
                else
                {
                    if ((br.BlobMember == null || br.BlobMember.Equals(string.Empty))
                        && br.BlobLookupId == Guid.Empty)
                    {
                        context.Response.Write("Both BlobMember and BlobLookupId are not specified. Cannot download blob.");
                        return;
                    }

                    bool processBlobField = false;

                    if (br.BlobMember != null && !br.BlobMember.Equals(string.Empty)
                        && br.Row[br.BlobMember] != DBNull.Value)
                    {
                        processBlobField = true;
                    }

                    if (br.BlobLookupId != Guid.Empty && ! processBlobField)
                    {
                        IDataLookupService lookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

                        object result = lookupService.GetDisplayText(br.BlobLookupId, DatasetTools.PrimaryKey(br.Row)[0], false, false, null);
                        byte[] bytes;

                        if (result == null)
                        {
                            context.Response.Write("Data source did not return any data.");
                            return;
                        }
                        else if (result is byte[])
                        {
                            bytes = (byte[])result;
                        }
                        else
                        {
                            context.Response.Write("Data source returned data that is not BLOB.");
                            return;
                        }

                        ms = new MemoryStream(bytes);
                    }
                    else
                    {
                        if (br.Row[br.BlobMember] == DBNull.Value)
                        {
                            context.Response.Write(Properties.Resources.ErrorBlobRecordEmpty);
                            return;
                        }
                        else
                        {
                            ms = new MemoryStream((byte[])br.Row[br.BlobMember]);
                        }
                    }

                    if (br.IsCompressed)
                    {
                        resultStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
                    }
                    else
                    {
                        resultStream = ms;
                    }

                    string fileName = (string)br.Row[br.Property];

                    // return
                    HttpResponse response = context.Response;
                    // set proper content type
                    response.ContentType = HttpTools.GetMimeType(fileName);
                    response.Charset = Encoding.UTF8.WebName;
                    string disposition = NetFxHttpTools.GetFileDisposition(context.Request, fileName);

                    if (!br.IsPreview) disposition = "attachment; " + disposition;

                    response.AppendHeader("content-disposition",disposition);

                    // write to response.OutputStream
                    byte[] data = new byte[2048];

                    int size;
                    do
                    {
                        size = resultStream.Read(data, 0, data.Length);
                        response.OutputStream.Write(data, 0, size);
                    } while (size > 0);
                }
            }
            finally
            {
                context.Application.Remove(requestId);

                if (ms != null) ms.Close();
                if (resultStream != null) resultStream.Close();
            }
        }

        #endregion
    }
}
