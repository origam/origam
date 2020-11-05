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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Origam.ServerCommon.Pages;

namespace Origam.Server.Pages
{
    internal class FxHttpRequestWrapper : IRequestWrapper
    {
        private readonly HttpRequest request;

        public FxHttpRequestWrapper(HttpRequest request)
        {
            this.request = request;

            Dictionary<string, string> parametersDict = 
                request.Params.AllKeys
                .ToDictionary(
                    key => key,
                    key => request.Params[key]);
            Params = new Parameters(parametersDict);
        }

        public string AppRelativeCurrentExecutionFilePath => request.AppRelativeCurrentExecutionFilePath;
        public string ContentType => request.ContentType;
        public string AbsoluteUri => request.Url.AbsoluteUri;
        public Parameters Params { get; }
        public Stream InputStream => request.InputStream;
        public string HttpMethod => request.HttpMethod;
        public string RawUrl => request.RawUrl;
        public string Url => request.Url.ToString();
        public string UrlReferrer => request.UrlReferrer?.ToString();
        public string UserAgent => request.UserAgent;
        public string Browser => request.Browser.Browser;
        public string BrowserVersion => request.Browser.Version;
        public string UserHostAddress => request.UserHostAddress;
        public string UserHostName => request.UserHostName;
        public IEnumerable<string> UserLanguages => request.UserLanguages;
        public Encoding ContentEncoding => request.ContentEncoding;
        public long ContentLength => request.ContentLength;
        public IDictionary BrowserCapabilities => request.Browser.Capabilities;
        public string UrlReferrerAbsoluteUri => request.UrlReferrer.AbsoluteUri;
        public PostedFile FilesGet(string name)
        {
            HttpPostedFile httpPostedFile = request.Files[name];
            return new PostedFile
            {
                ContentType = httpPostedFile.ContentType,
                InputStream = httpPostedFile.InputStream,
                ContentLength = httpPostedFile.ContentLength,
                FileName = httpPostedFile.FileName
            };
        }
    }
}