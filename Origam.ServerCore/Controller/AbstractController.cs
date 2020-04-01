#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using IdentityServer4;
using System;
using System.Collections.Generic;
using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Server;
using Origam.ServerCore.Extensions;
using Origam.ServerCore.Model;
using Origam.ServerCore.Model.UIService;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controller
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("internalApi/[controller]")]
    public abstract class AbstractController: ControllerBase
    {
        protected readonly SessionObjects sessionObjects;
        protected class EntityData
        {
            public FormReferenceMenuItem MenuItem { get; set; }
            public DataStructureEntity Entity { get; set; }
        }
        // ReSharper disable once InconsistentNaming
        protected readonly ILogger<AbstractController> log;
        protected AbstractController(ILogger<AbstractController> log, SessionObjects sessionObjects)
        {
            this.log = log;
            this.sessionObjects = sessionObjects;
        }
        protected static MenuLookupIndex MenuLookupIndex {
            get
            {
                const string allowedLookups = "AllowedLookups";
                if(!OrigamUserContext.Context.ContainsKey(allowedLookups))
                {
                    OrigamUserContext.Context.Add(
                        allowedLookups, new MenuLookupIndex());
                }
                var lookupIndex = (MenuLookupIndex)OrigamUserContext.Context[
                    allowedLookups];
                return lookupIndex;
            }
        }
        protected IActionResult RunWithErrorHandler(Func<IActionResult> func)
        {
            try
            {
                return func();
            }
            catch(ArgumentOutOfRangeException ex)
            {
                return NotFound(ex.ActualValue);
            }
            catch (SessionExpiredException ex)
            {
                return NotFound(ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
        protected Result<FormReferenceMenuItem, IActionResult> Authorize(
            FormReferenceMenuItem menuItem)
        {
            return SecurityManager.GetAuthorizationProvider().Authorize(
                User, menuItem.Roles)
                ? Result.Success<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Failure<FormReferenceMenuItem, IActionResult>(Forbid());
        }
        protected Result<T,IActionResult> FindItem<T>(Guid id) where T : class
        {
            return !(ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id)) is T instance)
                ? Result.Failure<T, IActionResult>(
                    NotFound("Object with requested id not found."))
                : Result.Success<T, IActionResult>(instance);
        }
        protected Result<EntityData, IActionResult> GetEntityData(
            Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
        {
            return FindEntity(dataStructureEntityId)
                .Map(entity => 
                    new EntityData {MenuItem = menuItem, Entity = entity});
        }
        protected Result<EntityData, IActionResult> CheckEntityBelongsToMenu(
            EntityData entityData)
        {
            return (entityData.MenuItem.Screen.DataStructure.Id 
                == entityData.Entity.RootEntity.ParentItemId)
                ? Result.Success<EntityData, IActionResult>(entityData)
                : Result.Failure<EntityData, IActionResult>(
                    BadRequest("The requested Entity does not belong to the menu."));
        }
        protected Result<RowData, IActionResult> GetRow(
            IDataService dataService,
            DataStructureEntity entity, Guid dataStructureEntityId,
            Guid methodId, Guid rowId)
        {
            var rows = SessionStore.LoadRows(dataService, entity, 
                dataStructureEntityId, methodId, new List<Guid> { rowId });
            if(rows.Count == 0)
            {
                return Result.Failure<RowData, IActionResult>(
                    NotFound("Requested data row was not found."));
            }
            return Result.Success<RowData, IActionResult>(
                new RowData{Row = rows[0], Entity = entity});
        }
        protected IActionResult UnwrapReturnValue(
            Result<IActionResult, IActionResult> result)
        {
            return result.IsSuccess ? result.Value : result.Error;
        }
        protected Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id)
                .OnFailureCompensate(error =>
                    Result.Failure<DataStructureEntity, IActionResult>(
                        NotFound("Requested DataStructureEntity not found. " 
                        + error.GetMessage())));
        }
        protected IActionResult SubmitChange(
            RowData rowData, Operation operation)
        {
            try
            {
                DataService.StoreData(
                    dataStructureId: rowData.Entity.RootEntity.ParentItemId,
                    data: rowData.Row.Table.DataSet,
                    loadActualValuesAfterUpdate: false,
                    transactionId: null);
            }
            catch(DBConcurrencyException ex)
            {
                if(string.IsNullOrEmpty(ex.Message) 
                    && (ex.InnerException != null))
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
        protected Result<RowData, IActionResult> AmbiguousInputToRowData(
            AmbiguousInput input, IDataService dataService, 
            SessionObjects sessionObjects)
        {
            if(input.SessionFormIdentifier == Guid.Empty)
            {
                return FindItem<FormReferenceMenuItem>(input.MenuId)
                    .Bind(Authorize)
                    .Bind(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .Bind(CheckEntityBelongsToMenu)
                    .Bind(entityData => GetRow(
                        dataService,
                        entityData.Entity,
                        input.DataStructureEntityId,
                        Guid.Empty,
                        input.RowId));
            }
            else
            {
                return sessionObjects.UIService.GetRow(
                    input.SessionFormIdentifier, input.Entity, input.RowId);
            }
        }
        protected Result<Guid, IActionResult> AmbiguousInputToEntityId(
            AmbiguousInput input, IDataService dataService, 
            SessionObjects sessionObjects)
        {
            if(input.SessionFormIdentifier == Guid.Empty)
            {
                return FindItem<FormReferenceMenuItem>(input.MenuId)
                    .Bind(Authorize)
                    .Bind(menuItem => GetEntityData(
                        input.DataStructureEntityId, menuItem))
                    .Bind(CheckEntityBelongsToMenu)
                    .Bind(EntityDataToEntityId);
            }
            else
            {
                return sessionObjects.UIService.GetEntityId(
                    input.SessionFormIdentifier, input.Entity);
            }
        }
        protected Result<Guid, IActionResult> EntityDataToEntityId(
            EntityData entityData)
        {
            return Result.Ok<Guid, IActionResult>(entityData.Entity.EntityId);
        }
        protected IActionResult ToActionResult(object obj)
        {
            return Ok(obj);
        }
        
        protected Result<DataStructureQuery, IActionResult> AddMethodAndSource(
            object sessionFormIdentifier, Guid masterRowId, EntityData entityData,
            DataStructureQuery query)
        {
            bool isLazyLoaded = entityData.MenuItem.ListDataStructure != null;
            if (isLazyLoaded)
            {
                if (entityData.MenuItem.ListEntity.Name
                    == entityData.Entity.Name)
                {
                    query.MethodId = entityData.MenuItem.ListMethodId;
                    query.DataSourceId
                        = entityData.MenuItem.ListDataStructure.Id;
                    // get parameters from session store
                    var parameters = sessionObjects.UIService.GetParameters(
                        sessionFormIdentifier);
                    foreach (var key in parameters.Keys)
                    {
                        query.Parameters.Add(
                            new QueryParameter(key.ToString(),
                                parameters[key]));
                    }
                }
                else
                {
                    return FindItem<DataStructureMethod>(entityData.MenuItem.MethodId)
                        .Map(CustomParameterService.GetFirstNonCustomParameter)
                        .Bind(parameterName =>
                        {
                            query.DataSourceId
                                = entityData.Entity.RootEntity.ParentItemId;
                            query.Parameters.Add(new QueryParameter(
                                parameterName, masterRowId));
                            if (masterRowId == Guid.Empty)
                            {
                                return Result
                                    .Failure<DataStructureQuery, IActionResult>(
                                        BadRequest("MasterRowId cannot be empty"));
                            }

                            query.MethodId = entityData.MenuItem.MethodId;
                            return Result
                                .Success<DataStructureQuery, IActionResult>(query);
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

    }
}
