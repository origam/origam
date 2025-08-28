#region license

/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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
using System.Net;

namespace Origam;

public class Request
{
    private static readonly int emptyHashTableHash =
        EqualityComparer<Hashtable>.Default.GetHashCode(new Hashtable());

    public string Url { get; }
    public string Method { get; }
    public string Content { get; }
    public string ContentType { get; }
    public Hashtable Headers { get; }
    public bool ReturnAsStream { get; }
    public int? Timeout { get; }
    public bool ThrowExceptionOnError { get; }
    public CookieCollection Cookies { get; }
    public bool IgnoreHttpsErrors { get; }
    public string Password { get; }
    public string UserName { get; }
    public string AuthenticationType { get; }

    public Request(
        string url,
        string method,
        string content = null,
        string contentType = null,
        Hashtable headers = null,
        string authenticationType = null,
        string userName = null,
        string password = null,
        bool returnAsStream = false,
        int? timeout = null,
        bool throwExceptionOnError = true,
        CookieCollection cookies = null,
        bool ignoreHttpsErrors = false
    )
    {
        Url = url;
        Method = method;
        Content = content;
        ContentType = contentType;
        Headers = headers ?? new Hashtable();
        ReturnAsStream = returnAsStream;
        Timeout = timeout;
        ThrowExceptionOnError = throwExceptionOnError;
        Cookies = cookies;
        IgnoreHttpsErrors = ignoreHttpsErrors;
        (AuthenticationType, UserName, Password) = ParseAuthentication(
            url,
            authenticationType,
            userName,
            password
        );
    }

    private (string type, string userName, string password) ParseAuthentication(
        string url,
        string authenticationType,
        string userName,
        string password
    )
    {
        if (
            string.IsNullOrEmpty(authenticationType)
            && string.IsNullOrEmpty(password)
            && string.IsNullOrEmpty(userName)
        )
        {
            var myUrl = new Uri(url);
            // try to parse user credentials and if present:
            // use it as a http basic authorization
            string credentials = myUrl.UserInfo;
            if (!credentials.Contains(":"))
            {
                return (authenticationType, userName, password);
            }
            string[] credentialsSplit = credentials.Split(':');
            if (credentialsSplit.Length == 2)
            {
                return (
                    "Basic",
                    Uri.UnescapeDataString(credentialsSplit[0]),
                    Uri.UnescapeDataString(credentialsSplit[1])
                );
            }
        }
        return (authenticationType, userName, password);
    }

    public override bool Equals(object obj)
    {
        return obj is Request request
            && Url == request.Url
            && Method == request.Method
            && Content == request.Content
            && ContentType == request.ContentType
            && (
                EqualityComparer<Hashtable>.Default.Equals(Headers, request.Headers)
                || (
                    (Headers != null && Headers.Count == 0)
                    && (request.Headers != null && request.Headers.Count == 0)
                )
            )
            && ReturnAsStream == request.ReturnAsStream
            && Timeout == request.Timeout
            && ThrowExceptionOnError == request.ThrowExceptionOnError
            && EqualityComparer<CookieCollection>.Default.Equals(Cookies, request.Cookies)
            && IgnoreHttpsErrors == request.IgnoreHttpsErrors
            && Password == request.Password
            && UserName == request.UserName
            && AuthenticationType == request.AuthenticationType;
    }

    public override int GetHashCode()
    {
        int hashCode = 612240280;
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Url);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Method);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Content);
        hashCode =
            (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(ContentType);
        hashCode =
            (hashCode * -1521134295)
            + (
                Headers is { Count: 0 }
                    ? emptyHashTableHash
                    : EqualityComparer<Hashtable>.Default.GetHashCode(Headers)
            );
        hashCode = (hashCode * -1521134295) + ReturnAsStream.GetHashCode();
        hashCode = (hashCode * -1521134295) + Timeout.GetHashCode();
        hashCode = (hashCode * -1521134295) + ThrowExceptionOnError.GetHashCode();
        hashCode =
            (hashCode * -1521134295)
            + EqualityComparer<CookieCollection>.Default.GetHashCode(Cookies);
        hashCode = (hashCode * -1521134295) + IgnoreHttpsErrors.GetHashCode();
        hashCode =
            (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Password);
        hashCode =
            (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(UserName);
        hashCode =
            (hashCode * -1521134295)
            + EqualityComparer<string>.Default.GetHashCode(AuthenticationType);
        return hashCode;
    }
}
