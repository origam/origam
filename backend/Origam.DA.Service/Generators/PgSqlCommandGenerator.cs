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
using System.Linq;
using System.Text;
using MoreLinq;
using Npgsql;
using NpgsqlTypes;
using Origam.DA.Service.Generators;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service;
public class PgSqlCommandGenerator : AbstractSqlCommandGenerator
{
	public PgSqlCommandGenerator() 
		: base(
			trueValue: "true",
			falseValue: "false",
			sqlValueFormatter: new SQLValueFormatter("true", "false", (text) => text.Replace("%", "\\%").Replace("_", "\\_")),
            filterRenderer: new PgSqlFilterRenderer(),
			new PgSqlRenderer())
	{
	}
	public override IDbCommand GetCommand(string cmdText)
	{
		return new NpgsqlCommand(cmdText);
	}
	public override IDbCommand GetCommand(string cmdText, IDbConnection connection)
	{
		return new NpgsqlCommand(cmdText, connection as NpgsqlConnection);
	}
	public override DbDataAdapter GetAdapter()
	{
		return new NpgsqlDataAdapter();
	}
	public override IDbDataParameter GetParameter()
	{
		return new NpgsqlParameter();
	}
	public override DbDataAdapter GetAdapter(IDbCommand command)
	{
		return new NpgsqlDataAdapter(command as NpgsqlCommand);
	}
	public override IDbCommand GetCommand(string cmdText, IDbConnection connection, IDbTransaction transaction)
	{
		return new NpgsqlCommand(cmdText, connection as NpgsqlConnection, transaction as NpgsqlTransaction);
	}
	public override void DeriveStoredProcedureParameters(IDbCommand command)
	{
		NpgsqlCommandBuilder.DeriveParameters(command as NpgsqlCommand);
		// add ParameterDeclarationChar to all input parameters
		command.Parameters.
			OfType<NpgsqlParameter>().
			Where(param => param.Direction == ParameterDirection.Input).
			ForEach(param => param.ParameterName = ParameterDeclarationChar + param.ParameterName);
    }
	#region Cloning
	public override DbDataAdapter CloneAdapter(DbDataAdapter adapter)
	{
		NpgsqlDataAdapter newa = GetAdapter() as NpgsqlDataAdapter;
		NpgsqlDataAdapter sqla = adapter as NpgsqlDataAdapter;
		if(sqla == null) throw new ArgumentOutOfRangeException("adapter", adapter, ResourceUtils.GetString("InvalidAdapterType"));
		newa.AcceptChangesDuringFill = adapter.AcceptChangesDuringFill;
		newa.ContinueUpdateOnError = adapter.ContinueUpdateOnError;
		newa.DeleteCommand = (NpgsqlCommand)CloneCommand(sqla.DeleteCommand);
		newa.InsertCommand = (NpgsqlCommand)CloneCommand(sqla.InsertCommand);
		newa.MissingMappingAction = sqla.MissingMappingAction;
		newa.MissingSchemaAction = sqla.MissingSchemaAction;
		newa.SelectCommand = (NpgsqlCommand)CloneCommand(sqla.SelectCommand);
		newa.UpdateCommand = (NpgsqlCommand)CloneCommand(sqla.UpdateCommand);
		foreach(DataTableMapping tm in adapter.TableMappings)
		{
			DataTableMapping newtm = new DataTableMapping(tm.SourceTable, tm.DataSetTable);
			foreach(DataColumnMapping cm in tm.ColumnMappings)
			{
				newtm.ColumnMappings.Add(new DataColumnMapping(cm.SourceColumn, cm.DataSetColumn));
			}
			newa.TableMappings.Add(newtm);
		}
		
		return newa;
	}
	public override IDbCommand CloneCommand(IDbCommand command)
	{
		if(command == null) return null;
		NpgsqlCommand newc = GetCommand(command.CommandText) as NpgsqlCommand;
		newc.CommandTimeout = command.CommandTimeout;
		newc.CommandType = command.CommandType;
		newc.UpdatedRowSource = command.UpdatedRowSource;
		foreach(IDbDataParameter param in command.Parameters)
		{
			newc.Parameters.Add(CloneParameter(param));
		}
		return newc;
	}
	private IDbDataParameter CloneParameter(IDbDataParameter param)
	{
		NpgsqlParameter newp = GetParameter() as NpgsqlParameter;
		newp.DbType = param.DbType;
		newp.Direction = param.Direction;
		newp.IsNullable = param.IsNullable;
		newp.ParameterName = param.ParameterName;
		newp.Precision = param.Precision;
		newp.Scale = param.Scale;
		newp.Size = param.Size;
		newp.SourceColumn = param.SourceColumn;
		newp.SourceVersion = param.SourceVersion;
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
		return entity.Name + "_" + index.Name;
	}
    internal override string SqlDataType(IDataParameter Iparam)
    {
        NpgsqlParameter param = Iparam as Npgsql.NpgsqlParameter;
        string result = param.NpgsqlDbType.ToString();
        if(param.NpgsqlDbType == (NpgsqlDbType.Array| NpgsqlDbType.Text))
        {
            result = "text[]";
        }
        if (param.DbType == DbType.String)
        {
            result += "(" + param.Size + ")";
        }
        return result;
    }
    internal override string FixAggregationDataType(OrigamDataType dataType, string expression)
    {
        switch (dataType)
        {
            case OrigamDataType.UniqueIdentifier:
                return " CAST (" + expression + " AS VARCHAR (36))";
            case OrigamDataType.Boolean:
                return " CAST (" + expression + "  AS INTEGER)";
            default:
                return expression;
        }
    }
    
    internal override string MergeSql(string tableName, StringBuilder keysBuilder, StringBuilder searchPredicatesBuilder, StringBuilder updateBuilder, StringBuilder insertColumnsBuilder, StringBuilder insertValuesBuilder)
    {
        StringBuilder sqlExpression = new StringBuilder();
        return sqlExpression.AppendFormat(
            "INSERT INTO {0} ({1}) VALUES ({2}) ON CONFLICT ({3}) DO UPDATE SET {4};",
            tableName,
            insertColumnsBuilder,
            insertValuesBuilder,
            keysBuilder,
            updateBuilder
            ).ToString(); 
    }
    
    public override object Clone()
    {
        PgSqlCommandGenerator gen = new PgSqlCommandGenerator();
        return gen;
    }
    public override string CreateOutputTableSql(string tmpTable)
    {
        return string.Format("CREATE TEMP TABLE {0} ON COMMIT DROP AS {1}",
            sqlRenderer.NameLeftBracket + tmpTable + sqlRenderer.NameRightBracket, Environment.NewLine);
    }
    public override string CreateDataStructureFooterSql(List<string> tmpTables)
    {
        StringBuilder output = new StringBuilder();
        output.Append(string.Format("{0}END $$;", Environment.NewLine));
        foreach (string tmpTable in tmpTables)
        {
            output.Append(string.Format("{0}SELECT * FROM {1};", Environment.NewLine,
                sqlRenderer.NameLeftBracket+tmpTable+sqlRenderer.NameRightBracket));
        }
        return output.ToString();
    }
  
    internal override string ChangeColumnDef(FieldMappingItem field)
    {
        StringBuilder ddl = new StringBuilder();
        if (field.AllowNulls)
        {
            ddl.Append(" DROP");
        }
        else
        {
            ddl.Append(" SET");
        }
        ddl.Append(" NOT NULL");
        return ddl.ToString();
    }
    internal override string DropDefaultValue(FieldMappingItem field, string constraintName)
    {
        return string.Format("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT;",
                RenderExpression(field.ParentItem as TableMappingItem),
                sqlRenderer.NameLeftBracket + field.MappedColumnName + sqlRenderer.NameRightBracket);
    }
    
    public override string FunctionDefinitionDdl(Function function)
    {
        if (function.FunctionType == OrigamFunctionType.Database)
        {
	        StringBuilder builder = new StringBuilder("CREATE FUNCTION ");
	        builder.Append(function.Name + "(");
	        int i = 0;
	        foreach (FunctionParameter parameter in function.ChildItems)
	        {
		        if (i > 0) builder.Append(", ");
		        builder.Append(ParameterDeclarationChar + parameter.Name +
		                       " ?");
		        i++;
	        }
	        builder.Append(")" + Environment.NewLine);
	        builder.Append("RETURNS " +
	                       DdlDataType(function.DataType, 0, null) +
	                       Environment.NewLine);
	        builder.Append("AS $$" + Environment.NewLine);
	        builder.Append("DECLARE " + ParameterDeclarationChar + " "
	                       + DdlDataType(function.DataType, 0, null) +
	                       Environment.NewLine);
	        builder.Append("BEGIN" + Environment.NewLine);
	        builder.Append("RETURN " + sqlRenderer.ParameterReferenceChar +
	                       Environment.NewLine);
	        builder.Append("END;" + Environment.NewLine);
	        builder.Append("$$ LANGUAGE plpgsql;");
	        return builder.ToString();
        }
        else
        {
	        throw new InvalidOperationException(
		        ResourceUtils.GetString("DDLForFunctionsOnly"));
        }
    }
    public override string DefaultDdlDataType(OrigamDataType columnType)
    {
        switch (columnType)
        {
	        case OrigamDataType.Date:
		        return string.Format("{0}(3)",
			        ConvertDataType(columnType, null).ToString());
	        default:
		        return ConvertDataType(columnType, null).ToString();
        }
    }
    public override IDbDataParameter BuildParameter(string paramName,
        string sourceColumn, OrigamDataType dataType,
        DatabaseDataType dbDataType,
        int dataLength, bool allowNulls)
    {
        NpgsqlDbType convDataType = ConvertDataType(dataType, dbDataType);
        if (dataType == OrigamDataType.Array)
        {
	        convDataType = ConvertDataType(dataType, dbDataType) |
	                       NpgsqlDbType.Text;
        }
        NpgsqlParameter sqlParam = new NpgsqlParameter(
	        paramName,
	        convDataType,
	        dataLength,
	        sourceColumn
        )
        {
	        IsNullable = allowNulls
        };
        if (sqlParam.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Numeric)
        {
	        sqlParam.Precision = 18;
	        sqlParam.Scale = 10;
        }
        if (sqlParam.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Timestamp)
        {
	        sqlParam.Precision = 3;
        }
        return sqlParam;
    }
    private NpgsqlDbType ConvertDataType(OrigamDataType columnType,
        DatabaseDataType dbDataType)
    {
        if (dbDataType != null)
        {
	        return (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType),
		        dbDataType.MappedDatabaseTypeName);
        }
        switch (columnType)
        {
	        case OrigamDataType.Blob:
		        return NpgsqlDbType.Bytea;
	        case OrigamDataType.Boolean:
		        return NpgsqlDbType.Boolean;
	        case OrigamDataType.Byte:
		        //TODO: check right 
		        return NpgsqlDbType.Smallint;
	        case OrigamDataType.Currency:
		        return NpgsqlDbType.Money;
	        case OrigamDataType.Date:
		        return NpgsqlDbType.Timestamp;
	        case OrigamDataType.Long:
		        return NpgsqlDbType.Bigint;
	        case OrigamDataType.Xml:
	        case OrigamDataType.Geography:
	        case OrigamDataType.Memo:
		        return NpgsqlDbType.Text;
	        case OrigamDataType.Array:
		        return NpgsqlDbType.Array;
	        case OrigamDataType.Integer: 
		        return NpgsqlDbType.Integer;
	        case OrigamDataType.Float:
		        return NpgsqlDbType.Numeric;
	        case OrigamDataType.Object:
	        case OrigamDataType.String:
		        return NpgsqlDbType.Varchar;
	        case OrigamDataType.UniqueIdentifier:
		        return NpgsqlDbType.Uuid;
	        default:
		        throw new NotSupportedException(ResourceUtils.GetString("UnsupportedType"));
        }
    }
    protected override string SqlPrimaryIndex()
    {
		return " PRIMARY KEY";
	}
    protected override string RenderUpsertKey(string paramName, string fieldName)
    {
		return fieldName;
    }
}
