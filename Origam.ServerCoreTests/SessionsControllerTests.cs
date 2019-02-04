using System;
using NUnit.Framework;
using Origam.ServerCore;
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Models;

namespace Tests
{
    public class SessionsControllerTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ShouldCreateNewSession()
        {
            var sessionObjects = new SessionObjects();
            var sessionsController = new SessionsController(sessionObjects);
            sessionsController.New(new NewSessionData
            {
                MenuId = new Guid("8713c618-b4fb-4749-ab28-0811b85481b0"),
                InitializeStructure=true
            });
        }
    }
}