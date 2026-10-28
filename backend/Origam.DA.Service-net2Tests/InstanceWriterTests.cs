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
using Moq;
using NUnit.Framework;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.NamespaceMapping;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.DA.ServiceTests;

[TestFixture]
public class InstanceWriterTests
{
    [OneTimeSetUp]
    public void SetUpNamespaceMappings()
    {
        PropertyToNamespaceMapping.AddMapping(typeof(TableMappingItem));
        PropertyToNamespaceMapping.AddMapping(typeof(EntityRelationItem));
    }

    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("Entity/Field", "Entity/Field/")]
    [TestCase("Entity\\Field", "Entity/Field/")]
    public void ShouldWriteRelativeFilePathInReference(string subPath, string expectedSubPath)
    {
        var referenceId = Guid.NewGuid();
        var referencedEntity = new Mock<IDataEntity>();
        var persistentEntity = referencedEntity.As<IFilePersistent>();
        persistentEntity
            .SetupGet(entity => entity.RelativeFilePath)
            .Returns("Package\\Entities\\Target.origam");
        persistentEntity.SetupGet(entity => entity.Path).Returns(subPath);
        persistentEntity
            .SetupGet(entity => entity.PrimaryKey)
            .Returns(new ModelElementKey(referenceId));
        referencedEntity.Setup(entity => entity.ToString()).Returns("Target");
        var provider = new Mock<IPersistenceProvider>();
        provider
            .Setup(persistence =>
                persistence.RetrieveInstance(
                    typeof(ISchemaItem),
                    It.Is<Key>(key => (Guid)key["Id"] == referenceId)
                )
            )
            .Returns(referencedEntity.Object);
        var itemToWrite = new EntityRelationItem
        {
            Name = "TestRelation",
            RelatedEntityId = referenceId,
            PersistenceProvider = provider.Object,
        };
        var document = new OrigamXmlDocument();
        var writer = new InstanceWriter(new NullExternalFileManager(), document);

        writer.Write(itemToWrite);

        var reference = document.SelectSingleNode("//@*[local-name()='relatedEntity']");
        Assert.That(reference, Is.Not.Null);
        Assert.That(
            reference.Value,
            Is.EqualTo("Package/Entities/Target.origam#" + expectedSubPath + referenceId)
        );
    }

    [Test]
    public void ShouldWriteFile()
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.Name = "TestName";
        itemToWrite.PersistenceProvider = new NullPersistenceProvider();
        OrigamXmlDocument document = new OrigamXmlDocument();
        InstanceWriter sut = new InstanceWriter(new NullExternalFileManager(), document);
        sut.Write(itemToWrite);

        Assert.That(!document.IsEmpty);
    }
}
