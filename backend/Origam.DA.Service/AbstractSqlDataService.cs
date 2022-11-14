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
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using log4net;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.Generators;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.DA.Service
{
	#region Data Loader
	internal class DataLoader
	{
		private static readonly ILog log = 
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string ConnectionString = null;
		public DataStructureFilterSet Filter = null;
		public DataStructureSortSet SortSet = null;
		public DataStructureEntity Entity = null;
		public DataStructureQuery Query = null;
		public DataStructure Ds = null;
		public DataSet Dataset = null;
		public AbstractSqlDataService DataService = null;
		public IDbTransaction Transaction = null;
		public string TransactionId;
		public UserProfile CurrentProfile = null;
		public int Timeout;

		public void Fill()
		{
			IDbConnection connection;
			if(this.Transaction == null)
			{
				connection = DataService.GetConnection(this.ConnectionString);
			}
			else
			{
				connection = this.Transaction.Connection;
			}
			try
			{
				DbDataAdapter adapter;
				switch(Query.DataSourceType)
				{
					case QueryDataSourceType.DataStructure:
					    var selectParameters = new SelectParameters
					    {
					        DataStructure = Ds,
					        Entity = Entity,
					        Filter = Filter,
					        SortSet = SortSet,
					        Parameters = Query.Parameters.ToHashtable(),
					        Paging = Query.Paging,
					        ColumnsInfo = Query.ColumnsInfo,
					        AggregatedColumns = Query.AggregatedColumns
					    };
                        adapter = DataService.GetAdapter(selectParameters, CurrentProfile);
						break;
					case QueryDataSourceType.DataStructureEntity:
						adapter = DataService.GetSelectRowAdapter(Entity, Filter, Query.ColumnsInfo);
						break;

					default:
						throw new ArgumentOutOfRangeException("DataSourceType", 
                            Query.DataSourceType, ResourceUtils.GetString("UnknownDataSource"));
				}
				
				IDbDataAdapter dbDataAdapter = adapter;
				dbDataAdapter.SelectCommand.Connection = connection;
				dbDataAdapter.SelectCommand.Transaction = this.Transaction;
				dbDataAdapter.SelectCommand.CommandTimeout = this.Timeout;
				// ignore any extra fields returned by the select statement - e.g. RowNum returned when paging is turned on
				adapter.MissingMappingAction = MissingMappingAction.Ignore;
				DataService.BuildParameters(Query.Parameters, dbDataAdapter.SelectCommand.Parameters, CurrentProfile);
				try
				{
					if (this.Transaction == null)
					{
						connection.Open();
					}

					Dataset.Tables[Entity.Name].BeginLoadData();
					this.DataService.TraceCommand(dbDataAdapter.SelectCommand,
						this.TransactionId);
					adapter.Fill(Dataset);
					Dataset.Tables[Entity.Name].EndLoadData();
				}
				catch (Exception ex)
				{
					HandleException(
						ex: ex,
						commandText: dbDataAdapter.SelectCommand.CommandText,
						logAsDebug: Entity.Name == "AsapModelVersion" && 
						            ex is SqlException && 
						            ex.HResult == -2146232060 );
				}
				finally
				{
					((IDbDataAdapter) adapter).SelectCommand.Transaction = null;
					((IDbDataAdapter) adapter).SelectCommand.Connection = null;
				}
			}
			finally
			{
				try
				{
					if(this.Transaction == null)
					{
						connection.Close();
					}
				} 
				catch{}
				if(this.Transaction == null)
				{
					connection.Dispose();
				}
			}
		}

		private void HandleException(Exception ex, string commandText, bool logAsDebug)
		{
			if (log.IsErrorEnabled && !logAsDebug)
			{
				log.LogOrigamError(
					$"{ex.Message}, SQL: {commandText}",
					ex);
			}
			else if(log.IsDebugEnabled && logAsDebug)
			{
				log.Debug(
					$"{ex.Message}, SQL: {commandText}",
					ex);
			}

			string standardMessage = ResourceUtils.GetString(
				"ErrorLoadingData",
				(Entity.EntityDefinition as TableMappingItem)
				.MappedObjectName,
				Entity.Name,
				Environment.NewLine, ex.Message);
			this.DataService.HandleException(ex, standardMessage, null);
		}
	}
    #endregion

    // version of log4net for NetStandard 1.3 does not have the method
    // LogManager.GetLogger(string)... have to use the overload with Type as parameter 
    public class ConcurrencyExceptionLogger
    {
    }

    public abstract class AbstractSqlDataService : AbstractDataService
	{				
		private readonly Profiler profiler = new Profiler(); 
		
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // Special logger for concurrency exception detail logging
        private static readonly ILog concurrencyLog = LogManager.GetLogger(typeof(ConcurrencyExceptionLogger));
		private IDbDataAdapterFactory _adapterFactory;
		private string _connectionString = "";
        private const int DATA_VISUALIZATION_MAX_LENGTH = 100;
        internal abstract string GetAllTablesSQL();
        

        #region Constructors
        public AbstractSqlDataService()
        {
        }

		public AbstractSqlDataService(string connection, int bulkInsertThreshold,
            int updateBatchSize)
		{
			_connectionString = connection;
            UpdateBatchSize = updateBatchSize;
            BulkInsertThreshold = bulkInsertThreshold;
		}
        #endregion

        #region Public Methods

        public abstract string CreateSystemRole(string roleName);
        public abstract string CreateInsert(int fieldcount);

        public override string ConnectionString
		{
			get
			{
				return _connectionString;
			}
			set
			{
				_connectionString = value;
			}
		}


		public override IDbDataAdapterFactory DbDataAdapterFactory
		{
			get
			{
				return _adapterFactory;
			}
			internal set
			{
				_adapterFactory = value;
			}
		}

        internal override IDbTransaction GetTransaction(string transactionId, IsolationLevel isolationLevel)
		{
			IDbConnection connection;
			IDbTransaction transaction;

			if(transactionId == null)
			{
				connection = GetConnection(this.ConnectionString);
				connection.Open();
				transaction = connection.BeginTransaction(isolationLevel);
			}
			else
			{
				OrigamDbTransaction origamDbTransaction = ResourceMonitor.GetTransaction(
                    transactionId, this.ConnectionString) as OrigamDbTransaction;
				if(origamDbTransaction == null)
				{
					transaction = this.GetTransaction(null, isolationLevel);
					ResourceMonitor.RegisterTransaction(transactionId, 
                        this.ConnectionString, new OrigamDbTransaction(transaction));
				}
				else
				{
					transaction = origamDbTransaction.Transaction;
					connection = transaction.Connection;

					if(transaction.IsolationLevel != isolationLevel)
					{
						throw new Exception("Existing transaction has a different isolation level then the current query. When using a different isolation level the query has to be executed under a different or no transaction.");
					}
				}
			}

			return transaction;
		}

		public override DataSet LoadDataSet(DataStructureQuery dataStructureQuery, 
            IPrincipal principal, string transactionId)
		{
			DataSet loadedDataSet = LoadDataSet(dataStructureQuery, principal, null, transactionId);
			profiler.LogRememberedExecutionTimes();
			return loadedDataSet;
		}

		public override DataSet LoadDataSet(DataStructureQuery query,
            IPrincipal principal, DataSet dataset, string transactionId)
		{
			OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
			int timeout = settings.DataServiceSelectTimeout;

			UserProfile currentProfile = null;
			if(query.LoadByIdentity)
			{
				currentProfile = SecurityManager.GetProfileProvider().GetProfile(principal.Identity) as UserProfile;
			}

			if(this.PersistenceProvider == null)
			{
				throw new NullReferenceException(ResourceUtils.GetString("NoProviderForMS"));
			}

			ArrayList entities;
			DataStructure ds = null;
			DataStructureFilterSet filter = null;
			DataStructureSortSet sortSet = null;

			switch(query.DataSourceType)
			{
				case QueryDataSourceType.DataStructure:
					ds = this.GetDataStructure(query);
					filter = this.GetFilterSet(query.MethodId);
					sortSet = this.GetSortSet(query.SortSetId);
                    if (string.IsNullOrEmpty(query.Entity))
                    {
                        entities = ds.Entities;
                    }
                    else
                    {
                        entities = new ArrayList();
                        foreach (DataStructureEntity e in ds.Entities)
                        {
                            if (e.Name == query.Entity)
                            {
                                entities.Add(e);
                                break;
                            }
                        }
                        if (entities.Count == 0)
                        {
                            throw new ArgumentOutOfRangeException(
                                string.Format("Entity {0} not found in data structure {1}.",
                                    query.Entity, ds.Path));
                        }
                    }

					if(dataset == null)
					{
						dataset = this.GetDataset(ds, query.DefaultSetId);
					}
					break;

				case QueryDataSourceType.DataStructureEntity:
					entities = new ArrayList();
                    filter = this.GetFilterSet(query.MethodId);
                    entities.Add(this.GetDataStructureEntity(query));

					if(dataset == null)
					{
						throw new NullReferenceException(ResourceUtils.GetString("DataSetMustBeProvided"));
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("DataSourceType", query.DataSourceType, ResourceUtils.GetString("UnknownDataSource"));
			}

			IDictionary<DataColumn, string> expressions = DatasetTools.RemoveExpressions(dataset, true);

			if (dataset.Tables.Count > 1 && query.Paging)
			{
				throw new Exception("Paging is allowed only on data sturctures with a single entity.");
			}

			bool enforceConstraints = dataset.EnforceConstraints;

			dataset.EnforceConstraints = false;

			ArrayList threads = new ArrayList(entities.Count);

			foreach(DataStructureEntity entity in entities)
			{
				if (LoadWillReturnZeroResults(dataset, entity, query.DataSourceType)) continue;
				// Skip self joins, they are just relations, not really entities
				if(entity.Columns.Count > 0 & !(entity.Entity is IAssociation && (entity.Entity as IAssociation).IsSelfJoin))
				{
					profiler.ExecuteAndRememberLoadDuration(
						entity: entity,
						actionToExecute: () =>
						{
							DataLoader loader = new DataLoader();
							loader.ConnectionString = _connectionString;
							loader.DataService = this;
							loader.Dataset = dataset;
							loader.TransactionId = transactionId;
							if (transactionId != null)
							{
								loader.Transaction =
									this.GetTransaction(transactionId,
										query.IsolationLevel);
							}
							loader.Ds = ds;
							loader.Entity = entity;
							loader.Filter = filter;
							loader.SortSet = sortSet;
							loader.Query = query;
							loader.Timeout = timeout;
							loader.CurrentProfile = currentProfile;
							loader.Fill();
						});
				}
			}

			if(query.EnforceConstraints)
			{
				try
				{
					dataset.EnforceConstraints = enforceConstraints;
				}
				catch(Exception ex)
				{
					try
					{
						log.LogOrigamError(DebugClass.ListRowErrors(dataset), ex);
						using(System.IO.StreamWriter w = System.IO.File.CreateText(AppDomain.CurrentDomain.BaseDirectory + @"\debug\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-") + DateTime.Now.Ticks.ToString() + "___" + "MsSqlDataService_error.txt"))
						{
							w.WriteLine(DebugClass.ListRowErrors(dataset));
							w.Close();
						}
					}
					catch{}
					throw;
				}
			}
			DatasetTools.SetExpressions(expressions);
			return dataset;
		}
		
		private bool LoadWillReturnZeroResults(DataSet dataset,
			DataStructureEntity entity, QueryDataSourceType dataSourceType)
		{
			DataStructureEntity rootEntity = entity.RootEntity;
		    if (dataSourceType == QueryDataSourceType.DataStructureEntity) return false;
			if (rootEntity == entity) return false;
			if (entity.RelationType != RelationType.Normal) return false;
			if (!(rootEntity.Entity is TableMappingItem mappingItem)) return false;

            string rootEntityTableName = mappingItem.MappedObjectName;
			DataTable rootTable = dataset.Tables
				.Cast<DataTable>()
				.FirstOrDefault(table => table.TableName == rootEntityTableName);
				
			return rootTable != null && rootTable.Rows.Count == 0;
		}

		public override int UpdateData(
            DataStructureQuery query, IPrincipal userProfile, DataSet dataset, 
            string transactionId)
        {
            return UpdateData(
                query, userProfile, dataset, transactionId, false);
        }

		public override int UpdateData(
            DataStructureQuery query, IPrincipal userProfile, DataSet dataset, 
            string transactionId, bool forceBulkInsert)
		{
			if (log.IsDebugEnabled)
			{
				log.RunHandled(() =>
				{
					log.Debug("UpdateData; Data Structure Id: " + query.DataSourceId.ToString());
					StringBuilder sb = new StringBuilder();
					System.IO.StringWriter sw = new System.IO.StringWriter(sb);
					dataset.WriteXml(sw, XmlWriteMode.DiffGram);
					log.Debug("UpdateData; " + sb.ToString());
				});
			}
			bool newTransaction = (transactionId == null);
			if(transactionId == null) transactionId = Guid.NewGuid().ToString();
			// If there is nothing to update, we quit immediately
			if(!dataset.HasChanges())
            {
                return 0;
            }
            UserProfile profile = null;
			if(query.LoadByIdentity)
			{
				profile = SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity) as UserProfile;
			}
            IStateMachineService stateMachine = this.StateMachine;
			DataSet changedDataset = dataset;//.GetChanges();
			DataStructure ds = this.GetDataStructure(query);
			if (ds.IsLocalized)
			{
				throw new OrigamException(String.Format("Couldn't update localized datastructure `{0}' ({1})", ds.Name, ds.Id));
			}
			IDbTransaction transaction = GetTransaction(transactionId, query.IsolationLevel);
			IDbConnection connection = transaction.Connection;
			string currentEntityName = "";
			string lastTableName = "";
			ArrayList entities = ds.Entities;
			ArrayList changedTables = new ArrayList();
            ArrayList deletedRowIds = new ArrayList();
			DataRowState[] rowStates = new DataRowState[]{DataRowState.Deleted, DataRowState.Added, DataRowState.Modified};
			DataTable changedTable = null;
			try
			{
				foreach(DataRowState rowState in rowStates)
				{
					ArrayList actualEntities = entities;
					// for delete reverse entity order
					if(rowState == DataRowState.Deleted)
					{
						actualEntities = new ArrayList(entities.Count);
						for(int i = entities.Count - 1; i >= 0; i--)
						{
							actualEntities.Add(entities[i]);
						}
					}
					foreach(DataStructureEntity entity in actualEntities)
					{
						currentEntityName = entity.Name;
						TableMappingItem tableMapping = entity.EntityDefinition as TableMappingItem;
						if (tableMapping != null)
						{
							// We check if the dataset actually contains the entity.
							// E.g. for self-joins the entity name is not contained in the dataset
							// but that does not matter because we save such an entity once anyway.
							if (changedDataset.Tables.Contains(entity.Name))
							{
								lastTableName = dataset.Tables[currentEntityName].DisplayExpression
									.Replace("'", "");
								if (lastTableName == "") lastTableName = currentEntityName;
								// we clone the table because if without complete dataset, some expresssions
								// might not work if they reference other entities
								changedTable =
									DatasetTools.CloneTable(changedDataset.Tables[entity.Name],
										false);
								foreach (DataRow row in changedDataset.Tables[entity.Name].Rows)
								{
									if (row.RowState == rowState)
									{
										// Constraints might fail right here if the source dataset
										// has constraints turned off, e.g. through a flag in a 
										// sequential workflow. That is OK. Otherwise they would
										// fail in the database.
										changedTable.ImportRow(row);
									}
								}

								if (tableMapping.DatabaseObjectType ==
								    DatabaseMappingObjectType.Table)
								{
									if (changedTable != null)
									{
										if (stateMachine != null && query.FireStateMachineEvents)
										{
											stateMachine.OnDataChanging(changedTable, transactionId);
										}
									}
									int rowCount = changedTable.Rows.Count;
									if (rowCount > 0)
									{
										profiler.ExecuteAndLogStoreActionDuration(
											entity: entity,
											actionToExecute: () =>
										{
											ExecuteUpdate(query, transactionId, profile,
												ds, transaction, connection,
												deletedRowIds, 
	                                            changedTable,
												rowState, entity, rowCount, 
	                                            forceBulkInsert);
										});
									}
								}

								// finally add the table to a list of changed tables so 
								// changes can be accepted later
								if (!changedTables.Contains(changedTable.TableName))
								{
									changedTables.Add(changedTable.TableName);
								}
							}
						}						
					}
				}
				// execute the state machine events
				if(stateMachine != null && query.FireStateMachineEvents)
				{
					stateMachine.OnDataChanged(dataset, changedTables, transactionId);
				}
				// accept changes
				this.AcceptChanges(dataset, changedTables, query, transactionId, userProfile);
				profiler.LogRememberedExecutionTimes();
                // delete attachments if any
                if (deletedRowIds.Count > 0 && query.SynchronizeAttachmentsOnDelete)
                {
                    IAttachmentService attachmentService = ServiceManager.Services.GetService(typeof(IAttachmentService)) as IAttachmentService;
                    foreach (Guid recordId in deletedRowIds)
                    {
                        attachmentService.RemoveAttachment(recordId, transactionId);
                    }
                }
				if(newTransaction)
				{
					ResourceMonitor.Commit(transactionId);
				}
			}
			catch(DBConcurrencyException e)
            {
                if (log.IsErrorEnabled)
                {
                    // Once has happened that a concurrency exception
                    // has been logged only from this place and not from higher place in
                    // in the place (from callers). So there is obviously some place,
                    // where this exception isn't caught. To identify such a place,
                    // full stack trace is printed here.
                    log.Error("DBConcurrencyException occurred! See \"Origam.DA.Service.ConcurrencyExceptionLogger\" logger (Debug mode) for more details");
                }
                if (concurrencyLog.IsDebugEnabled)
                {
                    concurrencyLog.DebugFormat(
                        "Concurrency exception data structure query details: ds: `{0}', method: `{1}', sortSet: `{2}', default set: `{3}' ",
                        query.DataSourceId,
                        query.MethodId,
                        query.SortSetId,
                        query.DefaultSetId);
                }
                if (newTransaction)
                {
                    ResourceMonitor.Rollback(transactionId);
                    transactionId = null;
                }
                string errorString = ComposeConcurrencyErrorMessage(userProfile, dataset,
                    transactionId, currentEntityName, lastTableName, e);
                // log before throw (because there are some place(s) where the exception isn't caught
                if (concurrencyLog.IsDebugEnabled)
                {
                    concurrencyLog.DebugFormat(
                        "Concurrency exception details: {0}", errorString);
                }
                throw new DBConcurrencyException(errorString, e);
            }
            catch (Exception e)
            {
                if (log.IsErrorEnabled)
                {
	                log.LogOrigamError("Update failed", e);
                }
                if (newTransaction)
                {
                    ResourceMonitor.Rollback(transactionId);
                }
                ComposeGeneralErrorMessage(lastTableName, changedTable, e);
                throw;
            }
            finally
			{
				if(transactionId == null)
				{
					connection.Close();
					connection.Dispose();
				}
			}
			return 0;
		}

		private void ComposeGeneralErrorMessage(string lastTableName, DataTable changedTable, Exception e)
        {
            string rowErrorMessage = null;
            DataRow errorRow = DatasetTools.GetErrorRow(changedTable);
            if (errorRow != null)
            {
                string operation = "";
                switch (errorRow.RowState)
                {
                    case DataRowState.Added:
                        operation = ResourceUtils.GetString("ErrorCouldNotAddRow");
                        break;
                    case DataRowState.Deleted:
                        operation = ResourceUtils.GetString("ErrorCouldNotDeleteRow");
                        break;
                    case DataRowState.Modified:
                        operation = ResourceUtils.GetString("ErrorCouldNotModifyRow");
                        break;
                }

                string recordDescription = DatasetTools.GetRowDescription(errorRow);
                if (recordDescription != null) recordDescription = " " + recordDescription;

                rowErrorMessage = string.Format(operation, lastTableName, recordDescription);

                this.HandleException(e, rowErrorMessage, errorRow);
            }
        }

        private string ComposeConcurrencyErrorMessage(IPrincipal userProfile, 
            DataSet dataset, string transactionId, string currentEntityName, 
            string lastTableName, DBConcurrencyException e)
        {
            string concurrentUserName = "";
            string errorString = "";
            try
            {
                DataTable table = dataset.Tables[currentEntityName];
                DataRow row = table.Rows.Find(DatasetTools.PrimaryKey(e.Row));
                // if the row in the queue is being deleted we will not find it in the original data
                // in that case we will use the row provided by the event which contains the data
                if (row == null) row = e.Row;
                // row.RowError = ResourceUtils.GetString("DataChangedByOtherUser");

                // load the existing row from the database to see what changes have been made by the other user
                DataSet storedData = CloneDatasetForActualRow(table);
                Guid entityId = (Guid)table.ExtendedProperties["Id"];
                DataStructureEntity entity = GetEntity(entityId);
                string rowName = DatasetTools.PrimaryKey(row)[0].ToString();
                IDataEntityColumn describingField = entity.EntityDefinition.DescribingField;
                if (describingField != null && table.Columns.Contains(describingField.Name))
                {
                    if (!row.IsNull(describingField.Name))
                    {
                        rowName = row[describingField.Name].ToString();
                    }
                }
                LoadActualRow(storedData, entityId, Guid.Empty, row, userProfile, transactionId);
                DataTable storedTable = storedData.Tables[currentEntityName];
                if (storedTable.Rows.Count == 0)
                {
                    errorString = ResourceUtils.GetString("DataDeletedByOtherUserException", rowName, lastTableName);
                }
                else
                {
                    DataRow storedRow = storedTable.Rows[0];

                    if (storedTable.Columns.Contains("RecordUpdatedBy") && !storedRow.IsNull("RecordUpdatedBy"))
                    {
                        UserProfile concurrentProfile = SecurityManager.GetProfileProvider().GetProfile((Guid)storedRow["RecordUpdatedBy"]) as UserProfile;
                        concurrentUserName = concurrentProfile.FullName;
                    }
                    errorString = ResourceUtils.GetString("DataChangedByOtherUserException", rowName, lastTableName, concurrentUserName);
                    foreach (DataColumn col in row.Table.Columns)
                    {
                        IDataEntityColumn field = GetField((Guid)col.ExtendedProperties["Id"]);
                        if (col.ColumnName != "RecordUpdatedBy" && field is FieldMappingItem)
                        {
                            string storedValue = "";
                            string myValue = "";
                            if (!storedRow.IsNull(col.ColumnName)) storedValue = storedRow[col.ColumnName].ToString();
                            if (!row.IsNull(col, DataRowVersion.Original)) myValue = row[col, DataRowVersion.Original].ToString();

                            if (!storedValue.Equals(myValue))
                            {
                                if (col.ExtendedProperties.Contains(Const.DefaultLookupIdAttribute))
                                {
                                    IDataLookupService lookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
                                    // this is a lookup column, we pass the looked-up value
                                    Guid lookupId = (Guid)col.ExtendedProperties[Const.DefaultLookupIdAttribute];
                                    if (myValue != null && myValue != "")
                                    {
                                        myValue = lookupService.GetDisplayText(lookupId, myValue, transactionId).ToString();
                                    }
                                    if (storedValue != null && storedValue != "")
                                    {
                                        storedValue = lookupService.GetDisplayText(lookupId, storedValue, transactionId).ToString();
                                    }
                                }

                                errorString += Environment.NewLine
                                    + "- "
                                    + col.Caption
                                    + ": "
                                    + myValue
                                    + " > "
                                    + storedValue;
                            }
                        }
                    }
                }

                while (row.Table.ParentRelations.Count > 0)
                {
                    row = row.GetParentRow(row.Table.ParentRelations[0]);
                    row.RowError = ResourceUtils.GetString("ChildErrors");
                }
            }
            catch { }

            return errorString;
        }

        private void ExecuteUpdate(
            DataStructureQuery query, string transactionId, 
            UserProfile profile, DataStructure ds, 
            IDbTransaction transaction, IDbConnection connection, 
			ArrayList deletedRowIds, 
            DataTable changedTable, DataRowState rowState, 
            DataStructureEntity entity, int rowCount,
            bool forceBulkInsert)
        {
            // LOGGING
            LogData(changedTable, profile, transactionId, connection, transaction);
            if ((forceBulkInsert || ((BulkInsertThreshold != 0)
            && (rowCount > BulkInsertThreshold)))
            && (rowState == DataRowState.Added))
            {
                BulkInsert(entity, connection,
                    transaction, changedTable);
                if (log.IsInfoEnabled)
                {
                    log.Info("BulkCopy; Entity: "
                        + changedTable?.TableName
                        + ", " + rowState.ToString()
                        + " " + rowCount.ToString()
                        + " row(s). Transaction id: "
                        + transactionId);
                }
            }
            else
            {
                DataStructureFilterSet filter = GetFilterSet(query.MethodId);
                DataStructureSortSet sortSet = GetSortSet(query.SortSetId);
                // CONFIGURE DATA ADAPTER
                var adapterParameters = new SelectParameters
                {
                    DataStructure = ds,
                    Entity = entity,
                    Filter = filter,
                    SortSet = sortSet,
                    Parameters = query.Parameters.ToHashtable(),
                    Paging = false
                };
                DbDataAdapter adapter = this.GetAdapter(adapterParameters, profile);
                SetConnection(adapter, connection);
                SetTransaction(adapter, transaction);
                if (UpdateBatchSize != 0)
                {
                    adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                    adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                    adapter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                    adapter.UpdateBatchSize = UpdateBatchSize;
                }
                // EXECUTE THE UPDATE
                if (rowState == DataRowState.Modified) this.TraceCommand(((IDbDataAdapter)adapter).UpdateCommand, transactionId);
                if (rowState == DataRowState.Deleted)
                {
                    this.TraceCommand(((IDbDataAdapter)adapter).DeleteCommand, transactionId);
                    // remember row in order to delete an attachment later at the end of updateData
                    /* Key pk = entity.PrimaryKey;
                        if (pk.Count == 1)
                        */
                    if (changedTable.PrimaryKey.Length == 1 && changedTable.PrimaryKey[0].DataType == typeof(Guid))
                    {
                        // entity has a primary key Id taken from IOrigamEntity2
                        foreach (DataRow r in changedTable.Rows)
                        {
                            deletedRowIds.Add(r[changedTable.PrimaryKey[0].ColumnName, DataRowVersion.Original]);
                        }
                    }
                }
                if (rowState == DataRowState.Added) this.TraceCommand(((IDbDataAdapter)adapter).InsertCommand, transactionId);
                int result = adapter.Update(changedTable);
                if (log.IsInfoEnabled)
                {
                    log.Info("UpdateData; Entity: " + changedTable.TableName + ", " + rowState.ToString() + " " + result.ToString() + " row(s). Transaction id: " + transactionId);
                }
                // FREE THE ADAPTER
                SetTransaction(adapter, null);
                SetConnection(adapter, null);
                // CHECK CONCURRENCY VIOLATION
                if (result != rowCount)
                {
                    throw new DBConcurrencyException(ResourceUtils.GetString("ConcurrencyViolation", changedTable.TableName));
                }
            }
        }

        internal virtual void BulkInsert(
            DataStructureEntity entity,
            IDbConnection connection,
            IDbTransaction transaction,
            DataTable table)
        {
            throw new NotImplementedException();
        }

		private Hashtable GetScalarCommandCache()
		{
			Hashtable context = OrigamUserContext.Context;
            lock (context)
            {
                if (!context.Contains("ScalarCommandCache"))
                {
                    context.Add("ScalarCommandCache", new Hashtable());
                }
            }
			return (Hashtable)OrigamUserContext.Context["ScalarCommandCache"];
		}


		public override object GetScalarValue(DataStructureQuery query, ColumnsInfo columnsInfo, IPrincipal principal, string transactionId)
		{
			IDbCommand command;
			object result = null;

			UserProfile currentProfile = null;
			if(query.LoadByIdentity)
			{
				currentProfile = SecurityManager.GetProfileProvider().GetProfile(principal.Identity) as UserProfile;
			}

			DataStructure ds = this.GetDataStructure(query);

			string cacheId = query.DataSourceId.ToString() + query.MethodId.ToString() + query.SortSetId.ToString() + columnsInfo;
			Hashtable cache = GetScalarCommandCache();

			if(cache.Contains(cacheId))
			{
				command = (IDbCommand)cache[cacheId];
				command = this.DbDataAdapterFactory.CloneCommand(command);
			}
			else
			{
				lock(cache)
				{
					command = this.DbDataAdapterFactory.ScalarValueCommand(
						ds,
						this.GetFilterSet(query.MethodId),
						this.GetSortSet(query.SortSetId),
						columnsInfo,
						query.Parameters.ToHashtable()
						);
					cache[cacheId] = command;
				}
			}

			IDbTransaction transaction = null;
			IDbConnection connection;
			
			if(transactionId == null)
			{
				connection = GetConnection(this.ConnectionString);
				connection.Open();
			}
			else
			{
				transaction = this.GetTransaction(transactionId, query.IsolationLevel);
				connection = transaction.Connection;
			}

			try
			{
				this.BuildParameters(query.Parameters, command.Parameters, currentProfile);
				command.Connection = connection;
				//					SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
				command.Transaction = transaction;

				this.TraceCommand(command, transactionId);

				result = command.ExecuteScalar();

				OrigamDataType dataType = OrigamDataType.Xml;
				foreach(DataStructureColumn col in (ds.Entities[0] as DataStructureEntity).Columns)
				{
					if(col.Name == columnsInfo?.ToString())
					{
						DataStructureColumn finalColumn = col.FinalColumn;

						if(col.Aggregation == AggregationType.Count)
						{
							dataType = OrigamDataType.Long;
						}
						else
						{
							dataType = finalColumn.DataType;
						}
						break;
					}
				}

				switch(dataType)
				{
					case OrigamDataType.UniqueIdentifier:
						if(result is string)
						{
							result = new Guid((string)result);
						}
						break;
					case OrigamDataType.Boolean:
						if(result is int) 
						{
							result = (result.Equals(1));
						}
						break;
                    case OrigamDataType.Long:
                        if(result is int)
                        {
                            result = (long)(int)result;
                        }
                        break;
				}

				//					// Reset the transaction isolation level to its default. See the following from MSDN:
				//					// =====================================================================================
				//					// Note   After a transaction is committed or rolled back, the isolation level 
				//					// of the transaction persists for all subsequent commands that are in autocommit mode 
				//					// (the Microsoft SQL Server default). This can produce unexpected results, 
				//					// such as an isolation level of Repeatable read persisting and locking other users out 
				//					// of a row. To reset the isolation level to the default (Read committed), execute 
				//					// the Transact-SQL SET TRANSACTION ISOLATION LEVEL READ COMMITTED statement, 
				//					// or call SqlConnection.BeginTransaction followed immediately by SqlTransaction.Commit. 
				//					// For more information about isolation levels, see SQL Server Books Online.
				//					
				//					transaction = connection.BeginTransaction();
				//					transaction.Commit();
				//					transaction.Dispose();
				
				ResetTransactionIsolationLevel(command);
			}
			catch(Exception e)
			{
				throw new DataException(ResourceUtils.GetString("ErrorWhenScalar", Environment.NewLine + e.Message), e);
			}
			finally
			{
				if(transactionId == null)
				{
					try
					{
						connection.Close();
					}
					finally
					{
						connection.Dispose();
					}
				}
			}

			return result;
		}

        protected abstract void ResetTransactionIsolationLevel(IDbCommand command);

        public override DataSet ExecuteProcedure(string name, string entityOrder, DataStructureQuery query, string transactionId)
		{
			OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;

			DataStructure ds = GetDataStructure(query);
			DataSet result = null;
			
			if(ds != null)
			{
				result = GetDataset(ds, Guid.Empty);
			}

			IDbTransaction transaction = null;
			IDbConnection connection;
			
			if(transactionId == null)
			{
				connection = GetConnection(this.ConnectionString);
				connection.Open();
			}
			else
			{
				transaction = this.GetTransaction(transactionId, query.IsolationLevel);
				connection = transaction.Connection;
			}

			try
			{
				DbDataAdapter adapter = null;
				IDbCommand cmd = null;
				UserProfile profile = null;
				if(query.LoadByIdentity)
				{
                    profile = SecurityManager.CurrentUserProfile();
                }

				try
				{
					// no output data structure - no results - execute non-query
					if(result == null)
					{
						cmd = this.DbDataAdapterFactory.GetCommand(name, connection);
						cmd.Transaction = transaction;
						cmd.CommandTimeout = settings.DataServiceExecuteProcedureTimeout;
						foreach(QueryParameter param in query.Parameters)
						{
							if(param.Value != null)
							{
								IDbDataParameter dbParam = this.DbDataAdapterFactory.GetParameter(param.Name, param.Value.GetType());
								dbParam.Value = param.Value;
								cmd.Parameters.Add(dbParam);
							}
						}
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Prepare();
						cmd.ExecuteNonQuery();
					}
					// results present - we take the first entity of the output data structure and fill the results into it
					else
					{
						// make a sorted list of entities that corresponds to an output
						// from SP
						ArrayList entitiesOrdered;
						if (entityOrder == null)
						{
							entitiesOrdered = ds.Entities;
						}
						else
						{
							entitiesOrdered = new ArrayList();
							foreach (string entityName in entityOrder.Split(';'))
							{
								bool found = false;
								foreach (DataStructureEntity dse in ds.Entities)
								{
									if (dse.Name == entityName)
									{
										entitiesOrdered.Add(dse);
										found = true;
										break;
									}
								}
								if (!found)
								{
									throw new ArgumentException(String.Format(
										"Entity `{0}' defined in EntityOrder `{1}'"
									    + "not found in a datastructure {2}",
									   entityName, entityOrder, ds.Id.ToString()));
								}
							}
						}

						adapter = this.DbDataAdapterFactory.CreateDataAdapter(
							name, entitiesOrdered, connection, transaction);
						cmd = ((IDbDataAdapter)adapter).SelectCommand;
						cmd.CommandTimeout = settings.DataServiceExecuteProcedureTimeout;
						BuildParameters(query.Parameters, cmd.Parameters, profile);

						try {
							adapter.Fill(result);
						}
						catch (ConstraintException)
						{
							// make the exception far more verbose
							throw new ConstraintException(DatasetTools.GetDatasetErrors(result));
						}
					}
				}
				finally
				{
					IDisposable dispCmd = cmd as IDisposable;
					if(dispCmd != null) dispCmd.Dispose();
					IDisposable disp = adapter as IDisposable;
					if(disp != null) disp.Dispose();
				}
			}
			 catch(Exception e)
			{
				if(log.IsErrorEnabled)
				{
					log.LogOrigamError("Stored Procedure Call failed", e);
				}

				throw;
			}
			finally
			{
				if(transactionId == null)
				{
					try
					{
						connection.Close();
					}
					finally
					{
						connection.Dispose();
					}
				}
			}

			return result;
		}

		public override int UpdateField(Guid entityId, Guid fieldId, object oldValue, object newValue, IPrincipal userProfile, string transactionId)
		{
			int result = 0;
			UserProfile profile = SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity) as UserProfile;
			IDbTransaction transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
			IDbConnection connection = transaction.Connection;

			try
			{
				TableMappingItem table = this.GetTable(entityId);
				
				if(table.DatabaseObjectType != DatabaseMappingObjectType.Table)
				{
					throw new ArgumentOutOfRangeException("DatabaseObjectType", table.DatabaseObjectType, ResourceUtils.GetString("UpdateFieldUpdate"));
				}

				FieldMappingItem field = this.GetTableColumn(fieldId);

				// get data for audit log
				DatasetGenerator dg = new DatasetGenerator(true);
				DataSet ds = dg.CreateUpdateFieldDataSet(table, field);
				DataTable dt = ds.Tables[table.Name];

				DbDataAdapter adapter = this.DbDataAdapterFactory.CreateUpdateFieldDataAdapter(table, field);
				((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
				((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
				((IDbDataAdapter)adapter).SelectCommand.CommandTimeout = 0;

				QueryParameterCollection logParameters = new QueryParameterCollection();
				logParameters.Add(new QueryParameter(field.Name, oldValue));

				this.BuildParameters(logParameters, ((IDbDataAdapter)adapter).SelectCommand.Parameters, profile);
				dt.BeginLoadData();
				this.TraceCommand(((IDbDataAdapter)adapter).SelectCommand, transactionId);
				adapter.Fill(ds);
				dt.EndLoadData();

				if(dt.Rows.Count > 0)
				{
					foreach(DataRow row in dt.Rows)
					{
						row[field.Name] = newValue;
					}

					LogData(dt, profile, transactionId, connection, transaction, 32);

					using(IDbCommand cmd = this.DbDataAdapterFactory.UpdateFieldCommand(table, field))
					{
						cmd.Connection = connection;
						cmd.Transaction = transaction;
					
						QueryParameterCollection parameters = new QueryParameterCollection();
						parameters.Add(new QueryParameter("oldValue", oldValue));
						parameters.Add(new QueryParameter("newValue", newValue));

						this.BuildParameters(parameters, cmd.Parameters, profile);

						cmd.CommandTimeout = 0;
						result = cmd.ExecuteNonQuery();
					}
				}

				return result;
			}
			catch
			{
				if(transactionId == null)
				{
					transaction.Rollback();
				}
				throw;
			}
			finally
			{
				if(transactionId == null)
				{
					connection.Close();
					connection.Dispose();
					transaction.Dispose();
				}
			}
		}

		public override int ReferenceCount(Guid entityId, Guid fieldId, object value, IPrincipal userProfile, string transactionId)
		{
			UserProfile profile = SecurityManager.GetProfileProvider().GetProfile(userProfile.Identity) as UserProfile;
			IDbTransaction transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
			IDbConnection connection = transaction.Connection;

			try
			{
				TableMappingItem table = this.GetTable(entityId);
				
				if(table.DatabaseObjectType != DatabaseMappingObjectType.Table)
				{
					throw new ArgumentOutOfRangeException("DatabaseObjectType", table.DatabaseObjectType, ResourceUtils.GetString("UpdateFieldUpdate"));
				}

				FieldMappingItem field = this.GetTableColumn(fieldId);

				IDbCommand cmd = this.DbDataAdapterFactory.SelectReferenceCountCommand(table, field);
				cmd.Connection = connection;
				cmd.Transaction = transaction;
				cmd.CommandTimeout = 0;

				QueryParameterCollection parameters = new QueryParameterCollection();
				parameters.Add(new QueryParameter(field.Name, value));

				this.BuildParameters(parameters, cmd.Parameters, profile);
				this.TraceCommand(cmd, transactionId);
				int result = (int)cmd.ExecuteScalar();

				return result;
			}
			finally
			{
				if(transactionId == null)
				{
					connection.Close();
					connection.Dispose();
					transaction.Dispose();
				}
			}
		}

        /// <summary>
        /// Executes an arbitrary SQL command on the database.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="transactionId">Existing transaction id. IMPORTANT: if transactionId is NULL the command runs without a transaction!</param>
        /// <returns></returns>
        public override string ExecuteUpdate(string command, string transactionId)
		{
			IDbTransaction transaction = null;
			IDbConnection connection;
            if(transactionId == null)
            {
                connection = GetConnection(this.ConnectionString);
                connection.Open();
            }
            else
            {
                transaction = GetTransaction(transactionId, IsolationLevel.ReadCommitted);
                connection = transaction.Connection;
            }
            try
            {
                DataSet dataset = new DataSet();
                int records = 0;
                using (IDbCommand cmd = this.DbDataAdapterFactory.GetCommand(command, connection, transaction))
                {
                    DbDataAdapter adapter = DbDataAdapterFactory.GetAdapter(cmd);
                    cmd.CommandTimeout = 0;
                    records = adapter.Fill(dataset);
                }
                StringBuilder builder = FormatResults(dataset);
                string recordsText;
                if (records == 1 )
                {
                    recordsText = "record";
                }
                else
                {
                    recordsText = "records";
                }
                builder.AppendLine(records.ToString() + " " + recordsText + " affected.");
                return builder.ToString();
            }
            finally
			{
				if(transactionId == null)
				{
					connection.Close();
					connection.Dispose();
				}
			}
		}

        private static StringBuilder FormatResults(DataSet dataset)
        {
            StringBuilder builder = new StringBuilder();
            foreach (DataTable table in dataset.Tables)
            {
                foreach (DataColumn column in table.Columns)
                {
                    builder.Append(column.ColumnName.PadRight(
                         GetLength(column), ' ') + " ");
                }
                builder.AppendLine();
                foreach (DataColumn column in table.Columns)
                {
                    builder.Append(new string('-',
                         GetLength(column)) + " ");
                }
                builder.AppendLine();
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        string value;
                        if (row.IsNull(col))
                        {
                            value = "NULL";
                        }
                        else
                        {
                            value = row[col].ToString();
                            if (value.Length > DATA_VISUALIZATION_MAX_LENGTH)
                            {
                                value = value.Substring(0, DATA_VISUALIZATION_MAX_LENGTH);
                            }
                        }
                        builder.Append(value.PadRight(
                             GetLength(col)) + " ");
                    }
                    builder.AppendLine();
                }
                builder.AppendLine();
            }
            return builder;
        }

        private static int GetLength(DataColumn column)
        {
            int nameLength = column.ColumnName.Length + 1;
            int dataLength;
            if (column.DataType == typeof(string))
            {
                dataLength = column.MaxLength;
                if (dataLength > DATA_VISUALIZATION_MAX_LENGTH)
                {
                    dataLength = DATA_VISUALIZATION_MAX_LENGTH;
                }
            }
            else if (column.DataType == typeof(Guid))
            {
                dataLength = 36;
            }
            else if (column.DataType == typeof(DateTime))
            {
                dataLength = DateTime.MaxValue.ToString().Length;
            }
            else
            {
                dataLength = 10;
            }
            if (nameLength > dataLength)
            {
                return nameLength;
            }
            else
            {
                return dataLength;
            }
        }

        private void LogData(DataTable changedTable, UserProfile profile, string transactionId, 
            IDbConnection connection, IDbTransaction transaction)
		{
			LogData(changedTable, profile, transactionId, connection, transaction, -1);
		}

		private void LogData(DataTable changedTable, UserProfile profile, string transactionId, 
            IDbConnection connection, IDbTransaction transaction, int overrideActionType)
		{
			DataAuditLog log = GetLog(changedTable, profile, transactionId, overrideActionType);
			if(log != null && log.AuditRecord.Count > 0)
			{
				ISchemaService schemaService = ServiceManager.Services.
                    GetService(typeof(ISchemaService)) as ISchemaService;

				DataStructure logDataStructure = this.GetDataStructure(
                    new Guid("530eba45-40db-470d-8e53-8b98ace758ad"));

			    var adapterParameters = new SelectParameters
			    {
			        DataStructure = logDataStructure,
			        Entity = (DataStructureEntity)logDataStructure.Entities[0],
			        Parameters = new Hashtable(),
			        Paging = false,
			    };
                DbDataAdapter logAdapter = this.GetAdapter(adapterParameters, profile);
				SetConnection(logAdapter, connection);
				SetTransaction(logAdapter, transaction);

				logAdapter.Update(log);

				SetTransaction(logAdapter, null);
				SetConnection(logAdapter, null);
			}
		}

		public override string DatabaseSchemaVersion()
		{
			try
			{
			    OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;

			    string result;

			    using(IDbConnection connection = GetConnection(_connectionString))
			    {
				    connection.Open();
				    result = RunSchemaVersionQuery(connection, settings);
			    }

			    return result;
			}
			catch(Exception ex)
			{
                HandleException(ex, ex.Message, null);
                throw;
			}
		}

		private string RunSchemaVersionQuery(IDbConnection connection,
			OrigamSettings settings)
		{
			try
			{
				return TryGetSchemaVersion(
					connection: connection,
					settings: settings,
					versionCommandName: "OrigamDatabaseSchemaVersion");
			} 
			catch 
			{
				return TryGetSchemaVersion(
					connection: connection,
					settings: settings, 
					versionCommandName: "AsapDatabaseSchemaVersion"); // pre 5 version would have the command named like this
			}
		}

		private string TryGetSchemaVersion(IDbConnection connection,
			OrigamSettings settings, string versionCommandName)
		{
			using (IDbCommand cmd =
				this.DbDataAdapterFactory.GetCommand(versionCommandName,
					connection))
			{
				cmd.CommandTimeout = settings.DataServiceExecuteProcedureTimeout;
				cmd.CommandType = CommandType.StoredProcedure;

				return (string) cmd.ExecuteScalar();
			}
		}
        public override string EntityDdl(Guid entityId)
        {
            TableMappingItem table = PersistenceProvider.RetrieveInstance(
                typeof(TableMappingItem), new ModelElementKey(entityId)) as TableMappingItem;
            if (table == null)
            {
                throw new ArgumentOutOfRangeException("entityId", entityId, "Element is not a table mapping.");
            }
            return DbDataAdapterFactory.TableDefinitionDdl(table);
        }

        public override string[] FieldDdl(Guid fieldId)
        {
            string[] result = new string[2];
            FieldMappingItem column = PersistenceProvider.RetrieveInstance(
                typeof(FieldMappingItem), new ModelElementKey(fieldId)) as FieldMappingItem;
            if (column == null)
            {
                throw new ArgumentOutOfRangeException("fieldId", fieldId, "Element is not a column mapping.");
            }
            result[0] = DbDataAdapterFactory.AddColumnDdl(column);
            result[1] = DbDataAdapterFactory.AddForeignKeyConstraintDdl(column.ParentItem as TableMappingItem, column.ForeignKeyConstraint);
            return result;
        }
        public override IDataReader ExecuteDataReader(DataStructureQuery query,
            IPrincipal principal, string transactionId)
        {
            DataSet dataSet = null;
            OrigamSettings settings 
                = ConfigurationManager.GetActiveConfiguration() ;
            int timeout = settings.DataServiceSelectTimeout;
            UserProfile currentProfile = null;
            if(query.LoadByIdentity)
            {
                currentProfile = SecurityManager.GetProfileProvider().GetProfile(
                    principal.Identity) as UserProfile;
            }
            if(PersistenceProvider == null)
            {
                throw new NullReferenceException(
                    ResourceUtils.GetString("NoProviderForMS"));
            }

            DataStructure dataStructure = GetDataStructure(query);
            DataStructureFilterSet filterSet = GetFilterSet(query.MethodId);
            DataStructureSortSet sortSet = GetSortSet(query.SortSetId);
            DataStructureEntity entity = GetEntity(query, dataStructure);
            dataSet = GetDataset(dataStructure, query.DefaultSetId);
            DatasetTools.RemoveExpressions(dataSet, true);

			IDbConnection connection = null;
            IDbTransaction transaction = null;
            CommandBehavior commandBehavior = CommandBehavior.Default;
            if (transactionId != null)
            {
                transaction = GetTransaction(
                    transactionId, query.IsolationLevel);
            }
            if(transaction == null)
			{
				connection = GetConnection(_connectionString);
			    commandBehavior = CommandBehavior.CloseConnection;
            }
			else
			{
				connection = transaction.Connection;
			}
            var adapterParameters = new SelectParameters
            {
                DataStructure = dataStructure,
                Entity = entity,
                Filter=filterSet,
                SortSet = sortSet,
                Parameters = query.Parameters.ToHashtable(),
                Paging = query.Paging,
                ColumnsInfo = query.ColumnsInfo,
                CustomFilters = query.CustomFilters,
                CustomOrderings = query.CustomOrderings,
                CustomGrouping = query.CustomGrouping,
                RowLimit = query.RowLimit,
                RowOffset = query.RowOffset,
                ForceDatabaseCalculation = query.ForceDatabaseCalculation,
                AggregatedColumns = query.AggregatedColumns
            };
            DbDataAdapter adapter = GetAdapter(
                adapterParameters, currentProfile);
            ((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
            ((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
            ((IDbDataAdapter)adapter).SelectCommand.CommandTimeout = timeout;
            BuildParameters(query.Parameters, adapter.SelectCommand.Parameters, currentProfile);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return adapter.SelectCommand.ExecuteReader(commandBehavior);
        }
        
        public override IEnumerable<IEnumerable<object>> ExecuteDataReader(DataStructureQuery query)
        {
	        return ExecuteDataReaderInternal(query)
		        .Select(line
			        => line.Select(pair => pair.Value));
        }

        public override IEnumerable<Dictionary<string, object>> ExecuteDataReaderReturnPairs(DataStructureQuery query)
        {
	        return ExecuteDataReaderInternal(query)
		        .Select(line => ExpandAggregationData(line, query))
		        .Select( line=> line
			        .Where(pair => pair.Key != null)
			        .ToDictionary(
			        pair => pair.Key, 
			        pair => pair.Value));
        }

        private List<KeyValuePair<string, object>> ExpandAggregationData(
	        IEnumerable<KeyValuePair<string, object>> line, DataStructureQuery query)
        {
	        var processedItems = new List<KeyValuePair<string, object>>();
	        var aggregationData = new List<object>();
	        foreach (var pair in line)
	        {
		        var aggregatedColumn = query.AggregatedColumns
			        ?.FirstOrDefault(column => column.SqlQueryColumnName == pair.Key);
		        if (aggregatedColumn != null)
		        {
			        aggregationData.Add(
				        new
				        {
					        Column = aggregatedColumn.ColumnName,
					        Type = aggregatedColumn.AggregationType.ToString(),
					        Value = pair.Value
				        }
			        );
		        }
		        else
		        {
			        processedItems.Add(pair);
		        }
	        }
	        processedItems.Add(new KeyValuePair<string, object>("aggregations", aggregationData));
	        return processedItems;
        }

        private IEnumerable<T> ToEnumerable<T>(T item)
        {
	        yield return item;
        }

        private List<ColumnData> GetAllQueryColumns(DataStructureQuery query)
        {
	        return query.GetAllQueryColumns()
		        .SelectMany(column =>
		        {
			        if (!string.IsNullOrWhiteSpace(query.CustomGrouping?.GroupingUnit) &&
			            query.CustomGrouping?.GroupBy == column.Name)
			        {
				        return TimeGroupingRenderer
					        .GetColumnNames(column.Name, query.CustomGrouping.GroupingUnit)
					        .Select(columnName =>
						        new ColumnData(columnName, column.IsVirtual,
							        column.DefaultValue, column.HasRelation)
					        );
			        }
			        else
			        {
				        return ToEnumerable(column);
			        }

		        })
		        .ToList();
        }

        private IEnumerable<IEnumerable<KeyValuePair<string, object>>> ExecuteDataReaderInternal(DataStructureQuery query)
        {
	        using(IDataReader reader = ExecuteDataReader(
		        query, SecurityManager.CurrentPrincipal, null))
	        {
		        var queryColumns = GetAllQueryColumns(query);
		        while(reader.Read())
		        {
			        var values = new KeyValuePair<string, object>[queryColumns.Count];
			        for (int i = 0; i < queryColumns.Count; i++)
			        {
				        ColumnData queryColumn = queryColumns[i];
				        if (queryColumn.IsVirtual && !queryColumn.HasRelation)
				        {
					        continue;
				        }
				        object value = reader.GetValue(reader.GetOrdinal(queryColumn.Name));
				        values[i] = new KeyValuePair<string, object>(
					        queryColumn.Name , value);
			        }
			        yield return ProcessReaderOutput(
				        values, queryColumns);
		        }
	        }
        }
        
        private static List<KeyValuePair<string, object>> ProcessReaderOutput(KeyValuePair<string, object>[] values, List<ColumnData> columnData)
        {
	        if (columnData == null)
		        throw new ArgumentNullException(nameof(columnData));
	        var updatedValues = new List<KeyValuePair<string, object>>();

	        for (int i = 0; i < columnData.Count; i++)
	        {
		        if (columnData[i].IsVirtual)
		        {
			        if (columnData[i].HasRelation && values[i].Value != null && values[i].Value != DBNull.Value)
			        {
				        updatedValues.Add(new KeyValuePair<string, object>(
					        values[i].Key, ((string)values[i].Value).Split((char)1)));
				        continue;
			        }
			        else
			        {
				        updatedValues.Add(new KeyValuePair<string, object>
					        (values[i].Key, columnData[i].DefaultValue));
				        continue;
			        }
		        }
		        updatedValues.Add(new KeyValuePair<string, object>(
			        values[i].Key, values[i].Value));
	        }

	        return updatedValues;
        }
        

        private static DataStructureEntity GetEntity(DataStructureQuery query, DataStructure dataStructure)
	    {
	        DataStructureEntity entity;
	        switch (query.DataSourceType)
	        {
	            case QueryDataSourceType.DataStructure:
	                ArrayList entities;
	                if (string.IsNullOrEmpty(query.Entity))
	                {
	                    entities = dataStructure.Entities;
	                }
	                else
	                {
	                    entities = new ArrayList();
	                    foreach (DataStructureEntity e in dataStructure.Entities)
	                    {
	                        if (e.Name == query.Entity)
	                        {
	                            entities.Add(e);
	                            break;
	                        }
	                    }

	                    if (entities.Count == 0)
	                    {
	                        throw new ArgumentOutOfRangeException(
	                            string.Format(
	                                "Entity {0} not found in data structure {1}.",
	                                query.Entity, dataStructure.Path));
	                    }
	                }

	                entity = entities[0] as DataStructureEntity;
	                break;
	            default:
	                throw new ArgumentOutOfRangeException("DataSourceType",
	                    query.DataSourceType, ResourceUtils.GetString(
	                        "UnknownDataSource"));
	        }

	        return entity;
	    }

	    #endregion

        #region Is Schema Item in Database
        public override bool IsSchemaItemInDatabase(ISchemaItem schemaItem)
        {
            if (schemaItem is DataEntityIndex)
            {
                return IsDataEntityIndexInDatabase(schemaItem as DataEntityIndex);
            }
            return false;
        }

        internal abstract bool IsDataEntityIndexInDatabase(DataEntityIndex dataEntityIndex);
        

        #endregion

		#region Compare Schema
		public override ArrayList CompareSchema(IPersistenceProvider provider)
		{
			ArrayList results = new ArrayList();
            ArrayList schemaTables = GetSchemaTables(provider);

            // tables
            Hashtable schemaTableList = GetSchemaTableList(schemaTables);
			Hashtable schemaColumnList = new Hashtable();
			Hashtable schemaIndexListAll = new Hashtable();
            Hashtable dbTableList = getDbTableList();
			            
			DataSet columns = GetData(GetAllColumnsSQL());
			columns.CaseSensitive = true;

			DoCompare(results, dbTableList, schemaTableList, columns, DbCompareResultType.MissingInDatabase, typeof(TableMappingItem), provider);
			DoCompare(results, dbTableList, schemaTableList, columns, DbCompareResultType.MissingInSchema, typeof(TableMappingItem), provider);

            // fields
            // model exists in database
            DoCompareModelInDatabase(results,schemaTables, dbTableList,schemaColumnList, columns);
            DoCompareDatabaseInModel(results,schemaTableList,schemaColumnList,columns);
            //End fields

            //indexes
			DataSet indexes = GetData(GetSqlIndexes());
			DataSet indexFields = GetData(GetSqlIndexFields());
			indexFields.CaseSensitive = true;

            Hashtable schemaIndexListGenerate = GetSchemaIndexListGenerate(schemaTables, dbTableList, schemaIndexListAll);
            Hashtable dbIndexList = GetDbIndexList(indexes, schemaTableList);

			DoCompare(results, dbIndexList, schemaIndexListGenerate, columns, 
                DbCompareResultType.MissingInDatabase, typeof(DataEntityIndex), provider);
			DoCompare(results, dbIndexList, schemaIndexListAll, columns, 
                DbCompareResultType.MissingInSchema, typeof(DataEntityIndex), provider);

            // For index fields we only compare if the whole index is equal, we do not return
            // result for each different field. In case of a different index, we have to re-create
            // the whole index anyway.
            DoCompareIndex(results, schemaTables, indexFields);
			// foreign keys
			DataSet foreignKeys = GetData(GetSqlFk());
            // for each existing table - we skip foreign keys where table does not exist in the database or schema,
            // they will be re-created completely anyway
            DoCompareIndexExistingTables(results, schemaTables, foreignKeys, columns);
			return results;
		}

        private void DoCompareIndexExistingTables(ArrayList results, ArrayList schemaTables, DataSet foreignKeys, DataSet columns)
        {
            foreach (TableMappingItem t in schemaTables)
            {
                // not for views and not for tables where generating script is turned off
                if (t.GenerateDeploymentScript & t.DatabaseObjectType == DatabaseMappingObjectType.Table)
                {
                    DataRow[] dbRows = foreignKeys.Tables[0].Select("FK_Table = '" + t.MappedObjectName + "'");

                    // there are some constraints for this table in the database
                    if (dbRows.Length > 0)
                    {
                        // we try to see which of these we don't find in the model
                        foreach (DataRow row in dbRows)
                        {
                            bool found = false;
                            foreach (DataEntityConstraint constraint in t.Constraints)
                            {
                                if (constraint.Type == ConstraintType.ForeignKey && constraint.ForeignEntity is TableMappingItem)
                                {
                                    if ((string)row["PK_Table"] == (constraint.ForeignEntity as TableMappingItem).MappedObjectName
                                        & (string)row["FK_Table"] == t.MappedObjectName)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }

                            if (!found)
                            {
                                // constraint found in database but not in the model
                                SchemaDbCompareResult result = new SchemaDbCompareResult();
                                result.ResultType = DbCompareResultType.MissingInSchema;
                                result.ItemName = (string)row["Constraint"];
                                // TODO: result.SchemaItem = ?
                                result.ParentSchemaItem = t;
                                result.SchemaItemType = typeof(DataEntityConstraint);

                                results.Add(result);
                            }
                        }
                    }

                    // we compare what is missing in the database
                    foreach (DataEntityConstraint constraint in t.Constraints)
                    {
                        if (constraint.Type == ConstraintType.ForeignKey && constraint.ForeignEntity is TableMappingItem && constraint.Fields[0] is FieldMappingItem)
                        {
                            DataRow[] rows = foreignKeys.Tables[0].Select("PK_Table = '" + (constraint.ForeignEntity as TableMappingItem).MappedObjectName + "'"
                                + " AND FK_Table = '" + t.MappedObjectName + "' AND cKeyCol1 = '" + (constraint.Fields[0] as FieldMappingItem).MappedColumnName + "'");

                            if (columns.Tables[0].Select("TABLE_NAME = '" + t.MappedObjectName + "' AND COLUMN_NAME = '" + (constraint.Fields[0] as FieldMappingItem).MappedColumnName + "'").Length > 0)
                            {
                                if (rows.Length == 0)
                                {
                                    // constraint was not found in the database at all
                                    SchemaDbCompareResult result = new SchemaDbCompareResult();
                                    result.ResultType = DbCompareResultType.MissingInDatabase;
                                    result.ItemName = ConstraintName(t, constraint);
                                    result.SchemaItem = t;
                                    result.SchemaItemType = typeof(DataEntityConstraint);
                                    result.Script = (this.DbDataAdapterFactory as AbstractSqlCommandGenerator).AddForeignKeyConstraintDdl(t, constraint);

                                    results.Add(result);
                                }
                                else
                                {
                                    bool constraintEqual = true;

                                    // constraint found in database, we check if it has the same fields
                                    foreach (IDataEntityColumn col in constraint.Fields)
                                    {
                                        if (col is FieldMappingItem)
                                        {
                                            if (col.ForeignKeyField is FieldMappingItem)
                                            {
                                                string pk = (col.ForeignKeyField as FieldMappingItem).MappedColumnName;
                                                string fk = (col as FieldMappingItem).MappedColumnName;

                                                bool foundPair = false;
                                                for (int i = 1; i < 17; i++)
                                                {
                                                    if (rows[0]["cKeyCol" + i.ToString()] != DBNull.Value)
                                                    {
                                                        if ((string)rows[0]["cKeyCol" + i.ToString()] == fk &
                                                            (string)rows[0]["cRefCol" + i.ToString()] == pk)
                                                        {
                                                            foundPair = true;
                                                            break;
                                                        }
                                                    }
                                                }

                                                // if pair was found, we set constraint to equal
                                                if (!foundPair)
                                                {
                                                    constraintEqual = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                // one of the columns is not a physical column
                                                constraintEqual = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // one of the columns is not a physical column
                                            constraintEqual = false;
                                            break;
                                        }
                                    }

                                    if (!constraintEqual)
                                    {
                                        // constraint found but different
                                        SchemaDbCompareResult result = new SchemaDbCompareResult();
                                        result.ResultType = DbCompareResultType.ExistingButDifferent;
                                        result.ItemName = "FK_" + t.MappedObjectName + "_" + (constraint.Fields[0] as FieldMappingItem).MappedColumnName + "_" + (constraint.ForeignEntity as TableMappingItem).MappedObjectName;
                                        result.SchemaItem = t;
                                        result.SchemaItemType = typeof(DataEntityConstraint);

                                        results.Add(result);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal abstract string GetSqlFk();

        private void DoCompareIndex(ArrayList results, ArrayList schemaTables, DataSet indexFields)
        {
            foreach (TableMappingItem t in schemaTables)
            {
                if (t.GenerateDeploymentScript & t.DatabaseObjectType == DatabaseMappingObjectType.Table)
                {
                    foreach (DataEntityIndex index in t.EntityIndexes)
                    {
                        if (index.GenerateDeploymentScript == false)
                        {
                            continue;
                        }
                        bool different = false;

                        DataRow[] rows = indexFields.Tables[0].Select("TableName = '" + t.MappedObjectName + "' AND IndexName = '" + index.Name + "'");

                        // for indexes that exist in both schema and database we compare if they are equal
                        if (rows.Length > 0)
                        {
                            // if there is a different number of fields, we consider them non-equal without even checking the details
                            if (rows.Length != index.ChildItemsByType(DataEntityIndexField.CategoryConst).Count)
                            {
                                different = true;
                            }

                            if (!different)
                            {
                                foreach (DataEntityIndexField fld in index.ChildItemsByType(DataEntityIndexField.CategoryConst))
                                {
                                    rows = indexFields.Tables[0].Select("TableName = '" + t.MappedObjectName
                                        + "' AND IndexName = '" + index.Name
                                        + "' AND ColumnName = '" + (fld.Field as FieldMappingItem).MappedColumnName
                                        + "' AND OrdinalPosition = " + (fld.OrdinalPosition + 1).ToString()
                                        + " AND IsDescending = " + (fld.SortOrder == DataEntityIndexSortOrder.Descending ? "1" : "0"));

                                    if (rows.Length == 0)
                                    {
                                        different = true;
                                        break;
                                    }
                                }
                            }

                            if (different)
                            {
                                // exists in both schema and database, but they are different
                                SchemaDbCompareResult result = new SchemaDbCompareResult();
                                result.ResultType = DbCompareResultType.ExistingButDifferent;
                                result.ItemName = t.MappedObjectName + "." + index.Name;
                                result.SchemaItem = index;
                                result.SchemaItemType = typeof(DataEntityIndex);
                                result.Script = (this.DbDataAdapterFactory as AbstractSqlCommandGenerator).IndexDefinitionDdl(t, result.SchemaItem as DataEntityIndex, true);

                                results.Add(result);
                            }
                        }
                    }
                }
            }
        }

        internal abstract Hashtable GetDbIndexList(DataSet indexes, Hashtable schemaTableList);
        internal abstract Hashtable GetSchemaIndexListGenerate(ArrayList schemaTables, Hashtable dbTableList, Hashtable schemaIndexListAll);
        
        internal abstract string GetSqlIndexFields();
        internal abstract string GetSqlIndexes();

        private void DoCompareDatabaseInModel(ArrayList results, Hashtable schemaTableList, Hashtable schemaColumnList, DataSet columns)
        {
            AbstractSqlCommandGenerator gen = (AbstractSqlCommandGenerator)this.DbDataAdapterFactory;
            foreach (DataRow row in columns.Tables[0].Rows)
            {
                string key = row["TABLE_NAME"] + "." + row["COLUMN_NAME"];

                // only if the table exists in the model - otherwise we will be creating the whole table later on
                // so it makes no sense to list all the columns with it
                if (schemaTableList.ContainsKey(row["TABLE_NAME"]))
                {
                    if (schemaColumnList.ContainsKey(key))
                    {
                        FieldMappingItem fld = schemaColumnList[key] as FieldMappingItem;
                        string differenceDescription = "";
                        if ((string)row["IS_NULLABLE"] == "YES" && fld.AllowNulls == false)
                        {
                            differenceDescription =
                                (differenceDescription == "" ? "" : "; ")
                                + "AllowNulls: Schema-NO, Database-YES";
                        }
                        if ((string)row["IS_NULLABLE"] == "NO" && fld.AllowNulls == true)
                        {
                            differenceDescription =
                                (differenceDescription == "" ? "" : "; ")
                                + "AllowNulls: Schema-YES, Database-NO";
                        }
                        if (CompareType(row, gen.DdlDataType(fld.DataType, fld.MappedDataType).ToUpper()))
                        {
								differenceDescription = (differenceDescription == "" ? "" : "; ")
									+ "DataType: Schema-"
									+ gen.DdlDataType(fld.DataType, fld.MappedDataType)
									+ ", Database-" + (string)row["DATA_TYPE"];
                        }
                        if (fld.DataType == OrigamDataType.String
                            && !row.IsNull("CHARACTER_MAXIMUM_LENGTH")
                            && (int)row["CHARACTER_MAXIMUM_LENGTH"] != fld.DataLength)
                        {
                            differenceDescription =
                                (differenceDescription == "" ? "" : "; ")
                                + "DataLength: Schema-" + fld.DataLength.ToString()
                                + ", Database-" + row["CHARACTER_MAXIMUM_LENGTH"].ToString();
                        }
                        if (differenceDescription != "")
                        {
                            // exists in both schema and database, but they are different
                            SchemaDbCompareResult result = new SchemaDbCompareResult
                            {
                                ResultType = DbCompareResultType.ExistingButDifferent,
                                ItemName = key,
                                SchemaItem = fld,
                                Remark = differenceDescription,
                                SchemaItemType = typeof(FieldMappingItem),
                                Script = gen.AlterColumnDdl(fld)
                            };
                            results.Add(result);
                        }
                    }
                    else
                    {
                        // does not exist in schema
                        SchemaDbCompareResult result = new SchemaDbCompareResult();
                        result.ResultType = DbCompareResultType.MissingInSchema;
                        result.ItemName = key;
                        result.SchemaItem = null;
                        result.ParentSchemaItem = schemaTableList[row["TABLE_NAME"]];
                        result.SchemaItemType = typeof(FieldMappingItem);
                        results.Add(result);
                    }
                }
            }
        }

        private bool CompareType(DataRow row, string modelType)
        {
			string columnType = GetColumnType(row);
			if (columnType.Contains("TIMESTAMP") && modelType.Contains("TIMESTAMP"))
			{
				return false;
			}
			if(columnType.Contains("CHARACTER VARYING") && modelType.Contains("VARCHAR"))
            {
				return false;
            }
			return columnType != modelType;
		}

        private string GetColumnType(DataRow row)
        {
			StringBuilder finalDataType = new StringBuilder();
			finalDataType.Append((string)row["DATA_TYPE"]);
            int length = Convert.IsDBNull(row["CHARACTER_MAXIMUM_LENGTH"]) ? 0 : (int)row["CHARACTER_MAXIMUM_LENGTH"];
			if (length ==-1)
			{
				finalDataType.Append("(MAX)");
            }
			return finalDataType.ToString().ToUpper();
		}

        private void DoCompareModelInDatabase(ArrayList results, ArrayList schemaTables, Hashtable dbTableList, Hashtable schemaColumnList, DataSet columns)
        {
            foreach (TableMappingItem t in schemaTables)
            {
                // only if the table exists in the database - otherwise we will be creating the whole table later on
                // so it makes no sense to list all the columns with it
                if (dbTableList.ContainsKey(t.MappedObjectName))
                {
                    foreach (IDataEntityColumn col in t.EntityColumns)
                    {
                        if (col is FieldMappingItem)
                        {
                            string key = t.MappedObjectName + "." + (col as FieldMappingItem).MappedColumnName;
                            if (!schemaColumnList.ContainsKey(key))
                            {
                                schemaColumnList.Add(key, col);
                            }

                            if (columns.Tables[0].Select("TABLE_NAME = '" + t.MappedObjectName + "' AND COLUMN_NAME = '" + (col as FieldMappingItem).MappedColumnName + "'").Length == 0)
                            {
                                // column does not exist in the database
                                SchemaDbCompareResult result = new SchemaDbCompareResult
                                {
                                    ResultType = DbCompareResultType.MissingInDatabase,
                                    ItemName = key,
                                    SchemaItem = col,
                                    SchemaItemType = typeof(FieldMappingItem)
                                };
                                if (t.DatabaseObjectType == DatabaseMappingObjectType.Table)
                                {
                                    result.Script = (this.DbDataAdapterFactory as AbstractSqlCommandGenerator).AddColumnDdl(col as FieldMappingItem);
                                }
                                else
                                {
                                    result.Remark = ResourceUtils.GetString("ViewCantBeScripted");
                                }
                                results.Add(result);
                                // foreign key
                                DataEntityConstraint fk = col.ForeignKeyConstraint;
                                if (t.DatabaseObjectType == DatabaseMappingObjectType.Table
                                    && fk != null)
                                {
                                    result = new SchemaDbCompareResult
                                    {
                                        ResultType = DbCompareResultType.MissingInDatabase,
                                        ItemName = ConstraintName(t, fk),
                                        SchemaItem = col,
                                        SchemaItemType = typeof(DataEntityConstraint),
                                        Script = (this.DbDataAdapterFactory as AbstractSqlCommandGenerator)
                                        .AddForeignKeyConstraintDdl(t, fk)
                                    };
                                    results.Add(result);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal abstract string GetAllColumnsSQL();

        private Hashtable getDbTableList()
        {
            DataSet tables = GetData(GetAllTablesSQL());
            Hashtable dbTableList = new Hashtable();
            foreach (DataRow row in tables.Tables[0].Rows)
            {
                dbTableList.Add(row["TABLE_NAME"], null);
            }
            return dbTableList;
        }
        private Hashtable GetSchemaTableList(ArrayList schemaTables)
        {
            Hashtable schemaTableList = new Hashtable();
            foreach (TableMappingItem t in schemaTables)
            {
                if (!schemaTableList.Contains(t.MappedObjectName))
                {
                    schemaTableList.Add(t.MappedObjectName, t);
                }
            }
            return schemaTableList;
        }

        private ArrayList GetSchemaTables(IPersistenceProvider provider)
        {
            List<AbstractSchemaItem> entityList = provider.RetrieveListByCategory<AbstractSchemaItem>(AbstractDataEntity.CategoryConst);
            ArrayList schemaTables = new ArrayList();

            foreach (IDataEntity e in entityList)
            {
                if (e is TableMappingItem)
                {
                    schemaTables.Add(e);
                }
            }
            return schemaTables;
        }

        private static string ConstraintName(TableMappingItem t, 
            DataEntityConstraint constraint)
        {
            return "FK_" + t.MappedObjectName + "_" 
                + (constraint.Fields[0] as FieldMappingItem).MappedColumnName 
                + "_" + (constraint.ForeignEntity as TableMappingItem).MappedObjectName;
        }

		private void DoCompare(ArrayList results, Hashtable dbList, 
            Hashtable schemaList, DataSet columns, 
            DbCompareResultType direction, Type schemaItemType,
            IPersistenceProvider provider)
		{
            switch (direction)
			{
                case DbCompareResultType.MissingInDatabase:
                    CompareMissingInDatabase(results, dbList, schemaList, 
                        schemaItemType);
                    break;

                case DbCompareResultType.MissingInSchema:
                    CompareMissingInModel(results, dbList, schemaList, 
                        columns, schemaItemType, provider);
                    break;
            }
		}

        private void CompareMissingInModel(ArrayList results, Hashtable dbList, 
            Hashtable schemaList, DataSet columns, Type schemaItemType, 
            IPersistenceProvider provider)
        {
            AbstractSqlCommandGenerator sqlGenerator =
                DbDataAdapterFactory as AbstractSqlCommandGenerator;
            foreach (DictionaryEntry entry in dbList)
            {
                if (!schemaList.ContainsKey(entry.Key))
                {
                    SchemaDbCompareResult result = new SchemaDbCompareResult();
                    result.ResultType = DbCompareResultType.MissingInSchema;
                    result.ItemName = (string)entry.Key;
                    result.ParentSchemaItem = entry.Value;
                    result.SchemaItemType = schemaItemType;
                    // generate a model element
                    if (schemaItemType == typeof(TableMappingItem))
                    {
                        ISchemaService schema = ServiceManager.Services.GetService(
                            typeof(ISchemaService)) as ISchemaService;
                        TableMappingItem entity = new TableMappingItem();
                        entity.PersistenceProvider = provider;
                        entity.SchemaExtensionId = schema.ActiveSchemaExtensionId;
                        entity.RootProvider = schema.GetProvider(
                            typeof(EntityModelSchemaItemProvider));
                        entity.Name = result.ItemName;
                        foreach (DataRow columnRow in columns.Tables[0].Select(
                            "TABLE_NAME = '" + entity.Name + "'"))
                        {
                            FieldMappingItem field = entity.NewItem(typeof(FieldMappingItem), 
                                schema.ActiveSchemaExtensionId, null) as FieldMappingItem;
                            field.Name = (string)columnRow["COLUMN_NAME"];
                            field.AllowNulls = (string)columnRow["IS_NULLABLE"] == "YES";
                            // find a specific data type
                            DatabaseDataTypeSchemaItemProvider dataTypes = schema.GetProvider(
                                typeof(DatabaseDataTypeSchemaItemProvider)) 
                                as DatabaseDataTypeSchemaItemProvider;
                            string dataTypeName = (string)columnRow["DATA_TYPE"];
                            DatabaseDataType type = dataTypes.FindDataType(dataTypeName);
                            if (type == null)
                            {
                                // if not found, get a generic data type
                                field.DataType = sqlGenerator.ToOrigamDataType(
                                        dataTypeName);
                            }
                            else
                            {
                                field.DataType = type.DataType;
                                field.MappedDataType = type;
                            }
                            if (!columnRow.IsNull("CHARACTER_MAXIMUM_LENGTH"))
                            {
                                field.DataLength = (int)columnRow["CHARACTER_MAXIMUM_LENGTH"];
                            }
                        }
                        result.SchemaItem = entity;
                    }
                    results.Add(result);
                }
            }
        }

        private void CompareMissingInDatabase(ArrayList results,
            Hashtable dbList, Hashtable schemaList, Type schemaItemType)
        {
            AbstractSqlCommandGenerator sqlGenerator =
                DbDataAdapterFactory as AbstractSqlCommandGenerator;
            foreach (DictionaryEntry entry in schemaList)
            {
                bool process = true;
                if ((entry.Value is TableMappingItem)
                && (entry.Value as TableMappingItem).GenerateDeploymentScript == false)
                {
                    process = false;
                }
                if ((entry.Value is DataEntityIndex)
                && ((((entry.Value as DataEntityIndex).ParentItem
                    as TableMappingItem).GenerateDeploymentScript
                == false)
                || ((entry.Value as DataEntityIndex).GenerateDeploymentScript == false)))
                {
                    process = false;
                }
                if (!dbList.ContainsKey(entry.Key) & process)
                {
                    SchemaDbCompareResult result = new SchemaDbCompareResult();
                    result.ResultType = DbCompareResultType.MissingInDatabase;
                    result.ItemName = (string)entry.Key;
                    result.SchemaItem = (ISchemaItem)entry.Value;
                    result.ParentSchemaItem = ((ISchemaItem)entry.Value).ParentItem;
                    result.SchemaItemType = schemaItemType;
                    if (schemaItemType == typeof(TableMappingItem))
                    {
                        if ((result.SchemaItem as TableMappingItem).DatabaseObjectType
                            == DatabaseMappingObjectType.Table)
                        {
                            result.Script = sqlGenerator.TableDefinitionDdl(
                                result.SchemaItem as TableMappingItem);
                            result.Script2 = sqlGenerator.ForeignKeyConstraintsDdl(
                                result.SchemaItem as TableMappingItem);
                        }
                        else
                        {
                            result.Remark = "View cannot be scripted to the database.";
                        }
                    }
                    if (schemaItemType == typeof(DataEntityIndex))
                    {
                        result.Script = sqlGenerator.IndexDefinitionDdl(
                            result.ParentSchemaItem as IDataEntity,
                            result.SchemaItem as DataEntityIndex, true);
                    }
                    results.Add(result);
                }
            }
        }

        internal DataSet GetData(string sql)
		{
			using(IDbConnection connection = GetConnection(_connectionString))
			{
				connection.Open();

				try
				{
					DbDataAdapter adapter = this.DbDataAdapterFactory.GetAdapter(this.DbDataAdapterFactory.GetCommand(sql, connection));
					DataSet data = new DataSet("SchemaCompare");
					adapter.Fill(data);

					return data;
				}
				finally
				{
					connection.Close();
				}
			}
		}
		#endregion

		#region Private Methods
		private void AcceptChanges(DataSet dataset, ArrayList changedTables, DataStructureQuery query, string transactionId, IPrincipal userProfile)
		{
			// Retrieve actual values and accept changes. Much faster than DataSet.AcceptChanges()
			foreach(string tableName in changedTables)
			{
				DataTable table = dataset.Tables[tableName];

				int rowCount = table.Rows.Count;
				DataRow[] rowArray = new DataRow[rowCount];
				table.Rows.CopyTo(rowArray, 0);

				// create dataset of this particular data table for loading new data
				DataSet newData = CloneDatasetForActualRow(table);

				for (int i = 0; i < rowCount; i++)
				{
					if(rowArray[i].RowState == DataRowState.Deleted)
					{
						rowArray[i].AcceptChanges();
					}
					else if(rowArray[i].RowState != DataRowState.Unchanged
						& rowArray[i].RowState != DataRowState.Detached
						& rowArray[i].RowState != DataRowState.Deleted)
					{
						if(query.LoadActualValuesAfterUpdate)
						{
							Guid entityId = (Guid)table.ExtendedProperties["Id"];
							DataStructureEntity entity = GetEntity(entityId);

							if(entity.EntityDefinition.GetType() == typeof(TableMappingItem))
							{
								newData.Clear();
								LoadActualRow(newData, entityId, query.MethodId,
                                    rowArray[i], userProfile, transactionId);								
								if(newData.Tables[0].Rows.Count == 0)
								{
									throw new Exception(ResourceUtils.GetString("NoDataRowAfterUpdate"));
								}

								//								rowArray[i].AcceptChanges();

								ArrayList detachedColumns = new ArrayList();
								foreach(DataStructureColumn col in entity.Columns)
								{
									if(! (col.Field is FieldMappingItem))
									{
										detachedColumns.Add(col.Name);
									}
								}
	
								try
								{
									rowArray[i].AcceptChanges();

									rowArray[i].BeginEdit();

									foreach(DataColumn col in table.Columns)
									{
										if(! detachedColumns.Contains(col.ColumnName) && col.ReadOnly == false)
										{
                                            object newValue = newData.Tables[0].Rows[0][col.ColumnName];
                                            if (!rowArray[i][col].Equals(newValue))
                                            {
                                                rowArray[i][col] = newData.Tables[0].Rows[0][col.ColumnName];
                                            }
										}
									}

									rowArray[i].EndEdit();

									rowArray[i].AcceptChanges();
								}
								catch(Exception ex)
								{
									throw new Exception(ResourceUtils.GetString("FailedUpdateRow", rowArray[i].RowState.ToString()), ex);
								}
								//rowArray[i].Table.LoadDataRow(newData.Tables[0].Rows[0].ItemArray, true);
							}
							else
							{
								rowArray[i].AcceptChanges();
							}
						}
						else
						{
							rowArray[i].AcceptChanges();
						}
					}
				}
			}
		}

		private DataStructureEntity GetEntity (Guid entityId)
		{
			return this.PersistenceProvider.RetrieveInstance(
				typeof(DataStructureEntity), 
				new ModelElementKey(entityId)
				) as DataStructureEntity;		
        }

        private IDataEntityColumn GetField(Guid fieldId)
        {
            return this.PersistenceProvider.RetrieveInstance(
                typeof(AbstractSchemaItem),
                new ModelElementKey(fieldId)
                ) as IDataEntityColumn;
        }
        
        private DataSet CloneDatasetForActualRow(DataTable table)
		{
			DataSet newData = new DataSet(table.DataSet.DataSetName);
			newData.Tables.Add(new OrigamDataTable(table.TableName));
			foreach(DataColumn col in table.Columns)
			{
				DataColumn newCol = new DataColumn(col.ColumnName, col.DataType, "", col.ColumnMapping);
				newCol.MaxLength = col.MaxLength;
				newCol.AllowDBNull = col.AllowDBNull;
				newCol.DefaultValue = col.DefaultValue;
				newData.Tables[0].Columns.Add(newCol);
			}
			
			return newData;
		}

		private void LoadActualRow(DataSet newData, Guid entityId, 
            Guid filterSetId, DataRow row, IPrincipal userProfile,
            string transactionId)
		{
			DataStructureQuery newDataQuery = 
                new DataStructureQuery(entityId, filterSetId);
			newDataQuery.DataSourceType = QueryDataSourceType.DataStructureEntity;
			foreach(DataColumn col in row.Table.PrimaryKey)
			{
				if (row.RowState == DataRowState.Deleted)
				{
					newDataQuery.Parameters.Add(new QueryParameter(col.ColumnName, row[col, DataRowVersion.Original]));
				}
				else
				{
					newDataQuery.Parameters.Add(new QueryParameter(col.ColumnName, row[col]));
				}
			}

			this.LoadDataSet(newDataQuery, userProfile, newData, transactionId);
		}

		internal void TraceCommand(IDbCommand command, string transactionId)
		{
			if(log.IsDebugEnabled)
			{
				IDbCommand processIdCommand = command.Connection.CreateCommand();
                processIdCommand.CommandText = GetPid(); 
				processIdCommand.Transaction = command.Transaction;
				object spid = processIdCommand.ExecuteScalar();
				log.Debug("SQL Command; Connection ID: " + spid.ToString() + ", Transaction ID: " + transactionId + ", " + command.CommandText); 
				foreach(IDbDataParameter param in command.Parameters)
				{
					string paramValue = "NULL";
					if(param.Value != null) paramValue = param.Value.ToString();
					log.Debug("Parameter: " + param.ParameterName + " Value: " + paramValue);
				}
			}
		}

        internal abstract string GetPid();

        private void SetTransaction(DbDataAdapter adapter, IDbTransaction transaction)
		{
			((IDbDataAdapter)adapter).SelectCommand.Transaction = transaction;
			((IDbDataAdapter)adapter).UpdateCommand.Transaction = transaction;
			((IDbDataAdapter)adapter).DeleteCommand.Transaction = transaction;
			((IDbDataAdapter)adapter).InsertCommand.Transaction = transaction;
		}

		private void SetConnection(DbDataAdapter adapter, IDbConnection connection)
		{
			((IDbDataAdapter)adapter).SelectCommand.Connection = connection;
			((IDbDataAdapter)adapter).UpdateCommand.Connection = connection;
			((IDbDataAdapter)adapter).DeleteCommand.Connection = connection;
			((IDbDataAdapter)adapter).InsertCommand.Connection = connection;
		}

        private static void CheckDatabaseName(string name)
        {
            if (name.Contains("]"))
            {
                throw new Exception(string.Format("Invalid database name: {0}", name));
            }
        }
        #endregion

		#region IDisposable
		public override void Dispose()
		{
			_connectionString = null;
			
			base.Dispose ();
		}

        public virtual void CreateSchema(string databaseName)
        {
        }

        public void CreateFirstNewWebUser(QueryParameterCollection parameters)
        {
			string transaction1 = Guid.NewGuid().ToString();
			try
			{
				ExecuteUpdate(CreateBusinessPartnerInsert(parameters), transaction1);
				ExecuteUpdate(CreateOrigamUserInsert(parameters), transaction1);
				ExecuteUpdate(CreateBusinessPartnerRoleIdInsert(parameters),transaction1);
				ExecuteUpdate(AlreadyCreatedUser(parameters), transaction1);
				ResourceMonitor.Commit(transaction1);
			}
			catch (Exception)
			{
				ResourceMonitor.Rollback(transaction1);
				throw;
			}
		}
		public abstract string CreateBusinessPartnerInsert(QueryParameterCollection parameters);
		public abstract string CreateOrigamUserInsert(QueryParameterCollection parameters);
		public abstract string CreateBusinessPartnerRoleIdInsert(QueryParameterCollection parameters);
		public abstract string AlreadyCreatedUser(QueryParameterCollection parameters);
		#endregion
	}
    // version of log4net for NetStandard 1.3 does not have the method
    // LogManager.GetLogger(string)... have to use the overload with Type as parameter 
    public class WorkflowProfiling
    {
    }

    internal class Profiler
		{
			private static readonly ILog workflowProfilingLog = 
				LogManager.GetLogger(typeof(Profiler));
			
			private readonly Dictionary<DataStructureEntity,List<double>> durations_ms = 
				new Dictionary<DataStructureEntity, List<double>>();
			
			private readonly List<DataStructureEntity> entityOrder =
				new List<DataStructureEntity>();
			
			private static string currentTaskId;

			public void LogRememberedExecutionTimes()
			{
				string taskPath = (string) ThreadContext.Properties["currentTaskpath"];
				string taskId = (string) ThreadContext.Properties["currentTaskId"];
				string serviceMethodName = (string)ThreadContext.Properties["ServiceMethodName"];
				if (taskId == null) return;

				foreach (var entity in entityOrder)
				{
					LogDuration(
						logEntryType: serviceMethodName,
						path: $"{taskPath}/Load/{entity.Name}",
						id: taskId,
						duration: durations_ms[entity].Sum(),
						rows: durations_ms[entity].Count);
				}
				durations_ms.Clear();
				entityOrder.Clear();
			}

			public void ExecuteAndRememberLoadDuration(DataStructureEntity entity,
				 Action actionToExecute)
			{
				ExecuteAndTakeLoggingAction(entity, RememberLoadDuration, actionToExecute);
			}

			public void ExecuteAndLogStoreActionDuration(DataStructureEntity entity,
			Action actionToExecute)
			{
				ExecuteAndTakeLoggingAction(entity, LogStoreDuration, actionToExecute);
			}

			private static void ExecuteAndTakeLoggingAction(DataStructureEntity entity,
				Action<DataStructureEntity,Stopwatch> loggingAction, Action actionToExecute)	
			{

			   if (workflowProfilingLog.IsDebugEnabled)
				{
					string taskId = (string) ThreadContext.Properties["currentTaskId"];
					if (taskId != null)
					{
						Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start();

						actionToExecute.Invoke();

						stopwatch.Stop();
						
						loggingAction.Invoke(entity,stopwatch);
						return;
					} 
				} 
				actionToExecute.Invoke();
			}
		
			private void RememberLoadDuration(DataStructureEntity entity, 
				Stopwatch stoppedWatch)
			{
				string taskId = (string) ThreadContext.Properties["currentTaskId"];
				if (currentTaskId != taskId)
				{
					entityOrder.Clear();
					durations_ms.Clear();
					currentTaskId = taskId;
				}
				if (!entityOrder.Contains(entity))
				{
					entityOrder.Add(entity);
					durations_ms[entity] = new List<double>();
				}
				durations_ms[entity].Add(stoppedWatch.Elapsed.TotalMilliseconds);
			}

			private static void LogStoreDuration(DataStructureEntity entity, 
				Stopwatch stoppedWatch)
			{
				string taskPath = (string) ThreadContext.Properties["currentTaskpath"];
				string taskId = (string) ThreadContext.Properties["currentTaskId"];
				string serviceMethodName = (string)ThreadContext.Properties["ServiceMethodName"];
				LogDuration(
					logEntryType: serviceMethodName,
					path: $"{taskPath}/Store/{entity.Name}", 
					id: taskId, 
					duration: stoppedWatch.Elapsed.TotalMilliseconds);
			}

			private static void LogDuration(string logEntryType, string path, 
				string id, double duration, int rows=0)
			{
				string typeWithDoubleColon = $"{logEntryType}:";

				string message = string.Format(
					"{0,-18}{1,-80} Id: {2}  Duration: {3,7:0.0} ms",
					typeWithDoubleColon,
					path,
					id,
					duration);
				if (rows != 0)
				{
					message += " rows: " + rows;
				}
				workflowProfilingLog.Debug(message);
			}
		}
}
