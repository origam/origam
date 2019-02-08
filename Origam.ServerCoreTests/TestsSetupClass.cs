using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Origam.ServerCore.Controllers;

namespace Origam.ServerCoreTests
{
    [SetUpFixture]
    public class TestsSetupClass
    {

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            var configuration = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
            RestoreDatabase(
                databaseName: configuration.TestDbName,
                backUpFile: configuration.PathToBakFile, 
                serverName: configuration.ServerName, 
                userName: configuration.UserName, 
                password: configuration.Password);
            OrigamEngine.OrigamEngine.ConnectRuntime();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }

        private void RestoreDatabase(String databaseName, String backUpFile, String serverName, String userName, String password)
        {
            ServerConnection connection = new ServerConnection(serverName, userName, password);
            Microsoft.SqlServer.Management.Smo.Server sqlServer = new Microsoft.SqlServer.Management.Smo.Server(connection);
            Restore rstDatabase = new Restore();
            rstDatabase.Action = RestoreActionType.Database;
            rstDatabase.Database = databaseName;
            BackupDeviceItem bkpDevice = new BackupDeviceItem(backUpFile, DeviceType.File);
            rstDatabase.Devices.Add(bkpDevice);
            rstDatabase.ReplaceDatabase = true;
            rstDatabase.SqlRestore(sqlServer);
        }
    }
}