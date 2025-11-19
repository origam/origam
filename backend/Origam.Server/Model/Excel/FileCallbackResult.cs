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
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Origam.Server.Model.Excel;

public class FileCallbackResult : FileResult
{
    private Func<Stream, ActionContext, Task> _callback;

    public FileCallbackResult(
        MediaTypeHeaderValue contentType,
        Func<Stream, ActionContext, Task> callback
    )
        : base(contentType?.ToString())
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = new FileCallbackResultExecutor(
            context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
        );
        return executor.ExecuteAsync(context, this);
    }

    private sealed class FileCallbackResultExecutor : FileResultExecutorBase
    {
        public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
            : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory)) { }

        public Task ExecuteAsync(ActionContext context, FileCallbackResult result)
        {
            // SetHeadersAndLog(context, result);
            return result._callback(context.HttpContext.Response.Body, context);
        }
    }
}
