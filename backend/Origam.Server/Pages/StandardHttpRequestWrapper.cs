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

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace Origam.Server.Pages;
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
            ? MediaTypeHeaderValue.Parse(request.ContentType) 
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
    public string Browser 
        => clientInfo != null ? clientInfo.UserAgent.Family : "";
    public string BrowserVersion 
        => clientInfo != null 
            ? clientInfo.UserAgent.Major + "." + clientInfo.UserAgent.Minor
            : "";
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
    public string UrlReferrerAbsoluteUri => headerDictionary[HeaderNames.Referer];
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
        if (string.IsNullOrEmpty(request.Headers[HeaderNames.UserAgent]))
        {
            return null;
        }
        var uaParser = Parser.GetDefault();
        return uaParser.Parse(request.Headers[HeaderNames.UserAgent]);
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
        if (request.HasFormContentType)
        {
            foreach (var keyValuePair in request.Form)
            {
                parameters.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        return new Parameters(parameters);
    }
}
