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

using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Server.Middleware;

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
            if (env.IsDevelopment())
            {
                return new
                {
                    type = ex.GetType().FullName,
                    message = ex.Message,
                    stackTrace = ex.StackTrace,
                    detail = ex.ToString(),
                };
            }

            return new { message = defaultMessage ?? Resources.ErrorDetailsInLog };
        }

        try
        {
            await next(context);
        }
        catch (SessionExpiredException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteJsonAsync(context, GetReturnObject(ex, ex.Message));
        }
        catch (RowNotFoundException ex)
        {
            object returnObject = new
            {
                message = "row not found",
                exception = env.IsDevelopment() ? ex : null,
            };
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteJsonAsync(context, returnObject);
        }
        catch (DBConcurrencyException ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteJsonAsync(context, GetReturnObject(ex));
        }
        catch (ServerObjectDisposedException ex)
        {
            // Suggests to the client that this error could be ignored
            context.Response.StatusCode = 474;
            await WriteJsonAsync(context, GetReturnObject(ex));
        }
        catch (Exception ex)
        {
            // Let MVC handle Account UI errors => redirect to /Error (generic error page)
            if (context.Request.Path.StartsWithSegments("/Account"))
            {
                throw;
            }

            switch (ex)
            {
                case OrigamDataException or OrigamSecurityException:
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await WriteJsonAsync(context, GetReturnObject(ex));
                    break;
                }
                case OrigamValidationException:
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await WriteJsonAsync(context, GetReturnObject(ex, ex.Message));
                    break;
                }
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
