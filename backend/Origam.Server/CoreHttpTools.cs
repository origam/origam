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

using Origam.Server;
using Origam.Server.Pages;
using System.Net;
using System.Text;

namespace Origam.Server
{
    public class CoreHttpTools : IHttpTools
    {
        public void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file,
            string fileName, bool isPreview)
        {
            throw new System.NotImplementedException();
        }

        public void WriteFile(IRequestWrapper request, IResponseWrapper response, byte[] file,
            string fileName, bool isPreview, string overrideContentType)
        {
            response.ContentType = overrideContentType ?? HttpTools.Instance.GetMimeType(fileName);
            string disposition = GetFileDisposition(request.UserAgent, fileName);
            if (!isPreview)
            {
                disposition = "attachment; " + disposition;
            }
            response.AppendHeader(
                "content-length", 
                file == null ? "0" : file.Length.ToString());
            response.AppendHeader("content-disposition", disposition);
            response.OutputStreamWrite(file, 0, file.Length);
        }

        public string GetFileDisposition(string userAgent, string fileName)
        {
            bool isFirefox 
                = (userAgent != null) 
                && (userAgent.IndexOf("Firefox") >= 0);
            string dispositionLeft = "filename=";
            if(isFirefox)
            {
                dispositionLeft = "filename*=utf-8''";
            }
            // no commas allowed in the file name
            fileName = fileName.Replace(",", "");
            string disposition = dispositionLeft + WebUtility.UrlEncode(fileName);
            return disposition;
        }
    }
}