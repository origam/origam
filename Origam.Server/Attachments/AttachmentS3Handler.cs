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
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Origam.DA.Service;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;
using Origam.Schema;
using System.Data;
using Origam;
using core = Origam.Workbench.Services.CoreServices;
using log4net;
using System.Web;

namespace Origam.Server
{
    class AttachmentS3Handler : IAttachmentHandler
    {
        protected static readonly ILog perfLog = LogManager.GetLogger("Performance");
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static AttachmentS3Handler instance = new AttachmentS3Handler();

        public static AttachmentS3Handler Instance
        {
            get { return instance; }
        }

        public void StoreFile(HttpContext context, AttachmentUploadRequest uploadRequest, HttpPostedFile file)
        {
            UserProfile profile = SecurityTools.CurrentUserProfile();
            AmazonS3 s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    S3Settings.Settings.AWSAccessKey, S3Settings.Settings.AWSSecretKey);
            PutObjectRequest putRequest = new PutObjectRequest();
            Guid key = Guid.NewGuid();
            MD5PassThroughStream inputStream = new MD5PassThroughStream(file.InputStream);
            putRequest.WithBucketName(S3Settings.Settings.BucketName)
                .WithKey(key.ToString())
                .WithTimeout(S3Settings.Settings.Timeout)
                .WithInputStream(inputStream);
            PutObjectResponse putResponse = s3Client.PutObject(putRequest);
            byte[] md5output = inputStream.GetMD5();
            String localMD5 = MD5ByteArrayToAWSMD5String(md5output);
            String awsMD5 = putResponse.ETag;
            if (!localMD5.Equals(awsMD5))
            {
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest();
                deleteRequest.WithKey(key.ToString());
                DeleteObjectResponse deleteResponse = s3Client.DeleteObject(deleteRequest);
                if (log.IsErrorEnabled) log.Error("Checksum failed.");
                context.Response.Write("Checksum failed.");
                return;
            }
            DatasetGenerator dsg = new DatasetGenerator(true);
            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure ds = (DataStructure)ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f")));
            DataSet data = dsg.CreateDataSet(ds);
            DataRow r = data.Tables["Attachment"].NewRow();
            r["Id"] = key;
            r["Data"] = md5output;
            r["FileName"] = file.FileName;
            r["RecordCreated"] = DateTime.Now;
            r["RecordCreatedBy"] = profile.Id;
            r["refParentRecordId"] = uploadRequest.Id;
            r["refParentRecordEntityId"] = uploadRequest.EntityId;
            data.Tables["Attachment"].Rows.Add(r);
            core.DataService.StoreData(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"), data, false, null);
        }

        private string MD5ByteArrayToAWSMD5String(byte[] md5)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            for (int i = 0; i < md5.Length; i++)
            {
                sb.Append(md5[i].ToString("x2"));
            }
            sb.Append("\"");
            return sb.ToString();
        }

        public void GetFile(HttpContext context, AttachmentRequest request)
        {
            if (request.Ids.Length > 1)
            {
                throw new NotSupportedException("Downloading multiple attachments from Amazon S3 is not supported.");
            }
            object id = request.Ids[0];
            IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
            byte[] md5 = (byte[])ls.GetDisplayText(new Guid("0094d09b-a103-45f5-b011-d01667a0ea97"), id, false, false, null);
            string fileName = (string)ls.GetDisplayText(new Guid("eeaffeb8-6f47-414b-8e72-835690aaab87"), id, false, false, null);
            AmazonS3 s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    S3Settings.Settings.AWSAccessKey, S3Settings.Settings.AWSSecretKey);
            GetObjectMetadataRequest getRequest = new GetObjectMetadataRequest();
            getRequest.WithBucketName(S3Settings.Settings.BucketName)
                .WithKey(id.ToString());
            GetObjectMetadataResponse getResponse = s3Client.GetObjectMetadata(getRequest);
            String localMD5 = MD5ByteArrayToAWSMD5String(md5);
            String awsMD5 = getResponse.ETag;
            if (!localMD5.Equals(awsMD5))
            {
                if (log.IsErrorEnabled) log.Error("Checksum doesn't match.");
                context.Response.Write("Checksum doesn't match.");
                return;
            }
            string disposition = NetFxHttpTools.GetFileDisposition(context.Request, fileName);
            if (!request.IsPreview) disposition = "attachment; " + disposition;
            ResponseHeaderOverrides responseHeaderOverrides = new ResponseHeaderOverrides();
            responseHeaderOverrides.ContentDisposition = disposition;
            GetPreSignedUrlRequest urlRequest = new GetPreSignedUrlRequest();
            urlRequest.WithBucketName(S3Settings.Settings.BucketName)
                .WithKey(id.ToString())
                .WithResponseHeaderOverrides(responseHeaderOverrides)
                .WithExpires(DateTime.Now.AddMilliseconds(S3Settings.Settings.UrlExpiration));
            string url = s3Client.GetPreSignedURL(urlRequest);
            context.Response.Redirect(url);
        }

        public void DeleteFile(object id)
        {
            DataRow data = AttachmentUtils.LoadAttachmentInfo(id);
            AmazonS3 s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    S3Settings.Settings.AWSAccessKey, S3Settings.Settings.AWSSecretKey);
            DeleteObjectRequest deleteRequest = new DeleteObjectRequest();
            deleteRequest.WithBucketName(S3Settings.Settings.BucketName)
                .WithKey(id.ToString());
            s3Client.DeleteObject(deleteRequest);
            data.Delete();
            AttachmentUtils.SaveAttachmentInfo(data);
        }
    }
}
