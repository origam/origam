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
        public string UrlReferrerAbsolutePath { get; set; }
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