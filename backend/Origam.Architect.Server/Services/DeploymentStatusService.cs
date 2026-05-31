#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Enums;
using Origam.Architect.Server.Models.Responses.DeploymentScripts;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class DeploymentStatusService(
    IDeploymentService deploymentService,
    SchemaService schemaService
)
{
    public StatusResponseModel BuildStatus()
    {
        Package activeExtension = schemaService.ActiveExtension;
        var packages = new List<Package>(activeExtension.IncludedPackages) { activeExtension }
            .DistinctBy(package => package.Id)
            .ToList();

        var deploymentVersionsByPackage = schemaService
            .GetProvider<DeploymentSchemaItemProvider>()
            .ChildItemsByType<DeploymentVersion>(DeploymentVersion.CategoryConst)
            .GroupBy(deplVersion => deplVersion.SchemaExtensionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var response = new StatusResponseModel();
        foreach (Package package in packages)
        {
            response.Packages.Add(BuildPackageStatus(package, deploymentVersionsByPackage));
        }
        return response;
    }

    private PackageStatusDto BuildPackageStatus(
        Package package,
        IReadOnlyDictionary<Guid, List<DeploymentVersion>> deploymentVersionsByPackage
    )
    {
        PackageVersion deployedVersion = deploymentService.CurrentDeployedVersion(package);
        var packageDto = new PackageStatusDto
        {
            PackageId = package.Id,
            PackageName = package.Name,
            PackageModelVersion = package.VersionString,
            DeployedVersion = deployedVersion.ToString(),
        };

        if (!deploymentVersionsByPackage.TryGetValue(package.Id, out var deploymentVersions))
        {
            return packageDto;
        }

        foreach (var deploymentVersion in deploymentVersions.OrderBy(v => v.Version))
        {
            var status =
                deploymentVersion.Version <= deployedVersion
                    ? DeploymentActivityStatus.Done
                    : DeploymentActivityStatus.Pending;

            var versionDto = new DeploymentVersionStatusDto
            {
                Id = deploymentVersion.Id,
                Name = deploymentVersion.Name,
                Version = deploymentVersion.VersionString,
                Status = status,
                IsCurrentVersion = deploymentVersion.IsCurrentVersion,
            };

            foreach (
                AbstractUpdateScriptActivity activity in deploymentVersion.UpdateScriptActivities
            )
            {
                versionDto.Activities.Add(
                    new ActivityStatusDto
                    {
                        Id = activity.Id,
                        Name = activity.Name,
                        ActivityType = ResolveActivityType(activity),
                        ActivityOrder = activity.ActivityOrder,
                        Status = status,
                    }
                );
            }

            packageDto.Versions.Add(versionDto);
        }

        return packageDto;
    }

    private static string ResolveActivityType(AbstractUpdateScriptActivity activity)
    {
        return activity switch
        {
            ServiceCommandUpdateScriptActivity => "ServiceCommand",
            FileRestoreUpdateScriptActivity => "FileRestore",
            _ => activity.GetType().Name,
        };
    }
}
