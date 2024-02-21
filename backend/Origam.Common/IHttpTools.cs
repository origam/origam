#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Net;
using System.Text;

namespace Origam;

public interface IHttpTools
{
    string BuildUrl(string url, Hashtable parameters, bool forceExternal, string externalScheme, bool isUrlEscaped);

    HttpResult SendRequest(Request request);

    WebResponse GetResponse(Request request);

    string ReadResponseText(HttpWebResponse httpResponse, Stream responseStream);
    string ReadResponseTextRespectingContentEncoding(HttpWebResponse httpResponse);
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