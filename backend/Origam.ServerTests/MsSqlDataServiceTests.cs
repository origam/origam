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

using System.Data;
using Moq;
using NUnit.Framework;
using Origam.DA.Service;

namespace Origam.ServerTests;

// This test class would be better placed at Origam.DA.ServiceTests. But the Origam.DA.ServiceTests
// project runs on net472 and is therefore not executed as a part of the server tests.
// That is why the class is here we should move it once we get rid of net472.

[TestFixture]
public class MsSqlDataServiceTests
{
    [Test]
    public void SqlServerExplainsDisabledSnapshotIsolation()
    {
        var command = new Mock<IDbCommand>();
        command.Setup(x => x.ExecuteScalar()).Returns(0); // Database reports snapshot isolation disabled.
        var connection = new Mock<IDbConnection>();
        connection.Setup(x => x.CreateCommand()).Returns(command.Object);
        connection.SetupGet(x => x.Database).Returns("InventoryDb");

        var dataService = new TestMsSqlDataService();
        var exception = Assert.Throws<Exception>(() =>
            dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot)
        );

        Assert.That(exception.Message, Does.Contain("InventoryDb"));
        Assert.That(exception.Message, Does.Contain("ALLOW_SNAPSHOT_ISOLATION"));
    }

    [Test]
    public void SqlServerAcceptsEnabledSnapshotIsolation()
    {
        var command = new Mock<IDbCommand>();
        command.Setup(x => x.ExecuteScalar()).Returns(1); // Database reports snapshot isolation enabled.
        var connection = new Mock<IDbConnection>();
        connection.Setup(x => x.CreateCommand()).Returns(command.Object);

        var dataService = new TestMsSqlDataService();
        Assert.DoesNotThrow(() =>
            dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot)
        );
    }

    [Test]
    public void SqlServerChecksSnapshotIsolationOnlyOncePerDatabase()
    {
        var command = new Mock<IDbCommand>();
        command.Setup(x => x.ExecuteScalar()).Returns(1); // Database reports snapshot isolation enabled.
        var connection = new Mock<IDbConnection>();
        connection.Setup(x => x.CreateCommand()).Returns(command.Object);
        connection.SetupGet(x => x.Database).Returns("InventoryDb");

        var dataService = new TestMsSqlDataService();
        dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot);
        dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot);

        command.Verify(x => x.ExecuteScalar(), Times.Once);
    }

    [Test]
    public void SqlServerChecksSnapshotIsolationForEachServiceInstance()
    {
        var command = new Mock<IDbCommand>();
        command.Setup(x => x.ExecuteScalar()).Returns(1); // Database reports snapshot isolation enabled.
        var connection = new Mock<IDbConnection>();
        connection.Setup(x => x.CreateCommand()).Returns(command.Object);

        var firstDataService = new TestMsSqlDataService();
        var secondDataService = new TestMsSqlDataService();
        firstDataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot);
        secondDataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot);

        command.Verify(x => x.ExecuteScalar(), Times.Exactly(2));
    }

    [Test]
    public void SqlServerRetriesFailedSnapshotIsolationCheck()
    {
        var command = new Mock<IDbCommand>();
        command.SetupSequence(x => x.ExecuteScalar()).Returns(0).Returns(1); // Database reports disabled, then enabled on retry.
        var connection = new Mock<IDbConnection>();
        connection.Setup(x => x.CreateCommand()).Returns(command.Object);
        connection.SetupGet(x => x.Database).Returns("InventoryDb");

        var dataService = new TestMsSqlDataService();
        Assert.Throws<Exception>(() =>
            dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot)
        );
        Assert.DoesNotThrow(() =>
            dataService.CheckIsolationLevel(connection.Object, IsolationLevel.Snapshot)
        );

        command.Verify(x => x.ExecuteScalar(), Times.Exactly(2));
    }

    private class TestMsSqlDataService : MsSqlDataService
    {
        public void CheckIsolationLevel(IDbConnection connection, IsolationLevel isolationLevel)
        {
            ValidateIsolationLevel(connection, isolationLevel);
        }
    }
}
