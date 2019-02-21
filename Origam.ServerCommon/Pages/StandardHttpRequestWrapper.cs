using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace Origam.ServerCommon.Pages
{
    internal class StandardHttpRequestWrapper : IRequestWrapper
    {
        private readonly HttpContext httpContext;
        private readonly HttpRequest request;
        private readonly IHeaderDictionary headerDictionary;
        private readonly ClientInfo clientInfo;
        private readonly MediaTypeHeaderValue mediaTypeHeader;

        public StandardHttpRequestWrapper(HttpContext httpContext)
        {
            this.httpContext = httpContext;
            this.request = httpContext.Request;
            headerDictionary = this.httpContext.Request.Headers;

            clientInfo = GetClientInfo();
            Params = GetParameters();
            mediaTypeHeader = request.ContentType != null 
                ? new MediaTypeHeaderValue(request.ContentType) 
                : null;
        }

        public string AppRelativeCurrentExecutionFilePath => request.Path.ToUriComponent();
        public string ContentType => mediaTypeHeader?.MediaType.Value;
        public string AbsoluteUri => Url;
        public Stream InputStream => request.Body;
        public string HttpMethod => request.Method;
        public string RawUrl => request.GetDisplayUrl();
        public string Url => request.Host.ToUriComponent() + "/" + request.Path.ToUriComponent();
        public string UrlReferrer => headerDictionary[HeaderNames.Referer].ToString();
        public string UserAgent => headerDictionary[HeaderNames.UserAgent].ToString();
        public string Browser => clientInfo.UserAgent.Family;
        public string BrowserVersion => clientInfo.UserAgent.Major+"."+ clientInfo.UserAgent.Minor;
        public string UserHostAddress => httpContext.Connection.RemoteIpAddress?.ToString();
        public string UserHostName => request.Host.Value;
        public IEnumerable<string> UserLanguages
        {
            get
            {
                var languages = httpContext.Request.Headers[HeaderNames.AcceptLanguage].ToArray();
                return languages.Length == 0 
                    ? new string[0] 
                    : languages[0].Split(',');
            }
        }

        public Encoding ContentEncoding => mediaTypeHeader?.Encoding;
        public long ContentLength => request.ContentLength ?? 0;
        public IDictionary BrowserCapabilities => new Dictionary<string,string>(); //

        public string UrlReferrerAbsolutePath => headerDictionary[HeaderNames.Referer];
        public Parameters Params { get; }

        public PostedFile FilesGet(string name)
        {
            var httpPostedFile = request.Form.Files[name];
            return new PostedFile
            {
                ContentType = httpPostedFile.ContentType,
                InputStream = httpPostedFile.OpenReadStream(),
                ContentLength = httpPostedFile.Length,
                FileName = httpPostedFile.FileName
            };
        }

        private ClientInfo GetClientInfo()
        {
            var uaParser = Parser.GetDefault();
            return uaParser.Parse(httpContext.Request.Headers[HeaderNames.UserAgent]);
        }

        private Parameters GetParameters()
        {
            var parameters = request.Query.Keys
                .ToDictionary(
                    key => key,
                    key => request.Query[key].ToString());

            foreach (var keyValuePair in request.Cookies)
            {
                parameters.Add(keyValuePair.Key, keyValuePair.Value);
            }

            if (request.ContentType != null)
            {
                foreach (var keyValuePair in request.Form)
                {
                    parameters.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return new Parameters(parameters);
        }
    }
}