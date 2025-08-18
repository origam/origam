﻿#region license

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
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Server.Model.About;

namespace Origam.Server.Controller;
[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
[ApiController]
[Route("internalApi/[controller]")]
public class AboutController : AbstractController
{
    public AboutController(ILogger<AbstractController> log,
        SessionObjects sessionObjects, IWebHostEnvironment environment)
        : base(log, sessionObjects, environment)
    {
    }
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new AboutInfo
        {
            ServerVersion = "ServerVersion Placeholder to be changed at build time",
            LinkToCommit = "LinkToCommit Placeholder to be changed at build time",
            CommitId = "CommitId Placeholder to be changed at build time"
        });
    }
}
