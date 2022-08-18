using System;
using System.Collections;
using System.Data;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Server.Controller;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Server
{
    public class DataServiceSoap: IDataServiceSoap
    {
        
        private readonly ILogger<AbstractController> log;

        public DataServiceSoap(ILogger<AbstractController> log)
        {
            this.log = log;
        }

        public Task<DataSet> LoadDataAsync(string dataStructureId, string filterId, string defaultSetId,
            string sortSetId, Parameter[] parameters)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "LoadData");
            }

            Guid dsId = new Guid(dataStructureId);
            Guid fId = string.IsNullOrWhiteSpace(filterId)
                ? Guid.Empty 
                : new Guid(filterId);
            Guid dId = string.IsNullOrWhiteSpace(defaultSetId)
                ? Guid.Empty 
                : new Guid(defaultSetId);
            Guid sId = string.IsNullOrWhiteSpace(sortSetId) 
                ? Guid.Empty 
                : new Guid(sortSetId);

            var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters);
            var dataSet = core.DataService.Instance.LoadData(dsId, fId, dId, sId, null,
                parameterCollection);
            return Task.FromResult(dataSet);
        }

        public Task<DataSet> LoadData0Async(string dataStructureId, string filterId, string sortSetId, string defaultSetId)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "LoadData0");
            }

            Guid dsId = new Guid(dataStructureId);
            Guid fId = string.IsNullOrWhiteSpace(filterId)
                ? Guid.Empty 
                : new Guid(filterId);
            Guid dId = string.IsNullOrWhiteSpace(defaultSetId)
                ? Guid.Empty 
                : new Guid(defaultSetId);
            Guid sId = string.IsNullOrWhiteSpace(sortSetId) 
                ? Guid.Empty 
                : new Guid(sortSetId);

            var dataSet = core.DataService.Instance.LoadData(dsId, fId, dId, sId, null);
            return Task.FromResult(dataSet);
        }

        public Task<DataSet> LoadData1Async(string dataStructureId, string filterId, string defaultSetId, string sortSetId, string paramName1, string paramValue1)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "LoadData1");
            }

            Guid dsId = new Guid(dataStructureId);
            Guid fId = string.IsNullOrWhiteSpace(filterId)
                ? Guid.Empty 
                : new Guid(filterId);
            Guid dId = string.IsNullOrWhiteSpace(defaultSetId)
                ? Guid.Empty 
                : new Guid(defaultSetId);
            Guid sId = string.IsNullOrWhiteSpace(sortSetId) 
                ? Guid.Empty 
                : new Guid(sortSetId);

            var dataSet = core.DataService.Instance.LoadData(dsId, fId, dId,
                sId, null, paramName1, paramValue1);
            return Task.FromResult(dataSet);
        }

        public Task<DataSet> LoadData2Async(string dataStructureId, string filterId, 
            string defaultSetId, string sortSetId, string paramName1, string paramValue1,
            string paramName2, string paramValue2)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "LoadData2");
            }

            Guid dsId = new Guid(dataStructureId);
            Guid fId = string.IsNullOrWhiteSpace(filterId)
                ? Guid.Empty 
                : new Guid(filterId);
            Guid dId = string.IsNullOrWhiteSpace(defaultSetId)
                ? Guid.Empty 
                : new Guid(defaultSetId);
            Guid sId = string.IsNullOrWhiteSpace(sortSetId) 
                ? Guid.Empty 
                : new Guid(sortSetId);

            var dataSet = core.DataService.Instance.LoadData(
                dsId, fId, dId, sId, null,
                paramName1, paramValue1, paramName2, paramValue2);
            return Task.FromResult(dataSet);
        }

        public Task<DataSet> ExecuteProcedureAsync(string procedureName, Parameter[] parameters)
        {
            log.Log(LogLevel.Information,"ExecuteProcedure");
            
            var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters);
            var dataSet = core.DataService.Instance.ExecuteProcedure(procedureName,
                parameterCollection, null);
            return Task.FromResult(dataSet);
        }

        public Task<DataSet> StoreDataAsync(string dataStructureId, DataSet data,
            bool loadActualValuesAfterUpdate)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "StoreData");
                log.Log(LogLevel.Information, data.GetXml());
            }
            
            Guid guid = new Guid(dataStructureId);
            IPersistenceService service = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure structure = service.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(guid)) as DataStructure;
            DataSet set = new DatasetGenerator(true).CreateDataSet(structure);
            foreach (DataTable table in data.Tables)
            {
                if (set.Tables.Contains(table.TableName))
                {
                    DataTable setTable = set.Tables[table.TableName];
                    table.ExtendedProperties.Clear();
                    foreach (DictionaryEntry entry in setTable.ExtendedProperties)
                    {
                        table.ExtendedProperties.Add(entry.Key, entry.Value);
                    }

                    foreach (DataColumn col in table.Columns)
                    {
                        if (setTable.Columns.Contains(col.ColumnName))
                        {
                            col.ExtendedProperties.Clear();
                            foreach (DictionaryEntry entry in setTable.Columns[col.ColumnName].ExtendedProperties)
                            {
                                col.ExtendedProperties.Add(entry.Key, entry.Value);
                            }
                        }
                    }
                }
            }

            DataSet returnDataSet = core.DataService.Instance.StoreData(guid, data, loadActualValuesAfterUpdate, null);
            return Task.FromResult(returnDataSet);
        }

        public Task<DataSet> StoreXmlAsync(string dataStructureId, XmlNode xml,
            bool loadActualValuesAfterUpdate)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "StoreXml");
                log.Log(LogLevel.Information, xml.OuterXml);
            }

            Guid guid = new Guid(dataStructureId);
            IPersistenceService service = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            DataStructure structure = service.SchemaProvider.RetrieveInstance(typeof(DataStructure), new ModelElementKey(guid)) as DataStructure;
            DataSet set = new DatasetGenerator(true).CreateDataSet(structure);
            set.EnforceConstraints = false;
            set.ReadXml(new XmlNodeReader(xml));
            DataSet set2 = new DatasetGenerator(true).CreateDataSet(structure);
            object obj2 = SecurityManager.CurrentUserProfile().Id;

            MergeParams mergeParams = new MergeParams
            {
                PreserveChanges = true,
                PreserveNewRowState = true,
                SourceIsFragment = false,
                ProfileId = obj2,
                TrueDelete = false
            };
            DatasetTools.MergeDataSet(set2, set, null, mergeParams);

            if(log.IsEnabled(LogLevel.Debug))
            {
                log.Log(LogLevel.Debug, "StoreXml - merged xml below");
                log.Log(LogLevel.Debug, set2.GetXml());
            }

            DataSet dataSet = core.DataService.Instance.StoreData(guid, set2, loadActualValuesAfterUpdate, null);
            return Task.FromResult(dataSet);
        }
    }
}