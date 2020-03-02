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
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.Schema.MenuModel;
using Origam.Server;
using Origam.ServerCore.Model.Blob;
using Origam.ServerCore.Model.UIService;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controller
{
    [Controller]
    [Route("internalApi/[controller]")]
    public class BlobController : AbstractController
    {
        private readonly SessionObjects sessionObjects;
        private readonly IDataService dataService;
        public BlobController(
            SessionObjects sessionObjects, 
            ILogger<AbstractController> log) : base(log)
        {
            this.sessionObjects = sessionObjects;
            dataService = DataService.GetDataService();
        }
        [HttpPost("[action]")]
        public IActionResult DownloadToken(
            [FromBody][Required]BlobDownloadTokenInput input)
        {
            return FindItem<FormReferenceMenuItem>(input.MenuId)
                .OnSuccess(Authorize)
                .OnSuccess(menuItem => GetEntityData(
                    input.DataStructureEntityId, menuItem))
                .OnSuccess(CheckEntityBelongsToMenu)
                .OnSuccess(entityData => 
                    GetRow(
                        dataService,
                        entityData.Entity, 
                        input.DataStructureEntityId,
                        Guid.Empty,
                        input.RowId))
                .OnSuccess(rowData => CreateToken(input, rowData))
                .OnBoth<IActionResult, IActionResult>(UnwrapReturnValue);
        }

        private IActionResult CreateToken(
            BlobDownloadTokenInput input, RowData rowData)
        {
            var token = Guid.NewGuid();
            sessionObjects.SessionManager.AddBlobDownloadRequest(
                token,
                new BlobDownloadRequest(
                    rowData.Row, 
                    input.Parameters, 
                    input.Property, 
                    input.IsPreview));
            return Ok(token);
        }
    }
}