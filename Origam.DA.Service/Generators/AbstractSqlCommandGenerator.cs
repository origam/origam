#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Linq;
using Origam.DA.Service.Generators;

namespace Origam.DA.Service
{
    public abstract class AbstractSqlCommandGenerator : DbDataAdapterFactory, IDisposable
    {
        internal readonly ParameterReference PageNumberParameterReference = new ParameterReference();
        internal readonly ParameterReference PageSizeParameterReference = new ParameterReference();
        internal readonly FilterRenderer filterRenderer = new FilterRenderer();
        internal string _pageNumberParameterName;
        internal string _pageSizeParameterName;
        internal int _indentLevel = 0;

        internal struct SortOrder
        {
            public string Expression;
            public DataStructureColumnSortDirection SortDirection;
        }

        internal enum JoinBeginType
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
            IDbDataParameter dbParam = GetParameter();
            if (type == typeof(DateTime))
            {
                dbParam.DbType = DbType.DateTime;
            }
            else if (type == typeof(Guid))
            {
                dbParam.DbType = DbType.Guid;
            }
            else if (type == typeof(int))
            {
                dbParam.DbType = DbType.Int32;
            }
            else if (type == typeof(double))
            {
                dbParam.DbType = DbType.Double;
            }
            else if (type == typeof(bool))
            {
                dbParam.DbType = DbType.Boolean;
            }
            else if (type == typeof(Decimal) || type == typeof(float))
            {
                dbParam.DbType = DbType.Decimal;
            }

            dbParam.ParameterName = ParameterDeclarationChar + name;

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
        public abstract string NameLeftBracket { get; }
        public abstract string NameRightBracket { get; }
        public abstract string ParameterDeclarationChar { get; }
        public abstract string ParameterReferenceChar { get; }
        public abstract string StringConcatenationChar { get; }
        public abstract bool PagingCanIncludeOrderBy { get; }

        public abstract string GetIndexName(IDataEntity entity, DataEntityIndex index);
        public abstract string SelectClause(string finalQuery, int top);

        public abstract string True { get; }
        public abstract string False { get; }

        public const string RowNumColumnName = "RowNum";

        public abstract IDbDataParameter BuildParameter(string paramName,
            string sourceColumn, OrigamDataType dataType, DatabaseDataType dbDataType,
            int dataLength, bool allowNulls);

        public bool UserDefinedParameters { get; set; } = false;

        public bool ResolveAllFilters { get; set; }

        public bool PrettyFormat { get; set; }

        public IDbCommand ScalarValueCommand(DataStructure ds, DataStructureFilterSet filter,
            DataStructureSortSet sortSet, string columnName, Hashtable parameters)
        {
            Hashtable selectParameterReferences = new Hashtable();
            IDbCommand cmd = GetCommand(
                SelectSql(ds, ds.Entities[0] as DataStructureEntity, filter, sortSet,
                columnName, null, parameters, selectParameterReferences, true, false, false, true));
            cmd.CommandType = CommandType.Text;
            BuildSelectParameters(cmd, selectParameterReferences);
            BuildFilterParameters(cmd, ds, filter, null, parameters);
            return cmd;
        }

        public DbDataAdapter CreateDataAdapter(SelectParameters adParameters, bool forceDatabaseCalculation)
        {
            if (!(adParameters.Entity.EntityDefinition is TableMappingItem))
            {
                throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
            }

            DbDataAdapter adapter = GetAdapter();
            BuildCommands(adapter, adParameters,
                forceDatabaseCalculation);
            adapter.TableMappings.Clear();
            adapter.TableMappings.Add(CreateMapping(adParameters.Entity));

            return adapter;
        }

        public DbDataAdapter CreateDataAdapter(string procedureName, ArrayList entitiesOrdered,
            IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand cmd = GetCommand(procedureName);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = connection;
            cmd.Transaction = transaction;
            DeriveStoredProcedureParameters(cmd);

            DbDataAdapter adapter = GetAdapter(cmd);

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
            if (!(entity.EntityDefinition is TableMappingItem))
            {
                throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
            }

            DbDataAdapter adapter = GetAdapter();
            BuildSelectRowCommand(adapter, entity, columnName, forceDatabaseCalculation);
            adapter.TableMappings.Clear();
            adapter.TableMappings.Add(CreateMapping(entity));

            return adapter;
        }

        public DbDataAdapter CreateUpdateFieldDataAdapter(TableMappingItem table, FieldMappingItem field)
        {
            DbDataAdapter adapter = GetAdapter();
            BuildSelectUpdateFieldCommand(adapter, table, field);
            adapter.TableMappings.Clear();
            adapter.TableMappings.Add(CreateUpdateFieldMapping(table, field));

            return adapter;
        }

        public abstract IDbCommand UpdateFieldCommand(TableMappingItem entity, FieldMappingItem field);
        

        private DataTableMapping CreateMapping(DataStructureEntity entity)
        {
            return CreateMapping(entity, 0);
        }

        private DataTableMapping CreateMapping(DataStructureEntity entity, int tableOrder)
        {
            DataTableMapping dtm = new DataTableMapping();

            if (tableOrder == 0)
            {
                dtm.SourceTable = "Table";
            }
            else
            {
                dtm.SourceTable = "Table" + tableOrder;
            }

            dtm.DataSetTable = entity.Name;

            foreach (DataStructureColumn column in entity.Columns)
            {
                dtm.ColumnMappings.Add(column.Name, column.Name);
            }

            return dtm;
        }

        private DataTableMapping CreateUpdateFieldMapping(TableMappingItem table, FieldMappingItem field)
        {
            DataTableMapping dtm = new DataTableMapping();

            dtm.SourceTable = "Table";
            dtm.DataSetTable = table.Name;

            foreach (IDataEntityColumn column in table.EntityPrimaryKey)
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

            adapter.SelectCommand = GetCommand(
                SelectRowSql(entity, selectParameterReferences, columnName,
                forceDatabaseCalculation));

            BuildPrimaryKeySelectParameters(adapter.SelectCommand, entity);
            BuildSelectParameters(adapter.SelectCommand, selectParameterReferences);
        }

        public void BuildSelectUpdateFieldCommand(DbDataAdapter adapter, TableMappingItem table, FieldMappingItem field)
        {
            ((IDbDataAdapter)adapter).SelectCommand = GetCommand(SelectUpdateFieldSql(table, field));

            BuildUpdateFieldParameters(((IDbDataAdapter)adapter).SelectCommand, field);
        }

        public IDbCommand SelectReferenceCountCommand(TableMappingItem table, FieldMappingItem field)
        {
            IDbCommand cmd = GetCommand(SelectReferenceCountSql(table, field));
            cmd.CommandType = CommandType.Text;
            BuildUpdateFieldParameters(cmd, field);
            return cmd;
        }

        public void BuildCommands(IDbDataAdapter adapter, SelectParameters selectParameters,
            bool forceDatabaseCalculation)
        {
            CustomCommandParser commandParser = 
                new CustomCommandParser(NameLeftBracket, NameRightBracket);
            string customWhereClause = commandParser.ToSqlWhere(selectParameters.CustomFilters);
            string customOrderByClause = commandParser.ToSqlOrderBy(selectParameters.CustomOrdering);

            Hashtable selectParameterReferences = new Hashtable();
            DataStructure dataStructure = selectParameters.DataStructure;
            DataStructureEntity entity = selectParameters.Entity;
            adapter.SelectCommand =
                GetCommand(SelectSql(
                    dataStructure,
                    entity,
                    selectParameters.Filter, 
                    selectParameters.SortSet, 
                    selectParameters.ColumnName,
                    null, selectParameters.Parameters,
                    selectParameterReferences, 
                    false, 
                    selectParameters.Paging,
                    false,
                    forceDatabaseCalculation,
                    customWhereClause,
                    customOrderByClause,
                    selectParameters.RowLimit));

            BuildSelectParameters(adapter.SelectCommand, selectParameterReferences);
            BuildFilterParameters(adapter.SelectCommand, dataStructure,
                selectParameters.Filter, null, selectParameters.Parameters);

            if (!dataStructure.Name.StartsWith("Lookup") || entity.AllFields)
            {
                adapter.UpdateCommand = GetCommand(UpdateSql(dataStructure, entity));
                adapter.InsertCommand = GetCommand(InsertSql(dataStructure, entity));
                adapter.DeleteCommand = GetCommand(DeleteSql(dataStructure, entity));

                BuildUpdateParameters(adapter.UpdateCommand, entity);
                BuildInsertParameters(adapter.InsertCommand, entity);
                BuildDeleteParameters(adapter.DeleteCommand, entity);
            }
        }

        public void BuildFilterParameters(IDbCommand command, DataStructure ds, DataStructureFilterSet filterSet, Hashtable replaceParameterTexts, Hashtable parameters)
        {
            foreach (DataStructureEntity entity in ds.Entities)
            {
                foreach (EntityFilter filter in Filters(filterSet, entity, parameters, false))
                {
                    Hashtable paramReferences = filter.ParameterReferences;
                    foreach (DictionaryEntry entry in paramReferences)
                    {
                        ParameterReference parameterRef = entry.Value as ParameterReference;
                        string paramName = RenderExpression(parameterRef, entity, replaceParameterTexts, null, null);

                        if (!command.Parameters.Contains(paramName))
                        {
                            IDbDataParameter sqlParam = BuildParameter(paramName, parameterRef);
                            command.Parameters.Add(sqlParam);
                        }
                    }
                }

                // recursion for lookup values - they can also have parameters, e.g. row level security ones
                foreach (DataStructureColumn column in entity.Columns)
                {
                    if (column.UseLookupValue)
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
            foreach (DictionaryEntry entry in parameterReferences)
            {
                ParameterReference parameterRef = entry.Value as ParameterReference;
                string paramName = (string)entry.Key; //RenderExpression(parameterRef, entity, replaceParameterTexts, (string)entry.Key);
                if (!command.Parameters.Contains(paramName))
                {
                    IDataParameter sqlParam = BuildParameter(paramName, parameterRef);
                    command.Parameters.Add(sqlParam);
                }
            }
        }

        public void BuildUpdateParameters(IDbCommand command, DataStructureEntity entity)
        {
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (ShouldUpdateColumn(column, entity))
                {
                    command.Parameters.Add(CreateNewValueParameter(column));
                    command.Parameters.Add(CreateOriginalValueParameter(column));
                    command.Parameters.Add(CreateOriginalValueParameterForNullComparison(column));
                }
            }
        }

        public void BuildInsertParameters(IDbCommand command, DataStructureEntity entity)
        {
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
                {
                    command.Parameters.Add(CreateNewValueParameter(column));
                }
            }
        }

        public void BuildDeleteParameters(IDbCommand command, DataStructureEntity entity)
        {
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
                {
                    command.Parameters.Add(CreateOriginalValueParameter(column));
                    command.Parameters.Add(CreateOriginalValueParameterForNullComparison(column));
                }
            }
        }

        public void BuildPrimaryKeySelectParameters(IDbCommand command, DataStructureEntity entity)
        {
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field is FieldMappingItem && column.Field.IsPrimaryKey && column.UseLookupValue == false && column.UseCopiedValue == false)
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
                OriginalParameterName(column, true),
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

            foreach (TableMappingItem table in tables)
            {
                if (table.DatabaseObjectType == DatabaseMappingObjectType.Table)
                {
                    ddl.Append(TableDefinitionDdl(table));
                    ddl.Append(Environment.NewLine);
                    ddl.Append(Environment.NewLine);
                }
            }

            return ddl.ToString();
        }

        public abstract string FunctionDefinitionDdl(Function function);
        

        public string ForeignKeyConstraintsDdl(TableMappingItem table)
        {
            string result = "";

            foreach (DataEntityConstraint constraint in table.Constraints)
            {
                if (constraint.Type == ConstraintType.ForeignKey)
                {
                    result += Environment.NewLine + AddForeignKeyConstraintDdl(table, constraint);
                }
            }
            return result;
        }

        public abstract string AddForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint);


        public abstract string ForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint);
        public abstract string AddColumnDdl(FieldMappingItem field);
        public abstract string AlterColumnDdl(FieldMappingItem field);
        internal abstract string ColumnDefinitionDdl(FieldMappingItem field);
        public abstract string IndexDefinitionDdl(IDataEntity entity, DataEntityIndex index, bool complete);
        public abstract string TableDefinitionDdl(TableMappingItem table);
        public string SelectParameterDeclarationsSql(DataStructureFilterSet filter, bool paging,
            string columnName)
        {
            StringBuilder result = new StringBuilder();
            Hashtable ht = new Hashtable();
            DataStructure ds = filter.RootItem as DataStructure;

            foreach (DataStructureEntity entity in ds.Entities)
            {
                SelectParameterDeclarationsSql(result, ht, ds, entity, filter, null, paging,
                    columnName);
            }
            SelectParameterDeclarationsSetSql(result, ht);
            return result.ToString();
        }

        public string SelectParameterDeclarationsSql(DataStructure ds, DataStructureEntity entity,
            DataStructureFilterSet filter, bool paging, string columnName)
        {
            StringBuilder result = new StringBuilder();
            Hashtable ht = new Hashtable();
            SelectParameterDeclarationsSql(result, ht, ds, entity, filter, null, paging, columnName);
            SelectParameterDeclarationsSetSql(result, ht);
            return result.ToString();
        }

        public string SelectParameterDeclarationsSql(DataStructure ds, DataStructureEntity entity,
            DataStructureSortSet sort, bool paging, string columnName)
        {
            StringBuilder result = new StringBuilder();
            Hashtable ht = new Hashtable();
            SelectParameterDeclarationsSql(result, ht, ds, entity, null, sort, paging, columnName);
            SelectParameterDeclarationsSetSql(result, ht);
            return result.ToString();
        }

        internal ArrayList Parameters(IDbCommand cmd)
        {
            int declarationLength = ParameterDeclarationChar.Length;
            ArrayList list = new ArrayList(cmd.Parameters.Count);
            foreach (IDataParameter param in cmd.Parameters)
            {
                list.Add(param.ParameterName.Substring(declarationLength));
            }

            list.Sort();
            return list;
        }

        internal IDbCommand SelectCommand(DataStructure ds, DataStructureEntity entity,
            DataStructureFilterSet filter, DataStructureSortSet sort, bool paging,
            string columnName)
        {
            var adapterParameters = new SelectParameters
            {
                DataStructure = ds,
                Entity = entity,
                Filter = filter,
                SortSet = sort,
                Parameters = new Hashtable(),
                Paging = paging,
                ColumnName = columnName,
            };
            DbDataAdapter adapter = CreateDataAdapter(adapterParameters, false);
            return ((IDbDataAdapter)adapter).SelectCommand;
        }

        public ArrayList Parameters(DataStructure ds, DataStructureEntity entity,
            DataStructureFilterSet filter, DataStructureSortSet sort, bool paging,
            string columnName)
        {
            IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
            return Parameters(cmd);
        }

        internal abstract void SelectParameterDeclarationsSql(StringBuilder result, Hashtable ht,
            DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter,
            DataStructureSortSet sort, bool paging, string columnName);
        internal abstract string SqlDataType(IDataParameter param);
        internal abstract void SelectParameterDeclarationsSetSql(StringBuilder result, Hashtable parameters);
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

        internal abstract string SelectSql(DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter,
              DataStructureSortSet sortSet, string scalarColumn, Hashtable replaceParameterTexts,
              Hashtable dynamicParameters, Hashtable selectParameterReferences, bool restrictScalarToTop1,
              bool paging, bool isInRecursion, bool forceDatabaseCalculation,
              string customWhereClause = null, string customOrderByClause = null, int? rowLimit = null);
        

        internal bool IgnoreEntityWhenNoFilters(DataStructureEntity relation, DataStructureFilterSet filter, Hashtable dynamicParameters)
        {
            // If IgnoreWhenNoFilters is on and there is no filter for this entity, we skip it.
            // This is important for dynamic queries, where filters depend on actual parameters (e.g. null value means not filtering at all).
            // When no filter, then also the whole relation (which would be inner join, thus limiting the parent rows) is ignored.

            bool ignoreImplicitFilters = relation.IgnoreCondition == DataStructureIgnoreCondition.IgnoreWhenNoExplicitFilters;

            int filterCount = Filters(filter, relation, dynamicParameters, ignoreImplicitFilters).Count;

            if (filterCount > 0) return false;

            // we test for child entities as well
            foreach (DataStructureEntity childEntity in relation.ChildItemsByTypeRecursive(DataStructureEntity.ItemTypeConst))
            {
                filterCount += Filters(filter, childEntity, dynamicParameters, ignoreImplicitFilters).Count;

                // some filters found, we break
                if (filterCount > 0) return false;
            }

            return true;
        }

        internal bool IgnoreConditionalEntity(DataStructureEntity relation, Hashtable dynamicParameters)
        {
            // skip dynamic entity relations
            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

            string constant = null;

            if (parameterService != null & relation.ConditionEntityConstant != null)
            {
                constant = (string)parameterService.GetParameterValue(relation.ConditionEntityConstantId, OrigamDataType.String);
            }

            if (relation.ConditionEntityParameterName != null)
            {
                if (dynamicParameters[relation.ConditionEntityParameterName] == null & constant == null)
                {
                    return true;
                }
                else if (dynamicParameters[relation.ConditionEntityParameterName] == null | constant == null)
                {
                    return false;
                }
                else if (dynamicParameters[relation.ConditionEntityParameterName].ToString() != constant)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract string UpsertSql(DataStructure ds, DataStructureEntity entity);

        public abstract string InsertSql(DataStructure ds, DataStructureEntity entity);
        
        internal abstract void RenderSelectUpdatedData(StringBuilder sqlExpression, DataStructureEntity entity);
        public  abstract string SelectRowSql(DataStructureEntity entity, Hashtable selectParameterReferences,
            string columnName, bool forceDatabaseCalculation);

        public abstract string SelectUpdateFieldSql(TableMappingItem table, FieldMappingItem updatedField);

        public abstract string SelectReferenceCountSql(TableMappingItem table, FieldMappingItem updatedField);
        public abstract string DeleteSql(DataStructure ds, DataStructureEntity entity);
        public abstract string UpdateSql(DataStructure ds, DataStructureEntity entity);
        internal bool ShouldUpdateColumn(DataStructureColumn column, DataStructureEntity entity)
        {
            if (!(column.Field is FieldMappingItem)) return false;
            if (column.UseLookupValue) return false;
            if (column.UseCopiedValue) return false;
            if (column.Entity != null) return false;

            return true;
        }

        #endregion

        #region Select parts
        internal bool RenderSelectColumns(DataStructure ds, StringBuilder sqlExpression,
            StringBuilder orderByBuilder, StringBuilder groupByBuilder, DataStructureEntity entity,
            string scalarColumn, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            DataStructureSortSet sortSet, Hashtable selectParameterReferences,
            bool forceDatabaseCalculation)
        {
            return RenderSelectColumns(ds, sqlExpression, orderByBuilder, groupByBuilder,
                entity, scalarColumn, replaceParameterTexts, dynamicParameters, sortSet,
                selectParameterReferences, false, true, forceDatabaseCalculation);
        }

        internal abstract bool RenderSelectColumns(DataStructure ds, StringBuilder sqlExpression,
            StringBuilder orderByBuilder, StringBuilder groupByBuilder, DataStructureEntity entity,
            string scalarColumn, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            DataStructureSortSet sortSet, Hashtable selectParameterReferences, bool isInRecursion,
            bool concatScalarColumns, bool forceDatabaseCalculation);
        

        internal IEnumerable<DataStructureColumn> GetSortedColumns(DataStructureEntity entity,
            ArrayList scalarColumnNames)
        {
            if (scalarColumnNames.Count == 0)
            {
                return entity.Columns.Cast<DataStructureColumn>();
            }
            return entity.Columns
                .Cast<DataStructureColumn>()
                .OrderBy(x => scalarColumnNames.IndexOf(x.Name));
        }

        public string RenderDataStructureColumn(DataStructure ds, 
            DataStructureEntity entity, 
            Hashtable replaceParameterTexts, 
            Hashtable dynamicParameters, DataStructureSortSet sortSet, 
            Hashtable selectParameterReferences, bool isInRecursion,
            bool forceDatabaseCalculation, ArrayList group, SortedList order, 
            ref bool groupByNeeded, ArrayList scalarColumnNames, DataStructureColumn column)
        {
            string result = null;
            bool processColumn = false;
            FunctionCall functionCall = column.Field as FunctionCall;
            AggregatedColumn aggregatedColumn = column.Field as AggregatedColumn;
            // parent column references will be calculated by the dataset
            // generator
            if (column.IsWriteOnly)
            {
                processColumn = false;
            }
            else if (column.IsFromParentEntity())
            {
                processColumn = false;
            }
            else if (scalarColumnNames.Contains(column.Name) && 
                     ShouldBeProcessed(forceDatabaseCalculation, column, functionCall))
            {
                processColumn = true;
            }
            else if (scalarColumnNames.Count == 0 &&
                     ShouldBeProcessed(forceDatabaseCalculation, column, functionCall))
            {
                processColumn = true;
            }
            else if (scalarColumnNames.Count == 0 && aggregatedColumn != null)
            {
                bool found = false;
                foreach (DataStructureEntity childEntity in entity.ChildItemsByType(DataStructureEntity.ItemTypeConst))
                {
                    // if we have an aggregation column and
                    // and the aggregation sub-entity with source field
                    // exist in our DS, then we don't render sql for the
                    // column, but just rely on dataset aggregation computation
                    if (childEntity.Entity.PrimaryKey.Equals(aggregatedColumn.Relation.PrimaryKey))
                    {
                        // search for aggregation source column in related entity 
                        if (childEntity.ExistsEntityFieldAsColumn(aggregatedColumn.Field))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found) processColumn = true;
            }

            string resultExpression = "";
            string groupExpression = "";

            if (processColumn || column.IsColumnSorted(sortSet))
            {
                if (column.UseLookupValue)
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

                    if (column.Aggregation != AggregationType.None)
                    {
                        if (column.Field is AggregatedColumn)
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

                if (processColumn)
                {
                    result = string.Format("{0} AS {1}",
                        resultExpression,
                        NameLeftBracket + column.Name + NameRightBracket
                        );
                    // anything not having aggregation will eventually go to GROUP BY
                    if (column.Aggregation == AggregationType.None)
                    {
                        group.Add(groupExpression);
                    }
                }
            }

            // does not matter if processColumn=true, because we want to sort anytime sorting is specified,
            // e.g. if this is a scalar query and sorting is by another than the scalar column
            if (column.IsColumnSorted(sortSet))
            {
                System.Diagnostics.Debug.Assert(resultExpression != String.Empty, "No expression generated for sorting.", "Column: " + column.Path);
                SortOrder sortOrder;
                string sortExpression = resultExpression;
                // if the column is a lookup column, we will sort by the looked-up
                // value, not by the source value, this will bring the same logic
                // as in the UI - when user sorts, it will always sort by a looked-up
                // values
                if (column.FinalLookup != null && !column.UseLookupValue)
                {
                    sortExpression = RenderLookupColumnExpression(ds, entity, column,
                        replaceParameterTexts, dynamicParameters, selectParameterReferences);
                }
                sortOrder.Expression = sortExpression;
                sortOrder.SortDirection = column.SortDirection(sortSet);

                if (order.Contains(column.SortOrder(sortSet)))
                {
                    throw new InvalidOperationException(ResourceUtils.GetString("ErrorSortOrder", column.SortOrder(sortSet).ToString(), column.Path));
                }

                order.Add(column.SortOrder(sortSet), sortOrder);
            }
            return result;
        }

        private static bool ShouldBeProcessed(bool forceDatabaseCalculation, DataStructureColumn column, FunctionCall functionCall)
        {
            return 
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
                   );
        }

        internal void PrettyLine(StringBuilder sqlExpression)
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

        internal void PrettyIndent(StringBuilder sqlExpression)
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

        internal abstract string FixAggregationDataType(OrigamDataType dataType, string expression);


        internal abstract string FixSumAggregation(AggregationType aggregationType, string expression);
        

        private string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity,
            DataStructureColumn column, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences)
        {
            if (column.Aggregation != AggregationType.None)
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

        internal abstract string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity, IDataEntityColumn field,
            IDataLookup lookup, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences, bool isInRecursion);
        

        internal abstract void RenderSelectFromClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts);

        internal abstract void RenderSelectExistsClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences);
        

        internal bool CanSkipSelectRelation(DataStructureEntity relation, DataStructureEntity stopAtEntity)
        {
            if (relation.RelationType != RelationType.Normal) return false;

            if (stopAtEntity.PrimaryKey.Equals(relation.PrimaryKey)) return false;

            return true;
        }

        internal abstract void RenderSelectRelation(StringBuilder sqlExpression, DataStructureEntity dsEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, bool skipStopAtEntity, bool includeFilter, int numberOfJoins, bool includeAllRelations, Hashtable dynamicParameters, Hashtable parameterReferences);
        
        internal void RenderSelectRelationKey(StringBuilder sqlExpression,
            EntityRelationColumnPairItem key,
            DataStructureEntity parentEntity, DataStructureEntity relatedEntity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable paremeterReferences)
        {
            string parentField = RenderExpression(key.BaseEntityField as AbstractSchemaItem,
                    parentEntity, replaceParameterTexts, dynamicParameters, paremeterReferences);
            string relatedField = RenderExpression(key.RelatedEntityField as AbstractSchemaItem,
                    relatedEntity, replaceParameterTexts, dynamicParameters, paremeterReferences);
            sqlExpression.Append(filterRenderer.Equal(parentField, relatedField));
        }

        internal abstract void RenderUpdateDeleteWherePart(StringBuilder sqlExpression, DataStructureEntity entity);
        

        internal abstract void RenderSelectWherePart(StringBuilder sqlExpression, DataStructureEntity entity, DataStructureFilterSet filterSet, Hashtable replaceParameterTexts, Hashtable parameters, Hashtable parameterReferences);
        

        internal ArrayList Filters(DataStructureFilterSet filterSet, DataStructureEntity entity, Hashtable parameters, bool ignoreImplicitFilters)
        {
            ArrayList result = new ArrayList();

            if (filterSet != null)
            {
                foreach (DataStructureFilterSetFilter filterPart in filterSet.ChildItems)
                {
                    if (entity.PrimaryKey.Equals(filterPart.Entity.PrimaryKey))
                    {
                        // skip filters with wrong role
                        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
                        if (filterPart.Roles == "" || filterPart.Roles == null || auth.Authorize(SecurityManager.CurrentPrincipal, filterPart.Roles))
                        {
                            // skip dynamic filter parts
                            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

                            string constant = null;

                            if (parameterService != null & filterPart.IgnoreFilterConstant != null)
                            {
                                constant = (string)parameterService.GetParameterValue(filterPart.IgnoreFilterConstantId, OrigamDataType.String);
                            }

                            bool skip = false;

                            if (filterPart.IgnoreFilterParameterName != null)
                            {
                                object paramValue = parameters[filterPart.IgnoreFilterParameterName];
                                ArrayList paramArray = paramValue as ArrayList;

                                if ((paramValue == null || paramValue == DBNull.Value) && constant == null)
                                {
                                    skip = true;
                                }
                                // only parameter name exists, no constant = we do filter when array is not empty
                                else if (constant == null && paramArray != null && paramArray.Count == 0)
                                {
                                    skip = true;
                                }
                                // parameter exists for an array = we check if one of the array values equals to
                                // the parameter value
                                else if (paramArray != null)
                                {
                                    foreach (object arrayValue in paramArray)
                                    {
                                        if (XmlTools.ConvertToString(arrayValue) == constant)
                                        {
                                            skip = true;
                                            break;
                                        }
                                    }
                                }
                                else if ((paramValue == null || paramValue == DBNull.Value) || constant == null)
                                {
                                    skip = false;
                                }
                                else if (XmlTools.ConvertToString(paramValue) == constant)
                                {
                                    skip = true;
                                }
                            }

                            // reverse condition if PassWhenParameterMatch = true
                            if (filterPart.PassWhenParameterMatch) skip = !skip;

                            if (!skip || ResolveAllFilters)
                            {
                                result.Add(filterPart.Filter);
                            }
                        }
                    }
                }
            }

            if (!(ignoreImplicitFilters || entity.IgnoreImplicitFilters))
            {
                foreach (EntitySecurityFilterReference rowLevel in entity.EntityDefinition.ChildItemsByType(EntitySecurityFilterReference.ItemTypeConst))
                {
                    if (!result.Contains(rowLevel.Filter))
                    {
                        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
                        System.Security.Principal.IPrincipal principal = SecurityManager.CurrentPrincipal;

                        if (auth.Authorize(principal, rowLevel.Roles))
                        {
                            result.Add(rowLevel.Filter);
                        }
                    }
                }
            }

            return result;
        }


        internal void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity, Hashtable parameterReferences)
        {
            RenderFilter(sqlExpression, filter, entity, null, null, parameterReferences);
        }

        internal abstract void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences);
        

        #endregion

        #region Parameters
        /// <summary>
        /// Returns name of new value parameter in Insert/Update/Delete statement
        /// </summary>
        public string NewValueParameterName(DataStructureColumn column, bool declaration)
        {
            if (declaration)
            {
                return ParameterDeclarationChar + column.Name;
            }
            else
            {
                string result = ParameterReferenceChar + column.Name;

                if (column.DataType == OrigamDataType.Geography)
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
            if (declaration)
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
            if (declaration)
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
        internal string RenderExpression(ISchemaItem item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            return RenderExpression(item, entity, replaceParameterTexts, dynamicParameters, parameterReferences, false);
        }

        private string RenderExpression(ISchemaItem item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences, bool isInRecursion)
        {
            if (item is TableMappingItem)
                return RenderExpression(item as TableMappingItem);
            if (item is EntityRelationItem)
                return RenderExpression(item as EntityRelationItem);
            else if (item is FieldMappingItem)
                return RenderExpression(item as FieldMappingItem, entity);
            else if (item is LookupField)
                return RenderLookupColumnExpression(entity.RootItem as DataStructure, entity, (item as LookupField).Field, (item as LookupField).Lookup, replaceParameterTexts, dynamicParameters, parameterReferences);
            else if (item is EntityColumnReference)
                return RenderExpression(item as EntityColumnReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
            else if (item is FunctionCall)
                return RenderExpression(item as FunctionCall, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
            else if (item is ParameterReference)
                return RenderExpression(item as ParameterReference, entity, replaceParameterTexts, null, parameterReferences);
            else if (item is DataConstantReference)
                return RenderExpression(item as DataConstantReference);
            else if (item is EntityFilterReference)
                return RenderExpression(item as EntityFilterReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
            else if (item is EntityFilterLookupReference)
                return RenderExpression(item as EntityFilterLookupReference, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
            else if (item is DetachedField)
                return "";
            else if (item is AggregatedColumn)
                return RenderExpression(item as AggregatedColumn, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
            else
                throw new NotImplementedException(ResourceUtils.GetString("TypeNotSupported", item.GetType().ToString()));
        }

        internal abstract string AggregationHelper(AggregatedColumn topLevelItem, DataStructureEntity topLevelEntity, AggregatedColumn item, Hashtable replaceParameterTexts, int level, StringBuilder joins, Hashtable dynamicParameters, Hashtable parameterReferences);
        internal abstract string RenderExpression(EntityFilterLookupReference lookupReference, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences);
        internal string RenderExpression(AggregatedColumn item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            StringBuilder joins = new StringBuilder();

            return AggregationHelper(item, entity, item, replaceParameterTexts, 1, joins, dynamicParameters, parameterReferences);
        }

        internal string RenderExpression(TableMappingItem item)
        {
            return NameLeftBracket + item.MappedObjectName + NameRightBracket;
        }

        private string RenderExpression(EntityRelationItem item)
        {
            return RenderExpression(item.RelatedEntity as ISchemaItem, null, null, null, null);
        }

        internal abstract string RenderExpression(FieldMappingItem item, DataStructureEntity dsEntity);
        
        internal string RenderExpression(ParameterReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, string parameterName, Hashtable parameterReferences)
        {
            if (parameterName == null)
            {
                parameterName = GetParameterName(entity, item);
            }

            if (replaceParameterTexts != null && replaceParameterTexts.ContainsKey(parameterName))
            {
                return (string)replaceParameterTexts[parameterName];
            }
            else
            {
                string name = ParameterReferenceChar + parameterName;
                string declarationName = ParameterDeclarationChar + parameterName;
                if (parameterReferences != null)
                {
                    if (!parameterReferences.Contains(declarationName))
                    {
                        parameterReferences.Add(declarationName, item);
                    }
                }

                return name;
            }
        }

        private string GetParameterName(DataStructureEntity entity, ParameterReference item)
        {
            return entity.Name + "_" + item.Parameter.Name;
        }

        internal string RenderExpression(EntityColumnReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            if (item.Field == null)
            {
                throw new Exception("Column not specified for " + item.Path);
            }

            return RenderExpression(item.Field, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
        }

        private string RenderExpression(EntityFilterReference item, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
            if (item.Roles == "" || item.Roles == null || auth.Authorize(SecurityManager.CurrentPrincipal, item.Roles))
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

        internal abstract string RenderConstant(DataConstant constant, bool userDefinedParameters);
        
        private string RenderExpression(DataConstantReference item)
        {
            return RenderConstant(item.DataConstant, UserDefinedParameters);
        }

//        private string RenderExpression(DataConstantReference item)
//        {
//            return RenderConstant(item.DataConstant);
//        }

        internal string RenderString(string text)
        {
            return "'" + text.Replace("'", "''") + "'";
        }

        internal abstract string RenderSortDirection(DataStructureColumnSortDirection direction);
        

        private string RenderExpression(FunctionCall item, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences)
        {
            if (item.Function.FunctionType == OrigamFunctionType.Database)
            {
                return RenderDatabaseFunction(item, entity, replaceParameterTexts,
                    dynamicParameters, parameterReferences);
            }
            else
            {
                return RenderBuiltinFunction(item, entity, replaceParameterTexts,
                    dynamicParameters, parameterReferences);
            }
        }
        internal abstract string RenderDatabaseFunction(FunctionCall item, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences);
        internal abstract string RenderBuiltinFunction(FunctionCall item, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences);
        internal string GetItemByFunctionParameter(
            FunctionCall item, string parameterName, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences)
        {
            ISchemaItem param = GetFunctionParameter(item, parameterName);
            ISchemaItem value = null;
            if(param.ChildItems.Count > 1)
            {
                throw new ArgumentOutOfRangeException("parameterName", 
                    parameterName, "Only 1 argument can be present to " 
                    + item.Path);
            }
            else if (param.HasChildItems)
            {
                value = param.ChildItems[0];
            }
            if (value == null)
            {
                return null;
            }
            return RenderExpression(value, entity, replaceParameterTexts, 
                dynamicParameters, parameterReferences);
        }
        internal IList<string> GetItemListByFunctionParameter(
            FunctionCall item, string parameterName, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences)
        {
            ISchemaItem param = GetFunctionParameter(item, parameterName);
            var result = new List<string>();
            foreach (var child in param.ChildItems)
            {
                result.Add(RenderExpression(child, entity,
                    replaceParameterTexts, dynamicParameters,
                    parameterReferences));
            }
            return result;
        }
        private static ISchemaItem GetFunctionParameter(FunctionCall item,
            string parameterName)
        {
            ISchemaItem param = item.GetChildByName(parameterName);
            if (param == null)
            {
                throw new ArgumentOutOfRangeException("parameterName",
                    parameterName, "Parameter not found for function "
                    + item.Path);
            }
            return param;
        }
        internal string RenderConcat(ArrayList concatSchemaItems, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
		{
			List<KeyValuePair<ISchemaItem, DataStructureEntity>> concatSchemaItemList =
				new List<KeyValuePair<ISchemaItem, DataStructureEntity>>();

			foreach (object o in concatSchemaItems)
			{
				concatSchemaItemList.Add(new KeyValuePair<ISchemaItem, DataStructureEntity>(o as ISchemaItem, entity));
			}
            return RenderConcat(concatSchemaItemList, null, replaceParameterTexts, dynamicParameters, parameterReferences);
		}
        internal abstract string RenderConcat(List<KeyValuePair<ISchemaItem, DataStructureEntity>> concatSchemaItemList, string separator, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences);
        #endregion

        #region Operators
        internal abstract string GetAggregationString(AggregationType type);
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

        public abstract string DdlDataType(OrigamDataType columnType, int dataLenght,
            DatabaseDataType dbDataType);
		#endregion

		#region ICloneable Members

		public object Clone()
		{
			MsSqlCommandGenerator gen = new MsSqlCommandGenerator();
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
