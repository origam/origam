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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.ServerCore;
using Origam.ServerCore.Controller;
using Origam.ServerCore.Model.UIService;

namespace Origam.ServerCoreTests;
abstract class ControllerTests
{
    protected readonly SessionObjects sessionObjects;
    protected readonly ControllerContext context;
    private readonly UIServiceController uiServiceController;
    protected ControllerTests()
    {
        var configuration = ConfigHelper
            .GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
        sessionObjects = new SessionObjects();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, configuration.UserName),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("name", configuration.UserName),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        context = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        Thread.CurrentPrincipal = principal;
        uiServiceController = new UIServiceController(
            sessionObjects,
            null,
            new NullLogger<AbstractController>());
        uiServiceController.ControllerContext = context;
    }
    protected void AssertObjectIsPersisted(Guid menuId, Guid dataStructureId, Guid id,
        string[] columnNames, object[] expectedColumnValues)
    {
        if (expectedColumnValues.Length != columnNames.Length)
        {
            throw new Exception("columnValues must have the same length as columnNames");
        }
        IActionResult entitiesActionResult = uiServiceController.GetRows(new GetRowsInput
        {
            MenuId = menuId,
            DataStructureEntityId = dataStructureId,
            Filter = $"[\"Id\",\"eq\",\"{id}\"]",
            Ordering = new List<List<string>>(),
            RowLimit = 0,
            ColumnNames = columnNames
        });
        Assert.IsInstanceOf<OkObjectResult>(entitiesActionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult)entitiesActionResult;
        Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
        var retrievedObjects = ((IEnumerable<object>)entitiesObjResult.Value).ToList();
        Assert.That(retrievedObjects, Has.Count.EqualTo(1));
        object[] retrievedObject = (object[])retrievedObjects[0];
        for (var i = 0; i < columnNames.Length; i++)
        {
            Assert.That(retrievedObject[i], Is.EqualTo(expectedColumnValues[i]));
        }
    }
}
