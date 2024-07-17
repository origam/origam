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
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using MoreLinq;
using NUnit.Framework;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.NamespaceMapping;
using Origam.DA.Service_net2Tests;
using Origam.TestCommon;
using static Origam.DA.ObjectPersistence.ExternalFileExtension;


namespace Origam.DA.ServiceTests;
public class FilePersistenceProviderTests: AbstractFileTestClass
{     
    protected override TestContext TestContext =>
        TestContext.CurrentContext;
    
    protected override string DirName => "FilePersistenceProviderTests";
   ///[Test]
    public void ShouldUpdateItemInOrigamFile()
    {
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        ClearTestDir();
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var testObj1 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            TestBool = true,
            TestInt = 5,
            TestString="test1",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        var testObj2 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("9602d3b7-60df-4f43-92d0-881ab4764d63"),
            TestBool = true,
            TestInt = 5,
            TestString="test2",
            FileParentId = new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.Persist(new List<IFilePersistent> {testObj1, testObj2});
        XmlNode rootNode = LoadXml(testObj2.RelativeFilePath).ChildNodes[1];
        var persistedTestBool =
            bool.Parse(rootNode.FirstChild.FirstChild.Attributes["ti:testBool"].Value);
        Assert.That(persistedTestBool, Is.EqualTo(true));
        
        testObj1.TestBool = false;
        persistor.PersistSingle(testObj1);
        
        rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        persistedTestBool =
            bool.Parse(rootNode.FirstChild.Attributes["ti:testBool"].Value);
         Assert.That(persistedTestBool, Is.EqualTo(false));
    }
   /// [Test]
    public void ShouldMoveItemInFileWhenParentChangedToSomethingOutSideOfTheFile()
    {
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        ClearTestDir();
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var testObj1 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            TestBool = true,
            TestInt = 5,
            TestString="test1",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        var testObj2 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("9602d3b7-60df-4f43-92d0-881ab4764d63"),
            TestBool = true,
            TestInt = 5,
            TestString="test2",
            FileParentId = new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2});
        
        XmlNode rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        Guid firstNodeIdBefore = XmlUtils.ReadId(rootNode.FirstChild).Value;
        Guid firstNodeParentIdBefore = XmlUtils.ReadParenId(rootNode.FirstChild).Value;
        Guid firstNodeChildIdBefore = XmlUtils.ReadId(rootNode.FirstChild.FirstChild).Value;
        Guid? firstNodeChildParentIdBefore = XmlUtils.ReadParenId(rootNode.FirstChild.FirstChild);
        Assert.That(firstNodeIdBefore, Is.EqualTo(testObj1.Id));
        Assert.That(firstNodeParentIdBefore, Is.EqualTo(testObj1.FileParentId));
        Assert.That(firstNodeChildIdBefore, Is.EqualTo(testObj2.Id));
        Assert.That(firstNodeChildParentIdBefore.HasValue == false);
        Assert.That(rootNode.ChildNodes, Has.Count.EqualTo(1));
        
        testObj2.FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6");
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2});
        
        rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        Guid firstNodeIdAfter = XmlUtils.ReadId(rootNode.FirstChild).Value;
        Guid firstNodeParentIdAfter = XmlUtils.ReadParenId(rootNode.FirstChild).Value;
        Guid secondNodeIdAfter = XmlUtils.ReadId(rootNode.ChildNodes[1]).Value;
        Guid secondNodeParentIdAfter = XmlUtils.ReadParenId(rootNode.ChildNodes[1]).Value;
        Assert.That(firstNodeIdAfter, Is.EqualTo(testObj1.Id));
        Assert.That(firstNodeParentIdAfter, Is.EqualTo(testObj1.FileParentId));
        Assert.That(secondNodeIdAfter, Is.EqualTo(testObj2.Id));
        Assert.That(secondNodeParentIdAfter, Is.EqualTo(testObj2.FileParentId));
        Assert.That(rootNode.ChildNodes, Has.Count.EqualTo(2));
    }
    [Test]
    public void ShouldMoveItemInFileWhenParentChangedToAnItemTheFile()
    {
        PropertyToNamespaceMapping.Init();
        PropertyToNamespaceMapping.AddMapping(typeof(TestItem));
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        ClearTestDir();
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var testObj1 = new TestItem(persistor.DefaultFolders)
        {
            Id = new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            TestBool = true,
            TestInt = 5,
            TestString = "test1",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        var testObj2 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("9602d3b7-60df-4f43-92d0-881ab4764d63"),
            TestBool = true,
            TestInt = 5,
            TestString="test2",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2});
        
        XmlNode rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        Guid firstNodeIdBefore = XmlUtils.ReadId(rootNode.FirstChild).Value;
        Guid firstNodeParentIdBefore = XmlUtils.ReadParenId(rootNode.FirstChild).Value;
        Guid secondNodeIdBefore = XmlUtils.ReadId(rootNode.ChildNodes[1]).Value;
        Guid secondNodeParentIdBefore = XmlUtils.ReadParenId(rootNode.ChildNodes[1]).Value;
        Assert.That(firstNodeIdBefore, Is.EqualTo(testObj1.Id));
        Assert.That(firstNodeParentIdBefore, Is.EqualTo(testObj1.FileParentId));
        Assert.That(secondNodeIdBefore, Is.EqualTo(testObj2.Id));
        Assert.That(secondNodeParentIdBefore, Is.EqualTo(testObj2.FileParentId));
        Assert.That(rootNode.ChildNodes, Has.Count.EqualTo(2));
 
        testObj2.FileParentId = new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322");
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2});
        
        rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        Guid firstNodeIdBeAfter = XmlUtils.ReadId(rootNode.FirstChild).Value;
        Guid firstNodeParentIdAfter = XmlUtils.ReadParenId(rootNode.FirstChild).Value;
        Guid firstNodeChildIdAfter = XmlUtils.ReadId(rootNode.FirstChild.FirstChild).Value;
        Guid? firstNodeChildParentIdAfter = XmlUtils.ReadParenId(rootNode.FirstChild.FirstChild);
        Assert.That(firstNodeIdBeAfter, Is.EqualTo(testObj1.Id));
        Assert.That(firstNodeParentIdAfter, Is.EqualTo(testObj1.FileParentId));
        Assert.That(firstNodeChildIdAfter, Is.EqualTo(testObj2.Id));
        Assert.That(firstNodeChildParentIdAfter.HasValue == false);
        Assert.That(rootNode.ChildNodes, Has.Count.EqualTo(1));
    }
    
   /// [Test]
    public void ShouldNotDeleteChildrenWhenUpdatingParentNode()
    {
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        ClearTestDir();
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var testObj1 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322"),
            TestBool = true,
            TestInt = 5,
            TestString="test1",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        var testObj2 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("9602d3b7-60df-4f43-92d0-881ab4764d63"),
            TestBool = true,
            TestInt = 5,
            TestString="test2",
            FileParentId = new Guid("147775f5-451d-4efd-8634-7f27a2cf50a6"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        var testObj3 = new TestItem(persistor.DefaultFolders)
        {
            Id= new Guid("0c0cc916-5142-41ea-b3ca-a9916736157a"),
            TestString="test3",
            FileParentId = new Guid("9602d3b7-60df-4f43-92d0-881ab4764d63"),
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2, testObj3});
        
        testObj2.FileParentId = new Guid("1d5ff1c0-0972-4e9d-a2da-cb5e62202322");
        persistor.Persist(new List<IFilePersistent>{testObj1, testObj2});
        
        XmlNode rootNode = LoadXml(testObj1.RelativeFilePath).ChildNodes[1];
        Guid firstNodeId = XmlUtils.ReadId(rootNode.FirstChild).Value;
        Guid firstNodeParentId = XmlUtils.ReadParenId(rootNode.FirstChild).Value;
        Guid firstNodeChildId = XmlUtils.ReadId(rootNode.FirstChild.FirstChild).Value;
        Guid? firstNodeChildParentId = XmlUtils.ReadParenId(rootNode.FirstChild.FirstChild);
        Guid firstNodeChildChildId = XmlUtils.ReadId(rootNode.FirstChild.FirstChild.FirstChild).Value;
        Assert.That(firstNodeId, Is.EqualTo(testObj1.Id));
        Assert.That(firstNodeParentId, Is.EqualTo(testObj1.FileParentId));
        Assert.That(firstNodeChildId, Is.EqualTo(testObj2.Id));
        Assert.That(firstNodeChildChildId, Is.EqualTo(testObj3.Id));
        Assert.That(firstNodeChildParentId.HasValue == false);
        Assert.That(rootNode.ChildNodes, Has.Count.EqualTo(1));
    }
    
    private XmlDocument LoadXml(string fileName)
    {
        var testFilePath =
            Path.Combine(TestFilesDir.FullName, fileName);
        XmlDocument document = new XmlDocument();
        document.LoadXml(File.ReadAllText(testFilePath));
        return document;
    }
   ///[Test]
    public void ShouldWriteAndReadTestItem()
    {
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        ClearTestDir();
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var origTestObject = new TestItem(persistor.DefaultFolders)
        {
            TestBool = true,
            TestInt = 5,
            TestString = "test1",
            TestXml = "&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;&lt;string&gt;&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;&lt;ComponentBindings xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; /&gt;&lt;/string&gt;",
            TestImage = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            NullImage = null,
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.PersistSingle(origTestObject);            
        TestItem retrievedTestObject = (TestItem)persistor.RetrieveSingle(typeof(TestItem),
            new Key
            {
                ["Id"] = new Guid("1111687f-be11-49ec-a2eb-fba58d945b3e")
            });
        Assert.That(origTestObject, Is.EqualTo(retrievedTestObject));
    }
    
    ///[Test]
    public void ShouldNotWriteFieldsWithDefaultValues()
    {
        ConfigurationManager.SetActiveConfiguration(GetTestOrigamSettings());
        var persistor = new PersitHelper(TestFilesDir.FullName);
        var origTestObject = new TestItem(persistor.DefaultFolders)
        {
            TestBool = true,
            TestInt = 5,
            TestString="test1",
            TestXml ="&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;&lt;string&gt;&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;&lt;ComponentBindings xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; /&gt;&lt;/string&gt;",
            TestImage = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9},
            NullImage = null,
            PersistenceProvider = persistor.GetPersistenceProvider()
        };
        persistor.PersistSingle(origTestObject); 
        
        XmlDocument doc = new XmlDocument();
        string pathToTestFile = Path.Combine(TestFilesDir.FullName, "testFile.origam");
        doc.Load(pathToTestFile);
        HashSet<string> attributeNames = new HashSet<string>(doc.ChildNodes[1].FirstChild.Attributes
            .Cast<XmlAttribute>()
            .Select(attr => attr.Name));
        
        Assert.That(attributeNames.Contains("ti:testEmptyBool"));
        Assert.That(!attributeNames.Contains("ti:testEmptyInt"));
        Assert.That(!attributeNames.Contains("ti:testEmptyString"));
    }
    private OrigamSettings GetTestOrigamSettings()
    {
        OrigamSettings settings = new OrigamSettings();
        return settings;
    }
}
[XmlRoot("test", Namespace = "http://schemas.origam.com/1.0.0/model-test")]
[ClassMetaVersion("6.0.1")]
internal class TestItem : IFilePersistent
{
    private readonly PropertyContainer<string> testXmlContainer;
    private readonly PropertyContainer<byte[]> testImageContainer;
    private readonly PropertyContainer<byte[]> nullImageContainer;
    public TestItem(Key primaryKey)
    {
        PrimaryKey = primaryKey;
        testXmlContainer = new PropertyContainer<string>(
          containerName: nameof(testXmlContainer),
          containingObject: this);
        testImageContainer = new PropertyContainer<byte[]>(
            containerName: nameof(testImageContainer),
            containingObject: this);
        nullImageContainer = new PropertyContainer<byte[]>(
            containerName: nameof(nullImageContainer),
            containingObject: this);
    }
    public TestItem(IList<string> persistorDefaultFolders)
    {
        ParentFolderIds.Add(persistorDefaultFolders[0], new Guid("1112687f-be11-49ec-a2eb-fba58d945b3e"));
        ParentFolderIds.Add(persistorDefaultFolders[1], new Guid("1113687f-be11-49ec-a2eb-fba58d945b3e"));
        PrimaryKey = new Key
        {
            ["Id"] = new Guid("1111687f-be11-49ec-a2eb-fba58d945b3e")
        };
        testXmlContainer = new PropertyContainer<string>(
         containerName: nameof(testXmlContainer),
         containingObject: this);
        testImageContainer = new PropertyContainer<byte[]>(
            containerName: nameof(testImageContainer),
            containingObject: this);
        nullImageContainer = new PropertyContainer<byte[]>(
            containerName: nameof(nullImageContainer),
            containingObject: this);
    }
    
    [XmlAttribute("testEmptyBool")]
    public bool TestEmptyBool { get; set; }
    
    [XmlAttribute("testEmptyInt")]
    public int TestEmptyInt { get; set; } 
    
    [XmlAttribute("testEmptyString")]
    public string TestEmptyString { get; set; }
    
    [XmlAttribute("testBool")]
    public bool TestBool { get; set; }
    
    [XmlAttribute("testInt")]
    public int TestInt { get; set; } 
    [XmlAttribute("testString")]
    public string TestString { get; set; }
    [XmlExternalFileReference(containerName: nameof(testXmlContainer), extension: Xml)]
    public string TestXml
    {
        get => testXmlContainer.Get();
        set => testXmlContainer.Set(value);
    }
    
    [XmlExternalFileReference(containerName: nameof(testImageContainer), extension: Png)]
    public byte[] TestImage
    {
        get => testImageContainer.Get();
        set => testImageContainer.Set(value);
    }
    
    [XmlExternalFileReference(containerName: nameof(nullImageContainer), extension: Png)]
    public byte[] NullImage
    {
        get => nullImageContainer.Get();
        set => nullImageContainer.Set(value);
    }
    
    public void Dispose()
    {
        
    }
    public event EventHandler Changed
    {
        add { }
        remove { }
    }
    public event EventHandler Deleted
    {
        add { }
        remove { }
    }
    public IPersistenceProvider PersistenceProvider { get; set; }
    public Key PrimaryKey { get; set; }
    public Guid Id
    {
        get => (Guid) PrimaryKey["Id"];
        set =>  PrimaryKey =  new Key
        {
            ["Id"] = value
        };
    }
    public void Persist()
    {
    }
    public void Refresh()
    {
    }
    public IPersistent GetFreshItem()
    {
        throw new NotImplementedException();
    }
    public bool IsDeleted { get; set; }
    public bool IsPersisted { get; set; }
    public bool UseObjectCache { get; set; }
    public string RelativeFilePath => "testFile.origam";
    public Guid FileParentId { get; set; }
    public bool IsFolder { get; }
    public IDictionary<string, Guid> ParentFolderIds { get; } = new Dictionary<string, Guid>();
    public string Path { get; }
    public bool IsFileRootElement => FileParentId == Guid.Empty;
    public List<string> Files => throw new NotImplementedException();
    public bool? HasGitChange { get; set; }
    protected bool Equals(TestItem other) =>
        TestBool == other.TestBool &&
        TestInt == other.TestInt &&
        string.Equals(TestString, other.TestString) &&
        TestEmptyBool == other.TestEmptyBool &&
        TestEmptyInt == other.TestEmptyInt &&
        string.Equals(TestEmptyString, other.TestEmptyString) &&
        string.Equals(TestXml, other.TestXml) &&
        ByteArraysEqual(TestImage, other.TestImage);
    private bool ByteArraysEqual(byte[] first, byte[] second)
    {
        if (first.Length != second.Length) return false;
        return !first
            .Where((byte1, i) => byte1 != second[i])
            .Any();
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TestItem) obj);
    }
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
