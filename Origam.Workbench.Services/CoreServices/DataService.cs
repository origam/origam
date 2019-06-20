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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Origam.DA;
using Origam.DA.Service;

namespace Origam.Workbench.Services.CoreServices
{
	/// <summary>
	/// Summary description for DataService.
	/// </summary>
	public class DataService
	{
        //private static IDataService _dataService = null;
        private static IDictionary<string, IDataService> _dataServiceDictionary = new Dictionary<string, IDataService>();

        public static IDataService GetDataService()
        {
            return GetDataService(null);
        }
        public static IDataService GetDataService(Platform deployPlatform)
        {
            string dictionarykey = "primary";
            if (deployPlatform != null)
            {
                dictionarykey = deployPlatform.Name;
            }
            KeyValuePair<string, IDataService> keyValue = 
                _dataServiceDictionary.Where(dict => dict.Key == dictionarykey)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(keyValue.Key))
            {
                _dataServiceDictionary.Add(dictionarykey, CreateDataService(deployPlatform));
            }
            return _dataServiceDictionary.Where(dict => dict.Key == dictionarykey).First().Value;
        }

        private static IDataService CreateDataService(Platform deployPlatform)
        {
            IDataService dataService = null;
            IPersistenceService persistence = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
            OrigamSettings settings
                = ConfigurationManager.GetActiveConfiguration();
            if (settings == null)
            {
                throw new NullReferenceException(
                    ResourceUtils.GetString("ErrorSettingsNotFound"));
            }
            string assembly = settings.DataDataService
                .Split(",".ToCharArray())[0].Trim();
            string classname = settings.DataDataService
                .Split(",".ToCharArray())[1].Trim();

            if (deployPlatform != null)
            {
                assembly = deployPlatform.DataService
                .Split(",".ToCharArray())[0].Trim();
                classname = deployPlatform.DataService
                    .Split(",".ToCharArray())[1].Trim();
            }
            dataService
            = Reflector.InvokeObject(assembly, classname) as IDataService;
            dataService.ConnectionString = settings.DataConnectionString;
            dataService.BulkInsertThreshold = settings.DataBulkInsertThreshold;
            dataService.UpdateBatchSize = settings.DataUpdateBatchSize;
            dataService.StateMachine = ServiceManager.Services.GetService(
                typeof(IStateMachineService)) as IStateMachineService;
            dataService.AttachmentService = ServiceManager.Services.GetService(
                typeof(IAttachmentService)) as IAttachmentService;
            dataService.UserDefinedParameters = true;
            (dataService as AbstractDataService).PersistenceProvider
                = persistence.SchemaProvider;
            if (deployPlatform != null)
            {
                dataService.ConnectionString = deployPlatform.DataConnectionString;
            }
            return dataService;
        }

        
        public static void ClearDataService()
        {
            foreach (var keyValue in _dataServiceDictionary)
            {
                IDataService dataService = keyValue.Value;
                dataService?.Dispose();
                dataService = null;
            }
            _dataServiceDictionary.Clear();
        }

        internal DataService()
		{
		}

		public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId, Guid sortSetId, string transactionId)
		{
            return LoadData(dataStructureId, methodId, defaultSetId, sortSetId, transactionId, null);
		}

		public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId, Guid sortSetId, string transactionId, string paramName1, object paramValue1)
		{
			if (paramName1 == null)
			{
				throw new ArgumentNullException(nameof(paramName1));
			}
			QueryParameterCollection p = new QueryParameterCollection();
			p.Add(new QueryParameter(paramName1, paramValue1));

			return LoadData(dataStructureId, methodId, defaultSetId, sortSetId, transactionId, p);
		}

		public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId, Guid sortSetId, string transactionId, string paramName1, object paramValue1, string paramName2, object paramValue2)
		{
			QueryParameterCollection p = new QueryParameterCollection();
			p.Add(new QueryParameter(paramName1, paramValue1));
			p.Add(new QueryParameter(paramName2, paramValue2));
			
			return LoadData(dataStructureId, methodId, defaultSetId, sortSetId, transactionId, p);
		}

		public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
            Guid sortSetId, string transactionId, QueryParameterCollection parameters)
		{
			return LoadData(dataStructureId, methodId, defaultSetId, sortSetId, transactionId, parameters, null);
		}

        public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId, 
            Guid sortSetId, string transactionId, QueryParameterCollection parameters, DataSet currentData)
        {
            return LoadData(dataStructureId, methodId, defaultSetId, sortSetId, transactionId,
                parameters, currentData, null, null);
        }

        public static DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId, 
            Guid sortSetId, string transactionId, QueryParameterCollection parameters, DataSet currentData,
            string entity, string columnName)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
			DataStructureQuery q = new DataStructureQuery(dataStructureId, methodId, defaultSetId, sortSetId);
			q.ColumnsInfo = new ColumnsInfo(columnName);
            q.Entity = entity;
			if(parameters != null)
			{
				q.Parameters.AddRange(parameters);
			}
			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", q);
			if(currentData != null)
			{
				dataServiceAgent.Parameters.Add("Data", currentData);
                q.EnforceConstraints = currentData.EnforceConstraints;
			}
            if (!q.ColumnsInfo.IsEmpty)
            {
                q.EnforceConstraints = false;
            }
			dataServiceAgent.TransactionId = transactionId;
			dataServiceAgent.Run();
			DataSet ds = dataServiceAgent.Result as DataSet;
			return ds;
		}

		public static DataSet LoadRow(Guid dataStructureEntityId, QueryParameterCollection parameters, DataSet currentData, string transactionId)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
			DataStructureQuery q = new DataStructureQuery(dataStructureEntityId);
			q.DataSourceType = QueryDataSourceType.DataStructureEntity;
			if(parameters != null)
			{
				q.Parameters.AddRange(parameters);
			}
			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", q);
			if(currentData != null)
			{
				dataServiceAgent.Parameters.Add("Data", currentData);
			}
			dataServiceAgent.TransactionId = transactionId;
			dataServiceAgent.Run();
			DataSet ds = dataServiceAgent.Result as DataSet;
			return ds;
		}

		public static DataSet StoreData(Guid dataStructureId, DataSet data, bool loadActualValuesAfterUpdate, string transactionId)
		{
			return StoreData(dataStructureId, Guid.Empty, data, loadActualValuesAfterUpdate, transactionId);
		}

		public static DataSet StoreData(Guid dataStructureId, Guid methodId, DataSet data, bool loadActualValuesAfterUpdate, string transactionId)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			DataStructureQuery q = new DataStructureQuery(dataStructureId, methodId);
			q.LoadActualValuesAfterUpdate = loadActualValuesAfterUpdate;

			dataServiceAgent.MethodName = "StoreDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", q);
			dataServiceAgent.Parameters.Add("Data", data);
			dataServiceAgent.TransactionId = transactionId;

			dataServiceAgent.Run();

			DataSet ds = dataServiceAgent.Result as DataSet;

			return ds;
		}

		public static DataSet ExecuteProcedure(string procedureName, QueryParameterCollection parameters, string transactionId)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			Hashtable ht = new Hashtable(parameters.Count);
			foreach(QueryParameter p in parameters)
			{
				ht.Add(p.Name, p.Value);
			}

			dataServiceAgent.MethodName = "ExecuteProcedure";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Name", procedureName);
			dataServiceAgent.Parameters.Add("Parameters", ht);
			dataServiceAgent.TransactionId = transactionId;

			dataServiceAgent.Run();

			DataSet ds = dataServiceAgent.Result as DataSet;

			return ds;
		}

		public static long ReferenceCount(Guid entityId, object value, string transactionId)
		{
			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			dataServiceAgent.MethodName = "ReferenceCount";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("EntityId", entityId);
			dataServiceAgent.Parameters.Add("Value", value);
			dataServiceAgent.TransactionId = transactionId;

			dataServiceAgent.Run();

			long result = (long)dataServiceAgent.Result;

			return result;
		}

        public static string EntityDdl(Guid entityId)
        {
            IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
            dataServiceAgent.MethodName = "EntityDdl";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add("EntityId", entityId);
            dataServiceAgent.Run();
            string result = (string)dataServiceAgent.Result;
            return result;
        }

        public static string[] FieldDdl(Guid fieldId)
        {
            IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
            dataServiceAgent.MethodName = "FieldDdl";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add("FieldId", fieldId);
            dataServiceAgent.Run();
            string[] result = (string[])dataServiceAgent.Result;
            return result;
        }

        public static string ExecuteSql(string command)
        {
            IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(
                typeof(IBusinessServicesService)) 
                as IBusinessServicesService).GetAgent("DataService", null, null);
            dataServiceAgent.MethodName = "ExecuteSql";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add("Command", command);
            dataServiceAgent.Run();
            string result = (string)dataServiceAgent.Result;
            return result;
        }
    }
}
