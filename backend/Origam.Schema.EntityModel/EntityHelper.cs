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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityHelper.
	/// </summary>
	public class EntityHelper
	{
		public static FieldMappingItem CreateForeignKey(string name, string caption, bool allowNulls, IDataEntity masterEntity, IDataEntity foreignEntity, IDataEntityColumn foreignField, IDataLookup lookup, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			EntityModelSchemaItemProvider entityProvider = schema.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;

			FieldMappingItem fk = masterEntity.NewItem(typeof(FieldMappingItem), schema.ActiveSchemaExtensionId, null) as FieldMappingItem;
			fk.Name = name;
			fk.MappedColumnName = name;
			fk.AllowNulls = allowNulls;
			fk.DataLength = foreignField.DataLength;
			fk.DataType = foreignField.DataType;
			fk.Caption = caption;
			fk.DefaultLookup = lookup;
			fk.XmlMappingType = EntityColumnXmlMapping.Attribute;

			fk.ForeignKeyEntity = foreignEntity;
			fk.ForeignKeyField = foreignField;

			if(persist) fk.Persist();

			return fk;
		}

		public static DataStructure CreateDataStructure(IDataEntity entity, string name, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			DataStructureSchemaItemProvider dsprovider = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;

			// Data Structure
			DataStructure ds = dsprovider.NewItem(typeof(DataStructure), schema.ActiveSchemaExtensionId, null) as DataStructure;
			ds.Name = name;

			if(entity.Group != null)
			{
				SchemaItemGroup group = GetDataStructureGroup(entity.Group.Name);
				if(group != null)
				{
					if(ds.Name.StartsWith("Lookup"))
					{
						SchemaItemGroup lookupGroup = group.GetGroup("Lookups");
						if(lookupGroup == null)
						{
							ds.Group = group;
						}
						else
						{
							ds.Group = lookupGroup;
						}
					}
					else
					{
						ds.Group = group;
					}
				}
			}

			// DS Entity
			DataStructureEntity dsEntity = ds.NewItem(typeof(DataStructureEntity), schema.ActiveSchemaExtensionId, null) as DataStructureEntity;
			dsEntity.Entity = entity as AbstractSchemaItem;
			dsEntity.Name = entity.Name;
			dsEntity.AllFields = true;

			ds.Persist();

			return ds;
		}

        public static DataStructure CreateDataStructure(IDataEntity entity, object nameOfEntity, bool v)
        {
            throw new NotImplementedException();
        }

        public static DataStructureColumn CreateDataStructureField(DataStructureEntity dataSturctureEntity, IDataEntityColumn field, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			DataStructureColumn c = dataSturctureEntity.NewItem(typeof(DataStructureColumn), schema.ActiveSchemaExtensionId, null) as DataStructureColumn;
			c.Field = field;
			c.Name = field.Name;

			if(persist) c.Persist();

			return c;
		}

		public static DataStructureFilterSet CreateFilterSet(DataStructure ds, string name, bool persist)	
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			DataStructureFilterSet fs = ds.NewItem(typeof(DataStructureFilterSet), schema.ActiveSchemaExtensionId, null) as DataStructureFilterSet;
			fs.Name = name;
			
			if(persist) fs.Persist();

			return fs;
		}

		public static DataStructureFilterSetFilter CreateFilterSetFilter(DataStructureFilterSet fs, DataStructureEntity dataStructureEntity, EntityFilter filter, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			DataStructureFilterSetFilter fsf = fs.NewItem(typeof(DataStructureFilterSetFilter), schema.ActiveSchemaExtensionId, null) as DataStructureFilterSetFilter ;
			fsf.Entity = dataStructureEntity;
			fsf.Filter = filter;

			if(persist) fsf.Persist();

			return fsf;
		}

		public static DataStructureSortSet CreateSortSet(
			DataStructure dataStructure, string name, bool persist)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var sortSet = dataStructure.NewItem(
					typeof(DataStructureSortSet), 
					schemaService.ActiveSchemaExtensionId, null) 
				as DataStructureSortSet;
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
			bool persist
			)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var sortSetItem = sortSet.NewItem(
					typeof(DataStructureSortSetItem),
					schemaService.ActiveSchemaExtensionId, null)
				as DataStructureSortSetItem;
			sortSetItem.Entity = entity;
			sortSetItem.FieldName = fieldName;
			sortSetItem.SortDirection = direction;
			if(persist)
			{
				sortSetItem.Persist();
			}
			return sortSetItem;
		}

        public static TableMappingItem CreateTable(string name, SchemaItemGroup group,
            bool persist)
        {
            return CreateTable(name, group, persist, true);
        }

        public static TableMappingItem CreateTable(string name, SchemaItemGroup group, 
            bool persist, bool useDefaultAncestor)
		{
			ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
			EntityModelSchemaItemProvider entityProvider = schema.GetProvider(
                typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;
            TableMappingItem newEntity = entityProvider.NewItem(typeof(TableMappingItem), 
                schema.StorageSchemaExtensionId, null) as TableMappingItem;
			newEntity.Name = name;
			newEntity.MappedObjectName = name;
			newEntity.Group = group;
            if (!useDefaultAncestor)
            {
                newEntity.Ancestors.RemoveAt(0);
            }
			if(persist) newEntity.Persist();
            return newEntity;
		}

        public static DataConstant CreateConstant(string name, IDataLookup lookup, 
            OrigamDataType dataType, object value, SchemaItemGroup group, bool persist)
        {
            ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
            DataConstantSchemaItemProvider constantProvider = schema.GetProvider(
                typeof(DataConstantSchemaItemProvider)) as DataConstantSchemaItemProvider;

            DataConstant newConstant = constantProvider.NewItem(
                typeof(DataConstant), schema.ActiveSchemaExtensionId, null) as DataConstant;
            newConstant.Name = name;
            newConstant.DataLookup = lookup;
            newConstant.Group = group;
            newConstant.DataType = dataType;
            newConstant.Value = value;
            if (persist) newConstant.Persist();
            return newConstant;
        }
        
        public static IDataEntity DefaultAncestor
		{
			get
			{
				ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

				EntityModelSchemaItemProvider entityProvider = schema.GetProvider(typeof(EntityModelSchemaItemProvider)) as EntityModelSchemaItemProvider;

				// Set IOrigamEntity as an ancestor, so we get primary key
				IDataEntity origamEntity = null;

				foreach(IDataEntity x in entityProvider.ChildItems)
				{
                    if (x.Name == DefaultAncestorName)
					{
						origamEntity = x;
						break;
					}
				}

				if(origamEntity == null) throw new Exception(ResourceUtils.GetString("ErrorIOrigamEntity2NotFound"));

				return origamEntity;
			}
		}

        public static string DefaultAncestorName => "IOrigamEntity2";

	    public static Guid LanguageEntityId => new Guid("efdff1b5-916a-4036-977e-e34f59e72d41");

	    public static Guid GetByLanguageIdFilterId => new Guid("afb5b84b-f4a3-4e3a-a382-c110624f71c9");

	    public static Guid refLanguageIdColymId => new Guid("13a2f9f8-c3e8-49fb-86e0-0515a72a10f2");

	    public static Guid ILocalizationEntityId => new Guid("b822344b-e1a6-4de0-ae41-ea49e9f981ac");

	    public static IDataEntityColumn DefaultPrimaryKey
		{
			get
			{
				foreach(IDataEntityColumn col in EntityHelper.DefaultAncestor.EntityColumns)
				{
					if(col.IsPrimaryKey && !col.ExcludeFromAllFields) return col;
				}

				throw new Exception(ResourceUtils.GetString("ErrorDefaultPrimaryKey"));
			}
		}

		public static EntityFilter DefaultPrimaryKeyFilter
		{
			get
			{
				foreach(EntityFilter filter in EntityHelper.DefaultAncestor.EntityFilters)
				{
					if(filter.Name == "GetId") return filter;
				}

				throw new Exception(ResourceUtils.GetString("ErrorDefaultPrimaryKey"));
			}
		}

		public static SchemaItemAncestor AddAncestor(IDataEntity entity, IDataEntity ancestorEntity, bool persist)
		{
			AbstractSchemaItem e = entity as AbstractSchemaItem;

			SchemaItemAncestor ancestor = new SchemaItemAncestor();
			ancestor.SchemaItem = entity as AbstractSchemaItem;
			ancestor.Ancestor = ancestorEntity as AbstractSchemaItem;
			ancestor.PersistenceProvider = e.PersistenceProvider;
			e.Ancestors.Add(ancestor);

			if(persist) ancestor.Persist();

			return ancestor;
		}

        public static FieldMappingItem CreateColumn(IDataEntity entity, 
            string name,bool allowNulls, OrigamDataType dataType, int dataLength,
            string caption, IDataEntity foreignKeyEntity, 
            IDataEntityColumn foreignKeyField, bool persist)
        {
            return CreateColumn(entity, name, allowNulls, dataType,  dataLength, 
                null, caption, foreignKeyEntity, foreignKeyField, persist);
        }

        public static FieldMappingItem CreateColumn(IDataEntity entity,
            string name, bool allowNulls, OrigamDataType dataType, int dataLength,
            DatabaseDataType databaseType, string caption, 
            IDataEntity foreignKeyEntity, IDataEntityColumn foreignKeyField,
            bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
			FieldMappingItem f = entity.NewItem(typeof(FieldMappingItem), 
                schema.StorageSchemaExtensionId, null) as FieldMappingItem;
			f.Name = name;
			f.MappedColumnName = name;
			f.AllowNulls = allowNulls;
			f.DataLength = dataLength;
			f.DataType = dataType;
            f.MappedDataType = databaseType;
			f.Caption = caption;
			f.XmlMappingType = EntityColumnXmlMapping.Attribute;
			f.ForeignKeyEntity = foreignKeyEntity;
			f.ForeignKeyField = foreignKeyField;
			if(persist) f.Persist();
			return f;
		}

		public static EntityRelationItem CreateRelation(IDataEntity parentEntity, IDataEntity relatedEntity, bool masterDetail, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			EntityRelationItem r = (parentEntity as AbstractSchemaItem).NewItem(typeof(EntityRelationItem), schema.ActiveSchemaExtensionId, null) as EntityRelationItem;
			r.Name = relatedEntity.Name;
			r.RelatedEntity = relatedEntity;
			r.IsParentChild = masterDetail;

			if(persist) r.Persist();

			return r;
		}

		public static EntityRelationColumnPairItem CreateRelationKey(EntityRelationItem relation, IDataEntityColumn baseField, IDataEntityColumn relatedField, bool persist)
		{
			return CreateRelationKey(relation,baseField,relatedField,persist,null);
		}

		public static EntityRelationColumnPairItem CreateRelationKey(EntityRelationItem relation, IDataEntityColumn baseField, IDataEntityColumn relatedField, bool persist,string NameOfKey)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			EntityRelationColumnPairItem key = relation.NewItem(typeof(EntityRelationColumnPairItem), schema.ActiveSchemaExtensionId, null) as EntityRelationColumnPairItem;
			key.BaseEntityField = baseField;
			key.RelatedEntityField = relatedField;
			if(!string.IsNullOrEmpty(NameOfKey))
            {
				key.Name = NameOfKey;
			}
			if(persist) key.Persist();

			return key;
		}

		public static DataStructureReference CreateDataStructureReference(AbstractSchemaItem parentItem, DataStructure structure, DataStructureMethod method, DataStructureSortSet sortSet, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			DataStructureReference dsr = parentItem.NewItem(typeof(DataStructureReference), schema.ActiveSchemaExtensionId, null) as DataStructureReference;
			dsr.DataStructure = structure;
			dsr.Method = method;
			dsr.SortSet = sortSet;

			if(persist) dsr.Persist();

			return dsr;
		}

		public static SchemaItemGroup GetDataStructureGroup(string name)
		{
			ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
			DataStructureSchemaItemProvider dsprovider = schema.GetProvider(
                typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
			return dsprovider.GetGroup(name);
		}

        public static SchemaItemGroup GetDataConstantGroup(string name)
        {
            ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
            DataConstantSchemaItemProvider dsprovider = schema.GetProvider(
                typeof(DataConstantSchemaItemProvider)) as DataConstantSchemaItemProvider;
            return dsprovider.GetGroup(name);
        }

        public static EntityFilter CreateFilter(IDataEntityColumn field, string functionName,
            string filterPrefix, bool createParameter)
        {
            return CreateFilter(field, functionName, filterPrefix, createParameter,
                null);
        }

        public static EntityFilter CreateFilter(IDataEntityColumn field, string functionName, 
            string filterPrefix, bool createParameter, IList<AbstractSchemaItem> generatedElements)
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
	
		private static EntityFilter CreateFilter(IDataEntityColumn field, string functionName, string filterPrefix, 
			bool createParameter, string leftName, string rightName, bool isRightArray, 
            IList<AbstractSchemaItem> generatedElements)
		{
		    if (field.Name == null) throw new ArgumentException("Filed Name is not set.");
            ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
			IDataEntity entity = field.ParentItem as IDataEntity;
            // filter
			EntityFilter filter = entity.NewItem(
                typeof(EntityFilter), schema.ActiveSchemaExtensionId, null) as EntityFilter;
			filter.Name = filterPrefix 
                + (field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name);
			if(isRightArray)
			{
				filter.Name += "List";
			}
			filter.Persist();
            if (generatedElements != null) generatedElements.Add(filter);
            // function call
			FunctionCall call = filter.NewItem(
                typeof(FunctionCall), schema.ActiveSchemaExtensionId, null) as FunctionCall;
			FunctionSchemaItemProvider functionProvider = 
                schema.GetProvider(typeof(FunctionSchemaItemProvider)) as FunctionSchemaItemProvider;
			Function function = (Function)functionProvider.GetChildByName(functionName, Function.CategoryConst);
			if(function == null) throw new Exception(functionName + " function not found. Cannot create filter.");
			call.Function = function;
			call.Name = functionName;
			call.Persist();
            // field reference
			EntityColumnReference reference1 = 
                call.GetChildByName(leftName).NewItem(
                    typeof(EntityColumnReference), 
                    schema.ActiveSchemaExtensionId, null) as EntityColumnReference;
			reference1.Field = field;
			reference1.Persist();
            // parameter
			if(createParameter)
			{
				DatabaseParameter param = entity.NewItem(
                    typeof(DatabaseParameter), 
                    schema.ActiveSchemaExtensionId, null) as DatabaseParameter;
				if(isRightArray)
				{
					param.DataType = OrigamDataType.Array;
				}
				else
				{
					param.DataType = field.DataType;
                    IDatabaseDataTypeMapping mappableDataType = 
                        field as IDatabaseDataTypeMapping;
                    if (mappableDataType != null)
                    {
                        param.MappedDataType = mappableDataType.MappedDataType;
                    }
                    param.DataLength = field.DataLength;
				}
				param.Name = "par" + (field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name);
				if(isRightArray)
				{
					param.Name += "List";
				}
				param.Persist();
				ParameterReference reference2 = call.GetChildByName(rightName).NewItem(
                    typeof(ParameterReference), schema.ActiveSchemaExtensionId, null) as ParameterReference;
				reference2.Parameter = param;
				reference2.Persist();
                if (generatedElements != null) generatedElements.Add(reference2);
            }
            return filter;
        }

        public static TableMappingItem CreateLanguageTranslationChildEntity(
            TableMappingItem parentEntity, ICollection selectedFields)
        {
            return CreateLanguageTranslationChildEntity(parentEntity, selectedFields, null);
        }

        public static TableMappingItem CreateLanguageTranslationChildEntity(
            TableMappingItem parentEntity, ICollection selectedFields,
            IList<AbstractSchemaItem> generatedElements)
		{			
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			// create child entity (based on IOrigamEntity2)
			TableMappingItem newEntity = EntityHelper.CreateTable(String.Format("{0}_l10n", parentEntity.Name), parentEntity.Group, false);
		    string entityCaption = string.IsNullOrEmpty(parentEntity.Caption)
		        ? parentEntity.Name
		        : parentEntity.Caption;
		    newEntity.Caption = String.Format("{0} Localization", entityCaption);
            if (generatedElements != null) generatedElements.Add(newEntity);
			// create unique composite index on foreign key to parent entity and reference to language
			DataEntityIndex index = newEntity.NewItem(typeof(DataEntityIndex), schema.ActiveSchemaExtensionId, null) as DataEntityIndex;
			index.Name = "ix_unq_" + parentEntity.Name;
			index.IsUnique = true;
			index.Persist();
		    newEntity.Persist();
            // Create relation from the parent entity
            EntityRelationItem parentRelation = EntityHelper.CreateRelation(parentEntity, newEntity, true, true);
			parentEntity.LocalizationRelation = parentRelation;
            if (generatedElements != null) generatedElements.Add(parentRelation);
            EntityRelationItem parentRelationAll = EntityHelper.CreateRelation(parentEntity, newEntity, true, false);
			parentRelationAll.Name = parentRelationAll.Name + "_all";
			parentRelationAll.Persist();
            if (generatedElements != null) generatedElements.Add(parentRelationAll);
            ArrayList indexColumns = new ArrayList();
			// Create reference columns
			foreach (IDataEntityColumn pk in parentEntity.EntityPrimaryKey)
			{
				if (!pk.ExcludeFromAllFields)
				{
					FieldMappingItem refParentColumn = EntityHelper.CreateColumn(newEntity, "ref" + parentEntity.Name + pk.Name, false, pk.DataType, pk.DataLength, parentEntity.Caption, parentEntity, pk, true);
					EntityRelationColumnPairItem key = EntityHelper.CreateRelationKey(parentRelation, pk, refParentColumn, true);
					EntityRelationColumnPairItem keyAll = EntityHelper.CreateRelationKey(parentRelationAll, pk, refParentColumn, true);
					indexColumns.Add(refParentColumn);
				}
			}
			// get language entity
			IPersistenceService persistenceService = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			// get ILocalization
			ModelElementKey ILocalizationKey = new ModelElementKey(ILocalizationEntityId);
			IDataEntity ILocalization = (IDataEntity)persistenceService.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), ILocalizationKey);
			// and set ancestor
			EntityHelper.AddAncestor(newEntity, ILocalization, true);
			// use language filter in parent relation
			EntityRelationFilter relationFilter = parentRelation.NewItem(typeof(EntityRelationFilter), schema.ActiveSchemaExtensionId, null) as EntityRelationFilter;
			relationFilter.Filter = ILocalization.GetChildById(GetByLanguageIdFilterId) as EntityFilter;
			relationFilter.Persist();	
			// add refLanguageId to unique index
			indexColumns.Add(ILocalization.GetChildById(refLanguageIdColymId) as FieldMappingItem);
			// create index items
			int i = 0;
			foreach (IDataEntityColumn col in indexColumns)
			{
				DataEntityIndexField field = index.NewItem(typeof(DataEntityIndexField), schema.ActiveSchemaExtensionId, null) as DataEntityIndexField;
				field.Field = col;
				field.OrdinalPosition = i;
				field.Persist();

				i++;
			}
			// create selected fields
			foreach (IDataEntityColumn col in selectedFields)
			{
				EntityHelper.CreateColumn(newEntity, col.Name, col.AllowNulls, col.DataType, col.DataLength, col.Caption, null, null, true);
			}
			newEntity.Persist();
			parentEntity.Persist();
			return newEntity;
		}

        public static DataEntityIndex CreateIndex(IDataEntity entity, 
            IDataEntityColumn field, bool unique, bool persist)
        {
            ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
            DataEntityIndex result = (entity as AbstractSchemaItem).NewItem(
                typeof(DataEntityIndex), schema.ActiveSchemaExtensionId, null)
                as DataEntityIndex;
            result.Name = "ix" + (unique ? "Unique": "") + field.Name;
            result.IsUnique = unique;
            if (persist) result.Persist();
            CreateIndexField(result, field, persist);
            return result;
        }

        public static DataEntityIndexField CreateIndexField(
            DataEntityIndex index, IDataEntityColumn field, bool persist)
        {
            ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
            DataEntityIndexField result = index.NewItem(typeof(
                DataEntityIndexField), schema.ActiveSchemaExtensionId, null) 
                as DataEntityIndexField;
            result.Field = field;
            if (persist) result.Persist();
            return result;
        }
    }
}
