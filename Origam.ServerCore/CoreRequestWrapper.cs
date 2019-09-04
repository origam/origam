using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Origam.ServerCommon.Pages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.ServerCore
{
    public class CoreRequestWrapper : IRequestWrapper
    {
        private readonly HttpRequest request;

        public CoreRequestWrapper(HttpRequest request)
        {
            this.request = request;
        }

        public string AppRelativeCurrentExecutionFilePath => throw new NotImplementedException();
        public string ContentType => throw new NotImplementedException();
        public string AbsoluteUri => throw new NotImplementedException();
        public Stream InputStream => throw new NotImplementedException();
        public string HttpMethod => throw new NotImplementedException();
        public string RawUrl => throw new NotImplementedException();
        public string Url => throw new NotImplementedException();
        public string UrlReferrer => throw new NotImplementedException();
        public string UserAgent 
            => request.Headers.ContainsKey(HeaderNames.UserAgent) 
            ? request.Headers[HeaderNames.UserAgent].ToString()
            : string.Empty;
        public string Browser => throw new NotImplementedException();
        public string BrowserVersion => throw new NotImplementedException();
        public string UserHostAddress => throw new NotImplementedException();
        public string UserHostName => throw new NotImplementedException();
        public IEnumerable<string> UserLanguages => throw new NotImplementedException();
        public Encoding ContentEncoding => throw new NotImplementedException();
        public long ContentLength => throw new NotImplementedException();
        public IDictionary BrowserCapabilities => throw new NotImplementedException();
        public string UrlReferrerAbsolutePath => throw new NotImplementedException();
        public Parameters Params => throw new NotImplementedException();
        public PostedFile FilesGet(string name)
        {
            throw new NotImplementedException();
        }
    }
}
