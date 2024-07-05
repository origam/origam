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
using Origam.Services;
using Origam.Workbench.Services;
using System.Collections;
using System.Collections.Generic;

namespace Origam.Schema.EntityModel;
public static class EntityHelper
{
	public static FieldMappingItem CreateForeignKey(
		string name, string caption, bool allowNulls, 
		IDataEntity masterEntity, IDataEntity foreignEntity, 
		IDataEntityColumn foreignField, IDataLookup lookup, bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var fieldMappingItem = masterEntity.NewItem<FieldMappingItem>(
			schemaService.ActiveSchemaExtensionId, null);
		fieldMappingItem.Name = name;
		fieldMappingItem.MappedColumnName = name;
		fieldMappingItem.AllowNulls = allowNulls;
		fieldMappingItem.DataLength = foreignField.DataLength;
		fieldMappingItem.DataType = foreignField.DataType;
		fieldMappingItem.Caption = caption;
		fieldMappingItem.DefaultLookup = lookup;
		fieldMappingItem.XmlMappingType = EntityColumnXmlMapping.Attribute;
		fieldMappingItem.ForeignKeyEntity = foreignEntity;
		fieldMappingItem.ForeignKeyField = foreignField;
		if(persist)
		{
			fieldMappingItem.Persist();
		}
		return fieldMappingItem;
	}
	public static DataStructure CreateDataStructure(
		IDataEntity entity, string name, bool persist)
	{
		var schemaService = ServiceManager.Services
			.GetService<ISchemaService>();
		var dataStructureSchemaItemProvider = schemaService
			.GetProvider<DataStructureSchemaItemProvider>();
		// Data Structure
		var dataStructure = dataStructureSchemaItemProvider
			.NewItem<DataStructure>(
			schemaService.ActiveSchemaExtensionId, null);
		dataStructure.Name = name;
		if(entity.Group != null)
		{
			var schemaItemGroup = GetDataStructureGroup(entity.Group.Name);
			if(schemaItemGroup != null)
			{
				if(dataStructure.Name.StartsWith("Lookup"))
				{
					var lookupGroup = schemaItemGroup.GetGroup("Lookups");
					dataStructure.Group = lookupGroup ?? schemaItemGroup;
				}
				else
				{
					dataStructure.Group = schemaItemGroup;
				}
			}
		}
		// DS Entity
		var dataStructureEntity = dataStructure
			.NewItem<DataStructureEntity>(
				schemaService.ActiveSchemaExtensionId, null);
		dataStructureEntity.Entity = entity as AbstractSchemaItem;
		dataStructureEntity.Name = entity.Name;
		dataStructureEntity.AllFields = true;
		if(persist)
		{
			dataStructure.Persist();
		}
		return dataStructure;
	}
    public static DataStructure CreateDataStructure(
        IDataEntity entity, object nameOfEntity, bool v)
    {
        throw new NotImplementedException();
    }
    public static DataStructureColumn CreateDataStructureField(
        DataStructureEntity dataStructureEntity, 
        IDataEntityColumn field, 
        bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var dataStructureColumn = dataStructureEntity
			.NewItem<DataStructureColumn>(
				schemaService.ActiveSchemaExtensionId, null);
		dataStructureColumn.Field = field;
		dataStructureColumn.Name = field.Name;
		if(persist)
		{
			dataStructureColumn.Persist();
		}
		return dataStructureColumn;
	}
	public static DataStructureFilterSet CreateFilterSet(
		DataStructure dataStructure, 
		string name, 
		bool persist)	
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var dataStructureFilterSet = dataStructure
			.NewItem<DataStructureFilterSet>(
				schemaService.ActiveSchemaExtensionId, null);
		dataStructureFilterSet.Name = name;
		if(persist)
		{
			dataStructureFilterSet.Persist();
		}
		return dataStructureFilterSet;
	}
	public static DataStructureFilterSetFilter CreateFilterSetFilter(
		DataStructureFilterSet dataStructureFilterSet, 
		DataStructureEntity dataStructureEntity, 
		EntityFilter filter, 
		bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var dataStructureFilterSetFilter = dataStructureFilterSet
			.NewItem<DataStructureFilterSetFilter>(
				schemaService.ActiveSchemaExtensionId, null);
		dataStructureFilterSetFilter.Entity = dataStructureEntity;
		dataStructureFilterSetFilter.Filter = filter;
		if(persist)
		{
			dataStructureFilterSetFilter.Persist();
		}
		return dataStructureFilterSetFilter;
	}
	
	public static DataStructureSortSet CreateSortSet(
		DataStructure dataStructure, string name, bool persist)
	{
		var schemaService
			= ServiceManager.Services.GetService<ISchemaService>();
		var sortSet = dataStructure.NewItem<DataStructureSortSet>(
				schemaService.ActiveSchemaExtensionId, null);
		sortSet.Name = name;
		if(persist)
		{
			sortSet.Persist();
		}
		return sortSet;
	}
	public static DataStructureSortSetItem CreateSortSetItem(
		DataStructureSortSet sortSet,
		DataStructureEntity entity,
		string fieldName,
		DataStructureColumnSortDirection direction,
		bool persist)
	{
		var schemaService
			= ServiceManager.Services.GetService<ISchemaService>();
		var sortSetItem = sortSet.NewItem<DataStructureSortSetItem>(
				schemaService.ActiveSchemaExtensionId, null);
		sortSetItem.Entity = entity;
		sortSetItem.FieldName = fieldName;
		sortSetItem.SortDirection = direction;
		if(persist)
		{
			sortSetItem.Persist();
		}
		return sortSetItem;
	}
    public static TableMappingItem CreateTable(
        string name, 
        SchemaItemGroup group,
        bool persist)
    {
        return CreateTable(name, group, persist, true);
    }
    public static TableMappingItem CreateTable(
        string name, 
        SchemaItemGroup group, 
        bool persist, 
        bool useDefaultAncestor)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var entityModelSchemaItemProvider = schemaService
			.GetProvider<EntityModelSchemaItemProvider>();
        var newEntity = entityModelSchemaItemProvider
            .NewItem<TableMappingItem>(
	            schemaService.StorageSchemaExtensionId, null);
		newEntity.Name = name;
		newEntity.MappedObjectName = name;
		newEntity.Group = group;
        if(!useDefaultAncestor)
        {
            newEntity.Ancestors.RemoveAt(0);
        }
        if(persist)
        {
            newEntity.Persist();
        }
        return newEntity;
	}
    public static DataConstant CreateConstant(
        string name, 
        IDataLookup lookup, 
        OrigamDataType dataType, 
        object value, 
        SchemaItemGroup group, 
        bool persist)
    {
        var schemaService 
            = ServiceManager.Services.GetService<ISchemaService>();
        var dataConstantSchemaItemProvider = schemaService
            .GetProvider<DataConstantSchemaItemProvider>();
        var newConstant = dataConstantSchemaItemProvider
            .NewItem<DataConstant>(
	            schemaService.ActiveSchemaExtensionId, null);
        newConstant.Name = name;
        newConstant.DataLookup = lookup;
        newConstant.Group = group;
        newConstant.DataType = dataType;
        newConstant.Value = value;
        if(persist)
        {
            newConstant.Persist();
        }
        return newConstant;
    }
    
    public static IDataEntity DefaultAncestor
	{
		get
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var entityModelSchemaItemProvider = schemaService
				.GetProvider<EntityModelSchemaItemProvider>();
			// Set IOrigamEntity as an ancestor, so we get primary key
			IDataEntity origamEntity = null;
			foreach(var abstractSchemaItem 
			        in entityModelSchemaItemProvider.ChildItems)
			{
				var dataEntity = (IDataEntity)abstractSchemaItem;
				if(dataEntity.Name == DefaultAncestorName)
				{
					origamEntity = dataEntity;
					break;
				}
			}
			if(origamEntity == null)
			{
				throw new Exception(
					ResourceUtils.GetString("ErrorIOrigamEntity2NotFound"));
			}
			return origamEntity;
		}
	}
    public static string DefaultAncestorName => "IOrigamEntity2";
    public static Guid LanguageEntityId 
	    => new Guid("efdff1b5-916a-4036-977e-e34f59e72d41");
    public static Guid GetByLanguageIdFilterId 
	    => new Guid("afb5b84b-f4a3-4e3a-a382-c110624f71c9");
    public static Guid refLanguageIdColumnId 
	    => new Guid("13a2f9f8-c3e8-49fb-86e0-0515a72a10f2");
    public static Guid ILocalizationEntityId 
	    => new Guid("b822344b-e1a6-4de0-ae41-ea49e9f981ac");
    public static IDataEntityColumn DefaultPrimaryKey
	{
		get
		{
			foreach(IDataEntityColumn dataEntityColumn 
			        in DefaultAncestor.EntityColumns)
			{
				if(dataEntityColumn.IsPrimaryKey
				&& !dataEntityColumn.ExcludeFromAllFields)
				{
					return dataEntityColumn;
				}
			}
			throw new Exception(
				ResourceUtils.GetString("ErrorDefaultPrimaryKey"));
		}
	}
	public static EntityFilter DefaultPrimaryKeyFilter
	{
		get
		{
			foreach(EntityFilter filter in DefaultAncestor.EntityFilters)
			{
				if(filter.Name == "GetId") return filter;
			}
			throw new Exception(
				ResourceUtils.GetString("ErrorDefaultPrimaryKey"));
		}
	}
	public static SchemaItemAncestor AddAncestor(
		IDataEntity entity, 
		IDataEntity ancestorEntity, 
		bool persist)
	{
		var abstractSchemaItem = entity as AbstractSchemaItem;
		var ancestor = new SchemaItemAncestor();
		ancestor.SchemaItem = entity as AbstractSchemaItem;
		ancestor.Ancestor = ancestorEntity as AbstractSchemaItem;
		ancestor.PersistenceProvider = abstractSchemaItem.PersistenceProvider;
		abstractSchemaItem.Ancestors.Add(ancestor);
		if(persist)
		{
			ancestor.Persist();
		}
		return ancestor;
	}
    public static FieldMappingItem CreateColumn(
        IDataEntity entity, 
        string name,
        bool allowNulls, 
        OrigamDataType dataType, 
        int dataLength,
        string caption, 
        IDataEntity foreignKeyEntity, 
        IDataEntityColumn foreignKeyField, 
        bool persist)
    {
        return CreateColumn(entity, name, allowNulls, dataType,  dataLength, 
            null, caption, foreignKeyEntity, foreignKeyField, persist);
    }
    public static FieldMappingItem CreateColumn(
        IDataEntity entity,
        string name, 
        bool allowNulls, 
        OrigamDataType dataType, 
        int dataLength, 
        DatabaseDataType databaseType, 
        string caption, 
        IDataEntity foreignKeyEntity, 
        IDataEntityColumn foreignKeyField,
        bool persist)
    {
        var schemaService 
	        = ServiceManager.Services.GetService<ISchemaService>();
		var fieldMappingItem = entity.NewItem<FieldMappingItem>( 
            schemaService.StorageSchemaExtensionId, null);
		fieldMappingItem.Name = name;
		fieldMappingItem.MappedColumnName = name;
		fieldMappingItem.AllowNulls = allowNulls;
		fieldMappingItem.DataLength = dataLength;
		fieldMappingItem.DataType = dataType;
        fieldMappingItem.MappedDataType = databaseType;
		fieldMappingItem.Caption = caption;
		fieldMappingItem.XmlMappingType = EntityColumnXmlMapping.Attribute;
		fieldMappingItem.ForeignKeyEntity = foreignKeyEntity;
		fieldMappingItem.ForeignKeyField = foreignKeyField;
		if(persist)
		{
			fieldMappingItem.Persist();
		}
		return fieldMappingItem;
	}
	public static EntityRelationItem CreateRelation(
		IDataEntity parentEntity, 
		IDataEntity relatedEntity, 
		bool masterDetail, 
		bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var entityRelationItem = ((AbstractSchemaItem)parentEntity)
			.NewItem<EntityRelationItem>(
				schemaService.ActiveSchemaExtensionId, null);
		entityRelationItem.Name = relatedEntity.Name;
		entityRelationItem.RelatedEntity = relatedEntity;
		entityRelationItem.IsParentChild = masterDetail;
		if(persist)
		{
			entityRelationItem.Persist();
		}
		return entityRelationItem;
	}
	public static EntityRelationColumnPairItem CreateRelationKey(
		EntityRelationItem relation, 
		IDataEntityColumn baseField, 
		IDataEntityColumn relatedField, 
		bool persist)
	{
		return CreateRelationKey(relation, baseField, relatedField, persist,
			null);
	}
	public static EntityRelationColumnPairItem CreateRelationKey(
		EntityRelationItem relation, 
		IDataEntityColumn baseField, 
		IDataEntityColumn relatedField, 
		bool persist,
		string nameOfKey)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var entityRelationColumnPairItem = relation
			.NewItem<EntityRelationColumnPairItem>(
				schemaService.ActiveSchemaExtensionId, null);
		entityRelationColumnPairItem.BaseEntityField = baseField;
		entityRelationColumnPairItem.RelatedEntityField = relatedField;
		if(!string.IsNullOrEmpty(nameOfKey))
        {
			entityRelationColumnPairItem.Name = nameOfKey;
		}
		if(persist)
		{
			entityRelationColumnPairItem.Persist();
		}
		return entityRelationColumnPairItem;
	}
	public static DataStructureReference CreateDataStructureReference(
		AbstractSchemaItem parentItem, 
		DataStructure structure, 
		DataStructureMethod method, 
		DataStructureSortSet sortSet, 
		bool persist)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var dataStructureReference = parentItem
			.NewItem<DataStructureReference>(
				schemaService.ActiveSchemaExtensionId, null);
		dataStructureReference.DataStructure = structure;
		dataStructureReference.Method = method;
		dataStructureReference.SortSet = sortSet;
		if(persist)
		{
			dataStructureReference.Persist();
		}
		return dataStructureReference;
	}
	public static SchemaItemGroup GetDataStructureGroup(string name)
	{
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		var dataStructureSchemaItemProvider 
			= schemaService.GetProvider<DataStructureSchemaItemProvider>();
		return dataStructureSchemaItemProvider.GetGroup(name);
	}
    public static SchemaItemGroup GetDataConstantGroup(string name)
    {
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
        var dataConstantSchemaItemProvider 
            = schemaService.GetProvider<DataConstantSchemaItemProvider>();
        return dataConstantSchemaItemProvider.GetGroup(name);
    }
    public static EntityFilter CreateFilter(
        IDataEntityColumn field, 
        string functionName,
        string filterPrefix, 
        bool createParameter)
    {
        return CreateFilter(field, functionName, filterPrefix, 
            createParameter, null);
    }
    public static EntityFilter CreateFilter(
        IDataEntityColumn field, 
        string functionName, 
        string filterPrefix, 
        bool createParameter, 
        IList<AbstractSchemaItem> generatedElements)
	{
		switch (functionName)
		{
			case "In":
				return CreateFilter(field, functionName, filterPrefix, 
                    createParameter, "FilterExpression", "List", true,
                    generatedElements);
			default:
				return CreateFilter(field, functionName, filterPrefix, 
                    createParameter, "Left", "Right", false,
                    generatedElements);
		}
	}

	private static EntityFilter CreateFilter(
		IDataEntityColumn field, 
		string functionName, 
		string filterPrefix, 
		bool createParameter, 
		string leftName, 
		string rightName, 
		bool isRightArray, 
        IList<AbstractSchemaItem> generatedElements)
	{
		if(field.Name == null)
		{
			throw new ArgumentException("Field Name is not set.");
		}
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
        var entity = (IDataEntity)field.ParentItem;
        // filter
		var filter = entity.NewItem<EntityFilter>(
			schemaService.ActiveSchemaExtensionId, null);
		filter.Name = filterPrefix + (field.Name.StartsWith("ref") 
			? field.Name.Substring(3) : field.Name);
		if(isRightArray)
		{
			filter.Name += "List";
		}
		filter.Persist();
		generatedElements?.Add(filter);
        // function call
		var functionCall = filter.NewItem<FunctionCall>(
			schemaService.ActiveSchemaExtensionId, null);
		var functionSchemaItemProvider 
			= schemaService.GetProvider<FunctionSchemaItemProvider>();
		var function = (Function)functionSchemaItemProvider.GetChildByName(
			functionName, Function.CategoryConst);
		if(function == null)
		{
			throw new Exception(
				$"{functionName} function not found. Cannot create filter.");
		}
		functionCall.Function = function;
		functionCall.Name = functionName;
		functionCall.Persist();
        // field reference
		var entityColumnReference = functionCall.GetChildByName(leftName)
			.NewItem<EntityColumnReference>(
				schemaService.ActiveSchemaExtensionId, null);
		entityColumnReference.Field = field;
		entityColumnReference.Persist();
		if(!createParameter)
		{
			return filter;
		}
        // parameter
        var databaseParameter = entity.NewItem<DatabaseParameter>(
            schemaService.ActiveSchemaExtensionId, null);
		if(isRightArray)
		{
			databaseParameter.DataType = OrigamDataType.Array;
		}
		else
		{
			databaseParameter.DataType = field.DataType;
			if (field is IDatabaseDataTypeMapping mappableDataType)
			{
				databaseParameter.MappedDataType 
					= mappableDataType.MappedDataType;
			}
			databaseParameter.DataLength = field.DataLength;
		}
		databaseParameter.Name = "par" + (field.Name.StartsWith("ref") 
			? field.Name.Substring(3) : field.Name);
		if(isRightArray)
		{
			databaseParameter.Name += "List";
		}
		databaseParameter.Persist();
		var parameterReference = functionCall.GetChildByName(rightName)
			.NewItem<ParameterReference>(
				schemaService.ActiveSchemaExtensionId, null);
		parameterReference.Parameter = databaseParameter;
		parameterReference.Persist();
		generatedElements?.Add(parameterReference);
		return filter;
    }
    public static TableMappingItem CreateLanguageTranslationChildEntity(
        TableMappingItem parentEntity, ICollection selectedFields)
    {
        return CreateLanguageTranslationChildEntity(
            parentEntity, selectedFields, null);
    }
    public static TableMappingItem CreateLanguageTranslationChildEntity(
        TableMappingItem parentEntity, ICollection selectedFields,
        IList<AbstractSchemaItem> generatedElements)
	{			
		var schemaService 
			= ServiceManager.Services.GetService<ISchemaService>();
		// create child entity (based on IOrigamEntity2)
		var newEntity = CreateTable($"{parentEntity.Name}_l10n", 
			parentEntity.Group, false);
	    var entityCaption = string.IsNullOrEmpty(parentEntity.Caption)
	        ? parentEntity.Name
	        : parentEntity.Caption;
	    newEntity.Caption = $"{entityCaption} Localization";
	    generatedElements?.Add(newEntity);
        // create unique composite index on foreign key to parent entity and reference to language
		var dataEntityIndex = newEntity.NewItem<DataEntityIndex>(
			schemaService.ActiveSchemaExtensionId, null);
		dataEntityIndex.Name = "ix_unq_" + parentEntity.Name;
		dataEntityIndex.IsUnique = true;
		dataEntityIndex.Persist();
	    newEntity.Persist();
        // Create relation from the parent entity
        var parentRelation = CreateRelation(
            parentEntity, newEntity, true, true);
		parentEntity.LocalizationRelation = parentRelation;
		generatedElements?.Add(parentRelation);
        var parentRelationAll = CreateRelation(
            parentEntity, newEntity, true, false);
		parentRelationAll.Name += "_all";
		parentRelationAll.Persist();
		generatedElements?.Add(parentRelationAll);
        var indexColumns = new List<FieldMappingItem>();
		// Create reference columns
		foreach(IDataEntityColumn primaryKey in parentEntity.EntityPrimaryKey)
		{
			if(primaryKey.ExcludeFromAllFields)
			{
				continue;
			}
			var refParentColumn = CreateColumn(
				newEntity, $"ref{parentEntity.Name}{primaryKey.Name}", 
				false, primaryKey.DataType, primaryKey.DataLength, 
				parentEntity.Caption, parentEntity, primaryKey, true);
			var key = CreateRelationKey(
				parentRelation, primaryKey, refParentColumn, true);
			var keyAll = CreateRelationKey(
				parentRelationAll, primaryKey, refParentColumn, true);
			indexColumns.Add(refParentColumn);
		}
		// get language entity
		var persistenceService 
			= ServiceManager.Services.GetService<IPersistenceService>();
		// get localization
		var localizationKey = new ModelElementKey(ILocalizationEntityId);
		var localizationEntity = (IDataEntity)persistenceService
			.SchemaProvider.RetrieveInstance<AbstractSchemaItem>(
				localizationKey.Id);
		// and set ancestor
		AddAncestor(newEntity, localizationEntity, true);
		// use language filter in parent relation
		var entityRelationFilter = parentRelation
			.NewItem<EntityRelationFilter>(
				schemaService.ActiveSchemaExtensionId, null);
		entityRelationFilter.Filter = localizationEntity.GetChildById(
			GetByLanguageIdFilterId) as EntityFilter;
		entityRelationFilter.Persist();	
		// add refLanguageId to unique index
		indexColumns.Add(localizationEntity.GetChildById(
			refLanguageIdColumnId) as FieldMappingItem);
		// create index items
		var i = 0;
		foreach(FieldMappingItem column in indexColumns)
		{
			var field = dataEntityIndex.NewItem<DataEntityIndexField>(
				schemaService.ActiveSchemaExtensionId, null);
			field.Field = column;
			field.OrdinalPosition = i;
			field.Persist();
			i++;
		}
		// create selected fields
		foreach(IDataEntityColumn column in selectedFields)
		{
			CreateColumn(newEntity, column.Name, column.AllowNulls, 
				column.DataType, column.DataLength, column.Caption, null, 
				null, true);
		}
		newEntity.Persist();
		parentEntity.Persist();
		return newEntity;
	}
    public static DataEntityIndex CreateIndex(IDataEntity entity, 
        IDataEntityColumn field, bool unique, bool persist)
    {
        var schemaService 
            = ServiceManager.Services.GetService<ISchemaService>();
        var dataEntityIndex = ((AbstractSchemaItem)entity)
            .NewItem<DataEntityIndex>(
	            schemaService.ActiveSchemaExtensionId, null);
        dataEntityIndex.Name = $"ix{(unique ? "Unique" : "")}{field.Name}";
        dataEntityIndex.IsUnique = unique;
        if(persist)
        {
            dataEntityIndex.Persist();
        }
        CreateIndexField(dataEntityIndex, field, persist);
        return dataEntityIndex;
    }
    public static DataEntityIndexField CreateIndexField(
        DataEntityIndex index, IDataEntityColumn field, bool persist)
    {
        var schemaService 
            = ServiceManager.Services.GetService<ISchemaService>();
        var dataEntityIndexField = index.NewItem<DataEntityIndexField>(
	            schemaService.ActiveSchemaExtensionId, null);
        dataEntityIndexField.Field = field;
        if(persist)
        {
            dataEntityIndexField.Persist();
        }
        return dataEntityIndexField;
    }
}
