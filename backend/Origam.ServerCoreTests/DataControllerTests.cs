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
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Origam.Extensions;
using Origam.ServerCore.Controller;
using Origam.ServerCore.Model.UIService;

namespace Origam.ServerCoreTests;
class DataControllerTests : ControllerTests
{
    private readonly UIServiceController uiServiceController;
    private Guid producerMenuId = new Guid("f38fdadb-4bba-4bb7-8184-c9109d5d40cd");
    private Guid produceDataStructureEntityId = new Guid("93053357-745b-4a8c-91d2-d60389d0f22e");
    private List<Guid> createdProducers = new List<Guid>();
    public DataControllerTests()
    {
        uiServiceController = new UIServiceController(
            sessionObjects,
            null,
            NullLogger<AbstractController>.Instance);
        uiServiceController.ControllerContext = context;
    }
    [Test, Order(101)]
    public void ShouldRetrieveProducer()
    {
        Guid producerId = new Guid("B61F9A1F-3719-46D3-BE0A-2713C1E5F4CA");
        string[] columnNames = new[] {"Id", "Name", "NameAndAddress", "Email"};
        IActionResult entitiesActionResult = uiServiceController.GetRows(new GetRowsInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            Filter = $"[\"Id\",\"eq\",\"{producerId}\"]",
            Ordering = new List<List<string>>(),
            RowLimit = 0,
            ColumnNames = columnNames
        });
        Assert.IsInstanceOf<OkObjectResult>(entitiesActionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult) entitiesActionResult;
        Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
        var retrievedObjects = ((IEnumerable<object>) entitiesObjResult.Value).ToList();
        Assert.That(retrievedObjects, Has.Count.EqualTo(1));
        object[] retrievedObject = (object[]) retrievedObjects[0];
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
            expectedColumnValues: new object[] {producerId, "hu", "huhu", "hu@hu.hu"});
    }
    [Test, Order(102)]
    public void ShouldCreateNewProducer()
    {
        var actionResult = uiServiceController.Row(new NewRowInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            NewValues = new Dictionary<string, string>
            {
                {"Name", "testName1"},
                {"NameAndAddress", "testNameAndAddress1"},
                {"Email", "test.test@test.test1"}
            }
        });
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult) actionResult;
        Assert.IsInstanceOf<DataRow>(entitiesObjResult.Value);
        Guid newProducerId = (Guid) ((DataRow) entitiesObjResult.Value)["Id"];
        AssertObjectIsPersisted(
            menuId: producerMenuId,
            dataStructureId: produceDataStructureEntityId,
            id: newProducerId,
            columnNames: new[] {"Name", "NameAndAddress", "Email"},
            expectedColumnValues: new object[] {"testName1", "testNameAndAddress1", "test.test@test.test1"});
        createdProducers.Add(newProducerId);
    }
    [Test, Order(103)]
    public void ShouldUpdateExistingProducer()
    {
        Guid producerId = createdProducers[0];
        var actionResult = uiServiceController.Row(new UpdateRowInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            NewValues = new Dictionary<string, string>
            {
                {"Name", "testNameUpdated"},
                {"NameAndAddress", "testNameAndAddressUpdated"},
                {"Email", "test.testUpdated@test.test1"}
            },
            RowId = producerId
        });
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult) actionResult;
        AssertObjectIsPersisted(
            menuId: producerMenuId,
            dataStructureId: produceDataStructureEntityId,
            id: producerId,
            columnNames: new[] {"Name", "NameAndAddress", "Email"},
            expectedColumnValues: new object[]
                {"testNameUpdated", "testNameAndAddressUpdated", "test.testUpdated@test.test1"});
    }
    [Test, Order(104)]
    public void ShouldDeleteProducer()
    {
        Guid producerId = createdProducers[0];
        uiServiceController.Row(new DeleteRowInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            RowIdToDelete = producerId
        });
        IActionResult entitiesActionResult = uiServiceController.GetRows(new GetRowsInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            Filter = $"[\"Id\",\"eq\",\"{producerId}\"]",
            Ordering = new List<List<string>>(),
            RowLimit = 0,
            ColumnNames = new[] {"Name", "NameAndAddress", "Email"}
        });
        Assert.IsInstanceOf<OkObjectResult>(entitiesActionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult) entitiesActionResult;
        Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
        var retrievedObjects = ((IEnumerable<object>) entitiesObjResult.Value).ToList();
        Assert.That(retrievedObjects, Has.Count.EqualTo(0));
    }
    [Test, Order(105)]
    public void ShouldRetrieveProducersWithParameters()
    {
        string[] columnNames = {"Id", "Name", "NameAndAddress", "PlotMinimumSize" };
        IActionResult entitiesActionResult = uiServiceController.GetRows(new GetRowsInput
        {
            MenuId = producerMenuId,
            DataStructureEntityId = produceDataStructureEntityId,
            Filter = $"[\"Name\",\"like\",\"name1%\"]",
            Ordering = new List<List<string>>
            {
                new List<string>{"PlotMinimumSize","asc"}, 
            },
            RowLimit = 3,
            ColumnNames = columnNames
        });
        Assert.IsInstanceOf<OkObjectResult>(entitiesActionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult) entitiesActionResult;
        Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
        var retrievedObjects = ((IEnumerable<object>) entitiesObjResult.Value).ToList();
        Assert.That(retrievedObjects, Has.Count.EqualTo(3));
        object[] firstRow = (object[]) retrievedObjects[0];
        object[] secondRow = (object[])retrievedObjects[1];
        object[] thirdRow = (object[])retrievedObjects[2];
        Assert.That(firstRow[1], Is.EqualTo("name11"));
        Assert.That(secondRow[1], Is.EqualTo("name14"));
        Assert.That(thirdRow[1], Is.EqualTo("name13"));
    }
    [Test, Order(106)]
    public void ShouldGetLookupLabels()
    {
        var actionResult = uiServiceController.GetLookupLabels(new LookupLabelsInput
        {
            MenuId = new Guid("6d181c9f-c89c-4b0a-bfc2-1a59f2a025ce"),
            LabelIds = new object[]
            {
                new Guid("3107b7f8-43ec-41cd-8fc6-563ffdf72278"),
                new Guid("251fc6a3-9cda-4cfb-b515-001367c92194")
            },
            LookupId = new Guid("78d745c1-e83a-4ab6-ae89-7bbac2d8d146")
        });
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult)actionResult;
        Assert.IsInstanceOf<IDictionary<Guid,string>>(entitiesObjResult.Value);
        var lookupDict = (IDictionary <Guid, string>)entitiesObjResult.Value;
        Assert.That(lookupDict[new Guid("3107b7f8-43ec-41cd-8fc6-563ffdf72278")], Is.EqualTo("50000000 TALDIS AG Üüö"));
        Assert.That(lookupDict[new Guid("251fc6a3-9cda-4cfb-b515-001367c92194")], Is.EqualTo("50000002 test678691"));
    }
    [Test, Order(107)]
    public void ShouldGetLookupList()
    {
        var actionResult = uiServiceController.GetLookupList( new LookupListInput
        {
            MenuId = new Guid("4c15ccbf-33ee-4746-b059-04f4a49c5a1a"),
            Id = new Guid("05692FC4-1206-48FC-80B5-75DD2C7084BE"),
            LookupId = new Guid("5f644fbe-7723-4004-9153-b296a1c27264"),
            Property = "Name",
            ColumnNames = new[] {"Name"},
            ShowUniqueValues = false,
            SearchText ="",
            PageNumber = 1,
            PageSize = 10,
            DataStructureEntityId= new Guid("fc652e79-6822-465b-ab89-1cd8ada8100d")
        });
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        OkObjectResult entitiesObjResult = (OkObjectResult)actionResult;
        Assert.IsInstanceOf<IEnumerable<object>>(entitiesObjResult.Value);
        var lookupList= ((IEnumerable<object>)entitiesObjResult.Value).ToList();
        Assert.That(lookupList, Has.Count.EqualTo(24));
    }
}
