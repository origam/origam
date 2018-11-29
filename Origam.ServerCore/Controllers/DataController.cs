using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
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

        [HttpPost("[action]")]
        public Dictionary<Guid, string> GetLookupLabels([FromBody] LookupLabelsData lookupData)
        {
            IDataLookupService lookupService =
                ServiceManager.Services.GetService<IDataLookupService>();

            return lookupData.LabelIds.ToDictionary(
                id => id,
                id =>
                {
                    object lookupResult =
                        lookupService.GetDisplayText(lookupData.LookupId, id, false, true, null);
                    return lookupResult is decimal
                        ? ((decimal) lookupResult).ToString("0.#")
                        : lookupResult.ToString();
                });
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupListEx([FromBody] LookupListData lookupData)
        {
            return FindEntity(lookupData.DataStructureEntityId)
                .OnSuccess(entity => GetRow(entity, lookupData.DataStructureEntityId, lookupData.Id))
                .OnSuccess(rowData => GetLookup(lookupData, rowData))
                .OnSuccess(ToActionResult)
                .OnBoth(UnwrapReturnValue);
        }

        [HttpPost("[action]")]
        public IActionResult EntitiesGet([FromBody] EntityGetData entityQueryData)
        {
            return FindItem<FormReferenceMenuItem>(entityQueryData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem =>
                {
                    return FindEntity(entityQueryData.DataStructureEntityId)
                        .OnSuccess(entity => new EntityData {MenuItem = menuItem, Entity = entity});
                })
                .OnSuccess(CheckBelongsToMenu)
                .OnSuccess(entityData => CreateEntitiesGetQuery(entityQueryData, entityData))
                .OnSuccess(query => ReadEntityData(entityQueryData, query))
                .OnSuccess(ToActionResult)
                .OnBoth(UnwrapReturnValue);
        }

        [HttpPut("[action]")]
        public IActionResult Entities([FromBody] EntityUpdateData entityData)
        {
            return FindEntity(entityData.DataStructureEntityId)
                .OnSuccess(entity => GetRow(entity, entityData.DataStructureEntityId, entityData.RowId))
                .OnSuccess(rowData => FillRow(rowData, entityData.NewValues))
                .OnSuccess(SubmitChange)
                .OnBoth(UnwrapReturnValue);
        }


        [HttpPost("[action]")]
        public IActionResult Entities([FromBody] EntityInsertData entityData)
        {
            return FindEntity(entityData.DataStructureEntityId)
                .OnSuccess(MakeEmptyRow)
                .OnSuccess(rowData => FillRow(entityData, rowData))
                .OnSuccess(SubmitChange)
                .OnBoth(UnwrapReturnValue);
        }

        [HttpDelete("[action]")]
        public IActionResult Entities([FromBody] EntityDeleteData entityData)
        {
            return
                FindEntity(entityData.DataStructureEntityId)
                    .OnSuccess(entity => GetRow(
                        entity,
                        entityData.DataStructureEntityId,
                        entityData.RowIdToDelete))
                    .OnSuccess(rowData =>
                    {
                        rowData.Row.Delete();
                        return SubmitChange(rowData);
                    })
                    .OnBoth(UnwrapReturnValue);
        }

        class EntityData
        {
            public FormReferenceMenuItem MenuItem { get; set; }
            public DataStructureEntity Entity { get; set; }
        }

        class RowData
        {
            public DataRow Row { get; set; }
            public DataStructureEntity Entity { get; set; }
        }

        private static IEnumerable<object[]> GetLookup(LookupListData lookupData, RowData rowData)
        {
            // Hashtable p = DictionaryToHashtable(request.Parameters);
            LookupListRequest internalRequest = new LookupListRequest();
            internalRequest.LookupId = lookupData.LookupId;
            internalRequest.FieldName = lookupData.Property;
            //internalRequest.ParameterMappings = p;
            internalRequest.CurrentRow = rowData.Row;
            internalRequest.ShowUniqueValues = lookupData.ShowUniqueValues;
            internalRequest.SearchText = lookupData.SearchText;
            internalRequest.PageSize = lookupData.PageSize;
            internalRequest.PageNumber = lookupData.PageNumber;
            DataTable dataTable = ServiceManager.Services
                .GetService<IDataLookupService>()
                .GetList(internalRequest);

            return dataTable.Rows
                .Cast<DataRow>()
                .Select(row => GetColumnValues(row, lookupData.ColumnNames));
        }

        private static object[] GetColumnValues(DataRow row, string[] columnNames)
        {
            return columnNames.Select(colName => row[colName]).ToArray();
        }

        private static RowData FillRow(EntityInsertData entityData, RowData rowData)
        {
            rowData.Row["Id"] = Guid.NewGuid();
            rowData.Row["RecordCreated"] = DateTime.Now;
            rowData.Row["RecordCreatedBy"] = SecurityManager.CurrentUserProfile().Id;
            FillRow(rowData, entityData.NewValues);
            rowData.Row.Table.Rows.Add(rowData.Row);
            return rowData;
        }

        private RowData MakeEmptyRow(DataStructureEntity entity)
        {
            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);

            DataTable table = dataSet.Tables[entity.Name];
            DataRow row = table.NewRow();
            return new RowData { Entity = entity, Row = row };
        }

        private Result<DataStructureQuery, IActionResult> CreateEntitiesGetQuery(
            EntityGetData entityQueryData, EntityData entityData)
        {
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceId = entityData.Entity.RootEntity.ParentItemId,
                Entity = entityData.Entity.Name,
                CustomFilters = entityQueryData.Filter,
                CustomOrdering = entityQueryData.OrderingAsTuples,
                RowLimit = entityQueryData.RowLimit,
                ColumnName = string.Join(";", entityQueryData.ColumnNames),
            };

            if (entityData.MenuItem.ListDataStructure == null)
            {
                query.MethodId = entityData.MenuItem.MethodId;
            }
            else
            {
                if (entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
                {
                    query.MethodId = entityData.MenuItem.ListMethodId;
                }
                else
                {
                    return FindItem<DataStructureMethod>(entityQueryData.MenuId)
                        .OnSuccess(CustomParameterService.GetFirstNonCustomParameter)
                        .OnSuccess(parameterName =>
                        {
                            query.Parameters.Add(new QueryParameter(parameterName, entityQueryData.MasterRowId));
                            query.MethodId = entityData.MenuItem.ListMethodId;
                            return Result.Ok<DataStructureQuery, IActionResult>(query);
                        });
                }
            }
            return Result.Ok<DataStructureQuery, IActionResult>(query);
        }

        private Result<RowData, IActionResult> GetRow( DataStructureEntity entity, Guid dataStructureEntityId, Guid rowId)
        {
            DataStructureQuery query = new DataStructureQuery
            {
                DataSourceType = QueryDataSourceType.DataStructureEntity,
                DataSourceId = dataStructureEntityId,
                Entity = entity.Name,
                EnforceConstraints = false
            };
            query.Parameters.Add(new QueryParameter("Id", rowId));

            DataSet dataSet = dataService.GetEmptyDataSet(
                entity.RootEntity.ParentItemId, CultureInfo.InvariantCulture);
            dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal,
                dataSet, null);
            DataTable dataSetTable = dataSet.Tables[entity.Name];
            if (dataSetTable.Rows.Count == 0)
            {
                return Result.Fail<RowData, IActionResult>(NotFound("Requested data row was not found."));
            }
            return Result.Ok<RowData, IActionResult>(
                new RowData{Row =dataSetTable.Rows[0], Entity = entity});
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

        private static void FillRow(RowData rowData, Dictionary<string, string> newValues)
        {
            foreach (var colNameValuePair in newValues)
            {
                Type dataType = rowData.Row.Table.Columns[colNameValuePair.Key].DataType;
                rowData.Row[colNameValuePair.Key] =
                    DatasetTools.ConvertValue(colNameValuePair.Value, dataType);
            }
        }

        private Result<T,IActionResult> FindItem<T>(Guid id)
        {
            T instance = (T)ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id));
            return instance == null
                ? Result.Fail<T, IActionResult>(BadRequest("Item not found."))
                : Result.Ok<T, IActionResult>(instance);

        }

        private Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id);
        }

        private IActionResult SubmitChange(RowData rowData)
        {
            try
            {
                DataService.StoreData(
                    dataStructureId: rowData.Entity.RootEntity.ParentItemId,
                    data: rowData.Row.Table.DataSet,
                    loadActualValuesAfterUpdate: false,
                    transactionId: null);
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

        private IActionResult ToActionResult(object obj)
        {
            return Ok(obj);
        }

        public IActionResult UnwrapReturnValue(Result<IActionResult, IActionResult> result)
        {
            if (result.IsSuccess) return result.Value;
            return result.Error;
        }

        private Result<EntityData, IActionResult> CheckBelongsToMenu(
            EntityData entityData)
        {
            return entityData.MenuItem.Screen.DataStructure.Id == entityData.Entity.RootEntity.ParentItemId
                ? Result.Ok<EntityData, IActionResult>(entityData)
                : Result.Fail<EntityData, IActionResult>(BadRequest("The requested Entity does not belong to the menu."));
        }

        private Result<FormReferenceMenuItem, IActionResult> Authorize(FormReferenceMenuItem menuItem)
        {
            return SecurityManager.GetAuthorizationProvider().Authorize(User, menuItem.Roles)
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(Forbid());
        }
    }
}
