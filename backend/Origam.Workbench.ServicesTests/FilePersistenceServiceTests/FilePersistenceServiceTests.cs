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
public class FilePersistenceServiceTests: AbstractFileTestClass
{
    private string PathToRuntimeModelConfig => 
        Path.Combine(PathToTestDirectory, "RuntimeModelConfiguration.json");
    private string PathToTestDirectory => Path.Combine(TestContext.WorkDirectory, 
        "FilePersistenceServiceTests", "TestFiles");
    protected override TestContext TestContext =>
        TestContext.CurrentContext;
    
    [Test]
    public void ShouldReloadModelWhenChangesDetected()
    {
        string pathToTestDirectory = Path.Combine(TestContext.WorkDirectory, 
            "FilePersistenceServiceTests", "TestFiles");
        string pathToTestFile = Path.Combine(pathToTestDirectory, "TestEntity.origam");
        Guid testEntityId = new Guid("4a803640-f5dd-46d8-8294-7ba3baa8ff6d");
        
        var sut = InitializeFilePersistenceService(pathToTestDirectory);
        bool reloadNeededEventCalled = false;
        var itemBeforeChange = sut.SchemaProvider
            .RetrieveInstance<TableMappingItem>(testEntityId);
        
        Assert.That(itemBeforeChange.Name, Is.EqualTo("TestEntity"));
        
        sut.ReloadNeeded += (sender, args) =>
        {
            reloadNeededEventCalled = true;
            Maybe<XmlLoadError> maybeError = sut.Reload();
            Assert.IsTrue(maybeError.HasNoValue);
            
            var itemAfterChange = sut.SchemaProvider
                .RetrieveInstance<TableMappingItem>(testEntityId);
            Assert.That(itemAfterChange.Name, Is.EqualTo("TestEntityChanged"));
        };
        string testFileContents = File.ReadAllText(pathToTestFile);
        testFileContents = testFileContents.Replace("asi:name=\"TestEntity\"","asi:name=\"TestEntityChanged\"");
        File.WriteAllText(pathToTestFile, testFileContents);
        Thread.Sleep(3000);
        
        Assert.IsTrue(reloadNeededEventCalled);
        File.Delete(pathToTestFile);
    }
    
    [Test]
    public void ShouldReadNewValueAfterRuntimeConfigChanged()
    {
        Guid testItemId = new Guid("5c42ad31-e3f6-4bb4-bc03-fd5f6d930b1d");
        
        SetTraceLevelInConfigFile(oldValue: "No",newValue: "Yes");
        var sut = InitializeFilePersistenceService(PathToTestDirectory);
        
        var itemBeforeChange = sut.SchemaProvider
            .RetrieveInstance<Schema.WorkflowModel.Workflow>(testItemId);
        Assert.That(itemBeforeChange.TraceLevel, Is.EqualTo(Trace.Yes));
        SetTraceLevelInConfigFile(oldValue: "Yes",newValue: "No");
        Thread.Sleep(500);
        
        var itemAfterChange = sut.SchemaProvider
            .RetrieveInstance<Schema.WorkflowModel.Workflow>(testItemId);
        Assert.That(itemAfterChange.TraceLevel, Is.EqualTo(Trace.No));
    }
    private void SetTraceLevelInConfigFile(string oldValue, string newValue)
    {
        string configText = File.ReadAllText(PathToRuntimeModelConfig);
        configText = configText.Replace(
            $"\"PropertyValue\": \"{oldValue}\"",
            $"\"PropertyValue\": \"{newValue}\"");
        File.WriteAllText(PathToRuntimeModelConfig, configText);
    }
    private FilePersistenceService InitializeFilePersistenceService(
        string pathToTestDirectory)
    {
        List<string> defaultFolders = new List<string>
        {
            CategoryFactory.Create(typeof(Package)),
            CategoryFactory.Create(typeof(SchemaItemGroup))
        };
        ConfigurationManager.SetActiveConfiguration(new OrigamSettings());
        
        var sut = new FilePersistenceService(
            metaModelUpgradeService: new NullMetaModelUpgradeService(),
            defaultFolders: defaultFolders,
            pathToRuntimeModelConfig: PathToRuntimeModelConfig,
            basePath: pathToTestDirectory,
            watchFileChanges: true,
            useBinFile: false,
            checkRules: false,
            mode: MetaModelUpgradeMode.Ignore);
        return sut;
    }
}
