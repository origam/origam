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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;
using Origam.TestCommon;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests
{
    [TestFixture]
    public class MetaModelUpGraderTests: AbstractFileTestClass
    {
        private XmlFileData LoadFile(string fileName)
        {
            var file = new FileInfo(Path.Combine(TestFilesDir.FullName, fileName));
            var document = new XmlDocument();
            document.Load(file.FullName);
            return new XmlFileData(document, file);
        }

        [Test]
        public void ShouldUpgradeByOneVersion()
        {
            XmlFileData xmlFileData = LoadFile("TestPersistedClassV1.0.1.origam");
            var sut = new MetaModelUpGrader(GetType().Assembly);
            bool someFilesWereUpgraded = sut.TryUpgrade(
                new List<XmlFileData>{xmlFileData});

            XmlNode classNode = xmlFileData.XmlDocument.ChildNodes[1].ChildNodes[0];
            Assert.True(classNode.Attributes["newProperty1"] != null); // assert the property eas not removed
            Assert.True(classNode.Attributes["newProperty1"].Value == "5"); // assert the property value was not changed
            Assert.True(classNode.Attributes["newProperty2"] != null);
            Assert.True(classNode.Attributes["version"] != null);
            Assert.True(classNode.Attributes["version"].Value == "1.0.2");
        }      
        
        [Test]
        public void ShouldUpgradeTwoVersions()
        {
            XmlFileData xmlFileData = LoadFile("TestPersistedClassV1.0.0.origam");
            var sut = new MetaModelUpGrader(GetType().Assembly);
            bool someFilesWereUpgraded = sut.TryUpgrade(
                new List<XmlFileData>{xmlFileData});

            XmlNode classNode = xmlFileData.XmlDocument.ChildNodes[1].ChildNodes[0];
            Assert.True(classNode.Attributes["newProperty1"] != null);
            Assert.True(classNode.Attributes["newProperty2"] != null);
            Assert.True(classNode.Attributes["version"] != null);
            Assert.True(classNode.Attributes["version"].Value == "1.0.2");
        }

        [Test]
        public void ShouldThrowIfOneOfUpgradeScriptsIsMissing()
        {
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() =>
            {
                XmlFileData xmlFileData = LoadFile("TestPersistedClass2V1.0.0.origam");
                var sut = new MetaModelUpGrader(GetType().Assembly);
                bool someFilesWereUpgraded = sut.TryUpgrade(
                    new List<XmlFileData>{xmlFileData});
            });
            Assert.That(exception.InnerException, Is.TypeOf(typeof(ClassUpgradeException)));
        } 
        
       
        

        protected override TestContext TestContext =>
            TestContext.CurrentContext;
    }
}