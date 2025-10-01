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
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Controller;

[Route("chatrooms/[controller]")]
[Route("internalApi/[controller]")]
[ApiController]
public class AvatarController : ControllerBase
{
    [HttpGet("{avatarId:guid}")]
    public IActionResult GetAvatarRequest(Guid avatarId)
    {
        QueryParameterCollection parameters = new QueryParameterCollection
        {
            new QueryParameter("BusinessPartner_parId", avatarId),
        };
        DataSet datasetUsersForInvite = LoadData(
            new Guid("d11d9049-8dcb-4d3f-824d-8d63d0fb0ba5"),
            new Guid("d014e645-dda1-4999-b577-d82221715583"),
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        if (datasetUsersForInvite.Tables[0].Rows.Count == 0)
        {
            return NotFound();
        }
        DataRow userRow = datasetUsersForInvite.Tables[0].Rows[0];
        byte[] imageBytes = userRow.Field<byte[]>("AvatarFile");
        if (imageBytes == null)
        {
            return Content(MakeInitialsSvg(userRow), "image/svg+xml; charset=utf-8");
        }
        return File(
            imageBytes,
            HttpTools.Instance.GetMimeType(userRow.Field<string>("AvatarFilename"))
        );
    }

    private static string MakeInitialsSvg(DataRow userRow)
    {
        string name = userRow.Field<string>("Name");
        string firstName = userRow.Field<string>("FirstName");
        string initials = "";
        if (!string.IsNullOrEmpty(firstName))
        {
            initials += firstName.First().ToString().ToUpper();
        }
        initials += name.First().ToString().ToUpper();
        string userSvg =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 25 25\">"
            + $"<text x=\"50%\" y=\"55%\" dominant-baseline=\"middle\" text-anchor=\"middle\" font-family=\"monospace\" fill=\"black\">{initials}</text>"
            + "</svg>";
        return userSvg;
    }

    private DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        QueryParameterCollection parameters
    )
    {
        return DataService.Instance.LoadData(
            dataStructureId,
            methodId,
            defaultSetId,
            sortSetId,
            transactionId,
            parameters
        );
    }
}
