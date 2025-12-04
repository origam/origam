#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Data;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Architect.Server;

public class OrigamErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<OrigamErrorHandlingMiddleware> logger,
    IWebHostEnvironment env
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        object GetReturnObject(Exception ex, string defaultMessage = null)
        {
            return env.IsDevelopment()
                ? ex.ToString()
                : new
                {
                    message = defaultMessage
                        ?? "An error has occured. There may be some details in the log file.",
                };
        }

        try
        {
            await next(context);
        }
        catch (DBConcurrencyException ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteJsonAsync(context, GetReturnObject(ex));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case IUserException:
                {
                    context.Response.StatusCode = 420;
                    await WriteJsonAsync(context, GetReturnObject(ex, ex.Message));
                    break;
                }
                default:
                {
                    logger.LogOrigamError(ex, ex.Message);
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await WriteJsonAsync(context, GetReturnObject(ex));
                    break;
                }
            }
        }
    }

    private static async Task WriteJsonAsync(HttpContext context, object payload)
    {
        context.Response.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        await context.Response.WriteAsync(json);
    }
}
