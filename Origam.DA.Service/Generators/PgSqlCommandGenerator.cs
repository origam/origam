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

using Npgsql;
using NpgsqlTypes;
using Origam.Schema;
using Origam.Schema.EntityModel;
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Origam.DA.Service
{
    /// <summary>
    /// Summary description for PgSqlCommandGenerator.
    /// </summary>
    public class PgSqlCommandGenerator : AbstractSqlCommandGenerator
	{

		public PgSqlCommandGenerator() : base()
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

		public override bool PagingCanIncludeOrderBy
		{
			get
			{
				return true;
			}
		}


		public override IDbDataParameter BuildParameter(string paramName, 
            string sourceColumn, OrigamDataType dataType, DatabaseDataType dbDataType,
            int dataLength, bool allowNulls)
		{
            NpgsqlDbType convDataType = ConvertDataType(dataType, dbDataType);
            if (dataType == OrigamDataType.Array)
            {
                convDataType = ConvertDataType(dataType, dbDataType) | NpgsqlDbType.Text;
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

		public override void DeriveStoredProcedureParameters(IDbCommand command)
		{
			NpgsqlCommandBuilder.DeriveParameters(command as NpgsqlCommand);
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

        public override string DefaultDdlDataType(OrigamDataType columnType)
        {
            switch (columnType)
            {
                case OrigamDataType.Date:
                    return string.Format("{0}(3)", ConvertDataType(columnType, null).ToString());
                default:
                    return ConvertDataType(columnType, null).ToString();
            }
        }

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

        public NpgsqlDbType ConvertDataType(OrigamDataType columnType,
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

		public override string NameLeftBracket
		{
			get
			{
				return "\"";
			}
		}

		public override string NameRightBracket
		{
			get
			{
				return "\"";
			}
		}

		public override string ParameterDeclarationChar
		{
			get
			{
				return "";
			}
		}

		public override string ParameterReferenceChar
		{
			get
			{
				return ":";
			}
		}

		public override string StringConcatenationChar
		{
			get
			{
				return "||";
			}
		}

		public override string GetIndexName(IDataEntity entity, DataEntityIndex index)
		{
			return entity.Name + "_" + index.Name;
		}

		public override string SelectClause(string finalQuery, int top)
		{
			if(top == 0)
			{
				return "SELECT" + finalQuery;
			}
			else
			{
				return "SELECT" + finalQuery + " LIMIT " + top.ToString();
			}
		}

		public override string True
		{
			get
			{
				return "true";
			}
		}

		public override string False
		{
			get
			{
				return "false";
			}
		}

		public override string ConvertGeoFromTextClause(string argument)
		{
			return "ST_GeomFromText(" + argument + ", 4326)";
		}

		public override string ConvertGeoToTextClause(string argument)
		{
            return "ST_AsText(" + argument + ")";
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
                    builder.Append(ParameterDeclarationChar + parameter.Name + " ?");
                    i++;
                }
                builder.Append(")" + Environment.NewLine);
                builder.Append("RETURNS " + DdlDataType(function.DataType, 0, null) + Environment.NewLine);
                builder.Append("AS $$" + Environment.NewLine);
                builder.Append("DECLARE " + ParameterDeclarationChar + " "
                    + DdlDataType(function.DataType, 0, null) + Environment.NewLine);
                builder.Append("BEGIN" + Environment.NewLine);
                
                builder.Append("RETURN " + ParameterReferenceChar + Environment.NewLine);
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

        internal override string SqlDataType(IDataParameter Iparam)
        {
            NpgsqlParameter param = Iparam as Npgsql.NpgsqlParameter;
            string result = param.NpgsqlDbType.ToString();

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
                "INSERT INTO {0} ({1}) VALUES ({2}) ON CONFLICT {3} DO UPDATE SET {4};",
                tableName,
                insertColumnsBuilder,
                insertValuesBuilder,
                keysBuilder,
                updateBuilder
                ).ToString(); 
        }

        internal override string SequenceSql(string entityName, string primaryKeyName)
        {
            StringBuilder actualsequence = new StringBuilder();
            actualsequence.Append(entityName);
            actualsequence.Append("_");
            actualsequence.Append(primaryKeyName);
            actualsequence.Append("_seq");
            return "; SELECT currval(" + actualsequence.ToString() + ")";
        }

        internal override string IsNullSql()
        {
            return "COALESCE";
        }
        internal override string CountAggregateSql()
        {
            return "COUNT";
        }

        public override object Clone()
        {
            PgSqlCommandGenerator gen = new PgSqlCommandGenerator();
            return gen;
        }

        internal override string DeclareAsSql()
        {
            return "";
        }
        internal override string FunctionPrefixSql()
        {
            return "";
        }
        internal override string VarcharSql()
        {
            return "VARCHAR";
        }
        internal override string LengthSql(string expresion)
        {
            return string.Format("LENGTH({0})", expresion);
        }
        internal override string TextSql(string expresion)
        {
            return string.Format("CAST ({0} AS {1} )", expresion, "TEXT");
        }
        internal override string DatePartSql(string datetype, string expresion)
        {
            return string.Format("DATE_PART('{0}',{1})", datetype ,expresion );
        }
        internal override string DateAddSql(DateTypeSql datepart, string number, string date)
        {
            return string.Format("({0} + ( {1} || '{2}')::interval)",date,number,GetAddDateSql(datepart));
        }

        private string GetAddDateSql(DateTypeSql datepart)
        {
            switch (datepart)
            {
                case DateTypeSql.Second:
                    return "second";
                case DateTypeSql.Minute:
                    return "minute";
                case DateTypeSql.Hour:
                    return "hour";
                case DateTypeSql.Day:
                    return "day";
                case DateTypeSql.Month:
                    return "month";
                case DateTypeSql.Year:
                    return "year";

                default:
                    throw new NotSupportedException("Unsuported in AddDateSql " + datepart.ToString());
            }
        }
        internal override string DateDiffSql(DateTypeSql datepart, string startdate, string enddate)
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (datepart)
            {
                case DateTypeSql.Day:
                    stringBuilder.Append("DATE_PART('day', {0}::timestamp - {1}::timestamp) ");
                    break;
                case DateTypeSql.Hour:
                    stringBuilder.Append("DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                    stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp) ");
                    break;
                case DateTypeSql.Minute:
                    stringBuilder.Append("(DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                    stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                    stringBuilder.Append("DATE_PART('minute', {0}::timestamp - {1}::timestamp)");
                    break;
                case DateTypeSql.Second:
                    stringBuilder.Append("(((DATE_PART('day', {0}::timestamp - {1}::timestamp) * 24 + ");
                    stringBuilder.Append("DATE_PART('hour', {0}::timestamp - {1}::timestamp)) * 60 + ");
                    stringBuilder.Append("DATE_PART('minute', {0}::timestamp - {1}::timestamp)) *60 ");
                    stringBuilder.Append("DATE_PART('second', {0}::timestamp - {1}::timestamp)");
                    break;
                default:
                    throw new NotSupportedException("Unsuported DateDiffSql " + datepart.ToString());

            }
            return string.Format(stringBuilder.ToString(),enddate, startdate);
        }
        internal override string STDistanceSql(string point1, string point2)
        {
            return string.Format("ST_Distance(('SRID=4326;' || {0})::geography,('SRID=4326;' || {1})::geography)", 
                ConvertGeoToTextClause(point1),
                ConvertGeoToTextClause(point2));
        }
        internal override string NowSql()
        {
            return "NOW()";
        }
        internal override string FreeTextSql(string columnsForSeach, string freetext_string, string languageForFullText)
        {
            return string.Format("{0} @@ to_tsquery({1},{2})", columnsForSeach, languageForFullText, freetext_string);
        }
        internal override string ContainsSql(string columnsForSeach, string freetext_string, string languageForFullText)
        {
            return string.Format("levenshtein({0},{1})", columnsForSeach, freetext_string);
        }
        internal override string LatLonSql(geoLatLonSql latLon, string expresion)
        {
            switch (latLon)
            {
                case geoLatLonSql.Lat:
                    return string.Format("st_y({0})", expresion);
                case geoLatLonSql.Lon:
                    return string.Format("st_x({0})", expresion);
                default:
                    throw new NotSupportedException("Unsuported in Latitude or Longtitude " + latLon.ToString());
            }
        }
        internal override string ArraySql(string expresion1, string expresion2)
        {
            return string.Format("{0}::text = ANY ({1})", expresion1, expresion2);
        }
    }
}