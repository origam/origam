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

using System.IO;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.Service.NamespaceMapping;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests;
[TestFixture]
public class MetaModelUpgraderTests: ClassUpgradeTestBase
{
    protected override string DirName { get; } = "MetaModelUpgraderTests";
    
    [Test]
    public void ShouldUpgradeByOneVersion()
    {
        XFileData xFileData = LoadFile("TestPersistedClassV6.0.1.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        XNamespace tbcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1";
        XElement fileElement = xFileData.Document.FileElement;
        XElement classNode = xFileData.Document.ClassNodes.First();
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")) != null); // assert the property was not removed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")).Value == "5"); // assert the property value was not changed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty2")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")).Value == "5");         
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")) != null);
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")).Value == "test v0");
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tbc"))?.Value, Is.EqualTo(tbcNamespace.ToString()));
    }             
    
    [Test]
    public void ShouldReplaceClass()
    {
        XFileData xFileData = LoadFile("TestDeadClassV6.0.1.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        
        bool classNamespacesPresent = xFileData.Document.Namespaces
            .Any(nameSpace => nameSpace.FullTypeName.Contains("TestDeadClass") ||
                              nameSpace.FullTypeName.Contains("TestBaseClass"));
        Assert.False(classNamespacesPresent);
        
        XElement classNode = xFileData.Document.ClassNodes.First();
        var fileElement = xFileData.Document.FileElement;
        Assert.That(classNode.Name.LocalName, Is.EqualTo("TestPersistedClass"));
        Assert.That(classNode.Attribute(tpcNamespace.GetName("name"))?.Value, Is.EqualTo("test v0")); // "test v0" was value of the dead class's name property
        Assert.That( fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
    }  
    
    [Test]
    public void ShouldReplaceClassOfVersion5()
    {
        XFileData xFileData = LoadFile("TestDeadClassV5.0.0.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        
        bool classNamespacesPresent = xFileData.Document.Namespaces
            .Any(nameSpace => nameSpace.FullTypeName.Contains("TestDeadClass") ||
                              nameSpace.FullTypeName.Contains("TestBaseClass"));
        Assert.False(classNamespacesPresent);
        
        XElement classNode = xFileData.Document.ClassNodes.First();
        var fileElement = xFileData.Document.FileElement;
        Assert.That(classNode.Name.LocalName, Is.EqualTo("TestPersistedClass"));
        Assert.That(classNode.Attribute(tpcNamespace.GetName("name"))?.Value, Is.EqualTo("test v0")); // "test v0" was value of the dead class's name property
        Assert.That( fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
    }      
    
    [Test]
    public void ShouldUpgradeTwoVersions()
    {
        XFileData xFileData = LoadFile("TestPersistedClassV5.0.0.origam");
        PropertyToNamespaceMapping.Init();
        PropertyToNamespaceMapping.AddMapping(typeof(TestBaseClass));
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        XNamespace tbcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1";
    
        XElement fileElement = xFileData.Document.FileElement;
        XElement classNode = xFileData.Document.ClassNodes.First();
        
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")) != null); // assert the property was not removed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")).Value == ""); // assert the property value was not changed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty2")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")).Value == "");         
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")) != null);
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")).Value == "test v0");
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tbc"))?.Value, Is.EqualTo(tbcNamespace.ToString()));
    }        
    
    [Test]
    public void ShouldRemoveDeadBaseClass()
    {
        XFileData xFileData = LoadFile("TestPersistedClassV6.0.2_WithDeadBaseClass.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        XNamespace tbcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1";
        XNamespace deadNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestDeadBaseClass/6.0.1";
        XElement fileElement = xFileData.Document.FileElement;
        XElement classNode = xFileData.Document.ClassNodes.First();
        
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")) != null); // assert the property was not removed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty1")).Value == "5"); // assert the property value was not changed
        Assert.True(classNode.Attribute(tpcNamespace.GetName("newProperty2")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")) != null);
        Assert.True(classNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")).Value == "5");         
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")) != null);
        Assert.True(classNode.Attribute(tpcNamespace.GetName("name")).Value == "test v0");
        Assert.That(classNode.Attribute(deadNamespace.GetName("deadClassProperty")), Is.Null);
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tbc"))?.Value, Is.EqualTo(tbcNamespace.ToString()));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tdbc"))?.Value, Is.Null);
    }          
    
    [Test]
    public void ShouldRemoveDeadBaseClassAndDeleteEmptyFile()
    {
        Mock<IFileWriter> mockWriter = new Mock<IFileWriter>();
        mockWriter.Setup(mock => mock.Write(It.IsAny<FileInfo>(), It.IsAny<string>()));
        mockWriter.Setup(mock => mock.Delete(It.IsAny<FileInfo>()));
        
        XFileData xFileData = LoadFile("TestDeadClass2V6.0.1.origam");
        var sut = new MetaModelAnalyzer(mockWriter.Object, new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        XNamespace tbcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1";
        XNamespace deadNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestDeadBaseClass/6.0.1";
        XElement fileElement = xFileData.Document.FileElement;
        Assert.That(fileElement.Elements().ToList(), Has.Count.EqualTo(0));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tdbc"))?.Value, Is.Null);
        mockWriter.Verify(mock => mock.Delete(xFileData.File), Times.Once());
    }   
    
    [Test]
    public void ShouldUpgradeRenamedClass()
    {
        XFileData xFileData = LoadFile("TestRenamedClassV6.0.0.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
    
        XNamespace toncNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestOldNameClass/6.0.0";
        XNamespace trcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1";
        
        XElement fileElement = xFileData.Document.FileElement;
        XElement classNode = xFileData.Document.ClassNodes.First();
        
        Assert.That(classNode.Name.LocalName, Is.EqualTo("TestRenamedClass"));
        Assert.That(classNode.Attribute(toncNamespace.GetName("name")), Is.Null);
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tonc"))?.Value, Is.Null);
        Assert.That(classNode.Attribute(trcNamespace.GetName("name"))?.Value, Is.EqualTo("test"));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("trc"))?.Value, Is.EqualTo(trcNamespace.ToString()));
    }
    
    [Test]
    public void ShouldUpgradeRenamedClassWithChild()
    {
        XFileData xFileData = LoadFile("TestRenamedClassV6.0.0_WithChild.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);
        XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
        XNamespace tbcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1";
        XNamespace toncNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestOldNameClass/6.0.0";
        XNamespace trcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1";
        
        XElement fileElement = xFileData.Document.FileElement;
        XElement renamedClassNode = xFileData.Document.ClassNodes.First();
        
        Assert.That(renamedClassNode.Name.LocalName, Is.EqualTo("TestRenamedClass"));
        Assert.That(renamedClassNode.Attribute(toncNamespace.GetName("name")), Is.Null);
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tonc"))?.Value, Is.Null);
        Assert.That(renamedClassNode.Attribute(trcNamespace.GetName("name"))?.Value, Is.EqualTo("test"));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("trc"))?.Value, Is.EqualTo(trcNamespace.ToString()));
        
        XElement childClassNode = renamedClassNode.Elements().First();
        Assert.True(childClassNode.Attribute(tpcNamespace.GetName("newProperty1")) != null); // assert the property was not removed
        Assert.True(childClassNode.Attribute(tpcNamespace.GetName("newProperty1")).Value == "5"); // assert the property value was not changed
        Assert.True(childClassNode.Attribute(tpcNamespace.GetName("newProperty2")) != null);
        Assert.True(childClassNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")) != null);
        Assert.True(childClassNode.Attribute(tbcNamespace.GetName("TestBaseClassProperty")).Value == "5");         
        Assert.True(childClassNode.Attribute(tpcNamespace.GetName("name")) != null);
        Assert.True(childClassNode.Attribute(tpcNamespace.GetName("name")).Value == "test v0");
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(tpcNamespace.ToString()));
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tbc"))?.Value, Is.EqualTo(tbcNamespace.ToString()));
    }
    [Test]
    public void ShouldThrowIfOneOfUpgradeScriptsIsMissing()
    {
        Assert.Throws<ClassUpgradeException>(() =>
        {
            XFileData xFileData = LoadFile("TestPersistedClass2V6.0.0.origam");
            var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
            sut.TryUpgrade(xFileData);
        });
    }  
    
    [Test]
    public void ShouldThrowIfNoUpgradeScriptExists()
    {
        Assert.Throws<ClassUpgradeException>(() =>
        {
            XFileData xFileData = LoadFile("TestPersistedClass3V6.0.0.origam");
            var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
            sut.TryUpgrade(xFileData);
        });
    }  
            
    [Test]
    public void ShouldThrowIfUpgradeScriptToLastVersionIsNotFound()
    {
        Assert.Throws<ClassUpgradeException>(() =>
        {
            XFileData xFileData = LoadFile("TestPersistedClass4V6.0.0.origam");
            var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
            sut.TryUpgrade(xFileData);
        });
    }  
    
    [Test]
    public void ShouldThrowIfAttributeIsAlreadyPresent()
    {
        Assert.Throws<ClassUpgradeException>(() =>
        {
            XFileData xFileData = LoadFile("TestPersistedClassV6.0.1_WrongVersion.origam");
            var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
            sut.TryUpgrade(xFileData);
        });
    }

    [Test]
    public void ShouldUpgradeTableMappingItem()
    {
        XFileData xFileData = LoadFile("TableMappingItemV6.0.0.origam");
        var sut = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader(GetType().Assembly));
        sut.TryUpgrade(xFileData);

        XNamespace newNs = "http://schemas.origam.com/Origam.Schema.EntityModel.TableMapping/6.1.0";
        XElement classNode = xFileData.Document.ClassNodes.First();
        Assert.That(classNode.Name.Namespace, Is.EqualTo(newNs));
        Assert.That(classNode.Name.LocalName, Is.EqualTo("DataEntity"));
    }
}
