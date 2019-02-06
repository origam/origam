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
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Models;

namespace Origam.ServerCoreTests
{
    abstract class ControllerTests
    {
        protected readonly SessionObjects sessionObjects;
        protected readonly ControllerContext context;
        private readonly DataController dataController;

        protected ControllerTests()
        {
            sessionObjects = new SessionObjects();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "JohnDoe"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "JohnDoe"),
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

            dataController = new DataController(new NullLogger<AbstractController>());
            dataController.ControllerContext = context;
        }

        protected void AssertObjectIsPersisted(Guid menuId, Guid dataStructureId, Guid id,
            string[] columnNames, object[] expectedColumnValues)
        {
            if (expectedColumnValues.Length != columnNames.Length)
            {
                throw new Exception("columnValues must have the same length as columnNames");
            }

            IActionResult entitiesActionResult = dataController.EntitiesGet(new EntityGetData
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
}