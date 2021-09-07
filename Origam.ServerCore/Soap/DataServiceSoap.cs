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
using Origam.ServerCore.Controller;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
    public class DataServiceSoap: IDataServiceSoap
    {
        
        private readonly ILogger<AbstractController> log;

        public DataServiceSoap(ILogger<AbstractController> log)
        {
            this.log = log;
        }

        public Task<XElement> LoadDataAsync(string dataStructureId, string filterId, string defaultSetId,
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
            var elements = core.DataService.LoadData(dsId, fId, dId, sId, null,
                parameterCollection);
            var element = ToXElement(elements, "LoadDataResult");
            return Task.FromResult(element);
        }

        public Task<XElement> LoadData0Async(string dataStructureId, string filterId, string sortSetId, string defaultSetId)
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

            var elements = core.DataService.LoadData(dsId, fId, dId, sId, null);
            var element = ToXElement(elements, "LoadData0Result");
            return Task.FromResult(element);
        }

        public Task<XElement> LoadData1Async(string dataStructureId, string filterId, string defaultSetId, string sortSetId, string paramName1, string paramValue1)
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

            var elements = core.DataService.LoadData(dsId, fId, dId,
                sId, null, paramName1, paramValue1);
            var element = ToXElement(elements, "LoadData1Result");
            return Task.FromResult(element);
        }

        public Task<XElement> LoadData2Async(string dataStructureId, string filterId, 
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

            var elements = core.DataService.LoadData(
                dsId, fId, dId, sId, null,
                paramName1, paramValue1, paramName2, paramValue2);
            var element = ToXElement(elements, "LoadData2Result");
            return Task.FromResult(element);
        }

        public Task<XElement> ExecuteProcedureAsync(string procedureName, Parameter[] parameters)
        {
            log.Log(LogLevel.Information,"ExecuteProcedure");
            
            var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters);
            var elements = core.DataService.ExecuteProcedure(procedureName,
                parameterCollection, null);
            var element = ToXElement(elements, "ExecuteProcedureResult");
            return Task.FromResult(element);
        }

        public Task<XElement> StoreDataAsync(string dataStructureId, DataSet data,
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

            DataSet returnDatSet = core.DataService.StoreData(guid, data, loadActualValuesAfterUpdate, null);
            var element = ToXElement(returnDatSet, "StoreDataResult");
            return Task.FromResult(element);
        }

        public Task<XElement> StoreXmlAsync(string dataStructureId, XmlNode xml,
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

            DataSet dataSet = core.DataService.StoreData(guid, set2, loadActualValuesAfterUpdate, null);
            var element = ToXElement(dataSet, "StoreXmlResult");
            return Task.FromResult(element);
        }
        
        private static XElement ToXElement(DataSet dataSet, string rootElementName)
        {
            string defaultNamespace = "http://asapenginewebapi.advantages.cz/";
            var document = new XDocument();
            using (var xmlWriter = document.CreateWriter())
            {
                xmlWriter.WriteStartElement( rootElementName,defaultNamespace);
                dataSet.WriteXmlSchema(xmlWriter);
                dataSet.WriteXml(xmlWriter, XmlWriteMode.DiffGram);
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
                return document.Root;
            }
        }
    }
}