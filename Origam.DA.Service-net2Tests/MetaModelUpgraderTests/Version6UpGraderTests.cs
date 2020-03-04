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
using System.Xml;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests
{
    [TestFixture]
    public class Version6UpGraderTests: MetaModelUpGradeTestBase
    {
        [Test]
        public void ShouldUpgradeToVersion6()
        {
            XmlFileData xmlFileData = LoadFile("TestPersistedClassV5.0.0.origam");
            var sut = new Version6UpGrader(xmlFileData.XmlDocument);
            sut.Run();

            XmlElement fileElement = xmlFileData.XmlDocument.FileElement;
            Assert.That(fileElement.Attributes, Has.Count.EqualTo(2)); 
            Assert.That(fileElement.Attributes["xmlns:tpc"]?.Value, Is.EqualTo("http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.0")); 
            Assert.That(fileElement.Attributes["xmlns:x"]?.Value, Is.EqualTo("http://schemas.origam.com/1.0.0/model-persistence")); 
            XmlNode classNode = fileElement.ChildNodes[0];
            Assert.That(classNode.Prefix, Is.EqualTo("tpc")); 
            Assert.That(classNode.Attributes["name"]?.Value, Is.Not.Null);
            Assert.That(classNode.Attributes["name"]?.NamespaceURI, Is.EqualTo("http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.0"));
        }             
    }
}