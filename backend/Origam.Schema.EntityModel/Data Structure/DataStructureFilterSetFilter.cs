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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for DataStructureFilterSetFilter.
/// </summary>
[SchemaItemDescription(name: "Filter", iconName: "icon_filter.png")]
[HelpTopic(topic: "Filter+Set+Filter")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "Entity")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureFilterSetFilter : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureFilterSetFilter";

    public DataStructureFilterSetFilter()
        : base() { }

    public DataStructureFilterSetFilter(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureFilterSetFilter(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataStructureEntityId;

    [TypeConverter(type: typeof(DataQueryEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Description(
        description: "An entity from this data structure on which the filter will be applied."
    )]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    public DataStructureEntity Entity
    {
        get
        {
            return (DataStructureEntity)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: this.DataStructureEntityId)
                );
        }
        set
        {
            this.DataStructureEntityId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            this.Filter = null;
            UpdateName();
        }
    }

    public Guid FilterId;

    [TypeConverter(type: typeof(DataQueryEntityFilterConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Description(description: "A filter that will be applied.")]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "filter", idField: "FilterId")]
    public EntityFilter Filter
    {
        get
        {
            return (EntityFilter)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(EntityFilter),
                    primaryKey: new ModelElementKey(id: this.FilterId)
                );
        }
        set
        {
            this.FilterId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
            UpdateName();
        }
    }
    public Guid IgnoreFilterConstantId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [Category(category: "Condition")]
    [Description(
        description: "This filter will be ignored (or not ignored if PassWhenParameterMatch = true) if a query parameter is equal to this value.\nWhen not set, it tests if the parameter is filled or not."
    )]
    [XmlReference(attributeName: "ignoreFilterConstant", idField: "IgnoreFilterConstantId")]
    public DataConstant IgnoreFilterConstant
    {
        get
        {
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(EntityFilter),
                    primaryKey: new ModelElementKey(id: this.IgnoreFilterConstantId)
                );
        }
        set
        {
            this.IgnoreFilterConstantId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    private string _ignoreFilterParameterName;

    [Category(category: "Condition")]
    [Description(
        description: "Name of the parameter that will be evaluated. If it matches with the IgnoreFilterConstant this filter will be ignored (or not ignored if PassWhenParameterMatch = true)."
    )]
    [XmlAttribute(attributeName: "ignoreFilterParameterName")]
    public string IgnoreFilterParameterName
    {
        get { return _ignoreFilterParameterName; }
        set
        {
            if (value == "")
            {
                value = null;
            }

            _ignoreFilterParameterName = value;
        }
    }
    private bool _passWhenParameterMatch = false;

    [Category(category: "Condition")]
    [DefaultValue(value: false)]
    [Description(
        description: "Applies the filter condition instead of ignoring it (revert condition)."
    )]
    [XmlAttribute(attributeName: "passWhenParameterMatch")]
    public bool PassWhenParameterMatch
    {
        get { return _passWhenParameterMatch; }
        set { _passWhenParameterMatch = value; }
    }
    private string _roles = "";

    [Category(category: "Condition"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Description(
        description: "An Application role. This filter will be used only if a user has this role assigned. If * or empty the filter will be always applied."
    )]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    #endregion
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
        if (this.Filter != null)
        {
            var references = new Dictionary<string, ParameterReference>();
            base.GetParameterReferences(parentItem: this.Filter, list: references);
            foreach (var entry in references)
            {
                string key = this.Entity.Name + "_" + (string)entry.Key;
                if (!list.ContainsKey(key: key))
                {
                    list.Add(key: key, value: entry.Value);
                }
            }
            if (this.IgnoreFilterParameterName != null)
            {
                if (!list.ContainsKey(key: IgnoreFilterParameterName))
                {
                    list.Add(key: IgnoreFilterParameterName, value: null);
                }
            }
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Entity);
        dependencies.Add(item: this.Filter);
        dependencies.Add(item: this.IgnoreFilterConstant);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.Entity.PrimaryKey))
                {
                    // store the old filter because setting an entity will reset the filter
                    EntityFilter oldFilter = this.Filter;
                    this.Entity = item as DataStructureEntity;
                    this.Filter = oldFilter;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        if (newNode is DataStructureFilterSet)
        {
            // only inside the same data structure
            return this.RootItem.Equals(obj: (newNode as ISchemaItem).RootItem);
        }

        return false;
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Private Methods
    private void UpdateName()
    {
        string entity = this.Entity == null ? "" : this.Entity.Name;
        string filter = this.Filter == null ? "" : this.Filter.Name;
        this.Name = entity + "_" + filter;
    }
    #endregion
}
