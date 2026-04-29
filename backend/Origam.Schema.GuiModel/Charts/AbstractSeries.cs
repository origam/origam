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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;

[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractSeries : AbstractSchemaItem
{
    public const string CategoryConst = "ChartSeries";

    public AbstractSeries()
        : base()
    {
        Init();
    }

    public AbstractSeries(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractSeries(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Properties
    private string _field = "";

    [Category(category: "Series")]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "field")]
    public string Field
    {
        get { return _field; }
        set { _field = value; }
    }
    private string _caption = "";

    [Category(category: "Series")]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private AggregationType _aggregation = AggregationType.Sum;

    [Category(category: "Series"), DefaultValue(value: AggregationType.Sum)]
    [XmlAttribute(attributeName: "aggregation")]
    public AggregationType Aggregation
    {
        get { return _aggregation; }
        set { _aggregation = value; }
    }
    public Guid ColorsLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [
        Category(category: "Series"),
        Description(
            description: "When defined this lookup should return a list of custom colors for the series in a fixed structure of Id, Color."
        )
    ]
    [XmlReference(attributeName: "colorsLookup", idField: "ColorsLookupId")]
    public IDataLookup ColorsLookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ColorsLookupId)
                );
        }
        set
        {
            this.ColorsLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
}
