#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NPOI.SS.UserModel;
using Origam.DA;
using Origam.Excel;
using Origam.Server.Model.Excel;

namespace Origam.Server.Controller;

[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
[ApiController]
[Route("internalApi/[controller]")]
public class ExcelExportController: AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;
        
    public ExcelExportController(
        ILogger<ExcelExportController> log,
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        IWebHostEnvironment environment
        ) : base(log, sessionObjects, environment)
    {
        this.localizer = localizer;
    }

    [HttpPost("[action]")]
    public IActionResult GetFile(
        [FromBody][Required]ExcelExportInput input)
    {
        return RunWithErrorHandler(() =>
        {
            SessionStore sessionStore = sessionObjects.SessionManager
                .GetSession(input.SessionFormIdentifier);
            if (!sessionStore.RuleEngine.IsExportAllowed(
                    sessionStore.GetEntityId(input.Entity)))
            {
                return StatusCode(403, 
                    localizer["ExcelExportForbidden"].ToString());
            }
            bool isLazyLoaded 
                = sessionStore.IsLazyLoadedEntity(input.Entity);
            switch (isLazyLoaded)
            {
                case true when input.LazyLoadedEntityInput == null:
                {
                    return BadRequest(
                        $"Export from lazy loaded entities requires {nameof(input.LazyLoadedEntityInput)}");
                }
                case false when input.RowIds == null 
                                || input.RowIds.Count == 0:
                {
                    return BadRequest(
                        $"Export from non lazy loaded entities requires {nameof(input.RowIds)}");
                }
                default:
                {
                    var entityExportInfo = new EntityExportInfo
                    {
                        Entity = input.Entity,
                        Fields = input.Fields,
                        RowIds = input.RowIds,
                        SessionFormIdentifier 
                            = input.SessionFormIdentifier.ToString(),
                        Store = sessionStore,
                        LazyLoadedEntityInput = input.LazyLoadedEntityInput
                    };
                    return GetExcelFile(entityExportInfo);
                }
            }
        });
    }
        
    private IActionResult GetExcelFile(EntityExportInfo entityExportInfo)
    {
        return RunWithErrorHandler(() =>
        {
            var excelEntityExporter = new ExcelEntityExporter();
            Result<IWorkbook, IActionResult> workbookResult 
                = FillWorkbook(entityExportInfo, excelEntityExporter);
            if (workbookResult.IsFailure)
            {
                return workbookResult.Error;
            }
            if (excelEntityExporter.ExportFormat == ExcelFormat.XLS)
            {
                Response.ContentType = "application/vnd.ms-excel";
                Response.Headers.Append(
                    "content-disposition", "attachment; filename=export.xls");
            }
            else
            {
                Response.ContentType
                    = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Append(
                    "content-disposition", "attachment; filename=export.xlsx");
            }
#pragma warning disable 1998 
            return new FileCallbackResult(new MediaTypeHeaderValue(Response.ContentType),
                async (outputStream, _) =>
                {
                    workbookResult.Value.Write(outputStream);
                });
#pragma warning restore 1998
        });
    }

    private class ReadResult {
        public DataStructureQuery DataStructureQuery { get; set; }
        public IEnumerable<IEnumerable<object>> Rows { get; set; }
    }

    private Result<ReadResult, IActionResult> ReadRows(
        EntityExportInfo entityExportInfo, 
        DataStructureQuery dataStructureQuery) {
        var result = ExecuteDataReader(
            dataStructureQuery: dataStructureQuery,
            methodId: entityExportInfo.LazyLoadedEntityInput.MenuId);
        var readerResult = new ReadResult 
        {
            DataStructureQuery = dataStructureQuery,
            Rows = result.Value as IEnumerable<IEnumerable<object>>
        };
        return result.IsSuccess
            ? Result.Success<ReadResult, IActionResult>(readerResult)
            : Result.Failure<ReadResult, IActionResult>(result.Error);
    }

    private Result<IWorkbook, IActionResult> FillWorkbook(
        EntityExportInfo entityExportInfo,
        ExcelEntityExporter excelEntityExporter)
    {
        if (entityExportInfo.Store 
            is WorkQueueSessionStore workQueueSessionStore)
        {

            return WorkQueueGetRowsGetRowsQuery(
                    entityExportInfo.LazyLoadedEntityInput, 
                    workQueueSessionStore)
                .Bind(dataStructureQuery => ReadRows(entityExportInfo, dataStructureQuery))
                .Map(readResult => excelEntityExporter.FillWorkBook(
                    entityExportInfo, 
                    readResult.DataStructureQuery
                        .GetAllQueryColumns()
                        .Select(x =>x.Name)
                        .ToList(),
                    readResult.Rows));               
        }
        bool isLazyLoaded 
            = entityExportInfo.Store.IsLazyLoadedEntity(entityExportInfo.Entity);
        if (isLazyLoaded)
        {
            ILazyRowLoadInput input = entityExportInfo.LazyLoadedEntityInput;
            return EntityIdentificationToEntityData(input)
                .Bind(entityData => GetRowsGetQuery(input, entityData))
                .Bind(dataStructureQuery => ReadRows(
                    entityExportInfo, dataStructureQuery))
                .Map(readResult => excelEntityExporter.FillWorkBook(
                    entityExportInfo,
                    readResult.DataStructureQuery
                        .GetAllQueryColumns()
                        .Select(x => x.Name)
                        .ToList(),
                    readResult.Rows));
        }
        IWorkbook workBook = excelEntityExporter.FillWorkBook(entityExportInfo);
        return Result.Success<IWorkbook, IActionResult>(workBook) ;
    }
}