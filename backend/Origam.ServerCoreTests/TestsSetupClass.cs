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

using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Origam.ServerCoreTests;
[SetUpFixture]
public class TestsSetupClass
{
    private ServerCoreTestConfiguration configuration;
    public TestsSetupClass()
    {
        configuration = ConfigHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
    }
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        RestoreDatabase(
            databaseName: configuration.TestDbName,
            backUpFile: configuration.PathToBakFile, 
            serverName: configuration.ServerName, 
            userName: configuration.UserName, 
            password: configuration.Password);
        UpdateTestUserName();
        OrigamEngine.OrigamEngine.ConnectRuntime();
    }
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
    }
    private void UpdateTestUserName()
    {
        SqlConnection conn = new SqlConnection($"Server={configuration.ServerName};Initial Catalog ={configuration.TestDbName}; Integrated Security = True; User ID =; Password=;Pooling=True");
        string sql = "Update [BusinessPartner] " +
                      "Set UserName = @username, FirstName = @username, Name = @username " + 
                      "Where Id = @id";
        SqlCommand command = new SqlCommand(sql, conn);
        command.Parameters.AddWithValue("@username", configuration.UserName);
        command.Parameters.AddWithValue("@id", configuration.UserIdInTestDatabase);
        try
        {
            conn.Open();
            command.ExecuteNonQuery();
        }
        finally
        {
            conn.Close();
        }
    }
    private void RestoreDatabase(string databaseName, string backUpFile, string serverName, string userName, string password)
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
