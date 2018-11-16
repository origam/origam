using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
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

        [HttpGet("[action]")]
        public IEnumerable<object> Entities([FromBody] EntityGetData entityData)
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

        [HttpPut("[action]")]
        public IActionResult Entities([FromBody] EntityUpdateData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            if (entity == null) return BadRequest();
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

            DataTable table = dataSet.Tables[entity.Name];
            var row = table.Rows[0];
            FillRow(row, entityData.NewValues, table);
            return SubmitChange(entity, dataSet);
        }

        [HttpPost("[action]")]
        public IActionResult Entities([FromBody] EntityInsertData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            if (entity == null) return BadRequest();
            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);

            DataTable table = dataSet.Tables[entity.Name];
            DataRow row = table.NewRow();
            row["Id"] = Guid.NewGuid();
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
            FillRow(row, entityData.NewValues, table);
            table.Rows.Add(row);
            return SubmitChange(entity, dataSet);
        }

        [HttpDelete("[action]")]
        public IActionResult Entities([FromBody] EntityDeleteData entityData)
        {
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            if (entity == null) return BadRequest();
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = entityData.DataStructureEntityId,
                Entity = entity.Name,
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", entityData.RowIdToDelete));

            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);
            if (dataSet.Tables[entity.Name].Rows.Count == 0) return NotFound();

            dataSet.Tables[entity.Name].Rows[0].Delete();
            return SubmitChange(entity, dataSet);
        }

        private static void FillRow(DataRow row, Dictionary<string, string> newValues, DataTable table)
        {
            foreach (var colNameValuePair in newValues)
            {
                Type dataType = table.Columns[colNameValuePair.Key].DataType;
                row[colNameValuePair.Key] =
                    DatasetTools.ConvertValue(colNameValuePair.Value, dataType);
            }
        }

        private static DataStructureEntity FindEntity(Guid id)
        {
            var entity = (DataStructureEntity)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(DataStructureEntity), new Key(id));
            return entity;
        }

        private IActionResult SubmitChange(DataStructureEntity entity, DataSet dataSet)
        {
            try
            {
                DataService.StoreData(entity.RootEntity.ParentItemId, dataSet, false, null);
            }
            catch (DBConcurrencyException ex)
            {
                if (string.IsNullOrEmpty(ex.Message) && ex.InnerException != null)
                {
                    return Conflict(ex.InnerException.Message);
                }
                return Conflict(ex.Message);
            }
            return Ok();
        }
    }
}
