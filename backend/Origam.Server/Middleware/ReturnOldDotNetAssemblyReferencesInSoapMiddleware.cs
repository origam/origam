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

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Origam.Server.Middleware;

// Some classes were in different namespaces and/or assemblies in the old .NET
// this creates issues when these classes are received by an old .NET client.
// This middleware fixes those issues by replacing the namespaces.
public class ReturnOldDotNetAssemblyReferencesInSoapMiddleware
{
    private readonly RequestDelegate next;

    public ReturnOldDotNetAssemblyReferencesInSoapMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;
            await next(context);
            using (var responseStream = await FixAssemblyReferencesInResponse(context.Response))
            {
                context.Response.Headers.ContentLength = null;
                await responseStream.CopyToAsync(originalBodyStream);
            }
        }
    }

    private async Task<MemoryStream> FixAssemblyReferencesInResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using (var streamReader = new StreamReader(response.Body))
        {
            var responseContent = await streamReader.ReadToEndAsync();
            var regex = new Regex(
                @"System.Private.CoreLib, Version=([\d\.]+), Culture=neutral, PublicKeyToken=([a-z0-9]+)"
            );
            Match match = regex.Match(responseContent);
            var coreLibVersion = match.Groups[1].Value;
            var coreLibKey = match.Groups[2].Value;
            responseContent = responseContent.Replace(
                $"System.Private.CoreLib, Version={coreLibVersion}, Culture=neutral, PublicKeyToken={coreLibKey}",
                "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            );
            var responseData = Encoding.UTF8.GetBytes(responseContent);
            return new MemoryStream(responseData);
        }
    }
}
