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
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests
{
    [TestFixture]
    public class Version6UpGraderTests: ClassUpgradeTestBase
    {
        [Test]
        public void ShouldUpgradeToVersion6()
        {
            XFileData xFileData = LoadFile("TestPersistedClassV5.0.0.origam");
            var sut = new Version6UpGrader(xFileData.Document);
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
    }
}