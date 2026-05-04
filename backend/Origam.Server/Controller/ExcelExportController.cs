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

[Authorize(Policy = "InternalApi")]
[ApiController]
[Route(template: "internalApi/[controller]")]
public class ExcelExportController : AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;

    public ExcelExportController(
        ILogger<ExcelExportController> log,
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment)
    {
        this.localizer = localizer;
    }

    [HttpPost(template: "[action]")]
    public IActionResult GetFile([FromBody] [Required] ExcelExportInput input)
    {
        SessionStore sessionStore = sessionObjects.SessionManager.GetSession(
            sessionFormIdentifier: input.SessionFormIdentifier
        );
        if (
            !sessionStore.RuleEngine.IsExportAllowed(
                entityId: sessionStore.GetEntityId(entity: input.Entity)
            )
        )
        {
            return StatusCode(
                statusCode: 403,
                value: localizer[name: "ExcelExportForbidden"].ToString()
            );
        }
        bool isLazyLoaded = sessionStore.IsLazyLoadedEntity(entity: input.Entity);
        switch (isLazyLoaded)
        {
            case true when input.LazyLoadedEntityInput == null:
            {
                return BadRequest(
                    error: $"Export from lazy loaded entities requires {nameof(input.LazyLoadedEntityInput)}"
                );
            }
            case false when input.RowIds == null || input.RowIds.Count == 0:
            {
                return BadRequest(
                    error: $"Export from non lazy loaded entities requires {nameof(input.RowIds)}"
                );
            }
            default:
            {
                var entityExportInfo = new EntityExportInfo
                {
                    Entity = input.Entity,
                    Fields = input.Fields,
                    RowIds = input.RowIds,
                    SessionFormIdentifier = input.SessionFormIdentifier.ToString(),
                    Store = sessionStore,
                    LazyLoadedEntityInput = input.LazyLoadedEntityInput,
                };
                return GetExcelFile(entityExportInfo: entityExportInfo);
            }
        }
    }

    private IActionResult GetExcelFile(EntityExportInfo entityExportInfo)
    {
        var excelEntityExporter = new ExcelEntityExporter();
        Result<IWorkbook, IActionResult> workbookResult = FillWorkbook(
            entityExportInfo: entityExportInfo,
            excelEntityExporter: excelEntityExporter
        );
        if (workbookResult.IsFailure)
        {
            return workbookResult.Error;
        }
        if (excelEntityExporter.ExportFormat == ExcelFormat.XLS)
        {
            Response.ContentType = "application/vnd.ms-excel";
            Response.Headers.Append(
                key: "content-disposition",
                value: "attachment; filename=export.xls"
            );
        }
        else
        {
            Response.ContentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Append(
                key: "content-disposition",
                value: "attachment; filename=export.xlsx"
            );
        }
#pragma warning disable 1998
        return new FileCallbackResult(
            contentType: new MediaTypeHeaderValue(mediaType: Response.ContentType),
            callback: async (outputStream, _) =>
            {
                workbookResult.Value.Write(stream: outputStream);
            }
        );
#pragma warning restore 1998
    }

    private class ReadResult
    {
        public DataStructureQuery DataStructureQuery { get; set; }
        public IEnumerable<IEnumerable<object>> Rows { get; set; }
    }

    private Result<ReadResult, IActionResult> ReadRows(
        EntityExportInfo entityExportInfo,
        DataStructureQuery dataStructureQuery
    )
    {
        var result = ExecuteDataReader(
            dataStructureQuery: dataStructureQuery,
            methodId: entityExportInfo.LazyLoadedEntityInput.MenuId
        );
        var readerResult = new ReadResult
        {
            DataStructureQuery = dataStructureQuery,
            Rows = result.Value as IEnumerable<IEnumerable<object>>,
        };
        return result.IsSuccess
            ? Result.Success<ReadResult, IActionResult>(value: readerResult)
            : Result.Failure<ReadResult, IActionResult>(error: result.Error);
    }

    private Result<IWorkbook, IActionResult> FillWorkbook(
        EntityExportInfo entityExportInfo,
        ExcelEntityExporter excelEntityExporter
    )
    {
        if (entityExportInfo.Store is WorkQueueSessionStore workQueueSessionStore)
        {
            return WorkQueueGetRowsGetRowsQuery(
                    input: entityExportInfo.LazyLoadedEntityInput,
                    sessionStore: workQueueSessionStore
                )
                .Bind(func: dataStructureQuery =>
                    ReadRows(
                        entityExportInfo: entityExportInfo,
                        dataStructureQuery: dataStructureQuery
                    )
                )
                .Map(func: readResult =>
                    excelEntityExporter.FillWorkBook(
                        info: entityExportInfo,
                        columns: readResult
                            .DataStructureQuery.GetAllQueryColumns()
                            .Select(selector: x => x.Name)
                            .ToList(),
                        rows: readResult.Rows
                    )
                );
        }
        bool isLazyLoaded = entityExportInfo.Store.IsLazyLoadedEntity(
            entity: entityExportInfo.Entity
        );
        if (isLazyLoaded)
        {
            ILazyRowLoadInput input = entityExportInfo.LazyLoadedEntityInput;
            return EntityIdentificationToEntityData(input: input)
                .Bind(func: entityData => GetRowsGetQuery(input: input, entityData: entityData))
                .Bind(func: dataStructureQuery =>
                    ReadRows(
                        entityExportInfo: entityExportInfo,
                        dataStructureQuery: dataStructureQuery
                    )
                )
                .Map(func: readResult =>
                    excelEntityExporter.FillWorkBook(
                        info: entityExportInfo,
                        columns: readResult
                            .DataStructureQuery.GetAllQueryColumns()
                            .Select(selector: x => x.Name)
                            .ToList(),
                        rows: readResult.Rows
                    )
                );
        }
        IWorkbook workBook = excelEntityExporter.FillWorkBook(info: entityExportInfo);
        return Result.Success<IWorkbook, IActionResult>(value: workBook);
    }
}
