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

using MoreLinq;
using Origam.Architect.Server.Enums;
using Origam.DA.Service;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Architect.Server.Services;

public class DeploymentScriptRunnerService(ILogger<DeploymentScriptRunnerService> logger)
{
    public void RunDeploymentScript(AbstractUpdateScriptActivity script)
    {
        var transactionId = Guid.NewGuid().ToString();

        try
        {
            logger.LogInformation("Executing deployment script: {ScriptName}", script.Name);
            ExecuteActivity(script, transactionId);
            ResourceMonitor.Commit(transactionId);
            logger.LogInformation(
                "Deployment script executed successfully: {ScriptName}",
                script.Name
            );
        }
        catch (Exception ex)
        {
            ResourceMonitor.Rollback(transactionId);
            logger.LogError(ex, "Error executing deployment script: {ScriptName}", script.Name);
            throw new Exception($"Error executing deployment script '{script.Name}'", ex);
        }
    }

    private void ExecuteActivity(AbstractUpdateScriptActivity activity, string transactionId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Executing deployment activity: {ActivityName}", activity.Name);
        }

        try
        {
            if (activity is ServiceCommandUpdateScriptActivity serviceActivity)
            {
                ExecuteServiceCommandActivity(serviceActivity, transactionId);
            }
            // TODO: FileRestoreUpdateScriptActivity
            // else if (activity is FileRestoreUpdateScriptActivity fileActivity)
            // {
            //     ExecuteFileRestoreActivity(fileActivity);
            // }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activity),
                    activity,
                    "Unsupported deployment activity type"
                );
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                logger.LogCritical(
                    ex,
                    "Error occurred while running deployment activity {ActivityPath}",
                    activity.Path
                );
            }
            throw;
        }
    }

    private void ExecuteServiceCommandActivity(
        ServiceCommandUpdateScriptActivity activity,
        string transactionId
    )
    {
        var service = ServiceManager.Services.GetService<IBusinessServicesService>();
        if (service == null)
        {
            throw new InvalidOperationException("Business services service not available");
        }

        IServiceAgent agent = service.GetAgent(activity.Service.Name, null, null);
        var result = "";

        if (
            activity.DatabaseType
            != ((AbstractSqlDataService)DataServiceFactory.GetDataService()).PlatformName
        )
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            settings.DeployPlatforms?.ForEach(platform =>
            {
                var databaseType = (DatabaseType)
                    Enum.Parse(typeof(DatabaseType), platform.GetParseEnum(platform.DataService));
                if (databaseType == (DatabaseType)activity.DatabaseType)
                {
                    agent.SetDataService(DataServiceFactory.GetDataService(platform));
                    result = agent.ExecuteUpdate(activity.CommandText, transactionId);
                }
            });
        }
        else
        {
            result = agent.ExecuteUpdate(activity.CommandText, transactionId);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(result);
        }
    }

    // TODO: FileRestoreUpdateScriptActivity
    // private void ExecuteFileRestoreActivity(FileRestoreUpdateScriptActivity activity)
    // {
    //     throw new NotImplementedException(
    //         "File restore deployment activities are not supported in this context"
    //     );
    // }
}
