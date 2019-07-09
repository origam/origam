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
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Server;
using Origam.ServerCore.Extensions;
using Origam.ServerCore.Model.Data;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controller
{
    public class DataController : AbstractController
    {
        protected internal IDataLookupService LookupService;
        private IDataService dataService => DataService.GetDataService();

        public DataController(ILogger<AbstractController> log) : base(log)
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
                .OnBoth<IActionResult,IActionResult>(UnwrapReturnValue);
        }

        [HttpPost("[action]")]
        public IActionResult GetRows([FromBody] GetRowsData getRowsData)
        {
            return FindItem<FormReferenceMenuItem>(getRowsData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(getRowsData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => GetRowsGetQuery(getRowsData, entityData))
                .OnSuccess(dataService.ExecuteDataReader)
                .OnSuccess(ToActionResult)
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }

        private Result<EntityData, IActionResult> GetEntityData(Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
        {
            return FindEntity(dataStructureEntityId)
                .OnSuccess(entity => new EntityData {MenuItem = menuItem, Entity = entity});
        }

        [HttpPut("[action]")]
        public IActionResult Row([FromBody] UpdateRowData updateRowData)
        {
            return FindItem<FormReferenceMenuItem>(updateRowData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(updateRowData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => 
                    GetRow(
                        entityData.Entity, 
                        updateRowData.DataStructureEntityId,
                        updateRowData.RowId))
                .OnSuccess(rowData => FillRow(rowData, updateRowData.NewValues))
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Update))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }


        [HttpPost("[action]")]
        public IActionResult Row([FromBody] NewRowData newRowData)
        {
            return FindItem<FormReferenceMenuItem>(newRowData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(newRowData.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => MakeEmptyRow(entityData.Entity))
                .OnSuccess(rowData => FillRow(newRowData, rowData))
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Create))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }


        [HttpPost("[action]")]
        public IActionResult NewEmptyRow([FromBody] NewEmptyRowData emptyRowData)
        {
            return FindItem<FormReferenceMenuItem>(emptyRowData.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem =>
                    GetEntityData(emptyRowData.DataStructureEntityId,
                        menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => MakeEmptyRow(entityData.Entity))
                .OnSuccess(rowData => PrepareNewRow(rowData))
                .OnSuccess(rowData => SubmitChange(rowData, Operation.Create))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }



        [HttpDelete("[action]")]
        public IActionResult Row([FromBody] DeleteRowData deleteRowData)
        {
            return
                FindItem<FormReferenceMenuItem>(deleteRowData.MenuId)
                    .OnSuccess(Authorize)
                    .OnSuccess(menuItem => GetEntityData(deleteRowData.DataStructureEntityId, menuItem))
                    .OnSuccess(CheckEntityBelongsToMenu)
                    .OnSuccess(entityData => GetRow(
                        entityData.Entity,
                        deleteRowData.DataStructureEntityId,
                        deleteRowData.RowIdToDelete))
                    .OnSuccess(rowData =>
                    {
                        rowData.Row.Delete();
                        return  SubmitDelete(rowData);
                    })
                    .OnSuccess(ThrowAwayReturnData)
                    .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }

        private IActionResult ThrowAwayReturnData(IActionResult arg)
        {
            return Ok();
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
                : Result.Fail<IEnumerable<object[]>, IActionResult>(BadRequest("Some of the supplied column names are not in the table."));
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
                string[] columnNamesWithoutPrimaryKey = FilterOutPrimaryKey(
                    lookupData.ColumnNames, dataTable.PrimaryKey);
                return dataTable.Rows
                    .Cast<DataRow>()
                    .Where(row => Filter(row, columnNamesWithoutPrimaryKey, 
                        lookupData.SearchText))
                    .Select(row => GetColumnValues(row, lookupData.ColumnNames));
            }
        }

        private static string[] FilterOutPrimaryKey(
            string[] columnNames, DataColumn[] primaryKey)
        {
            return columnNames
                .Where(
                    columnName => !primaryKey.Any(
                        dataColumn => dataColumn.ColumnName == columnName))
                .ToArray();
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

        private static RowData FillRow(NewRowData entityData, RowData rowData)
        {
            DatasetTools.ApplyPrimaryKey(rowData.Row);
            DatasetTools.UpdateOrigamSystemColumns(rowData.Row, true, SecurityManager.CurrentUserProfile().Id);
            FillRow(rowData, entityData.NewValues);
            rowData.Row.Table.Rows.Add(rowData.Row);
            return rowData;
        }

        private static RowData PrepareNewRow(RowData rowData)
        {
            DatasetTools.ApplyPrimaryKey(rowData.Row);
            DatasetTools.UpdateOrigamSystemColumns(rowData.Row, true, SecurityManager.CurrentUserProfile().Id);
            rowData.Row.Table.NewRow();
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
        

        private Result<DataStructureQuery, IActionResult> GetRowsGetQuery(
            GetRowsData entityQueryData, EntityData entityData)
        {
            DataStructureQuery query = new DataStructureQuery
            {
                Entity = entityData.Entity.Name,
                CustomFilters = string.IsNullOrWhiteSpace(entityQueryData.Filter)
                    ? null 
                    : entityQueryData.Filter,
                CustomOrdering = entityQueryData.OrderingAsTuples,
                RowLimit = entityQueryData.RowLimit,
                ColumnsInfo = new ColumnsInfo(entityQueryData.ColumnNames
                    .Select(colName =>
                    {
                        var field = entityData.Entity.Column(colName).Field;
                        return new ColumnData(
                            name: colName,
                            isVirtual: field is DetachedField,
                            defaultValue: (field as DetachedField)?.DefaultValue?.Value,
                            hasRelation: (field as DetachedField)?.ArrayRelation != null);
                    })
                    .ToList(),
                    renderSqlForDetachedFields:true),
                ForceDatabaseCalculation = true,
            };
            
            if (entityData.MenuItem.ListDataStructure != null)
            {
                if (entityData.MenuItem.ListEntity.Name == entityData.Entity.Name)
                {
                    query.MethodId = entityData.MenuItem.ListMethodId;
                    query.DataSourceId = entityData.MenuItem.ListDataStructure.Id;
                }
                else
                {
                    return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
                        .OnSuccess(CustomParameterService.GetFirstNonCustomParameter)
                        .OnSuccess(parameterName =>
                        {
                            query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
                            query.Parameters.Add(new QueryParameter(parameterName, entityQueryData.MasterRowId));
                            if (entityQueryData.MasterRowId == Guid.Empty)
                            {
                                return Result.Fail<DataStructureQuery, IActionResult>(BadRequest("MasterRowId cannot be empty"));
                            }
                            query.MethodId = entityData.MenuItem.MethodId;
                            return Result.Ok<DataStructureQuery, IActionResult>(query);
                        });
                }
            }
            else
            {
                query.MethodId = entityData.MenuItem.MethodId;
                query.DataSourceId = entityData.Entity.RootEntity.ParentItemId;
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

        private static void FillRow(RowData rowData, Dictionary<string, string> newValues)
        {
            foreach (var colNameValuePair in newValues)
            {
                Type dataType = rowData.Row.Table.Columns[colNameValuePair.Key].DataType;
                rowData.Row[colNameValuePair.Key] =
                    DatasetTools.ConvertValue(colNameValuePair.Value, dataType);
            }
        }

        private Result<T,IActionResult> FindItem<T>(Guid id) where T : class
        {
            T instance = ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id)) 
                as T;
            return instance == null
                ? Result.Fail<T, IActionResult>(NotFound("Object with requested id not found."))
                : Result.Ok<T, IActionResult>(instance);
        }

        private Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id)
                .OnFailureCompensate(error =>
                    Result.Fail<DataStructureEntity, IActionResult>(
                        NotFound("Requested DataStructureEntity not found. " + error.GetMessage())));
        }

        private IActionResult SubmitChange(RowData rowData, Operation operation)
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

            return Ok(SessionStore.GetChangeInfo(
                                requestingGrid: null, 
                                row: rowData.Row, 
                                operation: operation, 
                                RowStateProcessor: null));
        }

        private IActionResult SubmitDelete(RowData rowData)
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

            return Ok(SessionStore.GetDeleteInfo(
                                requestingGrid: null,
                                tableName: rowData.Row.Table.TableName,
                                objectId: null));
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
