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
using System.Web;
using core = Origam.Workbench.Services.CoreServices;
using Origam;
using Origam.Workbench.Services;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.Schema;
using System.Data;
using log4net;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace Origam.Server
{
    class AttachmentSQLDbHandler : IAttachmentHandler
    {
        protected static readonly ILog perfLog = LogManager.GetLogger("Performance");
        protected static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static AttachmentSQLDbHandler instance = new AttachmentSQLDbHandler();

        public static AttachmentSQLDbHandler Instance
        {
            get { return instance; }
        }

        public void StoreFile(HttpContext context, 
            AttachmentUploadRequest uploadRequest, HttpPostedFile file)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            if (log.IsDebugEnabled) log.Debug("Uploading file: '" 
                + file.FileName + "' size: " + file.ContentLength.ToString());
            // Initialize the stream.
            byte[] input = StreamTools.ReadToEnd(file.InputStream);
            if (log.IsDebugEnabled) log.Debug("Bytes read: '" + input.LongLength.ToString());
            if (input.Length != file.ContentLength)
            {
                log.Error(Properties.Resources.ErrorAttachmentNotFullyReceived);
                context.Response.Write(Properties.Resources.ErrorAttachmentNotFullyReceived);
                return;
            }
            else
            {
                DatasetGenerator dsg = new DatasetGenerator(true);
                IPersistenceService ps = ServiceManager.Services.GetService(
                    typeof(IPersistenceService)) as IPersistenceService;
                DataStructure ds = (DataStructure)ps.SchemaProvider.RetrieveInstance(
                    typeof(AbstractSchemaItem), new ModelElementKey(
                        new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f")));
                DataSet data = dsg.CreateDataSet(ds);
                DataRow r = data.Tables["Attachment"].NewRow();
                r["Id"] = Guid.NewGuid();
                r["Data"] = input;
                r["FileName"] = file.FileName;
                r["RecordCreated"] = DateTime.Now;
                r["RecordCreatedBy"] = profile.Id;
                r["refParentRecordId"] = uploadRequest.Id;
                r["refParentRecordEntityId"] = uploadRequest.EntityId;
                data.Tables["Attachment"].Rows.Add(r);
                core.DataService.StoreData(
                    new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"), data, false, null);
            }
        }

        public void GetFile(HttpContext context, AttachmentRequest request)
        {
            if (request.Ids.Length == 1)
            {
                GetSingleFile(context, request);
            }
            else
            {
                GetMultipleFiles(context, request);
            }
        }

        private static void GetMultipleFiles(HttpContext context, AttachmentRequest request)
        {
            if (request.IsPreview)
            {
                throw new NotSupportedException("Preview not supported for multiple files.");
            }
            string zipFileName = "attachments.zip";
            context.Response.ContentType = HttpTools.GetMimeType(zipFileName);
            context.Response.Charset = Encoding.UTF8.WebName;
            string disposition = "attachment; " 
                + NetFxHttpTools.GetFileDisposition(context.Request, zipFileName);
            context.Response.AppendHeader("content-disposition", disposition);
            using (var outputStream = new PositionWrapperStream(context.Response.OutputStream))
            {
                using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < request.Ids.Length; i++)
                    {
                        Zip(context, request.Ids[i], i, archive);
                    }
                }
            }
        }

        private static void Zip(HttpContext context, object id, int i,
            ZipArchive archive)
        {
            byte[] file;
            string fileName;
            GetFile(id, out file, out fileName);
            Zip(file, archive, fileName, DateTime.Now, i);
        }

        private static void GetSingleFile(HttpContext context, AttachmentRequest request)
        {
            byte[] file;
            string fileName;
            GetFile(request.Ids[0], out file, out fileName);
            NetFxHttpTools.WriteFile(context.Request, context.Response, file,
                fileName, request.IsPreview);
        }

        private static void GetFile(object id, out byte[] result, out string fileName)
        {
            // get attachment
            IDataLookupService ls = ServiceManager.Services.GetService(
                typeof(IDataLookupService)) as IDataLookupService;
            result = (byte[])ls.GetDisplayText(
                new Guid("0094d09b-a103-45f5-b011-d01667a0ea97"), id, false, false, null);
            fileName = (string)ls.GetDisplayText(
                new Guid("eeaffeb8-6f47-414b-8e72-835690aaab87"), id, false, false, null);
        }

        public void DeleteFile(object id)
        {
            DataRow data = AttachmentUtils.LoadAttachmentInfo(id);
            data.Delete();
            AttachmentUtils.SaveAttachmentInfo(data);
        }

        private static void Zip(byte[] input, ZipArchive archive,
            string fileName, DateTime dateCreated, int index)
        {
            var entry = archive.CreateEntry(fileName, CompressionLevel.NoCompression);
            entry.LastWriteTime = dateCreated;
            using (var entryStream = entry.Open())
            {
                StreamTools.Write(entryStream, input);
            }
        }
    }

    class PositionWrapperStream : Stream
    {
        private readonly Stream wrapped;

        private int pos = 0;

        public PositionWrapperStream(Stream wrapped)
        {
            this.wrapped = wrapped;
        }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Position
        {
            get { return pos; }
            set { throw new NotSupportedException(); }
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            pos += count;
            wrapped.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            wrapped.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            wrapped.Dispose();
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        // all the other required methods can throw NotSupportedException
    }
}
