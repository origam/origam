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
            logger.LogInformation(
                message: "Executing deployment script: {ScriptName}",
                args: script.Name
            );
            ExecuteActivity(activity: script, transactionId: transactionId);
            ResourceMonitor.Commit(transactionId: transactionId);
            logger.LogInformation(
                message: "Deployment script executed successfully: {ScriptName}",
                args: script.Name
            );
        }
        catch (Exception ex)
        {
            ResourceMonitor.Rollback(transactionId: transactionId);
            logger.LogError(
                exception: ex,
                message: "Error executing deployment script: {ScriptName}",
                args: script.Name
            );
            throw new Exception(
                message: $"Error executing deployment script '{script.Name}'",
                innerException: ex
            );
        }
    }

    private void ExecuteActivity(AbstractUpdateScriptActivity activity, string transactionId)
    {
        if (logger.IsEnabled(logLevel: LogLevel.Information))
        {
            logger.LogInformation(
                message: "Executing deployment activity: {ActivityName}",
                args: activity.Name
            );
        }

        try
        {
            if (activity is ServiceCommandUpdateScriptActivity serviceActivity)
            {
                ExecuteServiceCommandActivity(
                    activity: serviceActivity,
                    transactionId: transactionId
                );
            }
            else if (activity is FileRestoreUpdateScriptActivity fileActivity)
            {
                ExecuteFileRestoreActivity(activity: fileActivity);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(activity),
                    actualValue: activity,
                    message: Strings.DeploymentScript_UnsupportedDeploymentActivityType
                );
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(logLevel: LogLevel.Critical))
            {
                logger.LogCritical(
                    exception: ex,
                    message: "Error occurred while running deployment activity {ActivityPath}",
                    args: activity.Path
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
            throw new InvalidOperationException(message: "Business services service not available");
        }

        IServiceAgent agent = service.GetAgent(
            serviceType: activity.Service.Name,
            ruleEngine: null,
            workflowEngine: null
        );
        var result = "";

        if (
            activity.DatabaseType
            != ((AbstractSqlDataService)DataServiceFactory.GetDataService()).PlatformName
        )
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            settings.DeployPlatforms?.ForEach(action: platform =>
            {
                var databaseType = (DatabaseType)
                    Enum.Parse(
                        enumType: typeof(DatabaseType),
                        value: platform.GetParseEnum(dataDataService: platform.DataService)
                    );
                if (databaseType == (DatabaseType)activity.DatabaseType)
                {
                    agent.SetDataService(
                        dataService: DataServiceFactory.GetDataService(deployPlatform: platform)
                    );
                    result = agent.ExecuteUpdate(
                        command: activity.CommandText,
                        transactionId: transactionId
                    );
                }
            });
        }
        else
        {
            result = agent.ExecuteUpdate(
                command: activity.CommandText,
                transactionId: transactionId
            );
        }

        if (logger.IsEnabled(logLevel: LogLevel.Information))
        {
            logger.LogInformation(message: result);
        }
    }

    private void ExecuteFileRestoreActivity(FileRestoreUpdateScriptActivity activity)
    {
        throw new NotImplementedException();
    }
}
