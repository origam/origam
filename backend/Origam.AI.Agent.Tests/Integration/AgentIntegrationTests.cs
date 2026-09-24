#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure.Agent;
using Origam.AI.Agent.Tests.Infrastructure.Architect;

namespace Origam.AI.Agent.Tests.Integration;

[TestFixture]
[Explicit(AgentIntegrationTestBase.ExplicitReason)]
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class AgentIntegrationTests : AgentIntegrationTestBase
{
    [Test]
    [Category(MutatingCategory)]
    public async Task CreateNodeFunctionCall_ReallyCreatesTheEntityInTheModel()
    {
        var entityName = "AiBenchmark" + Guid.NewGuid().ToString("N")[..8];
        CreatedEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create a new database entity named {entityName} in dimensions folder"
                + "Do not ask me to confirm anything, just create it.",
            ChatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("CreateNode"),
            Is.True,
            "CreateNode was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );
        Assert.That(trace.Result.AffectedNodes, Is.Not.Empty);

        var matches = await Model.FindSchemaItemsAsync(entityName);
        Assert.That(
            matches,
            Is.Not.Empty,
            $"The agent reported success but '{entityName}' is not in the model."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task LookupWizard_CreatesTheLookupAndItsDataStructure()
    {
        var lookupName = "AiLookup" + Guid.NewGuid().ToString("N")[..8];
        var dataStructureName = "Lookup" + lookupName;
        CreatedEntityNames.Add(lookupName);
        CreatedEntityNames.Add(dataStructureName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create a lookup named {lookupName} for the entity Dimension1: display "
                + "field Name, id filter GetId, no list filter. Do not ask me to confirm "
                + "anything, just create it.",
            ChatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("PostWizardsLookups"),
            Is.True,
            "The lookup wizard was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called the lookup wizard but nothing was persisted. " + trace.Describe()
        );

        var builder = new ArchitectModelBuilder(AgentClient.Architect);
        Assert.That(
            await builder.FindItemIdAsync(lookupName, WidgetTestModel.LookupTypeName),
            Is.Not.Null,
            $"The agent reported success but the lookup '{lookupName}' is not in the model."
        );
        Assert.That(
            await builder.FindItemIdAsync(dataStructureName, itemTypeName: "Data Structure"),
            Is.Not.Null,
            $"The lookup was created but its data structure '{dataStructureName}' is not in the model."
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task OneToManyFunctionCalls_BothChildEntitiesPointAtTheMainEntity()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var mainEntityName = "AiBenchmarkOrder" + suffix;
        var firstChildEntityName = "AiBenchmarkOrderLine" + suffix;
        var secondChildEntityName = "AiBenchmarkOrderNote" + suffix;
        CreatedEntityNames.Add(firstChildEntityName);
        CreatedEntityNames.Add(secondChildEntityName);
        CreatedEntityNames.Add(mainEntityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Create three database entities named {mainEntityName}, "
                + $"{firstChildEntityName} and {secondChildEntityName}. {mainEntityName} is the "
                + "main entity and the other two are its children in a one-to-many relation: "
                + $"give each child a foreign key field named ref{mainEntityName}Id that points "
                + $"at {mainEntityName} and at its primary key. Do not ask me to confirm "
                + "anything, just create everything.",
            ChatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var mainEntityId = await RequireEntityAsync(mainEntityName);
        foreach (var childEntityName in new[] { firstChildEntityName, secondChildEntityName })
        {
            var childEntityId = await RequireEntityAsync(childEntityName);
            var foreignKeyFields = new List<string>();
            foreach (var field in await Model.ReadDatabaseFieldsAsync(childEntityId))
            {
                if (await Model.PointsAtEntityAsync(field.Id, mainEntityId))
                {
                    foreignKeyFields.Add(field.Id);
                }
            }

            Assert.That(
                foreignKeyFields,
                Is.Not.Empty,
                $"'{childEntityName}' has no database field whose ForeignKeyEntity and "
                    + $"ForeignKeyField point at '{mainEntityName}' ({mainEntityId}). "
                    + trace.Describe()
            );
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task DeleteFollowUp_RemovesTheEntityTheAgentCreatedInTheSameConversation()
    {
        var entityName = "AiBenchmarkDeleteMe" + Guid.NewGuid().ToString("N")[..8];
        CreatedEntityNames.Add(entityName);
        var conversation = new AgentConversation();

        var createTrace = await RunBenchmarkAsync(
            prompt: $"Create a new database entity named {entityName} with one database field "
                + "named Name. Do not ask me to confirm anything, just create it.",
            ChatFocus,
            conversation
        );

        Assert.That(createTrace.ErrorMessage, Is.Null);
        Assert.That(
            createTrace.Result!.ModelChanged,
            Is.True,
            "Nothing was persisted by the create turn. " + createTrace.Describe()
        );

        var entityId = await RequireEntityAsync(entityName);
        var fieldNames = (await Model.ReadDatabaseFieldsAsync(entityId)).Select(field =>
            field.Name
        );
        Assert.That(
            fieldNames,
            Does.Contain("Name"),
            $"'{entityName}' was created without a database field named Name. "
                + createTrace.Describe()
        );

        var deleteTrace = await RunBenchmarkAsync(
            prompt: "Now delete that entity you have just created, together with everything "
                + "inside it. Do not ask me to confirm anything, just delete it.",
            ChatFocus,
            conversation
        );

        Assert.That(deleteTrace.ErrorMessage, Is.Null);
        Assert.That(
            deleteTrace.Result!.ModelChanged,
            Is.True,
            "The delete turn changed nothing in the model. " + deleteTrace.Describe()
        );
        Assert.That(
            await Model.FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity"),
            Is.Empty,
            $"The agent reported the deletion but '{entityName}' ({entityId}) is still in the "
                + "model. "
                + deleteTrace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task CommunityGuidedApi_ReadsTheForumGuideAndThenCreatesTheDataPage()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkApi" + suffix;
        var dataStructureName = "AiBenchmarkApi" + suffix + "Structure";
        var pageName = "AiBenchmarkApi" + suffix + "Page";
        var pageUrl = "api/public/aibenchmark" + suffix;
        CreatedEntityNames.Add(pageName);
        CreatedEntityNames.Add(dataStructureName);
        CreatedEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: "Search the ORIGAM community forum for the guide on how to create a simple "
                + "API and read it before you touch the model, then build the API the way that "
                + $"guide describes: a database entity named {entityName} with one database "
                + $"field named Name, a data structure named {dataStructureName} over that "
                + $"entity, and a data page named {pageName} that publishes that data structure "
                + $"as JSON on the url {pageUrl}. Do not ask me to confirm anything, just create "
                + "everything.",
            ChatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);

        var createIndex = trace.FirstToolIndex("CreateNode");
        Assert.That(
            createIndex,
            Is.GreaterThanOrEqualTo(0),
            "CreateNode was never called. Tools used: " + toolNames
        );

        var searchIndex = trace.FirstToolIndex("SearchCommunity");
        Assert.That(
            searchIndex,
            Is.GreaterThanOrEqualTo(0).And.LessThan(createIndex),
            "The agent did not search the community forum before it started creating the API. "
                + "Tools used: "
                + toolNames
        );

        var readTopicIndex = trace.FirstToolIndex("ReadCommunityTopic");
        Assert.That(
            readTopicIndex,
            Is.GreaterThanOrEqualTo(0).And.LessThan(createIndex),
            "The agent never opened a community topic before it started creating the API, so it "
                + "built the API without the guide. Tools used: "
                + toolNames
        );

        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var dataStructureMatches = await Model.FindSchemaItemsAsync(
            dataStructureName,
            itemTypeName: "Data Structure"
        );
        Assert.That(
            dataStructureMatches,
            Is.Not.Empty,
            $"The data structure '{dataStructureName}' the API should read from is not in the "
                + "model. "
                + trace.Describe()
        );

        var pageMatches = await Model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pageMatches,
            Is.Not.Empty,
            $"The agent reported success but the data page '{pageName}' is not in the model. "
                + trace.Describe()
        );

        var pageProperties = await Model.ReadItemPropertiesAsync(pageMatches[0]);
        Assert.That(
            pageProperties.GetValueOrDefault(key: "Url", defaultValue: ""),
            Does.Contain(pageUrl).IgnoreCase,
            $"'{pageName}' was published on a different url. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "MimeType", defaultValue: ""),
            Does.Contain("json").IgnoreCase,
            $"'{pageName}' does not return JSON. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "DataStructure", defaultValue: ""),
            Does.Contain(dataStructureMatches[0]).IgnoreCase,
            $"'{pageName}' does not read from '{dataStructureName}' "
                + $"({dataStructureMatches[0]}). "
                + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task UnguidedApi_BuildsTheJsonApiWithoutSearchingTheForum()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkJsonApi" + suffix;
        var dataStructureName = "AiBenchmarkJsonApi" + suffix + "Structure";
        var pageName = "AiBenchmarkJsonApi" + suffix + "Page";
        var pageUrl = "api/public/aibenchmarkjson" + suffix;
        CreatedEntityNames.Add(pageName);
        CreatedEntityNames.Add(dataStructureName);
        CreatedEntityNames.Add(entityName);

        var trace = await RunBenchmarkAsync(
            prompt: $"Make me a public JSON API on the url {pageUrl} that returns the records of "
                + $"a new database entity named {entityName} with one text field named Name. Call "
                + $"the data structure {dataStructureName} and the data page {pageName}. Do not "
                + "ask me to confirm anything, just build it.",
            ChatFocus
        );

        var toolNames = trace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool("Community"),
            Is.False,
            "The community forum was available but this task is plain model work, so the agent "
                + "should have built the API without spending calls on the forum. Tools used: "
                + toolNames
        );
        Assert.That(
            trace.UsedTool("CreateNode"),
            Is.True,
            "CreateNode was never called. Tools used: " + toolNames
        );
        Assert.That(
            trace.Result!.ModelChanged,
            Is.True,
            "The agent called CreateNode but nothing was persisted. " + trace.Describe()
        );

        var entityId = await RequireEntityAsync(entityName);
        var fieldNames = (await Model.ReadDatabaseFieldsAsync(entityId))
            .Select(field => field.Name)
            .ToList();
        Assert.That(
            fieldNames,
            Does.Contain("Name"),
            $"'{entityName}' has no database field named Name. Fields found: "
                + string.Join(separator: ", ", fieldNames)
                + ". "
                + trace.Describe()
        );

        var dataStructureMatches = await Model.FindSchemaItemsAsync(
            dataStructureName,
            itemTypeName: "Data Structure"
        );
        Assert.That(
            dataStructureMatches,
            Is.Not.Empty,
            $"The data structure '{dataStructureName}' the API should read from is not in the "
                + "model. "
                + trace.Describe()
        );

        var pageMatches = await Model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pageMatches,
            Is.Not.Empty,
            $"The agent reported success but the data page '{pageName}' is not in the model. "
                + trace.Describe()
        );

        var pageProperties = await Model.ReadPersistedPropertiesAsync(pageMatches[0]);
        Assert.That(
            pageProperties.GetValueOrDefault(key: "Url", defaultValue: ""),
            Does.Contain(pageUrl).IgnoreCase,
            $"'{pageName}' was published on a different url. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "MimeType", defaultValue: ""),
            Does.Contain("json").IgnoreCase,
            $"'{pageName}' does not return JSON. " + trace.Describe()
        );
        Assert.That(
            pageProperties.GetValueOrDefault(key: "DataStructure", defaultValue: ""),
            Does.Contain(dataStructureMatches[0]).IgnoreCase,
            $"'{pageName}' does not read from '{dataStructureName}' "
                + $"({dataStructureMatches[0]}). "
                + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task PropertyEditorFollowUp_ChangesTheUrlOfTheDataPageItAlreadyPersisted()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var entityName = "AiBenchmarkProperty" + suffix;
        var dataStructureName = "AiBenchmarkProperty" + suffix + "Structure";
        var pageName = "AiBenchmarkProperty" + suffix + "Page";
        var originalUrl = "api/public/aibenchmarkproperty" + suffix;
        var changedUrl = "api/public/aibenchmarkproperty" + suffix + "renamed";
        CreatedEntityNames.Add(pageName);
        CreatedEntityNames.Add(dataStructureName);
        CreatedEntityNames.Add(entityName);
        var conversation = new AgentConversation();

        var createTrace = await RunBenchmarkAsync(
            prompt: $"Create a database entity named {entityName} with one database field named "
                + $"Name, a data structure named {dataStructureName} over that entity, and a data "
                + $"page named {pageName} that publishes that data structure as JSON on the url "
                + $"{originalUrl}. Do not ask me to confirm anything, just create everything.",
            ChatFocus,
            conversation
        );

        Assert.That(createTrace.ErrorMessage, Is.Null);

        var pagesBeforeEdit = await Model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page");
        Assert.That(
            pagesBeforeEdit,
            Is.Not.Empty,
            $"The setup turn left no data page named '{pageName}' in the model, so there is "
                + "nothing for the follow-up turn to edit. "
                + createTrace.Describe()
        );

        var pageId = pagesBeforeEdit[0];
        Assert.That(
            (await Model.ReadPersistedPropertiesAsync(pageId)).GetValueOrDefault(
                key: "Url",
                defaultValue: ""
            ),
            Does.Contain(originalUrl).IgnoreCase,
            $"The setup turn did not publish '{pageName}' on '{originalUrl}', so the follow-up "
                + "turn would not be changing what this test assumes. "
                + createTrace.Describe()
        );

        var editTrace = await RunBenchmarkAsync(
            prompt: $"Now change the url of the data page {pageName} to {changedUrl}. Edit the "
                + "page that already exists, do not create a second one, and do not ask me to "
                + "confirm anything.",
            ChatFocus,
            conversation
        );

        var toolNames = editTrace.DescribeToolNames();
        TestContext.WriteLine("tools: " + toolNames);

        Assert.That(editTrace.ErrorMessage, Is.Null);
        Assert.That(
            editTrace.UsedTool("PropertyEditor"),
            Is.True,
            "The agent never called PropertyEditor/Update, so it did not edit the page it had "
                + "already persisted. Tools used: "
                + toolNames
        );
        Assert.That(
            await Model.FindSchemaItemsAsync(pageName, itemTypeName: "Data Page"),
            Is.EqualTo(new[] { pageId }),
            $"The agent replaced '{pageName}' instead of editing the page it had already "
                + "persisted. "
                + editTrace.Describe()
        );
        Assert.That(
            (await Model.ReadPersistedPropertiesAsync(pageId)).GetValueOrDefault(
                key: "Url",
                defaultValue: ""
            ),
            Does.Contain(changedUrl).IgnoreCase,
            $"'{pageName}' still carries the old url once its tab is dropped and the item is "
                + "read again. PropertyEditor/Update only writes the copy held in the open tab, "
                + "so without a following Tab/PersistChanges the new url never reaches the disk. "
                + editTrace.Describe()
        );
    }

    private async Task<string> RequireEntityAsync(string entityName)
    {
        var matches = await Model.FindSchemaItemsAsync(entityName, itemTypeName: "Database Entity");
        Assert.That(
            matches,
            Is.Not.Empty,
            $"The agent reported success but the database entity '{entityName}' is not in the "
                + "model."
        );
        return matches[0];
    }
}
