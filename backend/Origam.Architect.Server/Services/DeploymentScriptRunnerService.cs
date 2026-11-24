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
    public string RunDeploymentScript(AbstractUpdateScriptActivity script)
    {
        var transactionId = Guid.NewGuid().ToString();

        try
        {
            logger.LogInformation(
                message: "Executing deployment script: {ScriptName}",
                script.Name
            );
            string output = ExecuteActivity(script, transactionId);
            ResourceMonitor.Commit(transactionId);
            logger.LogInformation(
                message: "Deployment script executed successfully: {ScriptName}",
                script.Name
            );
            return output;
        }
        catch (UserOrigamException)
        {
            ResourceMonitor.Rollback(transactionId);
            throw;
        }
        catch (Exception ex)
        {
            ResourceMonitor.Rollback(transactionId);
            logger.LogError(
                ex,
                message: "Error executing deployment script: {ScriptName}",
                script.Name
            );
            throw new UserOrigamException(
                string.Format(Strings.DeploymentScript_ExecutionFailed, script.Name, ex.Message)
            );
        }
    }

    private string ExecuteActivity(AbstractUpdateScriptActivity activity, string transactionId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                message: "Executing deployment activity: {ActivityName}",
                activity.Name
            );
        }

        try
        {
            if (activity is ServiceCommandUpdateScriptActivity serviceActivity)
            {
                return ExecuteServiceCommandActivity(serviceActivity, transactionId);
            }
            else if (activity is FileRestoreUpdateScriptActivity fileActivity)
            {
                return ExecuteFileRestoreActivity(fileActivity);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activity),
                    activity,
                    Strings.DeploymentScript_UnsupportedDeploymentActivityType
                );
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                logger.LogCritical(
                    ex,
                    message: "Error occurred while running deployment activity {ActivityPath}",
                    activity.Path
                );
            }
            throw;
        }
    }

    private string ExecuteServiceCommandActivity(
        ServiceCommandUpdateScriptActivity activity,
        string transactionId
    )
    {
        var service = ServiceManager.Services.GetService<IBusinessServicesService>();
        if (service == null)
        {
            throw new InvalidOperationException(
                Strings.DeploymentScript_BusinessServicesNotAvailable
            );
        }

        IServiceAgent agent = service.GetAgent(
            activity.Service.Name,
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
            var executed = false;
            settings.DeployPlatforms?.ForEach(platform =>
            {
                var databaseType = (DatabaseType)
                    Enum.Parse(typeof(DatabaseType), platform.GetParseEnum(platform.DataService));
                if (databaseType == (DatabaseType)activity.DatabaseType)
                {
                    agent.SetDataService(DataServiceFactory.GetDataService(platform));
                    result = agent.ExecuteUpdate(activity.CommandText, transactionId);
                    executed = true;
                }
            });
            if (!executed)
            {
                throw new UserOrigamException(
                    string.Format(
                        Strings.DeploymentScript_PlatformNotConfigured,
                        activity.Name,
                        activity.DatabaseType
                    )
                );
            }
        }
        else
        {
            result = agent.ExecuteUpdate(activity.CommandText, transactionId);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(result);
        }
        return result;
    }

    private string ExecuteFileRestoreActivity(FileRestoreUpdateScriptActivity activity)
    {
        throw new NotImplementedException();
    }
}
