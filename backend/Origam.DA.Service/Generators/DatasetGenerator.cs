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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;
using Origam.Schema.WorkflowModel;

namespace Origam.DA.Service;
/// <summary>
/// Summary description for DatasetGenerator.
/// </summary>
public class DatasetGenerator
{
	private IParameterService _parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
//      removed to Da.Common Const.
//		const string DefaultHideUIAttribute = "DefaultHideUI";
//		const string DefaultLookupIdAttribute = "DefaultLookupId";
	public DatasetGenerator(bool userDefinedParameters)
	{
		this.UserDefinedParameters = userDefinedParameters;
	}
	private bool _userDefinedParameters = false;
	public bool UserDefinedParameters
	{
		get
		{
			return _userDefinedParameters;
		}
		set
		{
			_userDefinedParameters = value;
		}
	}
	private void AddExtendedProperties(PropertyCollection collection, Key primaryKey)
	{
		foreach(DictionaryEntry entry in primaryKey)
			collection.Add(entry.Key, entry.Value);
	}
	/* @short Evaluates parameter value. Firstly look into custom parameters
	 * (e.g. parCurrentOrganizationId, etc.) and if not found there look at
	 *  the incoming parameter. If found, returns a parameter value otherwise
	 * returns null.		 
	 * */
	private static object EvaluateParameters(System.Data.DataColumn c,
											 object param,
											 UserProfile profile,
											 IDictionary parameters)
	{
		// try custom parameters first
		ICustomParameter customParameter =
				CustomParameterService.MatchParameter((string) param);
		if (customParameter != null)
		{
			object val = customParameter.Evaluate(profile);
			if (c.DataType == typeof(string) && val != null)
			{
				return val.ToString();
			}
			else
			{
				return val;
			}
		}
		// try ordinary input parameters
		if (parameters.Contains(param))
		{
			object val = parameters[param];
			if(c.DataType == typeof(string) && val != null)
			{
				return val.ToString();
			}
			else
			{
				return val;
			}
		}
		return null;
	}
	public static void ApplyDynamicDefaults(DataSet data, IDictionary parameters)
	{
        UserProfile profile = SecurityManager.CurrentUserProfile();
		foreach(DataTable t in data.Tables)
		{
			foreach(DataColumn c in t.Columns)
			{
				if(c.ExtendedProperties.Contains("DynamicEntityDefaultParameter"))
				{
					// defaultValue parameter name defined on entity level
					object entityParam = c.ExtendedProperties["DynamicEntityDefaultParameter"];
					c.DefaultValue = EvaluateParameters(c, entityParam, profile, parameters);
				}
				if (c.ExtendedProperties.Contains("DynamicDefaultParameter"))
				{
					// defaultValue parameter name defined on datastructure default set level
					// (overrides entity-level settings)
					object param = c.ExtendedProperties["DynamicDefaultParameter"];
					c.DefaultValue = EvaluateParameters(c, param, profile, parameters);
				}
			}
		}
	}
	public DataSet CreateDataSet(IDataEntity entity)
	{
		return CreateDataSet(entity, null);
	}

	public DataSet CreateDataSet(IDataEntity entity, CultureInfo culture)
	{
		DataSet dataset = new DataSet(dataSetName);
        if (culture == null)
		{
			culture = System.Globalization.CultureInfo.CurrentUICulture;
		}
		dataset.Locale = culture;
        AddTableFromEntity(entity, entity.Name, dataset);
		return dataset;
	}
    private void AddTableFromEntity(IDataEntity entity, string name, DataSet dataset)
    {
        ArrayList arrayTypeEntities = new ArrayList();
        // Add the table
        DataTable table = new OrigamDataTable(name);
        table.Locale = dataset.Locale;
        AddTableExtendedProperties(table, entity);
        // Add columns
        foreach (IDataEntityColumn column in entity.EntityColumns)
        {
            if (!column.ExcludeFromAllFields)
            {
                DataColumn tableColumn = new DataColumn(
                    column.Name,
                    ConvertDataType(column.DataType)
                    );
                AddColumnExtendedProperties(tableColumn, column, entity,
                    column.DefaultLookup);
                tableColumn.Caption = GetCaption(column);
                tableColumn.AllowDBNull = column.AllowNulls;
                if (column.DataType == OrigamDataType.Boolean)
                {
                    tableColumn.DefaultValue = false;
                }
                switch (column.XmlMappingType)
                {
                    case EntityColumnXmlMapping.Element:
                        tableColumn.ColumnMapping = MappingType.Element;
                        break;
                    case EntityColumnXmlMapping.Attribute:
                        tableColumn.ColumnMapping = MappingType.Attribute;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("XmlMappingType", column.XmlMappingType, ResourceUtils.GetString("UnknownXmlMapping"));
                }
                if (column.DefaultValue != null)
                {
                    tableColumn.DefaultValue = RenderDefaultValue(column.DefaultValue);
                }
                if (column.DataType == OrigamDataType.String)
                {
                    tableColumn.MaxLength = column.DataLength;
                }
                DetachedField detachedField = column as DetachedField;
                if (detachedField != null && detachedField.DataType == OrigamDataType.Array)
                {
                    string relationName = detachedField.ArrayRelation.Name;
                    tableColumn.ExtendedProperties.Add(Const.ArrayRelation, relationName);
                    tableColumn.ExtendedProperties.Add(Const.ArrayRelationField, detachedField.ArrayValueField.Name);
                    if (!arrayTypeEntities.Contains(relationName))
                    {
                        arrayTypeEntities.Add(relationName);
                    }
                }
                table.Columns.Add(tableColumn);
                if (column.IsPrimaryKey)
                {
                    AddPrimaryKey(table, column, tableColumn);
                }
            }
        }
        dataset.Tables.Add(table);
        // add children tables for array-type relationships
        foreach (IAssociation relation in entity.EntityRelations)
        {
            if (arrayTypeEntities.Contains(relation.Name))
            {
                AddTableFromEntity(relation.AssociatedEntity, relation.Name, dataset);
                AddRelation(dataset, entity.Name, relation, entity.Name, relation.Name, relation.Name);
            }
        }
    }
    private static void AddPrimaryKey(DataTable table, IDataEntityColumn column, DataColumn tableColumn)
    {
        //PrimaryKey
        int oldCount = table.PrimaryKey.Length;
        DataColumn[] keys = new DataColumn[++oldCount];
        table.PrimaryKey.CopyTo(keys, 0);
        keys.SetValue(tableColumn, --oldCount);
        table.Constraints.Add("PK_" + column.Name, keys, true);
        //tableColumn.Unique = true;
    }
    public DataSet CreateDataSet(DataStructure ds)
	{
		return this.CreateDataSet(ds, true);
	}
	public DataSet CreateDataSet(DataStructure ds, CultureInfo culture)
	{
		return this.CreateDataSet(ds, true, null, culture);
	}
	public DataSet CreateDataSet(DataStructure ds, DataStructureDefaultSet defaultSet)
	{
		return this.CreateDataSet(ds, true, defaultSet);
	}
	public DataSet CreateDataSet(DataStructure ds, bool includeCalculatedColumns)
	{
		return this.CreateDataSet(ds, true, null);
	}
	private Hashtable GetCache()
	{
		Hashtable context = OrigamUserContext.Context;
		if(! context.Contains("DataStructureCache"))
		{
			context.Add("DataStructureCache", new Hashtable());
		}
		return (Hashtable)OrigamUserContext.Context["DataStructureCache"];
	}
	public DataSet CreateDataSet(DataStructure ds, bool includeCalculatedColumns, DataStructureDefaultSet defaultSet)
	{
		return CreateDataSet(ds, includeCalculatedColumns, defaultSet, null);
	}
	public DataSet CreateDataSet(DataStructure ds, bool includeCalculatedColumns,
		DataStructureDefaultSet defaultSet, CultureInfo culture)
	{
		return CreateDataSet(ds, includeCalculatedColumns, defaultSet, culture, false);
	}
	private DataSet CreateDataSet(DataStructure ds, bool includeCalculatedColumns,
		DataStructureDefaultSet defaultSet, CultureInfo culture, bool forceBuildFromDatastructure)
	{
		if(ds == null) throw new ArgumentNullException("ds", ResourceUtils.GetString("ErrorDatastructureNull"));
		Hashtable dsCache = GetCache();
		string cacheKey = ds.Id.ToString() + (defaultSet == null ? Guid.Empty.ToString() : defaultSet.Id.ToString());
		// get strongly typed dataset class
		if (ds.DataSetClass != null && ds.DataSetClass.Trim() != "" && !forceBuildFromDatastructure)
		{
			// get DS from strongly typed dataset class
			string[] splittedDataSetClass = ds.DataSetClass.Split(',');
            if(splittedDataSetClass.Length < 2)
            {
                throw new ArgumentException(
                    ResourceUtils.GetString("ErrorDataSetClassInvalid"), 
                    "DataSetClass");
            }
			DataSet outDS = Reflector.InvokeObject(splittedDataSetClass[0].Trim(), splittedDataSetClass[1].Trim()) as DataSet;
			outDS.DataSetName = dataSetName;
			// get DS generated from datastructure
			// try to get from cache
			if (dsCache.Contains(cacheKey) && includeCalculatedColumns)
			{
				DataSet cachedDS = dsCache[cacheKey] as DataSet;
				return CopyPropertiesToTypedOneDataSet(cachedDS, outDS);
			}
			DataSet generatedDS = CreateDataSet(ds, includeCalculatedColumns, defaultSet, culture, true);
			return CopyPropertiesToTypedOneDataSet(generatedDS, outDS);
		}
		if(dsCache.Contains(cacheKey) & includeCalculatedColumns)
		{
			DataSet clone = DatasetTools.CloneDataSet((dsCache[cacheKey] as DataSet));
			return clone;
		}
		DataSet dataset = new DataSet(dataSetName);
		StateMachineSchemaItemProvider states = (ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService).GetProvider(typeof(StateMachineSchemaItemProvider)) as StateMachineSchemaItemProvider;
		//dataset.BeginInit();
		if(culture == null)
		{
			culture = CultureInfo.CurrentUICulture;
		}
		dataset.Locale = culture;
		AddExtendedProperties(dataset.ExtendedProperties, ds.PrimaryKey);
		//dataset.EndInit();
		List<DataStructureEntity> entities = ds.Entities;
		ArrayList aggregatedColumns = new ArrayList();
		// Add tables
		foreach(DataStructureEntity entity in entities)
		{
			// Skip self joins - they will be added only as relations
			if(!(entity.Entity is IAssociation && (entity.Entity as IAssociation).IsSelfJoin) && entity.Columns.Count > 0)
			{
				ArrayList nonPhysicalColumns = new ArrayList();
				DataTable table = new OrigamDataTable(entity.Name);
				//table.BeginInit();
				table.Locale = dataset.Locale;
				AddTableExtendedProperties(table, entity);
				// Set caption
				if(entity.Caption == null || entity.Caption == "")
				{
					if(entity.EntityDefinition.Caption == null || entity.EntityDefinition.Caption == "")
					{
						table.DisplayExpression = DatasetTools.TextExpression(entity.Name);
					}
					else
					{
						table.DisplayExpression = DatasetTools.TextExpression(entity.EntityDefinition.Caption);
					}
				}
				else
					table.DisplayExpression = DatasetTools.TextExpression(entity.Caption);
				//table.EndInit();
				// Add columns
				foreach(DataStructureColumn column in entity.Columns)
				{
					bool isCalculated = false;
					DataStructureColumn finalColumn = column.FinalColumn;
					OrigamDataType finalDataType = column.DataType;
					DataColumn tableColumn = new DataColumn(
						column.Name,
						ConvertDataType(finalDataType)
						);
					tableColumn.ExtendedProperties.Add(Const.OrigamDataType, finalDataType);
					tableColumn.ExtendedProperties.Add(Const.FieldId, finalColumn.Id);
					switch(column.Field)
					{							
						// mark column as a database field if the entity column
						// is of type FieldMappingItem and the datastructure column
						// doesn't have 'UseLookupValue' set (read-only datastructure lookups
						// aren't saveable so they aren't actually database fields)
						case FieldMappingItem _:
							tableColumn.ExtendedProperties.Add(
								Const.IsDatabaseField, !column.UseLookupValue);
							break;
						case DetachedField detachedField 
							when detachedField.DataType == OrigamDataType.Array:
						{
							var relatedEntity = LookupRelation(
								entity, detachedField.ArrayRelation);
							if(relatedEntity == null)
							{
								throw new Exception(
									"Data Structure does not contain entity " + 
									detachedField.ArrayRelation.Name + 
									" that is required for creating an Array field " 
									+ detachedField.Path);
							}
							tableColumn.ExtendedProperties.Add(
								Const.ArrayRelation, relatedEntity.Name);
							tableColumn.ExtendedProperties.Add(
									Const.ArrayRelationField, 
									LookupColumnName(relatedEntity, detachedField.ArrayValueField));
							aggregatedColumns.Add(column);
							break;
						}
					}
					// Set caption
					if(column.Caption == null || column.Caption == "")
					{
						tableColumn.Caption = GetCaption(column.Field);
					}
					else
					{
						tableColumn.Caption = column.Caption;
					}
				
					if(finalDataType == OrigamDataType.Boolean)
					{
						tableColumn.DefaultValue = false;
					}
					if(column.IsWriteOnly)
					{
						tableColumn.ExtendedProperties.Add(Const.IsWriteOnlyAttribute, true);
					}
					// Skip allow nulls on looked-up columns. With these it is never clear, if we get a value or not
					// The same e.g. with aliased primary key 
					if(column.UseLookupValue || column.UseCopiedValue
						|| column.Entity != null || column.IsWriteOnly)
					{
						tableColumn.AllowDBNull = true;
					}
					else
					{
						tableColumn.AllowDBNull = finalColumn.Field.AllowNulls;
						if(! finalColumn.Field.AllowNulls)
						{
							tableColumn.ExtendedProperties.Add("AllowNulls", false);
						}
					}
					//tableColumn.ReadOnly = (entity.EntityDefinition.EntityIsReadOnly | column.UseLookupValue) ? true : column.Field.ReadOnly;
					tableColumn.AutoIncrement = finalColumn.Field.AutoIncrement;
					tableColumn.AutoIncrementSeed = finalColumn.Field.AutoIncrementSeed;
					tableColumn.AutoIncrementStep = finalColumn.Field.AutoIncrementStep;
					switch(column.XmlMappingType)
					{
						case DataStructureColumnXmlMappingType.Default:
							switch(column.Field.XmlMappingType)
							{
								case EntityColumnXmlMapping.Element:
									tableColumn.ColumnMapping = MappingType.Element;
									break;
								case EntityColumnXmlMapping.Attribute:
									tableColumn.ColumnMapping = MappingType.Attribute;
									break;
								case EntityColumnXmlMapping.Hidden:
									tableColumn.ColumnMapping = MappingType.Hidden;
									break;
								default:
									throw new ArgumentOutOfRangeException("XmlMappingType", finalColumn.Field.XmlMappingType, "Unknown XmlMappingType");
							}
							break;
						case DataStructureColumnXmlMappingType.Attribute:
							tableColumn.ColumnMapping = MappingType.Attribute;
							break;
						case DataStructureColumnXmlMappingType.Element:
							tableColumn.ColumnMapping = MappingType.Element;
							break;
						default:
							throw new ArgumentOutOfRangeException("XmlMappingType", finalColumn.XmlMappingType, "Unknown XmlMappingType");
					}
					if (column.HideInOutput)
					{
						tableColumn.ColumnMapping = MappingType.Hidden;
					}
					tableColumn.ExtendedProperties.Add("OnCopyAction", finalColumn.Field.OnCopyAction);
                    // auditing exclusion
                    AbstractDataEntityColumn dataField = column.Field as AbstractDataEntityColumn;
                    EntityAuditingType auditing = entity.EntityDefinition.AuditingType;
                    if (dataField != null && dataField.ExcludeFromAuditing)
                    {
                        auditing = EntityAuditingType.None;
                    }
                    tableColumn.ExtendedProperties.Add(Const.EntityAuditingAttribute, auditing);
					StateMachine sm = null;
					if(states != null)
					{
						sm = states.GetMachine((Guid)entity.EntityDefinition.PrimaryKey["Id"], (Guid)finalColumn.Field.PrimaryKey["Id"]);
					}
					if(sm == null)
					{
						SetDefault(defaultSet, finalColumn, entity, tableColumn);
					}
					else
					{
						// we have a state machine, we use its initial state as default value
						object[] initStates = sm.InitialStateValues(null);
//							if(initStates.Length == 0)
//							{
//								throw new ArgumentNullException("InitialState", ResourceUtils.GetString("ErrorNoInitialState", entity.Name, tableColumn.ColumnName));
//							}
						if(initStates.Length == 1)
						{
							tableColumn.DefaultValue = initStates[0];
						}
						else
						{
							// more than 1 initial state - we have to use the default, otherwise no default value
							SetDefault(defaultSet, finalColumn, entity, tableColumn);
						}
						tableColumn.ExtendedProperties.Add("IsState", true);
					}
					if(finalDataType == OrigamDataType.String)
					{
						tableColumn.MaxLength = finalColumn.Field.DataLength;
					}
					
					if(column.UseCopiedValue)
					{
						AddExtendedProperties(tableColumn.ExtendedProperties, 
                            new ModelElementKey(Guid.Empty));
						tableColumn.ExtendedProperties.Add(Const.OriginalFieldId, column.Field.PrimaryKey["Id"]);
						tableColumn.ExtendedProperties.Add(Const.IsAddittionalFieldAttribute, true);
					}
					else if(column.UseLookupValue)
					{
						// Save the original field and lookup for the rule engine so it can refresh the data
						// when the original field value changes.
						tableColumn.ExtendedProperties.Add(Const.OriginalLookupIdAttribute, column.FinalLookup.PrimaryKey["Id"]);
						
						LookupField lookupField = column.Field as LookupField;
						if(lookupField != null)
						{
							AddExtendedProperties(tableColumn.ExtendedProperties, column.Field.PrimaryKey);
							tableColumn.ExtendedProperties.Add(Const.OriginalFieldId, lookupField.FieldId);
						}
						else
						{
							AddExtendedProperties(tableColumn.ExtendedProperties, finalColumn.PrimaryKey);
							tableColumn.ExtendedProperties.Add(Const.OriginalFieldId, column.Field.PrimaryKey["Id"]);
							tableColumn.ExtendedProperties.Add(Const.IsAddittionalFieldAttribute, true);
						}
					}
					else
					{
						AddExtendedProperties(tableColumn.ExtendedProperties, finalColumn.Field.PrimaryKey);
						// Put default lookup to the extended properties, so it can be mapped
						// automatically in the designer to the combo box.
						if(column.FinalLookup != null)
						{
							tableColumn.ExtendedProperties.Add(Const.DefaultLookupIdAttribute, column.FinalLookup.PrimaryKey["Id"]);
						}
						FunctionCall functionCall = column.Field as FunctionCall;
						if(column.Entity == null && functionCall != null && functionCall.Function.FunctionType == OrigamFunctionType.Standard
								&& functionCall.ForceDatabaseCalculation == false && column.Aggregation == AggregationType.None)
						{
							isCalculated = true;
							if(includeCalculatedColumns)
							{
								nonPhysicalColumns.Add(column);
							}
						}
						if(column.Field is AggregatedColumn 
							||
							column.IsFromParentEntity())
						{
							isCalculated = true;
							if(includeCalculatedColumns)
							{
								aggregatedColumns.Add(column);
							}
						}
					}
					if(includeCalculatedColumns || (includeCalculatedColumns == false && isCalculated == false))
					{
						table.Columns.Add(tableColumn);
					}
					// mark a primary key only for the physical fields from the original entity
					if(column.Field.IsPrimaryKey && column.UseLookupValue == false && column.UseCopiedValue == false
						&& column.IsWriteOnly == false && column.Entity == null && column.Aggregation == AggregationType.None)
					{
						bool columnInPkAlready = DatasetTools.IsAliasedColumn(table.PrimaryKey,  (Guid)column.Field.PrimaryKey["Id"]);
						if(! columnInPkAlready)
						{
							//PrimaryKey
							int oldCount = table.PrimaryKey.Length;
							DataColumn[] keys = new DataColumn[++oldCount];
							table.PrimaryKey.CopyTo(keys, 0);
							keys.SetValue(tableColumn, --oldCount);
							table.Constraints.Add("PK_" + column.Name, keys, true);
							//tableColumn.Unique = true;
						}
					}
				}
				foreach(DataStructureColumn column in nonPhysicalColumns)
				{
					try {
						table.Columns[column.Name].Expression = RenderExpression(column.Field, entity);
					} catch (Exception e)
					{
						throw new OrigamException(string.Format("Error occured while rendering column `{0}', ({1}) - {2}",
							(column as AbstractSchemaItem), (column as AbstractSchemaItem).Id, e.Message), e);
					}
				}
				dataset.Tables.Add(table);
			}
		}
		AddQueryRelations(dataset, ds);
		foreach(DataStructureColumn column in aggregatedColumns)
		{
			DataStructureEntity entity = column.ParentItem as DataStructureEntity;
			if(column.IsFromParentEntity())
			{
				dataset.Tables[entity.Name].Columns[column.Name].Expression = RenderParentField(column);
			}
			else
			{
				dataset.Tables[entity.Name].Columns[column.Name].Expression = RenderExpression(column.Field, entity);
			}
            if(!dataset.Tables[entity.Name].ExtendedProperties.ContainsKey(Const.HasAggregation))
            {
                dataset.Tables[entity.Name].ExtendedProperties.Add(Const.HasAggregation, true);
            }
        }
		if(includeCalculatedColumns)
		{
			lock(dsCache)
			{
                DataSet clone = DatasetTools.CloneDataSet(dataset);
                if (!dsCache.Contains(cacheKey))
				{
					dsCache.Add(cacheKey, dataset);
				}
                return clone;
            }
		}
		return dataset;
	}
	private DataSet CopyPropertiesToTypedOneDataSet(DataSet origamDS, DataSet typedDS)
	{
		foreach (DataTable origamTable in origamDS.Tables)
        {
			// find table in typedDS
			DataTable typedTable = typedDS.Tables[origamTable.TableName];
			if (typedTable == null)
			{
				continue;
			}
			// process table extended properties
			typedTable.ExtendedProperties.Clear();
			foreach (DictionaryEntry item in origamTable.ExtendedProperties)
			{
				typedTable.ExtendedProperties.Add(item.Key, item.Value);
			}
			// process columns				
			foreach (DataColumn origamColumn in origamTable.Columns)
			{
				// find column in typed DS
				DataColumn typedColumn = typedTable.Columns[origamColumn.ColumnName];
				if (typedColumn == null)
				{
					continue;
				}
				// process column extended properties and default values
				typedColumn.ExtendedProperties.Clear();
				foreach (DictionaryEntry item in origamColumn.ExtendedProperties)
				{
					typedColumn.ExtendedProperties.Add(item.Key, item.Value);
					typedColumn.DefaultValue = origamColumn.DefaultValue;
				}
			}
		}		
		return typedDS;			
	}
	private void SetDefault(DataStructureDefaultSet defaultSet, DataStructureColumn finalColumn, DataStructureEntity entity, DataColumn column)
	{
		// mark the dynamic default parameter form entity-level-defined 'DefaultValueParameter'
		if (finalColumn.Field.DefaultValueParameter != null)
		{
			column.ExtendedProperties.Add("DynamicEntityDefaultParameter",
				finalColumn.Field.DefaultValueParameter.Name);
		}
		// check if the default set has a default for this column
		if(defaultSet != null)
		{
			foreach(DataStructureDefaultSetDefault d in defaultSet.ChildItemsByType(DataStructureDefaultSetDefault.CategoryConst))
			{
				if(d.Field.PrimaryKey.Equals(finalColumn.Field.PrimaryKey) & d.Entity.PrimaryKey.Equals(entity.PrimaryKey))
				{
					if(d.Default != null)
					{
						column.DefaultValue = RenderDefaultValue(d.Default);
					}
					// mark the dynamic default parameter
					if(d.Parameter != null)
					{
						column.ExtendedProperties.Add("DynamicDefaultParameter", d.Parameter.Name);
					}
					
					return;
				}
			}
		}
		// no state machine and no default from the default set, we use the default value from the entity
		if(finalColumn.Field.DefaultValue != null)
		{
			column.DefaultValue = RenderDefaultValue(finalColumn.Field.DefaultValue);
		}
	}
	public DataSet CreateUpdateFieldDataSet(TableMappingItem table, FieldMappingItem field)
	{
		DataSet ds = new DataSet(dataSetName);
		ds.Locale = System.Globalization.CultureInfo.CurrentUICulture;
		DataTable dt = ds.Tables.Add(table.Name);
		dt.Locale = ds.Locale;
		AddTableExtendedProperties(dt, table);
		foreach(IDataEntityColumn col in table.EntityPrimaryKey)
		{
			DataColumn dtc = dt.Columns.Add(col.Name, ConvertDataType(col.DataType));
			AddColumnExtendedProperties(dtc, col, table, col.DefaultLookup);
            AddPrimaryKey(dt, col, dtc);
		}
		DataColumn fieldDtc = dt.Columns.Add(field.Name, ConvertDataType(field.DataType));
		AddColumnExtendedProperties(fieldDtc, field, table, field.DefaultLookup);
		return ds;
	}
	private string GetCaption(IDataEntityColumn column)
	{
		string result;
		// if there is just 1 dynamic label and it is actually static (never changes)
		// we take it as the default label so it can be used also in empty form or grid column
		// label
		List<ISchemaItem> dynamicLabels = column.ChildItemsByType(EntityFieldDynamicLabel.CategoryConst);
		EntityFieldDynamicLabel dynamicLabel = null;
		if(dynamicLabels.Count == 1)
		{
			EntityFieldDynamicLabel lbl = dynamicLabels[0] as EntityFieldDynamicLabel;
			if(lbl.IsStatic)
			{
				dynamicLabel = lbl;
			}
		}
		if(dynamicLabel != null)
		{
			result = (string)_parameterService.GetParameterValue(dynamicLabel.LabelConstantId, OrigamDataType.String);
		}
		else if(column.Caption == null || column.Caption == "")
		{
			result = column.Name;
		}
		else if(IsCaptionExpression(column.Caption))
		{
			// expression
			result = EvaluateCaptionExpression(column.Caption);
		}
		else
		{
			result = column.Caption;
		}
		return result;
	}
	public static bool IsCaptionExpression(string caption)
	{
		return caption.StartsWith("{") && caption.EndsWith("}");
	}
	public static string EvaluateCaptionExpression(string caption)
	{
		string expression = caption.Substring(1, caption.Length - 2);
		IRuleEngineService re = ServiceManager.Services.GetService(typeof(IRuleEngineService)) as IRuleEngineService;
		return re.EvaluateExpression(expression);
	}
	private void AddTableExtendedProperties(DataTable table, DataStructureEntity entity)
	{
		AddExtendedProperties(table.ExtendedProperties, entity.PrimaryKey);
		AddTableExtendedProperties(table, entity.EntityDefinition);
	}
	private void AddTableExtendedProperties(DataTable table, IDataEntity entity)
	{
		table.ExtendedProperties.Add("EntityId", entity.PrimaryKey["Id"]);
		table.ExtendedProperties.Add(Const.EntityAuditingAttribute, entity.AuditingType);
        table.ExtendedProperties.Add(
            Const.AuditingSecondReferenceKeyColumnAttribute,
            entity.AuditingSecondReferenceKeyColumn);
		if(entity.DescribingField != null)
		{
			table.ExtendedProperties.Add(Const.DescribingField, entity.DescribingField.Name);
		}
	}
	private void AddColumnExtendedProperties(DataColumn tableColumn, 
        IDataEntityColumn column, IDataEntity entity, IDataLookup dataLookup)
	{
		tableColumn.ExtendedProperties.Add(Const.OrigamDataType, column.DataType);
		AddExtendedProperties(tableColumn.ExtendedProperties, column.PrimaryKey);
		if(! column.AllowNulls)
		{
			tableColumn.ExtendedProperties.Add("AllowNulls", false);
		}
		tableColumn.ExtendedProperties.Add("OnCopyAction", column.OnCopyAction);
		// Put default lookup to the extended properties, so it can be mapped
		// automatically in the designer to the combo box.
		if(dataLookup != null)
		{
			tableColumn.ExtendedProperties.Add(Const.DefaultLookupIdAttribute, dataLookup.PrimaryKey["Id"]);
		}
        // auditing exclusion
        EntityAuditingType auditing = !(column is DetachedField)
            ? entity.AuditingType : EntityAuditingType.None;
        if (column is FieldMappingItem databaseField)
        {
			tableColumn.ExtendedProperties.Add(Const.IsDatabaseField, true);
            auditing = !databaseField.ExcludeFromAuditing ?
                entity.AuditingType : EntityAuditingType.None;
        }
        tableColumn.ExtendedProperties.Add(Const.EntityAuditingAttribute, auditing);
	}
	private void AddQueryRelations(DataSet dataset, DataStructure ds)
	{
		dataset.Relations.Clear();
		List<DataStructureEntity> entities = ds.Entities;
        string debugPath = ds.Path;
		foreach(DataStructureEntity entity in entities)
		{
			IAssociation assoc = entity.Entity as IAssociation;
			if(assoc != null && entity.Columns.Count > 0)
			{
				if(assoc.IsOR) throw new InvalidOperationException("IsOR = true not supported for data structures, only for data filtering.");
                string baseEntityName = entity.ParentItem.Name;
                string relatedEntityName = entity.Name;
                string relationName = entity.Name;
                relatedEntityName = AddRelation(dataset, debugPath, assoc, baseEntityName, relatedEntityName, relationName);
			}
		}	
	}
    private static string AddRelation(DataSet dataset, string debugPath, IAssociation assoc, string baseEntityName, string relatedEntityName, string relationName)
    {
        List<ISchemaItem> entityItems = assoc.ChildItemsByType(EntityRelationColumnPairItem.CategoryConst);
        DataColumn[] baseColumns = new DataColumn[entityItems.Count];
        DataColumn[] relatedColumns = new DataColumn[entityItems.Count];
        if (assoc.IsSelfJoin)
        {
            relatedEntityName = baseEntityName;
        }
        for (int i = 0; i < entityItems.Count; i++)
        {
            DataTable baseTable = dataset.Tables[baseEntityName];
            DataTable relatedTable = dataset.Tables[relatedEntityName];
            string baseFieldName = (entityItems[i] as EntityRelationColumnPairItem
                ).BaseEntityField.Name;
            string relatedFieldName = (entityItems[i] as EntityRelationColumnPairItem
                ).RelatedEntityField.Name;
            if (baseTable == null)
            {
                throw new ArgumentOutOfRangeException("baseEntityName",
                    string.Format("Cannot create relation {0} > {1}. {0} entity is not present because it contains no fields. DataStructure: {2}.",
                        baseEntityName, relatedEntityName, debugPath
                    ));
            }
            if (relatedTable == null)
            {
                throw new ArgumentOutOfRangeException("relatedEntityName",
                    string.Format("Cannot create relation {0} > {1}. {1} entity is not present because it contains no fields. DataStructure: {2}.",
                        baseEntityName, relatedEntityName, debugPath
                    ));
            }
            if (!baseTable.Columns.Contains(baseFieldName))
            {
                throw new ArgumentOutOfRangeException("baseFieldName",
                    string.Format("Cannot create relation {0} > {1}. {0} entity does not contain field {2}. DataStructure: {3}.",
                        baseEntityName, relatedEntityName, baseFieldName, debugPath
                    ));
            }
            if (!relatedTable.Columns.Contains(relatedFieldName))
            {
                throw new ArgumentOutOfRangeException("relatedFieldName",
                    string.Format("Cannot create relation {0} > {1}. {1} entity does not contain field {2}. DataStructure: {3}.",
                        baseEntityName, relatedEntityName, relatedFieldName, debugPath
                    ));
            }
            baseColumns[i] = baseTable.Columns[baseFieldName];
            relatedColumns[i] = relatedTable.Columns[relatedFieldName];
        }
        DataRelation relation;
        try
        {
            // This is a relation - not a base entity
            relation = dataset.Relations.Add(
                relationName,
                baseColumns,
                relatedColumns,
                false
                );
        }
        catch (Exception ex)
        {
            throw new Exception(ResourceUtils.GetString("ErrorCreateRelation", baseEntityName, relatedEntityName, debugPath), ex);
        }
        if (baseColumns[0].Unique)	// Only make such a key on parent(PK)-child(FK)
        {
            try
            {
                ForeignKeyConstraint fkConstraint = new ForeignKeyConstraint(
                    "FK_" + baseEntityName,
                    baseColumns,
                    relatedColumns
                    );
                fkConstraint.AcceptRejectRule = AcceptRejectRule.None;
                fkConstraint.UpdateRule = System.Data.Rule.Cascade;
                fkConstraint.DeleteRule = System.Data.Rule.Cascade;
                dataset.Tables[relatedEntityName].Constraints.Add(fkConstraint);
            }
            catch
            {
            }
        }
        relation.Nested = assoc.IsParentChild;
        return relatedEntityName;
    }
	public static Type ConvertDataType(OrigamDataType dataType)
	{
		Type returnType;
		switch(dataType)
		{
			case OrigamDataType.Blob:
				returnType = typeof(Byte[]);
				break;
			case OrigamDataType.Boolean:
				returnType = typeof(System.Boolean);
				break;
			case OrigamDataType.Byte:
				returnType = typeof(System.Byte);
				break;
				
			case OrigamDataType.Currency:
				returnType = typeof(System.Decimal);
				break;
				
			case OrigamDataType.Date:
				returnType = typeof(System.DateTime);
				break;
				
			case OrigamDataType.Array:
			case OrigamDataType.Long:
				returnType = typeof(System.Int64);
				break;
				
			case OrigamDataType.Float:
				returnType = typeof(System.Decimal);
				break;
				
			case OrigamDataType.Integer:
				returnType = typeof(System.Int32);
				break;
				
			case OrigamDataType.Memo:
			case OrigamDataType.Geography:
			case OrigamDataType.Xml:
			case OrigamDataType.String:
			case OrigamDataType.Object:
				returnType = typeof(System.String);
				break;
				
			case OrigamDataType.UniqueIdentifier:
				returnType = typeof(System.Guid);
				break;
			default:
				returnType = typeof(System.Object);
				break;
		}
		
		return returnType;
	}
	public string RenderExpression(ISchemaItem item, DataStructureEntity entity)
	{
		if(item is FieldMappingItem)
			return RenderExpression(item as FieldMappingItem, entity);
		else if(item is DetachedField)
			return RenderExpression(item as DetachedField, entity);
		else if(item is EntityColumnReference)
			return RenderExpression(item as EntityColumnReference, entity);
		else if(item is FunctionCall)
			return RenderExpression(item as FunctionCall, entity);
		else if(item is DataConstantReference)
			return RenderExpression(item as DataConstantReference);
		else if(item is DataConstant)
			return RenderExpression(item as DataConstant);
		else if(item is AggregatedColumn)
			return RenderExpression(item as AggregatedColumn, entity);
		else if(item is EntityFilterReference)
			return RenderExpression(item as EntityFilterReference, entity);
		else
			throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByDSGen", item.GetType().ToString()));
	}
	private string RenderExpression(DataConstantReference item)
	{
		return RenderExpression(item.DataConstant);
	}
	private string RenderExpression(EntityFilterReference item, DataStructureEntity entity)
	{
		StringBuilder builder = new StringBuilder();
		RenderFilter(builder, item.Filter, entity);
		return builder.ToString();
	}
	public void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity)
	{
		int i = 0;
		foreach(AbstractSchemaItem filterItem in filter.ChildItems)
		{
			if(i > 0)
				sqlExpression.Append(" AND ");
			else
				sqlExpression.Append(" (");
				
			sqlExpression.Append(RenderExpression(filterItem, entity));
			i++;
		}
		if(i > 0)
			sqlExpression.Append(")");
	}
	/// <summary>
	/// We find the relation in this data structure entity relations. 
	/// There may not be the same relation used twice.
	/// </summary>
	/// <param name="baseEntity"></param>
	/// <param name="relation"></param>
	/// <returns></returns>
	private DataStructureEntity LookupRelation(DataStructureEntity baseEntity, IAssociation relation)
	{
		foreach(DataStructureEntity childEntity in baseEntity.ChildItemsByType(DataStructureEntity.CategoryConst))
		{
			if(childEntity.Entity.PrimaryKey.Equals(relation.PrimaryKey) && childEntity.Columns.Count > 0)
			{
				return childEntity;
			}
		}
		return null;
	}
	
	private string LookupColumnName(DataStructureEntity entity, IDataEntityColumn field)
	{
		foreach(DataStructureColumn childColumn in entity.Columns)
		{
			if(childColumn.Field.PrimaryKey.Equals(field.PrimaryKey))
			{
				return childColumn.Name;
			}
		}
		return null;
	}
	private string RenderExpression(AggregatedColumn item, DataStructureEntity entity)
	{
		return RenderAggregation(item.Field, item.AggregationType, item.Relation, entity);
	}
	private string RenderAggregation(IDataEntityColumn field, 
		AggregationType aggregationType, IAssociation relation, DataStructureEntity entity)
	{
		string result = "";
		DataStructureEntity relatedEntity = LookupRelation(entity, relation);
		if(relatedEntity != null) 
		{
			// Now we find the right column
			string columnName = LookupColumnName(relatedEntity, field);
			if(columnName != null)
			{
				result = GetAggregationString(aggregationType) 
					+ "(Child(" 
					+ relatedEntity.Name 
					+ ")." 
					+ columnName
					+ ")";
				if(aggregationType == AggregationType.Sum || 
					aggregationType == AggregationType.Average)
				{
					result = "ISNULL(" + result + ", 0)";
				}
			}
		}
		return result;
	}
	private string RenderParentField(DataStructureColumn column)
	{
        StringBuilder result = new StringBuilder();
        DataStructureEntity requestedParentEntity 
            = column.Entity as DataStructureEntity;
        result.Append("Parent.");
        DataStructureEntity parentEntity 
            = (column.ParentItem as DataStructureEntity).ParentItem 
            as DataStructureEntity;
        if (parentEntity.PrimaryKey.Equals(
            requestedParentEntity.PrimaryKey))
        {
            result.Append(column.Field.Name);
        }
        else
        {
            DataStructureColumn matchingColumn = null;
            for (int i = 0, len = parentEntity.Columns.Count; i < len; i++)
            {
                DataStructureColumn parentEntityColumn 
                    = parentEntity.Columns[i] as DataStructureColumn;
                if ((parentEntityColumn.Entity != null) 
                && parentEntityColumn.Entity.PrimaryKey.Equals(
                    requestedParentEntity.PrimaryKey)
                && parentEntityColumn.Field.PrimaryKey.Equals(
                    column.Field.PrimaryKey))
                {
                    matchingColumn = parentEntityColumn;
                    break;
                }
            }
            if (matchingColumn == null)
            {
                throw new Exception(
                    ResourceUtils.GetString("ColumnNotFoundInParentEntity", 
                        parentEntity.Name, requestedParentEntity.Name, 
                        column.Field.Name));
            }
            result.Append(matchingColumn.Name);
        }
        return result.ToString();
	}
	private string RenderExpression(DataConstant constant)
	{
        if (constant.Name == "null") return "null";
        object value;
		if(UserDefinedParameters)
		{
			if(constant.DataType == OrigamDataType.Date)
			{
				value = _parameterService.GetParameterValue(constant.Id);
			}
			else
			{
				value = (string)_parameterService.GetParameterValue(constant.Id, OrigamDataType.String);
			}
		}
		else
		{
			if(constant.DataType == OrigamDataType.Date)
			{
				value = constant.Value;
			}
			else
			{
				value = constant.Value.ToString();
			}
		}
		return RenderObject(value, constant.DataType);
	}
	public string RenderObject(object value, OrigamDataType dataType)
	{
		switch(dataType)
		{
			case OrigamDataType.Boolean:
			case OrigamDataType.Currency:
			case OrigamDataType.Float:
			case OrigamDataType.Long:
			case OrigamDataType.Integer:
				return DatasetTools.NumberExpression(value);
			case OrigamDataType.UniqueIdentifier:
				return "CONVERT('" + value.ToString() + "', System.Guid)";
			case OrigamDataType.Memo:
			case OrigamDataType.Xml:
			case OrigamDataType.String:
				return DatasetTools.TextExpression(value.ToString());
			case OrigamDataType.Date:
				return DatasetTools.DateExpression(value);
			
			default:
				throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByDSGen1", dataType.ToString()));
		}
	}
	private object RenderDefaultValue(DataConstant constant)
	{
		return _parameterService.GetParameterValue(constant.Id);
	}
	private string RenderColumn(IDataEntityColumn item, DataStructureEntity entity)
	{
		// We have to find a column in the data structure by the entity-column
		// since we can repeat the same column more times in the data structure entity
		// because of lookup values, we will skip the looked-up data structure columns 
		// from the search.
		// With a single exception of a LookupField which is looked-up by default already from
		// the entity.
		foreach(DataStructureColumn column in entity.Columns)
		{
			if(! (column.UseLookupValue && !(item is LookupField)) && column.Field.PrimaryKey.Equals(item.PrimaryKey))
			{
				return column.Name;
			}
		}
		throw new Exception(ResourceUtils.GetString("ColumnNotInStructure", item.Name));
	}
	private string RenderExpression(FieldMappingItem item, DataStructureEntity entity)
	{
		return RenderColumn(item, entity);
	}
	private string RenderExpression(DetachedField item, DataStructureEntity entity)
	{
		if(item.DataType == OrigamDataType.Array)
		{
			DataStructureEntity relatedEntity = LookupRelation(entity, item.ArrayRelation);
			// find some non-guid column because counting guid collumns is not possible
			IDataEntityColumn aggregatedField = item.ArrayValueField;
			foreach(DataStructureColumn column in relatedEntity.Columns)
			{
				if(column.Field.DataType != OrigamDataType.UniqueIdentifier)
				{
					aggregatedField = column.Field;
					break;
				}
			}
			return RenderAggregation(aggregatedField, AggregationType.Count, item.ArrayRelation, entity);
		}
		else
		{
			return RenderColumn(item, entity);
		}
	}
	private string RenderExpression(EntityColumnReference item, DataStructureEntity entity)
	{
		return RenderColumn(item.Field, entity);
	}
	private string RenderExpression(FunctionCall item, DataStructureEntity entity)
	{
		string result = "";
		int i;
		switch(item.Function.Name)
		{
			case "Equal":
			case "NotEqual":
			case "Like":
			case "Add":
			case "Deduct":
			case "Multiply":
			case "Divide":
			case "LessThan":
			case "LessThanOrEqual":
			case "GreaterThan":
			case "GreaterThanOrEqual":
				ISchemaItem leftParam = item.GetChildByName("Left");
				ISchemaItem rightParam = item.GetChildByName("Right");
				ISchemaItem leftValue = null;
				ISchemaItem rightValue = null;
				if(leftParam.HasChildItems) leftValue = leftParam.ChildItems[0];
				if(rightParam.HasChildItems) rightValue = rightParam.ChildItems[0];
				
				if(leftValue == null) throw new ArgumentOutOfRangeException("Left", null, ResourceUtils.GetString("ErrorLeftParamEmpty"));
				// handle IS NULL
				if(rightValue == null && item.Function.Name == "Equal")
				{
					result =  RenderExpression(leftValue, entity) + " IS NULL";
				}
				else if(rightValue == null && item.Function.Name == "NotEqual")
				{
					result =  RenderExpression(leftValue, entity) + " IS NOT NULL";
				}
				else
				{
					if(rightValue == null) throw new ArgumentOutOfRangeException("Right", null, ResourceUtils.GetString("ErrorRightParamEmpty"));
					result = RenderExpression(leftValue, entity)
						+ " " + GetOperator(item.Function.Name) + " "
						+ RenderExpression(rightValue, entity);
				}
				break;
			case "Not":
				ISchemaItem argument = item.GetChildByName("Argument");
				
				if(! argument.HasChildItems) throw new ArgumentOutOfRangeException("Argument", null, ResourceUtils.GetString("ErrorArgumentEmpty"));
				if(argument.ChildItems.Count > 1) throw new ArgumentOutOfRangeException("Argument", null, ResourceUtils.GetString("ErrorOneNOTArgument"));
				ISchemaItem argumentValue = argument.ChildItems[0];
				result = "NOT(" + RenderExpression(argumentValue, entity) + ")";
				break;
			case "Concat":
				ISchemaItem concatArg = item.GetChildByName("Strings");
				List<AbstractSchemaItem> concatStrings = concatArg.ChildItems.ToList();
				if(concatStrings.Count < 2) throw new ArgumentOutOfRangeException("Strings", null, ResourceUtils.GetString("ErrorTwoCONCATArguments"));
				concatStrings.Sort();
				i = 0;
				StringBuilder concatBuilder = new StringBuilder();
				foreach(ISchemaItem concatString in concatStrings)
				{
					if(i > 0) concatBuilder.Append(" + ");
					concatBuilder.Append(RenderExpression(concatString, entity));
					i++;
				}
				result = concatBuilder.ToString();
				break;
			case "LogicalOr":
			case "LogicalAnd":
				ISchemaItem logicalArg = item.GetChildByName("Arguments");
				ISchemaItemCollection logicalArguments = logicalArg.ChildItems;
				if(logicalArguments.Count < 2) throw new ArgumentOutOfRangeException("Arguments", null, ResourceUtils.GetString("ErrorTwoArguments", item.Function.Name));
				i = 0;
				StringBuilder logicalBuilder = new StringBuilder();
				foreach(ISchemaItem logicalArgument in logicalArguments)
				{
					if(i > 0) logicalBuilder.Append(" " + GetOperator(item.Function.Name) + " ");
					logicalBuilder.Append(RenderExpression(logicalArgument, entity));
					i++;
				}
				result = logicalBuilder.ToString();
				break;
			case "Space":
				ISchemaItem spacesArg = item.GetChildByName("NumberOfSpaces");
				if(spacesArg.ChildItems.Count == 0) throw new ArgumentOutOfRangeException("NumberOfSpaces", null, ResourceUtils.GetString("ErrorNumberOfSpaces"));
				int numberOfSpaces = Convert.ToInt32(RenderExpression(spacesArg.ChildItems[0], entity));
				for(i=0; i<numberOfSpaces; i++)
				{
					result += " ";
				}
				result = DatasetTools.TextExpression(result);
				break;
			case "Substring":
				result = "SUBSTRING("
					+ RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity) + ", "
					+ RenderExpression(item.GetChildByName("Start").ChildItems[0], entity) + ", "
					+ RenderExpression(item.GetChildByName("Length").ChildItems[0], entity) + ")";
				break;
			case "Condition":
				result = "IIF("
					+ RenderExpression(item.GetChildByName("If").ChildItems[0], entity)
					+ ", "
					+ (item.GetChildByName("Then").ChildItems.Count == 0 ? "null" : RenderExpression(item.GetChildByName("Then").ChildItems[0], entity))
					+ ", "
					+ (item.GetChildByName("Else").ChildItems.Count == 0 ? "null" : RenderExpression(item.GetChildByName("Else").ChildItems[0], entity))
					+ ")";
				break;
			case "Length":
				result = "LEN("
					+ RenderExpression(item.GetChildByName("Text").ChildItems[0], entity)
					+ ")";
				break;
			case "ConvertDateToString":
				result = "Substring(Convert("
					+ RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity)
					+ ", System.String), 1, " + item.DataLength.ToString() + ")";
				break;
			case "In":
				ISchemaItem leftArg = item.GetChildByName("FilterExpression").ChildItems[0];
				ISchemaItem listArg = item.GetChildByName("List");
				ISchemaItemCollection listExpressions = listArg.ChildItems;
				if(listExpressions.Count < 2) throw new ArgumentOutOfRangeException("List", null, "There have to be at least 2 items in the List argument for IN function specified as a column.");
				i = 0;
				StringBuilder listBuilder = new StringBuilder();
				foreach(ISchemaItem listExpression in listExpressions)
				{
					if(i > 0) listBuilder.Append(", ");
					listBuilder.Append(RenderExpression(listExpression, entity));
					i++;
				}
				result = RenderExpression(leftArg, entity) + " IN (" + listBuilder.ToString() + ")";
				break;
			case "IsNull":
				ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
				ISchemaItem replacementArg = item.GetChildByName("ReplacementValue").ChildItems[0];
				
				result = "ISNULL(" + RenderExpression(expressionArg, entity) + ", " + RenderExpression(replacementArg, entity) + ")";
				break;
			case "Between":
				expressionArg = item.GetChildByName("Expression").ChildItems[0];
				leftArg = item.GetChildByName("Left").ChildItems[0];
				ISchemaItem rightArg = item.GetChildByName("Right").ChildItems[0];
				
				string betweenExpression = RenderExpression(expressionArg, entity);
				result = betweenExpression + " >= " + RenderExpression(leftArg, entity) + " AND " + betweenExpression + " <= " + RenderExpression(rightArg, entity);
				break;
            case "Round":
                expressionArg = item.GetChildByName("Expression").ChildItems[0];
                ISchemaItem precisionArg = item.GetChildByName("Precision").ChildItems[0];
                string expression = RenderExpression(expressionArg, entity);
                int precision = 0;
                if(! int.TryParse(RenderExpression(precisionArg, entity), out precision))
                {
                    throw new Exception("Precision must be an integer.");
                }
                result = string.Format("CONVERT({0} * {1}, System.Int64) / {1}", expression, Math.Pow(10, precision));
                break;
            case "Abs":
                expressionArg = item.GetChildByName("Expression").ChildItems[0];
                var renderedExpressionArgument = RenderExpression(expressionArg, entity);
                result = "IIF("
                         + renderedExpressionArgument + " > 0"
                         + ", "
                         + renderedExpressionArgument
                         + ", "
                         + renderedExpressionArgument + " * -1"
                         + ")";
                break;
            default:
                throw new Exception($"{item.Function.Name} is a database function and " 
                    + "cannot be used for in-memory calculation. Either set ForceDatabaseCalculation "
                    + $"to true in {item.Path} or use a data rule.");
		}
		return "(" + result + ")";
	}
	private string GetAggregationString(AggregationType type)
	{
		switch(type)
		{
			case AggregationType.Sum:
				return "SUM";
			case AggregationType.Count:
				return "COUNT";
			case AggregationType.Average:
				return "AVG";
			case AggregationType.Minimum:
				return "MIN";
			case AggregationType.Maximum:
				return "MAX";
			default:
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("UnsupportedAggreg"));
		}
	}
	private string GetOperator(string functionName)
	{
		switch(functionName)
		{
			case "NotEqual":
				return "<>";
			case "Equal":
				return "=";
			case "Like":
				return "LIKE";
			case "Add":
				return "+";
			case "Deduct":
				return "-";
			case "Multiply":
				return "*";
			case "Divide":
				return "/";
			case "LessThan":
				return "<";
			case "LessThanOrEqual":
				return "<=";
			case "GreaterThan":
				return ">";
			case "GreaterThanOrEqual":
				return ">=";
			case "LogicalOr":
				return "OR";
			case "LogicalAnd":
				return "AND";
			default:
				throw new ArgumentOutOfRangeException("functionName", functionName, ResourceUtils.GetString("UnsupportedOperator"));
		}
	}
	private const string dataSetName = "ROOT";
}
