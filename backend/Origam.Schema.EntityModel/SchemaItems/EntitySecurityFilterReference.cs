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
/// Summary description for EntitySecurityFilterReference.
/// </summary>
[SchemaItemDescription(
    "Row Level Security Filter",
    "Row Level Security",
    "icon_row-level-security-filter.png"
)]
[HelpTopic("Row+Level+Security+Filters")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Filter")]
[ClassMetaVersion("6.0.0")]
public class EntitySecurityFilterReference : AbstractSchemaItem
{
    public const string CategoryConst = "EntitySecurityFilterReference";

    public EntitySecurityFilterReference()
        : base() { }

    public EntitySecurityFilterReference(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public EntitySecurityFilterReference(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

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
            base.GetParameterReferences(Filter, list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.Filter);
        base.GetExtraDependencies(dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(this.Filter.PrimaryKey))
                {
                    this.Filter = item as EntityFilter;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    private string _roles;

    [Category("Security")]
    [XmlAttribute("roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }

    public Guid FilterId;

    [TypeConverter(typeof(EntityFilterConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRuleAttribute()]
    [Category("Security")]
    [XmlReference("filter", "FilterId")]
    public EntityFilter Filter
    {
        get
        {
            return (EntityFilter)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(this.FilterId)
                );
        }
        set
        {
            if (value == null)
            {
                this.FilterId = Guid.Empty;
                this.Name = "";
            }
            else
            {
                this.FilterId = (Guid)value.PrimaryKey["Id"];
                this.Name = this.Filter.Name;
            }
        }
    }
    #endregion
}
