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

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Security.Principal;
using System.Threading;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Origam.DA;
using Origam.Schema;
using Origam.Server.Common;
using Origam.Server.Extensions;
using Origam.Server.Model.Blob;
using Origam.Server.Model.UIService;
using Origam.Workbench.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Origam.Server.Controller;

[Authorize(Policy = "InternalApi")]
[Controller]
[Route(template: "internalApi/[controller]")]
public class BlobController : AbstractController
{
    private readonly IStringLocalizer<SharedResources> localizer;
    private readonly CoreHttpTools httpTools = new();

    public BlobController(
        SessionObjects sessionObjects,
        IStringLocalizer<SharedResources> localizer,
        ILogger<BlobController> log,
        IWebHostEnvironment environment
    )
        : base(log: log, sessionObjects: sessionObjects, environment: environment)
    {
        this.localizer = localizer;
    }

    [HttpPost(template: "[action]")]
    public IActionResult DownloadToken([FromBody] [Required] BlobDownloadTokenInput input)
    {
        return AmbiguousInputToRowData(input: input, dataService: dataService)
            .Map(func: rowData => CreateDownloadToken(input: input, rowData: rowData))
            .Finally(func: UnwrapReturnValue);
    }

    [HttpPost(template: "[action]")]
    public IActionResult UploadToken([FromBody] [Required] BlobUploadTokenInput input)
    {
        return AmbiguousInputToRowData(input: input, dataService: dataService)
            .Map(func: rowData => CreateUploadToken(input: input, rowData: rowData))
            .Finally(func: UnwrapReturnValue);
    }

    [AllowAnonymous]
    [HttpGet(template: "{token:guid}")]
    public IActionResult Get(Guid token)
    {
        try
        {
            var blobDownloadRequest = sessionObjects.SessionManager.GetBlobDownloadRequest(
                key: token
            );
            if (blobDownloadRequest == null)
            {
                return NotFound(value: localizer[name: "ErrorBlobNotAvailable"].ToString());
            }
            if (
                string.IsNullOrEmpty(value: blobDownloadRequest.BlobMember)
                && blobDownloadRequest.BlobLookupId == Guid.Empty
            )
            {
                return BadRequest(
                    error: localizer[name: "ErrorBlobMemberBlobLookupNotSpecified"].ToString()
                );
            }
            Stream resultStream;
            MemoryStream memoryStream;
            var blobMemberAvailable =
                !string.IsNullOrEmpty(value: blobDownloadRequest.BlobMember)
                && (
                    blobDownloadRequest.Row[columnName: blobDownloadRequest.BlobMember]
                    != DBNull.Value
                );
            if ((blobDownloadRequest.BlobLookupId != Guid.Empty) && !blobMemberAvailable)
            {
                var lookupService = ServiceManager.Services.GetService<IDataLookupService>();
                var result = lookupService.GetDisplayText(
                    lookupId: blobDownloadRequest.BlobLookupId,
                    lookupValue: DatasetTools.PrimaryKey(row: blobDownloadRequest.Row)[0],
                    useCache: false,
                    returnMessageIfNull: false,
                    transactionId: null
                );
                byte[] bytes;
                switch (result)
                {
                    case null:
                    {
                        return BadRequest(error: localizer[name: "ErrorBlobNoData"].ToString());
                    }
                    case byte[] arrayOfBytes:
                    {
                        bytes = arrayOfBytes;
                        break;
                    }
                    default:
                    {
                        return BadRequest(error: localizer[name: "ErrorBlobNotBlob"].ToString());
                    }
                }
                memoryStream = new MemoryStream(buffer: bytes);
            }
            else
            {
                if (
                    blobDownloadRequest.Row[columnName: blobDownloadRequest.BlobMember!]
                    == DBNull.Value
                )
                {
                    return BadRequest(error: localizer[name: "ErrorBlobRecordEmpty"].ToString());
                }
                memoryStream = new MemoryStream(
                    buffer: (byte[])
                        blobDownloadRequest.Row[columnName: blobDownloadRequest.BlobMember]
                );
            }
            if (blobDownloadRequest.IsCompressed)
            {
                resultStream = new GZipStream(
                    stream: memoryStream,
                    mode: CompressionMode.Decompress
                );
            }
            else
            {
                resultStream = memoryStream;
            }
            var filename = (string)
                blobDownloadRequest.Row[columnName: blobDownloadRequest.Property];
            var disposition = httpTools.GetFileDisposition(
                userAgent: Request.GetUserAgent(),
                fileName: filename
            );
            if (!blobDownloadRequest.IsPreview)
            {
                disposition = "attachment; " + disposition;
            }
            Response.Headers.Append(key: HeaderNames.ContentDisposition, value: disposition);
            return File(
                fileStream: resultStream,
                contentType: HttpTools.Instance.GetMimeType(fileName: filename)
            );
        }
        catch (Exception ex)
        {
            return StatusCode(statusCode: 500, value: ex);
        }
        finally
        {
            sessionObjects.SessionManager.RemoveBlobDownloadRequest(key: token);
        }
    }

    [AllowAnonymous]
    [HttpPost(template: "{token:guid}/{filename}")]
    public IActionResult Post(Guid token, string filename)
    {
        try
        {
            var blobUploadRequest = sessionObjects.SessionManager.GetBlobUploadRequest(key: token);
            if (blobUploadRequest == null)
            {
                return NotFound(value: localizer[name: "ErrorBlobFileNotAvailable"].ToString());
            }
            //todo: review user management
            Thread.CurrentPrincipal = new GenericPrincipal(
                identity: new GenericIdentity(name: blobUploadRequest.UserName),
                roles: new string[] { }
            );
            var profile = SecurityTools.CurrentUserProfile();
            if (CheckMember(val: blobUploadRequest.OriginalPathMember, throwExceptions: false))
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.OriginalPathMember] = filename;
            }
            if (CheckMember(val: blobUploadRequest.DateCreatedMember, throwExceptions: false))
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.DateCreatedMember] =
                    blobUploadRequest.DateCreated;
            }
            if (CheckMember(val: blobUploadRequest.DateLastModifiedMember, throwExceptions: false))
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.DateLastModifiedMember] =
                    blobUploadRequest.DateLastModified;
            }
            if (CheckMember(val: blobUploadRequest.CompressionStateMember, throwExceptions: false))
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.CompressionStateMember] =
                    blobUploadRequest.ShouldCompress;
            }
            var input = StreamTools.ReadToEnd(input: Request.Body);
            if (blobUploadRequest.ShouldCompress)
            {
                var gZipStream = new GZipStream(
                    stream: new MemoryStream(buffer: input),
                    mode: CompressionMode.Compress
                );
                blobUploadRequest.Row[columnName: blobUploadRequest.BlobMember] =
                    StreamTools.ReadToEnd(input: gZipStream);
            }
            else
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.BlobMember] = input;
            }
            if (CheckMember(val: blobUploadRequest.FileSizeMember, throwExceptions: false))
            {
                blobUploadRequest.Row[columnName: blobUploadRequest.FileSizeMember] =
                    input.LongLength;
            }
            if (CheckMember(val: blobUploadRequest.ThumbnailMember, throwExceptions: false))
            {
                try
                {
                    // The image is loaded to check that the input actually
                    // represents an image
                    using var image = Image.Load(buffer: input);
                    var parameterService = ServiceManager.Services.GetService<IParameterService>();
                    var width = (int)
                        parameterService.GetParameterValue(
                            id: blobUploadRequest.ThumbnailWidthConstantId,
                            targetType: OrigamDataType.Integer
                        );
                    var height = (int)
                        parameterService.GetParameterValue(
                            id: blobUploadRequest.ThumbnailHeightConstantId,
                            targetType: OrigamDataType.Integer
                        );
                    var row = blobUploadRequest.Row;
                    var thumbnailMember = blobUploadRequest.ThumbnailMember;
                    row[columnName: thumbnailMember] = ResizeImage(
                        byteArrayImage: input,
                        width: width,
                        height: height
                    );
                }
                catch
                {
                    blobUploadRequest.Row[columnName: blobUploadRequest.ThumbnailMember] =
                        DBNull.Value;
                }
            }
            DatasetTools.UpdateOrigamSystemColumns(
                row: blobUploadRequest.Row,
                isNew: false,
                profileId: profile.Id
            );
            blobUploadRequest.Row[columnName: blobUploadRequest.Property] = filename;
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(statusCode: 500, value: ex);
        }
        finally
        {
            sessionObjects.SessionManager.RemoveBlobUploadRequest(key: token);
        }
    }

    private IActionResult CreateDownloadToken(BlobDownloadTokenInput input, RowData rowData)
    {
        var token = Guid.NewGuid();
        sessionObjects.SessionManager.AddBlobDownloadRequest(
            key: token,
            request: new BlobDownloadRequest(
                row: rowData.Row,
                parameters: input.Parameters,
                property: input.Property,
                isPreview: input.IsPreview
            )
        );
        return Ok(value: token);
    }

    private IActionResult CreateUploadToken(BlobUploadTokenInput input, RowData rowData)
    {
        var token = Guid.NewGuid();
        sessionObjects.SessionManager.AddBlobUploadRequest(
            key: token,
            request: new BlobUploadRequest(
                row: rowData.Row,
                principal: SecurityManager.CurrentPrincipal,
                parameters: input.Parameters,
                dateCreated: input.DateCreated,
                dateLastModified: input.DateLastModified,
                property: input.Property,
                entity: rowData.Entity
            )
        );
        return Ok(value: token);
    }

    private static bool CheckMember(object val, bool throwExceptions)
    {
        if ((val != null) && !val.Equals(obj: string.Empty) && !val.Equals(obj: Guid.Empty))
        {
            return true;
        }
        if (throwExceptions)
        {
            throw new NullReferenceException(message: "Member not set.");
        }
        return false;
    }

    private static byte[] ResizeImage(byte[] byteArrayImage, int width, int height)
    {
        IImageFormat format = Image.DetectFormat(buffer: byteArrayImage);
        if (format == null)
        {
            throw new InvalidOperationException(message: "Unable to detect image format.");
        }
        using Image image = Image.Load(buffer: byteArrayImage);
        var sourceWidth = image.Width;
        var sourceHeight = image.Height;
        var destX = 0;
        var destY = 0;
        float percent;
        var percentWidth = width / (float)sourceWidth;
        var percentHeight = height / (float)sourceHeight;
        if (percentHeight < percentWidth)
        {
            percent = percentHeight;
            destX = Convert.ToInt16(value: (width - (sourceWidth * percent)) / 2);
        }
        else
        {
            percent = percentWidth;
            destY = Convert.ToInt16(value: (height - (sourceHeight * percent)) / 2);
        }
        var destWidth = (int)(sourceWidth * percent);
        var destHeight = (int)(sourceHeight * percent);
        using var backgroundImage = new Image<Rgba32>(
            width: width,
            height: height,
            backgroundColor: Color.Black
        );
        image.Mutate(operation: x => x.Resize(width: destWidth, height: destHeight));
        backgroundImage.Mutate(operation: x =>
            x.DrawImage(
                foreground: image,
                backgroundLocation: new Point(x: destX, y: destY),
                opacity: 1f
            )
        );
        using var memoryStream = new MemoryStream();
        backgroundImage.Save(stream: memoryStream, format: format);
        return memoryStream.ToArray();
    }
}
