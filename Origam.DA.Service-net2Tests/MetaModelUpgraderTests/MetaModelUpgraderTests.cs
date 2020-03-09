#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests
{
    [TestFixture]
    public class MetaModelUpGraderTests: MetaModelUpGradeTestBase
    {
        [Test]
        public void ShouldUpgradeByOneVersion()
        {
            XFileData xFileData = LoadFile("TestPersistedClassV6.0.1.origam");
            var sut = new MetaModelUpGrader(GetType().Assembly, new NullFileWriter());
            bool someFilesWereUpgraded = sut.TryUpgrade(
                new List<XFileData>{xFileData});
            
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
        public void ShouldRemoveDeadClass()
        {
            XFileData xFileData = LoadFile("TestDeadClassV6.0.1.origam");
            var sut = new MetaModelUpGrader(GetType().Assembly, new NullFileWriter());
            sut.TryUpgrade(new List<XFileData>{xFileData});
            
            bool classNamespacesPresent = xFileData.Document.Namespaces
                .Any(nameSpace => nameSpace.FullTypeName.Contains("TestDeadClass") ||
                                  nameSpace.FullTypeName.Contains("TestBaseClass"));
            Assert.False(classNamespacesPresent);
            Assert.That(xFileData.Document.ClassNodes.ToList(), Has.Count.EqualTo(0));
        }      
        
        [Test]
        public void ShouldUpgradeTwoVersions()
        {
            XFileData xFileData = LoadFile("TestPersistedClassV5.0.0.origam");
            var sut = new MetaModelUpGrader(GetType().Assembly, new NullFileWriter());
            sut.TryUpgrade(new List<XFileData>{xFileData});
            
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
        
        // [Test]
        // public void ShouldRemoveDeadBaseClass()
        // {
        //     XmlFileData xmlFileData = LoadFile("TestPersistedClassV6.0.2_WithDeadBaseClass.origam");
        //     var sut = new MetaModelUpGrader(GetType().Assembly, new NullFileWriter());
        //     bool someFilesWereUpgraded = sut.TryUpgrade(
        //         new List<XmlFileData>{xmlFileData});
        //
        //     XmlElement fileElement = xmlFileData.XmlDocument.FileElement;
        //     XmlNode classNode = fileElement.ChildNodes[0];
        //     Assert.True(classNode.Attributes["tpc:newProperty1"] != null); 
        //     Assert.True(classNode.Attributes["tpc:newProperty1"].Value == "5"); 
        //     Assert.True(classNode.Attributes["tpc:newProperty2"] != null);
        //     Assert.That(classNode.Attributes["tbc:TestBaseClassProperty"] != null);
        //     Assert.True(classNode.Attributes["tbc:TestBaseClassProperty"].Value == "5");         
        //     Assert.True(classNode.Attributes["tpc:name"] != null);
        //     Assert.True(classNode.Attributes["tpc:name"].Value == "test v0");
        //     Assert.That(classNode.Attributes["tdbc:deadClassProperty"]?.Value, Is.Null);
        //     Assert.That(fileElement.Attributes["xmlns:tpc"]?.Value, Is.EqualTo("http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2")); 
        //     Assert.That(fileElement.Attributes["xmlns:tbc"]?.Value, Is.EqualTo("http://schemas.origam.com/Origam.DA.ServiceTests.TestBaseClass/6.0.1"));
        //     Assert.That(fileElement.Attributes["xmlns:tdc"]?.Value, Is.Null);
        // }   
        //
        // [Test]
        // public void ShouldUpgradeRenamedClass()
        // {
        //     XmlFileData xmlFileData = LoadFile("TestRenamedClassV6.0.0.origam");
        //     var sut = new MetaModelUpGrader(GetType().Assembly, new NullFileWriter());
        //     bool someFilesWereUpgraded = sut.TryUpgrade(
        //         new List<XmlFileData>{xmlFileData});
        //
        //     XmlElement fileElement = xmlFileData.XmlDocument.FileElement;
        //     XmlNode classNode = fileElement.ChildNodes[0];
        //     Assert.That(classNode.LocalName, Is.EqualTo("TestRenamedClass"));
        //     Assert.That(classNode.Attributes["tonc:name"], Is.Null);
        //     Assert.That(fileElement.Attributes["xmlns:tonc"]?.Value, Is.Null);
        //     Assert.That(classNode.Attributes["trc:name"]?.Value, Is.EqualTo("test"));
        //     Assert.That(fileElement.Attributes["xmlns:trc"]?.Value, Is.EqualTo("http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1"));
        // }
        //
        // [Test]
        // public void ShouldThrowIfOneOfUpgradeScriptsIsMissing()
        // {
        //     Assert.Throws<ClassUpgradeException>(() =>
        //     {
        //         XmlFileData xmlFileData = LoadFile("TestPersistedClass2V6.0.0.origam");
        //         var sut = new MetaModelUpGrader(GetType().Assembly,  new NullFileWriter());
        //         bool someFilesWereUpgraded = sut.TryUpgrade(
        //             new List<XmlFileData>{xmlFileData});
        //     });
        // }  
        //
        // [Test]
        // public void ShouldThrowIfAttributeIsAlreadyPresent()
        // {
        //     Assert.Throws<ClassUpgradeException>(() =>
        //     {
        //         XmlFileData xmlFileData = LoadFile("TestPersistedClassV6.0.1_WrongVersion.origam");
        //         var sut = new MetaModelUpGrader(GetType().Assembly,  new NullFileWriter());
        //         bool someFilesWereUpgraded = sut.TryUpgrade(
        //             new List<XmlFileData>{xmlFileData});
        //     });
        // }
    }
}