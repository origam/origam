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
        private IDataService dataService => DataService.GetDataService();

        public DataController(ILogger<MetaDataController> log) : base(log)
        {
        }

        [HttpPost("[action]")]
        public IEnumerable<object[]> GetEntityData([FromBody] EntityGetData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            DataStructureQuery query = new DataStructureQuery(
                entity.RootEntity.ParentItemId,
                entity.Name,
                entityData.Filter,
                entityData.OrderingAsTuples,
                entityData.RowLimit);
            query.ColumnName = string.Join(";", entityData.ColumnNames);

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
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = entityData.DataStructureEntityId,
                Entity = entity.Name,
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter( "Id" , entityData.RowId));

            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);

            var row = dataSet.Tables[entity.Name].Rows[0];
            foreach (var colNameValuePair in entityData.NewValues)
            {
                row[colNameValuePair.Key] = colNameValuePair.Value;
                //row.AcceptChanges();
            }

            DataService.StoreData(entity.RootEntity.ParentItemId, dataSet,false,null);
        }

        [HttpPost("[action]")]
        public void InsertEntityData([FromBody] EntityInsertData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);

            DataRow row = dataSet.Tables[entity.Name].NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
            foreach (var colNameValuePair in entityData.NewValues)
            {
                row[colNameValuePair.Key] = colNameValuePair.Value;
            }
            dataSet.Tables[entity.Name].Rows.Add(row);
            DataService.StoreData(entity.RootEntity.ParentItemId, dataSet, false, null);
        }

        [HttpPost("[action]")]
        public void DeleteEntityData([FromBody] EntityDeleteData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = entityData.DataStructureEntityId,
                Entity = entity.Name,
                ColumnName = "Id",
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", entityData.RowIdToDelete));

            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);

            dataSet.Tables[entity.Name].Rows[0].Delete();
            DataService.StoreData(entity.RootEntity.ParentItemId, dataSet, false, null);
        }

        private static DataStructureEntity FindEntity(Guid id)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(id));
            return entity;
        }
    }
}
