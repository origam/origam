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
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Origam.Schema.DeploymentModel;
using static MoreLinq.Extensions.ForEachExtension;

namespace Origam.Workbench.Services;

public class DeploymentSorter
{
    private readonly List<IDeploymentVersion> sortedDeployments = new();
    private List<IDeploymentVersion> remainingDeployments;
    private IDeploymentVersion current;
    private List<IDeploymentVersion> allDeployments;
    public event EventHandler<string> SortingFailed;

    public List<IDeploymentVersion> SortToRespectDependencies(
        IEnumerable<IDeploymentVersion> deplVersionsToSort
    )
    {
        allDeployments = deplVersionsToSort.ToList();
        remainingDeployments = allDeployments.ToList();
        int inputSize = remainingDeployments.Count;
        while (remainingDeployments.Count > 0)
        {
            int remainingDeploymentsBefore = remainingDeployments.Count;
            remainingDeployments
                .Where(predicate: x =>
                    !HasActiveDependencies(deployment: x)
                    && FindPrerequisiteConflicts(deployment: x).Count == 0
                )
                .OrderBy(keySelector: deployment => deployment)
                .ToList()
                .ForEach(action: ProcessDependent);
            int remainingDeploymentsAfter = remainingDeployments.Count;
            if (
                remainingDeployments.Count > 0
                && remainingDeployments.Count(predicate: x => !HasActiveDependencies(deployment: x))
                    == 0
            )
            {
                HandleInfiniteLoopError();
                return new List<IDeploymentVersion>();
            }
            if (remainingDeploymentsAfter == remainingDeploymentsBefore)
            {
                HandleDeploymentDeadlock();
                return new List<IDeploymentVersion>();
            }
        }
        if (inputSize != sortedDeployments.Count)
        {
            throw new Exception(message: Strings.ErrorDeploymentSorterInputOutputCountMismatch);
        }
        return sortedDeployments;
    }

    private void HandleDeploymentDeadlock()
    {
        string sortedDeploymentsStr = string.Join(
            separator: "\r\n",
            values: sortedDeployments.Select(selector: x => $"{x.PackageName} {x.Version}")
        );
        var deadlockedDeployments = remainingDeployments
            .GroupBy(keySelector: x => x.SchemaExtensionId)
            .Select(selector: group =>
                group.OrderBy(keySelector: deploymentVersion => deploymentVersion.Version).First()
            );
        var nextStepCandidates = deadlockedDeployments.Select(selector: x =>
            string.Format(
                format: Strings.DeploymentDeadlockCandidateFormat,
                arg0: x.PackageName,
                arg1: x.Version,
                arg2: string.Join(separator: "\r\n\t\t", values: GetDependencyList(deployment: x))
            )
        );
        var prerequisiteConflicts = remainingDeployments
            .SelectMany(selector: FindPrerequisiteConflicts)
            .Where(predicate: blocker => blocker != null)
            .Distinct()
            .Where(predicate: blocker =>
                !HasActiveDependencies(deployment: blocker.ConflictingDependency)
            )
            .Select(selector: blocker =>
                string.Format(
                    format: Strings.DeploymentDeadlockBlockerFormat,
                    arg0: blocker.PackageName,
                    arg1: blocker.Version,
                    arg2: string.Join(
                        separator: "\r\n\t\t",
                        value: blocker.DeployedDependency.PackageName
                            + " "
                            + blocker.DeployedDependency.Version
                    )
                )
            )
            .ToList();

        string message = string.Format(
            format: Strings.DeploymentDeadlockHeader,
            arg0: sortedDeploymentsStr,
            arg1: string.Join(separator: "\r\n", values: nextStepCandidates)
        );

        if (prerequisiteConflicts.Count > 0)
        {
            message += string.Format(
                format: Strings.DeploymentDeadlockPrereqConflicts,
                arg0: string.Join(separator: "\r\n", values: prerequisiteConflicts)
            );
        }

        message += "\r\n";
        message += Strings.DeploymentDeadlockHint;

        SortingFailed?.Invoke(sender: this, e: message);
    }

    private IEnumerable<string> GetDependencyList(IDeploymentVersion deployment)
    {
        Dictionary<Guid, string> packageNameDictionary = allDeployments
            .GroupBy(keySelector: x => x.SchemaExtensionId)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.First().PackageName
            );
        var packageVersions = deployment
            .DeploymentDependencies.Select(selector: dependency =>
            {
                packageNameDictionary.TryGetValue(
                    key: dependency.PackageId,
                    value: out string packageName
                );
                return $"{packageName ?? dependency.PackageId.ToString()} {dependency.PackageVersion}";
            })
            .ToList();
        var previousDeployment = sortedDeployments.LastOrDefault(predicate: x =>
            x.SchemaExtensionId == deployment.SchemaExtensionId
        );

        if (previousDeployment != null)
        {
            packageVersions.Add(
                item: $"{previousDeployment.PackageName} {previousDeployment.Version}"
            );
        }
        return packageVersions;
    }

    private void HandleInfiniteLoopError()
    {
        foreach (var deploymentVersion in remainingDeployments)
        {
            var dependsOnItSelf = deploymentVersion.DeploymentDependencies.Any(
                predicate: dependency =>
                    dependency.PackageId == deploymentVersion.SchemaExtensionId
                    && dependency.PackageVersion == deploymentVersion.Version
            );
            if (dependsOnItSelf)
            {
                throw new Exception(
                    message: string.Format(
                        format: Strings.ErrorDeploymentDependsOnItself,
                        arg0: deploymentVersion.Version,
                        arg1: deploymentVersion.PackageName
                    )
                );
            }
        }
        SortingFailed?.Invoke(sender: this, e: Strings.ErrorInfiniteLoopNoDependencies);
    }

    private void ProcessDependent(IDeploymentVersion deployment)
    {
        MoveToSorted(deployment: deployment);
        GetDependentDeployments(deployment: deployment)
            .Where(predicate: x =>
                !HasActiveDependencies(deployment: x)
                && FindPrerequisiteConflicts(deployment: x).Count == 0
            )
            .OrderBy(
                keySelector: x => x,
                comparer: new OtherPackagesFirst(thisPackageId: current.SchemaExtensionId)
            )
            .ForEach(action: ProcessDependent);
    }

    private void MoveToSorted(IDeploymentVersion deployment)
    {
        if (sortedDeployments.Contains(item: deployment))
        {
            return;
        }

        current = deployment;
        sortedDeployments.Add(item: deployment);
        remainingDeployments.Remove(item: deployment);
    }

    private List<IDeploymentVersion> GetDependentDeployments(IDeploymentVersion deployment)
    {
        return remainingDeployments
            .Where(predicate: remainingDepl =>
                IsAmongDependencies(
                    dependencies: GetAllDependencies(deployment: remainingDepl),
                    deployment: deployment
                )
            )
            .ToList();
    }

    private bool IsAmongDependencies(
        IEnumerable<DeploymentDependency> dependencies,
        IDeploymentVersion deployment
    )
    {
        return dependencies.Any(predicate: dependency =>
            dependency.PackageId == deployment.SchemaExtensionId
            && dependency.PackageVersion == deployment.Version
        );
    }

    private bool HasActiveDependencies(IDeploymentVersion deployment)
    {
        return GetAllDependencies(deployment: deployment).Any(predicate: IsInRemainingDeployments);
    }

    private List<PrerequisiteConflict> FindPrerequisiteConflicts(IDeploymentVersion deployment)
    {
        var previousDeployment = sortedDeployments.LastOrDefault(predicate: x =>
            x.SchemaExtensionId == deployment.SchemaExtensionId
        );
        if (previousDeployment == null)
        {
            return [];
        }
        IEnumerable<IDeploymentVersion> haveToRunBefore = remainingDeployments
            .Where(predicate: x => x != deployment)
            .Where(predicate: x =>
                IsAmongDependencies(
                    dependencies: x.DeploymentDependencies,
                    deployment: previousDeployment
                )
            );
        return haveToRunBefore
            .Select(selector: blockingDeployment => new PrerequisiteConflict(
                Deployment: blockingDeployment,
                DeployedDependency: previousDeployment,
                ConflictingDependency: deployment
            ))
            .ToList();
    }

    // Represents a conflict where a deployment cannot proceed because another deployment
    // still depends on a previous version of the same package.
    //
    // Example scenario:
    //   - "Deployment" (Workflow 1.1.0) depends on "DeployedDependency"
    //     (Root 5.3.1) that has already been deployed (is in sortedDeployments).
    //   - "Deployment" (Workflow 1.1.0) also depends on other deployments (for example Security 5.4.1)
    //     which in turn depend on future version of the "DeployedDependency" (anything higher than Root 5.3.1)
    //   - This creates a prerequisite conflict "Deployment" (Workflow 1.1.0) needs DeployedDependency (Root 5.3.1)
    //     and at the same time some of its other dependencies require the same deployment in a higher
    //     version "ConflictingDependency" (Root 5.3.2)
    //   - This creates a prerequisite conflict:
    //       • "Deployment" (Workflow 1.1.0) insists on Root 5.3.1
    //       • Its other dependencies insist on Root ≥ 5.3.2
    record PrerequisiteConflict(
        IDeploymentVersion Deployment,
        IDeploymentVersion DeployedDependency,
        IDeploymentVersion ConflictingDependency
    )
    {
        public string PackageName { get; } = Deployment.PackageName;
        public string Version { get; } = Deployment.Version;

        private readonly string dependencyVersion = DeployedDependency.Version;

        public virtual bool Equals(PrerequisiteConflict other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(objA: this, objB: other))
            {
                return true;
            }
            return Equals(objA: PackageName, objB: other.PackageName)
                && Equals(objA: Version, objB: other.Version)
                && Equals(objA: dependencyVersion, objB: other.dependencyVersion)
                && Equals(
                    objA: DeployedDependency.PackageName,
                    objB: other.DeployedDependency.PackageName
                );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                value1: PackageName,
                value2: Version,
                value3: dependencyVersion,
                value4: DeployedDependency.PackageName
            );
        }
    }

    private IEnumerable<DeploymentDependency> GetAllDependencies(IDeploymentVersion deployment)
    {
        bool alreadyDependsOnPreviousVersion = deployment.DeploymentDependencies.Any(predicate: x =>
            x.PackageId == deployment.SchemaExtensionId
        );
        if (alreadyDependsOnPreviousVersion)
        {
            return deployment.DeploymentDependencies;
        }
        var dependencies = deployment.DeploymentDependencies.ToList();
        var mayBePreviousVersionDependency = GetDependencyOnPreviousVersion(deployment: deployment);
        if (mayBePreviousVersionDependency.HasValue)
        {
            dependencies.Add(item: mayBePreviousVersionDependency.Value);
        }
        return dependencies;
    }

    private Maybe<DeploymentDependency> GetDependencyOnPreviousVersion(
        IDeploymentVersion deployment
    )
    {
        return allDeployments
            .Where(predicate: x => x.SchemaExtensionId == deployment.SchemaExtensionId)
            .Where(predicate: x => x.Version < deployment.Version)
            .OrderByDescending(keySelector: x => x.Version)
            .Take(count: 1)
            .Select(selector: x => new DeploymentDependency(
                packageId: deployment.SchemaExtensionId,
                packageVersion: x.Version
            ))
            .FirstOrDefault();
    }

    private bool IsInRemainingDeployments(DeploymentDependency dependency)
    {
        return remainingDeployments.Any(predicate: deployment =>
            deployment.Version == dependency.PackageVersion
            && deployment.SchemaExtensionId == dependency.PackageId
        );
    }
}

class OtherPackagesFirst : Comparer<IDeploymentVersion>
{
    private readonly Guid thisPackageId;

    public OtherPackagesFirst(Guid thisPackageId)
    {
        this.thisPackageId = thisPackageId;
    }

    public override int Compare(IDeploymentVersion x, IDeploymentVersion y)
    {
        if (x.SchemaExtensionId == y.SchemaExtensionId)
        {
            return 0;
        }

        if (x.SchemaExtensionId == thisPackageId)
        {
            return 1;
        }

        if (y.SchemaExtensionId == thisPackageId)
        {
            return -1;
        }

        return 0;
    }
}
