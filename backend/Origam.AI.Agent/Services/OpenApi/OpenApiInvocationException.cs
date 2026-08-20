#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

namespace Origam.AI.Agent.Services.OpenApi;

public class OpenApiInvocationException : Exception
{
    public OpenApiInvocationException(
        string functionName,
        int statusCode,
        string responseBody,
        string requestUrl,
        string requestPayload
    )
        : base($"{functionName} failed with HTTP status {statusCode}.")
    {
        FunctionName = functionName;
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RequestUrl = requestUrl;
        RequestPayload = requestPayload;
    }

    public string FunctionName { get; }
    public int StatusCode { get; }
    public string ResponseBody { get; }
    public string RequestUrl { get; }
    public string RequestPayload { get; }
}
