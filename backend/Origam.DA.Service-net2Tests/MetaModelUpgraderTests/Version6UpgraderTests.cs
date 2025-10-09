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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests;
[TestFixture]
public class Version6UpgraderTests: ClassUpgradeTestBase
{
    protected override string DirName { get; } = "MetaModelUpgraderTests";
    
    [Test]
    public void ShouldUpgradeToVersion6()
    {
        XFileData xFileData = LoadFile("TestPersistedClassV5.0.0.origam");
        var sut = new Version6Upgrader(new ScriptContainerLocator(GetType().Assembly), xFileData.Document);
        sut.Run();
        XElement fileElement = xFileData.Document.FileElement;
        Assert.That(fileElement.Attributes().ToList(), Has.Count.EqualTo(2));
        XNamespace testClassNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.0";
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("tpc"))?.Value, Is.EqualTo(testClassNamespace.ToString()));
        XNamespace persistenceNamespace = "http://schemas.origam.com/model-persistence/1.0.0";
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("x"))?.Value, Is.EqualTo(persistenceNamespace.ToString())); 
        XElement classNode = fileElement.Descendants().First();
        Assert.That(classNode.GetPrefixOfNamespace(classNode.Name.Namespace), Is.EqualTo("tpc")); 
        Assert.That(classNode.Attribute(persistenceNamespace.GetName("id"))?.Value, Is.EqualTo("0000-0000")); 
        Assert.That(classNode.Attribute(persistenceNamespace.GetName("id"))?.Name.Namespace, Is.EqualTo(persistenceNamespace)); 
        Assert.That(classNode.Attribute(testClassNamespace.GetName("name"))?.Value, Is.Not.Null);
        Assert.That(classNode.Attribute(testClassNamespace.GetName("name"))?.Name.Namespace, Is.EqualTo(testClassNamespace));
    }            
    [Test]
    public void ShouldUpgradeGroupReferenceFileToVersion6()
    {
        XFileData xFileData = LoadFile("TestOrigamGroupReferenceV5.0.0.origam");
        var sut = new Version6Upgrader(new ScriptContainerLocator(GetType().Assembly), xFileData.Document);
        sut.Run();
        
        XNamespace persistenceNamespace = "http://schemas.origam.com/model-persistence/1.0.0";
        XElement fileElement = xFileData.Document.FileElement;
        Assert.That(fileElement.Attributes().ToList(), Has.Count.EqualTo(1)); 
        Assert.That(fileElement.Attribute(XNamespace.Xmlns.GetName("x"))?.Value, Is.EqualTo(persistenceNamespace.ToString())); 
        
        XElement firstGroupReferenceNode = fileElement.Descendants().First();
        Assert.That(firstGroupReferenceNode.Name.Namespace, Is.EqualTo(persistenceNamespace));
        Assert.That(firstGroupReferenceNode.GetPrefixOfNamespace(firstGroupReferenceNode.Name.Namespace), Is.EqualTo("x"));
        Assert.That(firstGroupReferenceNode.Attribute(persistenceNamespace.GetName("type"))?.Value, Is.EqualTo("package")); 
        Assert.That(firstGroupReferenceNode.Attribute(persistenceNamespace.GetName("refId"))?.Value, Is.EqualTo("951f2cda-2867-4b99-8824-071fa8749ead"));        
        
        XElement secondGroupReferenceNode = fileElement.Descendants().Skip(1).First();
        Assert.That(secondGroupReferenceNode.Name.Namespace, Is.EqualTo(persistenceNamespace));
        Assert.That(secondGroupReferenceNode.GetPrefixOfNamespace(secondGroupReferenceNode.Name.Namespace), Is.EqualTo("x"));
        Assert.That(secondGroupReferenceNode.Attribute(persistenceNamespace.GetName("type"))?.Value, Is.EqualTo("group")); 
        Assert.That(secondGroupReferenceNode.Attribute(persistenceNamespace.GetName("refId"))?.Value, Is.EqualTo("d266feb3-ff9e-4ac2-8386-517a31519d06")); 
    }             
}
