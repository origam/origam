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
using System.Threading;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Globalization;
using res = Origam.DA.Service;

using System.Collections.Generic;
using System.Text;
using Origam.Extensions;
using Origam.DA.Service;

namespace Origam.DA.ObjectPersistence.Providers
{
    /// <summary>
    /// Summary description for OrigamPersistenceProvider.
    /// </summary>
    public class DatabasePersistenceProvider : AbstractPersistenceProvider, IDatabasePersistenceProvider
    {
        /// <summary>
        /// Internal dataset used to interface with the DataService
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        DataSet _dataSet;
        Hashtable _objectCache = new Hashtable();

        private bool inTransaction;
        public override bool InTransaction => inTransaction;

        public DatabasePersistenceProvider()
        {
            LocalizationCache = new LocalizationCache();
        }

        public DatabasePersistenceProvider(LocalizationCache localizationCache)
        {
            LocalizationCache = localizationCache;
        }

        public DatabasePersistenceProvider(DataSet data) : this()
        {
            _dataSet = data;
        }

        public override void BeginTransaction()
        {
            base.BeginTransaction();
            inTransaction = true;
        }

        public override void EndTransactionDontSave()
        {
            base.EndTransactionDontSave();
            inTransaction = false;
        }

        public override void EndTransaction()
        {
            base.EndTransaction();
            inTransaction = false;
        }

        #region Private Methods
            private EntityNameAttribute Entity(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(EntityNameAttribute), true);

            if (attributes != null && attributes.Length > 0)
                return attributes[0] as EntityNameAttribute;
            else
                return null;

        }

        private void SetValue(DataRow row, string columnName, object value)
        {
            if (value is Guid && value.Equals(Guid.Empty))
                value = DBNull.Value;

            if (value == null)
                value = DBNull.Value;

            row[columnName] = value;
        }

        /// <summary>
        /// Gets all primary key column names of the entity.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Key DummyKey(Type type)
        {
            Key key = new Key();

            foreach (DataColumn col in _dataSet.Tables[this.Entity(type).Name].PrimaryKey)
            {
                key.Add(col.ColumnName, new Guid());
            }

            return key;
        }

        private object[] FilterKey(Key key, DataTable table)
        {
            object[] filterkey = new object[table.PrimaryKey.Length];

            for (int i = 0; i < table.PrimaryKey.Length; i++)
            {
                filterkey[i] = key[table.PrimaryKey[i].ColumnName];
            }

            return filterkey;
        }
        #endregion

        #region IPersistenceProvider Members
        public override ILocalizationCache LocalizationCache { get; }

	    public override ICompiledModel CompiledModel { get; set; }

	    /// <summary>
        /// Gets or sets main model's data structure id.
        /// </summary>
        public Guid DataStructureId { get; set; }

	    /// <summary>
        /// Gets or sets data service for this provider.
        /// </summary>
        public IDataService DataService { get; set; }

	    /// <summary>
        /// Gets or sets query for data structure with which we will load data from the database.
        /// </summary>
        public DataStructureQuery DataStructureQuery { get; set; }

	    /// <summary>
        /// Initializes the provider with an IDataStructure, which contains structure definition and
        /// dynamic parameters for loading the data.
        /// </summary>
        /// <param name="dataStructureQuery"></param>
        public void Init(DataStructureQuery dataStructureQuery)
        {
            this.DataStructureQuery = dataStructureQuery;
            this.Refresh(false, null);
        }

        public void Init(DataSet data)
        {
            _dataSet = data;
        }

        public void Init()
        {
            _dataSet = DataService.GetEmptyDataSet(
                        DataStructureId, CultureInfo.InvariantCulture);
        }

        public void Refresh(bool append, string transactionId)
        {
            if (append)
            {
                if (_dataSet == null)
                {
                    Init();
                }
            }
            else
            {
                _objectCache.Clear();
                Init();
            }
            DataService.LoadDataSet(DataStructureQuery, SecurityManager.CurrentPrincipal,
                _dataSet, transactionId);
        }

        public override void DeletePackage(Guid packageId)
        {
            DataStructureQuery query = new DataStructureQuery();
            query.Parameters.Add(new QueryParameter("packageId", packageId));
            query.LoadByIdentity = false;

            DataService.ExecuteProcedure("dbo.DeletePackage", "", query, null);
        }

        public void EnforceConstraints()
        {
            try
            {
                _dataSet.EnforceConstraints = true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
                log.Fatal(DebugClass.ListRowErrors(_dataSet));
            }
        }

        public void Update(string transactionId)
        {
            CheckStatus();
            // for update we use the main data structure
            DataStructureQuery q = new DataStructureQuery(DataStructureId);
            q.LoadByIdentity = false;
            q.LoadActualValuesAfterUpdate = false;
            q.FireStateMachineEvents = false;
            q.SynchronizeAttachmentsOnDelete = false;
            DataService.UpdateData(
                q, SecurityManager.CurrentPrincipal, _dataSet, transactionId);
        }

        public override object RetrieveInstance(Type type, Key primaryKey)
        {
            return RetrieveInstance(type, primaryKey, true, null);
        }

        public override object RetrieveInstance(Type type, Key primaryKey, bool useCache)
        {
            return RetrieveInstance(type, primaryKey, useCache, null);
        }

        public override object RetrieveInstance(Type type, Key primaryKey, bool useCache, bool throwNotFoundException)
        {
            return RetrieveInstance(type, primaryKey, useCache, null, throwNotFoundException);
        }

        private object RetrieveInstance(Type type, Key primaryKey, bool useCache, IPersistent objectToRefresh)
        {
            return RetrieveInstance(type, primaryKey, useCache, objectToRefresh, true);
        }

        private static string GetLocale()
        {
            return Thread.CurrentThread.CurrentUICulture.Name;
        }

        private object RetrieveInstance(Type type, Key primaryKey, bool useCache, IPersistent objectToRefresh, bool throwNotFoundException)
        {
            // CheckStatus();
            //
            // object instance = null;
            // string locale = GetLocale();
            //
            // lock (this)
            // {
            //     // We test if primary key is not an initialized Guid
            //     if (primaryKey[primaryKey.KeyArray[0]].Equals(Guid.Empty))
            //         return null;
            //
            //     if (useCache & primaryKey.ContainsKey("Id"))
            //     {
            //         if (_objectCache.Contains(locale))
            //         {
            //             Hashtable cache = _objectCache[locale] as Hashtable;
            //
            //             // We test if object is already in cache
            //             if (cache.Contains(primaryKey["Id"]))
            //             {
            //                 // it is, so we retrieve it from the cache and return it
            //                 return cache[primaryKey["Id"]];
            //             }
            //         }
            //     }
            //
            //     // object is not in cache, we try to load it using compiled model
            //     if (this.CompiledModel != null)
            //     {
            //         //instance = this.CompiledModel.RetrieveItem(primaryKey["Id"].ToString());
            //         try
            //         {
            //             instance = this.CompiledModel.GetType().InvokeMember("GetId_" + primaryKey["Id"].ToString().Replace("-", "_"),
            //                 BindingFlags.DeclaredOnly |
            //                 BindingFlags.Public | BindingFlags.NonPublic |
            //                 BindingFlags.Instance | BindingFlags.InvokeMethod
            //                 , null, this.CompiledModel, null);
            //         }
            //         catch
            //         {
            //         }
            //
            //         if (instance != null)
            //         {
            //             (instance as IPersistent).IsPersisted = true;
            //         }
            //     }
            //
            //     // object has not been found in the cache, so we restore it from the data source
            //
            //     if (instance == null)
            //     {
            //         Hashtable derivedRows = new Hashtable();
            //
            //         //Find entity attribute
            //         EntityNameAttribute entity = this.Entity(type);
            //
            //         if (entity != null)
            //         {
            //             string entityName = entity.Name;
            //
            //             //Filter the row out by primary key
            //             DataRow row = _dataSet.Tables[entityName].Rows.Find(this.FilterKey(primaryKey, _dataSet.Tables[entityName]));
            //
            //             if (row != null)
            //             {
            //                 if (objectToRefresh == null)
            //                 {
            //                     string typeName = type.FullName;
            //                     string assemblyName = type.Assembly.FullName;
            //
            //                     if (entity.InheritanceColumn != null)
            //                     {
            //                         // Our entity supports inheritance, so we have to load the derived entity
            //                         typeName = row[entity.InheritanceColumn].ToString();
            //                         assemblyName = typeName.Substring(0, typeName.LastIndexOf('.'));
            //                     }
            //
            //                     object[] constructorArray = new object[1] { primaryKey };
            //
            //                     // Construct the object
            //                     // Objects must have a constructor with parameter Key, otherwise we crash
            //                     instance = Reflector.InvokeObject(assemblyName, typeName, constructorArray);
            //                     // Set the persistence provider, so the object can persist itself later on
            //                     type = instance.GetType();
            //                     (instance as IPersistent).PersistenceProvider = this;
            //                 }
            //                 else
            //                 {
            //                     instance = objectToRefresh;
            //                 }
            //
            //                 // Mark the object as persisted, because we have just read it from the database
            //                 (instance as IPersistent).IsPersisted = true;
            //
            //                 // Set all the remaining properties
            //                 IList members = Reflector.FindMembers(type, typeof(EntityColumnAttribute), new Type[0]);
            //                 foreach (MemberAttributeInfo mi in members)
            //                 {
            //                     EntityColumnAttribute column = mi.Attribute as EntityColumnAttribute;
            //
            //                     // Skip reading foreign keys
            //                     // TODO: Support reading foreign keys
            //                     if (!column.IsForeignKey)
            //                     {
            //                         DataRow propertyRow = row;
            //
            //                         if (column.OverridenEntityName != null)
            //                         {
            //                             if (derivedRows.ContainsKey(column.OverridenEntityName))
            //                                 // We check if we already used this derived entity
            //                                 propertyRow = derivedRows[column.OverridenEntityName] as DataRow;
            //                             else
            //                             {
            //                                 // We did not get the derived row, yet, so we get one
            //                                 // First we try to look it up in the data
            //                                 if (!_dataSet.Relations.Contains(column.OverridenEntityName))
            //                                 {
            //                                     throw new InvalidOperationException(res.ResourceUtils.GetString("EntityNotInDataSet", column.OverridenEntityName));
            //                                 }
            //
            //                                 propertyRow = _dataSet.Relations[column.OverridenEntityName].ChildTable.Rows.Find(this.FilterKey(((IPersistent)instance).PrimaryKey, _dataSet.Relations[column.OverridenEntityName].ChildTable));
            //
            //                                 if (propertyRow == null)
            //                                     throw new Exception(res.ResourceUtils.GetString("NoRowsForDerived", column.OverridenEntityName));
            //                                 else
            //                                     derivedRows.Add(column.OverridenEntityName, propertyRow);
            //                             }
            //                         }
            //
            //                         // Get the column name
            //                         string columnName = column.Name;
            //
            //                         object value = propertyRow[columnName];
            //
            //                         // Handle NULL values
            //                         if (value == DBNull.Value)
            //                             value = null;
            //
            //                         Type memberType;
            //                         if (mi.MemberInfo is PropertyInfo)
            //                             memberType = (mi.MemberInfo as PropertyInfo).PropertyType;
            //                         else
            //                             memberType = (mi.MemberInfo as FieldInfo).FieldType;
            //
            //                         // If member is enum, we have to convert
            //                         if (memberType.IsEnum)
            //                             value = Enum.ToObject(memberType, Convert.ToInt32(value));
            //
            //                         // localization
            //                         if (LocalizationCache != null && value is String)
            //                         {
            //                             value = LocalizationCache.GetLocalizedString((Guid)primaryKey["Id"], mi.MemberInfo.Name, (string)value, locale);
            //                         }
            //
            //                         Reflector.SetValue(mi.MemberInfo, instance, value);
            //                     }
            //                 }
            //
            //             }
            //             else
            //             {
            //                 if (throwNotFoundException)
            //                 {
            //                     throw new Exception(res.ResourceUtils.GetString("NoDataForPrimaryKey", primaryKey.ValueArray));
            //                 }
            //                 else
            //                 {
            //                     return null;
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             throw new Exception(res.ResourceUtils.GetString("NoEntityNameForClass"));
            //         }
            //     }
            //
            //     if (useCache)
            //     {
            //         AddObjectToCache(instance as IPersistent, locale);
            //     }
            //
            //     // only set UseObjectCache if we are not refreshing
            //     if (objectToRefresh == null)
            //     {
            //         (instance as IPersistent).UseObjectCache = useCache;
            //     }
            //
            //     System.Diagnostics.Debug.Assert((instance as IPersistent).IsPersisted);
            // }
            //
            // //Return!
            // return instance;
            throw new NotImplementedException();
        }

        private void AddObjectToCache(IPersistent instance, string locale)
        {
            if (instance.UseObjectCache & instance.PrimaryKey.ContainsKey("Id"))
            {
                if (!_objectCache.Contains(locale))
                {
                    _objectCache.Add(locale, new Hashtable(10000));
                }

                Hashtable cache = _objectCache[locale] as Hashtable;
                if (!(cache.Contains(instance.PrimaryKey["Id"])))
                {
                    cache.Add(instance.PrimaryKey["Id"], instance);
                }
            }
        }

        public override void RemoveFromCache(IPersistent instance)
        {
            string locale = GetLocale();
            if (_objectCache.Contains(locale))
            {
                Hashtable cache = _objectCache[locale] as Hashtable;

                if (cache.Contains(instance.PrimaryKey["Id"]))
                {
                    cache.Remove(instance.PrimaryKey["Id"]);
                }
            }
        }

        public override void RefreshInstance(IPersistent persistentObject)
        {
            CheckStatus();

            if (!persistentObject.IsPersisted)
            {
                throw new InvalidOperationException(res.ResourceUtils.GetString("NoRefreshForNotPersisted"));
            }

            object throwAway = this.RetrieveInstance(persistentObject.GetType(), persistentObject.PrimaryKey, false, persistentObject) as IPersistent;
        }


        public override List<T> RetrieveListByParent<T>(Key primaryKey, string parentTableName, string childTableName, bool useCache)
        {
            CheckStatus();

            List<T> instances = new List<T>();

            DataRow parentRow = _dataSet.Tables[parentTableName].Rows.Find(this.FilterKey(primaryKey, _dataSet.Tables[parentTableName]));
            if (parentRow == null) return instances;

            DataRow[] rows = parentRow.GetChildRows(childTableName);

            for (int i = 0; i < rows.Length; i++)
            {
                DataRow row = rows[i];

                Key key = new Key();

                // Get primary keys for the row
                foreach (DataColumn col in _dataSet.Tables[parentTableName].ChildRelations[childTableName].ChildTable.PrimaryKey)
                {
                    key.Add(col.ColumnName, row[col]);
                }

                instances.Add((T)this.RetrieveInstance(typeof(T), key, useCache));
            }

            return instances;
        }

        private List<T> RetrieveList<T>( string filterField, object filterValue)
        {
            Dictionary<string, object> filterList = new Dictionary<string, object>();
            filterList.Add(filterField, filterValue);
            return RetrieveList<T>(filterList);
        }

        public List<T> RetrieveList<T>(IDictionary<string, object> filterList, string filterEntityName)
		{
			CheckStatus();
            string filter;
            if (filterList != null && filterList.Count > 0)
            {
                StringBuilder filterBuilder = new StringBuilder();
                foreach (var item in filterList)
                {
                    string value;
                    string oper = "=";
                    if (item.Value is string)
                    {
                        value = "'" + (string)item.Value + "'";
                        if (value.Contains("*"))
                        {
                            oper = "like";
                        }
                    }
                    else if (item.Value == null || item.Value.Equals(Guid.Empty))
                    {
                        value = "null";
                        oper = "is";
                    }
                    else if (item.Value is Guid)
                    {
                        value = "'" + item.Value.ToString() + "'";
                    }
                    else
                    {
                        value = item.Value.ToString();
                    }
                    if(filterBuilder.Length > 0)
                    {
                        filterBuilder.Append(" AND ");
                    }
                    filterBuilder.AppendFormat("{0} {1} {2}", item.Key, oper, value);
                }
                filter = filterBuilder.ToString();
            }
            else
            {
                filter = "";
            }
			List<T> instances = new List<T>();
			string entityName = null;
			
			// Find entity name
			if(filterEntityName != null & filterEntityName != "")
			{
				entityName = filterEntityName;
			}
			else if(this.Entity(typeof(T)) != null)
			{
				entityName = this.Entity(typeof(T)).Name;
			}

			if(entityName != null)
			{
				if(!_dataSet.Tables.Contains(entityName))
				{
					throw new ArgumentOutOfRangeException(entityName, entityName, res.ResourceUtils.GetString("CouldNotLoadList"));
				}
				DataRow[] rows = _dataSet.Tables[entityName].Select(filter, "", DataViewRowState.CurrentRows);
				foreach(DataRow row in rows)
				{
					Key key = new Key();

					// Get primary keys for the row
					foreach(DataColumn col in _dataSet.Tables[entityName].PrimaryKey)
					{
						key.Add(col.ColumnName, row[col]);
					}
					
					instances.Add((T)this.RetrieveInstance(typeof(T),key));
				}
			}
			else
			{
				//throw new Exception("Class has no entity name definition");
			}
			return instances;
		}

        public override List<T> RetrieveList<T>( IDictionary<string, object> filter)
        {
	        return this.RetrieveList<T>(filter, null);
        }

        public override void Persist(IPersistent instance)
		{
// 			CheckStatus();
//
// 			if(! instance.IsDeleted)
// 			{
// 			    RuleTools.DoOnFirstViolation(
// 			        objectToCheck: instance, 
// 			        action: ex => throw ex);
// 			}
//
// 			Type type = instance.GetType();
// 			DataRow row;
// 			DataTable table;
// 			Hashtable derivedRows = new Hashtable();
//
// 			//Find entity attribute
// 			EntityNameAttribute entity = this.Entity(type);
//
// 			if(entity != null)
// 			{
// 				string entityName = entity.Name;
//
// 				// Filter the row out by primary key
// 				table = _dataSet.Tables[entityName];
// 				row = table.Rows.Find(this.FilterKey(instance.PrimaryKey, table));
// 				
// 				if(row != null)
// 				{
// 					// In case of deleting, we go through all the routine, updating the rows, but
// 					// at the end we delete the rows. This is because we have to find out all the
// 					// derived tables. Could be done better, but this was the faster way now.
//
//
// //					// Row was found
// //					if(instance.IsDeleted)
// //					{
// //						// And we have to delete it, because the object is marked for removal
// //						row.Delete();
// //						// We do not really have to remove any child items, because underlying dataset
// //						// will take care.
// //
// //						return;
// //					}
// //					else
// //					{
// 						if(!instance.IsPersisted)
// 							// Object is marked as new, but we have found the row. Object is a duplicate
// 							throw new ArgumentException(res.ResourceUtils.GetString("ObjectWithSameKey"));
// //					}
// 				}
// 				else
// 				{
// 					// Row was not found
//
// 					if(instance.IsDeleted)
// 					{
// 						// but it has been deleted already, so we exit
// 						return;
// 					}
//
// //					if(instance.IsPersisted)
// //						// But the object says it has been persisted before, this is an error.
// //						throw new Exception("Data not found by provided primary key");
// //					else
// //					{
// 						// The object is new, so we create a new row
// 						row = table.NewRow();
//
// 						// And we set the primary key
// 						foreach(string key in instance.PrimaryKey.Keys)
// 						{
// 							row[key] = instance.PrimaryKey[key];
// 						}
// //					}
// 				}
// 			}
// 			else
// 			{
// 				throw new Exception(res.ResourceUtils.GetString("NoEntityNameForClass"));
// 			}
//
// 			// We have to check, if this entity supports row inheritance
// 			if(entity.InheritanceColumn != null)
// 			{
// 				// It does, so we fill the column with a type of ourselves
// 				row[entity.InheritanceColumn] = instance.GetType();
// 			}
//
// 			// Set all the remaining properties
// 			IList members = Reflector.FindMembers(type, typeof(EntityColumnAttribute), new Type[0]);
// 			foreach(MemberAttributeInfo mi in members)
// 			{
// 				DataRow propertyRow = row;
//
// 				// Get the column
// 				EntityColumnAttribute column = mi.Attribute as EntityColumnAttribute;
//
// 				// Check if we are digging into a derived entity
// 				if(column.OverridenEntityName != null)
// 				{
// 					if(derivedRows.ContainsKey(column.OverridenEntityName))
// 					{
// 						// We check if we already used this derived entity
// 						propertyRow = derivedRows[column.OverridenEntityName] as DataRow;
// 					}
// 					else
// 					{
// 						// We did not get the derived row, yet, so we get one
// 						// First we try to look it up in the data
// 						if(! _dataSet.Relations.Contains(column.OverridenEntityName))
// 						{
// 							throw new InvalidOperationException(res.ResourceUtils.GetString("EntityNotInDataSet", column.OverridenEntityName));
// 						}
//
// 						propertyRow = _dataSet.Relations[column.OverridenEntityName].ChildTable.Rows.Find(this.FilterKey(instance.PrimaryKey, _dataSet.Relations[column.OverridenEntityName].ChildTable));
// 						if(propertyRow == null)
// 						{
// 							// It was not stored, yet, we have to create a new row
// 							propertyRow = _dataSet.Relations[column.OverridenEntityName].ChildTable.NewRow();
//
// 							// And we set the primary key
// 							foreach(string key in instance.PrimaryKey.Keys)
// 							{
// 								propertyRow[key] = instance.PrimaryKey[key];
// 							}
// 						}
//
// 						// We add the row to the cache
// 						derivedRows.Add(column.OverridenEntityName, propertyRow);
// 					}
// 				}
//
// 				// Set the value
// 				if(mi.MemberInfo is PropertyInfo)
// 				{
// 					PropertyInfo pi = mi.MemberInfo as PropertyInfo;
//
// 					if(column.IsForeignKey)
// 					{
// 						// If this is a foreign key, we have to store all the keys in the reffered object
// 						// to the parent table. Naming = column name + key name.
// 						if(pi.GetValue(instance, new object[0]) != null)
// 						{
// 							// There is something stored, so we save the columns
// 							Key primaryKey = (pi.GetValue(instance, new object[0]) as IPersistent).PrimaryKey;
// 							foreach(string key in primaryKey.Keys)
// 							{
// 								SetValue(propertyRow, column.Name + key, primaryKey[key]);
// 							}
// 						}
// 						else
// 						{
// 							// Nothing is stored in the foreign key, we must store NULLs
// 							Key primaryKey = this.DummyKey(pi.DeclaringType);
// 							foreach(string key in primaryKey.Keys)
// 							{
// 								SetValue(propertyRow, column.Name + key, DBNull.Value);
// 							}
// 						}
// 					}
// 					else
// 					{
// 						SetValue(propertyRow, column.Name, pi.GetValue(instance, new object[0]));
// 					}
// 				}
// 				else
// 				{
// 					FieldInfo fi = mi.MemberInfo as FieldInfo;
//
// 					if(column.IsForeignKey)
// 					{
// 						// If this is a foreign key, we have to store all the keys in the reffered object
// 						// to the parent table. Naming = column name + key name.
// 						if(fi.GetValue(instance) != null)
// 						{
// 							// There is something stored, so we save the columns
// 							Key primaryKey = (fi.GetValue(instance) as IPersistent).PrimaryKey;
// 							foreach(string key in primaryKey.Keys)
// 							{
// 								SetValue(propertyRow, column.Name + key, primaryKey[key]);
// 							}
// 						}
// 						else
// 						{
// 							// Nothing is stored in the foreign key, we must store NULLs
// 							Key primaryKey = this.DummyKey(fi.DeclaringType);
// 							foreach(string key in primaryKey.Keys)
// 							{
// 								SetValue(propertyRow, column.Name + key, DBNull.Value);
// 							}
// 						}
// 					}
// 					else
// 					{
// 						SetValue(propertyRow, column.Name, fi.GetValue(instance));
// 					}
// 				}
// 			}
//
// 			try
// 			{
// 				if(instance.IsDeleted)
// 				{
// 					row.Delete();
// 				}
// 				else
// 				{
// 					// Finally we add the rows to the dataset
// 					if(row.RowState == DataRowState.Detached)
// 					{
// 						table.Rows.Add(row);
// 					}
// 				}
//
// 				foreach(string key in derivedRows.Keys)
// 				{
// 					DataRow derivedRow = derivedRows[key] as DataRow;
//
// 					if(instance.IsDeleted)
// 					{
// 						derivedRow.Delete();
// 					}
// 					else
// 					{
// 						if(derivedRow.RowState == DataRowState.Detached)
// 						{
// 							_dataSet.Relations[key].ChildTable.Rows.Add(derivedRow);
// 						}
// 					}
// 				}
// 			}
// 			catch(Exception)
// 			{
// 				// If we have added some rows already, we have to roll back, otherwise
// 				// data will be corrupted.
//
// 				if(row.RowState == DataRowState.Added) row.Delete();
//
// 				foreach(string key in derivedRows.Keys)
// 				{
// 					DataRow derivedRow = derivedRows[key] as DataRow;
//
// 					if(derivedRow.RowState == DataRowState.Added) row.Delete();
// 				}
//
// 				// Now we re-throw an exception, so the user sees something
//
// 				throw;
// 			}
//
// 			// after saving we always invalidate the cache
// 			RemoveFromCache(instance);
//
// 			// In case the object was new, we set a flag that it has just been persisted and we add it to cache, too
// 			if(! instance.IsPersisted)
// 			{
// 				//AddObjectToCache(instance, locale);
// 				instance.IsPersisted = true;
// 			}
//
//             // OnInstancePersisted(instance);
// 		    base.Persist(instance);
			throw new NotImplementedException();
		}

        public string DebugInfo()
		{
			return DebugClass.DataDebug(_dataSet);
		}

        public void DebugShow()
		{
			DebugClass.Show(_dataSet);
		}

        public string DebugChangesInfo()
		{	
			return DebugClass.DataDebug(_dataSet.GetChanges());
		}

        public void DebugChangesShow()
		{
			using(System.Data.DataSet chng=_dataSet.GetChanges())
			{
				if(chng ==null)
				{
					throw new Exception(res.ResourceUtils.GetString("NoChangesInDataSet"));
				}
				else
				{
					DebugClass.Show(chng);
				}
			}
		}

		public DataSet EmptyData()
		{
			return _dataSet.Clone();	
		}

        /// <summary>
        /// Persists internal dataset to file.
        /// </summary>
        /// <param name="fileName"></param>
        public void PersistToFile(string fileName, bool sort, 
	        List<IDatasetFormater> formaters = null)
        {
	        if (formaters == null)
			{
				formaters = new List<IDatasetFormater>();
			}
	        if(sort) formaters.Add(new SchemaItemSorter());
	        
	        CheckStatus();
			DataSet data = _dataSet;

	        foreach (IDatasetFormater formater in formaters)
	        {
		        data = formater.Format(data);
	        }

            data.ExtendedProperties.Add("ModelVersion", 
                OrigamEngine.VersionProvider.CurrentModelMetaVersion);
			data.WriteXml(fileName, XmlWriteMode.WriteSchema);
        }

        public override void FlushCache()
		{
			this._objectCache.Clear();
			GC.Collect();
		}

		public void MergeData(DataSet data)
		{	
            Version currentModelversion = OrigamEngine.VersionProvider.CurrentModelMeta;
			if (! data.ExtendedProperties.ContainsKey("ModelVersion")
                || !OrigamEngine.VersionProvider.IsCurrentMeta((string) data.ExtendedProperties["ModelVersion"])
                )
            {
                throw new Exception("Model imported was stored using an incompatible format. Model format version must be: " + currentModelversion+", but was: "+data.ExtendedProperties["ModelVersion"]);
            }
			_objectCache.Clear();

			ArrayList constraints = RemoveConstraints();
            MergeParams mergeParams = new MergeParams();
            mergeParams.TrueDelete = true;
			DatasetTools.MergeDataSet(_dataSet, data, null, mergeParams);

			EnforceConstraints(constraints);
		}

		public static void CheckInstanceRules(object instance)
		{
			IList members = Reflector.FindMembers(instance.GetType(), typeof(IModelElementRule), new Type[0]);
			foreach(MemberAttributeInfo mi in members)
			{
				IModelElementRule rule = mi.Attribute as IModelElementRule;

				Exception ex = rule.CheckRule(instance, mi.MemberInfo.Name);
                if (ex != null)
                {
                    throw ex;
                }
			}
		}

		private ArrayList RemoveConstraints()
		{
			ArrayList relations = new ArrayList();

			_dataSet.EnforceConstraints = false;

			foreach(DataTable table in _dataSet.Tables)
			{
				foreach(Constraint constraint in table.Constraints)
				{
					ForeignKeyConstraint fk = constraint as ForeignKeyConstraint;
					if(fk != null && fk.DeleteRule == System.Data.Rule.Cascade)
					{
						fk.DeleteRule = System.Data.Rule.None;
						relations.Add(fk);
					}
				}
			}

			return relations;
		}

		private void EnforceConstraints(ArrayList constraints)
		{
			foreach(ForeignKeyConstraint fk in constraints)
			{
				fk.DeleteRule = System.Data.Rule.Cascade;
			}

			//_dataSet.EnforceConstraints = true;
		}
		private void CheckStatus()
		{
			if(_dataSet == null)
			{
				throw new NullReferenceException(res.ResourceUtils.GetString("ProviderNotInitialized"));
			}
		}
        #endregion

        #region ICloneable Members

        public override object Clone()
		{
			return new DatabasePersistenceProvider();
		}

        #endregion

        #region IDisposable Members

        public override void Dispose()
		{
			DataStructureQuery = null;
			
			if(_dataSet != null)
			{
				_dataSet.Dispose();
				_dataSet = null;
			}

			DataService = null;
			_objectCache.Clear();
			LocalizationCache.Dispose();
		}

        public override T[] FullTextSearch<T>(string text)
        {
	        List<T> results = new List<T>();
            Guid dummy;
            bool isGuid = Guid.TryParse(text, out dummy);
            if (isGuid)
            {
                // search for the primary key
                results.AddRange(RetrieveList<T>("Id", text));
                results.AddRange(FilterResults<T>( "G01", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G02", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G03", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G04", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G05", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G06", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G07", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G08", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G09", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G10", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G11", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G12", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G13", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G14", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G15", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G16", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G17", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G18", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G19", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "G20", "SchemaItem", text, false));
            }
            else
            {
                results.AddRange(FilterResults<T>( "I01", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I02", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I03", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I04", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I05", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I06", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I07", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I08", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "I09", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "C01", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "C02", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "F01", "SchemaItem", text, false));
                results.AddRange(FilterResults<T>( "F02", "SchemaItem", text, false));
            }

            results.AddRange(FilterResults<T>( "Name", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "SS01", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "SS02", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "SS03", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "SS04", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "SS05", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "LS01", "SchemaItem", text, false));
            results.AddRange(FilterResults<T>( "M01", "SchemaItem", text, false));
            return results.ToArray();
        }

        private List<T> FilterResults<T>( string fieldName,
            string entityName, string searchText, bool useAlwaysWildcards)
        {
            try
            {
                Dictionary<string, object> filter = new Dictionary<string, object>();
                filter.Add(fieldName, FilterFormulaValue(searchText, useAlwaysWildcards));
                return RetrieveList<T>(filter, entityName);
            }
            catch
            {
                return new List<T>();
            }
        }

        private string FilterFormulaValue(string searchText, bool useAlwaysWildcards)
        {
            string wildcard = useAlwaysWildcards ? "*" : "";
            string text = searchText.Replace("'", "''");
            return wildcard + text + wildcard;
        }

        public override List<T> RetrieveListByPackage<T>( Guid packageId)
        {
            return RetrieveList<T>( "refSchemaExtensionId", packageId);
        }

        public override List<T> RetrieveListByCategory<T>(string category)
        {
            return RetrieveList<T>("ItemType", category);
        }

	    public override object RetrieveValue(Guid instanceId, Type parentType,
		    string fieldName)
	    {
		    Key primarykey = new Key {["Id"] = instanceId};
		    object instance = RetrieveInstance(
			    type: parentType, 
			    primaryKey: primarykey,
			    useCache: true,
			    throwNotFoundException: false);
		    if (instance == null) return null;
		    return Reflector.GetValue(parentType, instance, fieldName);
	    }

	    public override List<T> RetrieveListByGroup<T>( Key primaryKey)
        {
            return RetrieveList<T>("refParentGroupId", primaryKey["Id"]);
        }
        #endregion
    }

	public class SchemaItemSorter : IDatasetFormater
	{
		public DataSet Format(DataSet unsortedData)
		{
			DataSet data = unsortedData.Clone();
			data.EnforceConstraints = false;
			foreach (DataTable originalTable in unsortedData.Tables)
			{
				DataTable newTable = data.Tables[originalTable.TableName];
				if (originalTable.TableName == "SchemaItem")
				{
					foreach (DataRow row in originalTable.Select(
						"refParentItemId IS NULL", "ItemType, Id"))
					{
						newTable.ImportRow(row);
						ImportChildRows(originalTable, newTable, row);
					}
					foreach (DataRow row in originalTable.Rows)
					{
						if (newTable.Rows.Find(row["Id"]) == null)
						{
							newTable.ImportRow(row);
						}
					}
				} 
				else
				{
					foreach (DataRow row in originalTable.Rows)
					{
						newTable.ImportRow(row);
					}
				}
			}
			return data;
		}
		private void ImportChildRows(DataTable originalTable, DataTable newTable, DataRow parentRow)
		{
			foreach(DataRow row in parentRow.GetChildRows("ChildSchemaItem"))
			{
				newTable.ImportRow(row);
				ImportChildRows(originalTable, newTable, row);
			}
		}
    }

	internal class NullDataSetFormater: IDatasetFormater
	{
		public DataSet Format(DataSet data) => data;
	}
}
