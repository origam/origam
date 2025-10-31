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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using static Origam.DA.Common.Enums;

namespace Origam.DA.Service;

/// <summary>
/// Summary description for MsSqlDataService.
/// </summary>
public class MsSqlDataService : AbstractSqlDataService
{
    private const DatabaseType _PlatformName = DatabaseType.MsSql;
    private string _IISUser;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    #region Constructors
    public MsSqlDataService()
    {
        Init();
    }

    public MsSqlDataService(string connection, int bulkInsertThreshold, int updateBatchSize)
        : base(connection, bulkInsertThreshold, updateBatchSize)
    {
        Init();
    }
    #endregion
    private void Init()
    {
        this.DbDataAdapterFactory = new MsSqlCommandGenerator();
    }

    public override DatabaseType PlatformName
    {
        get { return _PlatformName; }
    }

    internal override IDbConnection GetConnection(string connectionString)
    {
        SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString);
        sb.ApplicationName = "ORIGAM [" + SecurityManager.CurrentPrincipal.Identity.Name + "]";
        SqlConnection result = new SqlConnection(sb.ToString());
        return result;
    }

    public override string BuildConnectionString(
        string serverName,
        int port,
        string databaseName,
        string userName,
        string password,
        bool integratedAuthentication,
        bool pooling
    )
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.IntegratedSecurity = integratedAuthentication;
        builder.UserID = userName;
        builder.Password = password;
        builder.DataSource = serverName;
        builder.InitialCatalog = databaseName;
        builder.Pooling = pooling;
        builder.Encrypt = false;
        if (!integratedAuthentication && port != 1433)
        {
            builder.DataSource = serverName + "," + port;
        }
        return builder.ConnectionString;
    }

    public override void UpdateDatabaseSchemaVersion(string version, string transactionId)
    {
        ExecuteUpdate(
            "ALTER  PROCEDURE [OrigamDatabaseSchemaVersion] AS SELECT '" + version + "'",
            transactionId
        );
    }

    public override void DeleteDatabase(string name)
    {
        CheckDatabaseName(name);
        ExecuteUpdate(string.Format("DROP DATABASE [{0}]", name), null);
    }

    public override void CreateDatabase(string name)
    {
        CheckDatabaseName(name);
        ExecuteUpdate(string.Format("CREATE DATABASE [{0}]", name), null);
    }

    private static void CheckDatabaseName(string name)
    {
        if (name.Contains("]"))
        {
            throw new Exception(string.Format("Invalid database name: {0}", name));
        }
    }

    internal override void HandleException(Exception ex, string recordErrorMessage, DataRow row)
    {
        SqlException sqle = ex as SqlException;
        if (log.IsDebugEnabled)
        {
            log.Debug("Handling MS SQL Exception", ex);
        }
        string customMessage = "";
        if (sqle != null)
        {
            if (sqle.Number == 208)
            {
                int firstApostrophe = ex.Message.IndexOf("'");
                if (ex.Message.Length > firstApostrophe)
                {
                    int secondApostrophe = ex.Message.IndexOf("'", firstApostrophe + 1);
                    string tableName = ex.Message.Substring(
                        firstApostrophe + 1,
                        secondApostrophe - firstApostrophe - 1
                    );
                    throw new DatabaseTableNotFoundException(tableName, ex);
                }
            }
            if (sqle.Number == 2812)
            {
                int firstApostrophe = ex.Message.IndexOf("'");
                if (ex.Message.Length > firstApostrophe)
                {
                    int secondApostrophe = ex.Message.IndexOf("'", firstApostrophe + 1);
                    string procedureName = ex.Message.Substring(
                        firstApostrophe + 1,
                        secondApostrophe - firstApostrophe - 1
                    );
                    throw new DatabaseProcedureNotFoundException(procedureName, ex);
                }
            }
            else if (sqle.Number == 547 && row.RowState == DataRowState.Deleted)
            {
                customMessage = ResourceUtils.GetString("IntegrityError");
            }
            else if (sqle.Number == 2601)
            {
                // check if there is a field name in parentheses, in that case
                // we will try to look for a unique index matching with the error
                // message so we can give a good error message to the user
                Match match = Regex.Match(sqle.Message, @"\((.*?)\)");
                if (match != null)
                {
                    string fieldName = ResolveUniqueFieldNames(row, sqle);
                    customMessage = ResourceUtils.GetString(
                        "DuplicityErrorDetailed",
                        fieldName,
                        match.Value
                    );
                }
                else
                {
                    customMessage = ResourceUtils.GetString("DuplicityError");
                }
            }
            else
            {
                customMessage = ResourceUtils.GetString("ExceptionWhenUpdate");
            }
        }
        string message = string.Format("{0} {1}", recordErrorMessage, customMessage);
        throw new UserOrigamException(message, ex.Message, ex);
    }

    internal override void BulkInsert(
        DataStructureEntity entity,
        IDbConnection connection,
        IDbTransaction transaction,
        DataTable table
    )
    {
        // batch insert
        using (
            SqlBulkCopy bulk = new SqlBulkCopy(
                connection as SqlConnection,
                SqlBulkCopyOptions.CheckConstraints,
                transaction as SqlTransaction
            )
        )
        {
            foreach (DataStructureColumn col in entity.Columns)
            {
                FieldMappingItem dbField = col.Field as FieldMappingItem;
                if (dbField != null)
                {
                    bulk.ColumnMappings.Add(col.Name, dbField.MappedColumnName);
                }
            }
            bulk.DestinationTableName = (
                entity.EntityDefinition as TableMappingItem
            ).MappedObjectName;
            bulk.BulkCopyTimeout = 1000;
            bulk.WriteToServer(table);
        }
    }

    private static string ResolveUniqueFieldNames(DataRow row, SqlException sqle)
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        Guid entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
        TableMappingItem entity = (TableMappingItem)
            persistence.SchemaProvider.RetrieveInstance(
                typeof(TableMappingItem),
                new ModelElementKey(entityId)
            );
        StringBuilder fieldNames = new StringBuilder();
        foreach (DataEntityIndex index in entity.EntityIndexes)
        {
            if (index.IsUnique && sqle.Message.Contains(index.Name))
            {
                List<ISchemaItem> sortedFields = index.ChildItems.ToList();
                sortedFields.Sort();
                foreach (DataEntityIndexField field in sortedFields)
                {
                    if (fieldNames.Length > 0)
                    {
                        fieldNames.Append(", ");
                    }
                    fieldNames.Append(field.Field.Caption ?? field.Field.Name);
                }
                break;
            }
        }
        return fieldNames.ToString();
    }

    public override string[] DatabaseSpecificDatatypes()
    {
        return Enum.GetNames(typeof(SqlDbType));
    }

    public override void CreateDatabaseUser(
        string _loginName,
        string password,
        string name,
        bool DatabaseIntegratedAuthentication
    )
    {
        if (DatabaseIntegratedAuthentication)
        {
            string command1 = string.Format(
                "CREATE LOGIN {0} FROM WINDOWS WITH DEFAULT_DATABASE=[master]",
                _loginName
            );
            string command2 = string.Format("CREATE USER {0} FOR LOGIN {0}", _loginName);
            string command3 = string.Format(
                "ALTER ROLE [db_datareader] ADD MEMBER {0}",
                _loginName
            );
            string command4 = string.Format(
                "ALTER ROLE [db_datawriter] ADD MEMBER {0}",
                _loginName
            );
            string transaction1 = Guid.NewGuid().ToString();
            try
            {
                ExecuteUpdate(command1, transaction1);
                ExecuteUpdate(command2, transaction1);
                ExecuteUpdate(command3, transaction1);
                ExecuteUpdate(command4, transaction1);
                ResourceMonitor.Commit(transaction1);
            }
            catch (Exception)
            {
                ResourceMonitor.Rollback(transaction1);
                throw;
            }
        }
    }

    public override void DeleteUser(string _loginName, bool DatabaseIntegratedAuthentication)
    {
        if (DatabaseIntegratedAuthentication)
        {
            string command1 = string.Format("DROP LOGIN {0}", _loginName);
            ExecuteUpdate(command1, null);
        }
    }

    internal override string GetAllTablesSql()
    {
        return "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
    }

    internal override string GetAllColumnsSQL()
    {
        return "SELECT TABLE_NAME, COLUMN_NAME, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME";
    }

    internal override string GetSqlIndexFields()
    {
        return "select	so.name TableName, si.name IndexName, "
            + "sc.name ColumnName, sik.keyno as OrdinalPosition, "
            + "indexkey_property(sik.id, sik.indid, sik.keyno, 'IsDescending') as IsDescending "
            + "from sysindexes si "
            + "inner join sysobjects so on si.id = so.id "
            + "inner join sysindexkeys sik on si.indid = sik.indid and sik.id = so.id "
            + "inner join syscolumns sc on sik.colid = sc.colid and sc.id = so.id "
            + "where indexproperty(si.id, si.name, 'IsStatistics') = 0	"
            + "and indexproperty(si.id, si.name, 'IsHypothetical') = 0 "
            + "and si.status & 2048 = 0 "
            + "and si.impid = 0";
    }

    internal override string GetSqlIndexes()
    {
        return "select	so.name TableName, si.name IndexName "
            + "from sysindexes si "
            + "inner join sysobjects so on si.id = so.id "
            + "where indexproperty(si.id, si.name, 'IsStatistics') = 0 " // no statistics
            + "and indexproperty(si.id, si.name, 'IsHypothetical') = 0 " // whatever it is, we don't want it...
            + "and si.status & 2048 = 0 " // no primary keys
            + "and si.impid = 0" // no awkward indexes...
            + "and si.name is not null";
    }

    internal override string GetSqlFk()
    {
        return "select "
            + "N'PK_Table' = PKT.name, "
            + "N'FK_Table' = FKT.name, "
            + "N'Constraint' = object_name(r.constid), "
            + "c.status, "
            + "cKeyCol1 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey1)), "
            + "cKeyCol2 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey2)), "
            + "cKeyCol3 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey3)), "
            + "cKeyCol4 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey4)), "
            + "cKeyCol5 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey5)), "
            + "cKeyCol6 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey6)), "
            + "cKeyCol7 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey7)), "
            + "cKeyCol8 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey8)), "
            + "cKeyCol9 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey9)), "
            + "cKeyCol10 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey10)), "
            + "cKeyCol11 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey11)), "
            + "cKeyCol12 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey12)), "
            + "cKeyCol13 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey13)), "
            + "cKeyCol14 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey14)), "
            + "cKeyCol15 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey15)), "
            + "cKeyCol16 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey16)), "
            + "cRefCol1 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey1)), "
            + "cRefCol2 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey2)),	 "
            + "cRefCol3 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey3)), "
            + "cRefCol4 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey4)), "
            + "cRefCol5 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey5)), "
            + "cRefCol6 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey6)), "
            + "cRefCol7 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey7)), "
            + "cRefCol8 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey8)), "
            + "cRefCol9 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey9)), "
            + "cRefCol10 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey10)), "
            + "cRefCol11 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey11)), "
            + "cRefCol12 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey12)), "
            + "cRefCol13 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey13)), "
            + "cRefCol14 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey14)), "
            + "cRefCol15 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey15)), "
            + "cRefCol16 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey16)), "
            + "N'PK_Table_Owner' = user_name(PKT.uid), "
            + "N'FK_Table_Owner' = user_name(FKT.uid), "
            + "N'DeleteCascade' = OBJECTPROPERTY( r.constid, N'CnstIsDeleteCascade'), "
            + "N'UpdateCascade' = OBJECTPROPERTY( r.constid, N'CnstIsUpdateCascade') "
            + "from dbo.sysreferences r, dbo.sysconstraints c, dbo.sysobjects PKT, dbo.sysobjects FKT "
            + "where r.constid = c.constid  "
            + "and PKT.id = r.rkeyid and FKT.id = r.fkeyid ";
    }

    internal override string GetPid()
    {
        return "SELECT @@SPID";
    }

    public override string CreateSystemRole(string roleName)
    {
        string roleId = Guid.NewGuid().ToString();
        return string.Format(
            @"INSERT INTO OrigamApplicationRole (Id, Name, Description, IsSystemRole , RecordCreated)
VALUES ('{0}', '{1}', '', 1, getdate())
-- add to the built-in SuperUser role
INSERT INTO OrigamRoleOrigamApplicationRole (Id, refOrigamRoleId, refOrigamApplicationRoleId, RecordCreated, IsFormReadOnly, IsInitialScreen)
VALUES (newid(), '{2}', '{0}', getdate(), 0, 0)",
            roleId,
            roleName,
            SecurityManager.BUILTIN_SUPER_USER_ROLE
        );
    }

    public override string CreateInsert(int fieldcount)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("INSERT INTO [{0}] (");
        for (int i = 1; i < fieldcount + 1; i++)
        {
            stringBuilder.Append("[{" + i + "}]");
            stringBuilder.Append(i == fieldcount ? "" : ",");
        }
        stringBuilder.Append(") VALUES (");
        for (int i = fieldcount + 1; i < fieldcount + fieldcount + 1; i++)
        {
            stringBuilder.Append("'{" + i + "}'");
            stringBuilder.Append(i == fieldcount + fieldcount ? "" : ",");
        }
        stringBuilder.Append(");\r\n");
        return stringBuilder.ToString();
    }

    public override string Info
    {
        get
        {
            string result = "";
            using (SqlConnection cn = new SqlConnection(this.ConnectionString))
            {
                cn.Open();
                result += "Server: " + cn.DataSource + Environment.NewLine;
                result += "Database: " + cn.Database + Environment.NewLine;
                result += "Server Version: " + cn.ServerVersion + Environment.NewLine;
                result += "WorkstationId: " + cn.WorkstationId + Environment.NewLine;
                result += "Packet Size: " + cn.PacketSize.ToString() + Environment.NewLine;
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();

                result +=
                    "Select Timeout: " + settings.DataServiceSelectTimeout + Environment.NewLine;
                result +=
                    "Execute Procedure Timeout: "
                    + settings.DataServiceExecuteProcedureTimeout
                    + Environment.NewLine;
                try
                {
                    object time;
                    string currentUser = "";
                    string systemUser = "";
                    using (SqlCommand cmd = new SqlCommand("SELECT getdate()", cn))
                    {
                        time = cmd.ExecuteScalar();
                    }

                    result += "Local Time: " + DateTime.Now.ToString() + Environment.NewLine;
                    result += "Server Time: " + time.ToString() + Environment.NewLine;
                    using (SqlCommand cmd = new SqlCommand("SELECT current_user", cn))
                    {
                        currentUser = (string)cmd.ExecuteScalar();
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT system_user", cn))
                    {
                        systemUser = (string)cmd.ExecuteScalar();
                    }
                    result += "Current User: " + currentUser + Environment.NewLine;
                    result += "System User: " + systemUser + Environment.NewLine;
                }
                catch (Exception ex)
                {
                    result += "Error getting data from the server:" + Environment.NewLine;
                    result += ex.Message + Environment.NewLine;
                }
                try
                {
                    string version = this.DatabaseSchemaVersion();
                    if (version != null)
                    {
                        result += "Database Schema Version: " + version + Environment.NewLine;
                    }
                }
                catch { }
                cn.Close();
            }
            return result;
        }
    }

    internal override bool IsDataEntityIndexInDatabase(DataEntityIndex dataEntityIndex)
    {
        string tableName = (dataEntityIndex.ParentItem as TableMappingItem).MappedObjectName;
        string indexName = dataEntityIndex.Name;
        // from CompareSchema
        string sqlIndex =
            "select so.name TableName, si.name IndexName "
            + "from sysindexes si "
            + "inner join sysobjects so on si.id = so.id "
            + "where indexproperty(si.id, si.name, 'IsStatistics') = 0 "
            + "and indexproperty(si.id, si.name, 'IsHypothetical') = 0 "
            + "and si.status & 2048 = 0 "
            + "and si.impid = 0 "
            + "and so.name = '"
            + tableName
            + "' "
            + "and si.name = '"
            + indexName
            + "'";
        DataSet index = GetData(sqlIndex);
        return index.Tables[0].Rows.Count == 1;
    }

    internal override Hashtable GetDbIndexList(DataSet indexes, Hashtable schemaTableList)
    {
        Hashtable dbIndexList = new Hashtable();
        foreach (DataRow row in indexes.Tables[0].Rows)
        {
            // only existing tables
            if (schemaTableList.ContainsKey(row["TableName"]))
            {
                dbIndexList.Add(
                    row["TableName"] + "." + row["IndexName"],
                    schemaTableList[row["TableName"]]
                );
            }
        }
        return dbIndexList;
    }

    internal override Hashtable GetSchemaIndexListGenerate(
        List<TableMappingItem> schemaTables,
        Hashtable dbTableList,
        Hashtable schemaIndexListAll
    )
    {
        Hashtable schemaIndexListGenerate = new Hashtable();
        foreach (TableMappingItem t in schemaTables)
        {
            if (
                t.GenerateDeploymentScript
                & t.DatabaseObjectType == DatabaseMappingObjectType.Table
            )
            {
                // only existing tables
                if (dbTableList.Contains(t.MappedObjectName))
                {
                    foreach (DataEntityIndex index in t.EntityIndexes)
                    {
                        string key = t.MappedObjectName + "." + index.Name;
                        schemaIndexListAll.Add(key, index);
                        if (index.GenerateDeploymentScript)
                        {
                            schemaIndexListGenerate.Add(key, index);
                        }
                    }
                }
            }
        }
        return schemaIndexListGenerate;
    }

    public override string DbUser
    {
        get { return _IISUser; }
        set { _IISUser = string.Format("[IIS APPPOOL\\{0}]", value); }
    }

    internal override object FillParameterArrayData(ICollection ar)
    {
        DataTable dt = new OrigamDataTable("ListTable");
        dt.Columns.Add("ListValue", typeof(string));
        dt.Columns[0].MaxLength = -1;
        dt.BeginLoadData();
        foreach (object v in ar)
        {
            DataRow row = dt.NewRow();
            row[0] = v.ToString();
            dt.Rows.Add(row);
        }
        dt.EndLoadData();
        return dt;
    }

    public override string CreateBusinessPartnerInsert(QueryParameterCollection parameters)
    {
        return string.Format(
            "INSERT INTO [dbo].[BusinessPartner] ([FirstName],[UserName],[Name],[Id],[UserEmail]) "
                + "VALUES ('{0}','{1}','{2}','{3}','{4}')",
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "FirstName")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "UserName")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Name")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Id")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Email")
                .Select(param => param.Value)
                .FirstOrDefault()
        );
    }

    public override string CreateOrigamUserInsert(QueryParameterCollection parameters)
    {
        return string.Format(
            "INSERT INTO [dbo].[OrigamUser] "
                + "([UserName],[EmailConfirmed],[refBusinessPartnerId],[Password],[Id],[FailedPasswordAttemptCount],[Is2FAEnforced]) "
                + "VALUES ('{0}',{1},'{2}','{3}','{4}','{5}','{6}')",
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "UserName")
                .Select(param => param.Value)
                .FirstOrDefault(),
            1,
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Id")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Password")
                .Select(param => param.Value)
                .FirstOrDefault(),
            Guid.NewGuid().ToString(),
            0,
            0
        );
    }

    public override string CreateBusinessPartnerRoleIdInsert(QueryParameterCollection parameters)
    {
        return string.Format(
            "INSERT INTO [dbo].[BusinessPartnerOrigamRole] ([Id],[refBusinessPartnerId],[refOrigamRoleId]) "
                + "VALUES ('{0}','{1}','{2}')",
            Guid.NewGuid().ToString(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "Id")
                .Select(param => param.Value)
                .FirstOrDefault(),
            parameters
                .Cast<QueryParameter>()
                .Where(param => param.Name == "RoleId")
                .Select(param => param.Value)
                .FirstOrDefault()
        );
    }

    public override string AlreadyCreatedUser(QueryParameterCollection parameters)
    {
        return string.Format(
            "UPDATE [dbo].[OrigamParameters] SET [BooleanValue] = 1 WHERE [Id] = 'e42f864f-5018-4967-abdc-5910439adc9a'"
        );
    }

    protected override void ResetTransactionIsolationLevel(IDbCommand command)
    {
        command.Connection = null;
    }
}
