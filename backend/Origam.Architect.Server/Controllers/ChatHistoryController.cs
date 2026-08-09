#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatHistoryController(ChatHistoryService chatHistoryService) : ControllerBase
{
    [HttpGet("GetAll")]
    public List<ChatThreadModel> GetAll()
    {
        return chatHistoryService.LoadAll();
    }

    [HttpPost("Save")]
    public IActionResult Save([Required] [FromBody] ChatThreadModel thread)
    {
        chatHistoryService.Save(thread);
        return Ok();
    }

    [HttpPost("Delete")]
    public IActionResult Delete([Required] [FromBody] ChatThreadIdentifier thread)
    {
        return chatHistoryService.Delete(thread.Id) ? Ok() : NotFound();
    }

    [HttpPost("SaveImage")]
    public IActionResult SaveImage([Required] [FromBody] ChatImageRequest image)
    {
        return chatHistoryService.SaveImage(image) ? Ok() : NotFound();
    }

    [HttpGet("Image")]
    public IActionResult Image(Guid threadId, Guid imageId)
    {
        ChatImageContent image = chatHistoryService.ReadImage(threadId, imageId);
        return image == null ? NotFound() : File(image.Content, image.MimeType);
    }
}

public record ChatThreadIdentifier(Guid Id);
