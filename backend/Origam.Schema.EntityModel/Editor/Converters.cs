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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

public class StringItemConverter : TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        StringSchemaItemProvider strings =
            _schema.GetProvider(type: typeof(StringSchemaItemProvider)) as StringSchemaItemProvider;
        var stringArray = new List<StringItem>(capacity: strings.ChildItems.Count);
        foreach (StringItem str in strings.ChildItems)
        {
            stringArray.Add(item: str);
        }
        stringArray.Add(item: null);
        stringArray.Sort();
        return new StandardValuesCollection(values: stringArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            StringSchemaItemProvider strings =
                _schema.GetProvider(type: typeof(StringSchemaItemProvider))
                as StringSchemaItemProvider;
            foreach (ISchemaItem item in strings.ChildItems)
            {
                if (item.ToString() == value.ToString())
                {
                    return item as StringItem;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityConverter : System.ComponentModel.TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        EntityModelSchemaItemProvider entities =
            _schema.GetProvider(type: typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        var columnArray = new List<IDataEntity>(capacity: entities.ChildItems.Count);
        foreach (IDataEntity entity in entities.ChildItems)
        {
            columnArray.Add(item: entity);
        }
        columnArray.Add(item: null);
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            EntityModelSchemaItemProvider entities =
                _schema.GetProvider(type: typeof(EntityModelSchemaItemProvider))
                as EntityModelSchemaItemProvider;
            foreach (ISchemaItem item in entities.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as IDataEntity;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class FunctionConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        FunctionSchemaItemProvider functions =
            _schema.GetProvider(type: typeof(FunctionSchemaItemProvider))
            as FunctionSchemaItemProvider;
        var columnArray = new List<Function>(capacity: functions.ChildItems.Count);
        foreach (Function function in functions.ChildItems)
        {
            columnArray.Add(item: function);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            FunctionSchemaItemProvider functions =
                _schema.GetProvider(type: typeof(FunctionSchemaItemProvider))
                as FunctionSchemaItemProvider;
            foreach (ISchemaItem item in functions.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as Function;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureEntityConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        List<ISchemaItem> entities;
        DataStructureEntity parentEntity =
            (context.Instance as DataStructureEntity)?.ParentItem as DataStructureEntity;
        if (parentEntity == null)
        {
            // Root entity, we display all available entities
            entities = _schema.GetProvider<EntityModelSchemaItemProvider>().ChildItems.ToList();
        }
        else
        {
            // Sub-entity (relation), we return only available relations
            if (parentEntity.Entity is IDataEntity entity)
            {
                // Parent is root entity
                entities = entity.EntityRelations.ToList<ISchemaItem>();
            }
            else if (parentEntity.Entity is IAssociation association)
            {
                // Parent is relation
                entities = association.AssociatedEntity.EntityRelations.ToList<ISchemaItem>();
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "ParentItem",
                    actualValue: parentEntity,
                    message: ResourceUtils.GetString(key: "ErrorParentNotIDataEntity")
                );
            }
        }
        entities.Sort();
        return new StandardValuesCollection(values: entities);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<EntityRelationItem> entities;
            DataStructureEntity parentEntity =
                (context.Instance as DataStructureEntity).ParentItem as DataStructureEntity;
            if (parentEntity == null)
            {
                // Root entity, we display all available entities
                entities = new List<EntityRelationItem>();
            }
            else
            {
                // Sub-entity (relation), we return only available relations
                if (parentEntity.Entity is IDataEntity)
                {
                    // Parent is root entity
                    entities = (parentEntity.Entity as IDataEntity).EntityRelations;
                }
                else if (parentEntity.Entity is IAssociation)
                {
                    // Parent is relation
                    entities = (parentEntity.Entity as IAssociation)
                        .AssociatedEntity
                        .EntityRelations;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "ParentItem",
                        actualValue: parentEntity,
                        message: ResourceUtils.GetString(key: "ErrorParentNotIDataEntity")
                    );
                }
            }
            foreach (EntityRelationItem entity in entities)
            {
                if (entity.Name == value.ToString())
                {
                    return entity;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class RelationPrimaryKeyColumnConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
        List<IDataEntityColumn> columns = ((EntityRelationItem)columnPair.ParentItem)
            .BaseEntity
            .EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            columnArray.Add(item: column);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            EntityRelationColumnPairItem columnPair =
                context.Instance as EntityRelationColumnPairItem;
            foreach (
                ISchemaItem item in ((EntityRelationItem)columnPair.ParentItem)
                    .BaseEntity
                    .ChildItems
            )
            {
                if (item.Name == value.ToString())
                {
                    return item as IDataEntityColumn;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class RelationForeignKeyColumnConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
        List<IDataEntityColumn> columns = ((EntityRelationItem)columnPair.ParentItem)
            .RelatedEntity
            .EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            columnArray.Add(item: column);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            EntityRelationColumnPairItem columnPair =
                context.Instance as EntityRelationColumnPairItem;
            foreach (
                IDataEntityColumn item in ((EntityRelationItem)columnPair.ParentItem)
                    .RelatedEntity
                    .EntityColumns
            )
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class RelationFilterConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        EntityRelationFilter filter = context.Instance as EntityRelationFilter;
        List<EntityFilter> filters = (
            (EntityRelationItem)filter.ParentItem
        ).RelatedEntity.ChildItemsByType<EntityFilter>(itemType: EntityFilter.CategoryConst);
        var filterArray = new List<EntityFilter>(capacity: filters.Count);
        foreach (EntityFilter f in filters)
        {
            filterArray.Add(item: f);
        }
        filterArray.Sort();
        return new StandardValuesCollection(values: filterArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            EntityRelationFilter filter = context.Instance as EntityRelationFilter;
            foreach (
                var item in (
                    (EntityRelationItem)filter.ParentItem
                ).RelatedEntity.ChildItemsByType<EntityFilter>(itemType: EntityFilter.CategoryConst)
            )
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityFilterConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItem item = context.Instance as ISchemaItem;
        List<EntityFilter> filters = ((IDataEntity)item.RootItem).ChildItemsByType<EntityFilter>(
            itemType: EntityFilter.CategoryConst
        );
        var filterArray = new List<EntityFilter>(capacity: filters.Count);
        foreach (EntityFilter f in filters)
        {
            filterArray.Add(item: f);
        }
        filterArray.Sort();
        return new StandardValuesCollection(values: filterArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem schemaItem = context.Instance as ISchemaItem;
            foreach (
                EntityFilter item in (
                    (IDataEntity)schemaItem.RootItem
                ).ChildItemsByType<EntityFilter>(itemType: EntityFilter.CategoryConst)
            )
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureColumnConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataStructureColumn dsColumn = context.Instance as DataStructureColumn;
        DataStructureEntity dsEntity = (
            dsColumn.Entity == null ? dsColumn.ParentItem as DataStructureEntity : dsColumn.Entity
        );
        List<IDataEntityColumn> columns = dsEntity.EntityDefinition.EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            columnArray.Add(item: column);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureColumn dsColumn = context.Instance as DataStructureColumn;
            DataStructureEntity dsEntity = (
                dsColumn.Entity == null
                    ? dsColumn.ParentItem as DataStructureEntity
                    : dsColumn.Entity
            );
            List<IDataEntityColumn> columns = dsEntity.EntityDefinition.EntityColumns;

            foreach (IDataEntityColumn item in columns)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureColumnStringConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataStructureSortSetItem dataStructureSortSet =
            context.Instance as DataStructureSortSetItem;
        if (dataStructureSortSet.Entity == null)
        {
            return null;
        }
        DataStructureEntity DataEntity = dataStructureSortSet.Entity;
        List<DataStructureColumn> childitem = DataEntity.ChildItemsByType<DataStructureColumn>(
            itemType: DataStructureColumn.CategoryConst
        );
        List<DataStructureColumn> columnEntity = DataEntity.GetColumnsFromEntity();
        var columnArray = new List<string>();
        columnArray.AddRange(collection: childitem.Select(selector: x => x.Name).ToList());
        columnArray.AddRange(collection: columnEntity.Select(selector: x => x.Name).ToList());
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureSortSetItem dataStructureSortSet =
                context.Instance as DataStructureSortSetItem;
            if (dataStructureSortSet.Entity == null)
            {
                return null;
            }
            DataStructureEntity DataEntity = dataStructureSortSet.Entity;
            List<DataStructureColumn> childitem = DataEntity.ChildItemsByType<DataStructureColumn>(
                itemType: DataStructureColumn.CategoryConst
            );
            List<DataStructureColumn> columnEntity = DataEntity.GetColumnsFromEntity();
            int count = childitem
                .Where(predicate: x => x.Name == value.ToString())
                .ToList()
                .Count();
            count += columnEntity
                .Where(predicate: x => x.Name == value.ToString())
                .ToList()
                .Count();
            if (count != 0)
            {
                return value.ToString();
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

//	public class DataStructureEntityFilterConverter : System.ComponentModel.TypeConverter
//	{
//		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
//		{
//			//true means show a combobox
//			return true;
//		}
//
//		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
//		{
//			//true will limit to list. false will show the list,
//			//but allow free-form entry
//			return true;
//		}
//
//		public override System.ComponentModel.TypeConverter.StandardValuesCollection
//			GetStandardValues(ITypeDescriptorContext context)
//		{
//			DataStructureEntityFilter dsFilter = context.Instance as DataStructureEntityFilter;
//			DataStructureEntity dsEntity = dsFilter.ParentItem as DataStructureEntity;
//
//			ISchemaItemCollection filters = dsEntity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
//
//			ArrayList columnArray = new ArrayList(filters.Count);
//			foreach(EntityFilter filter in filters)
//			{
//				columnArray.Add(filter.Name);
//			}
//
//			columnArray.Sort();
//
//			return new StandardValuesCollection(columnArray);
//		}
//
//		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
//		{
//			if( sourceType == typeof(string) )
//				return true;
//			else
//				return base.CanConvertFrom(context, sourceType);
//		}
//
//		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
//		{
//			if( value.GetType() == typeof(string) )
//			{
//				DataStructureEntityFilter dsFilter = context.Instance as DataStructureEntityFilter;
//				DataStructureEntity dsEntity = dsFilter.ParentItem as DataStructureEntity;
//
//				ISchemaItemCollection filters = dsEntity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
//
//				foreach(EntityFilter item in filters)
//				{
//					if(item.Name == value.ToString())
//						return item as EntityFilter;
//				}
//				return null;
//			}
//			else
//				return base.ConvertFrom(context, culture, value);
//		}
//	}
//
public class EntityColumnReferenceConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItem reference = context.Instance as ISchemaItem;
        IDataEntity entity = reference.RootItem as IDataEntity;
        List<IDataEntityColumn> columns = entity.EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            // TODO: Check for recursion and skip any columns that could cause it
            columnArray.Add(item: column);
        }
        columnArray.Add(item: null);
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem reference = context.Instance as ISchemaItem;
            IDataEntity entity = reference.RootItem as IDataEntity;
            List<IDataEntityColumn> columns = entity.EntityColumns;
            foreach (IDataEntityColumn item in columns)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityForeignColumnConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataEntity entity =
            (context.Instance as IDataEntityColumn).ForeignKeyEntity as IDataEntity;
        if (entity == null)
        {
            return null;
        }

        List<IDataEntityColumn> columns = entity.EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            // TODO: Check for recursion and skip any columns that could cause it
            columnArray.Add(item: column);
        }
        columnArray.Add(item: null);
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IDataEntity entity =
                (context.Instance as IDataEntityColumn).ForeignKeyEntity as IDataEntity;
            if (entity == null)
            {
                return null;
            }

            List<IDataEntityColumn> columns = entity.EntityColumns;
            foreach (IDataEntityColumn item in columns)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityRelationConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItem reference = context.Instance as ISchemaItem;
        IDataEntity entity = reference.RootItem as IDataEntity;
        List<EntityRelationItem> relations = entity.EntityRelations;
        var relationArray = new List<EntityRelationItem>(capacity: relations.Count);
        foreach (EntityRelationItem relation in relations)
        {
            relationArray.Add(item: relation);
        }
        relationArray.Add(item: null);
        relationArray.Sort();
        return new StandardValuesCollection(values: relationArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem reference = context.Instance as ISchemaItem;
            IDataEntity entity = reference.RootItem as IDataEntity;
            List<EntityRelationItem> relations = entity.EntityRelations;
            foreach (IAssociation item in relations)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityRelationColumnsConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IRelationReference reference = context.Instance as IRelationReference;
        if (reference.Relation == null)
        {
            return null;
        }

        IDataEntity entity = reference.Relation.AssociatedEntity as IDataEntity;
        List<IDataEntityColumn> columns = entity.EntityColumns;
        var columnArray = new List<IDataEntityColumn>(capacity: columns.Count);
        foreach (IDataEntityColumn column in columns)
        {
            // TODO: Check for recursion and skip any columns that could cause it
            columnArray.Add(item: column);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IRelationReference reference = context.Instance as IRelationReference;
            IDataEntity entity = reference.Relation.AssociatedEntity as IDataEntity;
            List<IDataEntityColumn> columns = entity.EntityColumns;
            foreach (IDataEntityColumn item in columns)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataStructureSchemaItemProvider dataStructures =
            _schema.GetProvider(type: typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        var dsArray = new List<AbstractDataStructure>(capacity: dataStructures.ChildItems.Count);
        foreach (AbstractDataStructure ds in dataStructures.ChildItems)
        {
            dsArray.Add(item: ds);
        }
        dsArray.Add(item: null);
        dsArray.Sort();
        return new StandardValuesCollection(values: dsArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureSchemaItemProvider dataStructures =
                _schema.GetProvider(type: typeof(DataStructureSchemaItemProvider))
                as DataStructureSchemaItemProvider;
            foreach (ISchemaItem item in dataStructures.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as AbstractDataStructure;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataConstantConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataConstantSchemaItemProvider dataConstants =
            _schema.GetProvider(type: typeof(DataConstantSchemaItemProvider))
            as DataConstantSchemaItemProvider;
        var dcArray = new List<DataConstant>(capacity: dataConstants.ChildItems.Count);
        foreach (DataConstant dc in dataConstants.ChildItems)
        {
            dcArray.Add(item: dc);
        }
        dcArray.Add(item: null);
        dcArray.Sort();
        return new StandardValuesCollection(values: dcArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataConstantSchemaItemProvider dataConstants =
                _schema.GetProvider(type: typeof(DataConstantSchemaItemProvider))
                as DataConstantSchemaItemProvider;
            foreach (ISchemaItem item in dataConstants.ChildItems)
            {
                if (item.ToString() == value.ToString())
                {
                    return item as DataConstant;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class TransformationConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        TransformationSchemaItemProvider transformations =
            _schema.GetProvider(type: typeof(TransformationSchemaItemProvider))
            as TransformationSchemaItemProvider;
        var osArray = new List<ITransformation>(capacity: transformations.ChildItems.Count);
        osArray.Add(item: null);
        foreach (ITransformation os in transformations.ChildItems)
        {
            osArray.Add(item: os);
        }
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            TransformationSchemaItemProvider transformations =
                _schema.GetProvider(type: typeof(TransformationSchemaItemProvider))
                as TransformationSchemaItemProvider;
            foreach (ISchemaItem item in transformations.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as ITransformation;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataQueryEntityConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItem currentItem = context.Instance as ISchemaItem;
        if (!(currentItem.RootItem is DataStructure))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Instance",
                actualValue: context.Instance,
                message: "Root item of current context must be DataStructure"
            );
        }

        List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
        var entityArray = new List<DataStructureEntity>(capacity: entities.Count);
        foreach (DataStructureEntity entity in entities)
        {
            entityArray.Add(item: entity);
        }
        entityArray.Sort();
        return new StandardValuesCollection(values: entityArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem currentItem = context.Instance as ISchemaItem;
            if (!(currentItem.RootItem is DataStructure))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Instance",
                    actualValue: context.Instance,
                    message: "Root item of current context must be DataStructure"
                );
            }

            List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
            foreach (ISchemaItem item in entities)
            {
                if (item.Name == value.ToString())
                {
                    return item as DataStructureEntity;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class TransformOutputScalarOrigamDataTypeConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        XsltInitialValueParameter currentItem = (XsltInitialValueParameter)context.Instance;
        List<OrigamDataType> osArray = currentItem.getOrigamDataType();
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            XsltInitialValueParameter currentItem = (XsltInitialValueParameter)context.Instance;
            if (currentItem == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Instance",
                    actualValue: context.Instance,
                    message: "Current context must be XsltInitialValueParameter"
                );
            }

            foreach (OrigamDataType item in currentItem.getOrigamDataType())
            {
                if (item.ToString() == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataQueryEntityConverterNoSelf : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItem currentItem = context.Instance as ISchemaItem;
        if (!(currentItem.RootItem is DataStructure))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Instance",
                actualValue: context.Instance,
                message: "Root item of current context must be DataStructure"
            );
        }

        List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
        var entityArray = new List<DataStructureEntity>(capacity: entities.Count);
        foreach (DataStructureEntity entity in entities)
        {
            if (!entity.PrimaryKey.Equals(obj: currentItem.ParentItem.PrimaryKey))
            {
                entityArray.Add(item: entity);
            }
        }
        entityArray.Sort();
        return new StandardValuesCollection(values: entityArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem currentItem = context.Instance as ISchemaItem;
            if (!(currentItem.RootItem is DataStructure))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Instance",
                    actualValue: context.Instance,
                    message: "Root item of current context must be DataStructure"
                );
            }

            List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
            foreach (ISchemaItem item in entities)
            {
                if (item.Name == value.ToString())
                {
                    return item as DataStructureEntity;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureEntityFieldConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<IDataEntityColumn> fields;

        if (context.Instance is DataStructureDefaultSetDefault)
        {
            if (((context.Instance as DataStructureDefaultSetDefault).Entity) == null)
            {
                return new StandardValuesCollection(values: null);
            }

            fields = (context.Instance as DataStructureDefaultSetDefault)
                .Entity
                .EntityDefinition
                .EntityColumns;
        }
        else if (context.Instance is DataStructureRule)
        {
            if (((context.Instance as DataStructureRule).Entity) == null)
            {
                return new StandardValuesCollection(values: null);
            }

            fields = (context.Instance as DataStructureRule).Entity.EntityDefinition.EntityColumns;
        }
        else if (context.Instance is DataStructureRuleDependency)
        {
            if (((context.Instance as DataStructureRuleDependency).Entity) == null)
            {
                return new StandardValuesCollection(values: null);
            }

            fields = (context.Instance as DataStructureRuleDependency)
                .Entity
                .EntityDefinition
                .EntityColumns;
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "Instance",
                actualValue: context.Instance,
                message: "Type not supported by DataStructureEntityFieldConverter."
            );
        }
        var fieldArray = new List<IDataEntityColumn>(capacity: fields.Count);
        foreach (IDataEntityColumn field in fields)
        {
            fieldArray.Add(item: field);
        }
        fieldArray.Add(item: null);
        fieldArray.Sort();
        return new StandardValuesCollection(values: fieldArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<IDataEntityColumn> fields;

            if (context.Instance is DataStructureDefaultSetDefault)
            {
                fields = (context.Instance as DataStructureDefaultSetDefault)
                    .Entity
                    .EntityDefinition
                    .EntityColumns;
            }
            else if (context.Instance is DataStructureRule)
            {
                fields = (context.Instance as DataStructureRule)
                    .Entity
                    .EntityDefinition
                    .EntityColumns;
            }
            else if (context.Instance is DataStructureRuleDependency)
            {
                fields = (context.Instance as DataStructureRuleDependency)
                    .Entity
                    .EntityDefinition
                    .EntityColumns;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "Instance",
                    actualValue: context.Instance,
                    message: ResourceUtils.GetString(
                        key: "ErrorDataStructureEntityFieldConverterUnknownType"
                    )
                );
            }
            foreach (ISchemaItem item in fields)
            {
                if (item.Name == value.ToString())
                {
                    return item as IDataEntityColumn;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataQueryEntityFilterConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataStructureFilterSetFilter queryFilter = context.Instance as DataStructureFilterSetFilter;

        if (queryFilter.Entity == null)
        {
            return null;
        }

        List<EntityFilter> filters =
            queryFilter.Entity.EntityDefinition.ChildItemsByType<EntityFilter>(
                itemType: EntityFilter.CategoryConst
            );
        var columnArray = new List<EntityFilter>(capacity: filters.Count);
        foreach (EntityFilter filter in filters)
        {
            columnArray.Add(item: filter);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureFilterSetFilter filter = context.Instance as DataStructureFilterSetFilter;

            if (filter.Entity == null)
            {
                return null;
            }

            List<EntityFilter> filters =
                filter.Entity.EntityDefinition.ChildItemsByType<EntityFilter>(
                    itemType: EntityFilter.CategoryConst
                );
            foreach (EntityFilter item in filters)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureReferenceMethodConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataStructureReference reference = context.Instance as IDataStructureReference;

        if (reference.DataStructure == null)
        {
            return null;
        }

        List<DataStructureMethod> methods = reference.DataStructure.Methods;
        var methodArray = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Add(item: null);
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IDataStructureReference reference = context.Instance as IDataStructureReference;

            if (reference.DataStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = reference.DataStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureReferenceDefaultSetConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataStructureReference reference = context.Instance as IDataStructureReference;
        if (reference.DataStructure == null)
        {
            return null;
        }

        List<DataStructureDefaultSet> defaultSets = reference.DataStructure.DefaultSets;
        var array = new List<DataStructureDefaultSet>(capacity: defaultSets.Count);
        foreach (DataStructureDefaultSet item in defaultSets)
        {
            array.Add(item: item);
        }
        array.Add(item: null);
        array.Sort();
        return new StandardValuesCollection(values: array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IDataStructureReference reference = context.Instance as IDataStructureReference;
            if (reference.DataStructure == null)
            {
                return null;
            }

            List<DataStructureDefaultSet> defaultSets = reference.DataStructure.DefaultSets;
            foreach (DataStructureDefaultSet item in defaultSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureReferenceSortSetConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataStructureReference reference = context.Instance as IDataStructureReference;

        if (reference.DataStructure == null)
        {
            return null;
        }

        List<DataStructureSortSet> sortSets = reference.DataStructure.SortSets;
        var sortSetArray = new List<DataStructureSortSet>(capacity: sortSets.Count);
        foreach (DataStructureSortSet sortSet in sortSets)
        {
            sortSetArray.Add(item: sortSet);
        }
        sortSetArray.Add(item: null);
        sortSetArray.Sort();
        return new StandardValuesCollection(values: sortSetArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IDataStructureReference reference = context.Instance as IDataStructureReference;

            if (reference.DataStructure == null)
            {
                return null;
            }

            List<DataStructureSortSet> sortSets = reference.DataStructure.SortSets;
            foreach (DataStructureSortSet item in sortSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataLookupConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataLookupSchemaItemProvider lookups =
            _schema.GetProvider(type: typeof(IDataLookupSchemaItemProvider))
            as IDataLookupSchemaItemProvider;
        var lookupArray = new List<ISchemaItem>(capacity: lookups.ChildItems.Count);
        foreach (ISchemaItem lookup in lookups.ChildItems)
        {
            lookupArray.Add(item: lookup);
        }
        lookupArray.Add(item: null);
        lookupArray.Sort();
        return new StandardValuesCollection(values: lookupArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IDataLookupSchemaItemProvider lookups =
                _schema.GetProvider(type: typeof(IDataLookupSchemaItemProvider))
                as IDataLookupSchemaItemProvider;
            foreach (ISchemaItem item in lookups.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataConstantLookupReaderConverter : System.ComponentModel.TypeConverter
{
    IDataLookupService _lookupManager =
        ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
        as IDataLookupService;
    SortedList _currentList;
    IDataLookup _currentLookup;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return false;
    }

    private void InitList(Guid id, string valueMember, string displayMember)
    {
        DataView list = _lookupManager.GetList(lookupId: id, transactionId: null);
        string[] members = displayMember.Split(separator: ";".ToCharArray());
        _currentList = new SortedList(initialCapacity: list.Count);
        foreach (DataRowView rv in list)
        {
            string displayText = _lookupManager.ValueFromRow(row: rv.Row, columns: members);
            if (_currentList.Contains(key: displayText))
            {
                displayText += ", " + rv[property: valueMember].ToString();
            }
            _currentList.Add(key: displayText, value: rv[property: valueMember]);
        }
    }

    private void InitLookup(IDataLookup lookup)
    {
        if (lookup != _currentLookup)
        {
            _currentLookup = lookup;
            _currentList = null;
        }
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataConstant constant = context.Instance as DataConstant;
        IDataLookup lookup = constant.DataLookup;
        InitLookup(lookup: lookup);
        if (constant.DataType == OrigamDataType.Boolean)
        {
            var list = new List<string>(capacity: 2);
            list.Add(item: ResourceUtils.GetString(key: "Yes"));
            list.Add(item: ResourceUtils.GetString(key: "No"));
            return new StandardValuesCollection(values: list);
        }
        if (lookup == null)
        {
            return null;
        }

        try
        {
            InitList(
                id: (Guid)lookup.PrimaryKey[key: "Id"],
                valueMember: lookup.ListValueMember,
                displayMember: lookup.ListDisplayMember
            );
            return new StandardValuesCollection(values: _currentList.Values);
        }
        catch
        {
            return new StandardValuesCollection(values: new List<string>());
        }
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        DataConstant constant = context.Instance as DataConstant;
        IDataLookup lookup = (context.Instance as DataConstant).DataLookup;
        if (constant.DataType == OrigamDataType.Boolean)
        {
            return value.ToString() == ResourceUtils.GetString(key: "Yes");
        }
        if (lookup == null | value == null)
        {
            return value;
        }
        InitLookup(lookup: lookup);
        if (_currentList == null)
        {
            InitList(
                id: (Guid)lookup.PrimaryKey[key: "Id"],
                valueMember: lookup.ListValueMember,
                displayMember: lookup.ListDisplayMember
            );
        }

        if (_currentList.ContainsKey(key: value))
        {
            return _currentList[key: value];
        }

        return null;
    }

    public override object ConvertTo(
        ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value,
        Type destinationType
    )
    {
        if (value == null)
        {
            return null;
        }

        if (context == null)
        {
            return value.ToString();
        }

        if (value is bool)
        {
            if ((bool)value)
            {
                return ResourceUtils.GetString(key: "Yes");
            }

            return ResourceUtils.GetString(key: "No");
        }
        if (destinationType == typeof(string))
        {
            IDataLookup lookup = (context.Instance as DataConstant).DataLookup;
            InitLookup(lookup: lookup);
            if (lookup == null)
            {
                return value.ToString();
            }

            if (_currentList == null)
            {
                try
                {
                    InitList(
                        id: (Guid)lookup.PrimaryKey[key: "Id"],
                        valueMember: lookup.ListValueMember,
                        displayMember: lookup.ListDisplayMember
                    );
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (_currentList.ContainsValue(value: value))
            {
                int pos = _currentList.IndexOfValue(value: value);
                return _currentList.GetKey(index: pos);
            }

            return value.ToString();
        }

        return base.ConvertTo(
            context: context,
            culture: culture,
            value: value,
            destinationType: destinationType
        );
    }
}

public class DataRuleConverter : TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IRuleSchemaItemProvider rules =
            _schema.GetProvider(type: typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        var osArray = new List<IDataRule>();
        foreach (IDataRule os in rules.DataRules)
        {
            osArray.Add(item: os);
        }

        osArray.Add(item: null);
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules =
                _schema.GetProvider(type: typeof(IRuleSchemaItemProvider))
                as IRuleSchemaItemProvider;
            foreach (IDataRule item in rules.DataRules)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EndRuleConverter : TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IRuleSchemaItemProvider rules =
            _schema.GetProvider(type: typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        var osArray = new List<IEndRule>();
        foreach (IEndRule os in rules.EndRules)
        {
            osArray.Add(item: os);
        }
        osArray.Add(item: null);
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules =
                _schema.GetProvider(type: typeof(IRuleSchemaItemProvider))
                as IRuleSchemaItemProvider;
            foreach (IEndRule item in rules.EndRules)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class StartRuleConverter : TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IRuleSchemaItemProvider rules =
            _schema.GetProvider(type: typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        var osArray = new List<IStartRule>();
        foreach (IStartRule os in rules.StartRules)
        {
            osArray.Add(item: os);
        }
        osArray.Add(item: null);
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules =
                _schema.GetProvider(type: typeof(IRuleSchemaItemProvider))
                as IRuleSchemaItemProvider;
            foreach (IStartRule item in rules.StartRules)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class EntityRuleConverter : TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IRuleSchemaItemProvider rules =
            _schema.GetProvider(type: typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        var osArray = new List<IEntityRule>();
        foreach (IEntityRule os in rules.EntityRules)
        {
            osArray.Add(item: os);
        }
        osArray.Add(item: null);
        osArray.Sort();
        return new StandardValuesCollection(values: osArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules =
                _schema.GetProvider(type: typeof(IRuleSchemaItemProvider))
                as IRuleSchemaItemProvider;
            foreach (IEntityRule item in rules.EntityRules)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataStructureRuleSetConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DataStructureRuleSetReference ruleSetReference =
            context.Instance as DataStructureRuleSetReference;
        // list all rulesets of datastructure and it's
        DataStructure ds = ruleSetReference.RootItem as DataStructure;
        List<DataStructureRuleSet> ruleSets = ds.RuleSets;
        var array = new List<DataStructureRuleSet>(capacity: ruleSets.Count);
        foreach (DataStructureRuleSet item in ruleSets)
        {
            array.Add(item: item);
        }
        array.Add(item: null);
        array.Sort();
        return new StandardValuesCollection(values: array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureRuleSetReference ruleSetReference =
                context.Instance as DataStructureRuleSetReference;
            // list all rulesets of datastructure and it's
            DataStructure ds = ruleSetReference.RootItem as DataStructure;
            List<DataStructureRuleSet> ruleSets = ds.RuleSets;
            foreach (DataStructureRuleSet item in ruleSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataTypeMappingConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        DatabaseDataTypeSchemaItemProvider mappings =
            _schema.GetProvider(type: typeof(DatabaseDataTypeSchemaItemProvider))
            as DatabaseDataTypeSchemaItemProvider;
        OrigamDataType dataType = (context.Instance as IDatabaseDataTypeMapping).DataType;
        var mappingArray = new List<DatabaseDataType>();
        foreach (DatabaseDataType mapping in mappings.ChildItems)
        {
            if (mapping.DataType == dataType)
            {
                mappingArray.Add(item: mapping);
            }
        }
        mappingArray.Add(item: null);
        mappingArray.Sort();
        return new StandardValuesCollection(values: mappingArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DatabaseDataTypeSchemaItemProvider mappings =
                _schema.GetProvider(type: typeof(DatabaseDataTypeSchemaItemProvider))
                as DatabaseDataTypeSchemaItemProvider;
            foreach (ISchemaItem mapping in mappings.ChildItems)
            {
                if (mapping.Name == value.ToString())
                {
                    return mapping;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataTypeMappingAvailableTypesConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IServiceAgent dataServiceAgent = (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        dataServiceAgent.MethodName = "DatabaseSpecificDatatypes";
        dataServiceAgent.Run();
        var result = new List<string>(collection: (string[])dataServiceAgent.Result);
        result.Sort();
        return new StandardValuesCollection(values: result);
    }
}
