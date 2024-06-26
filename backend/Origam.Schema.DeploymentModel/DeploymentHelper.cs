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

using Origam.Services;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using Origam.DA.Service;
using static Origam.DA.Common.Enums;

namespace Origam.Schema.DeploymentModel;
public class DeploymentHelper
{
	public static DeploymentVersion CreateVersion
        (SchemaItemGroup group, string name, string version)
	{
		var activePackage = ServiceManager.Services
			.GetService<ISchemaService>()
			.ActiveExtension;
		var deploymentVersion = group.NewItem<DeploymentVersion>( 
			activePackage.Id, null);
		deploymentVersion.Name = name;
		deploymentVersion.VersionString = version;
		deploymentVersion.Persist();
		return deploymentVersion;
	}
	
	public static ServiceCommandUpdateScriptActivity
        CreateDatabaseScript(
            string name, string script, DatabaseType platformName)
	{
		var schemaService
			= ServiceManager.Services.GetService<ISchemaService>();
        var serviceSchemaItemProvider 
            = schemaService.GetProvider<ServiceSchemaItemProvider>();
        var dataService = serviceSchemaItemProvider.GetChildByName(
            "DataService", 
            Origam.Schema.WorkflowModel.Service.CategoryConst) 
            as Origam.Schema.WorkflowModel.Service;
        var deploymentSchemaItemProvider 
            = schemaService.GetProvider<DeploymentSchemaItemProvider>();
        var currentVersion = deploymentSchemaItemProvider.CurrentVersion();
        var newActivity = currentVersion
            .NewItem<ServiceCommandUpdateScriptActivity>(
	            schemaService.ActiveSchemaExtensionId, null);
        newActivity.Name += name;
        newActivity.DatabaseType = platformName;
        newActivity.CommandText = script;
        newActivity.Service = dataService;
        newActivity.Persist();
        return newActivity;
    }
    public static ServiceCommandUpdateScriptActivity CreateSystemRole(
        string roleName, AbstractSqlDataService abstractSqlData)
    {
        var script = abstractSqlData.CreateSystemRole(roleName);
        var activity = CreateDatabaseScript(
            $"AddRole_{roleName}", script,abstractSqlData.PlatformName);
        return activity;
    }
}
