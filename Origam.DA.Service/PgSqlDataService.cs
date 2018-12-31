#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Data;
using Npgsql;

namespace Origam.DA.Service
{
	/// <summary>
	/// Summary description for MsSqlDataService.
	/// </summary>
	public class PgSqlDataService : AbstractSqlDataService
	{
		#region Constructors
		public PgSqlDataService() : base()
		{
			Init();
		}

		public PgSqlDataService(string connection, int bulkInsertThreshold,
            int updateBatchSize) : base(connection, bulkInsertThreshold, updateBatchSize)
		{
			Init();
		}
		#endregion

		private void Init()
		{
			this.DbDataAdapterFactory = new PgSqlCommandGenerator();
		}

		internal override IDbConnection GetConnection(string connectionString)
		{
			return new NpgsqlConnection(connectionString);
		}

        public override string BuildConnectionString(string serverName, string databaseName, string userName, string password, bool integratedAuthentication, bool pooling)
        {
            throw new NotImplementedException();
        }

        internal override void HandleException(Exception ex, string rowErrorMessage, DataRow row)
		{
			NpgsqlException sqle = ex as NpgsqlException;

//			if(sqle != null)
//			{
//				if(sqle.Number == 547)
//				{
//					throw new ConstraintException("Pøi aktualizaci nebo mazání dat došlo k porušení integrity dat. Patrnì jste se pokusili vymazat záznam, který je již použit jinde.\nOperace byla pøerušena a nebyla uložena žádná data.\n\nHlášení datového zdroje: " + ex.Message);
//				}
//				else if(sqle.Number == 2601)
//				{
//					throw new DataException("Pokusili jste se vložit duplicitní data. Data nebylo možno uložit.", ex);
//				}
//				else
//				{
//					throw new DataException("Exception was was encountered while updating data." + Environment.NewLine + ex.Message, ex);
//				}	
//			}
		}

        public override string[] DatabaseSpecificDatatypes()
        {
            return Enum.GetNames(typeof(NpgsqlTypes.NpgsqlDbType));
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

						using(NpgsqlCommand cmd = new NpgsqlCommand("SELECT CURRENT_TIMESTAMP", cn))
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
	}
}
