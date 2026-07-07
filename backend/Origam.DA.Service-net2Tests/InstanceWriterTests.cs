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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.DA.ServiceTests;

[TestFixture]
public class InstanceWriterTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        PropertyToNamespaceMapping.Init();
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

    [Test]
    public void ShouldOmitFalseXmlAttribute()
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.GenerateDeploymentScript = false;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Not.Contain("generateDeploymentScript="));
    }

    [Test]
    public void ShouldWriteTrueXmlAttribute()
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.GenerateDeploymentScript = true;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain("generateDeploymentScript=\"true\""));
    }

    [Test]
    public void ShouldOmitZeroIntegerXmlAttribute()
    {
        var itemToWrite = new DataEntityIndexField();
        itemToWrite.OrdinalPosition = 0;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Not.Contain("ordinalPosition="));
    }

    [Test]
    public void ShouldWriteNonZeroIntegerXmlAttribute()
    {
        var itemToWrite = new DataEntityIndexField();
        itemToWrite.OrdinalPosition = 5;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain("ordinalPosition=\"5\""));
    }

    [Test]
    public void ShouldOmitEmptyGuidXmlAttribute()
    {
        var itemToWrite = new ControlSetItem();
        itemToWrite.MultiColumnAdapterFieldCondition = Guid.Empty;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Not.Contain("multiColumnAdapterFieldCondition="));
    }

    [Test]
    public void ShouldWriteNonEmptyGuidXmlAttribute()
    {
        var id = Guid.NewGuid();
        var itemToWrite = new ControlSetItem();
        itemToWrite.MultiColumnAdapterFieldCondition = id;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain($"multiColumnAdapterFieldCondition=\"{id}\""));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ShouldOmitEmptyStringXmlAttribute(string value)
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.MappedObjectName = value;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Not.Contain("mappedObjectName="));
    }

    [Test]
    public void ShouldWriteNonEmptyStringXmlAttribute()
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.MappedObjectName = "TestTable";

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain("mappedObjectName=\"TestTable\""));
    }

    [Test]
    public void ShouldWriteEnumXmlAttribute()
    {
        var itemToWrite = new TableMappingItem();
        itemToWrite.DatabaseObjectType = DatabaseMappingObjectType.Table;

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain("databaseObjectType=\"Table\""));
    }

    private static readonly object[] DefaultPropertyValueItemValueAttributeCases =
    {
        new object[] { ControlPropertyValueType.Integer, 0 },
        new object[] { ControlPropertyValueType.Boolean, false },
        new object[] { ControlPropertyValueType.String, "" },
        new object[] { ControlPropertyValueType.UniqueIdentifier, Guid.Empty },
    };

    [TestCaseSource(nameof(DefaultPropertyValueItemValueAttributeCases))]
    public void ShouldOmitDefaultPropertyValueItemValueAttribute(
        ControlPropertyValueType propertyType,
        object value
    )
    {
        PropertyValueItem itemToWrite = CreatePropertyValueItem(propertyType, value);

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Not.Contain("value="));
    }

    private static readonly object[] NonDefaultPropertyValueItemValueAttributeCases =
    {
        new object[] { ControlPropertyValueType.Integer, 1 },
        new object[] { ControlPropertyValueType.Boolean, true },
        new object[] { ControlPropertyValueType.String, "0" },
        new object[] { ControlPropertyValueType.UniqueIdentifier, Guid.NewGuid() },
    };

    [TestCaseSource(nameof(NonDefaultPropertyValueItemValueAttributeCases))]
    public void ShouldWriteNonDefaultPropertyValueItemValueAttribute(
        ControlPropertyValueType propertyType,
        object value
    )
    {
        PropertyValueItem itemToWrite = CreatePropertyValueItem(propertyType, value);

        string xml = WriteToXml(itemToWrite);

        Assert.That(xml, Does.Contain("value="));
    }

    private PropertyValueItem CreatePropertyValueItem(
        ControlPropertyValueType propertyType,
        object value
    )
    {
        var propertyItem = new ControlPropertyItem { PropertyType = propertyType };
        var persistenceProvider = new Mock<IPersistenceProvider>();
        persistenceProvider
            .Setup(provider =>
                provider.RetrieveInstance(typeof(ControlPropertyItem), It.IsAny<Key>(), true, false)
            )
            .Returns(propertyItem);
        var valueItem = new PropertyValueItem { PersistenceProvider = persistenceProvider.Object };
        valueItem.ControlPropertyItem = propertyItem;
        valueItem.SetValue(value);
        return valueItem;
    }

    private string WriteToXml(IFilePersistent itemToWrite)
    {
        itemToWrite.PersistenceProvider = new NullPersistenceProvider();
        OrigamXmlDocument document = new OrigamXmlDocument();
        InstanceWriter sut = new InstanceWriter(new NullExternalFileManager(), document);
        sut.Write(itemToWrite);
        return document.OuterXml;
    }
}
