#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Threading;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Origam.DA;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.TestCommon;
using Origam.Workbench.Services;

namespace Origam.Workbench.ServicesTests;

[TestFixture]
public class FilePersistenceServiceTests : AbstractFileTestClass
{
    private string PathToRuntimeModelConfig =>
        Path.Combine(path1: PathToTestDirectory, path2: "RuntimeModelConfiguration.json");
    private string PathToTestDirectory =>
        Path.Combine(
            path1: TestContext.WorkDirectory,
            path2: "FilePersistenceServiceTests",
            path3: "TestFiles"
        );
    protected override TestContext TestContext => TestContext.CurrentContext;

    [Test]
    public void ShouldReloadModelWhenChangesDetected()
    {
        string pathToTestDirectory = Path.Combine(
            path1: TestContext.WorkDirectory,
            path2: "FilePersistenceServiceTests",
            path3: "TestFiles"
        );
        string pathToTestFile = Path.Combine(
            path1: pathToTestDirectory,
            path2: "TestEntity.origam"
        );
        Guid testEntityId = new Guid(g: "4a803640-f5dd-46d8-8294-7ba3baa8ff6d");

        var sut = InitializeFilePersistenceService(pathToTestDirectory: pathToTestDirectory);
        bool reloadNeededEventCalled = false;
        var itemBeforeChange = sut.SchemaProvider.RetrieveInstance<TableMappingItem>(
            instanceId: testEntityId
        );

        Assert.That(actual: itemBeforeChange.Name, expression: Is.EqualTo(expected: "TestEntity"));

        sut.ReloadNeeded += (sender, args) =>
        {
            reloadNeededEventCalled = true;
            Maybe<XmlLoadError> maybeError = sut.Reload();
            Assert.IsTrue(condition: maybeError.HasNoValue);

            var itemAfterChange = sut.SchemaProvider.RetrieveInstance<TableMappingItem>(
                instanceId: testEntityId
            );
            Assert.That(
                actual: itemAfterChange.Name,
                expression: Is.EqualTo(expected: "TestEntityChanged")
            );
        };
        string testFileContents = File.ReadAllText(path: pathToTestFile);
        testFileContents = testFileContents.Replace(
            oldValue: "asi:name=\"TestEntity\"",
            newValue: "asi:name=\"TestEntityChanged\""
        );
        File.WriteAllText(path: pathToTestFile, contents: testFileContents);
        Thread.Sleep(millisecondsTimeout: 3000);

        Assert.IsTrue(condition: reloadNeededEventCalled);
        File.Delete(path: pathToTestFile);
    }

    [Test]
    public void ShouldReadNewValueAfterRuntimeConfigChanged()
    {
        Guid testItemId = new Guid(g: "5c42ad31-e3f6-4bb4-bc03-fd5f6d930b1d");

        SetTraceLevelInConfigFile(oldValue: "No", newValue: "Yes");
        var sut = InitializeFilePersistenceService(pathToTestDirectory: PathToTestDirectory);

        var itemBeforeChange = sut.SchemaProvider.RetrieveInstance<Schema.WorkflowModel.Workflow>(
            instanceId: testItemId
        );
        Assert.That(
            actual: itemBeforeChange.TraceLevel,
            expression: Is.EqualTo(expected: Trace.Yes)
        );
        SetTraceLevelInConfigFile(oldValue: "Yes", newValue: "No");
        Thread.Sleep(millisecondsTimeout: 500);

        var itemAfterChange = sut.SchemaProvider.RetrieveInstance<Schema.WorkflowModel.Workflow>(
            instanceId: testItemId
        );
        Assert.That(actual: itemAfterChange.TraceLevel, expression: Is.EqualTo(expected: Trace.No));
    }

    private void SetTraceLevelInConfigFile(string oldValue, string newValue)
    {
        string configText = File.ReadAllText(path: PathToRuntimeModelConfig);
        configText = configText.Replace(
            oldValue: $"\"PropertyValue\": \"{oldValue}\"",
            newValue: $"\"PropertyValue\": \"{newValue}\""
        );
        File.WriteAllText(path: PathToRuntimeModelConfig, contents: configText);
    }

    private FilePersistenceService InitializeFilePersistenceService(string pathToTestDirectory)
    {
        List<string> defaultFolders = new List<string>
        {
            CategoryFactory.Create(type: typeof(Package)),
            CategoryFactory.Create(type: typeof(SchemaItemGroup)),
        };
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());

        var sut = new FilePersistenceService(
            metaModelUpgradeService: new NullMetaModelUpgradeService(),
            defaultFolders: defaultFolders,
            pathToRuntimeModelConfig: PathToRuntimeModelConfig,
            basePath: pathToTestDirectory,
            watchFileChanges: true,
            useBinFile: false,
            checkRules: false,
            mode: MetaModelUpgradeMode.Ignore
        );
        return sut;
    }
}
