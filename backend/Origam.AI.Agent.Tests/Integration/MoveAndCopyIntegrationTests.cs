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
[Category(AgentIntegrationTestBase.IntegrationCategory)]
public sealed class MoveAndCopyIntegrationTests : AgentIntegrationTestBase
{
    private const string EntityTypeName = "Database Entity";
    private const string ConstantTypeName = "Data Constant";
    private const string RootConstantName = "InitialUserCreated";
    private const string MoveToolName = "Movenode";
    private const string GroupReferenceFileName = ".origamGroupReference";

    private readonly List<string> copiedItemIds = [];
    private ModelItemFactory itemFactory = null!;
    private ModelTransferProbe transfers = null!;

    [SetUp]
    public void CreateModelHelpers()
    {
        itemFactory = new ModelItemFactory(AgentClient.Architect);
        transfers = new ModelTransferProbe(AgentClient.Architect);
    }

    [TearDown]
    public async Task DeleteTheCopies()
    {
        foreach (var itemId in copiedItemIds)
        {
            var status = await Model.DeleteSchemaItemAsync(itemId);
            TestContext.Progress.WriteLine($"cleanup: copy {itemId} -> {status}");
        }
        copiedItemIds.Clear();
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task MoveToAFolderOfAnotherPackage_KeepsThePackage_ThenMovesItBack()
    {
        var entityName = "AiMove" + NewSuffix();
        CreatedEntityNames.Add(entityName);
        var entityId = await CreateTopLevelEntityAsync(entityName);

        var beforeMove = await transfers.ReadMoveTargetsAsync(entityId);
        var originalLocation = RequireCurrentLocation(beforeMove, entityName);
        var originalFile = FindModelFile(entityName);
        var foreignFolder = beforeMove
            .Targets.Where(target =>
                target
                    is {
                        IsInActivePackage: false,
                        CanMove: true,
                        IsCurrentLocation: false,
                        IsTopLevelFolder: false,
                    }
            )
            .OrderBy(target => target.Path, StringComparer.Ordinal)
            .FirstOrDefault();
        if (foreignFolder is null)
        {
            Assert.Ignore($"No folder of another package accepts '{entityName}'.");
            return;
        }

        var conversation = new AgentConversation();
        var moveTrace = await RunAgentAsync(
            prompt: $"Move the database entity {entityName} into the folder "
                + $"{foreignFolder.NodeText} that belongs to the package "
                + $"{foreignFolder.PackageName}. Do not ask me to confirm anything, just move it.",
            conversation
        );

        Assert.That(moveTrace.ErrorMessage, Is.Null);
        Assert.That(
            moveTrace.UsedTool(MoveToolName),
            Is.True,
            "MoveNode was never called. Tools used: " + moveTrace.DescribeToolNames()
        );
        var afterMove = await transfers.ReadMoveTargetsAsync(entityId);
        Assert.That(
            afterMove.CurrentLocation?.Id,
            Is.EqualTo(foreignFolder.Id),
            $"'{entityName}' is not in {foreignFolder.Path} ({foreignFolder.PackageName}) after "
                + $"the move, it is {afterMove.Describe()}. "
                + moveTrace.Describe()
        );
        Assert.That(
            afterMove.IsSourceInActivePackage,
            Is.True,
            $"Moving '{entityName}' into a folder of {foreignFolder.PackageName} also moved it "
                + "out of the active package. "
                + moveTrace.Describe()
        );
        await AssertStillOneItemAsync(entityName, EntityTypeName, entityId, moveTrace);
        if (originalFile is not null)
        {
            var movedFile = FindModelFile(entityName)!;
            Assert.That(
                PackageFolderOf(movedFile),
                Is.EqualTo(PackageFolderOf(originalFile)),
                $"The file of '{entityName}' moved into another package folder: "
                    + movedFile.FullName
            );
            Assert.That(
                movedFile.Directory!.Name,
                Is.EqualTo(foreignFolder.NodeText),
                $"The file of '{entityName}' is not in a folder named after "
                    + $"{foreignFolder.NodeText}: {movedFile.FullName}"
            );
            Assert.That(
                File.Exists(Path.Combine(movedFile.DirectoryName!, GroupReferenceFileName)),
                Is.True,
                $"No {GroupReferenceFileName} next to {movedFile.FullName}, so the file does not "
                    + $"point at the folder of {foreignFolder.PackageName}."
            );
        }

        var backTrace = await RunAgentAsync(
            prompt: "Now move it back to the folder it was in before. Do not ask me to confirm "
                + "anything, just move it.",
            conversation
        );

        Assert.That(backTrace.ErrorMessage, Is.Null);
        var afterMoveBack = await transfers.ReadMoveTargetsAsync(entityId);
        Assert.That(
            afterMoveBack.CurrentLocation?.Id,
            Is.EqualTo(originalLocation.Id),
            $"'{entityName}' did not return to {originalLocation.Path}, it is "
                + $"{afterMoveBack.Describe()}. "
                + backTrace.Describe()
        );
        Assert.That(afterMoveBack.IsSourceInActivePackage, Is.True, backTrace.Describe());
        await AssertStillOneItemAsync(entityName, EntityTypeName, entityId, backTrace);
        if (originalFile is not null)
        {
            Assert.That(
                FindModelFile(entityName)!.FullName,
                Is.EqualTo(originalFile.FullName),
                $"The file of '{entityName}' did not return to where it was."
            );
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task CopyOfAnItemFromAnotherPackage_LandsInTheActivePackage()
    {
        var originalId = await RequireRootConstantAsync();
        var beforeCopy = await transfers.ReadMoveTargetsAsync(originalId);
        if (beforeCopy.IsSourceInActivePackage)
        {
            Assert.Ignore($"'{RootConstantName}' belongs to the active package here.");
            return;
        }

        var originalLocation = RequireCurrentLocation(beforeCopy, RootConstantName);
        var foreignFolder = beforeCopy
            .Targets.Where(target =>
                target
                    is {
                        IsInActivePackage: false,
                        CanCopy: true,
                        IsCurrentLocation: false,
                        IsTopLevelFolder: false,
                    }
            )
            .OrderBy(target => target.Path, StringComparer.Ordinal)
            .FirstOrDefault();
        if (foreignFolder is null)
        {
            Assert.Ignore($"No folder of another package accepts a copy of '{RootConstantName}'.");
            return;
        }

        var constantsBefore = await Model.FindSchemaItemsContainingAsync(
            RootConstantName,
            ConstantTypeName
        );
        var trace = await RunAgentAsync(
            prompt: $"Make a copy of the data constant {RootConstantName} and put the copy into "
                + $"the folder {foreignFolder.NodeText} that belongs to the package "
                + $"{foreignFolder.PackageName}. Leave the original where it is. Do not ask me to "
                + "confirm anything, just copy it."
        );

        var copies = await FindNewItemsAsync(RootConstantName, ConstantTypeName, constantsBefore);
        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool(MoveToolName),
            Is.True,
            "MoveNode was never called. Tools used: " + trace.DescribeToolNames()
        );
        Assert.That(
            copies,
            Has.Count.EqualTo(1),
            $"Expected exactly one new copy of '{RootConstantName}'. " + trace.Describe()
        );

        var copyTargets = await transfers.ReadMoveTargetsAsync(copies[0].Id);
        Assert.That(
            copyTargets.IsSourceInActivePackage,
            Is.True,
            $"The copy '{copies[0].Name}' was not created in the active package. "
                + trace.Describe()
        );
        Assert.That(
            copyTargets.CurrentLocation?.Id,
            Is.EqualTo(foreignFolder.Id),
            $"The copy '{copies[0].Name}' is not in {foreignFolder.Path} "
                + $"({foreignFolder.PackageName}), it is {copyTargets.Describe()}. "
                + trace.Describe()
        );
        await AssertOriginalUntouchedAsync(originalId, originalLocation, trace);

        var copyFile = FindModelFile(copies[0].Name);
        if (copyFile is not null)
        {
            Assert.That(
                PackageFolderOf(copyFile),
                Is.EqualTo(InProcessArchitect.ActivePackageName),
                $"The copy was written outside the active package folder: {copyFile.FullName}"
            );
            Assert.That(
                File.Exists(Path.Combine(copyFile.DirectoryName!, GroupReferenceFileName)),
                Is.True,
                $"No {GroupReferenceFileName} next to {copyFile.FullName}."
            );
        }
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task MoveOfAnItemFromAnotherPackage_IsRefused_AndNothingIsCopied()
    {
        var originalId = await RequireRootConstantAsync();
        var beforeMove = await transfers.ReadMoveTargetsAsync(originalId);
        if (beforeMove.IsSourceInActivePackage)
        {
            Assert.Ignore($"'{RootConstantName}' belongs to the active package here.");
            return;
        }

        var originalLocation = RequireCurrentLocation(beforeMove, RootConstantName);
        var folder = beforeMove
            .Targets.Where(target =>
                target is { CanCopy: true, IsCurrentLocation: false, IsTopLevelFolder: false }
            )
            .OrderByDescending(target => target.IsInActivePackage)
            .ThenBy(target => target.Path, StringComparer.Ordinal)
            .FirstOrDefault();
        if (folder is null)
        {
            Assert.Ignore($"No folder accepts '{RootConstantName}'.");
            return;
        }

        var constantsBefore = await Model.FindSchemaItemsContainingAsync(
            RootConstantName,
            ConstantTypeName
        );
        var trace = await RunAgentAsync(
            prompt: $"Move the data constant {RootConstantName} into the folder {folder.NodeText} "
                + $"that belongs to the package {folder.PackageName}. Do not ask me to confirm "
                + "anything."
        );

        var copies = await FindNewItemsAsync(RootConstantName, ConstantTypeName, constantsBefore);
        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            copies,
            Is.Empty,
            $"The user asked for a move that is not allowed, and the agent made a copy instead "
                + "of telling them. "
                + trace.Describe()
        );
        await AssertOriginalUntouchedAsync(originalId, originalLocation, trace);
        Assert.That(
            trace.ReplyText,
            Does.Contain("cop").IgnoreCase,
            "The agent did not tell the user that the item can only be copied. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task FieldMoveToAnotherEntity_KeepsTheFieldAndItsId()
    {
        var suffix = NewSuffix();
        var sourceEntityName = "AiMoveFrom" + suffix;
        var targetEntityName = "AiMoveTo" + suffix;
        var fieldName = "AiMovedField" + suffix;
        CreatedEntityNames.Add(sourceEntityName);
        CreatedEntityNames.Add(targetEntityName);
        var sourceEntityId = await CreateTopLevelEntityAsync(sourceEntityName);
        var targetEntityId = await CreateTopLevelEntityAsync(targetEntityName);
        var fieldId = await itemFactory.CreateTextFieldAsync(sourceEntityId, fieldName);
        await Model.CloseAllTabsAsync();

        var trace = await RunAgentAsync(
            prompt: $"Move the database field {fieldName} from the entity {sourceEntityName} to "
                + $"the entity {targetEntityName}. Do not ask me to confirm anything, just move it."
        );

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool(MoveToolName),
            Is.True,
            "MoveNode was never called. Tools used: " + trace.DescribeToolNames()
        );
        var targetFields = await Model.ReadDatabaseFieldsAsync(targetEntityId);
        Assert.That(
            targetFields.Select(field => field.Id),
            Does.Contain(fieldId),
            $"'{fieldName}' ({fieldId}) is not a field of '{targetEntityName}' after the move. "
                + trace.Describe()
        );
        var sourceFields = await Model.ReadDatabaseFieldsAsync(sourceEntityId);
        Assert.That(
            sourceFields.Select(field => field.Name),
            Does.Not.Contain(fieldName),
            $"'{fieldName}' is still in '{sourceEntityName}'. " + trace.Describe()
        );
        Assert.That(
            await Model.FindSchemaItemsAsync(fieldName),
            Is.EqualTo(new[] { fieldId }),
            $"The model holds other items named '{fieldName}' than the original field, so the "
                + "agent re-created the field instead of moving it. "
                + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task CopyOfAnEntityNextToTheOriginal_IsNamedCopyOfAndHasTheFields()
    {
        var entityName = "AiCopy" + NewSuffix();
        var copyName = "Copy of " + entityName;
        const string fieldName = "Note";
        CreatedEntityNames.Add(copyName);
        CreatedEntityNames.Add(entityName);
        var entityId = await CreateTopLevelEntityAsync(entityName);
        var fieldId = await itemFactory.CreateTextFieldAsync(entityId, fieldName);
        await Model.CloseAllTabsAsync();
        var originalLocation = RequireCurrentLocation(
            await transfers.ReadMoveTargetsAsync(entityId),
            entityName
        );

        var trace = await RunAgentAsync(
            prompt: $"Make a copy of the database entity {entityName} in the same folder as the "
                + "original and keep the original as it is. Do not ask me to confirm anything, "
                + "just copy it."
        );

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool(MoveToolName),
            Is.True,
            "MoveNode was never called. Tools used: " + trace.DescribeToolNames()
        );
        var copyIds = await Model.FindSchemaItemsAsync(copyName, EntityTypeName);
        Assert.That(
            copyIds,
            Has.Count.EqualTo(1),
            $"Expected exactly one entity named '{copyName}'. " + trace.Describe()
        );
        var copyTargets = await transfers.ReadMoveTargetsAsync(copyIds[0]);
        Assert.That(copyTargets.IsSourceInActivePackage, Is.True, trace.Describe());
        Assert.That(
            copyTargets.CurrentLocation?.Id,
            Is.EqualTo(originalLocation.Id),
            $"The copy is not next to the original in {originalLocation.Path}, it is "
                + $"{copyTargets.Describe()}. "
                + trace.Describe()
        );
        var copyFieldIds = (await Model.ReadDatabaseFieldsAsync(copyIds[0]))
            .Where(field => field.Name == fieldName)
            .Select(field => field.Id)
            .ToList();
        Assert.That(
            copyFieldIds,
            Has.Count.EqualTo(1),
            $"The copy does not have a '{fieldName}' field. " + trace.Describe()
        );
        Assert.That(
            copyFieldIds[0],
            Is.Not.EqualTo(fieldId),
            $"The copy shares the original's '{fieldName}' field instead of having its own."
        );
        Assert.That(
            (await Model.ReadDatabaseFieldsAsync(entityId)).Select(field => field.Id),
            Does.Contain(fieldId),
            $"The original lost its '{fieldName}' field. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task FieldCopyToAnotherEntity_KeepsTheNameAndTheOriginal()
    {
        var suffix = NewSuffix();
        var sourceEntityName = "AiCopyFrom" + suffix;
        var targetEntityName = "AiCopyTo" + suffix;
        var fieldName = "AiCopiedField" + suffix;
        CreatedEntityNames.Add(sourceEntityName);
        CreatedEntityNames.Add(targetEntityName);
        var sourceEntityId = await CreateTopLevelEntityAsync(sourceEntityName);
        var targetEntityId = await CreateTopLevelEntityAsync(targetEntityName);
        var fieldId = await itemFactory.CreateTextFieldAsync(sourceEntityId, fieldName);
        await Model.CloseAllTabsAsync();

        var trace = await RunAgentAsync(
            prompt: $"Copy the database field {fieldName} of the entity {sourceEntityName} to the "
                + $"entity {targetEntityName}, so that both entities have it. Do not ask me to "
                + "confirm anything, just copy it."
        );

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            trace.UsedTool(MoveToolName),
            Is.True,
            "MoveNode was never called. Tools used: " + trace.DescribeToolNames()
        );
        var copyIds = (await Model.ReadDatabaseFieldsAsync(targetEntityId))
            .Where(field => field.Name == fieldName)
            .Select(field => field.Id)
            .ToList();
        Assert.That(
            copyIds,
            Has.Count.EqualTo(1),
            $"'{targetEntityName}' has no field named '{fieldName}' after the copy. "
                + trace.Describe()
        );
        Assert.That(
            copyIds[0],
            Is.Not.EqualTo(fieldId),
            $"'{targetEntityName}' got the original field instead of a copy of it. "
                + trace.Describe()
        );
        Assert.That(
            (await Model.ReadDatabaseFieldsAsync(sourceEntityId)).Select(field => field.Id),
            Does.Contain(fieldId),
            $"'{sourceEntityName}' lost the original field. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task PackageChangeRequest_IsNotImitatedByCopyAndDelete()
    {
        var entityName = "AiPackage" + NewSuffix();
        CreatedEntityNames.Add("Copy of " + entityName);
        CreatedEntityNames.Add(entityName);
        var entityId = await CreateTopLevelEntityAsync(entityName);
        var foreignPackageName = (await transfers.ReadMoveTargetsAsync(entityId))
            .Targets.Where(target => !target.IsInActivePackage)
            .Select(target => target.PackageName)
            .OrderBy(packageName => packageName, StringComparer.Ordinal)
            .FirstOrDefault();
        if (foreignPackageName is null)
        {
            Assert.Ignore($"No other package offers a folder for '{entityName}'.");
            return;
        }

        var trace = await RunAgentAsync(
            prompt: $"Change the package of the database entity {entityName} to "
                + $"{foreignPackageName}, it should belong to {foreignPackageName} from now on. Do "
                + "not ask me to confirm anything."
        );

        Assert.That(trace.ErrorMessage, Is.Null);
        Assert.That(
            (await Model.FindSchemaItemsContainingAsync(entityName, EntityTypeName)).Select(item =>
                item.Id
            ),
            Is.EqualTo(new[] { entityId }),
            $"The agent copied, deleted or re-created '{entityName}' to imitate a package change "
                + "that no tool can make. "
                + trace.Describe()
        );
        Assert.That(
            (await transfers.ReadMoveTargetsAsync(entityId)).IsSourceInActivePackage,
            Is.True,
            $"'{entityName}' left the active package. " + trace.Describe()
        );
    }

    [Test]
    [Category(MutatingCategory)]
    public async Task MoveOfTwoEntitiesInOneRequest_MovesBothIntoTheFolder()
    {
        var suffix = NewSuffix();
        var firstEntityName = "AiMoveFirst" + suffix;
        var secondEntityName = "AiMoveSecond" + suffix;
        CreatedEntityNames.Add(firstEntityName);
        CreatedEntityNames.Add(secondEntityName);
        var firstEntityId = await CreateTopLevelEntityAsync(firstEntityName);
        var secondEntityId = await CreateTopLevelEntityAsync(secondEntityName);
        var folder = (await transfers.ReadMoveTargetsAsync(firstEntityId))
            .Targets.Where(target =>
                target is { CanMove: true, IsCurrentLocation: false, IsTopLevelFolder: false }
            )
            .OrderByDescending(target => target.Path, StringComparer.Ordinal)
            .FirstOrDefault();
        if (folder is null)
        {
            Assert.Ignore($"No folder accepts '{firstEntityName}'.");
            return;
        }

        var trace = await RunAgentAsync(
            prompt: $"Move the database entities {firstEntityName} and {secondEntityName} into the "
                + $"folder {folder.NodeText} that belongs to the package {folder.PackageName}. Do "
                + "not ask me to confirm anything, just move them."
        );

        Assert.That(trace.ErrorMessage, Is.Null);
        foreach (
            var (entityName, entityId) in new[]
            {
                (firstEntityName, firstEntityId),
                (secondEntityName, secondEntityId),
            }
        )
        {
            var afterMove = await transfers.ReadMoveTargetsAsync(entityId);
            Assert.That(
                afterMove.CurrentLocation?.Id,
                Is.EqualTo(folder.Id),
                $"'{entityName}' is not in {folder.Path} ({folder.PackageName}), it is "
                    + $"{afterMove.Describe()}. "
                    + trace.Describe()
            );
            await AssertStillOneItemAsync(entityName, EntityTypeName, entityId, trace);
        }
    }

    private static string NewSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    private async Task<AgentRunTrace> RunAgentAsync(
        string prompt,
        AgentConversation? conversation = null
    )
    {
        var trace = await RunBenchmarkAsync(prompt, ChatFocus, conversation);
        TestContext.WriteLine(trace.Describe());
        return trace;
    }

    private async Task<string> CreateTopLevelEntityAsync(string entityName)
    {
        var entitiesFolderId = await itemFactory.FindTopLevelFolderIdAsync(
            categoryText: "Data",
            folderText: "Entities"
        );
        var entityId = await itemFactory.CreateEntityAsync(entitiesFolderId, entityName);
        await Model.CloseAllTabsAsync();
        return entityId;
    }

    private async Task<string> RequireRootConstantAsync()
    {
        var matches = await Model.FindSchemaItemsAsync(RootConstantName, ConstantTypeName);
        if (matches.Count == 0)
        {
            Assert.Ignore($"The model has no data constant named '{RootConstantName}'.");
        }

        return matches[0];
    }

    private static MoveTarget RequireCurrentLocation(MoveTargets targets, string itemName)
    {
        Assert.That(
            targets.CurrentLocation,
            Is.Not.Null,
            $"GetMoveTargets does not list where '{itemName}' is now."
        );
        return targets.CurrentLocation!;
    }

    private async Task<IReadOnlyList<ModelItemReference>> FindNewItemsAsync(
        string nameFragment,
        string itemTypeName,
        IReadOnlyList<ModelItemReference> itemsBefore
    )
    {
        var knownIds = itemsBefore.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var newItems = (await Model.FindSchemaItemsContainingAsync(nameFragment, itemTypeName))
            .Where(item => !knownIds.Contains(item.Id))
            .ToList();
        copiedItemIds.AddRange(
            newItems.Select(item => item.Id).Where(itemId => !copiedItemIds.Contains(itemId))
        );
        return newItems;
    }

    private async Task AssertStillOneItemAsync(
        string itemName,
        string itemTypeName,
        string itemId,
        AgentRunTrace trace
    )
    {
        Assert.That(
            await Model.FindSchemaItemsAsync(itemName, itemTypeName),
            Is.EqualTo(new[] { itemId }),
            $"'{itemName}' is no longer exactly the original item {itemId}, so the agent "
                + "copied or re-created it instead of moving it. "
                + trace.Describe()
        );
    }

    private async Task AssertOriginalUntouchedAsync(
        string originalId,
        MoveTarget originalLocation,
        AgentRunTrace trace
    )
    {
        var original = await transfers.ReadMoveTargetsAsync(originalId);
        Assert.That(
            original.IsSourceInActivePackage,
            Is.False,
            $"'{RootConstantName}' changed its package. " + trace.Describe()
        );
        Assert.That(
            original.CurrentLocation?.Id,
            Is.EqualTo(originalLocation.Id),
            $"'{RootConstantName}' left {originalLocation.Path}, it is {original.Describe()}. "
                + trace.Describe()
        );
    }

    private static FileInfo? FindModelFile(string itemName)
    {
        var modelDirectory = ModelSandbox.CurrentModelDirectory;
        if (modelDirectory is null)
        {
            return null;
        }

        var files = modelDirectory.GetFiles(itemName + ".origam", SearchOption.AllDirectories);
        Assert.That(
            files.Select(file => file.FullName),
            Has.Exactly(1).Items,
            $"Expected exactly one model file for '{itemName}' in {modelDirectory.FullName}."
        );
        return files[0];
    }

    private static string PackageFolderOf(FileInfo file)
    {
        var relativePath = Path.GetRelativePath(
            ModelSandbox.CurrentModelDirectory!.FullName,
            file.FullName
        );
        return relativePath.Split(Path.DirectorySeparatorChar)[0];
    }
}
