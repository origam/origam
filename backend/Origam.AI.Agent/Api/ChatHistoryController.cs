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
using Origam.AI.Agent.Chat;
using Origam.AI.Agent.Models;
using Origam.AI.Agent.Services.OpenApi;

namespace Origam.AI.Agent.Api;

[ApiController]
[Route("agent/history")]
[Tags(OpenApiSectionProvider.AgentApiSectionName)]
public sealed class ChatHistoryController(ChatHistoryService chatHistoryService) : ControllerBase
{
    [HttpGet]
    public List<ChatThreadModel> GetAll()
    {
        return chatHistoryService.LoadAll();
    }

    [HttpPost("save")]
    public IActionResult Save([Required] [FromBody] ChatThreadModel thread)
    {
        if (thread.Id == Guid.Empty)
        {
            return BadRequest();
        }
        chatHistoryService.Save(thread);
        return Ok();
    }

    [HttpPost("delete")]
    public IActionResult Delete([Required] [FromBody] ChatThreadIdentifier thread)
    {
        return chatHistoryService.Delete(thread.Id) ? Ok() : NotFound();
    }

    [HttpPost("image")]
    public IActionResult SaveImage([Required] [FromBody] ChatImageRequest image)
    {
        try
        {
            return chatHistoryService.SaveImage(image) ? Ok() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("image")]
    public IActionResult Image(Guid threadId, Guid imageId)
    {
        ChatImageContent? image = chatHistoryService.ReadImage(threadId, imageId);
        return image == null ? NotFound() : File(image.Content, image.MimeType);
    }
}
