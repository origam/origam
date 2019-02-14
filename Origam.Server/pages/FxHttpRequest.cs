using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Origam.ServerCommon.Pages;
using HttpPostedFile = Origam.ServerCommon.Pages.HttpPostedFile;

namespace Origam.Server.Pages
{
    internal class FxHttpRequest : IRequest
    {
        private readonly HttpRequest request;

        public FxHttpRequest(HttpRequest request)
        {
            this.request = request;
        }

        public string AppRelativeCurrentExecutionFilePath => request.AppRelativeCurrentExecutionFilePath;
        public string ContentType => request.ContentType;
        public object AbsoluteUri => request.Url.AbsoluteUri;
        public NameValueCollection Params => request.Params;
        public Stream InputStream => request.InputStream;
        public string HttpMethod => request.HttpMethod;
        public string RawUrl => request.RawUrl;
        public object Url => request.Url;
        public object UrlReferrer => request.UrlReferrer;
        public string UserAgent => request.UserAgent;
        public string Browser => request.Browser.Browser;
        public string BrowserVersion => request.Browser.Version;
        public string UserHostAddress => request.UserHostAddress;
        public string UserHostName => request.UserHostName;
        public IEnumerable<string> UserLanguages => request.UserLanguages;
        public bool ContentEncoding { get; set; }
        public int ContentLength { get; set; }
        public IEnumerable<DictionaryEntry> BrowserCapabilities { get; set; }
        public string UrlReferrerAbsolutePath { get; set; }
        public HttpPostedFile FilesGet(string name)
        {
            System.Web.HttpPostedFile httpPostedFile = request.Files[name];
            return new HttpPostedFile
            {
                ContentType = httpPostedFile.ContentType,
                InputStream = httpPostedFile.InputStream,
                ContentLength = httpPostedFile.ContentLength,
                FileName = httpPostedFile.FileName
            };
        }
    }
}