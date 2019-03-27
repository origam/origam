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

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace Origam.DA.Service
{
    /// <summary>
	/// Summary description for MsSqlDataService.
	/// </summary>
	public class MsSqlDataService : AbstractSqlDataService
	{
        private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #region Constructors
		public MsSqlDataService() : base()
		{
			Init();
		}

		public MsSqlDataService(string connection, int bulkInsertThreshold,
            int updateBatchSize) : base(connection, bulkInsertThreshold, updateBatchSize)
		{
			Init();
		}
		#endregion

		private void Init()
		{
			this.DbDataAdapterFactory = new MsSqlCommandGenerator();
		}

		internal override IDbConnection GetConnection(string connectionString)
		{
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString);
            sb.ApplicationName = "ORIGAM [" + System.Threading.Thread.CurrentPrincipal.Identity.Name + "]";
            SqlConnection result = new SqlConnection(sb.ToString());
            return result;
		}

        public override string BuildConnectionString(string serverName, string databaseName, string userName, string password, bool integratedAuthentication, bool pooling)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = integratedAuthentication;
            builder.UserID = userName;
            builder.Password = password;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            builder.Pooling = pooling;
            return builder.ConnectionString;
        }

		internal override void HandleException(Exception ex, string recordErrorMessage, DataRow row)
		{
			SqlException sqle = ex as SqlException;
            if (log.IsDebugEnabled)
            {
                log.Debug("Handling MS SQL Exception", ex);
            }
			string customMessage = "";
			if(sqle != null)
			{
				if(sqle.Number == 208)
				{
                    int firstApostrophe = ex.Message.IndexOf("'");
                    if(ex.Message.Length > firstApostrophe)
                    {
                        int secondApostrophe = ex.Message.IndexOf("'", firstApostrophe + 1);
                        string tableName = ex.Message.Substring(firstApostrophe + 1,
                            secondApostrophe - firstApostrophe - 1);
                        throw new DatabaseTableNotFoundException(tableName, ex);
                    }
				}
                if (sqle.Number == 2812)
                {
                    int firstApostrophe = ex.Message.IndexOf("'");
                    if (ex.Message.Length > firstApostrophe)
                    {
                        int secondApostrophe = ex.Message.IndexOf("'", firstApostrophe + 1);
                        string procedureName = ex.Message.Substring(firstApostrophe + 1,
                            secondApostrophe - firstApostrophe - 1);
                        throw new DatabaseProcedureNotFoundException(procedureName, ex);
                    }
                }
                else if (sqle.Number == 547 && row.RowState == DataRowState.Deleted)
				{
					customMessage = ResourceUtils.GetString("IntegrityError");
				}
				else if(sqle.Number == 2601)
				{
                    // check if there is a field name in parentheses, in that case
                    // we will try to look for a unique index matching with the error 
                    // message so we can give a good error message to the user
                    Match match = Regex.Match(sqle.Message, @"\((.*?)\)");
                    if (match != null)
                    {
                        string fieldName = ResolveUniqueFieldNames(row, sqle);
                        customMessage = ResourceUtils.GetString("DuplicityErrorDetailed",
                            fieldName, match.Value);
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
			throw new OrigamException(message, ex.Message, ex);
		}

        internal override void BulkInsert(
            DataStructureEntity entity, 
            IDbConnection connection, 
            IDbTransaction transaction,
            DataTable table)
        {
            // batch insert
            using(SqlBulkCopy bulk = new SqlBulkCopy(
                connection as SqlConnection,
                SqlBulkCopyOptions.CheckConstraints,
                transaction as SqlTransaction))
            {
                foreach(DataStructureColumn col in entity.Columns)
                {
                    FieldMappingItem dbField = col.Field as FieldMappingItem;
                    if(dbField != null)
                    {
                        bulk.ColumnMappings.Add(
                            col.Name, dbField.MappedColumnName);
                    }
                }
                bulk.DestinationTableName 
                    = (entity.EntityDefinition as TableMappingItem)
                    .MappedObjectName;
                bulk.BulkCopyTimeout = 1000;
                bulk.WriteToServer(table);
            }
        }

        private static string ResolveUniqueFieldNames(DataRow row, SqlException sqle)
        {
            IPersistenceService persistence = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
            Guid entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
            TableMappingItem entity =
                (TableMappingItem)persistence.SchemaProvider.RetrieveInstance(
                typeof(TableMappingItem), new ModelElementKey(entityId));
            ArrayList sortedIndexes = new ArrayList(entity.EntityIndexes);
            sortedIndexes.Sort();
            // sort descending for cases where one index name would be
            // a subset of another, so they will come e.g.
            // ix_NameAndFirstName
            // ix_Name
            sortedIndexes.Reverse();
            StringBuilder fieldNames = new StringBuilder();
            foreach (DataEntityIndex index in entity.EntityIndexes)
            {
                if (index.IsUnique && sqle.Message.Contains(index.Name))
                {
                    ArrayList sortedFields = new ArrayList(index.ChildItems);
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

					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
				
					result += "Select Timeout: " + settings.DataServiceSelectTimeout + Environment.NewLine;
					result += "Execute Procedure Timeout: " + settings.DataServiceExecuteProcedureTimeout + Environment.NewLine;

					try
					{
						object time;
						string currentUser = "";
						string systemUser = "";

						using(SqlCommand cmd = new SqlCommand("SELECT getdate()", cn))
						{
							time = cmd.ExecuteScalar();
						}
					
						result += "Local Time: " + DateTime.Now.ToString() + Environment.NewLine;
						result += "Server Time: " + time.ToString() + Environment.NewLine;

						using(SqlCommand cmd = new SqlCommand("SELECT current_user", cn))
						{
							currentUser = (string)cmd.ExecuteScalar();
						}

						using(SqlCommand cmd = new SqlCommand("SELECT system_user", cn))
						{
							systemUser = (string)cmd.ExecuteScalar();
						}

						result += "Current User: " + currentUser + Environment.NewLine;
						result += "System User: " + systemUser + Environment.NewLine;
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
	}
}
