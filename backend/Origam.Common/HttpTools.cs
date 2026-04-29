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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Origam.Service.Core;

namespace Origam;

public class HttpTools : IHttpTools
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );

    public static HttpTools Instance { get; set; } = new();

    public static void SetDIServiceProvider(IServiceProvider serviceProvider)
    {
        Instance.serviceProvider = serviceProvider;
    }

    private IServiceProvider serviceProvider;

    private HttpTools() { }

    private void FixCookies(HttpWebRequest request, HttpWebResponse response)
    {
        if (response.Headers.Get(name: "Set-Cookie") is string setCookieHeader)
        {
            List<string> singleCookies = SplitCookiesHeaderToSingleCookies(
                setCookieHeader: setCookieHeader
            );
            string host = request.Host.Split(separator: ':')[0];
            CookiesFromStrings(host: host, singleCookies: singleCookies)
                .ForEach(action: cookie => response.Cookies.Add(cookie: cookie));
        }
    }

    public List<Cookie> CookiesFromStrings(string host, List<string> singleCookies)
    {
        List<Cookie> cookies = new List<Cookie>();
        foreach (string singleCookie in singleCookies)
        {
            Match match = Regex.Match(input: singleCookie, pattern: @"(\S+?)=(.+?);");
            if (match.Captures.Count == 0)
            {
                continue;
            }
            cookies.Add(
                item: new Cookie(
                    name: match.Groups[groupnum: 1].ToString(),
                    value: match.Groups[groupnum: 2].ToString(),
                    path: "/",
                    domain: host
                )
            );
        }
        return cookies;
    }

    public List<string> SplitCookiesHeaderToSingleCookies(string setCookieHeader)
    {
        string[] cookieSegments = setCookieHeader.Split(separator: ',');
        List<string> singleCookies = new List<string>();
        int i = 0;
        while (i < cookieSegments.Length)
        {
            bool containsExpiresParameter =
                cookieSegments[i]
                    .IndexOf(value: "expires=", comparisonType: StringComparison.OrdinalIgnoreCase)
                > 0;
            if (containsExpiresParameter)
            {
                singleCookies.Add(item: cookieSegments[i] + "," + cookieSegments[i + 1]);
                i++;
            }
            else
            {
                singleCookies.Add(item: cookieSegments[i]);
            }
            i++;
        }
        return singleCookies;
    }

    public string BuildUrl(
        string url,
        Hashtable parameters,
        bool forceExternal,
        string externalScheme,
        bool isUrlEscaped
    )
    {
        var urlParameters = new Hashtable();
        // replace url parts, e.g. {id}
        foreach (DictionaryEntry entry in parameters)
        {
            string key = entry.Key.ToString();
            string value = null;
            if (entry.Value != null)
            {
                value = entry.Value.ToString();
            }
            string replacement = "{" + key + "}";
            if (url.IndexOf(value: replacement) > -1)
            {
                if (value == null)
                {
                    throw new Exception(
                        message: ResourceUtils.GetString(key: "UrlPartParameterNull")
                    );
                }
                url = url.Replace(oldValue: replacement, newValue: value);
            }
            else
            {
                urlParameters.Add(key: key, value: value);
            }
        }
        var parameterBuilder = new StringBuilder();
        bool isFirst = true;
        // for the rest of the parameters add url parameters
        foreach (DictionaryEntry entry in urlParameters)
        {
            var key = (string)entry.Key;
            var value = (string)entry.Value;
            string paramSign = isFirst ? "?" : "&";
            parameterBuilder.Append(value: paramSign);
            parameterBuilder.Append(value: MyUri.EscapeUriString(stringToEscape: key));
            if (value != null)
            {
                parameterBuilder.Append(value: "=");
                parameterBuilder.Append(value: MyUri.EscapeUriString(stringToEscape: value));
            }
            isFirst = false;
        }
        string result = isUrlEscaped ? url : MyUri.EscapeUriString(stringToEscape: url);
        result += parameterBuilder.ToString();
        if (!forceExternal || result.IndexOf(value: ":") != -1)
        {
            return result;
        }
        if (string.IsNullOrEmpty(value: externalScheme))
        {
            externalScheme = "http";
        }
        string delimiter = Uri.SchemeDelimiter;
        if (externalScheme == "mailto")
        {
            delimiter = ":";
        }
        result = externalScheme.Trim() + delimiter + result.Trim();
        return result;
    }

    public HttpResult SendRequest(Request request)
    {
        try
        {
            using WebResponse response = GetResponse(request: request);
            if (response is not HttpWebResponse httpResponse)
            {
                throw new Exception(message: "WebResponse is not of HttpResponse type.");
            }
            return new HttpResult(
                Content: ProcessReturnValue(
                    returnAsStream: request.ReturnAsStream,
                    response: httpResponse
                ),
                StatusCode: (int)httpResponse.StatusCode,
                StatusDescription: httpResponse.StatusDescription,
                Headers: httpResponse.Headers.AllKeys.ToDictionary(
                    keySelector: key => key,
                    elementSelector: key => httpResponse.Headers[name: key]
                )
            );
        }
        catch (WebException webException)
        {
            return HandleWebException(request: request, webException: webException);
        }
    }

    private HttpResult HandleWebException(Request request, WebException webException)
    {
        if (request.ThrowExceptionOnError)
        {
            var info = "";
            if (webException.Response is HttpWebResponse httpResponse)
            {
                info = ReadResponseTextRespectingContentEncoding(httpResponse: httpResponse);
            }
            throw new Exception(
                message: webException.Message + Environment.NewLine + info,
                innerException: webException
            );
        }
        else
        {
            if (webException.Response is HttpWebResponse httpResponse)
            {
                return new HttpResult(
                    Content: ProcessReturnValue(
                        returnAsStream: request.ReturnAsStream,
                        response: httpResponse
                    ),
                    StatusCode: (int)httpResponse.StatusCode,
                    StatusDescription: httpResponse.StatusDescription,
                    Headers: httpResponse.Headers.AllKeys.ToDictionary(
                        keySelector: key => key,
                        elementSelector: key => httpResponse.Headers[name: key]
                    )
                );
            }

            return new HttpResult(
                Content: null,
                StatusCode: null,
                StatusDescription: null,
                Headers: null,
                Exception: webException
            );
        }
    }

    private object ProcessReturnValue(bool returnAsStream, HttpWebResponse response)
    {
        using Stream responseStream = StreamFromResponse(response: response);
        if (returnAsStream)
        {
            return responseStream;
        }
        if (
            response.ContentType.Equals(value: "text/xml")
            || response.ContentType.Equals(value: "application/xml")
            || response.ContentType.EndsWith(value: "+xml")
        )
        {
            // for xml we will ignore encoding set in the HTTP header
            // because sometimes it is not present
            // but we have the encoding in the XML header, so we take
            // it from there
            IXmlContainer container = new XmlContainer();
            if (response.ContentLength != 0)
            {
                container.Xml.Load(inStream: responseStream);
            }
            return container;
        }
        if (response.ContentType.StartsWith(value: "text/"))
        {
            // result is text
            return ReadResponseText(httpResponse: response, responseStream: responseStream);
        }
        if (
            response.ContentType.StartsWith(value: "application/json")
            || response.ContentType.EndsWith(value: "+json")
        )
        {
            string body = ReadResponseText(httpResponse: response, responseStream: responseStream);
            XmlDocument xmlDocument;
            // deserialize from JSON to XML
            try
            {
                xmlDocument = JsonConvert.DeserializeXmlNode(
                    value: body,
                    deserializeRootElementName: "ROOT",
                    writeArrayAttribute: false,
                    encodeSpecialCharacters: true
                );
            }
            catch (JsonSerializationException)
            {
                xmlDocument = JsonConvert.DeserializeXmlNode(
                    value: "{\"ARRAY\":" + body + "}",
                    deserializeRootElementName: "ROOT",
                    writeArrayAttribute: false,
                    encodeSpecialCharacters: true
                );
            }
            return new XmlContainer(xmlDocument: xmlDocument);
        }
        // result is binary
        return StreamTools.ReadToEnd(input: responseStream);
    }

    private static Stream StreamFromResponse(HttpWebResponse response)
    {
        var encodingString = string.IsNullOrEmpty(value: response.ContentEncoding)
            ? ""
            : response.ContentEncoding.ToLower();
        if (encodingString.Contains(value: "gzip"))
        {
            return new GZipStream(
                stream: response.GetResponseStream(),
                mode: CompressionMode.Decompress
            );
        }
        if (encodingString.Contains(value: "deflate"))
        {
            return new DeflateStream(
                stream: response.GetResponseStream(),
                mode: CompressionMode.Decompress
            );
        }
        return response.GetResponseStream();
    }

    public WebResponse GetResponse(Request request)
    {
        if (serviceProvider != null)
        {
            var providerContainer =
                serviceProvider.GetService<ClientAuthenticationProviderContainer>();
            providerContainer.TryAuthenticate(url: request.Url, headers: request.Headers);
        }
        else
        {
            log.Warn(
                message: "Request could not be authenticated because serviceProvider is null. This is expected if you send Http requests from the Architect."
            );
        }
        var webRequest = WebRequest.Create(requestUriString: request.Url);
        var httpWebRequest = webRequest as HttpWebRequest;
        if (request.Timeout != null)
        {
            webRequest.Timeout = request.Timeout.Value;
        }
        if (httpWebRequest != null)
        {
            httpWebRequest.UserAgent = "ORIGAM (origam.com)";
            if (request.IgnoreHttpsErrors)
            {
                httpWebRequest.ServerCertificateValidationCallback += (
                    sender,
                    certificate,
                    chain,
                    sslPolicyErrors
                ) => true;
            }
            if (request.Cookies is { Count: > 0 })
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                foreach (Cookie cookie in request.Cookies)
                {
                    httpWebRequest.CookieContainer.Add(cookie: cookie);
                }
            }
        }
        if (request.Headers != null)
        {
            foreach (DictionaryEntry entry in request.Headers)
            {
                string name = (string)entry.Key;
                string value = (string)entry.Value;
                if (
                    httpWebRequest != null
                    && name.Equals(
                        value: "User-Agent",
                        comparisonType: StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    httpWebRequest.UserAgent = value;
                }
                else
                {
                    httpWebRequest?.Headers.Add(name: name, value: value);
                }
            }
        }
        if (request.Method != null)
        {
            webRequest.Method = request.Method;
        }
        if (request.AuthenticationType != null)
        {
            var credentials = Convert.ToBase64String(
                inArray: Encoding.ASCII.GetBytes(s: request.UserName + ":" + request.Password)
            );
            webRequest.Headers[header: HttpRequestHeader.Authorization] =
                $"{request.AuthenticationType} {credentials}";
        }
        if (request.Content != null)
        {
            webRequest.ContentType = request.ContentType;
            var encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(s: request.Content);
            webRequest.ContentLength = bytes.LongLength;
            Stream stream = webRequest.GetRequestStream();
            StreamTools.Write(output: stream, bytes: bytes);
            stream.Close();
        }
        WebResponse response = webRequest.GetResponse();
        if (httpWebRequest != null && response is HttpWebResponse httpWebResponse)
        {
            FixCookies(request: httpWebRequest, response: httpWebResponse);
        }
        return response;
    }

    public string ReadResponseText(HttpWebResponse httpResponse, Stream responseStream)
    {
        using var streamReader = new StreamReader(
            stream: responseStream,
            encoding: EncodingFromResponse(response: httpResponse)
        );
        return streamReader.ReadToEnd();
    }

    public string ReadResponseTextRespectingContentEncoding(HttpWebResponse httpResponse)
    {
        if (httpResponse.GetResponseStream() == null)
        {
            return string.Empty;
        }
        string encoding = httpResponse.ContentEncoding.ToLower();
        using Stream responseStream = encoding switch
        {
            not null when encoding.Contains(value: "gzip") => new GZipStream(
                stream: httpResponse.GetResponseStream()!,
                mode: CompressionMode.Decompress
            ),
            not null when encoding.Contains(value: "deflate") => new DeflateStream(
                stream: httpResponse.GetResponseStream()!,
                mode: CompressionMode.Decompress
            ),
            _ => httpResponse.GetResponseStream(),
        };
        using var streamReader = new StreamReader(
            stream: responseStream!,
            encoding: EncodingFromResponse(response: httpResponse)
        );
        return streamReader.ReadToEnd();
    }

    public Encoding EncodingFromResponse(HttpWebResponse response)
    {
        if (!string.IsNullOrEmpty(value: response.CharacterSet))
        {
            return Encoding.GetEncoding(name: response.CharacterSet);
        }
        string[] contentType = response.ContentType.Split(separator: ";".ToCharArray());
        foreach (string contentTypePart in contentType)
        {
            if (contentTypePart.Trim().ToLower().StartsWith(value: "charset="))
            {
                string[] charset = contentTypePart.Split(separator: "=".ToCharArray());
                return Encoding.GetEncoding(name: charset[1]);
            }
        }
        return Encoding.UTF8;
    }

    public string GetMimeType(string fileName)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(
            subpath: fileName,
            contentType: out string contentType
        );
        return contentType ?? "application/unknown";
    }

    public string GetDefaultExtension(string mimeType)
    {
        var extensionMimeTypePair =
            new FileExtensionContentTypeProvider()
                .Mappings.Cast<KeyValuePair<string, string>?>()
                .FirstOrDefault(predicate: pair => pair.Value.Value == mimeType)
            ?? new KeyValuePair<string, string>(key: string.Empty, value: "");
        return extensionMimeTypePair.Key;
    }

    // http://stackoverflow.com/questions/6695208/uri-escapedatastring-invalid-uri-the-uri-string-is-too-long
    public string EscapeDataStringLong(string value)
    {
        int limit = 2000;
        var stringBuilder = new StringBuilder(capacity: value.Length);
        int loops = value.Length / limit;
        for (int i = 0; i <= loops; i++)
        {
            stringBuilder.Append(
                value: i < loops
                    ? Uri.EscapeDataString(
                        stringToEscape: value.Substring(startIndex: limit * i, length: limit)
                    )
                    : Uri.EscapeDataString(stringToEscape: value.Substring(startIndex: limit * i))
            );
        }
        return stringBuilder.ToString();
    }
}

public record HttpResult(
    object Content,
    int? StatusCode,
    string StatusDescription,
    Dictionary<string, string> Headers,
    Exception Exception = null
);

public class MyUri : Uri
{
    public MyUri(string url)
        : base(uriString: url) { }

    public static new string EscapeUriString(string stringToEscape)
    {
        return Uri.EscapeUriString(stringToEscape: stringToEscape);
    }

    public static new string EscapeDataString(string stringToEscape)
    {
        return Uri.EscapeDataString(stringToEscape: stringToEscape);
    }
}
