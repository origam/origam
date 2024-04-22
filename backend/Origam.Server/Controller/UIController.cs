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
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Server.Controller;

[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
public class UIController: AbstractController
{
    private readonly IPersistenceService persistenceService;

    public UIController(ILogger<UIController> log, SessionObjects sessionObjects) 
        : base(log, sessionObjects)
    {
            persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        }

    [HttpGet("[action]")]
    public IActionResult GetMenu()
    {
            var claimsPrincipal = User;
            return Ok(MenuXmlBuilder.GetMenu());
        }

    [HttpGet("[action]")]
    public IActionResult GetUI([FromQuery] [Required] Guid id)
    {
            FormReferenceMenuItem menuItem = persistenceService.SchemaProvider.RetrieveInstance(
                typeof(FormReferenceMenuItem), new ModelElementKey(id)) as FormReferenceMenuItem;

            if (menuItem == null) return BadRequest("Menu with that Id does not exist");
            
            XmlOutput xmlOutput = FormXmlBuilder.GetXml(id);
            MenuLookupIndex.AddIfNotPresent(id, xmlOutput.ContainedLookups);
            return Ok(xmlOutput.Document.OuterXml);
        }
}