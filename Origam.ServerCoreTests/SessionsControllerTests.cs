using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.Server;
using Origam.ServerCore;
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Models;

namespace Tests
{
    public class SessionsControllerTests
    {
        private SessionObjects sessionObjects;
        private SessionsController sut;
        private Guid sessionId;
        private Guid rowId;
        private DataController dataController;

        public SessionsControllerTests()
        {
            sessionObjects = new SessionObjects();
            sut = new SessionsController(sessionObjects);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "John Doe"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
            dataController = new DataController(new NullLogger<AbstractController>());
            dataController.ControllerContext = context;
        }

        [Test, Order(1)]
        public void ShouldCreateNewEmptySession()
        {
            var actionResult = sut.New(new NewSessionData
            {
                MenuId = new Guid("f38fdadb-4bba-4bb7-8184-c9109d5d40cd"),
                InitializeStructure = true
            });

            Assert.IsInstanceOf<OkObjectResult>(actionResult); // OkObjectResult means code 200 will be returned
            OkObjectResult okObjectResult = (OkObjectResult) actionResult;

            Assert.IsInstanceOf<Guid>(okObjectResult.Value);
            Guid newSessionId = (Guid) okObjectResult.Value;

            Assert.That(sessionObjects.SessionManager.HasFormSession(newSessionId));
            sessionId = newSessionId;
        }


        [Test, Order(2)]
        public void ShouldCreateNewRowInProducers()
        {
            var actionResult = sut.CreateRow(new NewRowData
            {
                SessionFormIdentifier = sessionId,
                Entity = "Producer",
                Values = new Dictionary<string, object>(),
                Parameters = new Dictionary<string, object>(),
                RequestingGridId = new Guid("36aa15e1-5fb8-4ca6-8804-e83ac09945d1")
            });
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult okObjectResult = (OkObjectResult) actionResult;

            Assert.IsInstanceOf<ArrayList>(okObjectResult.Value);
            var values = (ArrayList) okObjectResult.Value;
            Assert.That(values, Has.Count.EqualTo(1));

            Assert.IsInstanceOf<ChangeInfo>(values[0]);
            var changeInfo = (ChangeInfo) values[0];

            Assert.IsInstanceOf<Guid>(changeInfo.ObjectId);
            var newRowId = (Guid) changeInfo.ObjectId;
            Assert.That(newRowId, Is.Not.EqualTo(Guid.Empty));
            rowId = newRowId;
        }


        [Test, Order(3)]
        public void ShouldModifyTheNewRow()
        {
            sut.UpdateRow(new UpdateRowData
            {
                SessionFormIdentifier = sessionId,
                Entity = "Producer",
                Id = rowId,
                Property = "Name",
                NewValue = "test"
            });
            sut.UpdateRow(new UpdateRowData
            {
                SessionFormIdentifier = sessionId,
                Entity = "Producer",
                Id = rowId,
                Property = "NameAndAddress",
                NewValue = "test"
            });
            var actionResult = sut.UpdateRow(new UpdateRowData
            {
                SessionFormIdentifier = sessionId,
                Entity = "Producer",
                Id = rowId,
                Property = "Email",
                NewValue = "test@test.test"
            });

            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult okObjectResult = (OkObjectResult) actionResult;

            Assert.IsInstanceOf<ArrayList>(okObjectResult.Value);
            var values = (ArrayList) okObjectResult.Value;
            Assert.That(values, Has.Count.EqualTo(1));

            Assert.IsInstanceOf<ChangeInfo>(values[0]);
            var changeInfo = (ChangeInfo) values[0];

            Assert.IsInstanceOf<ArrayList>(changeInfo.WrappedObject);
            var currentRowValues = (ArrayList) changeInfo.WrappedObject;

            Assert.That(currentRowValues[3], Is.EqualTo("test"));
            Assert.That(currentRowValues[4], Is.EqualTo("test@test.test"));
            Assert.That(currentRowValues[5], Is.EqualTo("test"));
        }

        [Test, Order(4)]
        public void ShouldSaveSession()
        {
            var actionResult = sut.Save(new SaveSessionData
            {
                SessionId = sessionId
            });
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult okObjectResult = (OkObjectResult)actionResult;

            //okObjectResult.Value


            var session = (FormSessionStore)sessionObjects.SessionManager.GetSession(sessionId);

            //session.
            dataController.EntitiesGet(new EntityGetData
            {
                MenuId = session.MenuItem.Id,
                DataStructureEntityId = new Guid("93053357-745b-4a8c-91d2-d60389d0f22e"),
                Filter = "",
                Ordering = new List<List<string>>(),
                RowLimit = 0,
            });

        }
    }
}