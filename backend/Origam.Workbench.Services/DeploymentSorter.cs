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
using static MoreLinq.Extensions.ForEachExtension;
using Origam.Schema.DeploymentModel;

namespace Origam.Workbench.Services;
public class DeploymentSorter
{
    private readonly List<IDeploymentVersion> sortedDeployments = new();
    private List<IDeploymentVersion> remainingDeployments;
    private IDeploymentVersion current;
    private List<IDeploymentVersion> allDeployments;
    public event EventHandler<string> SortingFailed;
    public List<IDeploymentVersion> SortToRespectDependencies(
        IEnumerable<IDeploymentVersion> deplVersionsToSort)
    {
        allDeployments = deplVersionsToSort.ToList();
        remainingDeployments = allDeployments.ToList();
        int inputSize = remainingDeployments.Count;
        while (remainingDeployments.Count > 0)
        {
            int remainingDeploymentsBefore = remainingDeployments.Count;
            remainingDeployments
                .Where(x => !HasActiveDependencies(x) && FindPrerequisiteConflicts(x).Count == 0)
                .OrderBy(deployment => deployment)
                .ToList()
                .ForEach(ProcessDependent);
            int remainingDeploymentsAfter = remainingDeployments.Count;
            if (remainingDeployments.Count > 0 &&
                remainingDeployments.Count(x => !HasActiveDependencies(x)) == 0)
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
            throw new Exception(Strings.ErrorDeploymentSorterInputOutputCountMismatch);
        }
        return sortedDeployments;
    }
    private void HandleDeploymentDeadlock()
    {
        string sortedDeploymentsStr = string.Join("\r\n" ,
            sortedDeployments.Select(x => $"{x.PackageName} {x.Version}"));
        var deadlockedDeployments = remainingDeployments
            .GroupBy(x => x.SchemaExtensionId)
            .Select(group => 
                group.OrderBy(deploymentVersion => deploymentVersion.Version).First());
        var nextStepCandidates = deadlockedDeployments.Select(x =>
            string.Format(Strings.DeploymentDeadlockCandidateFormat,
                x.PackageName, x.Version,
                string.Join("\r\n\t\t", GetDependencyList(x))));
        var prerequisiteConflicts = remainingDeployments.SelectMany(FindPrerequisiteConflicts)
            .Where(blocker => blocker != null)
            .Distinct()
            .Where(blocker => !HasActiveDependencies(blocker.ConflictingDependency))
            .Select(blocker =>
                string.Format(Strings.DeploymentDeadlockBlockerFormat,
                    blocker.PackageName, blocker.Version,
                    string.Join("\r\n\t\t", blocker.DeployedDependency.PackageName + " " + blocker.DeployedDependency.Version)))
            .ToList();
        
        string message = string.Format(Strings.DeploymentDeadlockHeader,
            sortedDeploymentsStr,
            string.Join("\r\n", nextStepCandidates));

        if (prerequisiteConflicts.Count > 0)
        {
            message += string.Format(Strings.DeploymentDeadlockPrereqConflicts,
                string.Join("\r\n", prerequisiteConflicts));
        }

        message += "\r\n";
        message += Strings.DeploymentDeadlockHint;
        
        SortingFailed?.Invoke(this, message);
    }
    private IEnumerable<string> GetDependencyList(IDeploymentVersion deployment)
    {
        Dictionary<Guid,string> packageNameDictionary = allDeployments.
            GroupBy(x => x.SchemaExtensionId)
            .ToDictionary(
                group => group.Key,
                group => group.First().PackageName);
        var packageVersions = deployment.DeploymentDependencies
            .Select(dependency =>
            {
                packageNameDictionary.TryGetValue(dependency.PackageId, out string packageName);
                return $"{packageName ?? dependency.PackageId.ToString()} {dependency.PackageVersion}";
            }).ToList();
        var previousDeployment = sortedDeployments
            .LastOrDefault(x => x.SchemaExtensionId == deployment.SchemaExtensionId);
        
        if (previousDeployment != null)
        {
            packageVersions.Add($"{previousDeployment.PackageName} {previousDeployment.Version}");
        }
        return packageVersions;
    }
    private void HandleInfiniteLoopError()
    {
        foreach (var deploymentVersion in remainingDeployments)
        {
            var dependsOnItSelf = deploymentVersion.DeploymentDependencies
                .Any(dependency =>
                    dependency.PackageId == deploymentVersion.SchemaExtensionId &&
                    dependency.PackageVersion == deploymentVersion.Version);
            if (dependsOnItSelf)
            {
                throw new Exception(
                    string.Format(Strings.ErrorDeploymentDependsOnItself,
                        deploymentVersion.Version,
                        deploymentVersion.PackageName));
            }
        }
        SortingFailed?.Invoke(this, Strings.ErrorInfiniteLoopNoDependencies);
    }
    private void ProcessDependent(IDeploymentVersion deployment)
    {
        MoveToSorted(deployment);
        GetDependentDeployments(deployment)
            .Where(x => !HasActiveDependencies(x) && FindPrerequisiteConflicts(x).Count == 0)
            .OrderBy(x => x, new OtherPackagesFirst(current.SchemaExtensionId))
            .ForEach(ProcessDependent);
    }
    private void MoveToSorted(IDeploymentVersion deployment)
    {
        if (sortedDeployments.Contains(deployment)) return;
        current = deployment;
        sortedDeployments.Add(deployment);
        remainingDeployments.Remove(deployment);
    }
    private List<IDeploymentVersion> GetDependentDeployments(IDeploymentVersion deployment)
    {
        return remainingDeployments
            .Where(remainingDepl => IsAmongDependencies(GetAllDependencies(remainingDepl), deployment))
            .ToList();
    }
    private bool IsAmongDependencies(IEnumerable<DeploymentDependency> dependencies, IDeploymentVersion deployment)
    {
        return dependencies.Any(
            dependency =>
                dependency.PackageId == deployment.SchemaExtensionId &&
                dependency.PackageVersion == deployment.Version);
    }
    private bool HasActiveDependencies(IDeploymentVersion deployment)
    {
        return GetAllDependencies(deployment)
            .Any(IsInRemainingDeployments);
    }
    
    private List<PrerequisiteConflict> FindPrerequisiteConflicts(IDeploymentVersion deployment)
    {
        var previousDeployment = sortedDeployments
            .LastOrDefault(x => x.SchemaExtensionId == deployment.SchemaExtensionId);
        if (previousDeployment == null)
        {
            return [];
        }
        IEnumerable<IDeploymentVersion> haveToRunBefore =  remainingDeployments
            .Where(x => x != deployment)
            .Where(x => IsAmongDependencies(x.DeploymentDependencies, previousDeployment));
        return haveToRunBefore
            .Select(blockingDeployment => new PrerequisiteConflict(blockingDeployment, previousDeployment, deployment))
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
    record PrerequisiteConflict(IDeploymentVersion Deployment, IDeploymentVersion DeployedDependency, IDeploymentVersion ConflictingDependency)
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
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(PackageName, other.PackageName) 
                   && Equals(Version, other.Version) 
                   && Equals(dependencyVersion, other.dependencyVersion)
                   && Equals(DeployedDependency.PackageName, other.DeployedDependency.PackageName);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PackageName, Version, dependencyVersion, DeployedDependency.PackageName);
        }
    }

    private IEnumerable<DeploymentDependency> GetAllDependencies(IDeploymentVersion deployment)
    {
        bool alreadyDependsOnPreviousVersion =
            deployment.DeploymentDependencies
                .Any(x => x.PackageId == deployment.SchemaExtensionId);
        if (alreadyDependsOnPreviousVersion)
        {
            return deployment.DeploymentDependencies;
        }
        var dependencies = deployment.DeploymentDependencies.ToList();
        var mayBePreviousVersionDependency = GetDependencyOnPreviousVersion(deployment);
        if (mayBePreviousVersionDependency.HasValue)
        {
            dependencies.Add(mayBePreviousVersionDependency.Value);
        }
        return dependencies;
    }
    private Maybe<DeploymentDependency> GetDependencyOnPreviousVersion(IDeploymentVersion deployment)
    {
       return allDeployments
            .Where(x => x.SchemaExtensionId == deployment.SchemaExtensionId)
            .Where(x => x.Version < deployment.Version)
            .OrderByDescending(x => x.Version)
            .Take(1)
            .Select(x => new DeploymentDependency(deployment.SchemaExtensionId, x.Version))
            .FirstOrDefault();
    }
    private bool IsInRemainingDeployments(DeploymentDependency dependency)
    {
        return remainingDeployments.Any(deployment =>    
            deployment.Version == dependency.PackageVersion &&
            deployment.SchemaExtensionId == dependency.PackageId);
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
        if (x.SchemaExtensionId == y.SchemaExtensionId) return 0;
        if (x.SchemaExtensionId == thisPackageId) return 1;
        if (y.SchemaExtensionId == thisPackageId) return -1;
        return 0;
    }
}
