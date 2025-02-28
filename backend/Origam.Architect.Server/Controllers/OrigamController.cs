using System.Data;
using Microsoft.AspNetCore.Mvc;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Architect.Server.Controllers;

public abstract class OrigamController(
    IWebHostEnvironment environment,
    ILogger<OrigamController> log)
    : ControllerBase
{
    protected readonly ILogger<OrigamController> log = log;

    protected IActionResult RunWithErrorHandler(Func<IActionResult> func)
    {
        Task<IActionResult> AsynFunc() => Task.FromResult(func());
        return RunWithErrorHandlerAsync(AsynFunc).Result;
    }

    protected async Task<IActionResult> RunWithErrorHandlerAsync(
        Func<Task<IActionResult>> func)
    {
        object GetReturnObject(Exception ex, string defaultMessage = null)
        {
            // return environment.IsDevelopment()
            //     ? ex
            //     : new
            //     {
            //         message = defaultMessage ??
            //                   "An error has occured. There may be some details in the log file."
            //     };
            
            return new
                {
                    message = defaultMessage ??
                              "An error has occured. There may be some details in the log file."
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