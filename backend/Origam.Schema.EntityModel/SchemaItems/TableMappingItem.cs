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
using System.Reflection;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel.Attributes;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

public enum DatabaseMappingObjectType
{
    Table = 0,
    View = 1,
}

/// <summary>
/// Maps physical table to an entity.
/// </summary>
[SchemaItemDescription("Database Entity", "icon_database-entity.png")]
[HelpTopic("Entities")]
[ClassMetaVersion("6.0.0")]
public class TableMappingItem : AbstractDataEntity
{
    public TableMappingItem() { }

    public TableMappingItem(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public TableMappingItem(Key primaryKey)
        : base(primaryKey) { }

    #region Properties
    private string _sourceTableName;

    [LengthLimit]
    [Category("Mapping")]
    [StringNotEmptyModelElementRule()]
    [Description(
        "Name of the database table name. When loading data from a database for this entity, this name will be used as the table name."
    )]
    [XmlAttribute("mappedObjectName")]
    public string MappedObjectName
    {
        get { return _sourceTableName; }
        set { _sourceTableName = value; }
    }
    private DatabaseMappingObjectType _databaseObjectType = DatabaseMappingObjectType.Table;

    [Category("Mapping"), DefaultValue(DatabaseMappingObjectType.Table)]
    [Description(
        "Type of the database object - View or Table. For views the deployment scripts will not be generated."
    )]
    [XmlAttribute("databaseObjectType")]
    public DatabaseMappingObjectType DatabaseObjectType
    {
        get { return _databaseObjectType; }
        set { _databaseObjectType = value; }
    }
    private bool _generateDeploymentScript = true;

    [Category("Mapping"), DefaultValue(true)]
    [Description(
        "Indicates if deployment scripts will be generated for this entity. If set to false, this entity will be skipped from the deployment scripts generator. This is useful e.g. if creating a duplicate entity (from the same table as another one)."
    )]
    [XmlAttribute("generateDeploymentScript")]
    public bool GenerateDeploymentScript
    {
        get { return _generateDeploymentScript; }
        set { _generateDeploymentScript = value; }
    }

    public Guid LocalizationRelationId = Guid.Empty;

    [TypeConverter(typeof(EntityRelationConverter))]
    [XmlReference("localizationRelation", "LocalizationRelationId")]
    //[RefreshProperties(RefreshProperties.Repaint)]
    public EntityRelationItem LocalizationRelation
    {
        get
        {
            return (EntityRelationItem)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(EntityRelationItem),
                    new ModelElementKey(this.LocalizationRelationId)
                );
        }
        set
        {
            this.LocalizationRelationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
        }
    }
    #endregion
    public override string Icon
    {
        get
        {
            switch (DatabaseObjectType)
            {
                case DatabaseMappingObjectType.Table:
                {
                    return "icon_database-entity.png";
                }
                case DatabaseMappingObjectType.View:
                {
                    return "54";
                }
                default:
                {
                    return "0";
                }
            }
        }
    }

    public override void OnNameChanged(string originalName)
    {
        if (MappedObjectName == "" || MappedObjectName == null || MappedObjectName == originalName)
        {
            MappedObjectName = this.Name;
        }
    }

    [Browsable(false)]
    public override List<IDataEntityColumn> EntityPrimaryKey
    {
        get
        {
            var list = new List<IDataEntityColumn>();
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey && column is FieldMappingItem)
                {
                    list.Add(column);
                }
            }
            return list;
        }
    }

    public override bool CanConvertTo(Type type)
    {
        return (type == typeof(DetachedEntity));
    }

    public override ISchemaItem ConvertTo(Type type)
    {
        if (type == typeof(DetachedEntity))
        {
            var methodInfo = typeof(Function).GetMethod("ConvertTo", BindingFlags.NonPublic);
            var genericMethodInfo = methodInfo.MakeGenericMethod(type);
            return (ISchemaItem)genericMethodInfo.Invoke(this, null);
        }
        return base.ConvertTo(type);
    }

    protected override ISchemaItem ConvertTo<T>()
    {
        var converted = RootProvider.NewItem<T>(SchemaExtensionId, Group);
        converted.PrimaryKey["Id"] = this.PrimaryKey["Id"];
        converted.Name = this.Name;
        converted.IsAbstract = this.IsAbstract;
        // we have to delete first (also from the cache)
        DeleteChildItems = false;
        PersistChildItems = false;
        IsDeleted = true;
        var persistenceProvider = ServiceManager
            .Services.GetService<IPersistenceService>()
            .SchemaProvider;
        persistenceProvider.BeginTransaction();
        Persist();
        converted.Persist();
        persistenceProvider.EndTransaction();
        return converted;
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.LocalizationRelation);

        base.GetExtraDependencies(dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.LocalizationRelation != null)
            {
                if (item.OldPrimaryKey.Equals(this.LocalizationRelation.PrimaryKey))
                {
                    this.LocalizationRelation = item as EntityRelationItem;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
}
