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

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Origam.Server.Pages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Origam.Server;
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
    public string UrlReferrerAbsoluteUri => throw new NotImplementedException();
    public Parameters Params => throw new NotImplementedException();
    public PostedFile FilesGet(string name)
    {
        throw new NotImplementedException();
    }
}
