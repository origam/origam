using System.Collections;
using System.IO;
using System.Net;
using System.Text;

namespace Origam;

public interface IHttpTools
{
    string BuildUrl(string sURL, Hashtable parameters, bool forceExternal, string externalScheme, bool isUrlEscaped);
    object SendRequest(string url, string method);

    object SendRequest(string url, string method, string content,
        string contentType, Hashtable headers, string authenticationType,
        string userName, string password, bool returnAsStream,
        int? timeout);

    WebResponse GetResponse(string url, string method, string content,
        string contentType, Hashtable headers);

    WebResponse GetResponse(string url, string method, string content,
        string contentType, Hashtable headers, string authenticationType,
        string userName, string password, int? timeoutMs, CookieCollection cookies,
        bool ignoreHTTPSErrors);

    string ReadResponseText(HttpWebResponse httpResponse, Stream responseStream);
    string ReadResponseTextRespectionContentEncoding(HttpWebResponse httpResponse);
    Encoding EncodingFromResponse(HttpWebResponse response);
    string GetMimeType(string fileName);
    string GetDefaultExtension(string mimeType);

    /// <summary>
    /// http://stackoverflow.com/questions/6695208/uri-escapedatastring-invalid-uri-the-uri-string-is-too-long
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    string EscapeDataStringLong(string value);
}