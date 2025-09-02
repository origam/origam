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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Origam.DA.Service.Generators;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service;

public class MsSqlCommandGenerator : AbstractSqlCommandGenerator
{
    public MsSqlCommandGenerator()
        : base(
            trueValue: "1",
            falseValue: "0",
            sqlValueFormatter: new SQLValueFormatter(
                "1",
                "0",
                (text) => text.Replace("%", "[%]").Replace("_", "[_]")
            ),
            filterRenderer: new MsSqlFilterRenderer(),
            new MsSqlRenderer()
        ) { }

    public override IDbCommand GetCommand(string cmdText)
    {
        return new SqlCommand(cmdText);
    }

    public override IDbDataParameter GetParameter()
    {
        return new SqlParameter();
    }

    public override IDbCommand GetCommand(string cmdText, IDbConnection connection)
    {
        return new SqlCommand(cmdText, connection as SqlConnection);
    }

    public override DbDataAdapter GetAdapter()
    {
        return new SqlDataAdapter();
    }

    public override DbDataAdapter GetAdapter(IDbCommand command)
    {
        return new SqlDataAdapter(command as SqlCommand);
    }

    public override IDbCommand GetCommand(
        string cmdText,
        IDbConnection connection,
        IDbTransaction transaction
    )
    {
        return new SqlCommand(cmdText, connection as SqlConnection, transaction as SqlTransaction);
    }

    public override void DeriveStoredProcedureParameters(IDbCommand command)
    {
        SqlCommand cmd = command as SqlCommand;
        SqlCommandBuilder.DeriveParameters(cmd);
        // http://stackoverflow.com/questions/9921121/unable-to-access-table-variable-in-stored-procedure
        foreach (SqlParameter parameter in cmd.Parameters)
        {
            if (parameter.SqlDbType != SqlDbType.Structured)
            {
                continue;
            }
            string name = parameter.TypeName;
            int index = name.IndexOf(".");
            if (index == -1)
            {
                continue;
            }
            name = name.Substring(index + 1);
            if (name.Contains("."))
            {
                parameter.TypeName = name;
            }
        }
    }

    #region Cloning
    public override DbDataAdapter CloneAdapter(DbDataAdapter adapter)
    {
        SqlDataAdapter newa = GetAdapter() as SqlDataAdapter;
        SqlDataAdapter sqla = adapter as SqlDataAdapter;
        if (sqla == null)
        {
            throw new ArgumentOutOfRangeException(
                "adapter",
                adapter,
                ResourceUtils.GetString("InvalidAdapterType")
            );
        }

        newa.AcceptChangesDuringFill = adapter.AcceptChangesDuringFill;
        newa.ContinueUpdateOnError = adapter.ContinueUpdateOnError;
        newa.DeleteCommand = (SqlCommand)CloneCommand(sqla.DeleteCommand);
        newa.InsertCommand = (SqlCommand)CloneCommand(sqla.InsertCommand);
        newa.MissingMappingAction = sqla.MissingMappingAction;
        newa.MissingSchemaAction = sqla.MissingSchemaAction;
        newa.SelectCommand = (SqlCommand)CloneCommand(sqla.SelectCommand);
        newa.UpdateCommand = (SqlCommand)CloneCommand(sqla.UpdateCommand);
        foreach (DataTableMapping tm in adapter.TableMappings)
        {
            DataTableMapping newtm = new DataTableMapping(tm.SourceTable, tm.DataSetTable);
            foreach (DataColumnMapping cm in tm.ColumnMappings)
            {
                newtm.ColumnMappings.Add(new DataColumnMapping(cm.SourceColumn, cm.DataSetColumn));
            }
            newa.TableMappings.Add(newtm);
        }
        return newa;
    }

    public override IDbCommand CloneCommand(IDbCommand command)
    {
        if (command == null)
        {
            return null;
        }

        SqlCommand newc = GetCommand(command.CommandText) as SqlCommand;
        newc.CommandTimeout = command.CommandTimeout;
        newc.CommandType = command.CommandType;
        newc.UpdatedRowSource = command.UpdatedRowSource;
        foreach (IDbDataParameter param in command.Parameters)
        {
            newc.Parameters.Add(CloneParameter(param));
        }
        return newc;
    }

    private IDbDataParameter CloneParameter(IDbDataParameter param)
    {
        SqlParameter newp = GetParameter() as SqlParameter;
        newp.DbType = param.DbType;
        newp.Direction = param.Direction;
        newp.IsNullable = param.IsNullable;
        newp.ParameterName = param.ParameterName;
        newp.Precision = param.Precision;
        newp.Scale = param.Scale;
        // don't copy the size for blobs - they should always be 0 so the size is
        // automatically calculated on every update
        if (param.DbType != DbType.Binary)
        {
            newp.Size = param.Size;
        }

        newp.SourceColumn = param.SourceColumn;
        newp.SourceVersion = param.SourceVersion;
        newp.SqlDbType = ((SqlParameter)param).SqlDbType;
        newp.Offset = ((SqlParameter)param).Offset;
        newp.TypeName = ((SqlParameter)param).TypeName;
        return newp;
    }
    #endregion
    public override OrigamDataType ToOrigamDataType(string ddlType)
    {
        switch (ddlType.ToUpper())
        {
            case "IMAGE":
                return OrigamDataType.Blob;
            case "BIT":
                return OrigamDataType.Boolean;
            case "TINYINT":
                return OrigamDataType.Byte;
            case "MONEY":
                return OrigamDataType.Currency;
            case "DATETIME":
                return OrigamDataType.Date;
            case "BIGINT":
                return OrigamDataType.Long;
            case "NTEXT":
                return OrigamDataType.Memo;
            case "SMALLINT":
            case "INT":
                return OrigamDataType.Integer;
            case "FLOAT":
            case "DECIMAL":
                return OrigamDataType.Float;
            case "VARCHAR":
            case "NVARCHAR":
                return OrigamDataType.String;
            case "UNIQUEIDENTIFIER":
                return OrigamDataType.UniqueIdentifier;
            case "GEOGRAPHY":
                return OrigamDataType.Geography;
            default:
                return OrigamDataType.String;
        }
    }

    public override string GetIndexName(IDataEntity entity, DataEntityIndex index)
    {
        return index.Name;
    }

    internal override string SqlDataType(IDataParameter Iparam)
    {
        SqlParameter param = Iparam as SqlParameter;
        string result = param.SqlDbType.ToString();
        if (param.DbType == DbType.String)
        {
            string size = param.Size == -1 ? "MAX" : param.Size.ToString();
            result += $"({size})";
        }
        return result;
    }

    internal override string FixAggregationDataType(OrigamDataType dataType, string expression)
    {
        switch (dataType)
        {
            case OrigamDataType.UniqueIdentifier:
                return " CAST (" + expression + " AS NVARCHAR (36))";
            case OrigamDataType.Boolean:
                return " CAST (" + expression + "  AS INT)";
            default:
                return expression;
        }
    }

    internal override string MergeSql(
        string tableName,
        StringBuilder keysBuilder,
        StringBuilder searchPredicatesBuilder,
        StringBuilder updateBuilder,
        StringBuilder insertColumnsBuilder,
        StringBuilder insertValuesBuilder
    )
    {
        StringBuilder sqlExpression = new StringBuilder();
        return sqlExpression
            .AppendFormat(
                "MERGE INTO {0} USING (SELECT {1}) AS src ON {2} WHEN MATCHED THEN UPDATE SET {3} WHEN NOT MATCHED THEN INSERT ({4}) VALUES ({5});",
                tableName,
                keysBuilder,
                searchPredicatesBuilder,
                updateBuilder,
                insertColumnsBuilder,
                insertValuesBuilder
            )
            .ToString();
    }

    public override object Clone()
    {
        MsSqlCommandGenerator gen = new MsSqlCommandGenerator();
        return gen;
    }

    public override string CreateOutputTableSql(string tmptable)
    {
        return "";
    }

    public override string CreateDataStructureFooterSql(List<string> tmptables)
    {
        return "";
    }

    internal override string ChangeColumnDef(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();
        ddl.Append(DdlDataType(field.DataType, field.DataLength, field.MappedDataType));
        if (field.AllowNulls)
        {
            ddl.Append(" NULL");
        }
        else
        {
            ddl.Append(" NOT NULL");
        }

        return ddl.ToString();
    }

    internal override string DropDefaultValue(FieldMappingItem field, string constraintName)
    {
        return string.Format(
            "ALTER TABLE {0} DROP CONSTRAINT {1};",
            RenderExpression(field.ParentItem as TableMappingItem),
            sqlRenderer.NameLeftBracket + constraintName + sqlRenderer.NameRightBracket
        );
    }

    public override string FunctionDefinitionDdl(Function function)
    {
        if (function.FunctionType == OrigamFunctionType.Database)
        {
            StringBuilder builder = new StringBuilder("CREATE FUNCTION dbo.");
            builder.Append(function.Name + "(");
            int i = 0;
            foreach (FunctionParameter parameter in function.ChildItems)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(ParameterDeclarationChar + parameter.Name + " as ?");
                i++;
            }
            builder.Append(")" + Environment.NewLine);
            builder.Append(
                "RETURNS " + DdlDataType(function.DataType, 0, null) + Environment.NewLine
            );
            builder.Append("AS" + Environment.NewLine + "BEGIN" + Environment.NewLine);
            builder.Append(
                "DECLARE "
                    + ParameterDeclarationChar
                    + "result AS "
                    + DdlDataType(function.DataType, 0, null)
                    + Environment.NewLine
            );
            builder.Append(
                "RETURN " + sqlRenderer.ParameterReferenceChar + "result" + Environment.NewLine
            );
            builder.Append("END");
            return builder.ToString();
        }

        throw new InvalidOperationException(ResourceUtils.GetString("DDLForFunctionsOnly"));
    }

    public override string DefaultDdlDataType(OrigamDataType columnType)
    {
        switch (columnType)
        {
            case OrigamDataType.Geography:
                return "geography";
            case OrigamDataType.Memo:
                return "nvarchar(max)";
            case OrigamDataType.Object:
                return "nvarchar(max)";
            case OrigamDataType.Xml:
                return "nvarchar(max)";
            case OrigamDataType.Blob:
                return "varbinary(max)";
            default:
                return ConvertDataType(columnType, null).ToString();
        }
    }

    private SqlDbType ConvertDataType(OrigamDataType columnType, DatabaseDataType dbDataType)
    {
        if (dbDataType != null)
        {
            return (SqlDbType)Enum.Parse(typeof(SqlDbType), dbDataType.MappedDatabaseTypeName);
        }
        switch (columnType)
        {
            case OrigamDataType.Blob:
                return SqlDbType.Image;
            case OrigamDataType.Boolean:
                return SqlDbType.Bit;
            case OrigamDataType.Byte:
                //TODO: check right
                return SqlDbType.TinyInt;
            case OrigamDataType.Currency:
                return SqlDbType.Money;
            case OrigamDataType.Date:
                return SqlDbType.DateTime;
            case OrigamDataType.Long:
                return SqlDbType.BigInt;
            case OrigamDataType.Xml:
            case OrigamDataType.Memo:
                return SqlDbType.NVarChar;
            case OrigamDataType.Array:
                return SqlDbType.Structured;
            case OrigamDataType.Geography:
                return SqlDbType.Text;
            case OrigamDataType.Integer:
                return SqlDbType.Int;
            case OrigamDataType.Float:
                return SqlDbType.Decimal;
            case OrigamDataType.Object:
            case OrigamDataType.String:
                return SqlDbType.NVarChar;
            case OrigamDataType.UniqueIdentifier:
                return SqlDbType.UniqueIdentifier;
            default:
                throw new NotSupportedException(ResourceUtils.GetString("UnsupportedType"));
        }
    }

    public override IDbDataParameter BuildParameter(
        string paramName,
        string sourceColumn,
        OrigamDataType dataType,
        DatabaseDataType dbDataType,
        int dataLength,
        bool allowNulls
    )
    {
        SqlParameter sqlParam = new SqlParameter(
            paramName,
            ConvertDataType(dataType, dbDataType),
            dataLength,
            sourceColumn
        );
        sqlParam.IsNullable = allowNulls;
        if (sqlParam.SqlDbType == SqlDbType.Decimal)
        {
            sqlParam.Precision = 28;
            sqlParam.Scale = 10;
        }
        // Workaround: in .net 2.0 if NText is not -1,
        // sometimes the memo is truncated when storing to db
        if (sqlParam.SqlDbType == SqlDbType.NVarChar && dataLength == 0)
        {
            sqlParam.Size = -1;
        }
        if (sqlParam.SqlDbType == SqlDbType.NText || sqlParam.SqlDbType == SqlDbType.Text)
        {
            sqlParam.Size = -1;
        }
        if (sqlParam.SqlDbType == SqlDbType.Structured)
        {
            sqlParam.TypeName = "OrigamListValue";
        }
        return sqlParam;
    }

    protected override string SqlPrimaryIndex()
    {
        return " PRIMARY KEY NONCLUSTERED";
    }

    protected override string RenderUpsertKey(string paramName, string fieldName)
    {
        return string.Format("{0} as {1}", paramName, fieldName);
    }
}
