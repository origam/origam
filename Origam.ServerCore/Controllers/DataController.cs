using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.ServerCore.Models;
using Origam.Services;
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
        public Dictionary<Guid, string> GetLookupLabels([FromQuery] Guid lookupId,
            [FromQuery] Guid[] ids)
        {
            IDataLookupService lookupService =
                ServiceManager.Services.GetService<IDataLookupService>();

            return ids.ToDictionary(
                id => id,
                id =>
                {
                    object lookupResult =
                        lookupService.GetDisplayText(lookupId, id, false, true, null);
                    return lookupResult is decimal
                        ? ((decimal) lookupResult).ToString("0.#")
                        : lookupResult.ToString();
                });
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupListEx(LookupListData lookupData)
        {
            DataStructureEntity entity = FindEntity(lookupData.DataStructureEntityId);
            if (entity == null) return BadRequest();
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = lookupData.DataStructureEntityId,
                Entity = entity.Name,
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", lookupData.Id));

            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);

            DataRow row = dataSet.Tables[entity.Name].Rows[0];
           // Hashtable p = DictionaryToHashtable(request.Parameters);
            LookupListRequest internalRequest = new LookupListRequest();
            internalRequest.LookupId =lookupData.LookupId;
            internalRequest.FieldName = lookupData.Property;
            //internalRequest.ParameterMappings = p;
            internalRequest.CurrentRow = row;
            internalRequest.ShowUniqueValues = lookupData.ShowUniqueValues;
            internalRequest.SearchText = lookupData.SearchText;
            internalRequest.PageSize = lookupData.PageSize;
            internalRequest.PageNumber = lookupData.PageNumber;
            DataTable dataTable = ServiceManager.Services
                .GetService<IDataLookupService>()
                .GetList(internalRequest);
            return Ok(dataTable);
        }

        [HttpPost("[action]")]
        public IActionResult EntitiesGet([FromBody] EntityGetData entityData)
        {
            var menuItem = FindItem<FormReferenceMenuItem>(entityData.MenuId);
            bool authorize = SecurityManager.GetAuthorizationProvider().Authorize(User, menuItem.Roles);
            if (!authorize) return Forbid();
            DataStructureEntity entity = FindEntity(entityData.DataStructureEntityId);
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceId = entity.RootEntity.ParentItemId,
                Entity = entity.Name,
                CustomFilters = entityData.Filter,
                CustomOrdering = entityData.OrderingAsTuples,
                RowLimit = entityData.RowLimit,
                ColumnName = string.Join(";", entityData.ColumnNames),
                MethodId = menuItem.MethodId
            };

            return Ok(ReadEntityData(entityData, query));
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

        private IEnumerable<object> ReadEntityData(EntityGetData entityData, DataStructureQuery query)
        {
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

        private static void FillRow(DataRow row, Dictionary<string, string> newValues, DataTable table)
        {
            foreach (var colNameValuePair in newValues)
            {
                Type dataType = table.Columns[colNameValuePair.Key].DataType;
                row[colNameValuePair.Key] =
                    DatasetTools.ConvertValue(colNameValuePair.Value, dataType);
            }
        }

        private static T FindItem<T>(Guid id)
        {
            return (T)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id));
        }

        private static DataStructureEntity FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id);
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
