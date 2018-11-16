
using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.Schema;
using Origam.Schema.DeploymentModel;

namespace Origam.Workbench.Services
{
    public class DeploymentSorter
    {
        private readonly List<IDeploymentVersion> sortedDeployments =
            new List<IDeploymentVersion>();

        private List<IDeploymentVersion> remainingDeployments;
        private IDeploymentVersion current;

        public List<IDeploymentVersion> SortToRespectDependencies(
            IEnumerable<IDeploymentVersion> deplVersionsToSort)
        {
            remainingDeployments =
                new List<IDeploymentVersion>(deplVersionsToSort);
            int inputSize = remainingDeployments.Count;

            while (remainingDeployments.Count > 0)
            {
                remainingDeployments
                    .Where(x => !HasActiveDependencies(x))
                    .OrderBy(deployment => deployment)
                    .ToList()
                    .ForEach(ProcessDependent);
                if (remainingDeployments.Count > 0 &&
                    remainingDeployments.Count(x=>!HasActiveDependencies(x)) == 0)
                {
                    HandleInfiniteLoopError();
                }
            }

            if (inputSize != sortedDeployments.Count)
            {
                throw new Exception("Number of input and output deployments doesn't match");
            }
            return sortedDeployments;
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
                    throw new Exception("Deployment version: " + deploymentVersion.Version + " of package: " +
                                        deploymentVersion.Package + " depends on it self! Remove the dependency to continue.");
                }
            }

            throw new Exception("Infinite loop! Could not find any deployments without active dependencies.");
        }

        private void ProcessDependent(IDeploymentVersion deployment)
        {
            MoveToSorted(deployment);
 
            GetDependentDeployments(deployment)
                .Where(x=>!HasActiveDependencies(x))
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
                .Where(remainingDepl => IsAmongDependencies(remainingDepl.DeploymentDependencies, deployment))
                .ToList();
        }

        private bool IsAmongDependencies(List<DeploymentDependency> dependencies, IDeploymentVersion deployment)
        {
            return dependencies.Any(
                dependency =>
                    dependency.PackageId == deployment.SchemaExtensionId &&
                    dependency.PackageVersion == deployment.Version);
        }

        private bool HasActiveDependencies(IDeploymentVersion deployment)
        {
            return deployment.DeploymentDependencies
                       .Any(IsInRemainingDeployments);
        }

        private bool IsInRemainingDeployments(DeploymentDependency dependency)
        {
            bool isInRemaining = remainingDeployments.Any(deployment =>    
                deployment.Version == dependency.PackageVersion &&
                deployment.SchemaExtensionId == dependency.PackageId);
            return isInRemaining;
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
}