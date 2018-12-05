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
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.ServerCore.Models;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controllers
{
    public class DataController : AbstractController
    {
        protected internal IDataLookupService LookupService;
        private IDataService dataService => DataService.GetDataService();

        public DataController(ILogger<MetaDataController> log) : base(log)
        {
            LookupService = ServiceManager.Services.GetService<IDataLookupService>();
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupLabels([FromBody] LookupLabelsData lookupData)
        {
            var menuResult = FindItem<FormReferenceMenuItem>(lookupData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => CheckLookupIsAllowedInMenu(menuItem, lookupData.LookupId));
            if (menuResult.IsFailure) return menuResult.Error;

            Dictionary<Guid, string> labelDictionary = lookupData.LabelIds.ToDictionary(
                id => id,
                id =>
                {
                    object lookupResult = LookupService
                        .GetDisplayText(lookupData.LookupId, id, false, true, null);
                    return lookupResult is decimal
                        ? ((decimal) lookupResult).ToString("0.#")
                        : lookupResult.ToString();
                });
            return Ok(labelDictionary);
        }

        [HttpPost("[action]")]
        public IActionResult GetLookupListEx([FromBody] LookupListData lookupData)
        {
            return FindItem<FormReferenceMenuItem>(lookupData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => CheckLookupIsAllowedInMenu(menuItem, lookupData.LookupId))
                .OnSuccess(menuItem => GetEntityData(lookupData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => GetRow(entityData.Entity, lookupData.DataStructureEntityId, lookupData.Id))
                .OnSuccess(rowData => GetLookupRows(lookupData, rowData))
                .OnSuccess(ToActionResult)
                .OnBoth(UnwrapReturnValue);
        }

        [HttpPost("[action]")]
        public IActionResult EntitiesGet([FromBody] EntityGetData entityQueryData)
        {
            return FindItem<FormReferenceMenuItem>(entityQueryData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(entityQueryData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => CreateEntitiesGetQuery(entityQueryData, entityData))
                .OnSuccess(ReadEntityData)
                .OnSuccess(ToActionResult)
                .OnBoth(UnwrapReturnValue);
        }

        private Result<EntityData, IActionResult> GetEntityData(Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
        {
            return FindEntity(dataStructureEntityId)
                .OnSuccess(entity => new EntityData {MenuItem = menuItem, Entity = entity});
        }

        [HttpPut("[action]")]
        public IActionResult Entities([FromBody] EntityUpdateData entityUpdateData)
        {
            return FindItem<FormReferenceMenuItem>(entityUpdateData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(entityUpdateData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => 
                    GetRow(
                        entityData.Entity, 
                        entityUpdateData.DataStructureEntityId,
                        entityUpdateData.RowId))
                .OnSuccess(rowData => FillRow(rowData, entityUpdateData.NewValues))
                .OnSuccess(SubmitChange)
                .OnBoth(UnwrapReturnValue);
        }


        [HttpPost("[action]")]
        public IActionResult Entities([FromBody] EntityInsertData entityInsertData)
        {
            return FindItem<FormReferenceMenuItem>(entityInsertData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(entityInsertData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => MakeEmptyRow(entityData.Entity))
                .OnSuccess(rowData => FillRow(entityInsertData, rowData))
                .OnSuccess(SubmitChange)
                .OnBoth(UnwrapReturnValue);
        }

        [HttpDelete("[action]")]
        public IActionResult Entities([FromBody] EntityDeleteData entityDeleteData)
        {
            return
                FindItem<FormReferenceMenuItem>(entityDeleteData.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => GetEntityData(entityDeleteData.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRow(
                        entityData.Entity,
                        entityDeleteData.DataStructureEntityId,
                        entityDeleteData.RowIdToDelete))
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

        private Result<IEnumerable<object[]>,IActionResult> GetLookupRows(LookupListData lookupData, RowData rowData)
        {
            // Hashtable p = DictionaryToHashtable(request.Parameters);
            LookupListRequest internalRequest = new LookupListRequest();
            internalRequest.LookupId = lookupData.LookupId;
            internalRequest.FieldName = lookupData.Property;
            //internalRequest.ParameterMappings = p;
            internalRequest.CurrentRow = rowData.Row;
            internalRequest.ShowUniqueValues = lookupData.ShowUniqueValues;
            internalRequest.SearchText = "%" + lookupData.SearchText + "%";
            internalRequest.PageSize = lookupData.PageSize;
            internalRequest.PageNumber = lookupData.PageNumber;
            DataTable dataTable = LookupService.GetList(internalRequest);

            return AreColumnNamesValid(lookupData, dataTable)
                ? Result.Ok<IEnumerable<object[]>, IActionResult>(GetRowData(lookupData, dataTable))
                : Result.Fail<IEnumerable<object[]>, IActionResult>( BadRequest("Some of the supplied column names are not in the table."));
        }

        private static bool AreColumnNamesValid(LookupListData lookupData, DataTable dataTable)
        {
            var actualColumnNames = dataTable.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
            return lookupData.ColumnNames
                .All(colName => actualColumnNames.Contains(colName));
        }

        private IEnumerable<object[]> GetRowData(LookupListData lookupData, DataTable dataTable)
        {
            var lookup = FindItem<DataServiceDataLookup>(lookupData.LookupId).Value;
            if (lookup.IsFilteredServerside || string.IsNullOrEmpty(lookupData.SearchText))
            {
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Select(row => GetColumnValues(row, lookupData.ColumnNames));
            }
            else
            {
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Where(row => Filter(row, lookupData.ColumnNames, lookupData.SearchText))
                    .Select(row => GetColumnValues(row, lookupData.ColumnNames));
            }
        }

        private static bool Filter(DataRow row, string[] columnNames, string likeParameter)
        {
            return columnNames
                .Select(colName => row[colName])
                .Any(colValue => 
                    colValue.ToString().Contains(likeParameter,StringComparison.InvariantCultureIgnoreCase));
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
                ColumnNames = entityQueryData.ColumnNames
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

        private IEnumerable<object> ReadEntityData(DataStructureQuery query)
        {
            using (IDataReader reader = dataService.ExecuteDataReader(
                query, SecurityManager.CurrentPrincipal, null))
            {
                while (reader.Read())
                {
                    object[] values = new object[query.ColumnNames.Length];
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

        private Result<FormReferenceMenuItem, IActionResult> CheckLookupIsAllowedInMenu(FormReferenceMenuItem menuItem, Guid lookupId)
        {
            if (!MenuLookupIndex.HasDataFor(menuItem.Id)){
                XmlOutput xmlOutput = FormXmlBuilder.GetXml(menuItem.Id);
                MenuLookupIndex.AddIfNotPresent(menuItem.Id, xmlOutput.ContainedLookups);
            }

            return MenuLookupIndex.IsAllowed(menuItem.Id, lookupId)
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(BadRequest("Lookup is not referenced in any entity in the Menu item"));
        }

        private Result<EntityData, IActionResult> CheckEntityBelongsToMenu(
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
