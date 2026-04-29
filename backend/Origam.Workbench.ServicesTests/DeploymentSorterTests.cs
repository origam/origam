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
using NUnit.Framework;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services;

namespace Origam.WorkbenchTests;

[TestFixture]
public class DeploymentSorterTests
{
    private readonly Guid package1Id = new Guid(g: "10000000-2867-4B99-8824-071FA8749EAD");
    private readonly Guid package2Id = new Guid(g: "20000000-6519-4393-B5D0-87931F9FD609");
    private readonly Guid package3Id = new Guid(g: "30000000-BFCC-45DB-940E-DE685AE821EF");

    [Test]
    public void ShouldOrderTwoUnrelatedPackagesByDependencies()
    {
        //  P1    p2
        //  1.0<--
        //   |    1.0
        //   |    1.1
        //   |    1.2
        //  1.1
        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );

        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package2Id
        );
        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.2"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package2Id
        );
        var deployment5 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package1Id
        );
        var unsortedDeployments = new List<IDeploymentVersion>
        {
            deployment5,
            deployment3,
            deployment1,
            deployment4,
            deployment2,
        };
        var deploymentSorter = new DeploymentSorter();
        int sortingFailedCalledTimes = 0;
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            sortingFailedCalledTimes++;
        };
        List<IDeploymentVersion> sortedDeployments = deploymentSorter.SortToRespectDependencies(
            deplVersionsToSort: unsortedDeployments
        );

        Assert.That(actual: sortingFailedCalledTimes, expression: Is.EqualTo(expected: 0));
        Assert.That(
            actual: sortedDeployments[index: 0],
            expression: Is.EqualTo(expected: deployment1)
        );
        Assert.That(
            actual: sortedDeployments[index: 1],
            expression: Is.EqualTo(expected: deployment2)
        );
        Assert.That(
            actual: sortedDeployments[index: 2],
            expression: Is.EqualTo(expected: deployment3)
        );
        Assert.That(
            actual: sortedDeployments[index: 3],
            expression: Is.EqualTo(expected: deployment4)
        );
        Assert.That(
            actual: sortedDeployments[index: 4],
            expression: Is.EqualTo(expected: deployment5)
        );
    }

    [Test]
    public void ShouldOrderTwoRelatedPackagesByDependencies()
    {
        //  P1    p2
        //  1.0<--
        //   |   1.0
        //   |   1.1
        //  1.1-->
        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );

        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package2Id
        );

        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package1Id
        );
        CheckTheOrderIsCorrect(
            deployment1: deployment1,
            deployment2: deployment2,
            deployment3: deployment3,
            deployment4: deployment4
        );
    }

    [Test]
    public void ShouldOrderThreeUnrelatedPackagesByDependencies()
    {
        //  P1    p2   p3
        //  1.0<--
        //   |   1.0<--
        //   |        1.0
        //  1.1
        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );

        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package3Id
        );

        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package1Id
        );
        CheckTheOrderIsCorrect(
            deployment1: deployment1,
            deployment2: deployment2,
            deployment3: deployment3,
            deployment4: deployment4
        );
    }

    [Test]
    public void ShouldOrderThreeRelatedPackagesByDependencies()
    {
        //  P1    p2   p3
        //  1.0<--   <--
        //   |--->1.0<--
        //   |------->1.0
        //  1.1
        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );

        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package3Id
        );

        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package3Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package1Id
        );
        CheckTheOrderIsCorrect(
            deployment1: deployment1,
            deployment2: deployment2,
            deployment3: deployment3,
            deployment4: deployment4
        );
    }

    [Test]
    public void ShouldNotRunDeploymentBeforeAllDeploymentsReferencingItsParentWereAlsoExecuted()
    {
        //   P1     p2    p3
        //               1.0
        //      <--------
        //   1.0<--1.0
        //         1.1<--1.1
        //   1.1
        //
        //  P1 1.1 must run last. If it does not, the other deployments will
        //  not be able to run because they depend on P1 1.0

        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );
        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );
        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );
        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package2Id
        );
        var deployment5 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package3Id
        );
        var deployment6 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package3Id
        );

        var unsortedDeployments = new List<IDeploymentVersion>
        {
            deployment4,
            deployment1,
            deployment6,
            deployment3,
            deployment2,
            deployment5,
        };

        var deploymentSorter = new DeploymentSorter();
        int sortingFailedCalledTimes = 0;
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            sortingFailedCalledTimes++;
        };
        List<IDeploymentVersion> sortedDeployments = deploymentSorter.SortToRespectDependencies(
            deplVersionsToSort: unsortedDeployments
        );

        Assert.That(actual: sortingFailedCalledTimes, expression: Is.EqualTo(expected: 0));
        Assert.That(
            actual: sortedDeployments.Last(),
            expression: Is.EqualTo(expected: deployment2)
        );
    }

    [Test]
    public void ShouldRecognizeDeadlock()
    {
        //   P1    p2   p3
        //              1.0
        //        1.0<--
        //  1.0<--1.1
        //  1.1<--------1.1
        //
        //  If all packages version 1.0 are deployed,
        //  no other deployment can run because:
        //  - P1 1.1 has to wait for P2 1.1 because it depends on P1 1.0
        //  - P2 1.1 has to wait for P3 1.1 because it depends on P2 1.0
        //  - P3 1.1 cannot run because P1 1.1 is not deployed yet
        // => Deployment dead lock

        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );
        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );
        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package2Id
        );
        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package2Id
        );
        var deployment5 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package3Id
        );
        var deployment6 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.0")
                ),
            },
            schemaExtensionId: package3Id
        );

        var unsortedDeployments = new List<IDeploymentVersion>
        {
            deployment4,
            deployment1,
            deployment6,
            deployment3,
            deployment2,
            deployment5,
        };
        var deploymentSorter = new DeploymentSorter();
        int sortingFailedCalledTimes = 0;
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            sortingFailedCalledTimes++;
        };
        List<IDeploymentVersion> sortedDeployments = deploymentSorter.SortToRespectDependencies(
            deplVersionsToSort: unsortedDeployments
        );

        Assert.That(actual: sortingFailedCalledTimes, expression: Is.EqualTo(expected: 1));
        Assert.That(actual: sortedDeployments, expression: Has.Count.EqualTo(expected: 0));
    }

    [Test]
    public void ShouldOrderTwoRelatedAndOneUnrelatedPackageByDependencies()
    {
        //   P1    p2   p3
        //  1.0
        //  1.1<--
        //        1.0
        //     -->1.1<--
        //  1.2<--------
        //             1.0
        // P1 and P2 depend on each other, P3 depends on p2 1.1 only

        var deployment1 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>(),
            schemaExtensionId: package1Id
        );

        var deployment2 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency> { },
            schemaExtensionId: package1Id
        );

        var deployment3 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment4 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.1"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package2Id
        );

        var deployment5 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.2"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package1Id
        );

        var deployment6 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package2Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.1")
                ),
            },
            schemaExtensionId: package3Id
        );

        var deployment7 = new MockDeploymentVersion(
            version: new PackageVersion(completeVersionString: "1.0"),
            deploymentDependencies: new List<DeploymentDependency>
            {
                new DeploymentDependency(
                    packageId: package1Id,
                    packageVersion: new PackageVersion(completeVersionString: "1.2")
                ),
            },
            schemaExtensionId: package3Id
        );

        var unsortedDeployments = new List<IDeploymentVersion>
        {
            deployment4,
            deployment7,
            deployment1,
            deployment6,
            deployment3,
            deployment2,
            deployment5,
        };
        var deploymentSorter = new DeploymentSorter();
        int sortingFailedCalledTimes = 0;
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            sortingFailedCalledTimes++;
        };
        List<IDeploymentVersion> sortedDeployments = deploymentSorter.SortToRespectDependencies(
            deplVersionsToSort: unsortedDeployments
        );

        Assert.That(actual: sortingFailedCalledTimes, expression: Is.EqualTo(expected: 0));
        Assert.That(
            actual: sortedDeployments[index: 0],
            expression: Is.EqualTo(expected: deployment1)
        );
        Assert.That(
            actual: sortedDeployments[index: 1],
            expression: Is.EqualTo(expected: deployment2)
        );
        Assert.That(
            actual: sortedDeployments[index: 2],
            expression: Is.EqualTo(expected: deployment3)
        );
        Assert.That(
            actual: sortedDeployments[index: 3],
            expression: Is.EqualTo(expected: deployment4)
        );
        Assert.That(
            condition: sortedDeployments[index: 4].Equals(obj: deployment5)
                || sortedDeployments[index: 4].Equals(obj: deployment6)
        );
        Assert.That(
            condition: sortedDeployments[index: 5].Equals(obj: deployment5)
                || sortedDeployments[index: 5].Equals(obj: deployment6)
        );
        Assert.That(
            actual: sortedDeployments[index: 6],
            expression: Is.EqualTo(expected: deployment7)
        );
    }

    private static void CheckTheOrderIsCorrect(
        MockDeploymentVersion deployment1,
        MockDeploymentVersion deployment2,
        MockDeploymentVersion deployment3,
        MockDeploymentVersion deployment4
    )
    {
        var unsortedDeployments = new List<IDeploymentVersion>
        {
            deployment3,
            deployment1,
            deployment4,
            deployment2,
        };
        var deploymentSorter = new DeploymentSorter();
        int sortingFailedCalledTimes = 0;
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            sortingFailedCalledTimes++;
        };
        List<IDeploymentVersion> sortedDeployments = deploymentSorter.SortToRespectDependencies(
            deplVersionsToSort: unsortedDeployments
        );

        Assert.That(actual: sortingFailedCalledTimes, expression: Is.EqualTo(expected: 0));
        Assert.That(
            actual: sortedDeployments[index: 0],
            expression: Is.EqualTo(expected: deployment1)
        );
        Assert.That(
            actual: sortedDeployments[index: 1],
            expression: Is.EqualTo(expected: deployment2)
        );
        Assert.That(
            actual: sortedDeployments[index: 2],
            expression: Is.EqualTo(expected: deployment3)
        );
        Assert.That(
            actual: sortedDeployments[index: 3],
            expression: Is.EqualTo(expected: deployment4)
        );
    }
}

class MockDeploymentVersion : IDeploymentVersion
{
    public PackageVersion Version { get; }
    public List<DeploymentDependency> DeploymentDependencies { get; set; }
    public bool HasDependencies => DeploymentDependencies.Count != 0;
    public Guid SchemaExtensionId { get; }
    public string PackageName { get; }

    public MockDeploymentVersion(
        PackageVersion version,
        List<DeploymentDependency> deploymentDependencies,
        Guid schemaExtensionId
    )
    {
        Version = version;
        DeploymentDependencies = deploymentDependencies;
        SchemaExtensionId = schemaExtensionId;
    }

    public int CompareTo(object obj)
    {
        if (obj is IDeploymentVersion otherDeployment)
        {
            return Version.CompareTo(other: otherDeployment.Version);
        }

        return -1;
    }

    public override string ToString() => SchemaExtensionId + " " + Version;
}
