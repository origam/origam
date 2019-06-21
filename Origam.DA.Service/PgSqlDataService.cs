#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Text;
using Npgsql;
using Origam.Schema.EntityModel;
using static Origam.DA.Common.Enums;

namespace Origam.DA.Service
{
	/// <summary>
	/// Summary description for PgSqlDataService.
	/// </summary>
	public class PgSqlDataService : AbstractSqlDataService
	{
        private const DatabaseType _PlatformName = DatabaseType.PgSql;
        private string _DbUser = "";
        private static readonly IDetachedFieldPacker detachedFieldPacker = new DetachedFieldPackerPostgre();
        #region Constructors
        public PgSqlDataService() : base(detachedFieldPacker)
		{
			Init();
		}

        public override DatabaseType PlatformName
        {
            get
            {
                return _PlatformName;
            }
        }

        public PgSqlDataService(string connection, int bulkInsertThreshold,
            int updateBatchSize) : base(connection, bulkInsertThreshold,
            updateBatchSize, detachedFieldPacker)
		{
			Init();
		}
		#endregion

		private void Init()
		{
			this.DbDataAdapterFactory = new PgSqlCommandGenerator(detachedFieldPacker);
        }

        internal override IDbConnection GetConnection(string connectionString)
		{
			return new NpgsqlConnection(connectionString);
		}
        public override string BuildConnectionString(string hostName,int port, string databaseName, 
			string userName, string password, bool integratedAuthentication, bool pooling)
        {
            NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = "Origam",
                Host = hostName,
                Port = port,
                Username = userName,
                Password = password,
                Database = string.IsNullOrEmpty(databaseName) ? "postgres":databaseName,
                Pooling = pooling,
                SearchPath = string.IsNullOrEmpty(databaseName) ? "" : databaseName
            };
            return sb.ConnectionString;
        }

        internal override void HandleException(Exception ex, string recordErrorMessage, DataRow row)
		{
			NpgsqlException sqle = ex as NpgsqlException;
            string customMessage = "";
            if (sqle != null)
            {
                PostgresException postgresException = ((Npgsql.PostgresException)sqle);

                if (postgresException.SqlState == "42601")
                {
                    throw new DataException("Syntax Error" + 
                        Environment.NewLine  + recordErrorMessage + Environment.NewLine + ex.Message, ex);
                }
                else if (postgresException.SqlState == "42P01")
                {
                    int firstApostrophe = ex.Message.IndexOf("\"");
                    if (ex.Message.Length > firstApostrophe)
                    {
                        int secondApostrophe = ex.Message.IndexOf("\"", firstApostrophe + 1);
                        string tableName = ex.Message.Substring(firstApostrophe + 1,
                            secondApostrophe - firstApostrophe - 1);
                        throw new DatabaseTableNotFoundException(tableName, ex);
                    }
                }
                else if (postgresException.SqlState == "42883")
                {
                    int firstApostrophe = ex.Message.IndexOf("\"");
                    if (ex.Message.Length > firstApostrophe)
                    {
                        int secondApostrophe = ex.Message.IndexOf("\"", firstApostrophe + 1);
                        string procedureName = ex.Message.Substring(firstApostrophe + 1,
                            secondApostrophe - firstApostrophe - 1);
                        throw new DatabaseProcedureNotFoundException(procedureName, ex);
                    }
                }
                else if(postgresException.SqlState.StartsWith("23"))
                {
                    customMessage = ResourceUtils.GetString("IntegrityError");
                }
                else
                {
                    customMessage = ResourceUtils.GetString("ExceptionWhenUpdate");
                }
                string message = string.Format("{0} {1}", recordErrorMessage, customMessage);
                throw new OrigamException(message, ex.Message, ex);
            }
        }

        public override string[] DatabaseSpecificDatatypes()
        {
            return Enum.GetNames(typeof(NpgsqlTypes.NpgsqlDbType));
        }
        public override void CreateUser(string user,string password,string database, bool DatabaseIntegratedAuthentication)
        {
            string transaction1 = Guid.NewGuid().ToString();
            try
            {
            ExecuteUpdate(string.Format("CREATE USER \"{0}\" WITH LOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION PASSWORD '{1}'", user,password), transaction1);
            ExecuteUpdate(string.Format("GRANT CONNECT ON DATABASE \"{0}\" TO \"{1}\"", database, user), transaction1);
            ExecuteUpdate(string.Format("GRANT ALL PRIVILEGES ON DATABASE \"{0}\" TO \"{1}\" ", database, user), transaction1);
            ExecuteUpdate(string.Format("GRANT ALL ON SCHEMA \"{0}\" TO \"{1}\" WITH GRANT OPTION ", database, user), transaction1);
                ResourceMonitor.Commit(transaction1);
            }
            catch (Exception)
            {
                ResourceMonitor.Rollback(transaction1);
                throw;
            }
        }

        public override void DeleteUser(string user,bool DatabaseIntegratedAuthentication)
        {
            //change owner of object  to postgres. it is not good , but for testing is ok.
            ExecuteUpdate(string.Format("REASSIGN OWNED BY \"{0}\" TO postgres", user), null);
            ExecuteUpdate(string.Format("DROP OWNED BY \"{0}\" ", user), null);
            ExecuteUpdate(string.Format("DROP ROLE \"{0}\" ", user), null);
        }
        public override void UpdateDatabaseSchemaVersion(string version, string transactionId)
        {
            ExecuteUpdate("ALTER  PROCEDURE OrigamDatabaseSchemaVersion AS SELECT '" + version + "'", transactionId);
        }
        public override void DeleteDatabase(string name)
        {
            CheckDatabaseName(name);
            ExecuteUpdate(string.Format("DROP DATABASE \"{0}\"", name), null);
        }

        public override void CreateDatabase(string name)
        {
            CheckDatabaseName(name);
            ExecuteUpdate(string.Format("CREATE DATABASE \"{0}\" ENCODING ='UTF8' ", name), null);
        }

        public override void CreateSchema(string SchemaName)
        {
            ExecuteUpdate(string.Format("CREATE SCHEMA \"{0}\";", SchemaName), null);
            ExecuteUpdate(string.Format("CREATE EXTENSION pgcrypto SCHEMA \"{0}\";", SchemaName), null);
        }

        private void CheckDatabaseName(string name)
        {
            if(name.Contains("\""))
            {
                throw new Exception(string.Format("Invalid database name: {0}", name));
            }
        }

        internal override string GetAllTablesSQL()
        {
            return "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where table_schema=(select current_schema) ORDER BY TABLE_NAME";
        }

        internal override string GetAllColumnsSQL()
        {
            return "SELECT TABLE_NAME, COLUMN_NAME, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH " +
                "FROM INFORMATION_SCHEMA.COLUMNS where table_schema=(select current_schema) ORDER BY TABLE_NAME";
        }

        internal override string GetSqlIndexFields()
        {
            return "SELECT c.relname AS TableName, "
                + "i.relname as IndexName,f.attname AS ColumnName, "
                + "f.attnum as OrdinalPosition, "
                + "(select pg_index_column_has_property(i.oid, f.attnum, p.name) from unnest(array['desc']) p(name)) as IsDescending "
                + "FROM pg_attribute f "
                + "JOIN pg_class c ON c.oid = f.attrelid "
                + "JOIN pg_type t ON t.oid = f.atttypid "
                + "LEFT JOIN pg_attrdef d ON d.adrelid = c.oid AND d.adnum = f.attnum "
                + "LEFT JOIN pg_namespace n ON n.oid = c.relnamespace "
                + "LEFT JOIN pg_constraint p ON p.conrelid = c.oid AND f.attnum = ANY(p.conkey) "
                + "LEFT JOIN pg_class AS g ON p.confrelid = g.oid "
                + "LEFT JOIN pg_index AS ix ON f.attnum = ANY(ix.indkey) and c.oid = f.attrelid and c.oid = ix.indrelid "
                + "LEFT JOIN pg_class AS i ON ix.indexrelid = i.oid "
                + "WHERE c.relkind = 'r'::char "
                + "AND n.nspname = (select current_schema) "
                + "AND f.attnum > 0";
        }

        internal override string GetSqlIndexes()
        {
            return "select tablename as TableName,indexname as IndexName  from pg_indexes where schemaname =(select current_schema)";
        }

        internal override string GetSqlFk()
        {
            return @"SELECT 
    ccu.table_name AS PK_Table,
    tc.table_name AS FK_Table, 
    tc.constraint_name as ""Constraint"",
    2067 as Status, 
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 0 LIMIT 1) as cKeyCol1,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 1 LIMIT 1) as cKeyCol2,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 2 LIMIT 1) as cKeyCol3,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 3 LIMIT 1) as cKeyCol4,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 4 LIMIT 1) as cKeyCol5,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 5 LIMIT 1) as cKeyCol6,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 6 LIMIT 1) as cKeyCol7,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 7 LIMIT 1) as cKeyCol8,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 8 LIMIT 1) as cKeyCol9,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 9 LIMIT 1) as cKeyCol10,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 10 LIMIT 1) as cKeyCol11,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 11 LIMIT 1) as cKeyCol12,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 12 LIMIT 1) as cKeyCol13,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 13 LIMIT 1) as cKeyCol14,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 14 LIMIT 1) as cKeyCol15,
    (select kcu.column_name from  information_schema.key_column_usage AS kcu where tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema OFFSET 15 LIMIT 1) as cKeyCol16,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 0 LIMIT 1) as cRefCol1,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 1 LIMIT 1) as cRefCol2,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 2 LIMIT 1) as cRefCol3,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 3 LIMIT 1) as cRefCol4,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 4 LIMIT 1) as cRefCol5,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 5 LIMIT 1) as cRefCol6,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 6 LIMIT 1) as cRefCol7,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 7 LIMIT 1) as cRefCol8,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 8 LIMIT 1) as cRefCol9,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 9 LIMIT 1) as cRefCol10,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 10 LIMIT 1) as cRefCol11,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 11 LIMIT 1) as cRefCol12,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 12 LIMIT 1) as cRefCol13,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 13 LIMIT 1) as cRefCol14,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 14 LIMIT 1) as cRefCol15,
    (select ccu.column_name from  information_schema.constraint_column_usage AS ccu where ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema OFFSET 15 LIMIT 1) as cRefCol16
   FROM
    information_schema.table_constraints AS tc
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' and
ccu.table_schema = tc.table_schema
group by ccu.table_name,tc.table_name,tc.constraint_name,tc.table_schema ";

        }

        internal override string GetPid()
        {
            return "select pg_backend_pid()";
        }

        public override string CreateSystemRole(string roleName)
        {
            StringBuilder systemRole = new StringBuilder();
            string roleId = Guid.NewGuid().ToString();
            systemRole.Append(" INSERT INTO \"OrigamApplicationRole\" (\"Id\", \"Name\", \"Description\", \"IsSystemRole\" , \"RecordCreated\")");
            systemRole.Append(Environment.NewLine);
            systemRole.Append(" VALUES ('{0}', '{1}', '', true, now()); ");
            systemRole.Append(Environment.NewLine);
            systemRole.Append("-- add to the built-in SuperUser role ");
            systemRole.Append(Environment.NewLine);
            systemRole.Append(" INSERT INTO \"OrigamRoleOrigamApplicationRole\" (\"Id\", \"refOrigamRoleId\", \"refOrigamApplicationRoleId\", \"RecordCreated\", \"IsFormReadOnly\") ");
            systemRole.Append(Environment.NewLine);
            systemRole.Append(" VALUES (gen_random_uuid(), '{2}', '{0}', now(), false)");
            return string.Format(systemRole.ToString(),
                 roleId, roleName, SecurityManager.BUILTIN_SUPER_USER_ROLE);
        }

        public override string CreateInsert(int fieldcount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("INSERT INTO \"{0}\" (");
            for (int i=1;i<fieldcount+1;i++)
            {
                stringBuilder.Append("\"{"+i+"}\"");
                stringBuilder.Append(i == fieldcount ? "" : ",");
            }
            stringBuilder.Append(") VALUES (");
            for (int i = fieldcount+1; i < fieldcount + fieldcount+1 ; i++)
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

				using (NpgsqlConnection cn = new NpgsqlConnection(this.ConnectionString))
				{
					cn.Open();

					result += "Server: " + cn.Host + Environment.NewLine;
					result += "Port: " + cn.Port.ToString() + Environment.NewLine;
					//result += "Backend Protocol Version: " + cn.BackendProtocolVersion.ToString() + Environment.NewLine;
					result += "Database: " + cn.Database + Environment.NewLine;
					result += "Server Version: " + cn.ServerVersion + Environment.NewLine;
					//result += "SSL: " + cn.SSL.ToString() + Environment.NewLine;
					//result += "Sync Notification: " + cn.SyncNotification.ToString() + Environment.NewLine;

					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
				
					result += "Select Timeout: " + settings.DataServiceSelectTimeout + Environment.NewLine;
					result += "Execute Procedure Timeout: " + settings.DataServiceExecuteProcedureTimeout + Environment.NewLine;

					try
					{
						object time;
						string currentUser = "";
						string systemUser = "";

						using(NpgsqlCommand cmd = new NpgsqlCommand("SELECT CURRENT_TIMESTAMP(3)", cn))
						{
							time = cmd.ExecuteScalar();
						}
					
						result += "Local Time: " + DateTime.Now.ToString() + Environment.NewLine;
						result += "Server Time: " + time.ToString() + Environment.NewLine;

						using(NpgsqlCommand cmd = new NpgsqlCommand("SELECT current_user", cn))
						{
							currentUser = (string)cmd.ExecuteScalar();
						}

						using(NpgsqlCommand cmd = new NpgsqlCommand("SELECT session_user", cn))
						{
							systemUser = (string)cmd.ExecuteScalar();
						}

						result += "Current User: " + currentUser + Environment.NewLine;
						result += "Session User: " + systemUser + Environment.NewLine;
					}
					catch(Exception ex)
					{
						result += "Error getting data from the server:" + Environment.NewLine;
						result += ex.Message + Environment.NewLine;
					}

					try
					{
						string version = this.DatabaseSchemaVersion();
						if(version != null)
						{
							result += "Database Schema Version: " + version + Environment.NewLine;
						}
					}
					catch
					{
					}

					cn.Close();
				}

				return result;
			}
		}

        internal override bool IsDataEntityIndexInDatabase(DataEntityIndex dataEntityIndex)
        {
            string tableName = (dataEntityIndex.ParentItem as TableMappingItem)
                .MappedObjectName;
            string indexName = dataEntityIndex.Name;
            // from CompareSchema
            string sqlIndex = "SELECT  tablename as TableName,indexname as IndexName FROM pg_indexes  WHERE " +
                "tablename ='"+tableName+"'" +
                "and indexname ='"+indexName+"'" +
                " ORDER BY tablename, indexname; ";
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
                    dbIndexList.Add(row["TableName"] + "." + row["IndexName"], schemaTableList[row["TableName"]]);
                }
            }
            return dbIndexList;
        }

        internal override Hashtable GetSchemaIndexListGenerate(ArrayList schemaTables, Hashtable dbTableList, Hashtable schemaIndexListAll)
        {
            Hashtable schemaIndexListGenerate = new Hashtable();
            foreach (TableMappingItem t in schemaTables)
            {
                if (t.GenerateDeploymentScript & t.DatabaseObjectType == DatabaseMappingObjectType.Table)
                {
                    // only existing tables
                    if (dbTableList.Contains(t.MappedObjectName))
                    {
                        foreach (DataEntityIndex index in t.EntityIndexes)
                        {
                            string key = t.MappedObjectName + "." + t.MappedObjectName+"_"+index.Name;
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

        public override string DbUser { get { return _DbUser; }  set { _DbUser = string.Format("{0}", value);  }}

        internal override object FillParameterArrayData(ICollection ar)
        {
            String[] vs = new String[ar.Count];
            ar.CopyTo(vs, 0);
            return vs;
        }
    }
}
