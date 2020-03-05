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
using System.IO;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Origam.DA;
using Origam.Schema.MenuModel;
using Origam.Server;
using Origam.ServerCore.Model.Blob;
using Origam.ServerCore.Model.UIService;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore.Controller
{
    [Controller]
    [Route("internalApi/[controller]")]
    public class BlobController : AbstractController
    {
        private readonly SessionObjects sessionObjects;
        private readonly IStringLocalizer<SharedResources> localizer;
        private readonly IDataService dataService;
        private readonly CoreHttpTools httpTools = new CoreHttpTools();
        public BlobController(
            SessionObjects sessionObjects, 
            IStringLocalizer<SharedResources> localizer,
            ILogger<AbstractController> log) : base(log)
        {
            this.sessionObjects = sessionObjects;
            this.localizer = localizer;
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
        [AllowAnonymous]
        [HttpGet("{token:guid}")]
        public IActionResult Get(Guid token)
        {
            try
            {
                var blobDownloadRequest = sessionObjects.SessionManager
                    .GetBlobDownloadRequest(token);
                if(blobDownloadRequest == null)
                {
                    return NotFound(localizer["ErrorBlobNotAvailable"]
                        .ToString());
                }
                if(string.IsNullOrEmpty(blobDownloadRequest.BlobMember)
                && blobDownloadRequest.BlobLookupId == Guid.Empty)
                {
                    return BadRequest(
                        localizer["ErrorBlobMemberBlobLookupNotSpecified"]
                            .ToString());
                }
                Stream resultStream;
                MemoryStream memoryStream;
                var processBlobField 
                    = string.IsNullOrEmpty(blobDownloadRequest.BlobMember) 
                      && (blobDownloadRequest.Row[
                              blobDownloadRequest.BlobMember] != DBNull.Value);
                if((blobDownloadRequest.BlobLookupId != Guid.Empty) 
                && !processBlobField)
                {
                    var lookupService = ServiceManager.Services
                        .GetService<IDataLookupService>();
                    var result = lookupService.GetDisplayText(
                        lookupId: blobDownloadRequest.BlobLookupId, 
                        lookupValue: DatasetTools.PrimaryKey(
                            blobDownloadRequest.Row)[0], 
                        useCache: false, 
                        returnMessageIfNull: false, 
                        transactionId: null);
                    byte[] bytes;
                    switch(result)
                    {
                        case null:
                        {
                            return BadRequest(localizer["ErrorBlobNoData"]
                                .ToString());
                        }
                        case byte[] arrayOfBytes:
                        {
                            bytes = arrayOfBytes;
                            break;
                        }
                        default:
                        {
                            return BadRequest(localizer["ErrorBlobNotBlob"]
                                .ToString());
                        }
                    }
                    memoryStream = new MemoryStream(bytes);
                }
                else
                {
                    if(blobDownloadRequest.Row[blobDownloadRequest.BlobMember] 
                    == DBNull.Value)
                    {
                        return BadRequest(localizer["ErrorBlobRecordEmpty"]
                            .ToString());
                    }
                    memoryStream = new MemoryStream((byte[])blobDownloadRequest
                        .Row[blobDownloadRequest.BlobMember]);
                }
                if(blobDownloadRequest.IsCompressed)
                {
                    resultStream = new GZipStream(memoryStream,
                        CompressionMode.Decompress);
                }
                else
                {
                    resultStream = memoryStream;
                }
                var filename = (string)blobDownloadRequest
                    .Row[blobDownloadRequest.Property];
                var disposition = httpTools.GetFileDisposition(
                    new CoreRequestWrapper(Request), filename);
                if(!blobDownloadRequest.IsPreview)
                {
                    disposition = "attachment; " + disposition;
                }
                Response.Headers.Add(
                    HeaderNames.ContentDisposition, disposition);
                return File(resultStream, HttpTools.GetMimeType(filename));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
            finally
            {
                sessionObjects.SessionManager.RemoveBlobDownloadRequest(token);
            }
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