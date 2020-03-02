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

using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Server;
using Origam.ServerCore.Extensions;
using Origam.ServerCore.Model.UIService;
using Origam.Workbench.Services;

namespace Origam.ServerCore.Controller
{
    [Authorize]
    [ApiController]
    [Route("internalApi/[controller]")]
    public abstract class AbstractController: ControllerBase
    {
        protected class EntityData
        {
            public FormReferenceMenuItem MenuItem { get; set; }
            public DataStructureEntity Entity { get; set; }
        }
        // ReSharper disable once InconsistentNaming
        protected readonly ILogger<AbstractController> log;
        protected AbstractController(ILogger<AbstractController> log)
        {
            this.log = log;
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
                ? Result.Ok<FormReferenceMenuItem, IActionResult>(menuItem)
                : Result.Fail<FormReferenceMenuItem, IActionResult>(Forbid());
        }
        protected Result<T,IActionResult> FindItem<T>(Guid id) where T : class
        {
            return !(ServiceManager.Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T), new Key(id)) is T instance)
                ? Result.Fail<T, IActionResult>(
                    NotFound("Object with requested id not found."))
                : Result.Ok<T, IActionResult>(instance);
        }
        protected Result<EntityData, IActionResult> GetEntityData(
            Guid dataStructureEntityId, FormReferenceMenuItem menuItem)
        {
            return FindEntity(dataStructureEntityId)
                .OnSuccess(entity 
                    => new EntityData {MenuItem = menuItem, Entity = entity});
        }
        protected Result<EntityData, IActionResult> CheckEntityBelongsToMenu(
            EntityData entityData)
        {
            return (entityData.MenuItem.Screen.DataStructure.Id 
                == entityData.Entity.RootEntity.ParentItemId)
                ? Result.Ok<EntityData, IActionResult>(entityData)
                : Result.Fail<EntityData, IActionResult>(
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
                return Result.Fail<RowData, IActionResult>(
                    NotFound("Requested data row was not found."));
            }
            return Result.Ok<RowData, IActionResult>(
                new RowData{Row = rows[0], Entity = entity});
        }
        protected IActionResult UnwrapReturnValue(
            Result<IActionResult, IActionResult> result)
        {
            return result.IsSuccess ? result.Value : result.Error;
        }
        private Result<DataStructureEntity, IActionResult> FindEntity(Guid id)
        {
            return FindItem<DataStructureEntity>(id)
                .OnFailureCompensate(error =>
                    Result.Fail<DataStructureEntity, IActionResult>(
                        NotFound("Requested DataStructureEntity not found. " 
                        + error.GetMessage())));
        }
    }
}
