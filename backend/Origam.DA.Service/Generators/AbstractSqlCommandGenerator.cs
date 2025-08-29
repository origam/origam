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
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Origam.DA.Service.CustomCommandParser;
using Origam.DA.Service.Generators;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

namespace Origam.DA.Service;

public abstract class AbstractSqlCommandGenerator : IDbDataAdapterFactory, IDisposable
{
    protected readonly SQLValueFormatter sqlValueFormatter;
    internal readonly ParameterReference PageNumberParameterReference = new ParameterReference();
    internal readonly ParameterReference PageSizeParameterReference = new ParameterReference();
    internal readonly AbstractFilterRenderer filterRenderer;
    internal string _pageNumberParameterName;
    internal string _pageSizeParameterName;
    internal int _indentLevel = 0;
    protected readonly SqlRenderer sqlRenderer;

    public AbstractSqlCommandGenerator(
        string trueValue,
        string falseValue,
        SQLValueFormatter sqlValueFormatter,
        AbstractFilterRenderer filterRenderer,
        SqlRenderer sqlRenderer
    )
    {
        PageNumberParameterReference.ParameterId = new Guid("3e5e12e4-a0dd-4d35-a00a-2fdb267536d1");
        PageSizeParameterReference.ParameterId = new Guid("c310d577-d4d9-42da-af92-a5202ba26e79");
        True = trueValue;
        False = falseValue;
        this.sqlValueFormatter = sqlValueFormatter;
        this.filterRenderer = filterRenderer;
        this.sqlRenderer = sqlRenderer;
    }

    public abstract string CreateOutputTableSql(string tmpTable);
    public abstract string CreateDataStructureFooterSql(List<string> tmpTables);
    public abstract string FunctionDefinitionDdl(Function function);
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

        dbParam.ParameterName = sqlRenderer.ParameterDeclarationChar + name;

        return dbParam;
    }

    public abstract IDbCommand GetCommand(string cmdText);
    public abstract IDbCommand GetCommand(string cmdText, IDbConnection connection);
    public abstract IDbCommand GetCommand(
        string cmdText,
        IDbConnection connection,
        IDbTransaction transaction
    );

    public abstract DbDataAdapter GetAdapter();
    public abstract DbDataAdapter GetAdapter(IDbCommand command);
    public abstract DbDataAdapter CloneAdapter(DbDataAdapter adapter);
    public abstract IDbCommand CloneCommand(IDbCommand command);
    public abstract void DeriveStoredProcedureParameters(IDbCommand command);
    public abstract string GetIndexName(IDataEntity entity, DataEntityIndex index);

    public string True { get; }
    public string False { get; }

    public const string RowNumColumnName = "RowNum";

    public bool UserDefinedParameters { get; set; } = false;

    public bool ResolveAllFilters { get; set; }

    public bool PrettyFormat { get; set; }

    public bool GenerateConsoleUseSyntax
    {
        get => sqlRenderer.GenerateConsoleUseSyntax;
        set => sqlRenderer.GenerateConsoleUseSyntax = value;
    }

    public IDbCommand ScalarValueCommand(
        DataStructure ds,
        DataStructureFilterSet filter,
        DataStructureSortSet sortSet,
        ColumnsInfo columnsInfo,
        Hashtable parameters
    )
    {
        Hashtable selectParameterReferences = new Hashtable();
        IDbCommand cmd = GetCommand(
            SelectSql(
                selectParameters: new SelectParameters
                {
                    DataStructure = ds,
                    Entity = ds.Entities[0] as DataStructureEntity,
                    Filter = filter,
                    SortSet = sortSet,
                    ColumnsInfo = columnsInfo,
                    Parameters = parameters,
                    Paging = false,
                },
                replaceParameterTexts: null,
                selectParameterReferences: selectParameterReferences,
                restrictScalarToTop1: true,
                isInRecursion: false,
                forceDatabaseCalculation: true
            )
        );
        cmd.CommandType = CommandType.Text;
        BuildSelectParameters(cmd, selectParameterReferences);
        BuildFilterParameters(cmd, ds, filter, null, parameters);
        return cmd;
    }

    public DbDataAdapter CreateDataAdapter(
        SelectParameters adParameters,
        bool forceDatabaseCalculation
    )
    {
        if (!(adParameters.Entity.EntityDefinition is TableMappingItem))
        {
            throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
        }

        DbDataAdapter adapter = GetAdapter();
        BuildCommands(adapter, adParameters, forceDatabaseCalculation);
        adapter.TableMappings.Clear();
        adapter.TableMappings.Add(CreateMapping(adParameters.Entity));

        return adapter;
    }

    public DbDataAdapter CreateDataAdapter(
        string procedureName,
        List<DataStructureEntity> entitiesOrdered,
        IDbConnection connection,
        IDbTransaction transaction
    )
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

    public DbDataAdapter CreateSelectRowDataAdapter(
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        ColumnsInfo columnsInfo,
        bool forceDatabaseCalculation
    )
    {
        if (!(entity.EntityDefinition is TableMappingItem))
        {
            throw new Exception(ResourceUtils.GetString("OnlyMappedEntitiesToBeProcessed"));
        }

        DbDataAdapter adapter = GetAdapter();
        BuildSelectRowCommand(adapter, entity, filterSet, columnsInfo, forceDatabaseCalculation);
        adapter.TableMappings.Clear();
        adapter.TableMappings.Add(CreateMapping(entity));

        return adapter;
    }

    public DbDataAdapter CreateUpdateFieldDataAdapter(
        TableMappingItem table,
        FieldMappingItem field
    )
    {
        DbDataAdapter adapter = GetAdapter();
        BuildSelectUpdateFieldCommand(adapter, table, field);
        adapter.TableMappings.Clear();
        adapter.TableMappings.Add(CreateUpdateFieldMapping(table, field));

        return adapter;
    }

    public IDbCommand UpdateFieldCommand(TableMappingItem entity, FieldMappingItem field)
    {
        IDbCommand cmd = GetCommand(
            "UPDATE "
                + RenderExpression(entity)
                + " SET "
                + RenderExpression(field, null)
                + " = "
                + sqlRenderer.ParameterReferenceChar
                + "newValue WHERE "
                + RenderExpression(field, null)
                + " = "
                + sqlRenderer.ParameterReferenceChar
                + "oldValue"
        );
        cmd.CommandType = CommandType.Text;
        IDataParameter sqlParam = BuildParameter(
            sqlRenderer.ParameterDeclarationChar + "oldValue",
            null,
            field.DataType,
            field.MappedDataType,
            field.DataLength,
            field.AllowNulls
        );
        cmd.Parameters.Add(sqlParam);
        sqlParam = BuildParameter(
            sqlRenderer.ParameterDeclarationChar + "newValue",
            null,
            field.DataType,
            field.MappedDataType,
            field.DataLength,
            field.AllowNulls
        );
        cmd.Parameters.Add(sqlParam);
        return cmd;
    }

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

    private DataTableMapping CreateUpdateFieldMapping(
        TableMappingItem table,
        FieldMappingItem field
    )
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

    public void BuildSelectRowCommand(
        IDbDataAdapter adapter,
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        ColumnsInfo columnsInfo,
        bool forceDatabaseCalculation
    )
    {
        Hashtable selectParameterReferences = new Hashtable();

        adapter.SelectCommand = GetCommand(
            SelectRowSql(
                entity,
                filterSet,
                selectParameterReferences,
                columnsInfo,
                forceDatabaseCalculation
            )
        );

        BuildPrimaryKeySelectParameters(adapter.SelectCommand, entity);
        BuildSelectParameters(adapter.SelectCommand, selectParameterReferences);
    }

    public void BuildSelectUpdateFieldCommand(
        DbDataAdapter adapter,
        TableMappingItem table,
        FieldMappingItem field
    )
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

    public void BuildCommands(
        IDbDataAdapter adapter,
        SelectParameters selectParameters,
        bool forceDatabaseCalculation
    )
    {
        Hashtable selectParameterReferences = new Hashtable();
        DataStructure dataStructure = selectParameters.DataStructure;
        DataStructureEntity entity = selectParameters.Entity;

        FilterCommandParser filterCommandParser = new FilterCommandParser(
            dataStructureColumns: entity.Columns,
            filterRenderer: filterRenderer,
            whereFilterInput: selectParameters.CustomFilters.Filters,
            sqlRenderer: sqlRenderer
        );
        OrderByCommandParser orderByCommandParser = new OrderByCommandParser(
            orderingsInput: selectParameters.CustomOrderings.Orderings
        );

        adapter.SelectCommand = GetCommand(
            SelectSql(
                selectParameters: selectParameters,
                replaceParameterTexts: null,
                selectParameterReferences: selectParameterReferences,
                restrictScalarToTop1: false,
                isInRecursion: false,
                forceDatabaseCalculation: forceDatabaseCalculation,
                filterCommandParser: filterCommandParser,
                orderByCommandParser: orderByCommandParser
            )
        );

        BuildSelectParameters(adapter.SelectCommand, selectParameterReferences);
        BuildFilterParameters(
            adapter.SelectCommand,
            dataStructure,
            selectParameters.Filter,
            null,
            selectParameters.Parameters
        );
        AddCustomFilterParseParameters(
            adapter.SelectCommand,
            entity,
            filterCommandParser.ParameterDataList
        );

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

    private void AddCustomFilterParseParameters(
        IDbCommand command,
        DataStructureEntity dataStructureEntity,
        List<ParameterData> parameterDataList
    )
    {
        foreach (var parameterData in parameterDataList)
        {
            var dataStructureColumn = dataStructureEntity.Columns.Find(column =>
                column.Name == parameterData.ColumnName
            );
            var parameter = BuildParameter(
                parameterData.ParameterName,
                null,
                parameterData.DataType,
                null,
                dataStructureColumn.Field.DataLength,
                dataStructureColumn.Field.AllowNulls
            );
            parameter.Value = parameterData.Value;
            command.Parameters.Add(parameter);
        }
    }

    public void BuildFilterParameters(
        IDbCommand command,
        DataStructure ds,
        DataStructureFilterSet filterSet,
        Hashtable replaceParameterTexts,
        Hashtable parameters
    )
    {
        foreach (DataStructureEntity entity in ds.Entities)
        {
            foreach (EntityFilter filter in Filters(filterSet, entity, parameters, false))
            {
                Dictionary<string, ParameterReference> paramReferences = filter.ParameterReferences;
                foreach (var entry in paramReferences)
                {
                    ParameterReference parameterRef = entry.Value;
                    string paramName = RenderExpression(
                        parameterRef,
                        entity,
                        replaceParameterTexts,
                        null,
                        null
                    );

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
                    BuildFilterParameters(
                        command,
                        column.FinalLookup.ValueDataStructure,
                        null,
                        replaceParameterTexts,
                        parameters
                    );
                }
            }
        }
    }

    private IDbDataParameter BuildParameter(string paramName, ParameterReference parameterRef)
    {
        IDatabaseDataTypeMapping mappableDataType =
            parameterRef.Parameter as IDatabaseDataTypeMapping;
        return BuildParameter(
            paramName,
            null,
            parameterRef.Parameter.DataType,
            mappableDataType?.MappedDataType,
            parameterRef.Parameter.DataLength,
            parameterRef.Parameter.AllowNulls
        );
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
            if (
                column.Field is FieldMappingItem
                && column.UseLookupValue == false
                && column.UseCopiedValue == false
            )
            {
                command.Parameters.Add(CreateNewValueParameter(column));
            }
        }
    }

    public void BuildDeleteParameters(IDbCommand command, DataStructureEntity entity)
    {
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (
                column.Field is FieldMappingItem
                && column.UseLookupValue == false
                && column.UseCopiedValue == false
            )
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
            if (
                column.Field is FieldMappingItem
                && column.Field.IsPrimaryKey
                && column.UseLookupValue == false
                && column.UseCopiedValue == false
            )
            {
                command.Parameters.Add(CreateSelectRowParameter(column));
            }
        }
    }

    public void BuildUpdateFieldParameters(IDbCommand command, FieldMappingItem column)
    {
        command.Parameters.Add(
            BuildParameter(
                sqlRenderer.ParameterDeclarationChar + column.Name,
                column.Name,
                column.DataType,
                column.MappedDataType,
                column.DataLength,
                true
            )
        );
    }

    private IDataParameter CreateNewValueParameter(DataStructureColumn column)
    {
        FieldMappingItem dbField = GetDatabaseField(column);
        return BuildParameter(
            NewValueParameterName(column, true),
            column.Name,
            dbField.DataType,
            dbField.MappedDataType,
            dbField.DataLength,
            true
        );
    }

    private IDataParameter CreateSelectRowParameter(DataStructureColumn column)
    {
        FieldMappingItem dbField = GetDatabaseField(column);
        return BuildParameter(
            NewValueParameterName(column, true),
            column.Name,
            OrigamDataType.Array,
            null,
            dbField.DataLength,
            true
        );
    }

    private static FieldMappingItem GetDatabaseField(DataStructureColumn column)
    {
        FieldMappingItem dbField = column.Field as FieldMappingItem;
        if (dbField == null)
        {
            throw new InvalidCastException("Only database fields can be processed.");
        }
        return dbField;
    }

    private IDataParameter CreateOriginalValueParameter(DataStructureColumn column)
    {
        FieldMappingItem dbField = GetDatabaseField(column);
        IDataParameter result = BuildParameter(
            OriginalParameterName(column, true),
            column.Name,
            dbField.DataType,
            dbField.MappedDataType,
            dbField.DataLength,
            true
        );
        result.SourceVersion = DataRowVersion.Original;
        result.Direction = ParameterDirection.Input;
        return result;
    }

    private IDataParameter CreateOriginalValueParameterForNullComparison(DataStructureColumn column)
    {
        FieldMappingItem dbField = GetDatabaseField(column);
        IDataParameter result = BuildParameter(
            OriginalParameterNameForNullComparison(column, true),
            column.Name,
            dbField.DataType,
            dbField.MappedDataType,
            dbField.DataLength,
            true
        );
        result.SourceVersion = DataRowVersion.Original;
        result.Direction = ParameterDirection.Input;
        return result;
    }

    #region Main SQL Command Rendering

    public string TableListDefinitionDdl(ISchemaItemCollection tables)
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

    public string AddForeignKeyConstraintDdl(
        TableMappingItem table,
        DataEntityConstraint constraint
    )
    {
        StringBuilder ddl = new StringBuilder();

        ddl.AppendFormat(
            "ALTER TABLE {0} ADD {1}",
            sqlRenderer.NameLeftBracket + table.MappedObjectName + sqlRenderer.NameRightBracket,
            ForeignKeyConstraintDdl(table, constraint)
        );
        return ddl.ToString();
    }

    public string ParameterDeclarationChar => sqlRenderer.ParameterDeclarationChar;

    public string ForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint)
    {
        StringBuilder ddl = new StringBuilder();

        if (
            constraint.ForeignEntity is TableMappingItem
            && constraint.Fields[0] is FieldMappingItem
        )
        {
            string pkTableName = (constraint.ForeignEntity as TableMappingItem).MappedObjectName;

            ddl.AppendFormat(
                "CONSTRAINT {1}",
                sqlRenderer.NameLeftBracket + table.MappedObjectName + sqlRenderer.NameRightBracket,
                sqlRenderer.NameLeftBracket
                    + "FK_"
                    + table.MappedObjectName
                    + "_"
                    + (constraint.Fields[0] as FieldMappingItem).MappedColumnName
                    + "_"
                    + pkTableName
                    + sqlRenderer.NameRightBracket
            );

            ddl.Append(Environment.NewLine + "\tFOREIGN KEY (");
            int i = 0;
            foreach (FieldMappingItem field in constraint.Fields)
            {
                if (i > 0)
                {
                    ddl.Append(", ");
                }

                ddl.Append(Environment.NewLine + "\t\t" + RenderExpression(field, null));

                i++;
            }
            ddl.Append(Environment.NewLine + "\t)" + Environment.NewLine);

            ddl.AppendFormat(
                Environment.NewLine + "\tREFERENCES {0} (",
                sqlRenderer.NameLeftBracket + pkTableName + sqlRenderer.NameRightBracket
            );

            i = 0;
            foreach (FieldMappingItem field in constraint.Fields)
            {
                if (i > 0)
                {
                    ddl.Append(", ");
                }

                ddl.Append(
                    Environment.NewLine
                        + "\t\t"
                        + RenderExpression(field.ForeignKeyField, null, null, null, null)
                );

                i++;
            }

            ddl.Append(Environment.NewLine + "\t);");
        }

        return ddl.ToString();
    }

    public string AddColumnDdl(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();

        ddl.AppendFormat(
            "ALTER TABLE {0} ADD {1}",
            RenderExpression(field.ParentItem as TableMappingItem),
            ColumnDefinitionDdl(field)
        );

        if (!field.AllowNulls && field.DefaultValue != null)
        {
            string constraintName =
                "DF_"
                + (field.ParentItem as TableMappingItem).MappedObjectName
                + "_"
                + field.MappedColumnName;
            ddl.AppendFormat(
                " CONSTRAINT {0} DEFAULT {1};",
                sqlRenderer.NameLeftBracket + constraintName + sqlRenderer.NameRightBracket,
                this.RenderConstant(field.DefaultValue, false)
            );

            ddl.Append(Environment.NewLine);
            ddl.Append(DropDefaultValue(field, constraintName));
        }

        return ddl.ToString();
    }

    internal abstract string DropDefaultValue(FieldMappingItem field, string constraintName);

    public string AlterColumnDdl(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();

        ddl.AppendFormat(
            "ALTER TABLE {0} ALTER COLUMN {1}",
            RenderExpression(field.ParentItem as TableMappingItem),
            ChangeColumnDefinitionDdl(field)
        );
        return ddl.ToString();
    }

    private string ChangeColumnDefinitionDdl(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();
        ddl.AppendFormat(
            "{0} ",
            sqlRenderer.NameLeftBracket + field.MappedColumnName + sqlRenderer.NameRightBracket
        );
        ddl.Append(ChangeColumnDef(field));
        return ddl.ToString();
    }

    internal abstract string ChangeColumnDef(FieldMappingItem field);

    internal string ColumnDefinitionDdl(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();
        // fname | varchar(20) | NOT NULL | PRIMARY KEY
        ddl.AppendFormat(
            "{0} {1}",
            sqlRenderer.NameLeftBracket + field.MappedColumnName + sqlRenderer.NameRightBracket,
            DdlDataType(field.DataType, field.DataLength, field.MappedDataType)
        );
        if (field.AllowNulls)
        {
            ddl.Append(" NULL");
        }
        else
        {
            ddl.Append(" NOT NULL");
        }

        if (field.IsPrimaryKey)
        {
            ddl.Append(SqlPrimaryIndex());
        }

        return ddl.ToString();
    }

    protected abstract string SqlPrimaryIndex();

    public string IndexDefinitionDdl(IDataEntity entity, DataEntityIndex index, bool complete)
    {
        StringBuilder ddl = new StringBuilder();
        ddl.AppendFormat(
            "CREATE {0} INDEX  {1} ON {2} (",
            (index.IsUnique ? "UNIQUE " : ""),
            sqlRenderer.NameLeftBracket
                + GetIndexName(entity, index)
                + sqlRenderer.NameRightBracket,
            sqlRenderer.NameLeftBracket
                + (index.ParentItem as TableMappingItem).MappedObjectName
                + sqlRenderer.NameRightBracket
        );

        int i = 0;
        var sortedFields = index.ChildItemsByType<DataEntityIndexField>(
            DataEntityIndexField.CategoryConst
        );
        sortedFields.Sort();

        foreach (DataEntityIndexField field in sortedFields)
        {
            if (i > 0)
            {
                ddl.Append(", ");
            }

            ddl.AppendFormat(
                "{0} {1}",
                RenderExpression(field.Field, null, null, null, null),
                field.SortOrder == DataEntityIndexSortOrder.Descending ? "DESC" : "ASC"
            );

            i++;
        }
        ddl.Append(");");

        return ddl.ToString();
    }

    public abstract string DefaultDdlDataType(OrigamDataType columnType);

    public abstract IDbDataParameter BuildParameter(
        string paramName,
        string sourceColumn,
        OrigamDataType dataType,
        DatabaseDataType dbDataType,
        int dataLength,
        bool allowNulls
    );

    public string DdlDataType(OrigamDataType columnType, DatabaseDataType dbDataType)
    {
        if (dbDataType != null)
        {
            return dbDataType.MappedDatabaseTypeName;
        }

        return DefaultDdlDataType(columnType);
    }

    public string TableDefinitionDdl(TableMappingItem table)
    {
        if (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
        {
            throw new InvalidOperationException(
                ResourceUtils.GetString("CantDDLScript", table.DatabaseObjectType.ToString())
            );
        }

        StringBuilder ddl = new StringBuilder();
        ddl.AppendFormat(
            "CREATE TABLE {0} (",
            sqlRenderer.NameLeftBracket + table.MappedObjectName + sqlRenderer.NameRightBracket
        );

        int i = 0;
        foreach (IDataEntityColumn item in table.EntityColumns)
        {
            if (item is FieldMappingItem)
            {
                FieldMappingItem field = item as FieldMappingItem;

                if (i > 0)
                {
                    ddl.Append(",");
                }

                ddl.Append(Environment.NewLine + "\t" + ColumnDefinitionDdl(field));

                i++;
            }
        }

        ddl.Append(");");

        if (table.EntityIndexes.Count > 0)
        {
            foreach (DataEntityIndex index in table.EntityIndexes)
            {
                ddl.Append(Environment.NewLine);
                ddl.Append(IndexDefinitionDdl(table, index, false));
            }
        }

        return ddl.ToString();
    }

    public string SelectParameterDeclarationsSql(
        DataStructureFilterSet filter,
        bool paging,
        string columnName
    )
    {
        StringBuilder result = new StringBuilder();
        Hashtable ht = new Hashtable();
        DataStructure ds = filter.RootItem as DataStructure;
        result.AppendLine(sqlRenderer.CreateDataStructureHead());
        foreach (DataStructureEntity entity in ds.Entities)
        {
            SelectParameterDeclarationsSql(
                result,
                ht,
                ds,
                entity,
                filter,
                null,
                paging,
                columnName
            );
        }
        result.AppendLine(sqlRenderer.DeclareBegin());
        SelectParameterDeclarationsSetSql(result, ht);
        return result.ToString();
    }

    public string SelectParameterDeclarationsSql(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        bool paging,
        string columnName
    )
    {
        StringBuilder result = new StringBuilder();
        Hashtable ht = new Hashtable();
        SelectParameterDeclarationsSql(result, ht, ds, entity, filter, null, paging, columnName);
        return result.ToString();
    }

    public string SelectParameterDeclarationsSql(
        DataStructure ds,
        DataStructureSortSet sort,
        bool paging,
        string columnName
    )
    {
        StringBuilder result = new StringBuilder();
        Hashtable ht = new Hashtable();
        result.AppendLine(sqlRenderer.CreateDataStructureHead());
        foreach (DataStructureEntity entity in ds.Entities)
        {
            SelectParameterDeclarationsSql(result, ht, ds, entity, null, sort, paging, columnName);
        }
        result.AppendLine(sqlRenderer.DeclareBegin());
        SelectParameterDeclarationsSetSql(result, ht);
        return result.ToString();
    }

    internal List<string> Parameters(IDbCommand cmd)
    {
        int declarationLength = sqlRenderer.ParameterDeclarationChar.Length;
        var list = new List<string>(cmd.Parameters.Count);
        foreach (IDataParameter param in cmd.Parameters)
        {
            list.Add(param.ParameterName.Substring(declarationLength));
        }

        list.Sort();
        return list;
    }

    internal IDbCommand SelectCommand(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sort,
        bool paging,
        string columnName
    )
    {
        var adapterParameters = new SelectParameters
        {
            DataStructure = ds,
            Entity = entity,
            Filter = filter,
            SortSet = sort,
            Parameters = new Hashtable(),
            Paging = paging,
            ColumnsInfo = new ColumnsInfo(columnName),
        };
        DbDataAdapter adapter = CreateDataAdapter(adapterParameters, false);
        return ((IDbDataAdapter)adapter).SelectCommand;
    }

    public List<string> Parameters(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sort,
        bool paging,
        string columnName
    )
    {
        IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
        return Parameters(cmd);
    }

    internal void SelectParameterDeclarationsSql(
        StringBuilder result,
        Hashtable ht,
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sort,
        bool paging,
        string columnName
    )
    {
        IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
        List<string> list = Parameters(cmd);
        foreach (string paramName in list)
        {
            IDataParameter param =
                cmd.Parameters[sqlRenderer.ParameterDeclarationChar + paramName] as IDataParameter;

            if (!ht.Contains(param.ParameterName))
            {
                result.AppendFormat(
                    "DECLARE {0} " + sqlRenderer.DeclareAsSql() + " {1};{2}",
                    param.ParameterName,
                    SqlDataType(param),
                    Environment.NewLine
                );

                ht.Add(param.ParameterName, null);
            }
        }
    }

    internal abstract string SqlDataType(IDataParameter param);

    internal void SelectParameterDeclarationsSetSql(StringBuilder result, Hashtable parameters)
    {
        foreach (string name in parameters.Keys)
        {
            result.Append(sqlRenderer.SetParameter(name));
        }
    }

    public string SelectSql(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sortSet,
        ColumnsInfo columnsInfo,
        Hashtable parameters,
        Hashtable selectParameterReferences,
        bool paging
    )
    {
        return SelectSql(
            new SelectParameters
            {
                Entity = entity,
                DataStructure = ds,
                Filter = filter,
                ColumnsInfo = columnsInfo,
                SortSet = sortSet,
                Parameters = parameters,
                Paging = paging,
            },
            replaceParameterTexts: null,
            selectParameterReferences: selectParameterReferences,
            restrictScalarToTop1: true,
            isInRecursion: false,
            forceDatabaseCalculation: false
        );
    }

    public string SelectSql(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sortSet,
        ColumnsInfo columnsInfo,
        Hashtable parameters,
        Hashtable selectParameterReferences,
        bool paging,
        bool forceDatabaseCalculation
    )
    {
        return SelectSql(
            new SelectParameters
            {
                Entity = entity,
                DataStructure = ds,
                Filter = filter,
                ColumnsInfo = columnsInfo,
                SortSet = sortSet,
                Parameters = parameters,
                Paging = paging,
            },
            replaceParameterTexts: null,
            selectParameterReferences: selectParameterReferences,
            restrictScalarToTop1: true,
            isInRecursion: false,
            forceDatabaseCalculation: forceDatabaseCalculation
        );
    }

    public string SelectSql(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureFilterSet filter,
        DataStructureSortSet sortSet,
        ColumnsInfo columnsInfo,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable selectParameterReferences,
        bool forceDatabaseCalculation
    )
    {
        return SelectSql(
            selectParameters: new SelectParameters
            {
                Entity = entity,
                DataStructure = ds,
                Filter = filter,
                ColumnsInfo = columnsInfo,
                SortSet = sortSet,
                Parameters = dynamicParameters,
                Paging = false,
            },
            replaceParameterTexts: replaceParameterTexts,
            selectParameterReferences: selectParameterReferences,
            restrictScalarToTop1: true,
            isInRecursion: false,
            forceDatabaseCalculation: forceDatabaseCalculation
        );
    }

    internal string SelectSql(
        SelectParameters selectParameters,
        Hashtable replaceParameterTexts,
        Hashtable selectParameterReferences,
        bool restrictScalarToTop1,
        bool isInRecursion,
        bool forceDatabaseCalculation,
        FilterCommandParser filterCommandParser = null,
        OrderByCommandParser orderByCommandParser = null
    )
    {
        var entity = selectParameters.Entity;
        var distinct = selectParameters.Distinct;
        var paging = selectParameters.Paging;
        var ds = selectParameters.DataStructure;
        var sortSet = selectParameters.SortSet;
        var columnsInfo = selectParameters.ColumnsInfo;
        var dynamicParameters = selectParameters.Parameters;
        var customOrdering = selectParameters.CustomOrderings;
        var customGrouping = selectParameters.CustomGrouping;
        var filter = selectParameters.Filter;
        var rowLimit = selectParameters.RowLimit;
        var rowOffset = selectParameters.RowOffset;
        bool rowOffsetSpecified = rowOffset.HasValue && rowOffset != 0;

        if (!(entity.EntityDefinition is TableMappingItem))
        {
            throw new Exception(
                "Only database mapped entities can be processed by the Data Service!"
            );
        }

        if (paging)
        {
            if (PageNumberParameterReference.PersistenceProvider == null)
            {
                PageNumberParameterReference.PersistenceProvider = ds.PersistenceProvider;
                PageSizeParameterReference.PersistenceProvider = ds.PersistenceProvider;
                _pageNumberParameterName =
                    sqlRenderer.ParameterReferenceChar
                    + PageNumberParameterReference.Parameter.Name;
                _pageSizeParameterName =
                    sqlRenderer.ParameterReferenceChar + PageSizeParameterReference.Parameter.Name;
            }

            selectParameterReferences.Add(
                sqlRenderer.ParameterDeclarationChar + PageNumberParameterReference.Parameter.Name,
                PageNumberParameterReference
            );
            selectParameterReferences.Add(
                sqlRenderer.ParameterDeclarationChar + PageSizeParameterReference.Parameter.Name,
                PageSizeParameterReference
            );
        }

        StringBuilder sqlExpression = new StringBuilder();
        StringBuilder orderByBuilder = new StringBuilder();
        StringBuilder groupByBuilder = new StringBuilder();

        // when processing lookup columns we process semicolon delimited list of columns
        // to be returned as a single concatted field
        // Example: FirstName;Name -> concat(FirstName, ', ', Name)
        // Select
        RenderSelectColumns(
            selectParameters: selectParameters,
            sqlExpression: sqlExpression,
            orderByBuilder: orderByBuilder,
            groupByBuilder: groupByBuilder,
            replaceParameterTexts: replaceParameterTexts,
            selectParameterReferences: selectParameterReferences,
            isInRecursion: isInRecursion,
            concatScalarColumns: restrictScalarToTop1,
            forceDatabaseCalculation: forceDatabaseCalculation,
            filterCommandParser: filterCommandParser,
            orderByCommandParser: orderByCommandParser
        );
        bool orderBySpecified = (
            !string.IsNullOrWhiteSpace(orderByCommandParser?.Sql) || orderByBuilder.Length > 0
        );
        // paging column
        if (paging)
        {
            if (sortSet != null && (customOrdering?.IsEmpty ?? true))
            {
                sqlExpression.AppendFormat(
                    ", ROW_NUMBER() OVER (ORDER BY {0}) AS {1}",
                    orderByBuilder,
                    RowNumColumnName
                );
            }
            else
            {
                sqlExpression.AppendFormat(
                    ", ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS {0}",
                    RowNumColumnName
                );
            }
        }

        // From
        RenderSelectFromClause(sqlExpression, entity);

        bool whereExists = false;

        if (!entity.ParentItem.PrimaryKey.Equals(ds.PrimaryKey))
        {
            // render joins that we need for fields in this entity
            foreach (
                var relation in entity.ChildItemsByType<DataStructureEntity>(
                    DataStructureEntity.CategoryConst
                )
            )
            {
                if (
                    relation.RelationType == RelationType.LeftJoin
                    || relation.RelationType == RelationType.InnerJoin
                )
                {
                    RenderSelectRelation(
                        sqlExpression,
                        relation,
                        relation,
                        filter,
                        replaceParameterTexts,
                        true,
                        true,
                        0,
                        false,
                        dynamicParameters,
                        selectParameterReferences
                    );
                }
            }

            // if this is not a root entity, we make "where exists( )...to parent entities, so only detail records
            // are selected for their master records
            RenderSelectExistsClause(
                sqlExpression,
                entity.RootEntity,
                entity,
                filter,
                replaceParameterTexts,
                dynamicParameters,
                selectParameterReferences
            );
            whereExists = true;
        }
        else
        {
            // for the root entity we render all child relation filters (filterParent relations)
            StringBuilder joinedFilterBuilder = new StringBuilder();
            int counter = 0;
            foreach (
                var relation in entity.ChildItemsByType<DataStructureEntity>(
                    DataStructureEntity.CategoryConst
                )
            )
            {
                if (
                    relation.RelationType == RelationType.LeftJoin
                    || relation.RelationType == RelationType.InnerJoin
                )
                {
                    RenderSelectRelation(
                        sqlExpression,
                        relation,
                        relation,
                        filter,
                        replaceParameterTexts,
                        true,
                        true,
                        0,
                        false,
                        dynamicParameters,
                        selectParameterReferences
                    );
                }
                else if (
                    relation.RelationType == RelationType.FilterParent
                    || relation.RelationType == RelationType.NotExists
                )
                {
                    bool skip = false;

                    skip = IgnoreConditionalEntity(relation, dynamicParameters);

                    // skip dynamic filter parts
                    if (!skip && relation.IgnoreCondition != DataStructureIgnoreCondition.None)
                    {
                        skip = IgnoreEntityWhenNoFilters(relation, filter, dynamicParameters);
                    }

                    // process all joined entities except all those dynamically skipped
                    if (!skip)
                    {
                        if (counter > 0)
                        {
                            joinedFilterBuilder.Append(" AND ");
                        }

                        string existsClause = null;
                        switch (relation.RelationType)
                        {
                            case RelationType.FilterParent:
                            {
                                existsClause = " EXISTS";
                                break;
                            }

                            case RelationType.NotExists:
                            {
                                existsClause = " NOT EXISTS";
                                break;
                            }
                        }

                        joinedFilterBuilder.AppendFormat(
                            existsClause + " (SELECT * FROM {0} AS {1}",
                            RenderExpression(relation.EntityDefinition, null, null, null, null),
                            sqlRenderer.NameLeftBracket
                                + relation.Name
                                + sqlRenderer.NameRightBracket
                        );

                        RenderSelectRelation(
                            joinedFilterBuilder,
                            relation,
                            relation,
                            filter,
                            replaceParameterTexts,
                            true,
                            true,
                            0,
                            false,
                            dynamicParameters,
                            selectParameterReferences
                        );

                        joinedFilterBuilder.Append(")");

                        counter++;
                    }
                }
            }

            if (joinedFilterBuilder.Length > 0)
            {
                if (whereExists)
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

        // Where filter - only on root entity, all other filters are on relations
        StringBuilder whereBuilder = new StringBuilder();
        RenderSelectWherePart(
            whereBuilder,
            entity,
            filter,
            replaceParameterTexts,
            dynamicParameters,
            selectParameterReferences
        );

        if (whereBuilder.Length > 0)
        {
            if (whereExists)
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

            sqlExpression.Append(whereBuilder.ToString());
        }

        if (!string.IsNullOrEmpty(filterCommandParser?.Sql))
        {
            if (whereExists)
            {
                PrettyIndent(sqlExpression);
                sqlExpression.Append("AND ");
            }
            else
            {
                PrettyLine(sqlExpression);
                sqlExpression.Append("WHERE ");
            }
            PostProcessCustomCommandParserWhereClause(
                replaceParameterTexts,
                selectParameterReferences,
                filterCommandParser,
                sqlExpression,
                entity,
                dynamicParameters
            );
        }

        // GROUP BY
        if (groupByBuilder.Length > 0)
        {
            PrettyLine(sqlExpression);
            sqlExpression.AppendFormat("GROUP BY {0}", groupByBuilder.ToString());
        }

        // ORDER BY
        if (!string.IsNullOrWhiteSpace(orderByCommandParser?.Sql))
        {
            PrettyLine(sqlExpression);
            sqlExpression.AppendFormat("ORDER BY {0}", orderByCommandParser.Sql);
        }
        else
        {
            if (!paging && orderByBuilder.Length > 0)
            {
                PrettyLine(sqlExpression);
                sqlExpression.AppendFormat("ORDER BY {0}", orderByBuilder.ToString());
            }
        }

        string finalString = sqlExpression.ToString();

        // subqueries, etc. will have TOP 1, so it is sure that they select only 1 value
        if (columnsInfo != null && !columnsInfo.IsEmpty && restrictScalarToTop1)
        {
            finalString = sqlRenderer.SelectClause(finalString, 1);
        }
        else if (rowLimit.HasValue && (!rowOffset.HasValue || rowOffset == 0))
        {
            finalString = sqlRenderer.SelectClause(finalString, rowLimit.Value);
        }
        else if (distinct)
        {
            finalString = sqlRenderer.SelectClauseWithDistinct(finalString);
        }
        else
        {
            finalString = sqlRenderer.SelectClause(finalString, 0);
        }

        if (paging)
        {
            if (rowOffsetSpecified)
            {
                throw new ArgumentException(
                    "Cannot render SQL with \"WHERE\" pagination and \"OFFSET-FETCH\" pagination together"
                );
            }
            finalString =
                $"SELECT * FROM ({finalString}) _page WHERE "
                + $"_page.{RowNumColumnName} BETWEEN "
                + $"(({_pageNumberParameterName} - 1) "
                + $"* {_pageSizeParameterName}) + 1 "
                + $"AND {_pageSizeParameterName} * {_pageNumberParameterName}"
                + $" ORDER BY _page.{RowNumColumnName} ASC";
        }
        else if (rowOffsetSpecified && orderBySpecified)
        {
            finalString += $" OFFSET {rowOffset} ROWS FETCH NEXT {rowLimit} ROWS ONLY;";
        }

        if (customGrouping != null && GroupingUsesLookup(customGrouping, entity))
        {
            var columnNames = selectParameters.ColumnsInfo.ColumnNames;
            if (selectParameters.AggregatedColumns.Count > 0)
            {
                columnNames.AddRange(
                    selectParameters.AggregatedColumns.Select(x => x.SqlQueryColumnName)
                );
            }
            string sqlColumnNames = string.Join(
                ", ",
                columnNames.Select(col =>
                    sqlRenderer.NameLeftBracket + col + sqlRenderer.NameRightBracket
                )
            );
            finalString =
                $"SELECT {sqlColumnNames}, {sqlRenderer.CountAggregate()}(*) AS {ColumnData.GroupByCountColumn} FROM (\n"
                + finalString
                + "\n"
                + ") as Query\n"
                + $"GROUP BY {sqlColumnNames}";
        }

        return finalString;
    }

    private static bool GroupingUsesLookup(Grouping customGrouping, DataStructureEntity entity)
    {
        var allLookupColumnNames = entity
            .ChildrenRecursive.OfType<DataStructureEntity>()
            .Concat(new[] { entity })
            .SelectMany(entity =>
            {
                var dataStructureColumnNames = entity
                    .ChildItems.OfType<DataStructureColumn>()
                    .Where(x => x.UseLookupValue)
                    .Select(x => x.Name);
                var entityColumnNames = entity
                    .EntityDefinition.EntityColumns.OfType<LookupField>()
                    .Select(lookupField => lookupField.Name);
                return dataStructureColumnNames.Concat(entityColumnNames);
            });

        return customGrouping != null && allLookupColumnNames.Contains(customGrouping.GroupBy);
    }

    private void PostProcessCustomCommandParserWhereClause(
        Hashtable replaceParameterTexts,
        Hashtable selectParameterReferences,
        FilterCommandParser filterCommandParser,
        StringBuilder sqlExpression,
        DataStructureEntity entity,
        Hashtable dynamicParameters
    )
    {
        if (filterCommandParser.Sql == null)
        {
            return;
        }
        string[] sqlParts = filterCommandParser.Sql.Split("AND");
        for (int i = 0; i < sqlParts.Length; i++)
        {
            sqlExpression.Append(
                PostProcessCustomCommandParserWhereClauseSegment(
                    sqlParts[i],
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences
                )
            );
            if (i < sqlParts.Length - 1)
            {
                sqlExpression.Append(" AND ");
            }
        }
    }

    internal bool IgnoreEntityWhenNoFilters(
        DataStructureEntity relation,
        DataStructureFilterSet filter,
        Hashtable dynamicParameters
    )
    {
        // If IgnoreWhenNoFilters is on and there is no filter for this entity, we skip it.
        // This is important for dynamic queries, where filters depend on actual parameters (e.g. null value means not filtering at all).
        // When no filter, then also the whole relation (which would be inner join, thus limiting the parent rows) is ignored.

        bool ignoreImplicitFilters =
            relation.IgnoreCondition == DataStructureIgnoreCondition.IgnoreWhenNoExplicitFilters;

        int filterCount = Filters(filter, relation, dynamicParameters, ignoreImplicitFilters).Count;

        if (filterCount > 0)
        {
            return false;
        }

        // we test for child entities as well
        foreach (
            DataStructureEntity childEntity in relation.ChildItemsByTypeRecursive(
                DataStructureEntity.CategoryConst
            )
        )
        {
            filterCount += Filters(
                filter,
                childEntity,
                dynamicParameters,
                ignoreImplicitFilters
            ).Count;

            // some filters found, we break
            if (filterCount > 0)
            {
                return false;
            }
        }

        return true;
    }

    internal bool IgnoreConditionalEntity(DataStructureEntity relation, Hashtable dynamicParameters)
    {
        // skip dynamic entity relations
        IParameterService parameterService =
            ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

        string constant = null;

        if (parameterService != null & relation.ConditionEntityConstant != null)
        {
            constant = (string)
                parameterService.GetParameterValue(
                    relation.ConditionEntityConstantId,
                    OrigamDataType.String
                );
        }

        if (relation.ConditionEntityParameterName != null)
        {
            if (dynamicParameters[relation.ConditionEntityParameterName] == null & constant == null)
            {
                return true;
            }
            else if (
                dynamicParameters[relation.ConditionEntityParameterName] == null
                | constant == null
            )
            {
                return false;
            }
            else if (
                dynamicParameters[relation.ConditionEntityParameterName].ToString() != constant
            )
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
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (ShouldUpdateColumn(column, entity))
            {
                string fieldName = RenderExpression(column.Field, null, null, null, null);
                string paramName = NewValueParameterName(column, false);

                if (column.Field.AutoIncrement == false)
                {
                    if (insertColumns > 0)
                    {
                        insertColumnsBuilder.Append(", ");
                        insertValuesBuilder.Append(", ");
                    }
                    insertColumnsBuilder.Append(fieldName);
                    insertValuesBuilder.Append(paramName);
                    insertColumns++;
                }

                UpsertType type = column.UpsertType;
                if (entity.AllFields && column.Field.IsPrimaryKey)
                {
                    type = UpsertType.Key;
                }

                if (type != UpsertType.Key && type != UpsertType.InsertOnly)
                {
                    if (updateColumns > 0)
                    {
                        updateBuilder.Append(", ");
                    }
                    updateColumns++;
                }

                switch (type)
                {
                    case UpsertType.Key:
                    {
                        if (keys > 0)
                        {
                            keysBuilder.Append(", ");
                            searchPredicatesBuilder.Append(" AND ");
                        }
                        keysBuilder.AppendFormat(RenderUpsertKey(paramName, fieldName));
                        searchPredicatesBuilder.AppendFormat(
                            "{0}.{1} = src.{1}",
                            tableName,
                            fieldName
                        );
                        keys++;
                        break;
                    }

                    case UpsertType.Replace:
                    {
                        updateBuilder.AppendFormat("{0} = {1}", fieldName, paramName);
                        break;
                    }

                    case UpsertType.Increase:
                    {
                        updateBuilder.AppendFormat("{0} = {0} + {1}", fieldName, paramName);
                        break;
                    }

                    case UpsertType.Decrease:
                    {
                        updateBuilder.AppendFormat("{0} = {0} - {1}", fieldName, paramName);
                        break;
                    }

                    case UpsertType.InsertOnly:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            "UpsertType",
                            column.UpsertType,
                            "Unknown UpsertType"
                        );
                }
            }
        }

        if (keys == 0)
        {
            throw new Exception(
                "Cannot build an UPSERT command, no UPSERT keys specified in the entity."
            );
        }
        sqlExpression.AppendFormat(
            MergeSql(
                tableName,
                keysBuilder,
                searchPredicatesBuilder,
                updateBuilder,
                insertColumnsBuilder,
                insertValuesBuilder
            )
        );
        return sqlExpression.ToString();
    }

    internal abstract string MergeSql(
        string tableName,
        StringBuilder keysBuilder,
        StringBuilder searchPredicatesBuilder,
        StringBuilder updateBuilder,
        StringBuilder insertColumnsBuilder,
        StringBuilder insertValuesBuilder
    );

    public string InsertSql(DataStructure ds, DataStructureEntity entity)
    {
        if (entity.UseUpsert)
        {
            return UpsertSql(ds, entity);
        }
        StringBuilder sqlExpression = new StringBuilder();
        StringBuilder sqlExpression2 = new StringBuilder();
        sqlExpression.AppendFormat(
            "INSERT INTO {0} (",
            RenderExpression(entity.EntityDefinition, null, null, null, null)
        );
        bool existAutoIncrement = false;
        int i = 0;
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (ShouldUpdateColumn(column, entity) && column.Field.AutoIncrement == false)
            {
                if (i > 0)
                {
                    sqlExpression.Append(",");
                    sqlExpression2.Append(",");
                }
                PrettyIndent(sqlExpression);
                PrettyIndent(sqlExpression2);
                sqlExpression.Append(RenderExpression(column.Field, null, null, null, null));
                sqlExpression2.Append(NewValueParameterName(column, false));
                i++;
                if (column.Field.AutoIncrement)
                {
                    existAutoIncrement = true;
                }
            }
        }
        PrettyLine(sqlExpression);
        sqlExpression.Append(") VALUES (");
        sqlExpression.Append(sqlExpression2.ToString());
        sqlExpression.Append(")");
        // If there is any auto increment column, we include a SELECT statement after INSERT
        if (existAutoIncrement)
        {
            RenderSelectUpdatedData(sqlExpression, entity);
        }
        return sqlExpression.ToString();
    }

    internal void RenderSelectUpdatedData(StringBuilder sqlExpression, DataStructureEntity entity)
    {
        var primaryKeys = new List<DataStructureColumn>();
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (column.Field.IsPrimaryKey)
            {
                primaryKeys.Add(column);
            }
        }
        if (primaryKeys.Count == 0)
        {
            throw new OrigamException(ResourceUtils.GetString("NoPrimaryKey", entity.Name));
        }
        PrettyLine(sqlExpression);
        sqlExpression.Append(sqlRenderer.Sequence(entity.Name, (primaryKeys[0]).Name));
    }

    public string SelectRowSql(
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        Hashtable selectParameterReferences,
        ColumnsInfo columnsInfo,
        bool forceDatabaseCalculation
    )
    {
        StringBuilder sqlExpression = new StringBuilder();

        var primaryKeys = new List<DataStructureColumn>();
        StringBuilder primaryDetachKeys = new StringBuilder();
        sqlExpression.Append("SELECT ");

        RenderSelectColumns(
            entity.RootItem as DataStructure,
            sqlExpression,
            new StringBuilder(),
            new StringBuilder(),
            entity,
            columnsInfo,
            new Hashtable(),
            new Hashtable(),
            null,
            selectParameterReferences,
            forceDatabaseCalculation
        );

        foreach (DataStructureColumn column in entity.Columns)
        {
            if (
                column.Field is FieldMappingItem
                && column.UseLookupValue == false
                && column.UseCopiedValue == false
            )
            {
                if (column.Field.IsPrimaryKey)
                {
                    primaryKeys.Add(column);
                }
            }
            if (
                column.Field is DetachedField
                && column.UseLookupValue == false
                && column.UseCopiedValue == false
            )
            {
                if (column.Field.IsPrimaryKey)
                {
                    primaryDetachKeys.Append(column.Name).Append(";");
                }
            }
        }
        if (primaryKeys.Count == 0)
        {
            string errorMessage =
                "The primary key of entity " + entity.Name + " must be in the database.";
            if (!string.IsNullOrEmpty(primaryDetachKeys.ToString()))
            {
                errorMessage +=
                    "Primary key items "
                    + primaryDetachKeys
                        .ToString()
                        .Substring(0, primaryDetachKeys.ToString().Length - 1)
                    + " are virtual!";
                ;
            }

            throw new Exception(errorMessage);
        }
        PrettyLine(sqlExpression);
        sqlExpression.AppendFormat(
            "FROM {0} AS {1} ",
            RenderExpression(entity.EntityDefinition, null, null, null, null),
            sqlRenderer.NameLeftBracket + entity.Name + sqlRenderer.NameRightBracket
        );

        foreach (
            var relation in entity.ChildItemsByType<DataStructureEntity>(
                DataStructureEntity.CategoryConst
            )
        )
        {
            if (
                relation.RelationType == RelationType.LeftJoin
                || relation.RelationType == RelationType.InnerJoin
            )
            {
                RenderSelectRelation(
                    sqlExpression,
                    relation,
                    relation,
                    filterSet,
                    null,
                    true,
                    true,
                    0,
                    false,
                    null,
                    selectParameterReferences
                );
            }
        }
        PrettyLine(sqlExpression);
        sqlExpression.Append("WHERE (");
        int i = 0;
        foreach (DataStructureColumn column in primaryKeys)
        {
            if (i > 0)
            {
                sqlExpression.Append(" AND ");
            }

            PrettyIndent(sqlExpression);
            sqlExpression.Append(
                sqlRenderer.Array(
                    sqlRenderer.NameLeftBracket
                        + entity.Name
                        + sqlRenderer.NameRightBracket
                        + "."
                        + RenderExpression(column.Field, null, null, null, null),
                    NewValueParameterName(column, false)
                )
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

        var selectKeys = new List<DataStructureColumn>();
        sqlExpression.Append("SELECT ");

        int i = 0;
        foreach (DataStructureColumn column in entity.Columns)
        {
            // we only select the primary key and the changed field
            if (column.Field.IsPrimaryKey | column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
            {
                if (i > 0)
                {
                    sqlExpression.Append(", ");
                }

                if (column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
                {
                    selectKeys.Add(column);
                }

                sqlExpression.AppendFormat(
                    "{0} AS {1}",
                    RenderExpression(column.Field, entity, null, null, null),
                    sqlRenderer.NameLeftBracket + column.Name + sqlRenderer.NameRightBracket
                );

                i++;
            }
        }

        sqlExpression.AppendFormat(
            " FROM {0} AS {1} WHERE (",
            RenderExpression(entity.EntityDefinition, null, null, null, null),
            sqlRenderer.NameLeftBracket + entity.Name + sqlRenderer.NameRightBracket
        );

        i = 0;
        foreach (DataStructureColumn column in selectKeys)
        {
            if (i > 0)
            {
                sqlExpression.Append(" AND ");
            }

            sqlExpression.AppendFormat(
                "{0} = {1}",
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

        var selectKeys = new List<DataStructureColumn>();
        sqlExpression.Append("SELECT " + sqlRenderer.CountAggregate() + "(*) ");

        foreach (DataStructureColumn column in entity.Columns)
        {
            if (column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
            {
                selectKeys.Add(column);
            }
        }

        sqlExpression.AppendFormat(
            " FROM {0} AS {1} WHERE (",
            RenderExpression(entity.EntityDefinition, null, null, null, null),
            sqlRenderer.NameLeftBracket + entity.Name + sqlRenderer.NameRightBracket
        );

        int i = 0;
        foreach (DataStructureColumn column in selectKeys)
        {
            if (i > 0)
            {
                sqlExpression.Append(" AND ");
            }

            sqlExpression.AppendFormat(
                "{0} = {1}",
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

        sqlExpression.AppendFormat(
            "DELETE FROM {0} ",
            RenderExpression(entity.EntityDefinition, null, null, null, null)
        );

        RenderUpdateDeleteWherePart(sqlExpression, entity);

        return sqlExpression.ToString();
    }

    public string UpdateSql(DataStructure ds, DataStructureEntity entity)
    {
        StringBuilder sqlExpression = new StringBuilder();

        sqlExpression.AppendFormat(
            "UPDATE {0} SET ",
            RenderExpression(entity.EntityDefinition, null, null, null, null)
        );

        bool existAutoIncrement = false;

        int i = 0;
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (ShouldUpdateColumn(column, entity))
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
                    var dependenciesSource = column.Field.ChildItemsByType<EntityFieldDependency>(
                        EntityFieldDependency.CategoryConst
                    );
                    var dependencies = new List<EntityFieldDependency>();
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
                        sqlExpression.AppendFormat(
                            "{0} = (CASE " + writeOnlyValue + " END)",
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
                                if (
                                    dependentColumn.Field.Name == dep.Field.Name
                                    && ShouldUpdateColumn(dependentColumn, entity)
                                )
                                {
                                    sqlExpression.AppendFormat(
                                        " WHEN {0} IS NULL THEN NULL",
                                        NewValueParameterName(dependentColumn, false)
                                    );
                                    break;
                                }
                            }
                        }
                        sqlExpression.AppendFormat(
                            " " + writeOnlyValue + " END)",
                            field,
                            parameter
                        );
                    }
                }
                else
                {
                    // simple field, just update
                    sqlExpression.AppendFormat("{0} = {1}", field, parameter);
                }
                i++;
                if (column.Field.AutoIncrement)
                {
                    existAutoIncrement = true;
                }
            }
        }
        RenderUpdateDeleteWherePart(sqlExpression, entity);

        // If there is any auto increment column, we include a SELECT statement after INSERT
        if (existAutoIncrement)
        {
            RenderSelectUpdatedData(sqlExpression, entity);
        }
        sqlExpression.Append(";");
        return sqlExpression.ToString();
    }

    internal bool ShouldUpdateColumn(DataStructureColumn column, DataStructureEntity entity)
    {
        if (!(column.Field is FieldMappingItem))
        {
            return false;
        }

        if (column.UseLookupValue)
        {
            return false;
        }

        if (column.UseCopiedValue)
        {
            return false;
        }

        if (column.Entity != null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Select parts
    internal bool RenderSelectColumns(
        DataStructure ds,
        StringBuilder sqlExpression,
        StringBuilder orderByBuilder,
        StringBuilder groupByBuilder,
        DataStructureEntity entity,
        ColumnsInfo columnsInfo,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        DataStructureSortSet sortSet,
        Hashtable selectParameterReferences,
        bool forceDatabaseCalculation
    )
    {
        return RenderSelectColumns(
            selectParameters: new SelectParameters
            {
                DataStructure = ds,
                Entity = entity,
                SortSet = sortSet,
                ColumnsInfo = columnsInfo,
                Parameters = dynamicParameters,
            },
            sqlExpression: sqlExpression,
            orderByBuilder: orderByBuilder,
            groupByBuilder: groupByBuilder,
            replaceParameterTexts: replaceParameterTexts,
            selectParameterReferences: selectParameterReferences,
            isInRecursion: false,
            concatScalarColumns: true,
            forceDatabaseCalculation: forceDatabaseCalculation
        );
    }

    private record GroupByData(DataStructureColumn Column, string Expression);

    internal bool RenderSelectColumns(
        SelectParameters selectParameters,
        StringBuilder sqlExpression,
        StringBuilder orderByBuilder,
        StringBuilder groupByBuilder,
        Hashtable replaceParameterTexts,
        Hashtable selectParameterReferences,
        bool isInRecursion,
        bool concatScalarColumns,
        bool forceDatabaseCalculation,
        FilterCommandParser filterCommandParser = null,
        OrderByCommandParser orderByCommandParser = null
    )
    {
        var ds = selectParameters.DataStructure;
        var entity = selectParameters.Entity;
        var columnsInfo = selectParameters.ColumnsInfo;
        var sortSet = selectParameters.SortSet;
        var customOrderings = selectParameters.CustomOrderings;
        var customGrouping = selectParameters.CustomGrouping;
        var aggregatedColumns = selectParameters.AggregatedColumns;
        var dynamicParameters = selectParameters.Parameters;
        var customFilters = selectParameters.CustomFilters;

        GroupByData groupByData = null;
        int i = 0;
        List<string> group = new List<string>();
        SortedList<int, SortOrder> order = new SortedList<int, SortOrder>();
        bool groupByNeeded = false;
        string orderByExpression = "";
        if (concatScalarColumns && columnsInfo != null && columnsInfo.Count > 1)
        {
            sqlExpression.Append(" ");
            sqlExpression.Append(
                RenderConcat(
                    selectParameters,
                    isInRecursion,
                    forceDatabaseCalculation,
                    sqlValueFormatter.RenderString(", "),
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences,
                    filterCommandParser,
                    orderByCommandParser
                )
            );
            return false;
        }
        i = 0;
        var dataStructureColumns = GetSortedColumns(
            entity,
            columnsInfo?.ColumnNames,
            aggregatedColumns
        );
        foreach (DataStructureColumn column in dataStructureColumns)
        {
            LookupOrderingInfo customOrderingInfo = LookupOrderingInfo.TryCreate(
                customOrderings.Orderings,
                column.Name
            );
            string groupByExpression = "";
            ColumnRenderData columnRenderData = RenderDataStructureColumn(
                ds,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                sortSet,
                selectParameterReferences,
                isInRecursion,
                forceDatabaseCalculation,
                ref groupByExpression,
                order,
                ref groupByNeeded,
                columnsInfo ?? ColumnsInfo.Empty,
                column,
                customOrderingInfo,
                filterCommandParser,
                orderByCommandParser,
                selectParameters.RowOffset
            );
            if (customGrouping != null && column.Name == customGrouping.GroupBy)
            {
                groupByData = new GroupByData(column, groupByExpression);
            }
            string expression;
            if (columnRenderData != null)
            {
                if (i > 0)
                {
                    sqlExpression.Append(",");
                }

                PrettyIndent(sqlExpression);
                i++;
                if (!string.IsNullOrWhiteSpace(customGrouping?.GroupingUnit))
                {
                    var timeGroupingRenderer = new TimeGroupingRenderer(
                        columnRenderData: columnRenderData,
                        columnDataToSql: ColumnDataToSql,
                        groupingUnit: customGrouping.GroupingUnit,
                        sqlRenderer: sqlRenderer
                    );
                    string[] columnsWithoutAliases = timeGroupingRenderer.RenderWithoutAliases();
                    orderByCommandParser.SetColumnExpressionsIfMissing(
                        column.Name,
                        columnsWithoutAliases
                    );
                    string allColumnsExpression = string.Join(", ", columnsWithoutAliases);
                    group.Add(allColumnsExpression);
                    expression = timeGroupingRenderer.RenderWithAliases();
                }
                else
                {
                    if (customGrouping != null && customGrouping.GroupBy == column.Name)
                    {
                        orderByExpression = columnRenderData.Expression;
                    }

                    if (!string.IsNullOrWhiteSpace(groupByExpression))
                    {
                        group.Add(groupByExpression);
                    }

                    expression = ColumnDataToSql(columnRenderData);
                }
                sqlExpression.Append(expression);
            }
        }

        if (aggregatedColumns != null)
        {
            RenderAggregations(
                selectParameters: selectParameters,
                sqlExpression: sqlExpression,
                replaceParameterTexts: replaceParameterTexts,
                selectParameterReferences: selectParameterReferences,
                isInRecursion: isInRecursion,
                noColumnsRenderedYet: i == 0
            );
        }

        List<DataStructureColumn> expressionColumns = dataStructureColumns;
        if ((aggregatedColumns?.Count ?? 0) > 0 && customFilters.HasLookups)
        {
            // these should not be rendered, that is why they were not added to dataStructureColumns.
            var filterColumns = GetSortedColumns(
                    entity,
                    customFilters.FilterLookups.Keys.ToList(),
                    aggregatedColumns
                )
                .Where(column => customFilters.FilterLookups.Keys.Contains(column.Name))
                .ToList();
            expressionColumns.AddRange(filterColumns);
        }

        SetColumnExpressions(
            commandParser: filterCommandParser,
            lookUps: customFilters.FilterLookups,
            isInRecursion: isInRecursion,
            entity: entity,
            ds: ds,
            replaceParameterTexts: replaceParameterTexts,
            selectParameterReferences: selectParameterReferences,
            dynamicParameters: dynamicParameters,
            dataStructureColumns: expressionColumns,
            columnsInfo: columnsInfo
        );

        SetColumnExpressions(
            commandParser: orderByCommandParser,
            lookUps: customOrderings.FilterLookups,
            isInRecursion: isInRecursion,
            entity: entity,
            ds: ds,
            replaceParameterTexts: replaceParameterTexts,
            selectParameterReferences: selectParameterReferences,
            dynamicParameters: dynamicParameters,
            dataStructureColumns: dataStructureColumns,
            columnsInfo: columnsInfo
        );

        if (customGrouping != null)
        {
            sqlExpression.Append(
                $", {sqlRenderer.CountAggregate()}(*) AS {ColumnData.GroupByCountColumn} "
            );

            if (customGrouping.LookupId != Guid.Empty)
            {
                var lookup =
                    ServiceManager
                        .Services.GetService<IPersistenceService>()
                        .SchemaProvider.RetrieveInstance(
                            typeof(DataServiceDataLookup),
                            new Key(customGrouping.LookupId)
                        ) as DataServiceDataLookup;

                var resultExpression = RenderLookupColumnExpression(
                    ds,
                    entity,
                    groupByData.Column,
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences,
                    lookup
                );
                sqlExpression.Append(" , ");
                sqlExpression.Append(resultExpression);
                sqlExpression.Append($" AS {ColumnData.GroupByCaptionColumn} ");
            }
            else
            {
                if (
                    !group.Any(groupByExpression =>
                        groupByExpression.Contains(customGrouping.GroupBy)
                        || groupByExpression == orderByExpression
                    )
                )
                {
                    if (
                        groupByData.Column.Name == customGrouping.GroupBy
                        && !group.Contains(groupByData.Expression)
                    )
                    {
                        group.Add(groupByData.Expression);
                    }
                }
            }
            groupByNeeded = true;
        }
        if (order.Count > 0)
        {
            i = 0;
            foreach (KeyValuePair<int, SortOrder> entry in order)
            {
                if (customGrouping != null && customGrouping.GroupBy != entry.Value.ColumnName)
                {
                    continue;
                }

                if (i > 0)
                {
                    orderByBuilder.Append(",");
                }
                PrettyIndent(orderByBuilder);
                orderByBuilder.AppendFormat(
                    "{0} {1}",
                    entry.Value.Expression,
                    RenderSortDirection(entry.Value.SortDirection)
                );

                i++;
            }
        }
        if (groupByNeeded)
        {
            i = 0;
            foreach (string expression in group)
            {
                if (i > 0)
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

    private void SetColumnExpressions(
        ICustomCommandParser commandParser,
        Dictionary<string, Guid> lookUps,
        bool isInRecursion,
        DataStructureEntity entity,
        DataStructure ds,
        Hashtable replaceParameterTexts,
        Hashtable selectParameterReferences,
        Hashtable dynamicParameters,
        IEnumerable<DataStructureColumn> dataStructureColumns,
        ColumnsInfo columnsInfo
    )
    {
        if (commandParser == null)
        {
            return;
        }

        foreach (string columnName in commandParser.Columns)
        {
            if (lookUps != null && lookUps.ContainsKey(columnName))
            {
                Guid lookupId = lookUps[columnName];
                var lookup =
                    ServiceManager
                        .Services.GetService<IPersistenceService>()
                        .SchemaProvider.RetrieveInstance(
                            typeof(DataServiceDataLookup),
                            new Key(lookupId)
                        ) as DataServiceDataLookup;
                var dataStructureColumn = dataStructureColumns.First(x => x.Name == columnName);
                var resultExpression = RenderLookupColumnExpression(
                    ds,
                    entity,
                    dataStructureColumn,
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences,
                    lookup
                );
                commandParser.SetColumnExpressionsIfMissing(columnName, new[] { resultExpression });
            }
            else
            {
                var dataStructureColumn = entity.Columns.First(x => x.Name == columnName);
                bool groupByNeeded1 = false;
                string groupExpression = "";
                string columnExpression = GetDataStructureColumnSqlExpression(
                    ds,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences,
                    isInRecursion,
                    ref groupByNeeded1,
                    columnsInfo,
                    dataStructureColumn,
                    ref groupExpression
                );
                commandParser.SetColumnExpressionsIfMissing(columnName, new[] { columnExpression });
            }
        }
    }

    private void RenderAggregations(
        SelectParameters selectParameters,
        StringBuilder sqlExpression,
        Hashtable replaceParameterTexts,
        Hashtable selectParameterReferences,
        bool isInRecursion,
        bool noColumnsRenderedYet
    )
    {
        var ds = selectParameters.DataStructure;
        var entity = selectParameters.Entity;
        var columnsInfo = selectParameters.ColumnsInfo;
        var aggregatedColumns = selectParameters.AggregatedColumns;
        var dynamicParameters = selectParameters.Parameters;

        string groupExpression = "";
        bool groupByNeeded = false;
        for (int i = 0; i < aggregatedColumns.Count; i++)
        {
            var aggregation = aggregatedColumns[i];
            var column = entity.Columns.First(col => col.Name == aggregation.ColumnName);
            string renderedColumn = GetDataStructureColumnSqlExpression(
                ds,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                selectParameterReferences,
                isInRecursion,
                ref groupByNeeded,
                columnsInfo,
                column,
                ref groupExpression
            );
            if (i == 0 && noColumnsRenderedYet)
            {
                sqlExpression.Append(" ");
            }
            else
            {
                sqlExpression.Append(" ,");
            }

            switch (aggregation.AggregationType)
            {
                case CustomAggregationType.Max:
                {
                    sqlExpression.Append(
                        $"MAX({renderedColumn}) as {aggregation.SqlQueryColumnName} "
                    );
                    break;
                }

                case CustomAggregationType.Sum:
                {
                    sqlExpression.Append(
                        $"SUM({renderedColumn}) as {aggregation.SqlQueryColumnName} "
                    );
                    break;
                }

                case CustomAggregationType.Avg:
                {
                    sqlExpression.Append(
                        $"AVG(Cast ({renderedColumn} as Float)) as {aggregation.SqlQueryColumnName} "
                    );
                    break;
                }

                case CustomAggregationType.Min:
                {
                    sqlExpression.Append(
                        $"MIN({renderedColumn}) as {aggregation.SqlQueryColumnName} "
                    );
                    break;
                }

                case CustomAggregationType.Count:
                {
                    sqlExpression.Append(
                        $"{sqlRenderer.CountAggregate()}({renderedColumn}) as {aggregation.SqlQueryColumnName} "
                    );
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private List<DataStructureColumn> GetSortedColumns(
        DataStructureEntity entity,
        List<string> scalarColumnNames,
        List<Aggregation> aggregatedColumns
    )
    {
        bool noColumnsRequested = scalarColumnNames == null || scalarColumnNames.Count == 0;
        bool noAggregateColumns = aggregatedColumns == null || aggregatedColumns.Count == 0;
        if (noColumnsRequested && noAggregateColumns)
        {
            return entity.Columns;
        }
        if (noColumnsRequested && !noAggregateColumns)
        {
            return new List<DataStructureColumn>();
        }

        List<string> missingColumns = (scalarColumnNames ?? new List<string>())
            .Where(x =>
                !entity.Columns.Exists(y => y.Name == x)
                && x != ColumnData.GroupByCountColumn.Name
                && x != ColumnData.GroupByCaptionColumn.Name
            )
            .ToList();
        if (missingColumns.Count > 0)
        {
            throw new Exception(
                $@"Data structure entity {entity.Name}[{
                    entity.Id}] is missing {
                    string.Join(", ", missingColumns)} column(s)."
            );
        }
        return entity.Columns.OrderBy(x => scalarColumnNames.IndexOf(x.Name)).ToList();
    }

    private ColumnRenderData RenderDataStructureColumn(
        DataStructure ds,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        DataStructureSortSet sortSet,
        Hashtable selectParameterReferences,
        bool isInRecursion,
        bool forceDatabaseCalculation,
        ref string group,
        SortedList<int, SortOrder> order,
        ref bool groupByNeeded,
        ColumnsInfo columnsInfo,
        DataStructureColumn column,
        LookupOrderingInfo orderingInfo,
        FilterCommandParser filterCommandParser,
        OrderByCommandParser orderByCommandParser,
        int? rowOffset = null
    )
    {
        ColumnRenderData result = null;
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
        else if (
            columnsInfo.ColumnNames.Contains(column.Name)
            && ShouldBeProcessed(forceDatabaseCalculation, column, functionCall)
        )
        {
            processColumn = true;
        }
        else if (
            columnsInfo.ColumnNames.Count == 0
            && ShouldBeProcessed(forceDatabaseCalculation, column, functionCall)
        )
        {
            processColumn = true;
        }
        else if (
            ((columnsInfo.ColumnNames.Count == 0) || columnsInfo.ColumnNames.Contains(column.Name))
            && aggregatedColumn != null
        )
        {
            if (forceDatabaseCalculation)
            {
                processColumn = true;
            }
            else
            {
                bool found = false;
                foreach (
                    var childEntity in entity.ChildItemsByType<DataStructureEntity>(
                        DataStructureEntity.CategoryConst
                    )
                )
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
                if (!found)
                {
                    processColumn = true;
                }
            }
        }
        else if (
            columnsInfo.RenderSqlForDetachedFields
            && columnsInfo.ColumnNames.Contains(column.Name)
            && column.Field is DetachedField
        )
        {
            processColumn = true;
        }

        string resultExpression = "";
        string groupExpression = "";

        if (processColumn || column.IsColumnSorted(sortSet))
        {
            resultExpression = GetDataStructureColumnSqlExpression(
                ds,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                selectParameterReferences,
                isInRecursion,
                ref groupByNeeded,
                columnsInfo,
                column,
                ref groupExpression
            );

            if (processColumn && !string.IsNullOrWhiteSpace(resultExpression))
            {
                result = new ColumnRenderData
                {
                    Expression = resultExpression,
                    Alias = column.Name,
                };

                // anything not having aggregation will eventually go to GROUP BY
                if (column.Aggregation == AggregationType.None)
                {
                    group = groupExpression;
                }
            }
        }

        // does not matter if processColumn=true, because we want to sort anytime sorting is specified,
        // e.g. if this is a scalar query and sorting is by another than the scalar column
        if (column.IsColumnSorted(sortSet) || orderingInfo != null)
        {
            System.Diagnostics.Debug.Assert(
                resultExpression != String.Empty,
                "No expression generated for sorting.",
                "Column: " + column.Path
            );
            SortOrder sortOrder = new SortOrder();
            string sortExpression = resultExpression;
            // if the column is a lookup column, we will sort by the looked-up
            // value, not by the source value, this will bring the same logic
            // as in the UI - when user sorts, it will always sort by a looked-up
            // values
            if ((column.FinalLookup != null || orderingInfo != null) && !column.UseLookupValue)
            {
                sortExpression = RenderLookupColumnExpression(
                    ds,
                    entity,
                    column,
                    replaceParameterTexts,
                    dynamicParameters,
                    selectParameterReferences,
                    orderingInfo?.Lookup,
                    rowOffset
                );
            }
            sortOrder.ColumnName = column.Name;
            sortOrder.Expression = sortExpression;
            if (orderingInfo == null)
            {
                sortOrder.SortDirection = column.SortDirection(sortSet);
                if (order.ContainsKey(column.SortOrder(sortSet)))
                {
                    throw new InvalidOperationException(
                        ResourceUtils.GetString(
                            "ErrorSortOrder",
                            column.SortOrder(sortSet).ToString(),
                            column.Path
                        )
                    );
                }
                order.Add(column.SortOrder(sortSet), sortOrder);
            }
            else
            {
                sortOrder.SortDirection = orderingInfo.Direction;
                order.Add(orderingInfo.SortOrder, sortOrder);
            }
        }
        return result;
    }

    private string ColumnDataToSql(ColumnRenderData columnRenderData)
    {
        return string.Format(
            "{0} AS {1}",
            columnRenderData.Expression,
            sqlRenderer.NameLeftBracket + columnRenderData.Alias + sqlRenderer.NameRightBracket
        );
    }

    private string GetDataStructureColumnSqlExpression(
        DataStructure ds,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable selectParameterReferences,
        bool isInRecursion,
        ref bool groupByNeeded,
        ColumnsInfo columnsInfo,
        DataStructureColumn column,
        ref string groupExpression
    )
    {
        string resultExpression;
        if (column.UseLookupValue)
        {
            resultExpression = RenderLookupColumnExpression(
                ds,
                entity,
                column,
                replaceParameterTexts,
                dynamicParameters,
                selectParameterReferences
            );
            var field = column.Field is LookupField lookupField ? lookupField.Field : column.Field;
            groupExpression = RenderExpression(
                field,
                column.Entity ?? entity,
                replaceParameterTexts,
                dynamicParameters,
                selectParameterReferences
            );
        }
        else
        {
            resultExpression = RenderExpression(
                item: column.Field,
                entity: column.Entity ?? entity,
                replaceParameterTexts: replaceParameterTexts,
                dynamicParameters: dynamicParameters,
                parameterReferences: selectParameterReferences,
                renderSqlForDetachedFields: columnsInfo.RenderSqlForDetachedFields
            );
            groupExpression = resultExpression;

            if (column.Aggregation != AggregationType.None)
            {
                if (column.Field is AggregatedColumn)
                {
                    throw new NotSupportedException(
                        ResourceUtils.GetString("ErrorAggregInAggreg", column.Path)
                    );
                }

                resultExpression = FixAggregationDataType(column.DataType, resultExpression);
                resultExpression = FixSumAggregation(
                    column.Aggregation,
                    GetAggregationString(column.Aggregation) + "(" + resultExpression + ")"
                );
                groupByNeeded = true;
            }
        }

        if (column.DataType == OrigamDataType.Geography)
        {
            if (!isInRecursion)
            {
                // convert to text, becouse .net didn't have geolocation data type
                resultExpression = sqlRenderer.ConvertGeoToTextClause(resultExpression);
            }
        }

        return resultExpression;
    }

    private static bool ShouldBeProcessed(
        bool forceDatabaseCalculation,
        DataStructureColumn column,
        FunctionCall functionCall
    )
    {
        return (
            column.Field is FieldMappingItem
            || column.Field is LookupField
            || (
                functionCall != null
                && functionCall.Function.FunctionType == OrigamFunctionType.Database
            )
            || (functionCall != null && functionCall.ForceDatabaseCalculation)
            || (functionCall != null && forceDatabaseCalculation)
            || (functionCall != null && column.Entity != null)
            || (functionCall != null && column.Aggregation != AggregationType.None)
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

    internal string FixSumAggregation(AggregationType aggregationType, string expression)
    {
        if (aggregationType == AggregationType.Sum)
        {
            return sqlRenderer.IsNull() + " (" + expression + ", 0)";
        }

        return expression;
    }

    private string RenderLookupColumnExpression(
        DataStructure ds,
        DataStructureEntity entity,
        DataStructureColumn column,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences,
        DataServiceDataLookup customLookup = null,
        int? rowOffset = null
    )
    {
        if (column.Aggregation != AggregationType.None)
        {
            throw new InvalidOperationException(
                ResourceUtils.GetString("ErrorLookupAggreg", column.Path)
            );
        }

        return RenderLookupColumnExpression(
            ds,
            column.Entity == null ? entity : column.Entity,
            column.Field,
            column.FinalLookup ?? customLookup,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences,
            rowOffset
        );
    }

    private string RenderLookupColumnExpression(
        DataStructure ds,
        DataStructureEntity entity,
        IDataEntityColumn field,
        IDataLookup lookup,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences,
        int? rowOffset = null
    )
    {
        return RenderLookupColumnExpression(
            ds,
            entity,
            field,
            lookup,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences,
            false,
            rowOffset
        );
    }

    internal string RenderLookupColumnExpression(
        DataStructure ds,
        DataStructureEntity entity,
        IDataEntityColumn field,
        IDataLookup lookup,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences,
        bool isInRecursion,
        int? rowOffset = null
    )
    {
        DataServiceDataLookup dataServiceLookup = lookup as DataServiceDataLookup;

        if (dataServiceLookup == null)
        {
            throw new ArgumentOutOfRangeException(
                "DefaultLookup",
                lookup,
                ResourceUtils.GetString("LookupTypeUnsupportedException")
            );
        }

        if (
            dataServiceLookup.ValueMethod != null
            && !(dataServiceLookup.ValueMethod is DataStructureFilterSet)
        )
        {
            throw new ArgumentOutOfRangeException(
                "ListMethod",
                dataServiceLookup.ListMethod,
                ResourceUtils.GetString("LookupListMethodTypeUnsupportedException")
            );
        }

        DataStructureFilterSet valueFilterSet =
            dataServiceLookup.ValueMethod as DataStructureFilterSet;
        DataStructureEntity lookupEntity =
            dataServiceLookup.ValueDataStructure.Entities[0] as DataStructureEntity;

        // any lookups with same entity name as any of the entities in this datastructure must be renamed
        bool lookupRenamed = false;
        if (
            ds
                .Entities.Cast<DataStructureEntity>()
                .Any(dataStructureEntity => dataStructureEntity.Name == lookupEntity.Name)
        )
        {
            lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
            lookupEntity.Name = "lookup" + lookupEntity.Name;
            foreach (
                var dataStructureEntity in lookupEntity.ChildrenRecursive.OfType<DataStructureEntity>()
            )
            {
                dataStructureEntity.Name = "lookup" + dataStructureEntity.Name;
            }
            lookupRenamed = true;
        }

        Hashtable replaceTexts = new Hashtable(1);

        ISchemaItem renderField = field;
        if (field is LookupField)
        {
            renderField = (field as LookupField).Field;
        }
        string myColumn = RenderExpression(
            renderField,
            entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );

        if (dataServiceLookup.ValueMethod == null)
        {
            throw new ArgumentOutOfRangeException(
                "ValueFilterSet",
                null,
                ResourceUtils.GetString("NoValueFilterSetForLookup", dataServiceLookup.Path)
            );
        }

        // replace lookup parameters with keys from the entity
        foreach (string key in dataServiceLookup.ValueMethod.ParameterReferences.Keys)
        {
            string finalKey = key;
            if (lookupRenamed)
            {
                if (finalKey != null)
                {
                    finalKey = "lookup" + finalKey;
                }
            }

            // check if the parameters are not system (custom) parameters
            if (CustomParameterService.MatchParameter(finalKey) == null)
            {
                replaceTexts.Add(finalKey, myColumn);
            }
        }
        _indentLevel++;
        StringBuilder builder = new StringBuilder();
        try
        {
            builder.Append(
                "("
                    + SelectSql(
                        new SelectParameters
                        {
                            DataStructure = dataServiceLookup.ValueDataStructure,
                            Entity = lookupEntity,
                            Filter = valueFilterSet,
                            SortSet = dataServiceLookup.ValueSortSet,
                            ColumnsInfo = new ColumnsInfo(dataServiceLookup.ValueDisplayMember),
                            Parameters = dynamicParameters,
                            Paging = false,
                            RowOffset = rowOffset,
                        },
                        replaceParameterTexts: replaceTexts,
                        selectParameterReferences: parameterReferences,
                        restrictScalarToTop1: true,
                        isInRecursion: true,
                        forceDatabaseCalculation: true
                    )
            );
        }
        finally
        {
            _indentLevel--;
        }
        PrettyIndent(builder);
        builder.Append(")");
        return builder.ToString();
    }

    internal void RenderSelectFromClause(
        StringBuilder sqlExpression,
        DataStructureEntity baseEntity
    )
    {
        PrettyLine(sqlExpression);
        sqlExpression.Append("FROM");
        PrettyIndent(sqlExpression);
        sqlExpression.AppendFormat(
            "{0} AS {1}",
            RenderExpression(baseEntity.EntityDefinition, null, null, null, null),
            sqlRenderer.NameLeftBracket + baseEntity.Name + sqlRenderer.NameRightBracket
        );
    }

    internal void RenderSelectExistsClause(
        StringBuilder sqlExpression,
        DataStructureEntity baseEntity,
        DataStructureEntity stopAtEntity,
        DataStructureFilterSet filter,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        PrettyLine(sqlExpression);
        sqlExpression.AppendFormat(
            "WHERE EXISTS (SELECT * FROM {0} AS {1}",
            RenderExpression(baseEntity.Entity, null, null, null, null),
            sqlRenderer.NameLeftBracket + baseEntity.Name + sqlRenderer.NameRightBracket
        );

        bool stopAtIncluded = false;
        bool notExistsIncluded = false;

        foreach (
            var relation in baseEntity.ChildItemsByType<DataStructureEntity>(
                DataStructureEntity.CategoryConst
            )
        )
        {
            if (relation.RelationType != RelationType.LeftJoin)
            {
                if (relation.PrimaryKey.Equals(stopAtEntity.PrimaryKey))
                {
                    // current entity we do after all the others, because it will depend on the others
                    stopAtIncluded = true;
                }
                else if (relation.RelationType == RelationType.NotExists)
                {
                    notExistsIncluded = true;
                }
                else
                {
                    RenderSelectRelation(
                        sqlExpression,
                        relation,
                        stopAtEntity,
                        filter,
                        replaceParameterTexts,
                        true,
                        false,
                        0,
                        true,
                        dynamicParameters,
                        parameterReferences
                    );
                }
            }
        }

        // finally we do current entity
        if (stopAtIncluded)
        {
            RenderSelectRelation(
                sqlExpression,
                stopAtEntity,
                stopAtEntity,
                filter,
                replaceParameterTexts,
                true,
                false,
                0,
                false,
                dynamicParameters,
                parameterReferences
            );
        }

        if (notExistsIncluded)
        {
            int notExistsCount = (stopAtIncluded ? 1 : 0);
            foreach (
                DataStructureEntity relation in baseEntity.ChildItemsByType<DataStructureEntity>(
                    DataStructureEntity.CategoryConst
                )
            )
            {
                if (relation.RelationType == RelationType.NotExists)
                {
                    string s;
                    if (notExistsCount == 0)
                    {
                        s = " WHERE ";
                    }
                    else
                    {
                        s = " AND ";
                    }

                    sqlExpression.AppendFormat(
                        s + "NOT EXISTS (SELECT * FROM {0} AS {1}",
                        RenderExpression(relation.EntityDefinition, null, null, null, null),
                        sqlRenderer.NameLeftBracket + relation.Name + sqlRenderer.NameRightBracket
                    );

                    RenderSelectRelation(
                        sqlExpression,
                        relation,
                        relation,
                        filter,
                        replaceParameterTexts,
                        true,
                        false,
                        0,
                        true,
                        dynamicParameters,
                        parameterReferences
                    );

                    sqlExpression.Append(")");
                }
            }
        }

        StringBuilder whereBuilder = new StringBuilder();
        RenderSelectWherePart(
            whereBuilder,
            baseEntity,
            filter,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );

        if (whereBuilder.Length > 0)
        {
            sqlExpression.Append(" AND ");
            sqlExpression.Append(whereBuilder);
        }

        sqlExpression.Append(")");
    }

    internal bool CanSkipSelectRelation(
        DataStructureEntity relation,
        DataStructureEntity stopAtEntity
    )
    {
        if (relation.RelationType != RelationType.Normal)
        {
            return false;
        }

        if (stopAtEntity.PrimaryKey.Equals(relation.PrimaryKey))
        {
            return false;
        }

        return true;
    }

    internal void RenderSelectRelation(
        StringBuilder sqlExpression,
        DataStructureEntity dsEntity,
        DataStructureEntity stopAtEntity,
        DataStructureFilterSet filter,
        Hashtable replaceParameterTexts,
        bool skipStopAtEntity,
        bool includeFilter,
        int numberOfJoins,
        bool includeAllRelations,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        // we render the sub relation only if
        // 1. this relation is INNER JOIN (except when IgnoreWhenNoFilters = true and there ARE no filters)
        // 2. our target entity is one of sub-entities of this relation
        // 3. one of sub-relations of this relation is INNER JOIN (except when IgnoreWhenNoFilters = true and there ARE no filters)
        // 4. this is actually our stopAtEntity relation
        if (includeAllRelations)
        {
            if (!dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey))
            {
                if (
                    dsEntity.RelationType == RelationType.Normal
                    || dsEntity.RelationType == RelationType.LeftJoin
                )
                {
                    if (!dsEntity.ChildItemsRecursive.Contains(stopAtEntity))
                    {
                        return;
                    }
                }
            }
        }
        else
        {
            if (CanSkipSelectRelation(dsEntity, stopAtEntity))
            {
                return;
            }
        }

        // ignore conditional relations
        if (IgnoreConditionalEntity(dsEntity, dynamicParameters))
        {
            return;
        }

        // ignore relations when no filters
        if (dsEntity.IgnoreCondition != DataStructureIgnoreCondition.None)
        {
            if (IgnoreEntityWhenNoFilters(dsEntity, filter, dynamicParameters))
            {
                return;
            }
        }

        JoinBeginType beginType;
        if (
            skipStopAtEntity
            && dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey)
            && dsEntity.RelationType != RelationType.InnerJoin
            && dsEntity.RelationType != RelationType.LeftJoin
        )
        {
            if (numberOfJoins > 0)
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
            {
                string joinString = (
                    dsEntity.RelationType == RelationType.LeftJoin
                        ? "LEFT OUTER JOIN"
                        : "INNER JOIN"
                );

                relationBuilder.AppendFormat(
                    "{0} {1} AS {2} ON",
                    joinString,
                    RenderExpression(assoc.AssociatedEntity, null, null, null, null),
                    sqlRenderer.NameLeftBracket + dsEntity.Name + sqlRenderer.NameRightBracket
                );
                numberOfJoins++;
                break;
            }

            case JoinBeginType.Where:
            {
                relationBuilder.Append("WHERE ");
                break;
            }

            case JoinBeginType.And:
            {
                relationBuilder.Append("AND ");
                break;
            }
        }
        int i = 0;
        if (assoc.IsOR)
        {
            relationBuilder.Append("(");
        }

        foreach (ISchemaItem item in assoc.ChildItems)
        {
            PrettyIndent(relationBuilder);
            if (i > 0)
            {
                if (assoc.IsOR)
                {
                    relationBuilder.Append(" OR ");
                }
                else
                {
                    relationBuilder.Append(" AND ");
                }
            }

            if (item is EntityRelationColumnPairItem)
            {
                RenderSelectRelationKey(
                    relationBuilder,
                    item as EntityRelationColumnPairItem,
                    dsEntity.ParentItem as DataStructureEntity,
                    dsEntity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
            }
            else if (item is EntityRelationFilter)
            {
                RenderFilter(
                    relationBuilder,
                    (item as EntityRelationFilter).Filter,
                    dsEntity,
                    parameterReferences
                );
            }
            else
            {
                throw new NotSupportedException(
                    ResourceUtils.GetString("TypeNotSupportedByDatabase", item.GetType().ToString())
                );
            }

            i++;
        }

        if (assoc.IsOR)
        {
            relationBuilder.Append(")");
        }

        if (
            !(
                dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey)
                & skipStopAtEntity
                & includeFilter == false
            )
        )
        {
            StringBuilder whereBuilder = new StringBuilder();
            RenderSelectWherePart(
                whereBuilder,
                dsEntity,
                filter,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );

            if (whereBuilder.Length > 0)
            {
                PrettyIndent(relationBuilder);
                relationBuilder.AppendFormat(" AND {0}", whereBuilder);
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
        foreach (
            var relation in dsEntity.ChildItemsByType<DataStructureEntity>(
                DataStructureEntity.CategoryConst
            )
        )
        {
            RenderSelectRelation(
                recursionBuilder,
                relation,
                stopAtEntity,
                filter,
                replaceParameterTexts,
                skipStopAtEntity,
                includeFilter,
                numberOfJoins,
                includeAllRelations,
                dynamicParameters,
                parameterReferences
            );
        }

        if (beginType == JoinBeginType.Join)
        {
            sqlExpression.Append(relationBuilder);
            sqlExpression.Append(recursionBuilder);
        }
        else
        {
            sqlExpression.Append(recursionBuilder);
            sqlExpression.Append(relationBuilder);
        }
    }

    internal void RenderSelectRelationKey(
        StringBuilder sqlExpression,
        EntityRelationColumnPairItem key,
        DataStructureEntity parentEntity,
        DataStructureEntity relatedEntity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable paremeterReferences
    )
    {
        string parentField = RenderExpression(
            key.BaseEntityField,
            parentEntity,
            replaceParameterTexts,
            dynamicParameters,
            paremeterReferences
        );
        string relatedField = RenderExpression(
            key.RelatedEntityField,
            relatedEntity,
            replaceParameterTexts,
            dynamicParameters,
            paremeterReferences
        );
        sqlExpression.Append(filterRenderer.Equal(parentField, relatedField));
    }

    internal void RenderUpdateDeleteWherePart(
        StringBuilder sqlExpression,
        DataStructureEntity entity
    )
    {
        PrettyLine(sqlExpression);
        sqlExpression.Append("WHERE (");
        int i = 0;
        foreach (DataStructureColumn column in entity.Columns)
        {
            if (
                ShouldUpdateColumn(column, entity)
                && column.Field.DataType != OrigamDataType.Memo
                && column.Field.DataType != OrigamDataType.Blob
                && column.Field.DataType != OrigamDataType.Geography
                && !column.IsWriteOnly
                && (
                    entity.ConcurrencyHandling == DataStructureConcurrencyHandling.Standard
                    || column.Field.IsPrimaryKey
                )
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
                    OriginalParameterNameForNullComparison(column, false)
                );
                i++;
            }
        }
        sqlExpression.Append(")");
    }

    internal void RenderSelectWherePart(
        StringBuilder sqlExpression,
        DataStructureEntity entity,
        DataStructureFilterSet filterSet,
        Hashtable replaceParameterTexts,
        Hashtable parameters,
        Hashtable parameterReferences
    )
    {
        int i = 0;
        foreach (EntityFilter filter in Filters(filterSet, entity, parameters, false))
        {
            if (i > 0)
            {
                sqlExpression.Append(" AND");
            }
            else
            {
                sqlExpression.Append(" (");
            }
            PrettyIndent(sqlExpression);
            RenderFilter(
                sqlExpression,
                filter,
                entity,
                replaceParameterTexts,
                parameters,
                parameterReferences
            );

            i++;
        }

        if (i > 0)
        {
            sqlExpression.Append(")");
        }
    }

    internal List<EntityFilter> Filters(
        DataStructureFilterSet filterSet,
        DataStructureEntity entity,
        Hashtable parameters,
        bool ignoreImplicitFilters
    )
    {
        var result = new List<EntityFilter>();

        if (filterSet != null)
        {
            foreach (DataStructureFilterSetFilter filterPart in filterSet.ChildItems)
            {
                if (entity.PrimaryKey.Equals(filterPart.Entity.PrimaryKey))
                {
                    // skip filters with wrong role
                    IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
                    if (
                        filterPart.Roles == ""
                        || filterPart.Roles == null
                        || auth.Authorize(SecurityManager.CurrentPrincipal, filterPart.Roles)
                    )
                    {
                        // skip dynamic filter parts
                        IParameterService parameterService =
                            ServiceManager.Services.GetService(typeof(IParameterService))
                            as IParameterService;

                        string constant = null;

                        if (parameterService != null & filterPart.IgnoreFilterConstant != null)
                        {
                            constant = (string)
                                parameterService.GetParameterValue(
                                    filterPart.IgnoreFilterConstantId,
                                    OrigamDataType.String
                                );
                        }

                        bool skip = false;

                        if (filterPart.IgnoreFilterParameterName != null)
                        {
                            object paramValue = parameters[filterPart.IgnoreFilterParameterName];
                            var paramArray = paramValue as ArrayList;

                            if (
                                (paramValue == null || paramValue == DBNull.Value)
                                && constant == null
                            )
                            {
                                skip = true;
                            }
                            // only parameter name exists, no constant = we do filter when array is not empty
                            else if (
                                constant == null
                                && paramArray != null
                                && paramArray.Count == 0
                            )
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
                            else if (
                                (paramValue == null || paramValue == DBNull.Value)
                                || constant == null
                            )
                            {
                                skip = false;
                            }
                            else if (XmlTools.ConvertToString(paramValue) == constant)
                            {
                                skip = true;
                            }
                        }

                        // reverse condition if PassWhenParameterMatch = true
                        if (filterPart.PassWhenParameterMatch)
                        {
                            skip = !skip;
                        }

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
            foreach (
                var rowLevel in entity.EntityDefinition.ChildItemsByType<EntitySecurityFilterReference>(
                    EntitySecurityFilterReference.CategoryConst
                )
            )
            {
                if (!result.Contains(rowLevel.Filter))
                {
                    IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
                    System.Security.Principal.IPrincipal principal =
                        SecurityManager.CurrentPrincipal;

                    if (auth.Authorize(principal, rowLevel.Roles))
                    {
                        result.Add(rowLevel.Filter);
                    }
                }
            }
        }

        return result;
    }

    internal void RenderFilter(
        StringBuilder sqlExpression,
        EntityFilter filter,
        DataStructureEntity entity,
        Hashtable parameterReferences
    )
    {
        RenderFilter(sqlExpression, filter, entity, null, null, parameterReferences);
    }

    internal void RenderFilter(
        StringBuilder sqlExpression,
        EntityFilter filter,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        int i = 0;
        foreach (ISchemaItem filterItem in filter.ChildItems)
        {
            if (i > 0)
            {
                sqlExpression.Append(" AND ");
            }
            else
            {
                sqlExpression.Append(" (");
            }

            sqlExpression.Append(
                RenderExpression(
                    filterItem,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                )
            );

            i++;
        }

        if (i > 0)
        {
            sqlExpression.Append(")");
        }
    }

    #endregion

    #region Parameters
    /// <summary>
    /// Returns name of new value parameter in Insert/Update/Delete statement
    /// </summary>
    public string NewValueParameterName(DataStructureColumn column, bool declaration)
    {
        if (declaration)
        {
            return sqlRenderer.ParameterDeclarationChar + column.Name;
        }
        string result = sqlRenderer.ParameterReferenceChar + column.Name;

        if (column.DataType == OrigamDataType.Geography)
        {
            result = sqlRenderer.ConvertGeoFromTextClause(result);
        }

        return result;
    }

    /// <summary>
    /// Returns name of original value parameter in Update or Delete statement
    /// </summary>
    public string OriginalParameterName(DataStructureColumn column, bool declaration)
    {
        if (declaration)
        {
            return sqlRenderer.ParameterDeclarationChar + "Original_" + column.Name;
        }

        return sqlRenderer.ParameterReferenceChar + "Original_" + column.Name;
    }

    /// <summary>
    /// Returns name of original value parameter used for testing NULL value in Update or Delete statement
    /// </summary>
    public string OriginalParameterNameForNullComparison(
        DataStructureColumn column,
        bool declaration
    )
    {
        if (declaration)
        {
            return sqlRenderer.ParameterDeclarationChar + "OriginalIsNull_" + column.Name;
        }

        return sqlRenderer.ParameterReferenceChar + "OriginalIsNull_" + column.Name;
    }

    #endregion

    #region Expression rendering

    private string RenderExpression(
        ISchemaItem item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        return RenderExpression(
            item,
            entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences,
            false
        );
    }

    private string RenderExpression(
        ColumnRenderItem columnRenderItem,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        return RenderExpression(
            columnRenderItem.SchemaItem,
            columnRenderItem.Entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences,
            columnRenderItem.RenderSqlForDetachedFields
        );
    }

    private string RenderExpression(
        ISchemaItem item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences,
        bool renderSqlForDetachedFields
    )
    {
        if (item is TableMappingItem)
        {
            return RenderExpression(item as TableMappingItem);
        }

        if (item is EntityRelationItem)
        {
            return RenderExpression(item as EntityRelationItem);
        }
        else if (item is FieldMappingItem)
        {
            return RenderExpression(item as FieldMappingItem, entity);
        }
        else if (item is LookupField)
        {
            return RenderLookupColumnExpression(
                entity.RootItem as DataStructure,
                entity,
                (item as LookupField).Field,
                (item as LookupField).Lookup,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else if (item is EntityColumnReference)
        {
            return RenderExpression(
                item as EntityColumnReference,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else if (item is FunctionCall)
        {
            return RenderExpression(
                item as FunctionCall,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else if (item is ParameterReference)
        {
            return RenderExpression(
                item as ParameterReference,
                entity,
                replaceParameterTexts,
                null,
                parameterReferences
            );
        }
        else if (item is DataConstantReference)
        {
            return RenderExpression(item as DataConstantReference);
        }
        else if (item is EntityFilterReference)
        {
            return RenderExpression(
                item as EntityFilterReference,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else if (item is EntityFilterLookupReference)
        {
            return RenderExpression(
                item as EntityFilterLookupReference,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else if (item is DetachedField detachedField)
        {
            return renderSqlForDetachedFields
                ? RenderSqlExpression(entity, detachedField, parameterReferences)
                : "";
        }
        else if (item is AggregatedColumn)
        {
            return RenderExpression(
                item as AggregatedColumn,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }
        else
        {
            throw new NotImplementedException(
                ResourceUtils.GetString("TypeNotSupported", item.GetType().ToString())
            );
        }
    }

    public string RenderSqlExpression(
        DataStructureEntity entity,
        DetachedField detachedField,
        Hashtable parameterReferences
    )
    {
        if (detachedField.ArrayRelation == null)
        {
            return "";
        }

        DataStructureEntity relation = entity
            .ChildItems.OfType<DataStructureEntity>()
            .FirstOrDefault(child =>
                child.Entity.PrimaryKey.Equals(detachedField.ArrayRelation.PrimaryKey)
            );

        var columnRenderItem = new ColumnRenderItem
        {
            SchemaItem = detachedField.ArrayValueField,
            Entity = relation,
            RenderSqlForDetachedFields = false,
        };
        string columnToAggregate = RenderExpression(columnRenderItem, null, null, null);
        var sqlExpression = new StringBuilder(
            $"(SELECT STRING_AGG({sqlRenderer.Text(RenderExpression(columnRenderItem, null, null, null))} ,"
                + sqlRenderer.Char(1)
                + " ) "
        );
        RenderSelectFromClause(sqlExpression, relation);
        RenderSelectRelation(
            sqlExpression: sqlExpression,
            dsEntity: relation,
            stopAtEntity: relation,
            filter: null,
            replaceParameterTexts: null,
            skipStopAtEntity: true,
            includeFilter: true,
            numberOfJoins: 0,
            includeAllRelations: false,
            dynamicParameters: null,
            parameterReferences: parameterReferences
        );
        sqlExpression.Append(")");
        return sqlExpression.ToString();
    }

    internal string AggregationHelper(
        AggregatedColumn topLevelItem,
        DataStructureEntity topLevelEntity,
        AggregatedColumn item,
        Hashtable replaceParameterTexts,
        int level,
        StringBuilder joins,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        AggregatedColumn agg2 = item.Field as AggregatedColumn;

        DataStructureEntity aggregationVirtualEntity = new DataStructureEntity();
        aggregationVirtualEntity.PersistenceProvider = topLevelEntity.PersistenceProvider;
        aggregationVirtualEntity.ParentItem = topLevelEntity.RootItem;
        aggregationVirtualEntity.Name = "aggregation" + level;

        if (agg2 != null)
        {
            // allow nested
            //   sum(sum()) / avg(avg()) /  min(min()) / max(max())
            //   It will mean e.g. avg(table join second_table)
            //   sum(count()) - will mean count(table join second_table)
            //   count(count()) is not allowed
            if (
                agg2.AggregationType != item.AggregationType
                && !(
                    agg2.AggregationType == AggregationType.Count
                    && item.AggregationType == AggregationType.Sum
                )
            )
            {
                throw new ArgumentOutOfRangeException(
                    "AggregationType",
                    agg2.AggregationType,
                    "Nested aggregations must be of the same type. Path: " + agg2.Path
                );
            }
            if (
                agg2.AggregationType == AggregationType.Count
                && item.AggregationType == AggregationType.Count
            )
            {
                throw new ArgumentOutOfRangeException(
                    "AggregationType",
                    agg2.AggregationType,
                    "Count(Count()) nested aggregation is not allowed. "
                        + "Use Sum(Count()) for a total nested count. Path: "
                        + agg2.Path
                );
            }

            // nested aggregated expression
            DataStructureEntity aggregationVirtualEntity2 = new DataStructureEntity();
            aggregationVirtualEntity2.PersistenceProvider = topLevelEntity.PersistenceProvider;
            aggregationVirtualEntity2.ParentItem = topLevelEntity.RootItem;
            aggregationVirtualEntity2.Name = "aggregation" + (level + 1);

            joins.AppendFormat(
                " INNER JOIN {0} AS {1} ON ",
                RenderExpression(
                    agg2.Relation.AssociatedEntity,
                    null,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                ),
                sqlRenderer.NameLeftBracket
                    + aggregationVirtualEntity2.Name
                    + sqlRenderer.NameRightBracket
            );

            int i = 0;
            foreach (ISchemaItem relationItem in agg2.Relation.ChildItems)
            {
                EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
                EntityRelationFilter filter = relationItem as EntityRelationFilter;

                if (i > 0)
                {
                    joins.Append(" AND ");
                }

                if (key != null)
                {
                    RenderSelectRelationKey(
                        joins,
                        key,
                        aggregationVirtualEntity,
                        aggregationVirtualEntity2,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                }

                if (filter != null)
                {
                    RenderFilter(
                        joins,
                        filter.Filter,
                        aggregationVirtualEntity2,
                        parameterReferences
                    );
                }

                i++;
            }

            // recursion - get nested expressions
            return AggregationHelper(
                topLevelItem,
                topLevelEntity,
                agg2,
                replaceParameterTexts,
                level + 1,
                joins,
                dynamicParameters,
                parameterReferences
            );
        }
        else
        {
            // final - non-aggregated expression
            StringBuilder result = new StringBuilder();
            DataStructureEntity topLevelAggregationVirtualEntity = new DataStructureEntity();
            topLevelAggregationVirtualEntity.PersistenceProvider =
                topLevelEntity.PersistenceProvider;
            topLevelAggregationVirtualEntity.Name = "aggregation1";
            topLevelAggregationVirtualEntity.ParentItem = topLevelEntity.RootItem;
            string expression = RenderExpression(
                item.Field,
                aggregationVirtualEntity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
            expression = FixAggregationDataType(item.DataType, expression);
            string aggregationPart = string.Format(
                "{0}({1})",
                GetAggregationString(item.AggregationType),
                expression
            );
            aggregationPart = FixSumAggregation(item.AggregationType, aggregationPart);
            result.AppendFormat(
                "(SELECT {0} FROM {1} AS "
                    + sqlRenderer.NameLeftBracket
                    + "aggregation1"
                    + sqlRenderer.NameRightBracket
                    + " {2} WHERE ",
                aggregationPart,
                RenderExpression(
                    topLevelItem.Relation.AssociatedEntity,
                    null,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                ),
                joins
            );

            int i = 0;
            foreach (ISchemaItem relationItem in topLevelItem.Relation.ChildItems)
            {
                EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
                EntityRelationFilter filter = relationItem as EntityRelationFilter;

                if (i > 0)
                {
                    result.Append(" AND ");
                }

                if (key != null)
                {
                    RenderSelectRelationKey(
                        result,
                        key,
                        topLevelEntity,
                        topLevelAggregationVirtualEntity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                }

                if (filter != null)
                {
                    RenderFilter(
                        result,
                        filter.Filter,
                        topLevelAggregationVirtualEntity,
                        parameterReferences
                    );
                }

                i++;
            }

            result.Append(")");

            return result.ToString();
        }
    }

    internal string RenderExpression(
        EntityFilterLookupReference lookupReference,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        DataServiceDataLookup lookup = lookupReference.Lookup as DataServiceDataLookup;

        if (lookup == null)
        {
            throw new ArgumentOutOfRangeException(
                "lookup",
                lookupReference.Lookup,
                ResourceUtils.GetString("LookupTypeUnsupportedException")
            );
        }

        if (lookup.ListMethod != null && !(lookup.ListMethod is DataStructureFilterSet))
        {
            throw new ArgumentOutOfRangeException(
                "ListMethod",
                lookup.ListMethod,
                ResourceUtils.GetString("LookupListMethodTypeUnsupportedException")
            );
        }

        DataStructureEntity lookupEntity = (
            lookup.ListDataStructure.Entities[0] as DataStructureEntity
        );
        lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
        lookupEntity.Name = "lookup" + lookupEntity.Name;

        Hashtable replaceTexts = new Hashtable();

        foreach (ISchemaItem paramMapping in lookupReference.ChildItems)
        {
            replaceTexts.Add(
                paramMapping.Name,
                RenderExpression(
                    paramMapping,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                )
            );
        }

        string resultExpression =
            "("
            + SelectSql(
                selectParameters: new SelectParameters
                {
                    DataStructure = lookup.ListDataStructure,
                    Entity = lookupEntity,
                    Filter = lookup.ListMethod as DataStructureFilterSet,
                    SortSet = null,
                    ColumnsInfo = new ColumnsInfo(lookup.ListDisplayMember),
                    Paging = false,
                    Parameters = dynamicParameters,
                },
                replaceParameterTexts: replaceTexts,
                selectParameterReferences: parameterReferences,
                restrictScalarToTop1: false,
                isInRecursion: true,
                forceDatabaseCalculation: true
            )
            + ")";

        return resultExpression;
    }

    internal string RenderExpression(
        AggregatedColumn item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        StringBuilder joins = new StringBuilder();

        return AggregationHelper(
            item,
            entity,
            item,
            replaceParameterTexts,
            1,
            joins,
            dynamicParameters,
            parameterReferences
        );
    }

    internal string RenderExpression(TableMappingItem item)
    {
        return sqlRenderer.NameLeftBracket + item.MappedObjectName + sqlRenderer.NameRightBracket;
    }

    private string RenderExpression(EntityRelationItem item)
    {
        return RenderExpression(item.RelatedEntity, null, null, null, null);
    }

    internal string RenderExpression(
        FieldMappingItem fieldMappingItem,
        DataStructureEntity dataStructureEntity
    )
    {
        bool localize =
            dataStructureEntity?.RootItem is DataStructure dataStructure
            && dataStructure.IsLocalized;
        TableMappingItem tableMappingItem = null;
        FieldMappingItem localizedItem = null;
        if (localize)
        {
            tableMappingItem = dataStructureEntity.Entity as TableMappingItem;
            if (tableMappingItem == null)
            {
                // it could be a relation
                if (dataStructureEntity.Entity is EntityRelationItem entityRelationItem)
                {
                    tableMappingItem = entityRelationItem.RelatedEntity as TableMappingItem;
                }
            }
            localizedItem = fieldMappingItem.GetLocalizationField(tableMappingItem);
        }
        string nonLocalizedResult =
            sqlRenderer.NameLeftBracket
            + fieldMappingItem.MappedColumnName
            + sqlRenderer.NameRightBracket;
        if (dataStructureEntity != null)
        {
            nonLocalizedResult =
                sqlRenderer.NameLeftBracket
                + dataStructureEntity.Name
                + sqlRenderer.NameRightBracket
                + "."
                + nonLocalizedResult;
        }
        if (!localize || localizedItem == null)
        {
            return nonLocalizedResult;
        }
        var localizationTable = FieldMappingItem.GetLocalizationTable(tableMappingItem);
        var localizationEntity = dataStructureEntity
            .ChildItemsByType<DataStructureEntity>(DataStructureEntity.CategoryConst)
            .FirstOrDefault(entity => localizationTable.Id == entity.EntityDefinition.Id);
        if (localizationEntity == null)
        {
            throw new Exception(
                $@"Localization entity for {localizationTable.Name} 
                    not found among child entities of {dataStructureEntity.Name}"
            );
        }
        return sqlRenderer.IsNull()
            + "("
            + sqlRenderer.NameLeftBracket
            + localizationEntity.Name
            + sqlRenderer.NameRightBracket
            + "."
            + sqlRenderer.NameLeftBracket
            + fieldMappingItem.GetLocalizationField(tableMappingItem).MappedColumnName
            + sqlRenderer.NameRightBracket
            + ", "
            + nonLocalizedResult
            + ")";
    }

    internal string RenderExpression(
        ParameterReference item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        string parameterName,
        Hashtable parameterReferences
    )
    {
        if (parameterName == null)
        {
            parameterName = GetParameterName(entity, item);
        }

        if (replaceParameterTexts != null && replaceParameterTexts.ContainsKey(parameterName))
        {
            return (string)replaceParameterTexts[parameterName];
        }
        string name = sqlRenderer.ParameterReferenceChar + parameterName;
        string declarationName = sqlRenderer.ParameterDeclarationChar + parameterName;
        if (parameterReferences != null)
        {
            if (!parameterReferences.Contains(declarationName))
            {
                parameterReferences.Add(declarationName, item);
            }
        }

        return name;
    }

    private string GetParameterName(DataStructureEntity entity, ParameterReference item)
    {
        return entity.Name + "_" + item.Parameter.Name;
    }

    internal string RenderExpression(
        EntityColumnReference item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        if (item.Field == null)
        {
            throw new Exception("Column not specified for " + item.Path);
        }

        return RenderExpression(
            item.Field,
            entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );
    }

    private string RenderExpression(
        EntityFilterReference item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        if (
            item.Roles == ""
            || item.Roles == null
            || auth.Authorize(SecurityManager.CurrentPrincipal, item.Roles)
        )
        {
            StringBuilder builder = new StringBuilder();

            RenderFilter(
                builder,
                item.Filter,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );

            return builder.ToString();
        }

        return "";
    }

    internal string RenderConstant(DataConstant constant, bool userDefinedParameters)
    {
        //needs set
        if (constant.Name == "null")
        {
            return "NULL";
        }

        IParameterService parameterService =
            ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

        object value =
            userDefinedParameters && parameterService != null
                ? parameterService.GetParameterValue(constant.Id)
                : constant.Value;

        return sqlValueFormatter.Format(constant.DataType, value);
    }

    private string RenderExpression(DataConstantReference item)
    {
        return RenderConstant(item.DataConstant, UserDefinedParameters);
    }

    internal string RenderSortDirection(DataStructureColumnSortDirection direction)
    {
        switch (direction)
        {
            case DataStructureColumnSortDirection.Ascending:
                return "ASC";

            case DataStructureColumnSortDirection.Descending:
                return "DESC";

            default:
                throw new ArgumentOutOfRangeException(
                    "direction",
                    direction,
                    ResourceUtils.GetString("UnknownSortDirection")
                );
        }
    }

    private string RenderExpression(
        FunctionCall item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        if (item.Function.FunctionType == OrigamFunctionType.Database)
        {
            return RenderDatabaseFunction(
                item,
                entity,
                replaceParameterTexts,
                dynamicParameters,
                parameterReferences
            );
        }

        return RenderBuiltinFunction(
            item,
            entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );
    }

    internal string RenderDatabaseFunction(
        FunctionCall item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        string result = "";

        switch (item.Function.Name)
        {
            case "DaysToAnniversary":
            {
                if (item.ChildItems[0].ChildItems.Count == 0)
                {
                    throw new Exception(
                        ResourceUtils.GetString("ErrorExpressionNotSet", item.Path)
                    );
                }
                string date = RenderExpression(
                    item.ChildItems[0].ChildItems[0],
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                //return "DATEDIFF(dd, DATEADD(yy, -(DATEPART(yy,GETDATE())-1900),GETDATE()),"
                //            + "DATEADD(yy, -(DATEPART(yy,"
                //            + date
                //            + ")-1900),"
                //            + date
                //            + "))";
                result = sqlRenderer.DateDiff(
                    DateTypeSql.Day,
                    sqlRenderer.DateAdd(
                        DateTypeSql.Year,
                        "-(" + sqlRenderer.DatePart("year", sqlRenderer.Now()) + "-1900)",
                        sqlRenderer.Now()
                    ),
                    sqlRenderer.DateAdd(
                        DateTypeSql.Year,
                        "-(" + sqlRenderer.DatePart("year", date) + "-1900)",
                        date
                    )
                );
                break;
            }

            case "Exists":
            {
                if (item.ChildItems[0].ChildItems.Count == 0)
                {
                    throw new Exception(ResourceUtils.GetString("ErrorLookupNotSet", item.Path));
                }

                result =
                    "EXISTS ("
                    + RenderExpression(
                        item.ChildItems[0].ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ")";
                break;
            }

            case "Month":
            case "Year":
            case "Day":
            {
                if (item.ChildItems[0].ChildItems.Count == 0)
                {
                    throw new Exception(
                        ResourceUtils.GetString("ErrorExpressionNotSet", item.Path)
                    );
                }
                result = sqlRenderer.DatePart(
                    item.Function.Name,
                    RenderExpression(
                        item.ChildItems[0].ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "Hour":
            case "Minute":
            case "Second":
            {
                if (item.ChildItems[0].ChildItems.Count == 0)
                {
                    throw new Exception(
                        ResourceUtils.GetString("ErrorExpressionNotSet", item.Path)
                    );
                }
                result = sqlRenderer.DatePart(
                    item.Function.Name,
                    RenderExpression(
                        item.ChildItems[0].ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "AddDays":
            {
                {
                    ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
                    ISchemaItem daysArg = item.GetChildByName("Days").ChildItems[0];

                    result = sqlRenderer.DateAdd(
                        DateTypeSql.Day,
                        RenderExpression(
                            daysArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        ),
                        RenderExpression(
                            dateArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        )
                    );
                }
                break;
            }

            case "AddMinutes":
            {
                {
                    ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
                    ISchemaItem countArg = item.GetChildByName("Minutes").ChildItems[0];
                    result = sqlRenderer.DateAdd(
                        DateTypeSql.Minute,
                        RenderExpression(
                            countArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        ),
                        RenderExpression(
                            dateArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        )
                    );
                }
                break;
            }

            case "AddSeconds":
            {
                {
                    ISchemaItem dateArg = item.GetChildByName("Date").ChildItems[0];
                    ISchemaItem countArg = item.GetChildByName("Seconds").ChildItems[0];
                    result = sqlRenderer.DateAdd(
                        DateTypeSql.Second,
                        RenderExpression(
                            countArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        ),
                        RenderExpression(
                            dateArg,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        )
                    );
                }
                break;
            }

            case "FullTextContains":
            case "FullText":
            {
                ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
                ISchemaItem languageArg = item.GetChildByName("Language");
                ISchemaItem fieldsArg = item.GetChildByName("Fields");

                string columnsForSeach = "";
                if (fieldsArg.ChildItems.Count == 0)
                {
                    columnsForSeach +=
                        sqlRenderer.NameLeftBracket
                        + entity.Name
                        + sqlRenderer.NameRightBracket
                        + ".*";
                }
                else
                {
                    if (fieldsArg.ChildItems.Count > 1)
                    {
                        columnsForSeach += "(";
                    }

                    int fieldNum = 0;
                    foreach (ISchemaItem field in fieldsArg.ChildItems)
                    {
                        if (fieldNum > 0)
                        {
                            result += ", ";
                        }

                        columnsForSeach += RenderExpression(
                            field,
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        );
                        fieldNum++;
                    }

                    if (fieldsArg.ChildItems.Count > 1)
                    {
                        result += ")";
                    }
                }
                string freetext_string = RenderExpression(
                    expressionArg,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                string languageForFullText = "";
                if (languageArg.ChildItems.Count > 0)
                {
                    languageForFullText += RenderExpression(
                        languageArg.ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                }
                if (item.Function.Name == "FullText")
                {
                    result = sqlRenderer.FreeText(
                        columnsForSeach,
                        freetext_string,
                        languageForFullText
                    );
                }

                if (item.Function.Name == "FullTextContains")
                {
                    result = sqlRenderer.Contains(
                        columnsForSeach,
                        freetext_string,
                        languageForFullText
                    );
                }

                break;
            }

            case "Soundex":
            {
                result =
                    "SOUNDEX("
                    + RenderExpression(
                        item.GetChildByName("Text").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ")";
                break;
            }

            case "Distance":
            {
                ISchemaItem param1 = item.GetChildByName("Param1").ChildItems[0];
                ISchemaItem param2 = item.GetChildByName("Param2").ChildItems[0];

                result = sqlRenderer.STDistance(
                    RenderExpression(
                        param1,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    ),
                    RenderExpression(
                        param2,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "Latitude":
            {
                result = sqlRenderer.LatLon(
                    geoLatLonSql.Lat,
                    RenderExpression(
                        item.GetChildByName("Point").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "Longitude":
            {
                result = sqlRenderer.LatLon(
                    geoLatLonSql.Lon,
                    RenderExpression(
                        item.GetChildByName("Point").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "ToDate":
            {
                result =
                    " CAST ("
                    + RenderExpression(
                        item.GetChildByName("argument").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + " AS DATE )";
                break;
            }

            case "Round":
            {
                result =
                    "ROUND("
                    + RenderExpression(
                        item.GetChildByName("Expression").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ", "
                    + RenderExpression(
                        item.GetChildByName("Precision").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ")";
                break;
            }

            case "Abs":
            {
                result = string.Format(
                    "ABS({0})",
                    RenderExpression(
                        item.GetChildByName("Expression").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "DateDiffMinutes":
            {
                result = sqlRenderer.DateDiff(
                    DateTypeSql.Minute,
                    RenderExpression(
                        item.GetChildByName("DateFrom").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    ),
                    RenderExpression(
                        item.GetChildByName("DateTo").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            default:
            {
                result = sqlRenderer.FunctionPrefix() + item.Function.Name + "(";

                var sortedParams = item.ChildItems.CastToList<FunctionCallParameter>();
                sortedParams.Sort();

                int i = 0;
                foreach (FunctionCallParameter param in sortedParams)
                {
                    if (i > 0)
                    {
                        result += ", ";
                    }

                    if (param.ChildItems.Count != 1)
                    {
                        throw new ArgumentOutOfRangeException(
                            "Count",
                            param.ChildItems.Count,
                            "Argument number must be 1"
                        );
                    }

                    result += RenderExpression(
                        param.ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );

                    i++;
                }

                result += ")";
                break;
            }
        }

        return result;
    }

    internal string RenderBuiltinFunction(
        FunctionCall item,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        string result = "";

        switch (item.Function.Name)
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
            {
                string leftValue = GetItemByFunctionParameter(
                    item,
                    "Left",
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                string rightValue = GetItemByFunctionParameter(
                    item,
                    "Right",
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                result = filterRenderer.BinaryOperator(leftValue, rightValue, item.Function.Name);
                break;
            }

            case "Not":
            {
                string argument = GetItemByFunctionParameter(
                    item,
                    "Argument",
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                result = filterRenderer.Not(argument);
                break;
            }

            case "Concat":
            {
                ISchemaItem concatArg = item.GetChildByName("Strings");
                var concatStrings = concatArg.ChildItems.ToList<ISchemaItem>();
                if (concatStrings.Count < 2)
                {
                    throw new ArgumentOutOfRangeException(
                        "Strings",
                        null,
                        "There have to be at least 2 strings to concatenate."
                    );
                }

                concatStrings.Sort();
                result = RenderConcat(
                    concatStrings,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                break;
            }

            case "LogicalOr":
            case "LogicalAnd":
            {
                var arguments = GetItemListByFunctionParameter(
                    item,
                    "Arguments",
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                );
                result = filterRenderer.LogicalAndOr(item.Function.Name, arguments);
                break;
            }

            case "Space":
            {
                ISchemaItem spacesArg = item.GetChildByName("NumberOfSpaces");

                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                decimal numberOfSpaces = Convert.ToDecimal(
                    RenderExpression(
                        spacesArg.ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    ),
                    ci.NumberFormat
                );

                for (int i = 0; i < numberOfSpaces; i++)
                {
                    result += " ";
                }

                result = sqlValueFormatter.RenderString(result);
                break;
            }

            case "Substring":
            {
                result =
                    " SUBSTRING("
                    + RenderExpression(
                        item.GetChildByName("Expression").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ", "
                    + RenderExpression(
                        item.GetChildByName("Start").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ", "
                    + RenderExpression(
                        item.GetChildByName("Length").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ")";
                break;
            }

            case "Condition":
            {
                result =
                    "(CASE WHEN "
                    + RenderExpression(
                        item.GetChildByName("If").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + " THEN "
                    + (
                        item.GetChildByName("Then").ChildItems.Count == 0
                            ? "NULL"
                            : RenderExpression(
                                item.GetChildByName("Then").ChildItems[0],
                                entity,
                                replaceParameterTexts,
                                dynamicParameters,
                                parameterReferences
                            )
                    )
                    + " ELSE "
                    + (
                        item.GetChildByName("Else").ChildItems.Count == 0
                            ? "NULL"
                            : RenderExpression(
                                item.GetChildByName("Else").ChildItems[0],
                                entity,
                                replaceParameterTexts,
                                dynamicParameters,
                                parameterReferences
                            )
                    )
                    + " END)";
                break;
            }

            case "Length":
            {
                result = sqlRenderer.Length(
                    RenderExpression(
                        item.GetChildByName("Text").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                break;
            }

            case "ConvertDateToString":
            {
                result =
                    "CONVERT( "
                    + sqlRenderer.VarcharSql()
                    + "("
                    + item.DataLength.ToString()
                    + "), "
                    + RenderExpression(
                        item.GetChildByName("Expression").ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ", 104)";
                break;
            }

            case "In":
            {
                ISchemaItem leftArg = item.GetChildByName("FilterExpression");
                ISchemaItem listArg = item.GetChildByName("List");
                ISchemaItemCollection listExpressions = listArg.ChildItems;

                if (listExpressions.Count < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        "List",
                        null,
                        ResourceUtils.GetString("ErrorNoParamIN")
                    );
                }

                if (
                    listExpressions.Count == 1
                    && listExpressions[0] is ParameterReference
                    && (listExpressions[0] as ParameterReference).Parameter.DataType
                        == OrigamDataType.Array
                )
                {
                    result = sqlRenderer.Array(
                        RenderExpression(
                            leftArg.ChildItems[0],
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        ),
                        RenderExpression(
                            listExpressions[0],
                            entity,
                            replaceParameterTexts,
                            dynamicParameters,
                            parameterReferences
                        )
                    );
                }
                else
                {
                    // list of parameters
                    string leftOperand = RenderExpression(
                        leftArg.ChildItems[0],
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                    IEnumerable<string> options = listExpressions
                        .Cast<ISchemaItem>()
                        .Select(listExpression =>
                            RenderExpression(
                                listExpression,
                                entity,
                                replaceParameterTexts,
                                dynamicParameters,
                                parameterReferences
                            )
                        );
                    result = filterRenderer.In(leftOperand, options);
                }
                break;
            }

            case "IsNull":
            {
                ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
                ISchemaItem replacementArg = item.GetChildByName("ReplacementValue").ChildItems[0];
                result =
                    sqlRenderer.IsNull()
                    + "("
                    + RenderExpression(
                        expressionArg,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ", "
                    + RenderExpression(
                        replacementArg,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + ")";
                break;
            }

            case "Between":
            {
                ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
                ISchemaItem leftArg = item.GetChildByName("Left").ChildItems[0];
                ISchemaItem rightArg = item.GetChildByName("Right").ChildItems[0];

                result =
                    RenderExpression(
                        expressionArg,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + " BETWEEN "
                    + RenderExpression(
                        leftArg,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                    + " AND "
                    + RenderExpression(
                        rightArg,
                        entity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    "Function.Name",
                    item.Function.Name,
                    ResourceUtils.GetString("UnknownFunction")
                );
        }

        return result;
    }

    internal string GetItemByFunctionParameter(
        FunctionCall item,
        string parameterName,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        ISchemaItem param = GetFunctionParameter(item, parameterName);
        ISchemaItem value = null;
        if (param.ChildItems.Count > 1)
        {
            throw new ArgumentOutOfRangeException(
                "parameterName",
                parameterName,
                "Only 1 argument can be present to " + item.Path
            );
        }
        else if (param.HasChildItems)
        {
            value = param.ChildItems[0];
        }
        if (value == null)
        {
            return null;
        }
        return RenderExpression(
            value,
            entity,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );
    }

    internal IList<string> GetItemListByFunctionParameter(
        FunctionCall item,
        string parameterName,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        ISchemaItem param = GetFunctionParameter(item, parameterName);
        var result = new List<string>();
        foreach (var child in param.ChildItems)
        {
            result.Add(
                RenderExpression(
                    child,
                    entity,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                )
            );
        }
        return result;
    }

    private static ISchemaItem GetFunctionParameter(FunctionCall item, string parameterName)
    {
        ISchemaItem param = item.GetChildByName(parameterName);
        if (param == null)
        {
            throw new ArgumentOutOfRangeException(
                "parameterName",
                parameterName,
                "Parameter not found for function " + item.Path
            );
        }
        return param;
    }

    internal string RenderConcat(
        List<ISchemaItem> concatSchemaItems,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        List<ColumnRenderItem> concatSchemaItemList = new List<ColumnRenderItem>();

        foreach (var o in concatSchemaItems)
        {
            concatSchemaItemList.Add(new ColumnRenderItem { SchemaItem = o, Entity = entity });
        }
        return RenderConcat(
            concatSchemaItemList,
            null,
            replaceParameterTexts,
            dynamicParameters,
            parameterReferences
        );
    }

    internal string RenderConcat(
        List<ColumnRenderItem> columnRenderItems,
        string separator,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        int i = 0;
        StringBuilder concatBuilder = new StringBuilder();
        bool shouldTrimSeparator = !string.IsNullOrEmpty(separator) && columnRenderItems.Count > 1;
        if (shouldTrimSeparator)
        {
            concatBuilder.Append($"TRIM( {separator} FROM ");
        }
        foreach (ColumnRenderItem columnRenderItem in columnRenderItems)
        {
            if (i > 0)
            {
                concatBuilder.Append(" " + sqlRenderer.StringConcatenationChar + " ");
                if (separator != null)
                {
                    concatBuilder.Append(separator);
                    concatBuilder.Append(" " + sqlRenderer.StringConcatenationChar + " ");
                }
            }
            string sqlText = sqlRenderer.Text(
                RenderExpression(
                    columnRenderItem,
                    replaceParameterTexts,
                    dynamicParameters,
                    parameterReferences
                )
            );
            string nonNullExpression = $"{sqlRenderer.IsNull()} ({sqlText}, '')";
            concatBuilder.Append(nonNullExpression);
            i++;
        }
        if (shouldTrimSeparator)
        {
            concatBuilder.Append(")");
        }
        return concatBuilder.ToString();
    }

    internal string RenderConcat(
        SelectParameters selectParameters,
        bool isInRecursion,
        bool forceDatabaseCalculation,
        string separator,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences,
        FilterCommandParser filterCommandParser,
        OrderByCommandParser orderByCommandParser
    )
    {
        int i = 0;
        StringBuilder concatBuilder = new StringBuilder();
        bool shouldTrimSeparator =
            !string.IsNullOrEmpty(separator) && selectParameters.ColumnsInfo.ColumnNames.Count > 1;
        if (shouldTrimSeparator)
        {
            concatBuilder.Append($"TRIM( {separator} FROM ");
        }
        SortedList<int, SortOrder> order = new SortedList<int, SortOrder>();
        foreach (string columnName in selectParameters.ColumnsInfo.ColumnNames)
        {
            DataStructureColumn column = selectParameters.Entity.Column(columnName);
            if (i > 0)
            {
                concatBuilder.Append(" " + sqlRenderer.StringConcatenationChar + " ");
                if (separator != null)
                {
                    concatBuilder.Append(separator);
                    concatBuilder.Append(" " + sqlRenderer.StringConcatenationChar + " ");
                }
            }
            string groupByExpression = "";
            bool groupByNeeded = false;
            LookupOrderingInfo customOrderingInfo = LookupOrderingInfo.TryCreate(
                selectParameters.CustomOrderings.Orderings,
                column.Name
            );
            ColumnRenderData columnRenderData = RenderDataStructureColumn(
                selectParameters.DataStructure,
                selectParameters.Entity,
                replaceParameterTexts,
                dynamicParameters,
                selectParameters.SortSet,
                parameterReferences,
                isInRecursion,
                forceDatabaseCalculation,
                ref groupByExpression,
                order,
                ref groupByNeeded,
                selectParameters.ColumnsInfo ?? ColumnsInfo.Empty,
                column,
                customOrderingInfo,
                filterCommandParser,
                orderByCommandParser,
                selectParameters.RowOffset
            );
            if (column.DataType == OrigamDataType.Date)
            {
                string nonNullExpression =
                    $"{sqlRenderer.IsNull()} ({sqlRenderer.Format(columnRenderData.Expression, Thread.CurrentThread.CurrentCulture.Name)}, '')";
                concatBuilder.Append(nonNullExpression);
            }
            else
            {
                string nonNullExpression =
                    $"{sqlRenderer.IsNull()} ({columnRenderData.Expression}, '')";
                concatBuilder.Append(nonNullExpression);
            }
            i++;
        }
        if (shouldTrimSeparator)
        {
            concatBuilder.Append(")");
        }
        return concatBuilder.ToString();
    }

    internal string PostProcessCustomCommandParserWhereClauseSegment(
        string input,
        DataStructureEntity entity,
        Hashtable replaceParameterTexts,
        Hashtable dynamicParameters,
        Hashtable parameterReferences
    )
    {
        var output = input;
        foreach (var column in entity.Columns)
        {
            if ((column.DataType == OrigamDataType.Array) && input.Contains(column.Name))
            {
                var stringBuilder = new StringBuilder();
                var regex = new Regex($"\0.*{column.Name}.*\0");
                var placeholder = regex.Match(input, 0).Value;
                if (placeholder.Contains("NOT"))
                {
                    placeholder = placeholder.Replace("NOT", "");
                    stringBuilder.Append("NOT ");
                }
                var arrayRelation = (column.Field as DetachedField).ArrayRelation;
                stringBuilder.Append("EXISTS(SELECT * FROM ");
                stringBuilder.Append(RenderExpression(arrayRelation as EntityRelationItem));
                stringBuilder.Append(" WHERE");
                DataStructureEntity arrayEntity = null;
                foreach (
                    var relatedEntity in entity.ChildItemsByType<DataStructureEntity>(
                        DataStructureEntity.CategoryConst
                    )
                )
                {
                    if (relatedEntity.EntityDefinition.Id == arrayRelation.AssociatedEntity.Id)
                    {
                        arrayEntity = relatedEntity;
                        break;
                    }
                }
                if (arrayEntity == null)
                {
                    throw new Exception(
                        $@"Array entity {arrayRelation.AssociatedEntity.Name} 
                            not found among child entities of {entity.Name}"
                    );
                }
                var andNeeded = false;
                foreach (
                    var pairItem in arrayRelation.ChildItemsByType<EntityRelationColumnPairItem>(
                        EntityRelationColumnPairItem.CategoryConst
                    )
                )
                {
                    stringBuilder.Append(" ");
                    if (andNeeded)
                    {
                        stringBuilder.Append("AND ");
                    }
                    RenderSelectRelationKey(
                        stringBuilder,
                        pairItem,
                        entity,
                        arrayEntity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    );
                    andNeeded = true;
                }
                stringBuilder.Append(" ");
                if (andNeeded)
                {
                    stringBuilder.Append("AND ");
                }
                stringBuilder.Append(
                    RenderExpression(
                        (column.Field as DetachedField).ArrayValueField,
                        arrayEntity,
                        replaceParameterTexts,
                        dynamicParameters,
                        parameterReferences
                    )
                );
                var placeholderElements = placeholder.Replace("\0", "").Split(" ");
                for (var i = 1; i < placeholderElements.Length; i++)
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(placeholderElements[i]);
                }
                stringBuilder.Append(")");
                output = regex.Replace(output, stringBuilder.ToString());
            }
        }
        return output;
    }

    protected abstract string RenderUpsertKey(string paramName, string fieldName);
    #endregion

    #region Operators
    internal string GetAggregationString(AggregationType type)
    {
        switch (type)
        {
            case AggregationType.Sum:
                return "SUM";

            case AggregationType.Count:
                return sqlRenderer.CountAggregate();
            case AggregationType.Average:
                return "AVG";

            case AggregationType.Minimum:
                return "MIN";

            case AggregationType.Maximum:
                return "MAX";

            default:
                throw new ArgumentOutOfRangeException(
                    "type",
                    type,
                    ResourceUtils.GetString("UnsupportedAggreg")
                );
        }
    }

    #endregion
    #region Conversions

    public abstract OrigamDataType ToOrigamDataType(string ddlType);

    public string DdlDataType(
        OrigamDataType columnType,
        int dataLength,
        DatabaseDataType dbDataType
    )
    {
        switch (columnType)
        {
            case OrigamDataType.String:
                return DdlDataType(columnType, dbDataType) + "(" + dataLength + ")";

            case OrigamDataType.Xml:
                return DdlDataType(columnType, dbDataType);

            case OrigamDataType.Float:
                return DdlDataType(columnType, dbDataType) + "(28,10)";

            default:
                return DdlDataType(columnType, dbDataType);
        }
    }

    #endregion

    #region ICloneable Members

    public abstract object Clone();

    #endregion

    #region IDisposable Members

    public void Dispose() { }

    #endregion
}

internal class ColumnRenderItem
{
    public bool RenderSqlForDetachedFields { get; set; }
    public ISchemaItem SchemaItem { get; set; }
    public DataStructureColumn Column { get; set; }
    public DataStructureEntity Entity { get; set; }
}

internal class LookupOrderingInfo
{
    public DataServiceDataLookup Lookup { get; }
    public DataStructureColumnSortDirection Direction { get; }
    public int SortOrder { get; }

    internal static LookupOrderingInfo TryCreate(List<Ordering> orderings, string columnName)
    {
        var ordering = orderings?.FirstOrDefault(x => x.ColumnName == columnName);
        if (ordering == null || ordering.LookupId == Guid.Empty)
        {
            return null;
        }

        var lookup =
            ServiceManager
                .Services.GetService<IPersistenceService>()
                .SchemaProvider.RetrieveInstance(
                    typeof(DataServiceDataLookup),
                    new Key(ordering.LookupId)
                ) as DataServiceDataLookup;

        var direction =
            ordering.Direction.ToLower() == "asc"
                ? DataStructureColumnSortDirection.Ascending
                : DataStructureColumnSortDirection.Descending;
        return new LookupOrderingInfo(lookup, direction, ordering.SortOrder);
    }

    private LookupOrderingInfo(
        DataServiceDataLookup lookup,
        DataStructureColumnSortDirection direction,
        int sortOrder
    )
    {
        Lookup = lookup;
        Direction = direction;
        SortOrder = sortOrder;
    }
}

public class ColumnRenderData
{
    public string Expression { get; set; }
    public string Alias { get; set; }
}

internal struct SortOrder
{
    public string ColumnName;
    public string Expression;
    public DataStructureColumnSortDirection SortDirection;
}

internal enum JoinBeginType
{
    Join = 0,
    Where = 1,
    And = 2,
}

internal enum DateTypeSql
{
    Second,
    Minute,
    Hour,
    Day,
    Month,
    Year,
}

internal enum geoLatLonSql
{
    Lat,
    Lon,
}
