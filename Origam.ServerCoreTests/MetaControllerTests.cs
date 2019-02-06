using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.ServerCore.Controllers;

namespace Origam.ServerCoreTests
{
    [TestFixture]
    class MetaControllerTests: ControllerTests
    {
        private MetaDataController sut;
        private Guid sessionId;
        private Guid rowId;


        public MetaControllerTests()
        {
            sut = new MetaDataController(new NullLogger<MetaDataController>());
            sut.ControllerContext = context;
        }

        [Test, Order(201)]
        public void ShouldReturnMenuXml()
        {
            var actionResult = sut.GetMenu();
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult okObjectResult = (OkObjectResult)actionResult;

            Assert.IsInstanceOf<string>(okObjectResult.Value);
            string menuXml = (string)okObjectResult.Value;

            Assert.That(menuXml, Is.Not.Empty);
        }


        [Test, Order(202)]
        public void ShouldReturnScreenXml()
        {
            var actionResult = sut.GetScreeSection(new Guid("f38fdadb-4bba-4bb7-8184-c9109d5d40cd"));
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult okObjectResult = (OkObjectResult)actionResult;

            Assert.IsInstanceOf<string>(okObjectResult.Value);
            string menuXml = (string)okObjectResult.Value;

            Assert.That(menuXml, Is.Not.Empty);
        }

    }
}
