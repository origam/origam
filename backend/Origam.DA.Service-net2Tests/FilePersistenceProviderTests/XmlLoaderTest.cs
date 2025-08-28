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
using System.IO;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.TestCommon;

namespace Origam.DA.Service_net2Tests;
[TestFixture]
public class XmlLoaderTest: AbstractFileTestClass
{
    protected override TestContext TestContext =>
        TestContext.CurrentContext;
    
    private List<string> parentFolders = new List<string>
    {
        OrigamFile.PackageCategory,
        OrigamFile.GroupCategory
    };
    private static readonly XmlFileDataFactory XmlFileDataFactory = 
        new XmlFileDataFactory();
    private OrigamFileFactory MakeOrigamFileFactory(DirectoryInfo topDir)
    {
        var pathFactory = new OrigamPathFactory(topDir);
        var index = new FilePersistenceIndex(pathFactory);
        var origamFileManager = new OrigamFileManager(
            index,
            pathFactory,
            new FileEventQueue(index, new NullWatchDog()));
        return new OrigamFileFactory(
            origamFileManager, parentFolders,pathFactory, 
            new FileEventQueue(index, new NullWatchDog()));
    }
    
    private ObjectFileDataFactory MakeObjectFileDataFactory(DirectoryInfo topDir)=>
        new ObjectFileDataFactory(MakeOrigamFileFactory(topDir), parentFolders);
    [Test]
    public void ReadXlms()
    {
        ConfigurationManager.SetActiveConfiguration(new OrigamSettings());
        InitFilePersistenceProvider(parentFolders, TestProjectDir);
        var origamXmlLoader =
            new OrigamXmlLoader(
                objectFileDataFactory: MakeObjectFileDataFactory(TestProjectDir),
                topDirectory: TestProjectDir,
                xmlFileDataFactory: new XmlFileDataFactory(),
                metaModelUpgradeService: new NullMetaModelUpgradeService());
        var pathFactory = new OrigamPathFactory(TestProjectDir);
        ItemTracker itemTracker = new ItemTracker(pathFactory);
        origamXmlLoader.LoadInto(itemTracker, MetaModelUpgradeMode.Ignore);
    }
    [Test]
    public void ReadPackageFile()
    {
        string fileName = Path.Combine(TestFilesDir.FullName, OrigamFile.PackageFileName);
        var objectFileDataFactory = MakeObjectFileDataFactory(TestFilesDir);
        var packageFileData =
            objectFileDataFactory.NewPackageFileData(
                XmlFileDataFactory.Create(new FileInfo(fileName)).Value);
        Guid packageId = packageFileData.PackageId;
        var expectedPackageId =
            new Guid("3e37fa44-cdb7-4804-8176-8df118a918ae");
        Assert.That(packageId, Is.EqualTo(expectedPackageId));
    }
    [Test]
    public void ReadGroupFile()
    {
        string fileName = Path.Combine(TestFilesDir.FullName,OrigamFile.GroupFileName);
        var objectFileDataFactory = MakeObjectFileDataFactory(TestFilesDir);
        var packageFileData = objectFileDataFactory.NewGroupFileData(
            XmlFileDataFactory.Create(new FileInfo(fileName)).Value);
        Guid groupIdId = packageFileData.GroupId;
        var expectedPackageId =
            new Guid("d266feb3-ff9e-4ac2-8386-517a31519d06");
        Assert.That(groupIdId, Is.EqualTo(expectedPackageId));
    }
    [Test]
    public void ReadReferenceFile()
    {
        string fileName = Path.Combine(TestFilesDir.FullName,OrigamFile.ReferenceFileName);
        var objectFileDataFactory = MakeObjectFileDataFactory(TestFilesDir);
        var xmlFileData = XmlFileDataFactory.Create(new FileInfo(fileName)).Value;
        var referenceFileData =
            objectFileDataFactory.NewReferenceFileData(xmlFileData);
        var locationAttributes = referenceFileData.ParentFolderIds;
        Assert.That(locationAttributes, Has.Count.EqualTo(2));
        
        Guid actualPackageId = locationAttributes[OrigamFile.PackageCategory];
        var expectedPackageId = new Guid("1112687f-be11-49ec-a2eb-fba58d945b3e");
        Assert.That(actualPackageId, Is.EqualTo(expectedPackageId));
        
        Guid actualGroupId = locationAttributes[OrigamFile.GroupCategory];
        var expectedGropupId = new Guid("1113687f-be11-49ec-a2eb-fba58d945b3e");
        Assert.That(actualGroupId, Is.EqualTo(expectedGropupId));
    }
   // [Test]
    public void ReadObjectFile()
    {
        ConfigurationManager.SetActiveConfiguration(new OrigamSettings());
        InitFilePersistenceProvider(parentFolders, TestFilesDir);
        var objectFileData = new ObjectFileData(
            new ParentFolders(parentFolders), 
            XmlFileDataFactory.Create(
                new FileInfo(
                    Path.Combine(TestFilesDir.FullName,"MultiEntityTest.origam"))).Value,
            MakeOrigamFileFactory(TestFilesDir));
        
        objectFileData.ParentFolderIds[OrigamFile.PackageCategory] = 
            new Guid("e002a017-75b8-4f6e-8539-576ca05d6952");
        ITrackeableFile origamFile = objectFileData.Read();
    }
    protected override string DirName => "FilePersistenceProviderTests";
    
    private void InitFilePersistenceProvider(List<string> parentFolders,
        DirectoryInfo topDir)
    {
        var ignoredFileFilter = new FileFilter( new HashSet<string> {"bin", "bak"},new FileInfo[0],new string[0]);
        var fileChangesWatchDog = new FileChangesWatchDog(
            topDir: topDir,
            ignoredFileFilter);
        var pathToIndexBin = new FileInfo(Path.Combine(topDir.FullName, "index.bin"));
        var pathFactory = new OrigamPathFactory(TestProjectDir);
        var index = new FilePersistenceIndex(pathFactory);
        var fileEventQueue = 
            new FileEventQueue(index, fileChangesWatchDog);
        var nullEventQueue = 
            new FileEventQueue(index, new NullWatchDog());
        var origamFileManager = 
            new OrigamFileManager(index, pathFactory,
                fileEventQueue);
        var origamFileFactory = new OrigamFileFactory(
            origamFileManager, parentFolders, pathFactory,
            nullEventQueue);
        var objectFileDataFactory =
            new ObjectFileDataFactory(origamFileFactory, parentFolders);          
        var trackerLoaderFactory =
            new TrackerLoaderFactory(topDir, objectFileDataFactory,
                origamFileFactory,XmlFileDataFactory, pathToIndexBin,
                true,index, new NullMetaModelUpgradeService());
        var filePersistenceProvider =
            new FilePersistenceProvider(
                topDir,
                fileEventQueue,
                ignoredFileFilter,
                trackerLoaderFactory,
                origamFileFactory,
                index,
                origamFileManager,
                true,
                runtimeModelConfig: new NullRuntimeModelConfig());
    }
}
