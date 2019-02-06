using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.Extensions;
using Origam.ServerCore.Controllers;
using Origam.ServerCore.Models;

namespace Origam.ServerCoreTests
{
    class DataControllerTests: ControllerTests
    {
        private readonly DataController sut;
        private Guid producerMenuId = new Guid("f38fdadb-4bba-4bb7-8184-c9109d5d40cd");
        private Guid produceDataStructureEntityId = new Guid("93053357-745b-4a8c-91d2-d60389d0f22e");
        private List<Guid> createdProducers = new List<Guid>();
 
        public DataControllerTests()
        {
            sut = new DataController(NullLogger<AbstractController>.Instance);
            sut.ControllerContext = context;
        }

        [Test, Order(101)]
        public void ShouldRetrieveProducer()
        {
            Guid producerId = new Guid("B61F9A1F-3719-46D3-BE0A-2713C1E5F4CA");

            string[] columnNames = new[] { "Id", "Name", "NameAndAddress", "Email" };
            IActionResult entitiesActionResult = sut.EntitiesGet(new EntityGetData
            {
                MenuId = producerMenuId,
                DataStructureEntityId = produceDataStructureEntityId,
                Filter = $"[\"Id\",\"eq\",\"{producerId}\"]",
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
            Assert.That(retrievedObject[0], Is.EqualTo(producerId)); 
            Assert.That(retrievedObject[1], Is.EqualTo("hu"));
            Assert.That(retrievedObject[2], Is.EqualTo("huhu"));
            Assert.That(retrievedObject[3], Is.EqualTo("hu@hu.hu"));

            // This checks that the protected dataController in class ControllerTests works
            // correctly. The dataController is used by method AssertObjectIsPersisted which
            // is used in other tests to verify their results and that is why this should be
            // the first test to run.
            AssertObjectIsPersisted(
                menuId: producerMenuId,
                dataStructureId: produceDataStructureEntityId,
                id: producerId,
                columnNames: columnNames,
                expectedColumnValues: new object[] { producerId, "hu", "huhu", "hu@hu.hu" });
        }

        [Test, Order(102)]
        public void ShouldCreateNewProducer()
        {
            var actionResult = sut.Entities(new EntityInsertData
            {
                MenuId = producerMenuId,
                DataStructureEntityId = produceDataStructureEntityId,
                NewValues = new Dictionary<string, string>
                {
                    { "Name", "testName1" },
                    { "NameAndAddress", "testNameAndAddress1" } ,
                    { "Email", "test.test@test.test1" } 
                }
            });

            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult entitiesObjResult = (OkObjectResult)actionResult;

            Assert.IsInstanceOf<DataRow>(entitiesObjResult.Value);
            Guid newProducerId = (Guid)((DataRow) entitiesObjResult.Value)["Id"];

            AssertObjectIsPersisted(
                menuId: producerMenuId,
                dataStructureId: produceDataStructureEntityId,
                id: newProducerId,
                columnNames: new[] {"Name", "NameAndAddress", "Email" },
                expectedColumnValues: new object[] { "testName1", "testNameAndAddress1", "test.test@test.test1" });

            createdProducers.Add(newProducerId);
        }

        [Test, Order(103)]
        public void ShouldUpdateExistingProducer()
        {
            Guid producerId = createdProducers[0];

            var actionResult = sut.Entities(new EntityUpdateData
            {
                MenuId = producerMenuId,
                DataStructureEntityId = produceDataStructureEntityId,
                NewValues = new Dictionary<string, string>
                {
                    { "Name", "testNameUpdated" },
                    { "NameAndAddress", "testNameAndAddressUpdated" } ,
                    { "Email", "test.testUpdated@test.test1" }
                },
                RowId = producerId
            });

            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            OkObjectResult entitiesObjResult = (OkObjectResult)actionResult;

            AssertObjectIsPersisted(
                menuId: producerMenuId,
                dataStructureId: produceDataStructureEntityId,
                id: producerId,
                columnNames: new[] { "Name", "NameAndAddress", "Email" },
                expectedColumnValues: new object[] { "testNameUpdated", "testNameAndAddressUpdated", "test.testUpdated@test.test1" });
        }

        [Test, Order(104)]
        public void ShouldDeleteProducer()
        {
            Guid producerId = createdProducers[0];
            sut.Entities(new EntityDeleteData
            {
                MenuId = producerMenuId,
                DataStructureEntityId = produceDataStructureEntityId,
                RowIdToDelete = producerId
            });

            IActionResult entitiesActionResult = sut.EntitiesGet(new EntityGetData
            {
                MenuId = producerMenuId,
                DataStructureEntityId = produceDataStructureEntityId,
                Filter = $"[\"Id\",\"eq\",\"{producerId}\"]",
                Ordering = new List<List<string>>(),
                RowLimit = 0,
                ColumnNames = new[] { "Name", "NameAndAddress", "Email" }
            });
            Assert.IsInstanceOf<OkObjectResult>(entitiesActionResult);
            OkObjectResult entitiesObjResult = (OkObjectResult)entitiesActionResult;

            Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
            var retrievedObjects = ((IEnumerable<object>)entitiesObjResult.Value).ToList();
            Assert.That(retrievedObjects, Has.Count.EqualTo(0));
        }
    }
}
