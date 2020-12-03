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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Excel;
using Origam.Server;
using Origam.ServerCommon;
using Origam.ServerCore.Model.UIService;

namespace Origam.ServerCore.Controller
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("internalApi/[controller]")]
    public class ExcelExportController: AbstractController
    {
        
        public ExcelExportController(ILogger<AbstractController> log,
            SessionObjects sessionObjects) : base(log, sessionObjects)
        {
        }
        
        [HttpPost("[action]")]
        public IActionResult GetFileUrl(
            [FromBody][Required]ExcelExportInput input)
        {
            return RunWithErrorHandler(() =>
            {
                var entityExportInfo = new EntityExportInfo
                {
                    Entity = input.Entity,
                    Fields = input.Fields,
                    RowIds = new List<object>(), //input.RowIds.Cast<object>().ToList(),
                    SessionFormIdentifier = input.SessionFormIdentifier,
                    Store = sessionObjects.SessionManager
                        .GetSession(new Guid(input.SessionFormIdentifier))
                };
                Guid itemId = Guid.NewGuid();
                sessionObjects.SessionManager
                    .AddExcelFileRequest( itemId, entityExportInfo);
                return Ok("/internalApi/ExcelExport/"+itemId);
            });
        }

        [AllowAnonymous]
        [HttpGet("{itemId:guid}")]
        public IActionResult Get(Guid itemId)
        {
            return RunWithErrorHandler(() =>
            {
                try
                {
                    EntityExportInfo entityExportInfo =
                        sessionObjects.SessionManager.GetExcelFileRequest(itemId);
                    return entityExportInfo == null 
                        ? BadRequest($"No data for id: {itemId}") 
                        : GetExcelFile(entityExportInfo);
                }
                finally
                {
                    sessionObjects.SessionManager.RemoveExcelFileRequest(itemId); 
                }
            });
        }

        private IActionResult GetExcelFile(EntityExportInfo entityExportInfo)
        {
            var excelEntityExporter = new ExcelEntityExporter();
            using (MemoryStream excelStream = new MemoryStream())
            {
                excelEntityExporter
                    .FillWorkBook(entityExportInfo)
                    .Write(excelStream);
                if (excelEntityExporter.ExportFormat == ExcelFormat.XLS)
                {
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.Headers.Add(
                        "content-disposition", "attachment; filename=export.xls");
                }
                else
                {
                    Response.ContentType
                        = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.Headers.Add(
                        "content-disposition", "attachment; filename=export.xlsx");
                }
                return File(excelStream.ToArray(), Response.ContentType);
            }
        }
    }
}