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
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Xml;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Origam.Service.Core;

namespace Origam
{
    public class HttpTools : IHttpTools
	{
        private static readonly log4net.ILog log = 
			log4net.LogManager.GetLogger(System.Reflection.MethodBase
        .GetCurrentMethod().DeclaringType);

        public static HttpTools Instance { get; set; } = new();
		
		public static void SetDIServiceProvider(IServiceProvider serviceProvider)
		{
			Instance.serviceProvider = serviceProvider;
		}

		private IServiceProvider serviceProvider;

		private HttpTools()
		{
		}

		private void FixCookies(HttpWebRequest request,
			HttpWebResponse response)
		{			
			for (int i = 0; i < response.Headers.Count; i++)
			{
				string name = response.Headers.GetKey(i);
				
				if (name != "Set-Cookie")
					continue;
				
				string setCookieHeader = response.Headers.Get(i);
				List<string> singleCookies 
					= SplitCookiesHeaderToSingleCookies(setCookieHeader);

				string host = request.Host.Split(':')[0];

				CookiesFromStrings(host, singleCookies)
					.ForEach(cookie => response.Cookies.Add(cookie));
				
			}
		}

		public List<Cookie> CookiesFromStrings(string host,
			List<string> singleCookies)
		{
			List<Cookie> cookies = new List<Cookie>();
			foreach (string singleCookie in singleCookies)
			{
				Match match = Regex.Match(singleCookie, @"(\S+?)=(.+?);");
				if (match.Captures.Count == 0)
					continue;
				cookies.Add(
					new Cookie(
						match.Groups[1].ToString(),
						match.Groups[2].ToString(),
						"/",
						host));
			}

			return cookies;
		}

		public List<string> SplitCookiesHeaderToSingleCookies(
			string setCookieHeader)
		{
			string[] cookieSegments = setCookieHeader.Split(',');
			List<string> singleCookies = new List<string>();
			
			int i = 0;
			while (i < cookieSegments.Length)
			{
				bool containsExpisesParameter 
					= cookieSegments[i].IndexOf("expires=",
						  StringComparison.OrdinalIgnoreCase) > 0;
				
				if (containsExpisesParameter)
				{
					singleCookies.Add(
						cookieSegments[i] + "," + cookieSegments[i + 1]);
					i++;
				}
				else
				{
					singleCookies.Add(cookieSegments[i]);
				}
				i++;
			}

			return singleCookies;
		}

		public string BuildUrl(string sURL, Hashtable parameters, bool forceExternal, string externalScheme, bool isUrlEscaped)
		{
			Hashtable urlParameters = new Hashtable();

			// replace url parts, e.g. {id}
			foreach (DictionaryEntry entry in parameters)
			{
				string sKey = entry.Key.ToString();
				string sValue = null;

				if (entry.Value != null)
				{
					sValue = entry.Value.ToString();
				}

				string replacement = "{" + sKey + "}";

				if (sURL.IndexOf(replacement) > -1)
				{
					if (sValue == null)
					{
						throw new Exception(ResourceUtils.GetString("UrlPartParameterNull"));
					}

					sURL = sURL.Replace(replacement, sValue);
				}
				else
				{
					urlParameters.Add(sKey, sValue);
				}
			}

			StringBuilder parameterBuilder = new StringBuilder();

			bool isFirst = true;

			// for the rest of the parameters add url parameters
			foreach (DictionaryEntry entry in urlParameters)
			{
				string sKey = (string)entry.Key;
				string sValue = (string)entry.Value;

				string paramSign = isFirst ? "?" : "&";

				parameterBuilder.Append(paramSign);
				parameterBuilder.Append(MyUri.EscapeUriString(sKey));

				if (sValue != null)
				{
					parameterBuilder.Append("=");
					parameterBuilder.Append(MyUri.EscapeUriString(sValue));
				}

				isFirst = false;
			}

			string result;
			if (isUrlEscaped)
			{
				result = sURL;
			}
			else
			{
				result = MyUri.EscapeUriString(sURL);
			}

			result += parameterBuilder.ToString();

			if (forceExternal && result.IndexOf(":") == -1)
			{
				if (externalScheme == null || externalScheme == String.Empty)
				{
					externalScheme = "http";
				}

				string delimiter = Uri.SchemeDelimiter;
				if (externalScheme == "mailto") delimiter = ":";

				result = externalScheme.Trim() + delimiter + result.Trim();
			}

			return result;
		}

		public HttpResult SendRequest(Request request)
        {
            try
            {
				using WebResponse response = GetResponse(request);
				if (response is not HttpWebResponse httpResponse)
				{
					throw new Exception(
						"WebResponse is not of HttpResponse type.");
				}
				return new HttpResult(
					Content: ProcessReturnValue(request.ReturnAsStream, httpResponse), 
					StatusCode: (int)httpResponse.StatusCode, 
					StatusDescription: httpResponse.StatusDescription, 
					Headers: httpResponse.Headers.AllKeys
								.ToDictionary(key => key, key => httpResponse.Headers[key]));
			}
			catch (WebException webException)
			{
				return HandleWebException(request, webException);
			}
		}

		private HttpResult HandleWebException(Request request, WebException webException)
		{
			if (request.ThrowExceptionOnError)
			{
				var info = "";
				if (webException.Response is HttpWebResponse httpResponse)
				{
					info = ReadResponseTextRespectionContentEncoding(
						httpResponse);
				}
				throw new Exception(
					webException.Message + Environment.NewLine + info,
					webException);
			}
			else
			{
				if (webException.Response is HttpWebResponse httpResponse)
				{
					return new HttpResult(
						Content: ProcessReturnValue(request.ReturnAsStream, httpResponse),
						StatusCode: (int)httpResponse.StatusCode,
						StatusDescription: httpResponse.StatusDescription,
						Headers: httpResponse.Headers.AllKeys
							.ToDictionary(key => key, key => httpResponse.Headers[key]));
				}
				else
				{
					return new HttpResult(
						Content: null,
						StatusCode: null,
						StatusDescription: null,
						Headers: null,
						Exception: webException);
				}
			}
		}

		private object ProcessReturnValue(bool returnAsStream, HttpWebResponse response)
		{ 
			using Stream responseStream = StreamFromResponse(response);
			if (returnAsStream)
			{
				return responseStream;
			}
			if (response.ContentType.Equals("text/xml")
					|| response.ContentType.Equals("application/xml")
					|| response.ContentType.EndsWith("+xml"))
			{
				// for xml we will ignore encoding set in the HTTP header 
				// because sometimes it is not present
				// but we have the encoding in the XML header, so we take
				// it from there
				IXmlContainer container = new XmlContainer();
				if (response.ContentLength != 0)
				{
						container.Xml.Load(responseStream);
				}
				return container;
			}
			if (response.ContentType.StartsWith("text/"))
			{
				// result is text
				return ReadResponseText(response, responseStream);
			}
			if (response.ContentType.StartsWith("application/json")
					|| response.ContentType.EndsWith("+json"))
			{
				string body = ReadResponseText(
						response, responseStream);
				XmlDocument xmlDocument;
				// deserialize from JSON to XML
				try
				{
					xmlDocument = JsonConvert.DeserializeXmlNode(
							body, "ROOT");
				}
				catch (JsonSerializationException)
				{
					xmlDocument = JsonConvert.DeserializeXmlNode(
							"{\"ARRAY\":" + body + "}", "ROOT");
				}
				return new XmlContainer(xmlDocument);
			}
			// result is binary
			return StreamTools.ReadToEnd(responseStream);
		}

		private static Stream StreamFromResponse(HttpWebResponse response)
		{
			var encodingString = string.IsNullOrEmpty(response.ContentEncoding)
					? "" : response.ContentEncoding.ToLower();
			if (encodingString.Contains("gzip"))
			{
				return new GZipStream(response.GetResponseStream(),
					CompressionMode.Decompress);
			}
			if (encodingString.Contains("deflate"))
			{
				return new DeflateStream(response.GetResponseStream(),
					CompressionMode.Decompress);
			}
			return response.GetResponseStream();
		}

		public WebResponse GetResponse(Request request)
		{
			var headers = request.Headers ?? new Hashtable();
			if (serviceProvider != null)
			{
				var providerContainer = serviceProvider.GetService<ClientAuthenticationProviderContainer>();
				providerContainer.TryAuthenticate(request.Url, headers);
			}
			else 
			{
				log.Warn("Request could not be authenticated because serviceProvider is null. This is expected if you send Http requests from the Architect.");
			}

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
			                                       SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls;
			
			WebRequest webRequest = WebRequest.Create(request.Url);
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            if (request.Timeout != null)
			{
                webRequest.Timeout = request.Timeout.Value;
			}
			if (httpWebRequest != null)
			{
                httpWebRequest.UserAgent = "ORIGAM (origam.com)";
				if (request.IgnoreHTTPSErrors)
				{
					httpWebRequest.ServerCertificateValidationCallback
						+= (sender, certificate, chain, sslPolicyErrors) => { return true; };
				}
				if (request.Cookies != null && request.Cookies.Count > 0)
				{
					httpWebRequest.CookieContainer = new CookieContainer();
					foreach (Cookie c in request.Cookies)
					{
						httpWebRequest.CookieContainer.Add(c);
					}
				}
			}
			if (headers != null)
			{
				foreach (DictionaryEntry entry in headers)
				{
                    string name = (string)entry.Key;
                    string value = (string)entry.Value;
                    if (httpWebRequest != null &&
                        name.Equals("User-Agent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        httpWebRequest.UserAgent = value;
                    }
                    else
                    {
                        request.Headers.Add(name, value);
                    }
                }
			}
			if (request.Method != null)
			{
                webRequest.Method = request.Method;
			}
			if (request.AuthenticationType != null)
			{
				string credentials = Convert.ToBase64String(
					Encoding.ASCII.GetBytes(request.UserName + ":" + request.Password));
				request.Headers[HttpRequestHeader.Authorization] = string.Format(
					"{0} {1}", request.AuthenticationType, credentials);
			}
			if (request.Content != null)
			{
				webRequest.ContentType = request.ContentType;
				UTF8Encoding encoding = new UTF8Encoding();
				byte[] bytes = encoding.GetBytes(request.Content);
				webRequest.ContentLength = bytes.LongLength;

				Stream stream = webRequest.GetRequestStream();
				StreamTools.Write(stream, bytes);
				stream.Close();
			}
			WebResponse response = webRequest.GetResponse();
			if (webRequest as HttpWebRequest != null && response as HttpWebResponse != null)
			{
				FixCookies(webRequest as HttpWebRequest, response as HttpWebResponse);
			}
			return response;
		}


		public string ReadResponseText(HttpWebResponse httpResponse, Stream responseStream)
		{
			using (StreamReader sr = new StreamReader(responseStream, EncodingFromResponse(httpResponse)))
			{
				return sr.ReadToEnd();
			}
		}

		public string ReadResponseTextRespectionContentEncoding(HttpWebResponse httpResponse)
		{
			string encodingString = httpResponse.ContentEncoding==null?"": httpResponse.ContentEncoding.ToLower();
			using (Stream responseStream =
				encodingString.Contains("gzip") ?
					new GZipStream(httpResponse.GetResponseStream(), CompressionMode.Decompress)
					: encodingString.Contains("deflate") ?
						new DeflateStream(httpResponse.GetResponseStream(), CompressionMode.Decompress)
						: httpResponse.GetResponseStream()
			)
			{
				using (StreamReader sr = new StreamReader(responseStream, EncodingFromResponse(httpResponse)))
				{
					return sr.ReadToEnd();
				}
			}
		}

		public Encoding EncodingFromResponse(HttpWebResponse response)
		{
			if (response.CharacterSet != String.Empty && response.CharacterSet != null)
			{
				return Encoding.GetEncoding(response.CharacterSet);
			}

			string[] contentType = response.ContentType.Split(";".ToCharArray());

			foreach (string contentTypePart in contentType)
			{
				if (contentTypePart.Trim().ToLower().StartsWith("charset="))
				{
					string[] charset = contentTypePart.Split("=".ToCharArray());

					return Encoding.GetEncoding(charset[1]);
				}
			}

			return Encoding.UTF8;
		}

		public string GetMimeType(string fileName)
		{
			new FileExtensionContentTypeProvider()
				.TryGetContentType(fileName, out string contentType);
			return contentType ?? "application/unknown";
		}

		public string GetDefaultExtension(string mimeType)
		{
			var extensionMimeTypePair = new FileExtensionContentTypeProvider()
				                            .Mappings
				                            .Cast<KeyValuePair<string, string>?>()
				                            .FirstOrDefault(pair => pair.Value.Value == mimeType)
			                            ?? new KeyValuePair<string, string>(string.Empty, "");
			return extensionMimeTypePair.Key;
		}
		/// <summary>
		/// http://stackoverflow.com/questions/6695208/uri-escapedatastring-invalid-uri-the-uri-string-is-too-long
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string EscapeDataStringLong(string value)
		{
			int limit = 2000;

			StringBuilder sb = new StringBuilder(value.Length);
			int loops = value.Length / limit;

			for (int i = 0; i <= loops; i++)
			{
				if (i < loops)
				{
					sb.Append(Uri.EscapeDataString(value.Substring(limit * i, limit)));
				}
				else
				{
					sb.Append(Uri.EscapeDataString(value.Substring(limit * i)));
				}
			}
			return sb.ToString();
		}
	}

	public record HttpResult(
		object Content, 
		int? StatusCode,
		string? StatusDescription,
		Dictionary<string, string> Headers,
		Exception Exception=null);

	public class MyUri : Uri
	{
		public MyUri(string url) : base (url)  {}

		public static new string EscapeUriString(string stringToEscape)
		{
			return Uri.EscapeUriString(stringToEscape);
		}
		public static new string EscapeDataString(string stringToEscape)
		{
			return Uri.EscapeDataString(stringToEscape);
		}
	}
}
