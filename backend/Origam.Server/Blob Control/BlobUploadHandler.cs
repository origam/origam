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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Data;
using Origam.Workbench.Services;
using Origam;
using core = Origam.Workbench.Services.CoreServices;
using Origam.DA;
using Origam.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using log4net;
using System.Security.Principal;
using System.Text;
using Origam.Server;
using Origam.Server.Pages;
using ImageMagick;

namespace Origam.Server
{

    public class BlobUploadHandler
    {
//        protected static readonly ILog perfLog = LogManager.GetLogger("Performance");
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//        public void ProcessRequest(IHttpContext context)
//        {
//            try
//            {
//                if (perfLog.IsInfoEnabled)
//                {
//                    perfLog.Info("BlobUpload");
//                }
//
//                string requestId = null;
//
//                try
//                {
//                    requestId = context.Request.Params.Get("id");
//                }
//                catch (HttpException ex)
//                {
//                    if (ex.ErrorCode == -2147467259)
//                    {
//                        HttpRuntimeSection runTime = (HttpRuntimeSection)WebConfigurationManager.GetSection("system.web/httpRuntime");
//                        //Approx 100 Kb(for page content) size has been deducted because the maxRequestLength proprty is the page size, not only the file upload size
//                        int maxRequestLength = (runTime.MaxRequestLength - 100);
//
//                        context.Response.Write(String.Format(Properties.Resources.BlobMaxSizeError, maxRequestLength));
//
//                        if (log.IsErrorEnabled) log.Error(string.Format(Properties.Resources.ErrorAttachmentMaximumSize, maxRequestLength.ToString()), ex);
//                        return;
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//
//                try
//                {
//                    BlobUploadRequest br = (BlobUploadRequest)context.Application[requestId];
//
//                    if (br == null)
//                    {
//                        if (log.IsErrorEnabled) log.Error(Properties.Resources.BlobFileNotAvailable);
//                        context.Response.Write(Properties.Resources.BlobFileNotAvailable);
//                        return;
//                    }
//                    else
//                    {
//                        if (context.Request.Files.Count == 0)
//                        {
//                            if (log.IsErrorEnabled) log.Error(Properties.Resources.BlobNoFileSelected);
//                            context.Response.Write(Properties.Resources.BlobNoFileSelected);
//                            return;
//                        }
//
//                        if (context.Request.Files.Count > 1)
//                        {
//                            if (log.IsErrorEnabled) log.Error(Properties.Resources.BlobTooManyFilesSelected);
//                            context.Response.Write(Properties.Resources.BlobTooManyFilesSelected);
//                            return;
//                        }
//
//                        // bug in Flash Player - it does not send the cookie for file uploads.
//                        // We must create a temporary principal here.
//                        System.Threading.Thread.CurrentPrincipal = 
//                            new GenericPrincipal(new GenericIdentity(br.UserName), new string[] {});
//
//                        UserProfile profile = SecurityTools.CurrentUserProfile();
//
//                        foreach (string fileKey in context.Request.Files)
//                        {
//                            DatasetTools.UpdateOrigamSystemColumns(br.Row, false, profile.Id);
//
//                            HttpPostedFile file = context.Request.Files[fileKey];
//
//                            int fileLen = file.ContentLength;
//
//                            br.Row[br.Property] = Path.GetFileName(file.FileName);
//                           
//                            //if (CheckMember(br.BlobMember, true))
//                            //{
//                            //    br.Row[br.BlobMember] = input;
//                            //}
//
//                            if (CheckMember(br.OriginalPathMember, false))
//                            {
//                                br.Row[br.OriginalPathMember] = file.FileName;
//                            }
//
//                            if (CheckMember(br.DateCreatedMember, false))
//                            {
//                                br.Row[br.DateCreatedMember] = br.DateCreated;
//                            }
//
//                            if (CheckMember(br.DateLastModifiedMember, false))
//                            {
//                                br.Row[br.DateLastModifiedMember] = br.DateLastModified;
//                            }
//
//                            if (CheckMember(br.CompressionStateMember, false))
//                            {
//                                br.Row[br.CompressionStateMember] = br.ShouldCompress;
//                            }
//
//                            if (br.ShouldCompress)
//                            {
//                                System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(file.InputStream, System.IO.Compression.CompressionMode.Compress);
//                                byte[] input = StreamTools.ReadToEnd(gz);
//                                br.Row[br.BlobMember] = input;
//                            }
//                            else
//                            {
//                                byte[] input = StreamTools.ReadToEnd(file.InputStream);
//                                br.Row[br.BlobMember] = input;
//                            }
//
//                            if (CheckMember(br.FileSizeMember, false))
//                            {
//                                br.Row[br.FileSizeMember] = ((byte[])br.Row[br.BlobMember]).LongLength;
//                            }
//
//                            if (CheckMember(br.ThumbnailMember, false))
//                            {
//                                Image img = null;
//
//                                try
//                                {
//                                    img = Image.FromStream(file.InputStream);
//                                }
//                                catch
//                                {
//                                    br.Row[br.ThumbnailMember] = DBNull.Value;
//                                }
//
//                                if (img != null)
//                                {
//                                    try
//                                    {
//                                        IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
//                                        int width = (int)param.GetParameterValue(br.ThumbnailWidthConstantId, OrigamDataType.Integer);
//                                        int height = (int)param.GetParameterValue(br.ThumbnailHeightConstantId, OrigamDataType.Integer);
//                                        DataRow row = br.Row;
//                                        string thumbnailMember = br.ThumbnailMember;
//
//                                        row[thumbnailMember] = FixedSizeBytes(img, width, height);
//                                    }
//                                    finally
//                                    {
//                                        if (img != null) img.Dispose();
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                finally
//                {
//                    context.Application.Remove(requestId);
//                }
//
//                context.Response.Write("OK");
//            }
//            catch (Exception ex)
//            {
//                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
//                throw;
//            }
//        }

        public static byte[] FixedSizeBytes(MagickImage img, int width, int height)
        {
            using (MagickImage thumbnail = FixedSize(img, width, height))
            {
                MemoryStream ms = new MemoryStream();

                try
                {
                    thumbnail.Write(ms);
                    return ms.GetBuffer();
                }
                finally
                {
                    if (ms != null) ms.Close();
                }
            }
        }

//        public bool IsReusable
//        {
//            get { return true; }
//        }
//
//        private bool CheckMember(object val, bool throwExceptions)
//        {
//            if (val == null || val.Equals(String.Empty) || val.Equals(Guid.Empty))
//            {
//                if (throwExceptions)
//                {
//                    throw new NullReferenceException("Member not set.");
//                }
//                else
//                {
//                    return false;
//                }
//            }
//
//            return true;
//        }

        public static MagickImage FixedSize(MagickImage imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;

            float nPercentW = ((float)Width / (float)sourceWidth);
            float nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                    (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                    (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            var bmPhoto = new MagickImage(MagickColors.Black, Width, Height);
            bmPhoto.Format = MagickFormat.Png;

            imgPhoto.Resize(destWidth, destHeight);

            bmPhoto.Composite(imgPhoto, destX, destY);
            return bmPhoto;
        }
    }
}
