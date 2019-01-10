#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Data.Common;
using System.Text;

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using System.Collections.Generic;

namespace Origam.DA.Service
{
	public abstract class AbstractSqlCommandGenerator : DbDataAdapterFactory, IDisposable
	{
		private readonly ParameterReference PageNumberParameterReference = new ParameterReference();
		private readonly ParameterReference PageSizeParameterReference = new ParameterReference();
		private string _pageNumberParameterName;
		private string _pageSizeParameterName;
		private bool _userDefinedParameters = false;
		private bool _resolveAllFilters;
        private bool _prettyFormat;
        private int _indentLevel = 0;

        private struct SortOrder
		{
			public string Expression;
			public DataStructureColumnSortDirection SortDirection;
		}

		private enum JoinBeginType
		{
			Join = 0,
			Where = 1,
			And = 2
		}

		public AbstractSqlCommandGenerator()
		{
			PageNumberParameterReference.ParameterId = new Guid("3e5e12e4-a0dd-4d35-a00a-2fdb267536d1");
			PageSizeParameterReference.ParameterId = new Guid("c310d577-d4d9-42da-af92-a5202ba26e79");
		}

		public abstract IDbDataParameter GetParameter();
		public IDbDataParameter GetParameter(string name, Type type)
		{
			IDbDataParameter dbParam = this.GetParameter();
			if(type == typeof(DateTime))
			{
				dbParam.DbType = DbType.DateTime;
			}
			else if(type == typeof(Guid))
			{
				dbParam.DbType = DbType.Guid;
			}
			else if(type == typeof(int))
			{
				dbParam.DbType = DbType.Int32;
			}
			else if(type == typeof(double))
			{
				dbParam.DbType = DbType.Double;
			}
			else if(type == typeof(bool))
			{
				dbParam.DbType = DbType.Boolean;
			}
			else if(type == typeof(Decimal) || type == typeof(float))
			{
				dbParam.DbType = DbType.Decimal;
			}

			dbParam.ParameterName = this.ParameterDeclarationChar + name;

			return dbParam;
		}
		public abstract IDbCommand GetCommand(string cmdText);
		public abstract IDbCommand GetCommand(string cmdText, IDbConnection connection);
		public abstract IDbCommand GetCommand(string cmdText, IDbConnection connection, IDbTransaction transaction);
		
		public abstract DbDataAdapter GetAdapter();
		public abstract DbDataAdapter GetAdapter(IDbCommand command);
		public abstract DbDataAdapter CloneAdapter(DbDataAdapter adapter);
		public abstract IDbCommand CloneCommand(IDbCommand command);
		public abstract void DeriveStoredProcedureParameters(IDbCommand command);
		public abstract string NameLeftBracket {get;}
		public abstract string NameRightBracket {get;}
		public abstract string ParameterDeclarationChar {get;}
		public abstract string ParameterReferenceChar {get;}
		public abstract string StringConcatenationChar {get;}
		public abstract bool PagingCanIncludeOrderBy {get;}

		public abstract string GetIndexName(IDataEntity entity, DataEntityIndex index);
		public abstract string SelectClause(string finalQuery, int top);

		public abstract string True{get;}
		public abstract string False{get;}

		public const string RowNumColumnName = "RowNum";

		public abstract IDbDataParameter BuildParameter(string paramName, 
            string sourceColumn, OrigamDataType dataType, DatabaseDataType dbDataType,
            int dataLength, bool allowNulls);

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

		public bool ResolveAllFilters
		{
			get
			{
				return _resolveAllFilters;
			}
			set
			{
				_resolveAllFilters = value;
			}
		}

        public bool PrettyFormat
        {
            get
            {
                return _prettyFormat;
            }

            set
            {
                _prettyFormat = value;
            }
        }

        public IDbCommand ScalarValueCommand(DataStructure ds, DataStructureFilterSet filter, 
            DataStructureSortSet sortSet, string columnName, Hashtable parameters)
		{
			Hashtable selectParameterReferences = new Hashtable();
			IDbCommand cmd = this.GetCommand(
				this.SelectSql(ds, ds.Entities[0] as DataStructureEntity, filter, sortSet, 
                columnName, null, parameters, selectParameterReferences, true, false, false, true));
			cmd.CommandType = CommandType.Text;
			this.BuildSelectParameters(cmd, selectParameterReferences);
			this.BuildFilterParameters(cmd, ds, filter, null, parameters);
			return cmd;
		}

		public DbDataAdapter CreateDataAdapter(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, DataStructureSortSet sortSet, Hashtable parameters, 
            bool paging, string columnName, bool forceDatabaseCalculation)
		{
			if(!(entity.EntityDefinition is TableMappingItem))
			{
				throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
			}

			DbDataAdapter adapter = this.GetAdapter();
			BuildCommands(adapter, ds, entity, filter, sortSet, parameters, paging, columnName,
                forceDatabaseCalculation);
			adapter.TableMappings.Clear();
			adapter.TableMappings.Add(CreateMapping(entity));

			return adapter;
		}

		public DbDataAdapter CreateDataAdapter(string procedureName, ArrayList entitiesOrdered,
			IDbConnection connection, IDbTransaction transaction)
		{
			IDbCommand cmd = this.GetCommand(procedureName);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Connection = connection;
			cmd.Transaction = transaction;
			DeriveStoredProcedureParameters(cmd);

			DbDataAdapter adapter = this.GetAdapter(cmd);

			adapter.TableMappings.Clear();
			int order = 0;
			foreach (DataStructureEntity entity in entitiesOrdered)
			{
				adapter.TableMappings.Add(CreateMapping(entity, order));	
				order += 1;
			}
			return adapter;
		}

		public DbDataAdapter CreateSelectRowDataAdapter(DataStructureEntity entity, 
            string columnName, bool forceDatabaseCalculation)
		{
			if(!(entity.EntityDefinition is TableMappingItem))
			{
				throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
			}

			DbDataAdapter adapter = this.GetAdapter();
			BuildSelectRowCommand((IDbDataAdapter)adapter, entity, columnName, forceDatabaseCalculation);
			adapter.TableMappings.Clear();
			adapter.TableMappings.Add(CreateMapping(entity));

			return adapter;
		}

		public DbDataAdapter CreateUpdateFieldDataAdapter(TableMappingItem table, FieldMappingItem field)
		{
			DbDataAdapter adapter = this.GetAdapter();
			BuildSelectUpdateFieldCommand(adapter, table, field);
			adapter.TableMappings.Clear();
			adapter.TableMappings.Add(CreateUpdateFieldMapping(table, field));

			return adapter;
		}

		public IDbCommand UpdateFieldCommand(TableMappingItem entity,  FieldMappingItem field)
		{
			IDbCommand cmd = this.GetCommand(
				"UPDATE " 
				+ RenderExpression(entity) 
				+ " SET " 
				+ RenderExpression(field, null) 
				+ " = " + ParameterReferenceChar + "newValue WHERE " 
				+ RenderExpression(field, null) 
				+ " = " + ParameterReferenceChar + "oldValue");
			cmd.CommandType = CommandType.Text;
			IDataParameter sqlParam = BuildParameter(
                ParameterDeclarationChar + "oldValue", null,  field.DataType, 
                field.MappedDataType, field.DataLength, field.AllowNulls);
			cmd.Parameters.Add(sqlParam);
			sqlParam = BuildParameter(ParameterDeclarationChar + "newValue", 
                null, field.DataType, field.MappedDataType, field.DataLength, 
                field.AllowNulls);
			cmd.Parameters.Add(sqlParam);
			return cmd;
		}

		private System.Data.Common.DataTableMapping CreateMapping(DataStructureEntity entity)
		{
			return CreateMapping(entity, 0);
		}

		private System.Data.Common.DataTableMapping CreateMapping(DataStructureEntity entity, int tableOrder)
		{
			System.Data.Common.DataTableMapping dtm = new System.Data.Common.DataTableMapping();
            						
			if (tableOrder == 0)
			{
				dtm.SourceTable = "Table";
			} 
			else
			{
				dtm.SourceTable = "Table" + tableOrder.ToString();
			}

			dtm.DataSetTable = entity.Name;

			foreach(DataStructureColumn column in entity.Columns)
			{
				dtm.ColumnMappings.Add(column.Name, column.Name);
			}

			return dtm;
		}

		private System.Data.Common.DataTableMapping CreateUpdateFieldMapping(TableMappingItem table, FieldMappingItem field)
		{
			System.Data.Common.DataTableMapping dtm = new System.Data.Common.DataTableMapping();
            									
			dtm.SourceTable = "Table";
			dtm.DataSetTable = table.Name;

			foreach(IDataEntityColumn column in table.EntityPrimaryKey)
			{
				dtm.ColumnMappings.Add(column.Name, column.Name);
			}

			dtm.ColumnMappings.Add(field.Name, field.Name);

			return dtm;
		}

		public void BuildSelectRowCommand(IDbDataAdapter adapter, DataStructureEntity entity,
            string columnName, bool forceDatabaseCalculation)
		{
			Hashtable selectParameterReferences = new Hashtable();

			((IDbDataAdapter)adapter).SelectCommand = this.GetCommand(
                this.SelectRowSql(entity, selectParameterReferences, columnName,
                forceDatabaseCalculation));

			BuildPrimaryKeySelectParameters(adapter.SelectCommand, entity);
			this.BuildSelectParameters(adapter.SelectCommand, selectParameterReferences);
		}

		public void BuildSelectUpdateFieldCommand(DbDataAdapter adapter, TableMappingItem table, FieldMappingItem field)
		{
			((IDbDataAdapter)adapter).SelectCommand = this.GetCommand(this.SelectUpdateFieldSql(table, field));

			BuildUpdateFieldParameters(((IDbDataAdapter)adapter).SelectCommand, field);
		}

		public IDbCommand SelectReferenceCountCommand(TableMappingItem table, FieldMappingItem field)
		{
			Hashtable selectParameterReferences = new Hashtable();

			IDbCommand cmd =  this.GetCommand(this.SelectReferenceCountSql(table, field));

			cmd.CommandType = CommandType.Text;

			BuildUpdateFieldParameters(cmd, field);

			return cmd;
		}

		public void BuildCommands(DbDataAdapter adapter, DataStructure ds, DataStructureEntity entity, 
			DataStructureFilterSet filter, DataStructureSortSet sortSet, Hashtable parameters,
			bool paging, string columnName, bool forceDatabaseCalculation)
		{
			Hashtable selectParameterReferences = new Hashtable();

			((IDbDataAdapter)adapter).SelectCommand = 
				this.GetCommand(this.SelectSql(
					ds, entity, filter, sortSet, columnName, null, parameters, 
                    selectParameterReferences, false, paging, false, forceDatabaseCalculation));
			ArrayList entities = ds.Entities;

			BuildSelectParameters(((IDbDataAdapter)adapter).SelectCommand, selectParameterReferences);
			BuildFilterParameters(((IDbDataAdapter)adapter).SelectCommand, ds, filter, null, parameters);

			if(! ds.Name.StartsWith("Lookup") || entity.AllFields)
			{
				((IDbDataAdapter)adapter).UpdateCommand = this.GetCommand(this.UpdateSql(ds, entity));
				((IDbDataAdapter)adapter).InsertCommand = this.GetCommand(this.InsertSql(ds, entity));
				((IDbDataAdapter)adapter).DeleteCommand = this.GetCommand(this.DeleteSql(ds, entity));

				BuildUpdateParameters(((IDbDataAdapter)adapter).UpdateCommand, entity);
				BuildInsertParameters(((IDbDataAdapter)adapter).InsertCommand, entity);
				BuildDeleteParameters(((IDbDataAdapter)adapter).DeleteCommand, entity);
			}
		}

		public void BuildFilterParameters(IDbCommand command, DataStructure ds, DataStructureFilterSet filterSet, Hashtable replaceParameterTexts, Hashtable parameters)
		{
			foreach(DataStructureEntity entity in ds.Entities)
			{
				foreach(EntityFilter filter in Filters(filterSet, entity, parameters, false))
				{
                    Hashtable paramReferences = filter.ParameterReferences;
                    foreach (DictionaryEntry entry in paramReferences)
					{
						ParameterReference parameterRef = entry.Value as ParameterReference;
						string paramName = RenderExpression(parameterRef, entity, replaceParameterTexts, null, null);

						if(! command.Parameters.Contains(paramName))
						{
							IDbDataParameter sqlParam = BuildParameter(paramName, parameterRef);
							command.Parameters.Add(sqlParam);
						}
					}
				}

				// recursion for lookup values - they can also have parameters, e.g. row level security ones
				foreach(DataStructureColumn column in entity.Columns)
				{
					if(column.UseLookupValue)
					{
						BuildFilterParameters(command, column.FinalLookup.ValueDataStructure, null, replaceParameterTexts, parameters);
					}
				}
			}
		}

		private IDbDataParameter BuildParameter(string paramName, 
            ParameterReference parameterRef)
		{
            IDatabaseDataTypeMapping mappableDataType = parameterRef.Parameter
                as IDatabaseDataTypeMapping;
			return BuildParameter(paramName, null, 
                parameterRef.Parameter.DataType, 
                mappableDataType?.MappedDataType,
                parameterRef.Parameter.DataLength, 
                parameterRef.Parameter.AllowNulls);
		}

		public void BuildSelectParameters(IDbCommand command, Hashtable parameterReferences)
		{
			foreach(DictionaryEntry entry in parameterReferences)
			{
				ParameterReference parameterRef = entry.Value as ParameterReference;
				string paramName = (string)entry.Key; //RenderExpression(parameterRef, entity, replaceParameterTexts, (string)entry.Key);

				//				Hashtable paramList = new Hashtable();

				//				if(dynamicParameters != null && dynamicParameters.Contains((string)entry.Key))
				//				{
				//					int dynCount = (int)dynamicParameters[(string)entry.Key];
				//
				//					for(int dyn = 0; dyn < dynCount; dyn++)
				//					{
				//						paramList.Add(paramName + dyn.ToString(), parameterRef);
				//					}
				//				}
				//				else
				//				{
				//					paramList.Add(paramName, parameterRef);
				//				}

				//				foreach(DictionaryEntry pEntry in paramList)
				//				{
				//					string pName = (string)pEntry.Key;
				//					ParameterReference pRef = (ParameterReference)pEntry.Value;

				if(! command.Parameters.Contains(paramName))
				{
					System.Data.IDataParameter sqlParam = BuildParameter(paramName, parameterRef);

					command.Parameters.Add(sqlParam);
					//					}
				}
			}
		}

		public void BuildUpdateParameters(IDbCommand command, DataStructureEntity entity)
		{
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(ShouldUpdateColumn(column, entity))
				{
					command.Parameters.Add(CreateNewValueParameter(column));
					command.Parameters.Add(CreateOriginalValueParameter(column));
					command.Parameters.Add(CreateOriginalValueParameterForNullComparison(column));
				}
			}
		}

		public void BuildInsertParameters(IDbCommand command, DataStructureEntity entity)
		{
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
				{
					command.Parameters.Add(CreateNewValueParameter(column));
				}
			}
		}

		public void BuildDeleteParameters(IDbCommand command, DataStructureEntity entity)
		{
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
				{
					command.Parameters.Add(CreateOriginalValueParameter(column));
					command.Parameters.Add(CreateOriginalValueParameterForNullComparison(column));
				}
			}
		}

		public void BuildPrimaryKeySelectParameters(IDbCommand command, DataStructureEntity entity)
		{
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field is FieldMappingItem && column.Field.IsPrimaryKey && column.UseLookupValue == false && column.UseCopiedValue == false)
				{
					command.Parameters.Add(CreateNewValueParameter(column));
				}
			}
		}

		public void BuildUpdateFieldParameters(IDbCommand command, 
            FieldMappingItem column)
		{
			command.Parameters.Add(BuildParameter(
				ParameterDeclarationChar + column.Name,
				column.Name,
				column.DataType,
                column.MappedDataType,
				column.DataLength,
				true)
				);
		}

		private IDataParameter CreateNewValueParameter(
            DataStructureColumn column)
        {
            FieldMappingItem dbField = GetDatabaseField(column);
            return BuildParameter(
                NewValueParameterName(column, true),
                column.Name,
                dbField.DataType,
                dbField.MappedDataType,
                dbField.DataLength,
                true);
        }

        private static FieldMappingItem GetDatabaseField(
            DataStructureColumn column)
        {
            FieldMappingItem dbField = column.Field as FieldMappingItem;
            if (dbField == null)
            {
                throw new InvalidCastException(
                    "Only database fields can be processed.");
            }
            return dbField;
        }

        private IDataParameter CreateOriginalValueParameter(
            DataStructureColumn column)
		{
            FieldMappingItem dbField = GetDatabaseField(column);
            IDataParameter result = BuildParameter(
				this.OriginalParameterName(column, true),
				column.Name,
				dbField.DataType,
                dbField.MappedDataType,
				dbField.DataLength,
				true);
			result.SourceVersion = DataRowVersion.Original;
			result.Direction = ParameterDirection.Input;
			return result;
		}

		private IDataParameter CreateOriginalValueParameterForNullComparison(
            DataStructureColumn column)
		{
            FieldMappingItem dbField = GetDatabaseField(column);
            IDataParameter result = BuildParameter(
				OriginalParameterNameForNullComparison(column, true),
				column.Name,
				dbField.DataType,
                dbField.MappedDataType,
				dbField.DataLength,
				true);
			result.SourceVersion = DataRowVersion.Original;
			result.Direction = ParameterDirection.Input;
			return result;
		}

		#region Main SQL Command Rendering

		public string TableListDefinitionDdl(SchemaItemCollection tables)
		{
			StringBuilder ddl = new StringBuilder();
		
			foreach(TableMappingItem table in tables)
			{
				if(table.DatabaseObjectType == DatabaseMappingObjectType.Table)
				{
					ddl.Append(TableDefinitionDdl(table));
					ddl.Append(Environment.NewLine);
					ddl.Append(Environment.NewLine);
				}
			}

			return ddl.ToString();
		}

		public string FunctionDefinitionDdl(Function function)
		{
			if(function.FunctionType == OrigamFunctionType.Database)
			{
				StringBuilder builder = new StringBuilder("CREATE FUNCTION dbo.");
				builder.Append(function.Name + "(");
				int i = 0;
				foreach(FunctionParameter parameter in function.ChildItems)
				{
					if(i > 0) builder.Append(", ");
					builder.Append(ParameterDeclarationChar + parameter.Name + " as ?");
					i++;
				}
				builder.Append(")" + Environment.NewLine);
				builder.Append("RETURNS " + DdlDataType(function.DataType, 0, null) 
                    + Environment.NewLine);
				builder.Append("AS" + Environment.NewLine + "BEGIN" + Environment.NewLine);
				builder.Append("DECLARE " + ParameterDeclarationChar + "result AS " 
                    + DdlDataType(function.DataType, 0, null) + Environment.NewLine);
				builder.Append("RETURN " + ParameterReferenceChar + "result" 
                    + Environment.NewLine);
				builder.Append("END");
				return builder.ToString();
			}
			else
			{
				throw new InvalidOperationException(
                    ResourceUtils.GetString("DDLForFunctionsOnly"));
			}
		}

		public string ForeignKeyConstraintsDdl(TableMappingItem table)
		{
			string result = "";

			foreach(DataEntityConstraint constraint in table.Constraints)
			{
				if(constraint.Type == ConstraintType.ForeignKey)
				{
					result += Environment.NewLine + AddForeignKeyConstraintDdl(table, constraint);
				}
			}

			return result;
		}

		public string AddForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint)
		{
			StringBuilder ddl = new StringBuilder();

			ddl.AppendFormat("ALTER TABLE {0} ADD {1}",
				NameLeftBracket + table.MappedObjectName + NameRightBracket,
				ForeignKeyConstraintDdl(table, constraint));
			return ddl.ToString();
		}

		public string ForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint)
		{
			StringBuilder ddl = new StringBuilder();

			if(constraint.ForeignEntity is TableMappingItem && constraint.Fields[0] is FieldMappingItem)
			{
				string pkTableName = (constraint.ForeignEntity as TableMappingItem).MappedObjectName;

				ddl.AppendFormat("CONSTRAINT {1}",
					NameLeftBracket + table.MappedObjectName + NameRightBracket,
					"FK_" + table.MappedObjectName + "_" + (constraint.Fields[0] as FieldMappingItem).MappedColumnName + "_" + pkTableName);

				ddl.Append(Environment.NewLine + "\tFOREIGN KEY (");
				int i = 0;
				foreach(FieldMappingItem field in constraint.Fields)
				{
					if(i > 0) ddl.Append(", ");
					ddl.Append(Environment.NewLine + "\t\t" + RenderExpression(field, null));

					i++;
				}
				ddl.Append(Environment.NewLine + "\t)" + Environment.NewLine);

				ddl.AppendFormat(Environment.NewLine + "\tREFERENCES {0} (",
					NameLeftBracket + pkTableName + NameRightBracket);

				i = 0;
				foreach(FieldMappingItem field in constraint.Fields)
				{
					if(i > 0) ddl.Append(", ");
					ddl.Append(Environment.NewLine + "\t\t" + RenderExpression(field.ForeignKeyField, null, null, null, null));

					i++;
				}

				ddl.Append(Environment.NewLine + "\t)");
			}
			
			return ddl.ToString();
		}

		public string AddColumnDdl(FieldMappingItem field)
		{
			StringBuilder ddl = new StringBuilder();

			ddl.AppendFormat("ALTER TABLE {0} ADD {1}",
				RenderExpression(field.ParentItem as TableMappingItem),
				ColumnDefinitionDdl(field));

			if(! field.AllowNulls && field.DefaultValue != null)
			{
				string constraintName = "DF_" + (field.ParentItem as TableMappingItem).MappedObjectName + "_" + field.MappedColumnName;
				ddl.AppendFormat(" CONSTRAINT {0} DEFAULT {1}",
					NameLeftBracket + constraintName + NameRightBracket,
					this.RenderConstant(field.DefaultValue));

				ddl.Append(Environment.NewLine);
				ddl.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}",
					RenderExpression(field.ParentItem as TableMappingItem),
					NameLeftBracket + constraintName + NameRightBracket);
			}

			// ddl.Append(Environment.NewLine + "GO");

			return ddl.ToString();
		}

        public string AlterColumnDdl(FieldMappingItem field)
        {
            StringBuilder ddl = new StringBuilder();

            ddl.AppendFormat("ALTER TABLE {0} ALTER COLUMN {1}",
                RenderExpression(field.ParentItem as TableMappingItem),
                ColumnDefinitionDdl(field));
            return ddl.ToString();
        }

        private string ColumnDefinitionDdl(FieldMappingItem field)
		{
			StringBuilder ddl = new StringBuilder();
			// fname | varchar(20) | NOT NULL | PRIMARY KEY
			ddl.AppendFormat("{0} {1}",
				NameLeftBracket + field.MappedColumnName + NameRightBracket,
				DdlDataType(field.DataType, field.DataLength, field.MappedDataType)
				);
			if(field.AllowNulls)
				ddl.Append(" NULL");
			else
				ddl.Append(" NOT NULL");
			if(field.IsPrimaryKey)
				ddl.Append(" PRIMARY KEY NONCLUSTERED");
			return ddl.ToString();
		}

		public string IndexDefinitionDdl(IDataEntity entity, DataEntityIndex index, bool complete)
		{
			StringBuilder ddl = new StringBuilder();

			//			if(complete)
			//			{
			//				ddl.AppendFormat("IF EXISTS (SELECT name FROM sysindexes WHERE name = '{0}') DROP INDEX {1}.[{0}]",
			//					index.Name,
			//					RenderExpression(index.ParentItem, null, null)
			//					);
			//
			//				ddl.Append(Environment.NewLine + "GO" + Environment.NewLine);
			//			}

			ddl.AppendFormat("CREATE {0}INDEX {1} ON {2} (",
				(index.IsUnique ? "UNIQUE " : ""),
				NameLeftBracket + GetIndexName(entity, index) + NameRightBracket,
				NameLeftBracket + (index.ParentItem as TableMappingItem).MappedObjectName + NameRightBracket
				);

			int i = 0;
			ArrayList sortedFields = index.ChildItemsByType(DataEntityIndexField.ItemTypeConst);
			sortedFields.Sort();

			foreach(DataEntityIndexField field in sortedFields)
			{
				if(i > 0) ddl.Append(", ");

				ddl.AppendFormat("{0} {1}",
					RenderExpression(field.Field, null, null, null, null),
					field.SortOrder == DataEntityIndexSortOrder.Descending ? "DESC" : "ASC");

				i++;
			}

			ddl.Append(")");

			//			if(complete)
			//			{
			//				ddl.Append(Environment.NewLine + "GO");
			//			}

			return ddl.ToString();
		}

		public string TableDefinitionDdl(TableMappingItem table)
		{
			if(table.DatabaseObjectType != DatabaseMappingObjectType.Table)
			{
				throw new InvalidOperationException(ResourceUtils.GetString("CantDDLScript", table.DatabaseObjectType.ToString()));
			}

			StringBuilder ddl = new StringBuilder();

			//			ddl.AppendFormat("if exists (select * from dbo.sysobjects where id = object_id(N'[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [{0}]",
			//				table.SourceTableName
			//				);
			//
			//			ddl.Append(Environment.NewLine + "GO" + Environment.NewLine);

			ddl.AppendFormat("CREATE TABLE {0} (",
				NameLeftBracket + table.MappedObjectName + NameRightBracket);

			int i = 0;
			foreach(ISchemaItem item in table.EntityColumns)
			{
				if(item is FieldMappingItem)
				{
					FieldMappingItem field = item as FieldMappingItem;

					if(i > 0) ddl.Append(",");

					ddl.Append(Environment.NewLine + "\t" + ColumnDefinitionDdl(field));

					i++;
				}
			}

			ddl.Append(")");
			
			//			ddl.Append(Environment.NewLine + "GO");

			if(table.EntityIndexes.Count > 0)
			{
				foreach(DataEntityIndex index in table.EntityIndexes)
				{
                    ddl.Append(Environment.NewLine);
                    ddl.Append(IndexDefinitionDdl(table, index, false));
					//					ddl.Append(Environment.NewLine + "GO");
				}
			}

			return ddl.ToString();
		}

		private string GetFromPartId(DataStructureEntity entity, DataStructureFilterSet filter)
		{
			if(filter == null)
			{
				return entity.RootEntity.PrimaryKey["Id"].ToString();
			}
			else
			{
				return entity.RootEntity.PrimaryKey["Id"].ToString() + filter.PrimaryKey["Id"].ToString();
			}
		}

		private void RetrieveGroupByColumns(FunctionCall function, ArrayList groupByColumns)
		{
			foreach(FunctionCallParameter parameter in function.ChildItemsByType(FunctionCallParameter.ItemTypeConst))
			{
				foreach(ISchemaItem item in parameter.ChildItems)
				{
					if(item is EntityColumnReference)
					{
						if((item as EntityColumnReference).Column is FunctionCall)
						{
							RetrieveGroupByColumns((item as EntityColumnReference).Column as FunctionCall, groupByColumns);
						}
						else if((item as EntityColumnReference).Column is FieldMappingItem)
						{
							groupByColumns.Add((item as EntityColumnReference).Column);
						}
					}

					if(item is FunctionCall)
					{
						RetrieveGroupByColumns(item as FunctionCall, groupByColumns);
					}
				}
			}
		}

		public string SelectParameterDeclarationsSql(DataStructureFilterSet filter, bool paging,
            string columnName)
		{
			StringBuilder result = new StringBuilder();
			Hashtable ht = new Hashtable();
			DataStructure ds = filter.RootItem as DataStructure;

			foreach(DataStructureEntity entity in ds.Entities)
			{
				this.SelectParameterDeclarationsSql(result, ht, ds, entity, filter, null, paging,
                    columnName);
			}
            this.SelectParameterDeclarationsSetSql(result, ht);
			return result.ToString();
		}

		public string SelectParameterDeclarationsSql(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, bool paging, string columnName)
		{
			StringBuilder result = new StringBuilder();
			Hashtable ht = new Hashtable();
			this.SelectParameterDeclarationsSql(result, ht, ds, entity, filter, null, paging, columnName);
			this.SelectParameterDeclarationsSetSql(result, ht);
			return result.ToString();
		}

		public string SelectParameterDeclarationsSql(DataStructure ds, DataStructureEntity entity, 
            DataStructureSortSet sort, bool paging, string columnName)
		{
			StringBuilder result = new StringBuilder();
			Hashtable ht = new Hashtable();
			this.SelectParameterDeclarationsSql(result, ht, ds, entity, null, sort, paging, columnName);
			this.SelectParameterDeclarationsSetSql(result, ht);
			return result.ToString();
		}

		private ArrayList Parameters(IDbCommand cmd)
		{
			int declarationLength = this.ParameterDeclarationChar.Length;
			ArrayList list = new ArrayList(cmd.Parameters.Count);
			foreach(IDataParameter param in cmd.Parameters)
			{
				list.Add(param.ParameterName.Substring(declarationLength));
			}
				
			list.Sort();
			return list;
		}

		private IDbCommand SelectCommand(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, DataStructureSortSet sort, bool paging,
            string columnName)
		{
			DbDataAdapter adapter = this.CreateDataAdapter(ds, entity, filter, sort, 
                new Hashtable(), paging, columnName, false);
			return ((IDbDataAdapter)adapter).SelectCommand;
		}

		public ArrayList Parameters(DataStructure ds, DataStructureEntity entity,
            DataStructureFilterSet filter, DataStructureSortSet sort, bool paging,
            string columnName)
		{
			IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
			return Parameters(cmd);
		}

		private void SelectParameterDeclarationsSql(StringBuilder result, Hashtable ht, 
            DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter, 
            DataStructureSortSet sort, bool paging, string columnName)
		{
			IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
			ArrayList list = Parameters(cmd);
			foreach(string paramName in list)
			{
				IDataParameter param = cmd.Parameters[this.ParameterDeclarationChar + paramName] as IDataParameter;

				if(!ht.Contains(param.ParameterName))
				{
					result.AppendFormat("DECLARE {0} AS {1}{2}",
						param.ParameterName,
						SqlDataType(param as System.Data.SqlClient.SqlParameter),
						Environment.NewLine);

					ht.Add(param.ParameterName, null);
				}
			}
		}

		private string SqlDataType(System.Data.SqlClient.SqlParameter param)
		{
			string result = param.SqlDbType.ToString();

			if(param.DbType == DbType.String)
			{
				result += "(" + param.Size.ToString() + ")";
			}

			return result;
		}

		private void SelectParameterDeclarationsSetSql(StringBuilder result, Hashtable parameters)
		{
			foreach(string name in parameters.Keys)
			{
				result.AppendFormat("SET {0} = NULL{1}", name, Environment.NewLine);
			}
		}

		public string SelectSql(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, DataStructureSortSet sortSet, string scalarColumn, 
            Hashtable parameters, Hashtable selectParameterReferences,
            bool forceDatabaseCalculation)
		{
			return SelectSql(ds, entity, filter, sortSet, scalarColumn, null, parameters,
				selectParameterReferences, true, false, false, forceDatabaseCalculation);
		}

		public string SelectSql(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, DataStructureSortSet sortSet, string scalarColumn, 
            Hashtable parameters, Hashtable selectParameterReferences, bool paging,
            bool forceDatabaseCalculation)
		{
			return SelectSql(ds, entity, filter, sortSet, scalarColumn, null, parameters,
				selectParameterReferences, true, paging, false, forceDatabaseCalculation);
		}

		public string SelectSql(DataStructure ds, DataStructureEntity entity, 
            DataStructureFilterSet filter, DataStructureSortSet sortSet, string scalarColumn, 
            Hashtable replaceParameterTexts, Hashtable dynamicParameters, 
            Hashtable selectParameterReferences, bool forceDatabaseCalculation)
		{
			return SelectSql(ds, entity, filter, sortSet, scalarColumn, replaceParameterTexts, 
                dynamicParameters, selectParameterReferences, true, false, false,
                forceDatabaseCalculation);
		}

		private string SelectSql(DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter, 
			DataStructureSortSet sortSet, string scalarColumn, Hashtable replaceParameterTexts, 
            Hashtable dynamicParameters, Hashtable selectParameterReferences, bool restrictScalarToTop1, 
            bool paging, bool isInRecursion, bool forceDatabaseCalculation)
		{
			if(!(entity.EntityDefinition is TableMappingItem))
			{
				throw new Exception("Only database mapped entities can be processed by the Data Service!");
			}

			if(paging)
			{
				if(PageNumberParameterReference.PersistenceProvider == null)
				{
					PageNumberParameterReference.PersistenceProvider = ds.PersistenceProvider;
					PageSizeParameterReference.PersistenceProvider = ds.PersistenceProvider;
					_pageNumberParameterName = ParameterReferenceChar + PageNumberParameterReference.Parameter.Name;
					_pageSizeParameterName = ParameterReferenceChar + PageSizeParameterReference.Parameter.Name;
				}

				selectParameterReferences.Add(_pageNumberParameterName, PageNumberParameterReference);
				selectParameterReferences.Add(_pageSizeParameterName, PageSizeParameterReference);
			}

			StringBuilder sqlExpression;
			StringBuilder orderByBuilder = new StringBuilder();
			StringBuilder groupByBuilder = new StringBuilder();

			sqlExpression = new StringBuilder();
            // when processing lookup columns we process semicolon delimited list of columns
            // to be returned as a single concatted field
            // Example: FirstName;Name -> concat(FirstName, ', ', Name)
            bool concatScalarColumns = restrictScalarToTop1;
			// Select
			bool isAggregate = RenderSelectColumns(ds, sqlExpression, orderByBuilder, 
                groupByBuilder, entity, scalarColumn, replaceParameterTexts, dynamicParameters,
                sortSet, selectParameterReferences, isInRecursion, concatScalarColumns, 
                forceDatabaseCalculation);
			
			// paging column
			if(paging)
			{
                if (sortSet == null)
                {
                    sqlExpression.AppendFormat(", ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS {0}", RowNumColumnName);
                }
                else
                {
                    sqlExpression.AppendFormat(", ROW_NUMBER() OVER (ORDER BY {0}) AS {1}", orderByBuilder.ToString(), RowNumColumnName);
                }
			}

            // From

            //StringBuilder tmp = new StringBuilder();
            //RenderSelectFromClause(sqlExpression, entity.RootEntity, entity, filter, replaceParameterTexts);
			RenderSelectFromClause(sqlExpression, entity, entity, filter, replaceParameterTexts);
			//sqlExpression.Append(tmp.ToString());
			
			bool whereExists = false;

			if(!entity.ParentItem.PrimaryKey.Equals(ds.PrimaryKey))
			{
				// render joins that we need for fields in this entity
				foreach(DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
				{
					if(relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
					{
						RenderSelectRelation(sqlExpression, relation, relation, filter, replaceParameterTexts, true, true, 0, false, dynamicParameters, selectParameterReferences);
					}
				}

                // if this is not a root entity, we make "where exists( )...to parent entities, so only detail records 
                // are selected for their master records
                RenderSelectExistsClause(sqlExpression, entity.RootEntity, entity, filter, replaceParameterTexts, dynamicParameters, selectParameterReferences);
				whereExists = true;
			}
			else
			{
				// for the root entity we render all child relation filters (filterParent relations)
				StringBuilder joinedFilterBuilder = new StringBuilder();
				int counter = 0;
				foreach(DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
				{
					if(relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
					{
						RenderSelectRelation(sqlExpression, relation, relation, filter, replaceParameterTexts, true, true, 0, false, dynamicParameters, selectParameterReferences);
					}
					else if(relation.RelationType == RelationType.FilterParent || relation.RelationType == RelationType.NotExists)
					{
						bool skip = false;

						skip = IgnoreConditionalEntity(relation, dynamicParameters);

						// skip dynamic filter parts
						if(! skip && relation.IgnoreCondition != DataStructureIgnoreCondition.None)
						{
							skip = IgnoreEntityWhenNoFilters(relation, filter, dynamicParameters);
						}

						// process all joined entities except all those dynamically skipped
						if(! skip)
						{
							if(counter > 0)
							{
								joinedFilterBuilder.Append(" AND ");
							}

							string existsClause = null;
							switch(relation.RelationType)
							{
								case RelationType.FilterParent:
									existsClause = " EXISTS";
									break;

								case RelationType.NotExists:
									existsClause = " NOT EXISTS";
									break;
							}

							joinedFilterBuilder.AppendFormat(existsClause + " (SELECT * FROM {0} AS {1}",
								RenderExpression(relation.EntityDefinition, null, null, null, null),
								NameLeftBracket + relation.Name + NameRightBracket);

							RenderSelectRelation(joinedFilterBuilder, relation, relation, filter, replaceParameterTexts, true, true, 0, false, dynamicParameters, selectParameterReferences);

							joinedFilterBuilder.Append(")");

							counter++;
						}
					}
				}

				if(joinedFilterBuilder.Length > 0)
				{
					if(whereExists)
					{
                        PrettyIndent(sqlExpression);
						sqlExpression.Append("AND ");
					}
					else
					{
                        PrettyLine(sqlExpression);
						sqlExpression.Append("WHERE ");
						whereExists = true;
					}

					sqlExpression.Append(joinedFilterBuilder.ToString());
				}
			}

			//			else
			//			{
			//				// Where filter - only on root entity, all other filters are on relations
			StringBuilder whereBuilder = new StringBuilder();
			RenderSelectWherePart(whereBuilder, entity, filter, replaceParameterTexts, dynamicParameters, selectParameterReferences);

			if(whereBuilder.Length > 0)
			{
				if(whereExists)
                {
                    PrettyIndent(sqlExpression);
                    sqlExpression.Append("AND ");
                }
                else
                {
                    PrettyLine(sqlExpression);
                    sqlExpression.Append("WHERE ");
                }

                sqlExpression.Append(whereBuilder.ToString());
			}
			//			}

			// GROUP BY
			if(groupByBuilder.Length > 0)
			{
                PrettyLine(sqlExpression);
                sqlExpression.AppendFormat("GROUP BY {0}", groupByBuilder.ToString());
			}

			// ORDER BY
			if((!paging || paging && PagingCanIncludeOrderBy) && orderByBuilder.Length > 0)
			{
                PrettyLine(sqlExpression);
                sqlExpression.AppendFormat("ORDER BY {0}", orderByBuilder.ToString());
			}

			string finalString = sqlExpression.ToString();

			// subqueries, etc. will have TOP 1, so it is sure that they select only 1 value
			if(scalarColumn != null && restrictScalarToTop1)
			{
				finalString = SelectClause(finalString, 1);
			}
			else
			{
				finalString = SelectClause(finalString, 0);
			}

			if(paging)
			{
				finalString = string.Format(
					"SELECT * FROM ({0}) _page WHERE _page.{1} BETWEEN (({2} - 1) * {3}) + 1 AND {3} * {2}",
					finalString, RowNumColumnName, _pageNumberParameterName, _pageSizeParameterName);
			}

			return finalString;
		}

		private bool IgnoreEntityWhenNoFilters(DataStructureEntity relation, DataStructureFilterSet filter, Hashtable dynamicParameters)
		{
			// If IgnoreWhenNoFilters is on and there is no filter for this entity, we skip it.
			// This is important for dynamic queries, where filters depend on actual parameters (e.g. null value means not filtering at all).
			// When no filter, then also the whole relation (which would be inner join, thus limiting the parent rows) is ignored.

			bool ignoreImplicitFilters = relation.IgnoreCondition == DataStructureIgnoreCondition.IgnoreWhenNoExplicitFilters;
			
			int filterCount = Filters(filter, relation, dynamicParameters, ignoreImplicitFilters).Count;

			if(filterCount > 0) return false;

			// we test for child entities as well
			foreach(DataStructureEntity childEntity in relation.ChildItemsByTypeRecursive(DataStructureEntity.ItemTypeConst))
			{
				filterCount += Filters(filter, childEntity, dynamicParameters, ignoreImplicitFilters).Count;

				// some filters found, we break
				if(filterCount > 0) return false;
			}

			return true;
		}

		private bool IgnoreConditionalEntity(DataStructureEntity relation, Hashtable dynamicParameters)
		{
			// skip dynamic entity relations
			IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

			string constant = null;

			if(parameterService != null & relation.ConditionEntityConstant != null)
			{
				constant = (string)parameterService.GetParameterValue(relation.ConditionEntityConstantId, OrigamDataType.String);
			}

			if(relation.ConditionEntityParameterName != null)
			{
				if(dynamicParameters[relation.ConditionEntityParameterName] == null & constant == null)
				{
					return true;
				}
				else if(dynamicParameters[relation.ConditionEntityParameterName] == null | constant == null)
				{
					return false;
				}
				else if(dynamicParameters[relation.ConditionEntityParameterName].ToString() != constant)
				{
					return true;
				}
			}

			return false;
		}

		public string UpsertSql(DataStructure ds, DataStructureEntity entity)
		{
			StringBuilder sqlExpression = new StringBuilder();
			StringBuilder keysBuilder = new StringBuilder();
			StringBuilder searchPredicatesBuilder = new StringBuilder();
			StringBuilder updateBuilder = new StringBuilder();
			StringBuilder insertColumnsBuilder = new StringBuilder();
			StringBuilder insertValuesBuilder = new StringBuilder();
			string tableName = RenderExpression(entity.EntityDefinition, null, null, null, null);

			int updateColumns = 0;
			int insertColumns = 0;
			int keys = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(ShouldUpdateColumn(column, entity))
				{
					string fieldName = RenderExpression(column.Field, null, null, null, null);
					string paramName = NewValueParameterName(column, false);
					
					if(column.Field.AutoIncrement == false)
					{
						if(insertColumns > 0)
						{
							insertColumnsBuilder.Append(", ");
							insertValuesBuilder.Append(", ");
						}
						insertColumnsBuilder.Append(fieldName);
						insertValuesBuilder.Append(paramName);
						insertColumns++;
					}

					UpsertType type = column.UpsertType;
					if(entity.AllFields && column.Field.IsPrimaryKey)
					{
						type = UpsertType.Key;
					}

					if(type != UpsertType.Key && type != UpsertType.InsertOnly)
					{
						if(updateColumns > 0)
						{
							updateBuilder.Append(", ");
						}
						updateColumns++;
					}

					switch(type)
					{
						case UpsertType.Key:
							if(keys > 0)
							{
								keysBuilder.Append(", ");
								searchPredicatesBuilder.Append(" AND ");
							}
							keysBuilder.AppendFormat("{0} as {1}", 
								paramName,
								fieldName);
							searchPredicatesBuilder.AppendFormat("{0}.{1} = src.{1}",
								tableName, fieldName);
							keys++;
							break;
						case UpsertType.Replace:
							updateBuilder.AppendFormat("{0} = {1}", fieldName, paramName);
							break;
						case UpsertType.Increase:
							updateBuilder.AppendFormat("{0} = {0} + {1}", fieldName, paramName);
							break;
						case UpsertType.Decrease:
							updateBuilder.AppendFormat("{0} = {0} - {1}", fieldName, paramName);
							break;
						case UpsertType.InsertOnly:
							break;
						default:
							throw new ArgumentOutOfRangeException("UpsertType", column.UpsertType, "Unknown UpsertType");
					}
				}
			}

			if(keys == 0)
			{
				throw new Exception("Cannot build an UPSERT command, no UPSERT keys specified in the entity.");
			}

			sqlExpression.AppendFormat("MERGE INTO {0} USING (SELECT {1}) AS src ON {2} WHEN MATCHED THEN UPDATE SET {3} WHEN NOT MATCHED THEN INSERT ({4}) VALUES ({5});",
				tableName,
				keysBuilder.ToString(),
				searchPredicatesBuilder.ToString(),
				updateBuilder.ToString(),
				insertColumnsBuilder.ToString(),
				insertValuesBuilder.ToString()
				);

			return sqlExpression.ToString();
		}

		public string InsertSql(DataStructure ds, DataStructureEntity entity)
		{
			if(entity.UseUPSERT)
			{
				return UpsertSql(ds, entity);
			}
			StringBuilder sqlExpression = new StringBuilder();
			StringBuilder sqlExpression2 = new StringBuilder();
			sqlExpression.AppendFormat("INSERT INTO {0} (",
				RenderExpression(entity.EntityDefinition, null, null, null, null)
				);
			bool existAutoIncrement = false;
			int i = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(ShouldUpdateColumn(column, entity) && column.Field.AutoIncrement == false)
				{
					if(i > 0)
					{
						sqlExpression.Append(",");
						sqlExpression2.Append(",");
                    }
                    PrettyIndent(sqlExpression);
                    PrettyIndent(sqlExpression2);
                    sqlExpression.Append(RenderExpression(column.Field, null, null, null, null));
					sqlExpression2.Append(NewValueParameterName(column, false));
					i++;
					if(column.Field.AutoIncrement)	existAutoIncrement = true;
				}
			}
            PrettyLine(sqlExpression);
			sqlExpression.Append(") VALUES (");
			sqlExpression.Append(sqlExpression2.ToString());
			sqlExpression.Append(")");
			// If there is any auto increment column, we include a SELECT statement after INSERT
			if(existAutoIncrement)
			{
				RenderSelectUpdatedData(sqlExpression, entity);
			}
			return sqlExpression.ToString();
		}

		private void RenderSelectUpdatedData(StringBuilder sqlExpression, DataStructureEntity entity)
		{
			ArrayList primaryKeys = new ArrayList();
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field.IsPrimaryKey) primaryKeys.Add(column);
			}
			if(primaryKeys.Count == 0)
			{
				throw new OrigamException(ResourceUtils.GetString("NoPrimaryKey", entity.Name));
			}
            PrettyLine(sqlExpression);
			sqlExpression.Append("; SELECT @@IDENTITY AS " + ((DataStructureColumn)primaryKeys[0]).Name);
		}

		public string SelectRowSql(DataStructureEntity entity, Hashtable selectParameterReferences,
            string columnName, bool forceDatabaseCalculation)
		{
			StringBuilder sqlExpression = new StringBuilder();

			ArrayList primaryKeys = new ArrayList();
			sqlExpression.Append("SELECT ");

			this.RenderSelectColumns(entity.RootItem as DataStructure, sqlExpression, new StringBuilder(), 
                new StringBuilder(), entity, columnName, new Hashtable(), new Hashtable(), null, 
                selectParameterReferences, forceDatabaseCalculation);

			int i = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
				{
					if(column.Field.IsPrimaryKey) primaryKeys.Add(column);

					i++;
				}
			}
            PrettyLine(sqlExpression);
			sqlExpression.AppendFormat("FROM {0} AS {1} ", 
				RenderExpression(entity.EntityDefinition, null, null, null, null),
				NameLeftBracket + entity.Name + NameRightBracket
				);

			foreach(DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
			{
				if(relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
				{
					RenderSelectRelation(sqlExpression, relation, relation, null, null, true, true, 0, false, null, selectParameterReferences);
				}
			}
            PrettyLine(sqlExpression);
			sqlExpression.Append("WHERE (");

			i = 0;
			foreach(DataStructureColumn column in primaryKeys)
			{
				if(i > 0) sqlExpression.Append(" AND");
                PrettyIndent(sqlExpression);
				sqlExpression.AppendFormat("{0}.{1} = {2}",
					NameLeftBracket + entity.Name + NameRightBracket,
					RenderExpression(column.Field, null, null, null, null),
					NewValueParameterName(column, false)
					);
				i++;
			}

			sqlExpression.Append(")");

			return sqlExpression.ToString();
		}

		public string SelectUpdateFieldSql(TableMappingItem table, FieldMappingItem updatedField)
		{
			DataStructureEntity entity = new DataStructureEntity();
			entity.PersistenceProvider = table.PersistenceProvider;
			entity.Name = table.Name;
			entity.Entity = table;

			StringBuilder sqlExpression = new StringBuilder();

			ArrayList selectKeys = new ArrayList();
			sqlExpression.Append("SELECT ");

			int i = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				// we only select the primary key and the changed field
				if(column.Field.IsPrimaryKey | column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
				{
					if(i > 0) sqlExpression.Append(", ");

					if(column.Field.PrimaryKey.Equals(updatedField.PrimaryKey)) selectKeys.Add(column);

					sqlExpression.AppendFormat("{0} AS {1}", 
						RenderExpression(column.Field, entity, null, null, null),
						NameLeftBracket + column.Name + NameRightBracket
						);

					i++;
				}
			}

			sqlExpression.AppendFormat(" FROM {0} AS {1} WHERE (", 
				RenderExpression(entity.EntityDefinition, null, null, null, null),
				NameLeftBracket + entity.Name + NameRightBracket
				);

			i = 0;
			foreach(DataStructureColumn column in selectKeys)
			{
				if(i > 0) sqlExpression.Append(" AND ");

				sqlExpression.AppendFormat("{0} = {1}",
					RenderExpression(column.Field, null, null, null, null),
					NewValueParameterName(column, false)
					);
				i++;
			}

			sqlExpression.Append(")");

			return sqlExpression.ToString();
		}

		public string SelectReferenceCountSql(TableMappingItem table, FieldMappingItem updatedField)
		{
			DataStructureEntity entity = new DataStructureEntity();
			entity.PersistenceProvider = table.PersistenceProvider;
			entity.Name = table.Name;
			entity.Entity = table;

			StringBuilder sqlExpression = new StringBuilder();

			ArrayList selectKeys = new ArrayList();
			sqlExpression.Append("SELECT COUNT(*) ");

			foreach(DataStructureColumn column in entity.Columns)
			{
				if(column.Field.PrimaryKey.Equals(updatedField.PrimaryKey)) 
				{
					selectKeys.Add(column);
				}
			}

			sqlExpression.AppendFormat(" FROM {0} AS {1} WHERE (", 
				RenderExpression(entity.EntityDefinition, null, null, null, null),
				NameLeftBracket + entity.Name + NameRightBracket
				);

			int i = 0;
			foreach(DataStructureColumn column in selectKeys)
			{
				if(i > 0) sqlExpression.Append(" AND ");

				sqlExpression.AppendFormat("{0} = {1}",
					RenderExpression(column.Field, null, null, null, null),
					NewValueParameterName(column, false)
					);
				i++;
			}

			sqlExpression.Append(")");

			return sqlExpression.ToString();
		}

		public string DeleteSql(DataStructure ds, DataStructureEntity entity)
		{
			StringBuilder sqlExpression = new StringBuilder();
		
			sqlExpression.AppendFormat("DELETE FROM {0} ",
				RenderExpression(entity.EntityDefinition, null, null, null, null)
				);

			RenderUpdateDeleteWherePart(sqlExpression, entity);

			return sqlExpression.ToString();
		}

		public string UpdateSql(DataStructure ds, DataStructureEntity entity)
		{
			StringBuilder sqlExpression = new StringBuilder();

			sqlExpression.AppendFormat("UPDATE {0} SET ", 
				RenderExpression(entity.EntityDefinition, null, null, null, null));

			bool existAutoIncrement = false;

			int i = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(ShouldUpdateColumn(column, entity))
				{
                    string field = RenderExpression(column.Field, null, null, null, null);
                    string parameter = NewValueParameterName(column, false);
                    if (i > 0)
					{
						sqlExpression.Append(",");
                    }
                    PrettyIndent(sqlExpression);
                    if (column.IsWriteOnly)
                    {
                        // Check dependencies only on WriteOnly fields.
                        // This is the only way to empty a write-only field to make it dependent
                        // on something else. E.g. make blob field dependent ona file name field.
                        // When no file name, then the blob field will be emptied. Without dependency it would
                        // not touch the write only field.
                        const string writeOnlyValue = "WHEN {1} IS NULL THEN {0} ELSE {1}";
                        ArrayList dependenciesSource = column.Field.ChildItemsByType(EntityFieldDependency.ItemTypeConst);
                        ArrayList dependencies = new ArrayList();
                        // skip dependencies to virtual fields
                        foreach (EntityFieldDependency dep in dependenciesSource)
                        {
                            if (dep.Field is FieldMappingItem)
                            {
                                dependencies.Add(dep);
                            }
                        }
                        if (dependencies.Count == 0)
                        {
                            // no dependencies and the field is write only - in that case it is not possible
                            // to delete the contents of the field because empty = no change
                            sqlExpression.AppendFormat("{0} = (CASE " + writeOnlyValue + " END)",
                                field,
                                parameter
                                );
                        }
                        else
                        {
                            sqlExpression.AppendFormat("{0} = (CASE", field);
                            // if the field it depends on is empty this field has to be emptied as well
                            foreach (EntityFieldDependency dep in dependencies)
                            {
                                foreach (DataStructureColumn dependentColumn in entity.Columns)
                                {
                                    if (dependentColumn.Field.Name == dep.Field.Name
                                        && ShouldUpdateColumn(dependentColumn, entity))
                                    {
                                        sqlExpression.AppendFormat(" WHEN {0} IS NULL THEN NULL",
                                            NewValueParameterName(dependentColumn, false));
                                        break;
                                    }
                                }
                            }
                            sqlExpression.AppendFormat(" " + writeOnlyValue + " END)",
                                field,
                                parameter);
                        }
                    }
                    else
                    {
                        // simple field, just update
                        sqlExpression.AppendFormat("{0} = {1}",
                            field,
                            parameter
                            );
                    }
					i++;
					if(column.Field.AutoIncrement)	existAutoIncrement = true;
				}
			}
			RenderUpdateDeleteWherePart(sqlExpression, entity);

			// If there is any auto increment column, we include a SELECT statement after INSERT
			if(existAutoIncrement)
			{
				RenderSelectUpdatedData(sqlExpression, entity);
			}

			return sqlExpression.ToString();
		}

		private bool ShouldUpdateColumn(DataStructureColumn column, DataStructureEntity entity)
		{
			if(! (column.Field is FieldMappingItem)) return false;
			if(column.UseLookupValue) return false;
			if(column.UseCopiedValue) return false;
			if(column.Entity != null) return false;

			return true;
		}
		
		#endregion

		#region Select parts
		private bool RenderSelectColumns(DataStructure ds, StringBuilder sqlExpression, 
            StringBuilder orderByBuilder, StringBuilder groupByBuilder, DataStructureEntity entity, 
            string scalarColumn, Hashtable replaceParameterTexts, Hashtable dynamicParameters, 
            DataStructureSortSet sortSet, Hashtable selectParameterReferences, 
            bool forceDatabaseCalculation)
		{
			return RenderSelectColumns(ds, sqlExpression, orderByBuilder, groupByBuilder, 
                entity, scalarColumn, replaceParameterTexts, dynamicParameters, sortSet, 
                selectParameterReferences, false, true, forceDatabaseCalculation);
		}

		private bool RenderSelectColumns(DataStructure ds, StringBuilder sqlExpression, 
            StringBuilder orderByBuilder, StringBuilder groupByBuilder, DataStructureEntity entity, 
            string scalarColumn, Hashtable replaceParameterTexts, Hashtable dynamicParameters, 
            DataStructureSortSet sortSet, Hashtable selectParameterReferences, bool isInRecursion,
            bool concatScalarColumns, bool forceDatabaseCalculation)
		{

			int i = 0;
			ArrayList group = new ArrayList();
			SortedList order = new SortedList();
			bool groupByNeeded = false;
            ArrayList scalarColumnNames = new ArrayList();
            if (scalarColumn != null)
            {
                scalarColumnNames.AddRange(scalarColumn.Split(";".ToCharArray()));
            }
			if(concatScalarColumns && scalarColumnNames.Count > 1)
			{
				List<KeyValuePair<ISchemaItem, DataStructureEntity>> scalarColumns =
					new List<KeyValuePair<ISchemaItem, DataStructureEntity>>();

				foreach(string scalar in scalarColumnNames)
				{
					foreach(DataStructureColumn column in entity.Columns)
					{
						if(column.Name == scalar)
						{
							scalarColumns.Add(new KeyValuePair<ISchemaItem, DataStructureEntity>
								(column.Field, (column.Entity == null) ? entity : column.Entity));
							break;
						}
					}
				}
				sqlExpression.Append(" ");
				sqlExpression.Append(RenderConcat(scalarColumns, RenderString(", "), 
                    replaceParameterTexts, dynamicParameters, selectParameterReferences));
				return false;
			}
			foreach(DataStructureColumn column in entity.Columns)
			{
				bool processColumn = false;
				FunctionCall functionCall = column.Field as FunctionCall;
				AggregatedColumn aggregatedColumn = column.Field as AggregatedColumn;
				// parent column references will be calculated by the dataset
				// generator
				if(column.IsWriteOnly)
				{
					processColumn = false;
				}
				else if(column.IsFromParentEntity())
				{
					processColumn = false;
				}
				else if(scalarColumnNames.Contains(column.Name))
				{
					processColumn = true;
				}
				else if(scalarColumn == null &&
					(
					column.Field is FieldMappingItem
					||
					column.Field is LookupField
					||
					(
					functionCall != null
					&& functionCall.Function.FunctionType == OrigamFunctionType.Database
					)
					||
					(
					functionCall != null && functionCall.ForceDatabaseCalculation
					)
					||
					(
					functionCall != null && forceDatabaseCalculation
					)
					||
					(
					functionCall != null && column.Entity != null
					)
                    ||
                    (
                    functionCall != null && column.Aggregation != AggregationType.None
                    )
                    )
					)
				{
					processColumn = true;
				}
				else if(scalarColumn == null && aggregatedColumn != null)
				{
					bool found = false;
					foreach(DataStructureEntity childEntity in entity.ChildItemsByType(DataStructureEntity.ItemTypeConst))
					{
						// if we have an aggregation column and
						// and the aggregation sub-entity with source field
						// exist in our DS, then we don't render sql for the
						// column, but just rely on dataset aggregation computation
						if(childEntity.Entity.PrimaryKey.Equals(aggregatedColumn.Relation.PrimaryKey))
						{
							// search for aggregation source column in related entity 
							if (childEntity.ExistsEntityFieldAsColumn(aggregatedColumn.Field))
							{
								found = true;
								break;
							}
						}
					}

					if(! found) processColumn = true;
				}

				string resultExpression = "";
				string groupExpression = "";

				if(processColumn || column.IsColumnSorted(sortSet))
				{
					if(column.UseLookupValue)
					{
						resultExpression = RenderLookupColumnExpression(ds, entity, column, 
							replaceParameterTexts, dynamicParameters, selectParameterReferences);
						// if we would group by lookuped column, we use original column in group-by clause
						groupExpression = RenderExpression(column.Field as AbstractSchemaItem,
							column.Entity == null ? entity : column.Entity, replaceParameterTexts,
							dynamicParameters, selectParameterReferences); ;
					}
					else
					{
						resultExpression = RenderExpression(column.Field as AbstractSchemaItem, column.Entity == null ? entity : column.Entity, replaceParameterTexts, dynamicParameters, selectParameterReferences);
						groupExpression = resultExpression;

						if(column.Aggregation != AggregationType.None)
						{
							if(column.Field is AggregatedColumn)
							{
								throw new NotSupportedException(ResourceUtils.GetString("ErrorAggregInAggreg", column.Path));
							}
							resultExpression = FixAggregationDataType(column.DataType, resultExpression);
							resultExpression = FixSumAggregation(column.Aggregation, GetAggregationString(column.Aggregation) + "(" + resultExpression + ")");
							groupByNeeded = true;
						}	
					}
					if (column.DataType == OrigamDataType.Geography)
					{
						if (!isInRecursion)
						{
							// convert to text, becouse .net didn't have geolocation data type
							resultExpression = ConvertGeoToTextClause(resultExpression);
						}
					}
						
					if(processColumn)
                    {
                        if (i > 0) sqlExpression.Append(",");
                        PrettyIndent(sqlExpression);
                        i++;
                        sqlExpression.AppendFormat("{0} AS {1}",
                            resultExpression,
                            NameLeftBracket + column.Name + NameRightBracket
                            );
                        // anything not having aggregation will eventually go to GROUP BY
                        if (column.Aggregation == AggregationType.None)
                        {
                            group.Add(groupExpression);
                        }
                    }

                    //					if(column.Name == scalarColumn)
                    //					{
                    //						break;
                    //					}
                }

				// does not matter if processColumn=true, because we want to sort anytime sorting is specified,
				// e.g. if this is a scalar query and sorting is by another than the scalar column
				if(column.IsColumnSorted(sortSet))
				{
					System.Diagnostics.Debug.Assert(resultExpression != String.Empty, "No expression generated for sorting.", "Column: " + column.Path);
					SortOrder sortOrder;
					string sortExpression = resultExpression;
					// if the column is a lookup column, we will sort by the looked-up
					// value, not by the source value, this will bring the same logic
					// as in the UI - when user sorts, it will always sort by a looked-up
					// values
					if(column.FinalLookup != null && ! column.UseLookupValue)
					{
						sortExpression = RenderLookupColumnExpression(ds, entity, column, 
							replaceParameterTexts, dynamicParameters, selectParameterReferences);
					}
					sortOrder.Expression = sortExpression;
					sortOrder.SortDirection = column.SortDirection(sortSet);
					
					if(order.Contains(column.SortOrder(sortSet)))
					{
						throw new InvalidOperationException(ResourceUtils.GetString("ErrorSortOrder", column.SortOrder(sortSet).ToString(), column.Path));
					}

					order.Add(column.SortOrder(sortSet), sortOrder);
				}
			}

			if(order.Count > 0)
			{
				i = 0;
				foreach(DictionaryEntry entry in order)
				{
					if(i > 0)
                    {
                        orderByBuilder.Append(",");
                    }
                    PrettyIndent(orderByBuilder);
                    orderByBuilder.AppendFormat("{0} {1}",
						((SortOrder)entry.Value).Expression,
						RenderSortDirection(((SortOrder)entry.Value).SortDirection)
						);

					i++;
				}
			}
			if(groupByNeeded)
			{
				i = 0;
				foreach(string expression in group)
				{
					if(i > 0)
                    {
                        groupByBuilder.Append(",");
                    }
                    PrettyIndent(groupByBuilder);
                    groupByBuilder.Append(expression);
					i++;
				}
			}
			return groupByNeeded;
		}

        private void PrettyLine(StringBuilder sqlExpression)
        {
            if (PrettyFormat)
            {
                sqlExpression.AppendLine();
                sqlExpression.Append(new string('\t', _indentLevel));
            }
            else
            {
                sqlExpression.Append(" ");
            }
        }

        private void PrettyIndent(StringBuilder sqlExpression)
        {
            if (PrettyFormat)
            {
                sqlExpression.AppendLine();
                sqlExpression.Append(new string('\t', _indentLevel + 1));
            }
            else
            {
                sqlExpression.Append(" ");
            }
        }

        private string FixAggregationDataType(OrigamDataType dataType, string expression)
		{
			switch(dataType)
			{
				case OrigamDataType.UniqueIdentifier:
					return "CAST(" + expression + " AS NVARCHAR(36))";
				case OrigamDataType.Boolean:
					return "CAST(" + expression + " AS INT)";
				default:
					return expression;
			}
		}

		private string FixSumAggregation(AggregationType aggregationType, string expression)
		{
			if(aggregationType == AggregationType.Sum)
			{
				return "ISNULL(" + expression + ", 0)";
			}
			else
			{
				return expression;
			}
		}

		private string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity,
			DataStructureColumn column, Hashtable replaceParameterTexts, Hashtable dynamicParameters, 
			Hashtable parameterReferences)
		{
			if(column.Aggregation != AggregationType.None)
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorLookupAggreg", column.Path));
			}

			return RenderLookupColumnExpression(ds, column.Entity == null ? entity : column.Entity, column.Field,
				column.DefaultLookup == null ? column.Field.DefaultLookup : column.DefaultLookup,
				replaceParameterTexts, dynamicParameters, parameterReferences);
		}


		private string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity, IDataEntityColumn field,
			IDataLookup lookup, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
			Hashtable parameterReferences)
		{
			return RenderLookupColumnExpression(ds, entity, field, lookup, replaceParameterTexts, dynamicParameters,
				parameterReferences, false);
		}		

		private string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity, IDataEntityColumn field, 
			IDataLookup lookup, Hashtable replaceParameterTexts, Hashtable dynamicParameters, 
			Hashtable parameterReferences, bool isInRecursion)
		{
			DataServiceDataLookup dataServiceLookup = lookup as DataServiceDataLookup;

			if(dataServiceLookup == null)
			{
				throw new ArgumentOutOfRangeException("DefaultLookup", lookup, ResourceUtils.GetString("LookupTypeUnsupportedException"));
			}

			if(dataServiceLookup.ValueMethod != null && ! (dataServiceLookup.ValueMethod is DataStructureFilterSet))
			{
				throw new ArgumentOutOfRangeException("ListMethod", dataServiceLookup.ListMethod, ResourceUtils.GetString("LookupListMethodTypeUnsupportedException"));
			}

			DataStructureFilterSet valueFilterSet = dataServiceLookup.ValueMethod as DataStructureFilterSet;
			DataStructureEntity lookupEntity = dataServiceLookup.ValueDataStructure.Entities[0] as DataStructureEntity;

			// any lookups with same entity name as any of the entities in this datastructure must be renamed
			bool lookupRenamed = false;

			foreach(DataStructureEntity e in ds.Entities)
			{
				if(e.Name == lookupEntity.Name)
				{
					lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
					lookupEntity.Name = "lookup" + lookupEntity.Name;

					lookupRenamed = true;
					break;
				}
			}

			Hashtable replaceTexts = new Hashtable(1);
			
			AbstractSchemaItem renderField = field as AbstractSchemaItem;
			if(field is LookupField)
			{
				renderField = (field as LookupField).Field as AbstractSchemaItem;
			}
			string myColumn = RenderExpression(renderField, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

			if(dataServiceLookup.ValueMethod == null)
			{
				throw new ArgumentOutOfRangeException("ValueFilterSet", null, ResourceUtils.GetString("NoValueFilterSetForLookup", dataServiceLookup.Path));
			}

			// replace lookup parameters with keys from the entity
			foreach(object key in dataServiceLookup.ValueMethod.ParameterReferences.Keys)
			{
				string finalKey = key as string;
				if(lookupRenamed)
				{
					if(finalKey != null)
					{
						finalKey = "lookup" + finalKey;
					}
				}

				// check if the parameters are not system (custom) parameters
				if(CustomParameterService.MatchParameter(finalKey) == null)
				{
					replaceTexts.Add(finalKey, myColumn);
				}
			}
            _indentLevel++;
            StringBuilder builder = new StringBuilder();
            try
            {
                builder.Append("("
                    + SelectSql(dataServiceLookup.ValueDataStructure,
                    lookupEntity,
                    valueFilterSet,
                    dataServiceLookup.ValueSortSet,
                    dataServiceLookup.ValueDisplayMember,
                    replaceTexts,
                    dynamicParameters,
                    parameterReferences,
                    true, false, true, true)
                    );
            }
            finally
            {
                _indentLevel--;
            }
            PrettyIndent(builder);
            builder.Append(")");
            return builder.ToString(); ;
		}
		
		private void RenderSelectFromClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts)
		{
            PrettyLine(sqlExpression);
            sqlExpression.Append("FROM");
            PrettyIndent(sqlExpression);
			sqlExpression.AppendFormat("{0} AS {1}",
				RenderExpression(baseEntity.EntityDefinition, null, null, null, null),
				NameLeftBracket + baseEntity.Name + NameRightBracket);

			//			foreach(AbstractSchemaItem relation in (baseEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
			//			{
			//				if((relation as DataStructureEntity).RelationType == RelationType.FilterParent)
			//				{
			//					RenderSelectRelation(sqlExpression, relation as DataStructureEntity, stopAtEntity, filter, replaceParameterTexts, false, 0);
			//				}
			//			}
		}

		private void RenderSelectExistsClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
            PrettyLine(sqlExpression);
            sqlExpression.AppendFormat("WHERE EXISTS (SELECT * FROM {0} AS {1}",
				RenderExpression(baseEntity.Entity, null, null, null, null),
				NameLeftBracket + baseEntity.Name + NameRightBracket);

			bool stopAtIncluded = false;
			bool notExistsIncluded = false;

			foreach(DataStructureEntity relation in (baseEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
			{
				if(relation.RelationType != RelationType.LeftJoin)
				{
					if(relation.PrimaryKey.Equals(stopAtEntity.PrimaryKey))
					{
						// current entity we do after all the others, because it will depend on the others
						stopAtIncluded = true;
					}
					else if(relation.RelationType == RelationType.NotExists)
					{
						notExistsIncluded = true;
					}
					else
					{
						RenderSelectRelation(sqlExpression, relation, stopAtEntity, filter, replaceParameterTexts, true, false, 0, true, dynamicParameters, parameterReferences);
					}
				}
			}

			// finally we do current entity
			if(stopAtIncluded)
			{
				RenderSelectRelation(sqlExpression, stopAtEntity, stopAtEntity, filter, replaceParameterTexts, true, false, 0, false, dynamicParameters, parameterReferences);
			}

			if(notExistsIncluded)
			{
				int notExistsCount = (stopAtIncluded ? 1 : 0);
				foreach(DataStructureEntity relation in (baseEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
				{
					if(relation.RelationType == RelationType.NotExists)
					{
						string s;
						if(notExistsCount == 0)
						{
							s = " WHERE ";
						}
						else
						{
							s = " AND ";
						}

						sqlExpression.AppendFormat(s + "NOT EXISTS (SELECT * FROM {0} AS {1}",
							RenderExpression(relation.EntityDefinition, null, null, null, null),
							NameLeftBracket + relation.Name + NameRightBracket);

						RenderSelectRelation(sqlExpression, relation, relation, filter, replaceParameterTexts, true, false, 0, true, dynamicParameters, parameterReferences);

						sqlExpression.Append(")");
					}
				}
			}

			StringBuilder whereBuilder = new StringBuilder();
			RenderSelectWherePart(whereBuilder, baseEntity, filter, replaceParameterTexts, dynamicParameters, parameterReferences);

			if(whereBuilder.Length > 0)
			{
				sqlExpression.Append(" AND ");	
				sqlExpression.Append(whereBuilder.ToString());
			}

			sqlExpression.Append(")");
		}

		private bool CanSkipRelation(DataStructureEntity relation, DataStructureEntity stopAtEntity)
		{
			if(relation.RelationType != RelationType.Normal && relation.RelationType != RelationType.LeftJoin) return false;

			if(stopAtEntity.PrimaryKey.Equals(relation.PrimaryKey)) return false;
			
			return true;
		}

		private bool CanSkipSelectRelation(DataStructureEntity relation, DataStructureEntity stopAtEntity)
		{
			if(relation.RelationType != RelationType.Normal) return false;

			if(stopAtEntity.PrimaryKey.Equals(relation.PrimaryKey)) return false;
			
			return true;
		}

		private void RenderSelectRelation(StringBuilder sqlExpression, DataStructureEntity dsEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, bool skipStopAtEntity, bool includeFilter, int numberOfJoins, bool includeAllRelations, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			// we render the sub relation only if
			// 1. this relation is INNER JOIN (except when IgnoreWhenNoFilters = true and there ARE no filters)
			// 2. our target entity is one of sub-entities of this relation
			// 3. one of sub-relations of this relation is INNER JOIN (except when IgnoreWhenNoFilters = true and there ARE no filters)
			// 4. this is actually our stopAtEntity relation
			if(includeAllRelations)
			{
				if(! dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey))
				{
					if(dsEntity.RelationType == RelationType.Normal 
						|| dsEntity.RelationType == RelationType.LeftJoin)
					{
						if(! dsEntity.ChildItemsRecursive.Contains(stopAtEntity))
						{
							return;
						}
					}
				}
			}
			else
			{
				if(CanSkipSelectRelation(dsEntity, stopAtEntity))
				{
					return;
				}
			}

			// ignore conditional relations
			if(IgnoreConditionalEntity(dsEntity, dynamicParameters))
			{
				return;
			}

			// ignore relations when no filters
			if(dsEntity.IgnoreCondition != DataStructureIgnoreCondition.None)
			{
				if(IgnoreEntityWhenNoFilters(dsEntity, filter, dynamicParameters))
				{
					return;
				}
			}

			JoinBeginType beginType;
			if(skipStopAtEntity && dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey) 
				&& dsEntity.RelationType != RelationType.InnerJoin 
				&& dsEntity.RelationType != RelationType.LeftJoin
				)
			{
				if(numberOfJoins > 0)
				{
					beginType = JoinBeginType.And;
				}
				else
				{
					beginType = JoinBeginType.Where;
				}
			}
			else
			{
				beginType = JoinBeginType.Join;
			}

			IAssociation assoc = dsEntity.Entity as IAssociation;
			StringBuilder relationBuilder = new StringBuilder();
            PrettyIndent(relationBuilder);
            switch (beginType)
			{
				case JoinBeginType.Join:
					string joinString = (dsEntity.RelationType == RelationType.LeftJoin ? "LEFT OUTER JOIN" : "INNER JOIN");

					relationBuilder.AppendFormat("{0} {1} AS {2} ON",
						joinString,
						RenderExpression(assoc.AssociatedEntity as AbstractSchemaItem, null, null, null, null),
						NameLeftBracket + dsEntity.Name + NameRightBracket
						);
					numberOfJoins++;
					break;
				case JoinBeginType.Where:
					relationBuilder.Append("WHERE ");
					break;
				case JoinBeginType.And:
					relationBuilder.Append("AND ");
					break;
			}
			int i = 0;
			if(assoc.IsOR)
			{
				relationBuilder.Append("(");
			}

			foreach(AbstractSchemaItem item in assoc.ChildItems)
			{
                PrettyIndent(relationBuilder);
                if (i > 0)
				{
					if(assoc.IsOR)
					{
						relationBuilder.Append(" OR");
					}
					else
					{
						relationBuilder.Append(" AND");
					}
				}

				if(item is EntityRelationColumnPairItem)
				{
					RenderSelectRelationKey(relationBuilder,
						item as EntityRelationColumnPairItem, dsEntity.ParentItem as DataStructureEntity,
						dsEntity, replaceParameterTexts, dynamicParameters, parameterReferences);
				}
				else if(item is EntityRelationFilter)
				{
					RenderFilter(relationBuilder, (item as EntityRelationFilter).Filter, dsEntity, parameterReferences);
				}
				else
					throw new NotSupportedException(ResourceUtils.GetString("TypeNotSupportedByMS", item.GetType().ToString()));

				i++;
			}

			if(assoc.IsOR)
			{
				relationBuilder.Append(")");
			}
			
			if(!(dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey) & skipStopAtEntity & includeFilter == false))
			{
				StringBuilder whereBuilder = new StringBuilder();
				RenderSelectWherePart(whereBuilder, dsEntity, filter, replaceParameterTexts, dynamicParameters, parameterReferences);

				if(whereBuilder.Length > 0)
				{
                    PrettyIndent(relationBuilder);
                    relationBuilder.AppendFormat(" AND {0}", 
						whereBuilder.ToString());
				}

				// if this is our main entity, we check it's columns to reference parent entities
				// foreach(IAssociation parentRelation in dsEntity.UnresolvedParentRelations)
				// {
				//     // render relation keys, if there is a filter, throw exception
				// }

				// ArrayList - get parent entities 

				// for-each parent entity - render relation
			}

			StringBuilder recursionBuilder = new StringBuilder();
			// Let's go to recursion!
			foreach(DataStructureEntity relation in (dsEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
			{
				RenderSelectRelation(recursionBuilder, relation, stopAtEntity, filter, replaceParameterTexts, skipStopAtEntity, includeFilter, numberOfJoins, includeAllRelations, dynamicParameters, parameterReferences);
			}

			if(beginType == JoinBeginType.Join)
			{
				sqlExpression.Append(relationBuilder.ToString());
				sqlExpression.Append(recursionBuilder.ToString());
			}
			else
			{
				sqlExpression.Append(recursionBuilder.ToString());
				sqlExpression.Append(relationBuilder.ToString());
			}
		}

		private void RenderSelectRelationKey(StringBuilder sqlExpression, 
			EntityRelationColumnPairItem key,
			DataStructureEntity parentEntity, DataStructureEntity relatedEntity,
			Hashtable replaceParameterTexts, Hashtable dynamicParameters,
			Hashtable paremeterReferences)
		{
			string parentField;
			string relatedField;

			parentField = RenderExpression(key.BaseEntityField as AbstractSchemaItem, 
					parentEntity, replaceParameterTexts, dynamicParameters, paremeterReferences);

			relatedField = RenderExpression(key.RelatedEntityField as AbstractSchemaItem, 
					relatedEntity, replaceParameterTexts, dynamicParameters, paremeterReferences);

			sqlExpression.AppendFormat(" {0} = {1}",
				parentField,
				relatedField
				);
		}

		private void RenderUpdateDeleteWherePart(StringBuilder sqlExpression, DataStructureEntity entity)
		{
            PrettyLine(sqlExpression);
			sqlExpression.Append("WHERE (");
			int i = 0;
			foreach(DataStructureColumn column in entity.Columns)
			{
				if(ShouldUpdateColumn(column, entity)
				   && column.Field.DataType != OrigamDataType.Memo
				   && column.Field.DataType != OrigamDataType.Blob
				   && column.Field.DataType != OrigamDataType.Geography
				   && !column.IsWriteOnly
                   && (entity.ConcurrencyHandling 
                       == DataStructureConcurrencyHandling.Standard
                       || column.Field.IsPrimaryKey)
                   )
				{
                    if (i > 0)
                    {
                        PrettyIndent(sqlExpression);
						sqlExpression.Append("AND ");
                    }
					sqlExpression.AppendFormat(
                        "(({0} = {1}) OR ({0} IS NULL AND {2} IS NULL))", 
                        RenderExpression(column.Field, null, null, null, null), 
                        OriginalParameterName(column, false), 
                        OriginalParameterNameForNullComparison(column, false));
					i++;
				}
			}
			sqlExpression.Append(")");
		}

		private void RenderSelectWherePart(StringBuilder sqlExpression, DataStructureEntity entity, DataStructureFilterSet filterSet, Hashtable replaceParameterTexts, Hashtable parameters, Hashtable parameterReferences)
		{
			int i = 0;
			foreach(EntityFilter filter in Filters(filterSet, entity, parameters, false))
			{
				if(i > 0)
				{
					sqlExpression.Append(" AND");
				}
				else
				{
					sqlExpression.Append(" (");
				}
                PrettyIndent(sqlExpression);
                RenderFilter(sqlExpression, filter, entity, replaceParameterTexts, parameters, parameterReferences);

				i++;
			}

			if(i > 0) sqlExpression.Append(")");
		}

		private ArrayList Filters(DataStructureFilterSet filterSet, DataStructureEntity entity, Hashtable parameters, bool ignoreImplicitFilters)
		{
			ArrayList result = new ArrayList();
			
			if(filterSet != null)
			{
				foreach(DataStructureFilterSetFilter filterPart in filterSet.ChildItems)
				{
					if(entity.PrimaryKey.Equals(filterPart.Entity.PrimaryKey))
					{
						// skip filters with wrong role
						IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
						if(filterPart.Roles == "" || filterPart.Roles == null || auth.Authorize(SecurityManager.CurrentPrincipal, filterPart.Roles))
						{
							// skip dynamic filter parts
							IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

							string constant = null;

							if(parameterService != null & filterPart.IgnoreFilterConstant != null)
							{
								constant = (string)parameterService.GetParameterValue(filterPart.IgnoreFilterConstantId, OrigamDataType.String);
							}

							bool skip = false;

							if(filterPart.IgnoreFilterParameterName != null)
							{
								object paramValue = parameters[filterPart.IgnoreFilterParameterName];
								ArrayList paramArray = paramValue as ArrayList;

								if((paramValue == null || paramValue == DBNull.Value) && constant == null)
								{
									skip = true;
								}
									// only parameter name exists, no constant = we do filter when array is not empty
								else if(constant == null && paramArray != null && paramArray.Count == 0)
								{
									skip = true;
								}
								// parameter exists for an array = we check if one of the array values equals to
								// the parameter value
								else if(paramArray != null)
								{
									foreach(object arrayValue in paramArray)
									{
										if(XmlTools.ConvertToString(arrayValue) == constant)
										{
											skip = true;
											break;
										}
									}
								}
								else if((paramValue == null || paramValue == DBNull.Value) || constant == null)
								{
									skip = false;
								}
								else if(XmlTools.ConvertToString(paramValue) == constant)
								{
									skip = true;
								}
							}

							// reverse condition if PassWhenParameterMatch = true
							if(filterPart.PassWhenParameterMatch) skip = !skip;

							if(! skip || ResolveAllFilters)
							{
								result.Add(filterPart.Filter);
							}
						}
					}
				}
			}

			if(! (ignoreImplicitFilters || entity.IgnoreImplicitFilters))
			{
				foreach(EntitySecurityFilterReference rowLevel in entity.EntityDefinition.ChildItemsByType(EntitySecurityFilterReference.ItemTypeConst))
				{
					if(! result.Contains(rowLevel.Filter))
					{
						IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
						System.Security.Principal.IPrincipal principal = SecurityManager.CurrentPrincipal;

						if(auth.Authorize(principal, rowLevel.Roles))
						{
							result.Add(rowLevel.Filter);
						}
					}
				}
			}

			return result;
		}


		private void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity, Hashtable parameterReferences)
		{
			RenderFilter(sqlExpression, filter, entity, null, null, parameterReferences);
		}

		private void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			int i = 0;
			foreach(AbstractSchemaItem filterItem in filter.ChildItems)
			{
				if(i > 0)
					sqlExpression.Append(" AND ");
				else
					sqlExpression.Append(" (");
					
				sqlExpression.Append(RenderExpression(filterItem, entity, replaceParameterTexts, dynamicParameters, parameterReferences));

				i++;
			}

			if(i > 0)
				sqlExpression.Append(")");
		}

		#endregion

		#region Parameters
		/// <summary>
		/// Returns name of new value parameter in Insert/Update/Delete statement
		/// </summary>
		public string NewValueParameterName(DataStructureColumn column, bool declaration)
		{
			if(declaration)
			{
				return ParameterDeclarationChar + column.Name;
			}
			else
			{
				string result = ParameterReferenceChar + column.Name;

				if(column.DataType == OrigamDataType.Geography)
				{
					result = ConvertGeoFromTextClause(result);
				}

				return result;
			}	
		}

		public abstract string ConvertGeoFromTextClause(string argument);
		public abstract string ConvertGeoToTextClause(string argument);

		/// <summary>
		/// Returns name of original value parameter in Update or Delete statement
		/// </summary>
		public string OriginalParameterName(DataStructureColumn column, bool declaration)
		{
			if(declaration)
			{
				return ParameterDeclarationChar + "Original_" + column.Name;
			}
			else
			{
				return ParameterReferenceChar + "Original_" + column.Name;
			}	
		}

		/// <summary>
		/// Returns name of original value parameter used for testing NULL value in Update or Delete statement
		/// </summary>
		public string OriginalParameterNameForNullComparison(DataStructureColumn column, bool declaration)
		{
			if(declaration)
			{
				return ParameterDeclarationChar + "OriginalIsNull_" + column.Name;
			}
			else
			{
				return ParameterReferenceChar + "OriginalIsNull_" + column.Name;
			}	
		}

		#endregion

		#region Expression rendering
		private string RenderExpression(ISchemaItem item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			return RenderExpression(item, entity, replaceParameterTexts, dynamicParameters, parameterReferences, false);
		}

		private string RenderExpression(ISchemaItem item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences, bool isInRecursion)
		{
			if(item is TableMappingItem)
				return RenderExpression(item as TableMappingItem);
			if(item is EntityRelationItem)
				return RenderExpression(item as EntityRelationItem);
			else if(item is FieldMappingItem)
				return RenderExpression(item as FieldMappingItem, entity);
			else if(item is LookupField)
				return RenderLookupColumnExpression(entity.RootItem as DataStructure, entity, (item as LookupField).Field, (item as LookupField).Lookup, replaceParameterTexts, dynamicParameters, parameterReferences);
				//return RenderExpression((item as LookupField).Field, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else if(item is EntityColumnReference)
				return RenderExpression(item as EntityColumnReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else if(item is FunctionCall)
				return RenderExpression(item as FunctionCall, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else if(item is ParameterReference)
				return RenderExpression(item as ParameterReference, entity, replaceParameterTexts, null, parameterReferences);
			else if(item is DataConstantReference)
				return RenderExpression(item as DataConstantReference);
			else if(item is EntityFilterReference)
				return RenderExpression(item as EntityFilterReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else if(item is EntityFilterLookupReference)
				return RenderExpression(item as EntityFilterLookupReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else if(item is DetachedField)
				return "";
			else if(item is AggregatedColumn)
				return RenderExpression(item as AggregatedColumn, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
			else
				throw new NotImplementedException(ResourceUtils.GetString("TypeNotSupportedByMS", item.GetType().ToString()));
		}

		private string AggregationHelper(AggregatedColumn topLevelItem, DataStructureEntity topLevelEntity, AggregatedColumn item, Hashtable replaceParameterTexts, int level, StringBuilder joins, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			AggregatedColumn agg2 = item.Field as AggregatedColumn;
			
			DataStructureEntity aggregationVirtualEntity = new DataStructureEntity();
            aggregationVirtualEntity.PersistenceProvider = topLevelEntity.PersistenceProvider;
            aggregationVirtualEntity.ParentItem = topLevelEntity.RootItem;
            aggregationVirtualEntity.Name = "aggregation" + level.ToString();

			if(agg2 != null)
			{
				if(agg2.AggregationType != item.AggregationType)
				{
					throw new ArgumentOutOfRangeException("AggregationType", agg2.AggregationType, "Nested aggregations must be of the same type. Path: " + agg2.Path);
				}

				// nested aggregated expression
				DataStructureEntity aggregationVirtualEntity2 = new DataStructureEntity();
                aggregationVirtualEntity2.PersistenceProvider = topLevelEntity.PersistenceProvider;
				aggregationVirtualEntity2.ParentItem = topLevelEntity.RootItem;
				aggregationVirtualEntity2.Name = "aggregation" + (level + 1).ToString();

				joins.AppendFormat(" INNER JOIN {0} AS {1} ON ", 
					RenderExpression(agg2.Relation.AssociatedEntity as ISchemaItem, null, replaceParameterTexts, dynamicParameters, parameterReferences),
					NameLeftBracket + aggregationVirtualEntity2.Name + NameRightBracket
					);

				int i = 0;
				foreach(AbstractSchemaItem relationItem in agg2.Relation.ChildItems)
				{
					EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
					EntityRelationFilter filter = relationItem as EntityRelationFilter;

					if(i > 0) joins.Append(" AND ");

					if(key != null)
					{
						RenderSelectRelationKey(joins,
							key, aggregationVirtualEntity, aggregationVirtualEntity2,
							replaceParameterTexts, dynamicParameters, parameterReferences);
//						joins.AppendFormat("{0} = {1}",
//							RenderExpression(key.BaseEntityField, aggregationVirtualEntity, replaceParameterTexts, dynamicParameters, parameterReferences),
//							RenderExpression(key.RelatedEntityField, aggregationVirtualEntity2, replaceParameterTexts, dynamicParameters, parameterReferences)
//							);
					}

					if(filter != null)
					{
						RenderFilter(joins, filter.Filter, aggregationVirtualEntity2, parameterReferences);
					}

					i++;
				}

				// recursion - get nested expressions
				return AggregationHelper(topLevelItem, topLevelEntity, agg2, replaceParameterTexts, level + 1, joins, dynamicParameters, parameterReferences);
			}
			else
			{
				// final - non-aggregated expression
				StringBuilder result = new StringBuilder();
				DataStructureEntity topLevelAggregationVirtualEntity = new DataStructureEntity();
                topLevelAggregationVirtualEntity.PersistenceProvider = topLevelEntity.PersistenceProvider;
                topLevelAggregationVirtualEntity.Name = "aggregation1";
				topLevelAggregationVirtualEntity.ParentItem = topLevelEntity.RootItem;
				string expression = RenderExpression(item.Field as ISchemaItem, aggregationVirtualEntity, replaceParameterTexts, dynamicParameters, parameterReferences);
				expression = FixAggregationDataType(item.DataType, expression);
				string aggregationPart = string.Format("{0}({1})", GetAggregationString(item.AggregationType), expression);
				aggregationPart = FixSumAggregation(item.AggregationType, aggregationPart);
				result.AppendFormat("(SELECT {0} FROM {1} AS " + NameLeftBracket + "aggregation1" + NameRightBracket + " {2} WHERE ",
					aggregationPart,
					RenderExpression(topLevelItem.Relation.AssociatedEntity as ISchemaItem, null, replaceParameterTexts, dynamicParameters, parameterReferences),
					joins.ToString()
					);

				int i = 0;
				foreach(AbstractSchemaItem relationItem in topLevelItem.Relation.ChildItems)
				{
					EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
					EntityRelationFilter filter = relationItem as EntityRelationFilter;

					if(i > 0) result.Append(" AND ");

					if(key != null)
					{
						RenderSelectRelationKey(result,
							key, topLevelEntity, topLevelAggregationVirtualEntity,
							replaceParameterTexts, dynamicParameters, parameterReferences);

//						result.AppendFormat("{0} = {1}",
//							RenderExpression(key.BaseEntityField, topLevelEntity, replaceParameterTexts, dynamicParameters, parameterReferences),
//							RenderExpression(key.RelatedEntityField, topLevelAggregationVirtualEntity, replaceParameterTexts, dynamicParameters, parameterReferences)
//							);
					}

					if(filter != null)
					{
						RenderFilter(result, filter.Filter, topLevelAggregationVirtualEntity, parameterReferences);
					}
					
					i++;
				}

				result.Append(")");

				return result.ToString();
			}
		}

		private string RenderExpression(EntityFilterLookupReference lookupReference, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			DataServiceDataLookup lookup = lookupReference.Lookup as DataServiceDataLookup;

			if(lookup == null)
			{
				throw new ArgumentOutOfRangeException("lookup", lookupReference.Lookup, ResourceUtils.GetString("LookupTypeUnsupportedException"));
			}

			if(lookup.ListMethod != null && ! (lookup.ListMethod is DataStructureFilterSet))
			{
				throw new ArgumentOutOfRangeException("ListMethod", lookup.ListMethod, ResourceUtils.GetString("LookupListMethodTypeUnsupportedException"));
			}

			DataStructureEntity lookupEntity = (lookup.ListDataStructure.Entities[0] as DataStructureEntity);
			lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
			lookupEntity.Name = "lookup" + lookupEntity.Name;

			Hashtable replaceTexts = new Hashtable();

			foreach(AbstractSchemaItem paramMapping in lookupReference.ChildItems)
			{
				replaceTexts.Add(paramMapping.Name,
					RenderExpression(paramMapping, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
					);
			}

			string resultExpression = "(" 
				+ SelectSql(lookup.ListDataStructure, 
				lookupEntity, 
				lookup.ListMethod as DataStructureFilterSet, 
				null,
				lookup.ListDisplayMember,
				replaceTexts,
				dynamicParameters, 
				parameterReferences,
				false, false, true, true)
				+ ")";

			return resultExpression;
		}

		private string RenderExpression(AggregatedColumn item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			StringBuilder joins = new StringBuilder();

			return AggregationHelper(item, entity, item, replaceParameterTexts, 1, joins, dynamicParameters, parameterReferences);
		}

		private string RenderExpression(TableMappingItem item)
		{
			return NameLeftBracket + item.MappedObjectName + NameRightBracket;
		}

		private string RenderExpression(EntityRelationItem item)
		{
			return RenderExpression(item.RelatedEntity as ISchemaItem, null, null, null, null);
		}

		private string RenderExpression(FieldMappingItem item, DataStructureEntity dsEntity)
		{
			bool localize = dsEntity != null && dsEntity.RootItem is DataStructure 
                && (dsEntity.RootItem as DataStructure).IsLocalized;

			TableMappingItem tmi = null;
			FieldMappingItem localizedItem = null;
			if (localize)
			{
				if (dsEntity != null) {
					tmi = dsEntity.Entity as TableMappingItem;
					if (tmi == null)
					{
						// it could be a relation
						EntityRelationItem eri = dsEntity.Entity as EntityRelationItem;
						if (eri != null)
						{
							tmi = eri.RelatedEntity as TableMappingItem;
						}
					}
				}
				localizedItem = ((localize) ? (item.GetLocalizationField(tmi)) : null);
			}

			

			string nonLocalizedResult = NameLeftBracket + item.MappedColumnName + NameRightBracket;

			if (dsEntity != null)
				nonLocalizedResult = NameLeftBracket + dsEntity.Name + NameRightBracket + "." + nonLocalizedResult;
			

			if (localize && localizedItem != null)
			{
				string result = NameLeftBracket + item.GetLocalizationField(tmi).MappedColumnName + NameRightBracket;					
				result = NameLeftBracket + FieldMappingItem.GetLocalizationTable(tmi).Name
					+ NameRightBracket + "." + result;				
				result = String.Format("ISNULL({0},{1})", result, nonLocalizedResult);
				return result;
			}
			else
			{
				return nonLocalizedResult;
			}
		}

		private string RenderExpression(ParameterReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, string parameterName, Hashtable parameterReferences)
		{
			if(parameterName == null)
			{
				parameterName = GetParameterName(entity, item);
			}

			if(replaceParameterTexts != null && replaceParameterTexts.ContainsKey(parameterName))
			{
				return (string)replaceParameterTexts[parameterName];
			}
			else
			{
				string name = ParameterReferenceChar + parameterName;

				if(parameterReferences != null)
				{
					if(! parameterReferences.Contains(name))
					{
						parameterReferences.Add(name, item);
					}
				}

				return name;
			}
		}

		private string GetParameterName(DataStructureEntity entity, ParameterReference item)
		{
			return entity.Name + "_" + item.Parameter.Name;
		}

		private string RenderExpression(EntityColumnReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			if(item.Column == null)
			{
				throw new Exception("Column not specified for " + item.Path);
			}

			return RenderExpression(item.Column, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
		}

		private string RenderExpression(EntityFilterReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
			if(item.Roles == "" || item.Roles == null || auth.Authorize(SecurityManager.CurrentPrincipal, item.Roles))
			{
				StringBuilder builder = new StringBuilder();

				RenderFilter(builder, item.Filter, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

				return builder.ToString();
			}
			else
			{
				return "";
			}
		}

		private string RenderConstant(DataConstant constant)
		{
			if(constant.Name == "null") return "NULL";

			IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

			object value;
			if(UserDefinedParameters && parameterService != null)
			{
				value = parameterService.GetParameterValue(constant.Id);
			}
			else
			{
				value = constant.Value;
			}

			switch(constant.DataType)
			{
				case OrigamDataType.Integer:
				case OrigamDataType.Float:
				case OrigamDataType.Currency:
					return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);

				case OrigamDataType.Boolean:
					if((bool)constant.Value)
					{
						return this.True;
					}
					else
					{
						return this.False;
					}

				case OrigamDataType.UniqueIdentifier:
					return "'" + value.ToString() + "'";

				case OrigamDataType.Xml:
				case OrigamDataType.Memo:
				case OrigamDataType.String:
					return this.RenderString(value.ToString());
				
				case OrigamDataType.Date:
					if(value == null) return "null";
					
					return ((DateTime)value).ToString(@"{ \t\s \'yyyy-MM-dd HH:mm:ss\' }");

				default:
					throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByMS", constant.DataType.ToString()));
			}
		}

		private string RenderExpression(DataConstantReference item)
		{
			return RenderConstant(item.DataConstant);
		}

		private string RenderString(string text)
		{
			return "'" + text.Replace("'", "''") + "'";
		}

		private string RenderSortDirection(DataStructureColumnSortDirection direction)
		{
			switch(direction)
			{
				case DataStructureColumnSortDirection.Ascending:
					return "ASC";

				case DataStructureColumnSortDirection.Descending:
					return "DESC";

				default:
					throw new ArgumentOutOfRangeException("direction", direction, ResourceUtils.GetString("UnknownSortDirection"));
			}
		}

		private string RenderExpression(FunctionCall item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			string result = "";

			if(item.Function.FunctionType == OrigamFunctionType.Database)
			{
				switch(item.Function.Name)
				{
					case "DaysToAnniversary":
						if(item.ChildItems[0].ChildItems.Count == 0)
						{
							throw new Exception(ResourceUtils.GetString("ErrorExpressionNotSet", item.Path));
						}

						string date = RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences);

						result = "DATEDIFF(dd, DATEADD(yy, -(DATEPART(yy,GETDATE())-1900),GETDATE()),"
							+ "DATEADD(yy, -(DATEPART(yy,"
							+ date
							+ ")-1900),"
							+ date
							+ "))";
						break;

					case "Exists":
						if(item.ChildItems[0].ChildItems.Count == 0)
						{
							throw new Exception(ResourceUtils.GetString("ErrorLookupNotSet", item.Path));
						}

						result = "EXISTS (" + RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;

					case "Month":
					case "Year":
					case "Day":
						if(item.ChildItems[0].ChildItems.Count == 0)
						{
							throw new Exception(ResourceUtils.GetString("ErrorExpressionNotSet", item.Path));
						}

						result = item.Function.Name + "(" + RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;

					case "Hour":
					case "Minute":
					case "Second":
						if(item.ChildItems[0].ChildItems.Count == 0)
						{
							throw new Exception(ResourceUtils.GetString("ErrorExpressionNotSet", item.Path));
						}

						result = "DATEPART(" + item.Function.Name + ", " + RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;

					case "AddDays":
						{
							ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
							ISchemaItem daysArg = item.GetChildByName("Days").ChildItems[0];

							result = "DATEADD(d, " + RenderExpression(daysArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", " + RenderExpression(dateArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";
						}
						break;
					case "AddMinutes":
						{
							ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
							ISchemaItem countArg = item.GetChildByName("Minutes").ChildItems[0];
							result = string.Format("DATEADD(mi,{0},{1})"
								,RenderExpression(countArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
								,RenderExpression(dateArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences));
						}
						break;
					case "AddSeconds":
						{
							ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
							ISchemaItem countArg = item.GetChildByName("Seconds").ChildItems[0];
							result = string.Format("DATEADD(s,{0},{1})"
								, RenderExpression(countArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
								, RenderExpression(dateArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences));
						}
						break;
					case "FullTextContains":
					case "FullText":
						ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
						ISchemaItem languageArg = item.GetChildByName("Language");
						ISchemaItem fieldsArg = item.GetChildByName("Fields");

						if(item.Function.Name == "FullTextContains") result = "CONTAINS(";
						if(item.Function.Name == "FullText") result = "FREETEXT(";

						if(fieldsArg.ChildItems.Count == 0)
						{
							result += NameLeftBracket + entity.Name + NameRightBracket + ".*";
						}
						else
						{
							if(fieldsArg.ChildItems.Count > 1) result += "(";

							int fieldNum = 0;
							foreach(ISchemaItem field in fieldsArg.ChildItems)
							{
								if(fieldNum > 0) result += ", ";
								result += RenderExpression(field, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
								fieldNum++;
							}

							if(fieldsArg.ChildItems.Count > 1) result += ")";
						}

						result += ",";
						result += RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

						if(languageArg.ChildItems.Count > 0)
						{
							result += ",";
							result += RenderExpression(languageArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences);
						}
						result += ")";
						//RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " BETWEEN " + RenderExpression(leftArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " AND " + RenderExpression(rightArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
						break;

					case "Soundex":
						result = "SOUNDEX("
							+ RenderExpression(item.GetChildByName("Text").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;

					case "Distance":
						ISchemaItem param1 = item.GetChildByName("Param1").ChildItems[0];
						ISchemaItem param2 = item.GetChildByName("Param2").ChildItems[0];

						result = RenderExpression(param1, entity, replaceParameterTexts, dynamicParameters, parameterReferences) 
							+ ".STDistance(" 
							+ RenderExpression(param2, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";

						break;

					case "Latitude":
						result = RenderExpression(item.GetChildByName("Point").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ".Lat";
						break;

					case "Longitude":
						result = RenderExpression(item.GetChildByName("Point").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ".Long";
						break;

					case "ToDate":
						result = "CAST(" + RenderExpression(item.GetChildByName("argument").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ " AS DATE)";
						break;

					case "Round":
						result = "ROUND(" + RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ", " + RenderExpression(item.GetChildByName("Precision").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;
					case "Abs":
						result = string.Format("ABS({0})",
							RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							);
						break;
					case "DateDiffMinutes":
						result = string.Format("DATEDIFF(MINUTE, {0}, {1})",
							RenderExpression(item.GetChildByName("DateFrom").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							, RenderExpression(item.GetChildByName("DateTo").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							);
						break;



					default:
						result = "dbo." + item.Function.Name + "(";

						ArrayList sortedParams = new ArrayList(item.ChildItems);
						sortedParams.Sort();

						int i = 0;
						foreach(FunctionCallParameter param in sortedParams)
						{
							if(i > 0) result += ", ";

							if(param.ChildItems.Count != 1) throw new ArgumentOutOfRangeException("Count", param.ChildItems.Count, "Argument number must be 1");

							result += RenderExpression(param.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences);

							i++;
						}

						result += ")";
						break;
				}
			}
			else
			{
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
					
						if(leftValue == null) throw new ArgumentOutOfRangeException("Left", null, "Left parameter cannot be empty");

						// handle IS NULL
						if(rightValue == null && item.Function.Name == "Equal")
						{
							result =  RenderExpression(leftValue, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " IS NULL";
						}
						else if(rightValue == null && item.Function.Name == "NotEqual")
						{
							result =  RenderExpression(leftValue, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " IS NOT NULL";
						}
						else
						{
							result = "(" + RenderExpression(leftValue, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
								+ " " + GetOperator(item.Function.Name) + " "
								+ RenderExpression(rightValue, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";
						}
						break;

					case "Not":
						ISchemaItem argument = item.GetChildByName("Argument");
					
						if(! argument.HasChildItems) throw new ArgumentOutOfRangeException("Argument", null, "Argument cannot be empty");
						if(argument.ChildItems.Count > 1) throw new ArgumentOutOfRangeException("Argument", null, "Only 1 argument can be present to NOT operator.");

						ISchemaItem argumentValue = argument.ChildItems[0];

						result = "NOT(" + RenderExpression(argumentValue, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";
						break;

					case "Concat":
						ISchemaItem concatArg = item.GetChildByName("Strings");
						ArrayList concatStrings = new ArrayList(concatArg.ChildItems);

						if(concatStrings.Count < 2) throw new ArgumentOutOfRangeException("Strings", null, "There have to be at least 2 strings to concatenate.");

						concatStrings.Sort();

						result = RenderConcat(concatStrings, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
						break;

					case "LogicalOr":
					case "LogicalAnd":
						ISchemaItem logicalArg = item.GetChildByName("Arguments");
						SchemaItemCollection logicalArguments = logicalArg.ChildItems;

						if(logicalArguments.Count < 2) throw new ArgumentOutOfRangeException("Arguments", null, "There have to be at least 2 arguments for " + item.Function.Name);

						int i = 0;
						StringBuilder logicalBuilder = new StringBuilder();
						foreach(ISchemaItem logicalArgument in logicalArguments)
						{
							string expr = RenderExpression(logicalArgument, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

							if(expr != string.Empty)
							{
								if(i > 0) logicalBuilder.Append(" " + GetOperator(item.Function.Name) + " ");

								logicalBuilder.Append(expr);
							}

							i++;
						}

						result = "(" + logicalBuilder.ToString() + ")";

						break;

					case "Space":
						ISchemaItem spacesArg = item.GetChildByName("NumberOfSpaces");

						System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
						decimal numberOfSpaces = Convert.ToDecimal(RenderExpression(spacesArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences), ci.NumberFormat);

						for(i=0; i<numberOfSpaces; i++)
						{
							result += " ";
						}

						result = RenderString(result);
						break;

					case "Substring":
						result = "SUBSTRING("
							+ RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", "
							+ RenderExpression(item.GetChildByName("Start").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", "
							+ RenderExpression(item.GetChildByName("Length").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";
						break;

					case "Condition":
						result = "(CASE WHEN "
							+ RenderExpression(item.GetChildByName("If").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ " THEN "
							+ (item.GetChildByName("Then").ChildItems.Count == 0 ? "NULL" : RenderExpression(item.GetChildByName("Then").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences))
							+ " ELSE "
							+ (item.GetChildByName("Else").ChildItems.Count == 0 ? "NULL" : RenderExpression(item.GetChildByName("Else").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences))
							+ " END)";
						break;

					case "Length":
						result = "LEN("
							+ RenderExpression(item.GetChildByName("Text").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ")";
						break;

					case "ConvertDateToString":
						result = "CONVERT("
							+ "NVARCHAR(" + item.DataLength.ToString() + "), "
							+ RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
							+ ", 104)";
						break;

					case "In":
						ISchemaItem leftArg = item.GetChildByName("FilterExpression");
						ISchemaItem listArg = item.GetChildByName("List");
						SchemaItemCollection listExpressions = listArg.ChildItems;

						if(listExpressions.Count < 1) throw new ArgumentOutOfRangeException("List", null, ResourceUtils.GetString("ErrorNoParamIN"));

						if(listExpressions.Count == 1 && listExpressions[0] is ParameterReference && (listExpressions[0] as ParameterReference).Parameter.DataType == OrigamDataType.Array)
						{
							result = RenderExpression(leftArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) 
								+ " IN (SELECT ListValue FROM " + RenderExpression(listExpressions[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " origamListValue)";
						}
						else
						{
							// list of parameters
							i = 0;
							StringBuilder listBuilder = new StringBuilder();
							foreach(ISchemaItem listExpression in listExpressions)
							{
								if(i > 0) listBuilder.Append(", ");

								ParameterReference paramReference = listExpression as ParameterReference;

								//							if(paramReference != null && dynamicParameters.Contains(paramReference.Parameter.Name))
								//							{
								//								int dynamicCount = (int)dynamicParameters[listExpression.Name];
								//
								//								for(int dyn = 0; dyn < dynamicCount; dyn++)
								//								{
								//									string paramName = GetParameterName(entity, paramReference);
								//
								//									listBuilder.Append(RenderExpression(paramReference, entity, replaceParameterTexts, paramName + dyn.ToString()));
								//								}
								//							}
								//							else
								//							{
								listBuilder.Append(RenderExpression(listExpression, entity, replaceParameterTexts, dynamicParameters, parameterReferences));
								//							}

								i++;
							}

							result = RenderExpression(leftArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " IN (" + listBuilder.ToString() + ")";
						}
						break;

					case "IsNull":
						ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
						ISchemaItem replacementArg = item.GetChildByName("ReplacementValue").ChildItems[0];

						result = "ISNULL(" + RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", " + RenderExpression(replacementArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";

						break;

					case "Between":
						expressionArg = item.GetChildByName("Expression").ChildItems[0];
						leftArg = item.GetChildByName("Left").ChildItems[0];
						ISchemaItem rightArg = item.GetChildByName("Right").ChildItems[0];
						
						result = RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " BETWEEN " + RenderExpression(leftArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " AND " + RenderExpression(rightArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

						break;

					default:
						throw new ArgumentOutOfRangeException("Function.Name", item.Function.Name, ResourceUtils.GetString("UnknownFunction"));
				}
			}

			return result;
		}

		private string RenderConcat(ArrayList concatSchemaItems, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			List<KeyValuePair<ISchemaItem, DataStructureEntity>> concatSchemaItemList =
				new List<KeyValuePair<ISchemaItem, DataStructureEntity>>();

			foreach (object o in concatSchemaItems)
			{
				concatSchemaItemList.Add(new KeyValuePair<ISchemaItem, DataStructureEntity>(o as ISchemaItem, entity));
			}
            return RenderConcat(concatSchemaItemList, null, replaceParameterTexts, dynamicParameters, parameterReferences);
		}
	
		private string RenderConcat(List<KeyValuePair<ISchemaItem, DataStructureEntity> > concatSchemaItemList, string separator, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			int i = 0;
			StringBuilder concatBuilder = new StringBuilder();
			foreach(KeyValuePair<ISchemaItem, DataStructureEntity> concatItem in concatSchemaItemList)
			{
				if(i > 0) 
				{
					concatBuilder.Append(" " + StringConcatenationChar + " ");
					if(separator != null)
					{
						concatBuilder.Append(separator);
						concatBuilder.Append(" " + StringConcatenationChar + " ");
					}
				}

				concatBuilder.Append("CAST(");
				concatBuilder.Append(RenderExpression(concatItem.Key, concatItem.Value, replaceParameterTexts, dynamicParameters, parameterReferences));
				concatBuilder.Append(" AS NVARCHAR(MAX))");

				i++;
			}

			return concatBuilder.ToString();
		}
		#endregion

		#region Operators
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
		#endregion

		#region Conversions
		public string DdlDataType(OrigamDataType columnType,
            DatabaseDataType dbDataType)
		{
            if (dbDataType != null)
            {
                return dbDataType.MappedDatabaseTypeName;
            }
            else
            {
                return DefaultDdlDataType(columnType);
            }
		}

        public abstract string DefaultDdlDataType(OrigamDataType columnType);

        public abstract OrigamDataType ToOrigamDataType(string ddlType);

		public string DdlDataType(OrigamDataType columnType, int dataLenght,
            DatabaseDataType dbDataType)
		{
			switch(columnType)
			{
				case OrigamDataType.String:
					return DdlDataType(columnType, dbDataType) 
                        + "(" + dataLenght.ToString() + ")";

				case OrigamDataType.Xml:
					return DdlDataType(columnType, dbDataType) + "(2000)";

				case OrigamDataType.Float:
					return DdlDataType(columnType, dbDataType) + "(28,10)";

				default:
					return DdlDataType(columnType, dbDataType);
			}
		}
		#endregion

		#region ICloneable Members

		public object Clone()
		{
			MsSqlCommandGenerator gen = new MsSqlCommandGenerator();
			//gen.AdapterConnection = this.AdapterConnection;

			return gen;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion
	}
}
