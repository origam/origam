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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntityRelationFilter.
/// </summary>
[SchemaItemDescription("Filter Reference", 5)]
[HelpTopic("Relationship+Filter")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Filter")]
[ClassMetaVersion("6.0.0")]
public class EntityRelationFilter : AbstractSchemaItem
{
    public const string CategoryConst = "EntityRelationFilter";

    public EntityRelationFilter()
        : base() { }

    public EntityRelationFilter(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public EntityRelationFilter(Key primaryKey)
        : base(primaryKey) { }

    #region Properties
    public Guid FilterId;

    [TypeConverter(typeof(RelationFilterConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRuleAttribute()]
    [XmlReference("filter", "FilterId")]
    public EntityFilter Filter
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.FilterId;
            return (EntityFilter)
                this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
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
    #region Overriden ISchemaItem Members
    public override string Icon
    {
        get { return "5"; }
    }

    public override string ItemType
    {
        get { return EntityRelationFilter.CategoryConst; }
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
}
