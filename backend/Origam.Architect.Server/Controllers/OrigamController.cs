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
using Microsoft.AspNetCore.Mvc;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Architect.Server.Controllers;

public abstract class OrigamController(
    ILogger<OrigamController> log,
    IWebHostEnvironment environment
) : ControllerBase
{
    protected readonly ILogger<OrigamController> log = log;

    protected IActionResult RunWithErrorHandler(Func<IActionResult> func)
    {
        Task<IActionResult> AsynFunc() => Task.FromResult(func());
        return RunWithErrorHandlerAsync(AsynFunc).Result;
    }

    protected async Task<IActionResult> RunWithErrorHandlerAsync(Func<Task<IActionResult>> func)
    {
        object GetReturnObject(Exception ex, string defaultMessage = null)
        {
            return environment.IsDevelopment()
                ? ex.ToString()
                : new
                {
                    message = defaultMessage
                        ?? "An error has occured. There may be some details in the log file.",
                };
        }

        try
        {
            return await func();
        }
        catch (DBConcurrencyException ex)
        {
            log.LogError(ex, ex.Message);
            return StatusCode(409, GetReturnObject(ex));
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case IUserException:
                    return StatusCode(420, GetReturnObject(ex, ex.Message));
                default:
                    log.LogOrigamError(ex, ex.Message);
                    return StatusCode(500, GetReturnObject(ex));
            }
        }
    }
}
