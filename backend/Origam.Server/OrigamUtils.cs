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

using System;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Origam.DA.Common;
using Origam.DA.Service;
using Origam.Server.Middleware;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server;
public static class OrigamUtils
{
    public static void ConnectOrigamRuntime(ILoggerFactory loggerFactory,
        bool reloadModelWhenFilesChangesDetected)
    {
        OrigamEngine.OrigamEngine.ConnectRuntime();
        if (!reloadModelWhenFilesChangesDetected)
        {
            return;
        }
        IPersistenceService persistence 
            = ServiceManager.Services.GetService<IPersistenceService>();
        if (persistence is FilePersistenceService filePersistenceService)
        {
            filePersistenceService.ReloadNeeded += (sender, args) =>
            {
                string errorMessage = "";
                try
                {
                    Maybe<XmlLoadError> maybeError =
                        filePersistenceService.Reload();
                    if (maybeError.HasValue)
                    {
                        errorMessage = maybeError.Value.Message;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage += ex.Message + "\n" + ex.StackTrace;
                }
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    FatalErrorMiddleware.ErrorMessage =
                        "An error has occured during automatic model reload. Please restart the server.\n" + errorMessage;
                    loggerFactory.CreateLogger("Model Reload")
                        .Log(LogLevel.Error, FatalErrorMiddleware.ErrorMessage);
                }
            };
        }
    }

    public static void CleanUpDatabase()
    {
        IBusinessServicesService service = ServiceManager.Services.GetService<IBusinessServicesService>();
        IServiceAgent agent = service.GetAgent("DataService", null, null);
        Enums.DatabaseType platform = ((AbstractSqlDataService)DataServiceFactory.GetDataService())
            .PlatformName;
        switch (platform)
        {
            case Enums.DatabaseType.MsSql:
            {
                agent.ExecuteUpdate("Exec OrigamIdentityGrantCleanup", null);
                break;
            }
            case Enums.DatabaseType.PgSql:
            {
                agent.ExecuteUpdate("CALL \"OrigamIdentityGrantCleanup\"();", null);
                break;
            }
            default: 
                throw new Exception("Platform not supported " + platform);
        }
    }
}
