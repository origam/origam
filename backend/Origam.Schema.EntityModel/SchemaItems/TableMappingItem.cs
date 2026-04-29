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
[SchemaItemDescription(name: "Database Entity", iconName: "icon_database-entity.png")]
[HelpTopic(topic: "Entities")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class TableMappingItem : AbstractDataEntity
{
    public TableMappingItem() { }

    public TableMappingItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public TableMappingItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    private string _sourceTableName;

    [IndexNameLengthLimit]
    [Category(category: "(Schema Item)")]
    [StringNotEmptyModelElementRule]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlAttribute(attributeName: "name")]
    [Description(
        description: "Name of the model element. The name is mainly used for giving the model elements a human readable name. In some cases the name is an identificator of the model element (e.g. for defining XML structures or for requesting constants from XSLT tranformations)."
    )]
    public override string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    [LengthLimit]
    [Category(category: "Mapping")]
    [StringNotEmptyModelElementRule()]
    [Description(
        description: "Name of the database table name. When loading data from a database for this entity, this name will be used as the table name."
    )]
    [XmlAttribute(attributeName: "mappedObjectName")]
    public string MappedObjectName
    {
        get { return _sourceTableName; }
        set { _sourceTableName = value; }
    }
    private DatabaseMappingObjectType _databaseObjectType = DatabaseMappingObjectType.Table;

    [Category(category: "Mapping"), DefaultValue(value: DatabaseMappingObjectType.Table)]
    [Description(
        description: "Type of the database object - View or Table. For views the deployment scripts will not be generated."
    )]
    [XmlAttribute(attributeName: "databaseObjectType")]
    public DatabaseMappingObjectType DatabaseObjectType
    {
        get { return _databaseObjectType; }
        set { _databaseObjectType = value; }
    }
    private bool _generateDeploymentScript = true;

    [Category(category: "Mapping"), DefaultValue(value: true)]
    [Description(
        description: "Indicates if deployment scripts will be generated for this entity. If set to false, this entity will be skipped from the deployment scripts generator. This is useful e.g. if creating a duplicate entity (from the same table as another one)."
    )]
    [XmlAttribute(attributeName: "generateDeploymentScript")]
    public bool GenerateDeploymentScript
    {
        get { return _generateDeploymentScript; }
        set { _generateDeploymentScript = value; }
    }

    public Guid LocalizationRelationId = Guid.Empty;

    [TypeConverter(type: typeof(EntityRelationConverter))]
    [XmlReference(attributeName: "localizationRelation", idField: "LocalizationRelationId")]
    //[RefreshProperties(RefreshProperties.Repaint)]
    public EntityRelationItem LocalizationRelation
    {
        get
        {
            return (EntityRelationItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(EntityRelationItem),
                    primaryKey: new ModelElementKey(id: this.LocalizationRelationId)
                );
        }
        set
        {
            this.LocalizationRelationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
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

    [Browsable(browsable: false)]
    public override List<IDataEntityColumn> EntityPrimaryKey
    {
        get
        {
            var list = new List<IDataEntityColumn>();
            foreach (IDataEntityColumn column in this.EntityColumns)
            {
                if (column.IsPrimaryKey && column is FieldMappingItem)
                {
                    list.Add(item: column);
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
            var methodInfo = typeof(Function).GetMethod(
                name: "ConvertTo",
                bindingAttr: BindingFlags.NonPublic
            );
            var genericMethodInfo = methodInfo.MakeGenericMethod(typeArguments: type);
            return (ISchemaItem)genericMethodInfo.Invoke(obj: this, parameters: null);
        }
        return base.ConvertTo(type: type);
    }

    protected override ISchemaItem ConvertTo<T>()
    {
        var converted = RootProvider.NewItem<T>(schemaExtensionId: SchemaExtensionId, group: Group);
        converted.PrimaryKey[key: "Id"] = this.PrimaryKey[key: "Id"];
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
        dependencies.Add(item: this.LocalizationRelation);

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.LocalizationRelation != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.LocalizationRelation.PrimaryKey))
                {
                    this.LocalizationRelation = item as EntityRelationItem;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
}
