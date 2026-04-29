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
using System.Xml;
using NUnit.Framework;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Workbench.Services;

namespace Origam.Workbench.ServicesTests;

[TestFixture]
public class FileStorageDocumentationServiceTests
{
    [Test]
    public void ShouldAddTwoDocumentationItems()
    {
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());
        var sut = GetFileStorageDocumentationService(storageDir: WritingTestFiles);
        DocumentationComplete dataSet = GetTestDataSet(name: "inputDataSet_2Items.xml");
        sut.SaveDocumentation(documentationData: dataSet);
        XmlDocument xmlDocument = GetOutDocument();
        Assert.That(
            actual: xmlDocument.FirstChild.ChildNodes,
            expression: Has.Count.EqualTo(expected: 2)
        );
    }

    [Test]
    public void ShouldUpdateOneDocumentationItem()
    {
        var sut = GetFileStorageDocumentationService(storageDir: WritingTestFiles);
        DocumentationComplete dataSet = GetTestDataSet(name: "inputDataSet_1UpdatedItem.xml");
        sut.SaveDocumentation(documentationData: dataSet);
        XmlDocument xmlDocument = GetOutDocument();
        Assert.That(
            actual: xmlDocument.FirstChild.ChildNodes,
            expression: Has.Count.EqualTo(expected: 2)
        );
        XmlNode updatedNode = xmlDocument.FirstChild.ChildNodes[i: 0];
        Assert.That(condition: updatedNode.ChildNodes[i: 0].InnerText == "Updated text");
    }

    [Test]
    public void ShouldReadDatSet()
    {
        var sut = GetFileStorageDocumentationService(storageDir: ReadingTestFiles);
        DocumentationComplete loadedSet = sut.LoadDocumentation(
            schemaItemId: new Guid(g: "df7c2a53-c56a-426a-b748-08e656ae46db")
        );

        Assert.That(
            actual: loadedSet.Tables[index: 0].Rows,
            expression: Has.Count.EqualTo(expected: 2)
        );
    }

    [Test]
    public void ShouldLoadDocumentationOfSpecifiedType()
    {
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());
        var sut = GetFileStorageDocumentationService(storageDir: ReadingTestFiles);
        string loadedDoc = sut.GetDocumentation(
            schemaItemId: new Guid(g: "df7c2a53-c56a-426a-b748-08e656ae46db"),
            docType: DocumentationType.USER_SHORT_HELP
        );

        Assert.That(condition: loadedDoc == "Short help");
    }

    [Test]
    public void ShouldThrowBecauseCategoryNameIsWrong()
    {
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());
        var sut = GetFileStorageDocumentationService(
            storageDir: GetDirectory(dirName: "WrongCategoryName")
        );

        Assert.Throws<ArgumentException>(code: () =>
        {
            sut.LoadDocumentation(
                schemaItemId: new Guid(g: "df7c2a53-c56a-426a-b748-08e656ae46db")
            );
        });
    }

    [Test]
    public void ShouldThrowBecauseAStringCannotBeParsedToGuid()
    {
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());
        var sut = GetFileStorageDocumentationService(
            storageDir: GetDirectory(dirName: "WrongGuid")
        );

        Assert.Throws<ArgumentException>(code: () =>
        {
            sut.LoadDocumentation(
                schemaItemId: new Guid(g: "df7c2a53-c56a-426a-b748-08e656ae46db")
            );
        });
    }

    [Test]
    public void ShouldThrowBecauseANodeNameIsWrong()
    {
        ConfigurationManager.SetActiveConfiguration(configuration: new OrigamSettings());
        var sut = GetFileStorageDocumentationService(
            storageDir: GetDirectory(dirName: "WrongNodeName")
        );

        Assert.Throws<ArgumentException>(code: () =>
        {
            sut.LoadDocumentation(
                schemaItemId: new Guid(g: "df7c2a53-c56a-426a-b748-08e656ae46db")
            );
        });
    }

    private DirectoryInfo GetDirectory(string dirName)
    {
        string pathToDir = Path.Combine(path1: ReadingTestFiles.FullName, path2: dirName);
        var directoryInfo = new DirectoryInfo(path: pathToDir);
        return directoryInfo;
    }

    private FileStorageDocumentationService GetFileStorageDocumentationService(
        DirectoryInfo storageDir
    )
    {
        var mockFileProvider = new MockFileProvider(testDir: storageDir);
        var fileStorageDocumentationService = new FileStorageDocumentationService(
            persistenceService: mockFileProvider,
            fileEventQueue: new FileEventQueue(
                index: new FilePersistenceIndex(
                    pathFactory: new OrigamPathFactory(toDirectory: storageDir)
                ),
                watchDog: new NullWatchDog()
            )
        );
        return fileStorageDocumentationService;
    }

    private XmlDocument GetOutDocument()
    {
        string outFilePath = Path.Combine(path1: WritingTestFiles.FullName, path2: ".origamDoc");
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(filename: outFilePath);
        return xmlDocument;
    }

    private DocumentationComplete GetTestDataSet(string name)
    {
        string testInputPath = Path.Combine(path1: WritingTestFiles.FullName, path2: name);
        var dataSet = new DocumentationComplete();
        dataSet.ReadXml(fileName: testInputPath);
        return dataSet;
    }

    protected static readonly DirectoryInfo projectDir = new DirectoryInfo(
        path: TestContext.CurrentContext.TestDirectory
    );
    protected DirectoryInfo WritingTestFiles
    {
        get
        {
            string path = Path.Combine(
                path1: projectDir.FullName,
                path2: "FileStorageDocumentationServiceTests",
                path3: "WritingTestFiles"
            );
            Directory.CreateDirectory(path: path);
            return new DirectoryInfo(path: path);
        }
    }
    protected DirectoryInfo ReadingTestFiles
    {
        get
        {
            string path = Path.Combine(
                path1: projectDir.FullName,
                path2: "FileStorageDocumentationServiceTests",
                path3: "ReadingTestFiles"
            );
            Directory.CreateDirectory(path: path);
            return new DirectoryInfo(path: path);
        }
    }
}

internal class MockFileProvider : IFilePersistenceProvider
{
    private readonly DirectoryInfo testDir;

    public MockFileProvider(DirectoryInfo testDir)
    {
        this.testDir = testDir;
        this.LocalizationCache = new LocalizationCache();
    }

    public DirectoryInfo GetParentPackageDirectory(Guid itemId) => testDir;

    public bool Has(Guid id) => true;

    public DirectoryInfo TopDirectory { get; }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public event EventHandler<IPersistent> InstancePersisted
    {
        add { }
        remove { }
    }

    public void OnTransactionEnded(object sender)
    {
        throw new NotImplementedException();
    }

    public ICompiledModel CompiledModel { get; set; }

    public void RefreshInstance(IPersistent persistentObject)
    {
        throw new NotImplementedException();
    }

    public void RemoveFromCache(IPersistent instance)
    {
        throw new NotImplementedException();
    }

    public List<T> RetrieveList<T>(IDictionary<string, object> filter = null)
    {
        throw new NotImplementedException();
    }

    public List<T> RetrieveListByCategory<T>(string category)
    {
        throw new NotImplementedException();
    }

    public List<T> RetrieveListByPackage<T>(Guid packageId)
    {
        throw new NotImplementedException();
    }

    public T[] FullTextSearch<T>(string text)
    {
        throw new NotImplementedException();
    }

    public void Persist(IPersistent obj)
    {
        throw new NotImplementedException();
    }

    public string DebugInfo()
    {
        throw new NotImplementedException();
    }

    public void DebugShow()
    {
        throw new NotImplementedException();
    }

    public string DebugChangesInfo()
    {
        throw new NotImplementedException();
    }

    public void DebugChangesShow()
    {
        throw new NotImplementedException();
    }

    public void FlushCache()
    {
        throw new NotImplementedException();
    }

    public void DeletePackage(Guid packageId)
    {
        throw new NotImplementedException();
    }

    public bool IsInTransaction { get; }

    public void RunInTransaction(Action action)
    {
        action();
    }

    public void BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public void EndTransaction()
    {
        throw new NotImplementedException();
    }

    public void EndTransactionDontSave()
    {
        throw new NotImplementedException();
    }

    public object RetrieveValue(Guid instanceId, Type parentType, string fieldName)
    {
        throw new NotImplementedException();
    }

    public void RestrictToLoadedPackage(bool b)
    {
        throw new NotImplementedException();
    }

    public ILocalizationCache LocalizationCache { get; }

    public List<string> Files(IPersistent item)
    {
        throw new NotImplementedException();
    }

    public List<T> RetrieveListByGroup<T>(Key primaryKey)
    {
        throw new NotImplementedException();
    }

    public List<T> RetrieveListByParent<T>(
        Key primaryKey,
        string parentTableName,
        string childTableName,
        bool useCache
    )
    {
        throw new NotImplementedException();
    }

    public object RetrieveInstance(
        Type type,
        Key primaryKey,
        bool useCache,
        bool throwNotFoundException
    )
    {
        throw new NotImplementedException();
    }

    public object RetrieveInstance(Type type, Key primaryKey, bool useCache)
    {
        throw new NotImplementedException();
    }

    public object RetrieveInstance(Type type, Key primaryKey)
    {
        throw new NotImplementedException();
    }

    public List<T> GetReference<T>(Key key)
    {
        throw new NotImplementedException();
    }

    public bool IsOfType<T>(Guid id)
    {
        throw new NotImplementedException();
    }

    public T RetrieveInstance<T>(Guid instanceId)
    {
        throw new NotImplementedException();
    }

    public T RetrieveInstance<T>(Guid instanceId, bool useCache)
    {
        throw new NotImplementedException();
    }

    public T RetrieveInstance<T>(Guid instanceId, bool useCache, bool throwNotFoundException)
    {
        throw new NotImplementedException();
    }
}
