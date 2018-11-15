using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.ServerCore.Models;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controllers
{
    public class DataController : AbstractController
    {
        public DataController(ILogger<MetaDataController> log) : base(log)
        {
        }

        [HttpPost("[action]")]
        public IEnumerable<object[]> GetEntityData([FromBody] EntityGetData entityData)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(entityData.DataStructureEntityId));

            var dataStructureId = Guid.Parse(entity.ParentNode.NodeId);

            DataStructureQuery query = new DataStructureQuery(
                dataStructureId,
                entity.Name,
                entityData.Filter,
                entityData.OrderingAsTuples,
                entityData.RowLimit);
            query.ColumnName = string.Join(";", entityData.ColumnNames);

            IDataService dataService = DataService.GetDataService();
            using (IDataReader reader = dataService.ExecuteDataReader(
                query, SecurityManager.CurrentPrincipal, null))
            {
                while (reader.Read())
                {
                    object[] values = new object[entityData.ColumnNames.Count];
                    reader.GetValues(values);
                    yield return values;
                }
            }
        }

        [HttpPost("[action]")]
        public void UpdateEntityData([FromBody] EntityUpdateData entityData)
        {
            var entity = (DataStructureEntity) ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(entityData.DataStructureEntityId));

            Guid dataStructureId = Guid.Parse(entity.ParentNode.NodeId);

            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = entityData.DataStructureEntityId,
                Entity = entity.Name,
                //ColumnName = string.Join(";", entityData.ColumnNames),
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter( "Id" , entityData.RowId));

            IDataService dataService = DataService.GetDataService();
            DataSet dataSet = dataService.GetEmptyDataSet(
                dataStructureId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);

            foreach (var colNameValuePair in entityData.NewValues)
            {
                dataSet.Tables[entity.Name].Rows[0][colNameValuePair.Key] = colNameValuePair.Value;
            }

            DataService.StoreData(dataStructureId, dataSet,false,null);
        }

        [HttpPost("[action]")]
        public void InsertEntityData([FromBody] EntityInsertData entityData)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(entityData.DataStructureEntityId));

            Guid dataStructureId = Guid.Parse(entity.ParentNode.NodeId);

            IDataService dataService = DataService.GetDataService();
            DataSet dataSet = dataService.GetEmptyDataSet(
                dataStructureId, CultureInfo.InvariantCulture);

            DataRow row = dataSet.Tables[entity.Name].NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
            foreach (var colNameValuePair in entityData.NewValues)
            {
                row[colNameValuePair.Key] = colNameValuePair.Value;
            }
            dataSet.Tables[entity.Name].Rows.Add(row);
            DataService.StoreData(dataStructureId, dataSet, false, null);
        }

        [HttpPost("[action]")]
        public void DeleteEntityData([FromBody] EntityDeleteData entityData)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(entityData.DataStructureEntityId));

            Guid dataStructureId = Guid.Parse(entity.ParentNode.NodeId);

            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = entityData.DataStructureEntityId,
                Entity = entity.Name,
                //ColumnName = string.Join(";", entityData.ColumnNames),
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", entityData.RowIdToDelete));

            IDataService dataService = DataService.GetDataService();
            DataSet dataSet = dataService.GetEmptyDataSet(
                dataStructureId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);

            dataSet.Tables[entity.Name].Rows[0].Delete();
            DataService.StoreData(dataStructureId, dataSet, false, null);
        }

    }
}
