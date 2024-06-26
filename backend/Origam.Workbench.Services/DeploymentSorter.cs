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
using MoreLinq;
using Origam.Schema;
using Origam.Schema.DeploymentModel;

namespace Origam.Workbench.Services;
public class DeploymentSorter
{
    private readonly List<IDeploymentVersion> sortedDeployments =
        new List<IDeploymentVersion>();
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
                .Where(x => !HasActiveDependencies(x) && !SomeDeploymentsHaveToRunBefore(x))
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
            throw new Exception("Number of input and output deployments doesn't match");
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
            $"Candidate: {x.PackageName} {x.Version}\r\n" +
                $"\tDependencies:\r\n" +
                    $"\t\t{string.Join("\r\n\t\t", GetDependencyList(x))}");
        string message = 
            "Deployment version order could not be determined, because circular\r\n" +
            "dependencies were detected among some deployment versions.\r\n"+
            $"Successfully ordered deployment versions:\r\n" +
            $"{sortedDeploymentsStr}\r\n"+
            "The sorting process failed with these deployment versions as the next step candidates:\r\n"+
            $"{string.Join("\r\n", nextStepCandidates)}\r\n";
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
                    $"Deployment version: {deploymentVersion.Version}" +
                    $" of package: {deploymentVersion.PackageName} depends " +
                    "on itself! Remove the dependency to continue.");
            }
        }
        SortingFailed?.Invoke(this, "Infinite loop! Could not find any deployments without active dependencies.");
    }
    private void ProcessDependent(IDeploymentVersion deployment)
    {
        MoveToSorted(deployment);
        GetDependentDeployments(deployment)
            .Where(x => !HasActiveDependencies(x) && !SomeDeploymentsHaveToRunBefore(x))
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
    
    private bool SomeDeploymentsHaveToRunBefore(IDeploymentVersion deployment)
    {
        var previousDeployment = sortedDeployments
            .LastOrDefault(x => x.SchemaExtensionId == deployment.SchemaExtensionId);
        if (previousDeployment == null)
        {
            return false;
        }
        return remainingDeployments
            .Where(x => x != deployment)
            .Any(x => IsAmongDependencies(x.DeploymentDependencies, previousDeployment));
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
