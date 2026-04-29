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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.LookupModel;

/// <summary>
/// Summary description for AbstractDataTooltip.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class AbstractDataTooltip : AbstractSchemaItem, IComparable
{
    public const string CategoryConst = "DataTooltip";

    public AbstractDataTooltip()
        : base() { }

    public AbstractDataTooltip(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public AbstractDataTooltip(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        base.GetParameterReferences(parentItem: this.TooltipLoadMethod, list: list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.TooltipDataStructure);
        dependencies.Add(item: this.TooltipLoadMethod);
        dependencies.Add(item: this.TooltipTransformation);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        return newNode is AbstractDataLookup;
    }
    #endregion
    #region Properties
    public Guid TooltipDataStructureId;

    [Category(category: "Tooltip")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "looltipDataStructure", idField: "TooltipDataStructureId")]
    public DataStructure TooltipDataStructure
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.TooltipDataStructureId)
                    ) as DataStructure;
        }
        set
        {
            this.TooltipDataStructureId = (Guid)value.PrimaryKey[key: "Id"];
            this.TooltipLoadMethod = null;
        }
    }

    public Guid TooltipDataStructureMethodId;

    [TypeConverter(type: typeof(DataServiceDataTooltipFilterConverter))]
    [Category(category: "Tooltip")]
    [NotNullModelElementRule(conditionField: "TooltipDataStructure")]
    [XmlReference(attributeName: "tooltipLoadMethod", idField: "TooltipDataStructureMethodId")]
    public DataStructureMethod TooltipLoadMethod
    {
        get
        {
            return (DataStructureMethod)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.TooltipDataStructureMethodId)
                );
        }
        set
        {
            this.TooltipDataStructureMethodId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    public Guid TooltipTransformationId;

    [Category(category: "Tooltip")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "tooltipTransformation", idField: "TooltipTransformationId")]
    public ITransformation TooltipTransformation
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.TooltipTransformationId)
                    ) as ITransformation;
        }
        set { this.TooltipTransformationId = (Guid)value.PrimaryKey[key: "Id"]; }
    }
    private string _roles = "*";

    [Category(category: "Condition"), DefaultValue(value: "*")]
    [XmlAttribute(attributeName: "roles")]
    public virtual string Roles
    {
        get
        {
            if (_roles == null)
            {
                return "*";
            }

            return _roles;
        }
        set { _roles = value; }
    }
    private string _features;

    [Category(category: "Condition")]
    [XmlAttribute(attributeName: "features")]
    public virtual string Features
    {
        get { return _features; }
        set { _features = value; }
    }
    private int _level = 100;

    [
        Category(category: "Condition"),
        DefaultValue(value: 100),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [XmlAttribute(attributeName: "level")]
    public int Level
    {
        get { return _level; }
        set { _level = value; }
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        EntityFieldDynamicLabel compared = obj as EntityFieldDynamicLabel;
        if (compared != null)
        {
            return this.Level.CompareTo(value: compared.Level);
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}
