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
using System.Text;
using System.Web;

namespace Origam.Server
{
    public class NetFxHttpTools
    {
        public static void WriteFile(HttpRequest request, HttpResponse response, byte[] file, string fileName, bool isPreview)
        {
            WriteFile(request, response, file, fileName, isPreview, null);
        }

        public static void WriteFile(HttpRequest request, HttpResponse response, byte[] file, string fileName, bool isPreview, string overrideContentType)
        {
            // set proper content type
            if (overrideContentType != null)
            {
                response.ContentType = overrideContentType;
            }
            else
            {
                response.ContentType = Origam.HttpTools.GetMimeType(fileName);
            }
            response.Charset = Encoding.UTF8.WebName;
            string disposition = GetFileDisposition(request, fileName);
            if (!isPreview) disposition = "attachment; " + disposition;
            response.AppendHeader("content-disposition", disposition);
            // write to response.OutputStream
            response.OutputStream.Write(file, 0, file.Length);
        }
        
        public static string GetFileDisposition(HttpRequest request, string fileName)
        {
            bool isFirefox = request.UserAgent != null && request.UserAgent.IndexOf("Firefox") >= 0;

            string dispositionLeft = "filename=";
            if (isFirefox)
            {
                dispositionLeft = "filename*=utf-8''";
            }

            // no commas allowed in the file name
            fileName = fileName.Replace(",", "");

            string disposition = dispositionLeft + HttpUtility.UrlPathEncode(fileName);

            return disposition;
        }

    }
}