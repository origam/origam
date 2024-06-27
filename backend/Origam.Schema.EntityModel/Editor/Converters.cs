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
using System.Data;
using System.ComponentModel;
using System.Collections;
using Origam.Services;
using Origam.Workbench.Services;
using System.Linq;
using System.Collections.Generic;

namespace Origam.Schema.EntityModel;
public class StringItemConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		StringSchemaItemProvider strings = _schema.GetProvider(typeof(StringSchemaItemProvider)) as StringSchemaItemProvider;
		ArrayList stringArray = new ArrayList(strings.ChildItems.Count);
		foreach(StringItem str in strings.ChildItems)
		{
			stringArray.Add(str);
		}
		stringArray.Add(null);
		stringArray.Sort();
		return new StandardValuesCollection(stringArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			StringSchemaItemProvider strings = _schema.GetProvider(typeof(StringSchemaItemProvider)) as StringSchemaItemProvider;
			foreach(AbstractSchemaItem item in strings.ChildItems)
			{
				if(item.ToString() == value.ToString())
					return item as StringItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class EntityConverter : System.ComponentModel.TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		EntityModelSchemaItemProvider entities = _schema.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
		ArrayList columnArray = new ArrayList(entities.ChildItems.Count);
		foreach(IDataEntity entity in entities.ChildItems)
		{
			columnArray.Add(entity);
		}
		columnArray.Add(null);
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			EntityModelSchemaItemProvider entities = _schema.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
			foreach(AbstractSchemaItem item in entities.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as IDataEntity;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class FunctionConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		FunctionSchemaItemProvider functions = _schema.GetProvider(typeof(FunctionSchemaItemProvider)) as FunctionSchemaItemProvider;
		ArrayList columnArray = new ArrayList(functions.ChildItems.Count);
		foreach(Function function in functions.ChildItems)
		{
			columnArray.Add(function);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			FunctionSchemaItemProvider functions = _schema.GetProvider(typeof(FunctionSchemaItemProvider)) as FunctionSchemaItemProvider;
			foreach(AbstractSchemaItem item in functions.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as Function;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class DataStructureEntityConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		ArrayList columnArray;
		List<ISchemaItem> entities;
		DataStructureEntity parentEntity = (context.Instance as DataStructureEntity).ParentItem as DataStructureEntity;
		if(parentEntity == null)
		{
			// Root entity, we display all available entities
			entities = new List<ISchemaItem>();
		}
		else
		{
			// Sub-entity (relation), we return only available relations
			if(parentEntity.Entity is IDataEntity)
				// Parent is root entity
				entities = (parentEntity.Entity as IDataEntity).EntityRelations;
			else if(parentEntity.Entity is IAssociation)
				// Parent is relation
				entities = (parentEntity.Entity as IAssociation).AssociatedEntity.EntityRelations;
			else
				throw new ArgumentOutOfRangeException("ParentItem", parentEntity, ResourceUtils.GetString("ErrorParentNotIDataEntity"));
		}
		columnArray = new ArrayList(entities.Count);
		
		foreach(AbstractSchemaItem entity in entities)
		{
			columnArray.Add(entity);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			List<ISchemaItem> entities;
			DataStructureEntity parentEntity = (context.Instance as DataStructureEntity).ParentItem as DataStructureEntity;
			if(parentEntity == null)
			{
				// Root entity, we display all available entities
				entities = new List<ISchemaItem>();
			}
			else
			{
				// Sub-entity (relation), we return only available relations
				if(parentEntity.Entity is IDataEntity)
					// Parent is root entity
					entities = (parentEntity.Entity as IDataEntity).EntityRelations;
				else if(parentEntity.Entity is IAssociation)
					// Parent is relation
					entities = (parentEntity.Entity as IAssociation).AssociatedEntity.EntityRelations;
				else
					throw new ArgumentOutOfRangeException("ParentItem", parentEntity, ResourceUtils.GetString("ErrorParentNotIDataEntity"));
			}
			foreach(AbstractSchemaItem entity in entities)
			{
				if(entity.Name == value.ToString())
					return entity as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
		List<ISchemaItem> columns = ((EntityRelationItem)columnPair.ParentItem).BaseEntity.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			columnArray.Add(column);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
			foreach(AbstractSchemaItem item in ((EntityRelationItem)columnPair.ParentItem).BaseEntity.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
		List<ISchemaItem> columns = ((EntityRelationItem)columnPair.ParentItem).RelatedEntity.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			columnArray.Add(column);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			EntityRelationColumnPairItem columnPair = context.Instance as EntityRelationColumnPairItem;
			foreach(AbstractSchemaItem item in ((EntityRelationItem)columnPair.ParentItem).RelatedEntity.EntityColumns)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		EntityRelationFilter filter = context.Instance as EntityRelationFilter;
		List<ISchemaItem> filters = ((EntityRelationItem)filter.ParentItem).RelatedEntity.ChildItemsByType(EntityFilter.CategoryConst);
		ArrayList filterArray = new ArrayList(filters.Count);
		foreach(EntityFilter f in filters)
		{
			filterArray.Add(f);
		}
		filterArray.Sort();
		return new StandardValuesCollection(filterArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			EntityRelationFilter filter = context.Instance as EntityRelationFilter;
			foreach(AbstractSchemaItem item in ((EntityRelationItem)filter.ParentItem).RelatedEntity.ChildItemsByType(EntityFilter.CategoryConst))
			{
				if(item.Name == value.ToString())
					return item as EntityFilter;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		AbstractSchemaItem item = context.Instance as AbstractSchemaItem;
		List<ISchemaItem> filters = ((IDataEntity)item.RootItem).ChildItemsByType(EntityFilter.CategoryConst);
		ArrayList filterArray = new ArrayList(filters.Count);
		foreach(EntityFilter f in filters)
		{
			filterArray.Add(f);
		}
		filterArray.Sort();
		return new StandardValuesCollection(filterArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			AbstractSchemaItem schemaItem = context.Instance as AbstractSchemaItem;
			foreach(AbstractSchemaItem item in ((IDataEntity)schemaItem.RootItem).ChildItemsByType(EntityFilter.CategoryConst))
			{
				if(item.Name == value.ToString())
					return item as EntityFilter;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		DataStructureColumn dsColumn = context.Instance as DataStructureColumn;
		DataStructureEntity dsEntity = (dsColumn.Entity == null ? dsColumn.ParentItem as DataStructureEntity : dsColumn.Entity);
		List<ISchemaItem> columns = dsEntity.EntityDefinition.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			columnArray.Add(column);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			DataStructureColumn dsColumn = context.Instance as DataStructureColumn;
			DataStructureEntity dsEntity = (dsColumn.Entity == null ? dsColumn.ParentItem as DataStructureEntity : dsColumn.Entity);
			List<ISchemaItem> columns = dsEntity.EntityDefinition.EntityColumns;
		
			foreach(IDataEntityColumn item in columns)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        DataStructureSortSetItem dataStructureSortSet = context.Instance as DataStructureSortSetItem;
        if (dataStructureSortSet.Entity == null)
        {
            return null;
        }
        DataStructureEntity DataEntity = dataStructureSortSet.Entity;
        IEnumerable<DataStructureColumn> childitem = DataEntity.ChildItemsByType(DataStructureColumn.CategoryConst).Cast<DataStructureColumn>();
        IEnumerable<DataStructureColumn> columnEntity = DataEntity.GetColumnsFromEntity().Cast<DataStructureColumn>();
        ArrayList columnArray = new ArrayList();
        columnArray.AddRange(childitem.Select(x => x.Name).ToList());
        columnArray.AddRange(columnEntity.Select(x => x.Name).ToList());
        columnArray.Sort();
        return new StandardValuesCollection(columnArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureSortSetItem dataStructureSortSet = context.Instance as DataStructureSortSetItem;
            if (dataStructureSortSet.Entity == null)
            {
                return null;
            }
            DataStructureEntity DataEntity = dataStructureSortSet.Entity;
            IEnumerable<DataStructureColumn> childitem = DataEntity.ChildItemsByType(DataStructureColumn.CategoryConst).Cast<DataStructureColumn>();
            IEnumerable<DataStructureColumn> columnEntity = DataEntity.GetColumnsFromEntity().Cast<DataStructureColumn>();
            int count = childitem.Where(x => x.Name == value.ToString()).ToList().Count();
            count += columnEntity.Where(x => x.Name == value.ToString()).ToList().Count();
            if(count!=0)
            {
                return value.ToString();
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
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
//			SchemaItemCollection filters = dsEntity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
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
//				SchemaItemCollection filters = dsEntity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
		IDataEntity entity = reference.RootItem as IDataEntity;
		List<ISchemaItem> columns = entity.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			// TODO: Check for recursion and skip any columns that could cause it
			columnArray.Add(column);
		}
		columnArray.Add(null);
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
			IDataEntity entity = reference.RootItem as IDataEntity;
			List<ISchemaItem> columns = entity.EntityColumns;
			foreach(IDataEntityColumn item in columns)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IDataEntity entity = (context.Instance as IDataEntityColumn).ForeignKeyEntity as IDataEntity;
		if(entity == null) return null;
		List<ISchemaItem> columns = entity.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			// TODO: Check for recursion and skip any columns that could cause it
			columnArray.Add(column);
		}
		columnArray.Add(null);
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IDataEntity entity = (context.Instance as IDataEntityColumn).ForeignKeyEntity as IDataEntity;
			if(entity == null) return null;
			List<ISchemaItem> columns = entity.EntityColumns;
			foreach(IDataEntityColumn item in columns)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
		IDataEntity entity = reference.RootItem as IDataEntity;
		List<ISchemaItem> relations = entity.EntityRelations;
		ArrayList relationArray = new ArrayList(relations.Count);
		foreach(IAssociation relation in relations)
		{
			relationArray.Add(relation);
		}
		relationArray.Add(null);
		relationArray.Sort();
		return new StandardValuesCollection(relationArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
			IDataEntity entity = reference.RootItem as IDataEntity;
			List<ISchemaItem> relations = entity.EntityRelations;
			foreach(IAssociation item in relations)
			{
				if(item.Name == value.ToString())
					return item as IAssociation;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IRelationReference reference = context.Instance as IRelationReference;
		if(reference.Relation == null) return null;
		IDataEntity entity = reference.Relation.AssociatedEntity as IDataEntity;
		List<ISchemaItem> columns = entity.EntityColumns;
		ArrayList columnArray = new ArrayList(columns.Count);
		foreach(IDataEntityColumn column in columns)
		{
			// TODO: Check for recursion and skip any columns that could cause it
			columnArray.Add(column);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IRelationReference reference = context.Instance as IRelationReference;
			IDataEntity entity = reference.Relation.AssociatedEntity as IDataEntity;
			List<ISchemaItem> columns = entity.EntityColumns;
			foreach(IDataEntityColumn item in columns)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class DataStructureConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		DataStructureSchemaItemProvider dataStructures = _schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
		ArrayList dsArray = new ArrayList(dataStructures.ChildItems.Count);
		foreach(AbstractDataStructure ds in dataStructures.ChildItems)
		{
			dsArray.Add(ds);
		}
		dsArray.Add(null);
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			DataStructureSchemaItemProvider dataStructures = _schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
			foreach(AbstractSchemaItem item in dataStructures.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as AbstractDataStructure;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class DataConstantConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		DataConstantSchemaItemProvider dataConstants = _schema.GetProvider(typeof(DataConstantSchemaItemProvider)) as DataConstantSchemaItemProvider;
		ArrayList dcArray = new ArrayList(dataConstants.ChildItems.Count);
		foreach(DataConstant dc in dataConstants.ChildItems)
		{
			dcArray.Add(dc);
		}
		dcArray.Add(null);
		dcArray.Sort();
		return new StandardValuesCollection(dcArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			DataConstantSchemaItemProvider dataConstants = _schema.GetProvider(typeof(DataConstantSchemaItemProvider)) as DataConstantSchemaItemProvider;
			foreach(AbstractSchemaItem item in dataConstants.ChildItems)
			{
				if(item.ToString() == value.ToString())
					return item as DataConstant;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class TransformationConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		TransformationSchemaItemProvider transformations = _schema.GetProvider(typeof(TransformationSchemaItemProvider)) as TransformationSchemaItemProvider;
		ArrayList osArray = new ArrayList(transformations.ChildItems.Count);
		osArray.Add(null);
		foreach(ITransformation os in transformations.ChildItems)
		{
			osArray.Add(os);
		}
		osArray.Sort();
		return new StandardValuesCollection(osArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			TransformationSchemaItemProvider transformations = _schema.GetProvider(typeof(TransformationSchemaItemProvider)) as TransformationSchemaItemProvider;
			foreach(AbstractSchemaItem item in transformations.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as ITransformation;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		AbstractSchemaItem currentItem = context.Instance as AbstractSchemaItem;
		if(!(currentItem.RootItem is DataStructure)) throw new ArgumentOutOfRangeException("Instance", context.Instance, "Root item of current context must be DataStructure");
		List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
		ArrayList entityArray = new ArrayList(entities.Count);
		foreach(DataStructureEntity entity in entities)
		{
			entityArray.Add(entity);
		}
		entityArray.Sort();
		return new StandardValuesCollection(entityArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			AbstractSchemaItem currentItem = context.Instance as AbstractSchemaItem;
			if(!(currentItem.RootItem is DataStructure)) throw new ArgumentOutOfRangeException("Instance", context.Instance, "Root item of current context must be DataStructure");
			List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
			foreach(AbstractSchemaItem item in entities)
			{
				if(item.Name == value.ToString())
					return item as DataStructureEntity;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class TransformOutputScalarOrigamDataTypeConverter : TypeConverter
{
    ISchemaService _schema = ServiceManager.Services.GetService(
        typeof(ISchemaService)) as ISchemaService;
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        XsltInitialValueParameter currentItem = (XsltInitialValueParameter)context.Instance;
        List<OrigamDataType> osArray = currentItem.getOrigamDataType();
        osArray.Sort();
        return new StandardValuesCollection(osArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
           XsltInitialValueParameter currentItem = (XsltInitialValueParameter)context.Instance;
           if (currentItem == null) throw new ArgumentOutOfRangeException("Instance", context.Instance, "Current context must be XsltInitialValueParameter");
            foreach (OrigamDataType item in currentItem.getOrigamDataType())
            {
                if (item.ToString() == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        AbstractSchemaItem currentItem = context.Instance as AbstractSchemaItem;
        if (!(currentItem.RootItem is DataStructure)) throw new ArgumentOutOfRangeException("Instance", context.Instance, "Root item of current context must be DataStructure");
        List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
        ArrayList entityArray = new ArrayList(entities.Count);
        foreach (DataStructureEntity entity in entities)
        {
            if(!entity.PrimaryKey.Equals(currentItem.ParentItem.PrimaryKey))
            {
                entityArray.Add(entity);
            }
        }
        entityArray.Sort();
        return new StandardValuesCollection(entityArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            AbstractSchemaItem currentItem = context.Instance as AbstractSchemaItem;
            if (!(currentItem.RootItem is DataStructure)) throw new ArgumentOutOfRangeException("Instance", context.Instance, "Root item of current context must be DataStructure");
            List<DataStructureEntity> entities = ((DataStructure)currentItem.RootItem).Entities;
            foreach (AbstractSchemaItem item in entities)
            {
                if (item.Name == value.ToString())
                    return item as DataStructureEntity;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		List<ISchemaItem> fields;
		
		if(context.Instance is DataStructureDefaultSetDefault)
		{
			if(((context.Instance as DataStructureDefaultSetDefault).Entity) == null) return new StandardValuesCollection(null);
			fields = (context.Instance as DataStructureDefaultSetDefault).Entity.EntityDefinition.EntityColumns;
		}
		else if(context.Instance is DataStructureRule)
		{
			if(((context.Instance as DataStructureRule).Entity) == null) return new StandardValuesCollection(null);
			fields = (context.Instance as DataStructureRule).Entity.EntityDefinition.EntityColumns;
		}
		else if(context.Instance is DataStructureRuleDependency)
		{
			if(((context.Instance as DataStructureRuleDependency).Entity) == null) return new StandardValuesCollection(null);
			fields = (context.Instance as DataStructureRuleDependency).Entity.EntityDefinition.EntityColumns;
		}
		else
		{
			throw new ArgumentOutOfRangeException("Instance", context.Instance, "Type not supported by DataStructureEntityFieldConverter.");
		}
		ArrayList fieldArray = new ArrayList(fields.Count);
		foreach(IDataEntityColumn field in fields)
		{
			fieldArray.Add(field);
		}
		fieldArray.Add(null);
		fieldArray.Sort();
		return new StandardValuesCollection(fieldArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			List<ISchemaItem> fields;
		
			if(context.Instance is DataStructureDefaultSetDefault)
			{
				fields = (context.Instance as DataStructureDefaultSetDefault).Entity.EntityDefinition.EntityColumns;
			}
			else if(context.Instance is DataStructureRule)
			{
				fields = (context.Instance as DataStructureRule).Entity.EntityDefinition.EntityColumns;
			}
			else if(context.Instance is DataStructureRuleDependency)
			{
				fields = (context.Instance as DataStructureRuleDependency).Entity.EntityDefinition.EntityColumns;
			}
			else
			{
				throw new ArgumentOutOfRangeException("Instance", context.Instance, ResourceUtils.GetString("ErrorDataStructureEntityFieldConverterUnknownType"));
			}
			foreach(AbstractSchemaItem item in fields)
			{
				if(item.Name == value.ToString())
					return item as IDataEntityColumn;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		DataStructureFilterSetFilter queryFilter = context.Instance as DataStructureFilterSetFilter;
		
		if(queryFilter.Entity == null) return null;
		List<ISchemaItem> filters = queryFilter.Entity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
		ArrayList columnArray = new ArrayList(filters.Count);
		foreach(EntityFilter filter in filters)
		{
			columnArray.Add(filter);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			DataStructureFilterSetFilter filter = context.Instance as DataStructureFilterSetFilter;
		
			if(filter.Entity == null) return null;
			List<ISchemaItem> filters = filter.Entity.EntityDefinition.ChildItemsByType(EntityFilter.CategoryConst);
			foreach(EntityFilter item in filters)
			{
				if(item.Name == value.ToString())
					return item as EntityFilter;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IDataStructureReference reference = context.Instance as IDataStructureReference;
		
		if(reference.DataStructure == null) return null;
		List<ISchemaItem> methods = reference.DataStructure.Methods;
		ArrayList methodArray = new ArrayList(methods.Count);
		foreach(DataStructureMethod method in methods)
		{
			methodArray.Add(method);
		}
		methodArray.Add(null);
		methodArray.Sort();
		return new StandardValuesCollection(methodArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IDataStructureReference reference = context.Instance as IDataStructureReference;
		
			if(reference.DataStructure == null) return null;
			List<ISchemaItem> methods = reference.DataStructure.Methods;
			foreach(DataStructureMethod item in methods)
			{
				if(item.Name == value.ToString())
					return item as DataStructureMethod;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
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
        public override System.ComponentModel.TypeConverter.StandardValuesCollection
                GetStandardValues(ITypeDescriptorContext context)
        {
            IDataStructureReference reference = context.Instance as IDataStructureReference;
            if (reference.DataStructure == null) return null;
            List<ISchemaItem> defaultSets = reference.DataStructure.DefaultSets;
            ArrayList array = new ArrayList(defaultSets.Count);
            foreach (AbstractSchemaItem item in defaultSets)
            {
                array.Add(item);
            }
            array.Add(null);
            array.Sort();
            return new StandardValuesCollection(array);
        }
        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                IDataStructureReference reference = context.Instance as IDataStructureReference;
                if (reference.DataStructure == null) return null;
                List<ISchemaItem> defaultSets = reference.DataStructure.DefaultSets;                    
                foreach (AbstractSchemaItem item in defaultSets)
                {
                    if (item.Name == value.ToString())
                        return item as DataStructureDefaultSet;
                }
                return null;
            }
            else
                return base.ConvertFrom(context, culture, value);
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IDataStructureReference reference = context.Instance as IDataStructureReference;
		
		if(reference.DataStructure == null) return null;
		List<ISchemaItem> sortSets = reference.DataStructure.SortSets;
		ArrayList sortSetArray = new ArrayList(sortSets.Count);
		foreach(DataStructureSortSet sortSet in sortSets)
		{
			sortSetArray.Add(sortSet);
		}
		sortSetArray.Add(null);
		sortSetArray.Sort();
		return new StandardValuesCollection(sortSetArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IDataStructureReference reference = context.Instance as IDataStructureReference;
		
			if(reference.DataStructure == null) return null;
			List<ISchemaItem> sortSets = reference.DataStructure.SortSets;
			foreach(DataStructureSortSet item in sortSets)
			{
				if(item.Name == value.ToString())
					return item as DataStructureSortSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class DataLookupConverter : TypeConverter
{
	ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IDataLookupSchemaItemProvider lookups = _schema.GetProvider(typeof(IDataLookupSchemaItemProvider)) as IDataLookupSchemaItemProvider;
		ArrayList lookupArray = new ArrayList(lookups.ChildItems.Count);
		foreach(AbstractSchemaItem lookup in lookups.ChildItems)
		{
			lookupArray.Add(lookup);
		}
		lookupArray.Add(null);
		lookupArray.Sort();
		return new StandardValuesCollection(lookupArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IDataLookupSchemaItemProvider lookups = _schema.GetProvider(typeof(IDataLookupSchemaItemProvider)) as IDataLookupSchemaItemProvider;
			foreach(AbstractSchemaItem item in lookups.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class DataConstantLookupReaderConverter : System.ComponentModel.TypeConverter
{
	IDataLookupService _lookupManager = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
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
		DataView list = _lookupManager.GetList(id, null);
		string[] members = displayMember.Split(";".ToCharArray());
		_currentList = new SortedList(list.Count);
		foreach(DataRowView rv in list)
		{
			string displayText = _lookupManager.ValueFromRow(rv.Row, members);
			if(_currentList.Contains(displayText))
			{
				displayText += ", " + rv[valueMember].ToString();
			}
			_currentList.Add(displayText, rv[valueMember]);
		}
	}
	private void InitLookup(IDataLookup lookup)
	{
		if(lookup != _currentLookup)
		{
			_currentLookup = lookup;
			_currentList = null;
		}
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		DataConstant constant = context.Instance as DataConstant;
		IDataLookup lookup = constant.DataLookup;
		InitLookup(lookup);
		if(constant.DataType == OrigamDataType.Boolean)
		{
			ArrayList list = new ArrayList(2);
			list.Add(ResourceUtils.GetString("Yes"));
			list.Add(ResourceUtils.GetString("No"));
			return new StandardValuesCollection(list);
		}
		if(lookup == null) return null;
		try
		{
			InitList((Guid)lookup.PrimaryKey["Id"], lookup.ListValueMember, lookup.ListDisplayMember);
			return new StandardValuesCollection(_currentList.Values);
		}
		catch
		{
			return new StandardValuesCollection(new ArrayList());
		}
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		DataConstant constant = context.Instance as DataConstant;
		IDataLookup lookup = (context.Instance as DataConstant).DataLookup;
		if(constant.DataType == OrigamDataType.Boolean)
		{
            return value.ToString() == ResourceUtils.GetString("Yes");
		}
		if(lookup == null | value == null)
		{
			return value;
		}
		else
		{
			InitLookup(lookup);
			if(_currentList == null) InitList((Guid)lookup.PrimaryKey["Id"], lookup.ListValueMember, lookup.ListDisplayMember);
			if(_currentList.ContainsKey(value))
			{
				return _currentList[value];
			}
			else
			{
				return null;
			}
		}
	}
	public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
	{
		if(value == null) return null;
		if(context == null) return value.ToString();
		if(value is bool)
		{
			if((bool)value)
			{
				return ResourceUtils.GetString("Yes");
			}
			else
			{
				return ResourceUtils.GetString("No");
			}
		}
		if(destinationType == typeof(string))
		{
			IDataLookup lookup = (context.Instance as DataConstant).DataLookup;
			InitLookup(lookup);
			if(lookup == null) return value.ToString();
			if(_currentList == null)
            {
                try
                {
                    InitList((Guid)lookup.PrimaryKey["Id"], lookup.ListValueMember, lookup.ListDisplayMember);
                }
                catch
                {
                    return value.ToString();
                }
            }
			
			if(_currentList.ContainsValue(value))
			{
				int pos = _currentList.IndexOfValue(value);
				return _currentList.GetKey(pos);
			}
			else
			{
				return value.ToString();
			}
		}
		else
		{
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
public class DataRuleConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(
		typeof(ISchemaService)) as ISchemaService;
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
		ArrayList osArray = new ArrayList();
        foreach (IRule os in rules.DataRules)
		{
			osArray.Add(os);
		}
		
		osArray.Add(null);
		osArray.Sort();
		return new StandardValuesCollection(osArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
            foreach (AbstractSchemaItem item in rules.DataRules)
			{
				if(item.Name == value.ToString())
					return item as IRule;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class EndRuleConverter : TypeConverter
{
    static ISchemaService _schema = ServiceManager.Services.GetService(
        typeof(ISchemaService)) as ISchemaService;
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        ArrayList osArray = new ArrayList();
        foreach (IRule os in rules.EndRules)
        {
            osArray.Add(os);
        }
        osArray.Add(null);
        osArray.Sort();
        return new StandardValuesCollection(osArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
            foreach (AbstractSchemaItem item in rules.EndRules)
            {
                if (item.Name == value.ToString())
                    return item as IRule;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
    }
}
public class StartRuleConverter : TypeConverter
{
    static ISchemaService _schema = ServiceManager.Services.GetService(
        typeof(ISchemaService)) as ISchemaService;
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        ArrayList osArray = new ArrayList();
        foreach (IRule os in rules.StartRules)
        {
            osArray.Add(os);
        }
        osArray.Add(null);
        osArray.Sort();
        return new StandardValuesCollection(osArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
            foreach (AbstractSchemaItem item in rules.StartRules)
            {
                if (item.Name == value.ToString())
                    return item as IRule;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
    }
}
public class EntityRuleConverter : TypeConverter
{
    static ISchemaService _schema = ServiceManager.Services.GetService(
        typeof(ISchemaService)) as ISchemaService;
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
        ArrayList osArray = new ArrayList();
        foreach (IRule os in rules.EntityRules)
        {
            osArray.Add(os);
        }
        osArray.Add(null);
        osArray.Sort();
        return new StandardValuesCollection(osArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            IRuleSchemaItemProvider rules = _schema.GetProvider(typeof(IRuleSchemaItemProvider)) as IRuleSchemaItemProvider;
            foreach (AbstractSchemaItem item in rules.EntityRules)
            {
                if (item.Name == value.ToString())
                    return item as IRule;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        DataStructureRuleSetReference ruleSetReference = context.Instance as DataStructureRuleSetReference;
        // list all rulesets of datastructure and it's 
        DataStructure ds = ruleSetReference.RootItem as DataStructure;
        List<ISchemaItem> ruleSets = ds.RuleSets;
        ArrayList array = new ArrayList(ruleSets.Count);
        foreach (AbstractSchemaItem item in ruleSets)
        {
            array.Add(item);
        }
        array.Add(null);
        array.Sort();
        return new StandardValuesCollection(array);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            DataStructureRuleSetReference ruleSetReference = context.Instance as DataStructureRuleSetReference;
            // list all rulesets of datastructure and it's 
            DataStructure ds = ruleSetReference.RootItem as DataStructure;
            List<ISchemaItem> ruleSets = ds.RuleSets;
            foreach (AbstractSchemaItem item in ruleSets)
            {
                if (item.Name == value.ToString())
                    return item as DataStructureRuleSet;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
    }
}
public class DataTypeMappingConverter : TypeConverter
{
    ISchemaService _schema = ServiceManager.Services.GetService(
        typeof(ISchemaService)) as ISchemaService;
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        DatabaseDataTypeSchemaItemProvider mappings = _schema.GetProvider(
            typeof(DatabaseDataTypeSchemaItemProvider)) as DatabaseDataTypeSchemaItemProvider;
        OrigamDataType dataType = (context.Instance as IDatabaseDataTypeMapping).DataType;
        ArrayList mappingArray = new ArrayList();
        foreach (DatabaseDataType mapping in mappings.ChildItems)
        {
            if (mapping.DataType == dataType)
            {
                mappingArray.Add(mapping);
            }
        }
        mappingArray.Add(null);
        mappingArray.Sort();
        return new StandardValuesCollection(mappingArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            DatabaseDataTypeSchemaItemProvider mappings = _schema.GetProvider(
                typeof(DatabaseDataTypeSchemaItemProvider)) as DatabaseDataTypeSchemaItemProvider;
            foreach (AbstractSchemaItem mapping in mappings.ChildItems)
            {
                if (mapping.Name == value.ToString())
                    return mapping as AbstractSchemaItem;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
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
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
        dataServiceAgent.MethodName = "DatabaseSpecificDatatypes";
        dataServiceAgent.Run();
        ArrayList result = new ArrayList((string[])dataServiceAgent.Result);
        result.Sort();
        return new StandardValuesCollection(result);
    }
}
