using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Origam.ServerCoreTests
{
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
}