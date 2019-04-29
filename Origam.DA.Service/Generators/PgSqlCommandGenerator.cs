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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

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
			NpgsqlParameter sqlParam = new NpgsqlParameter(
				paramName,
				ConvertDataType(dataType, dbDataType),
				dataLength,
				sourceColumn
				);

			sqlParam.IsNullable = allowNulls;

			if(sqlParam.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Numeric)
			{
				sqlParam.Precision = 18;
				sqlParam.Scale = 10;
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
            return ConvertDataType(columnType, null).ToString();
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
				case OrigamDataType.Array:
					return NpgsqlDbType.Text;
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
			return "AsText(" + argument + ")";
		}

        public override string AddColumnDdl(FieldMappingItem field)
        {
            StringBuilder ddl = new StringBuilder();

            ddl.AppendFormat("ALTER TABLE {0} ADD {1}",
                RenderExpression(field.ParentItem as TableMappingItem),
                ColumnDefinitionDdl(field));

            if (!field.AllowNulls && field.DefaultValue != null)
            {
                string constraintName = "DF_" + (field.ParentItem as TableMappingItem).MappedObjectName + "_" + field.MappedColumnName;
                ddl.AppendFormat(" CONSTRAINT {0} DEFAULT {1}",
                    NameLeftBracket + constraintName + NameRightBracket,
                    this.RenderConstant(field.DefaultValue, false));

                ddl.Append(Environment.NewLine);
                ddl.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}",
                    RenderExpression(field.ParentItem as TableMappingItem),
                    NameLeftBracket + constraintName + NameRightBracket);
            }

            return ddl.ToString();
        }

        internal override string RenderConstant(DataConstant constant, bool userDefinedParameters)
        {
            //needs set
            if (constant.Name == "null") return "NULL";

            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

            object value;
            if (userDefinedParameters && parameterService != null)
            {
                value = parameterService.GetParameterValue(constant.Id);
            }
            else
            {
                value = constant.Value;
            }

            switch (constant.DataType)
            {
                case OrigamDataType.Integer:
                case OrigamDataType.Float:
                case OrigamDataType.Currency:
                    return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);

                case OrigamDataType.Boolean:
                    if ((bool)constant.Value)
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
                    if (value == null) return "null";

                    return ((DateTime)value).ToString(@"{ \t\s \'yyyy-MM-dd HH:mm:ss\' }");

                default:
                    throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByPostgres", constant.DataType.ToString()));
            }
        }

        internal override string ColumnDefinitionDdl(FieldMappingItem field)
        {
            StringBuilder ddl = new StringBuilder();
            // fname | varchar(20) | NOT NULL | PRIMARY KEY
            ddl.AppendFormat("{0} {1}",
                NameLeftBracket + field.MappedColumnName + NameRightBracket,
                DdlDataType(field.DataType, field.DataLength, field.MappedDataType)
                );
            if (field.AllowNulls)
                ddl.Append(" NULL");
            else
                ddl.Append(" NOT NULL");
            if (field.IsPrimaryKey)
                ddl.Append(" PRIMARY KEY ");
            return ddl.ToString();
        }

        public override string AlterColumnDdl(FieldMappingItem field)
        {
            StringBuilder ddl = new StringBuilder();

            ddl.AppendFormat("ALTER TABLE {0} ALTER COLUMN {1}",
                RenderExpression(field.ParentItem as TableMappingItem),
                ColumnDefinitionDdl(field));
            return ddl.ToString();
        }

        public override string TableDefinitionDdl(TableMappingItem table)
        {
            if (table.DatabaseObjectType != DatabaseMappingObjectType.Table)
            {
                throw new InvalidOperationException(ResourceUtils.GetString("CantDDLScript", table.DatabaseObjectType.ToString()));
            }

            StringBuilder ddl = new StringBuilder();
            ddl.AppendFormat("CREATE TABLE {0} (",
                NameLeftBracket + table.MappedObjectName + NameRightBracket);

            int i = 0;
            foreach (ISchemaItem item in table.EntityColumns)
            {
                if (item is FieldMappingItem)
                {
                    FieldMappingItem field = item as FieldMappingItem;

                    if (i > 0) ddl.Append(",");

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

        public override string IndexDefinitionDdl(IDataEntity entity, DataEntityIndex index, bool complete)
        {
            StringBuilder ddl = new StringBuilder();
            ddl.AppendFormat("CREATE {0} INDEX  {1} ON {2} (",
                (index.IsUnique ? "UNIQUE " : ""),
                NameLeftBracket + GetIndexName(entity, index) + NameRightBracket,
                NameLeftBracket + (index.ParentItem as TableMappingItem).MappedObjectName + NameRightBracket
                );

            int i = 0;
            ArrayList sortedFields = index.ChildItemsByType(DataEntityIndexField.ItemTypeConst);
            sortedFields.Sort();

            foreach (DataEntityIndexField field in sortedFields)
            {
                if (i > 0) ddl.Append(", ");

                ddl.AppendFormat("{0} {1}",
                    RenderExpression(field.Field, null, null, null, null),
                    field.SortOrder == DataEntityIndexSortOrder.Descending ? "DESC" : "ASC");

                i++;
            }
            ddl.Append(");");

            return ddl.ToString();
        }

        internal override string RenderExpression(FieldMappingItem item, DataStructureEntity dsEntity)
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
                result = String.Format("coalesce({0},{1})", result, nonLocalizedResult);
                return result;
            }
            else
            {
                return nonLocalizedResult;
            }
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

        internal override void SelectParameterDeclarationsSql(StringBuilder result, Hashtable ht, DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter, DataStructureSortSet sort, bool paging, string columnName)
        {
            IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
            ArrayList list = Parameters(cmd);
            foreach (string paramName in list)
            {
                IDataParameter param = cmd.Parameters[ParameterDeclarationChar + paramName] as IDataParameter;

                if (!ht.Contains(param.ParameterName))
                {
                    result.AppendFormat("DECLARE {0} {1}{2}",
                        param.ParameterName,
                        SqlDataType(param),
                        Environment.NewLine);

                    ht.Add(param.ParameterName, null);
                }
            }
        }

        internal override string SqlDataType(IDataParameter Iparam)
        {
            NpgsqlParameter param = Iparam as Npgsql.NpgsqlParameter;
            string result = param.PostgresType.ToString();

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

        internal override string FixSumAggregation(AggregationType aggregationType, string expression)
        {
            if (aggregationType == AggregationType.Sum)
            {
                return " COALESCE (" + expression + ", 0)";
            }
            else
            {
                return expression;
            }
        }
        internal override string RenderDatabaseFunction(FunctionCall item, DataStructureEntity entity,
            Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences)
        {
            string result = "";

            switch (item.Function.Name)
            {
                case "DaysToAnniversary":
                    if (item.ChildItems[0].ChildItems.Count == 0)
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
                    if (item.ChildItems[0].ChildItems.Count == 0)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorLookupNotSet", item.Path));
                    }

                    result = "EXISTS (" + RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                        + ")";
                    break;

                case "Month":
                case "Year":
                case "Day":
                    if (item.ChildItems[0].ChildItems.Count == 0)
                    {
                        throw new Exception(ResourceUtils.GetString("ErrorExpressionNotSet", item.Path));
                    }

                    result = item.Function.Name + "(" + RenderExpression(item.ChildItems[0].ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                        + ")";
                    break;

                case "Hour":
                case "Minute":
                case "Second":
                    if (item.ChildItems[0].ChildItems.Count == 0)
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
                            , RenderExpression(countArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                            , RenderExpression(dateArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences));
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

                    if (item.Function.Name == "FullTextContains") result = "CONTAINS(";
                    if (item.Function.Name == "FullText") result = " @@ to_tsquery ('";

                    if (fieldsArg.ChildItems.Count == 0)
                    {
                        result += NameLeftBracket + entity.Name + NameRightBracket + ".*";
                    }
                    else
                    {
                        if (fieldsArg.ChildItems.Count > 1) result += "(";

                        int fieldNum = 0;
                        foreach (ISchemaItem field in fieldsArg.ChildItems)
                        {
                            if (fieldNum > 0) result += ", ";
                            result += RenderExpression(field, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
                            fieldNum++;
                        }

                        if (fieldsArg.ChildItems.Count > 1) result += ")";
                    }

                    result += ",";
                    result += RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

                    if (languageArg.ChildItems.Count > 0)
                    {
                        result += ",";
                        result += RenderExpression(languageArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences);
                    }
                    result += "')";
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
                    result = " CAST (" + RenderExpression(item.GetChildByName("argument").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                        + " AS DATE )";
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
                    result = item.Function.Name + "(";

                    ArrayList sortedParams = new ArrayList(item.ChildItems);
                    sortedParams.Sort();

                    int i = 0;
                    foreach (FunctionCallParameter param in sortedParams)
                    {
                        if (i > 0) result += ", ";

                        if (param.ChildItems.Count != 1) throw new ArgumentOutOfRangeException("Count", param.ChildItems.Count, "Argument number must be 1");

                        result += RenderExpression(param.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences);

                        i++;
                    }

                    result += ")";
                    break;
            }

            return result;
        }

        internal override string RenderBuiltinFunction(FunctionCall item, DataStructureEntity entity,
           Hashtable replaceParameterTexts, Hashtable dynamicParameters,
           Hashtable parameterReferences)
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
                    string leftValue = GetItemByFunctionParameter(item, "Left", entity,
                        replaceParameterTexts, dynamicParameters, parameterReferences);
                    string rightValue = GetItemByFunctionParameter(item, "Right", entity,
                        replaceParameterTexts, dynamicParameters, parameterReferences);
                    result = filterRenderer.BinaryOperator(leftValue, rightValue, item.Function.Name);
                    break;

                case "Not":
                    string argument = GetItemByFunctionParameter(item, "Argument", entity,
                        replaceParameterTexts, dynamicParameters, parameterReferences);
                    result = filterRenderer.Not(argument);
                    break;

                case "Concat":
                    ISchemaItem concatArg = item.GetChildByName("Strings");
                    ArrayList concatStrings = new ArrayList(concatArg.ChildItems);
                    if (concatStrings.Count < 2) throw new ArgumentOutOfRangeException("Strings", null, "There have to be at least 2 strings to concatenate.");
                    concatStrings.Sort();
                    result = RenderConcat(concatStrings, entity, replaceParameterTexts, dynamicParameters, parameterReferences);
                    break;

                case "LogicalOr":
                case "LogicalAnd":
                    var arguments = GetItemListByFunctionParameter(item, "Arguments",
                         entity, replaceParameterTexts, dynamicParameters,
                         parameterReferences);
                    result = filterRenderer.LogicalAndOr(item.Function.Name, arguments);
                    break;

                case "Space":
                    ISchemaItem spacesArg = item.GetChildByName("NumberOfSpaces");

                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                    decimal numberOfSpaces = Convert.ToDecimal(RenderExpression(spacesArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences), ci.NumberFormat);

                    for (int i = 0; i < numberOfSpaces; i++)
                    {
                        result += " ";
                    }

                    result = RenderString(result);
                    break;

                case "Substring":
                    result = " SUBSTRING("
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
                        + "VARCHAR(" + item.DataLength + "), "
                        + RenderExpression(item.GetChildByName("Expression").ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                        + ", 104)";
                    break;

                case "In":
                    ISchemaItem leftArg = item.GetChildByName("FilterExpression");
                    ISchemaItem listArg = item.GetChildByName("List");
                    SchemaItemCollection listExpressions = listArg.ChildItems;

                    if (listExpressions.Count < 1) throw new ArgumentOutOfRangeException("List", null, ResourceUtils.GetString("ErrorNoParamIN"));

                    if (listExpressions.Count == 1 && listExpressions[0] is ParameterReference && (listExpressions[0] as ParameterReference).Parameter.DataType == OrigamDataType.Array)
                    {
                        result = RenderExpression(leftArg.ChildItems[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences)
                            + " IN (SELECT ListValue FROM " + RenderExpression(listExpressions[0], entity, replaceParameterTexts, dynamicParameters, parameterReferences) + " origamListValue)";
                    }
                    else
                    {
                        // list of parameters
                        string leftOperand = RenderExpression(leftArg.ChildItems[0], entity,
                            replaceParameterTexts, dynamicParameters, parameterReferences);
                        IEnumerable<string> options = listExpressions
                            .ToEnumerable()
                            .Cast<ISchemaItem>()
                            .Select(listExpression =>
                                RenderExpression(listExpression, entity, replaceParameterTexts,
                                    dynamicParameters, parameterReferences));
                        result = filterRenderer.In(leftOperand, options);
                    }
                    break;

                case "IsNull":
                    ISchemaItem expressionArg = item.GetChildByName("Expression").ChildItems[0];
                    ISchemaItem replacementArg = item.GetChildByName("ReplacementValue").ChildItems[0];

                    result =  "COALESCE (" + RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", " + RenderExpression(replacementArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";

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

            return result;
        }

        internal override string RenderConcat(List<KeyValuePair<ISchemaItem, DataStructureEntity>> concatSchemaItemList, string separator, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            int i = 0;
            StringBuilder concatBuilder = new StringBuilder();
            foreach (KeyValuePair<ISchemaItem, DataStructureEntity> concatItem in concatSchemaItemList)
            {
                if (i > 0)
                {
                    concatBuilder.Append(" " + StringConcatenationChar + " ");
                    if (separator != null)
                    {
                        concatBuilder.Append(separator);
                        concatBuilder.Append(" " + StringConcatenationChar + " ");
                    }
                }

                concatBuilder.Append("CAST (");
                concatBuilder.Append(RenderExpression(concatItem.Key, concatItem.Value, replaceParameterTexts, dynamicParameters, parameterReferences));
                concatBuilder.Append(" ");
                concatBuilder.Append("AS TEXT");
                concatBuilder.Append(")");

                i++;
            }

            return concatBuilder.ToString();
        }

        internal override string GetAggregationString(AggregationType type)
        {
            switch (type)
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

        public override string UpsertSql(DataStructure ds, DataStructureEntity entity)
        {
            StringBuilder sqlExpression = new StringBuilder();
            StringBuilder keysBuilder = new StringBuilder();
            StringBuilder updateBuilder = new StringBuilder();
            StringBuilder insertColumnsBuilder = new StringBuilder();
            StringBuilder insertValuesBuilder = new StringBuilder();
            string tableName = RenderExpression(entity.EntityDefinition, null, null, null, null);

            int updateColumns = 0;
            int insertColumns = 0;
            int keys = 0;
            keysBuilder.Append("(");
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
                            if (keys > 0)
                            {
                                keysBuilder.Append(", ");
                            }
                            keysBuilder.AppendFormat("{0}",
                                fieldName);
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
            keysBuilder.Append(")");
            if (keys == 0)
            {
                throw new Exception("Cannot build an UPSERT command, no UPSERT keys specified in the entity.");
            }

            sqlExpression.AppendFormat(
                "INSERT INTO {0} ({1}) VALUES ({2}) ON CONFLICT {3} DO UPDATE SET {4};",
                tableName,
                insertColumnsBuilder,
                insertValuesBuilder,
                keysBuilder,
                updateBuilder
                );

            return sqlExpression.ToString();
        }

        public override string InsertSql(DataStructure ds, DataStructureEntity entity)
        {
            if (entity.UseUPSERT)
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
                    if (column.Field.AutoIncrement) existAutoIncrement = true;
                }
            }
            PrettyLine(sqlExpression);
            sqlExpression.Append(") VALUES (");
            sqlExpression.Append(sqlExpression2);
            sqlExpression.Append(")");
            // If there is any auto increment column, we include a SELECT statement after INSERT
            if (existAutoIncrement)
            {
                RenderSelectUpdatedData(sqlExpression, entity);
            }
            return sqlExpression.ToString();
        }

        internal override void RenderSelectUpdatedData(StringBuilder sqlExpression, DataStructureEntity entity)
        {
            ArrayList primaryKeys = new ArrayList();
            StringBuilder actualsequence = new StringBuilder();
            actualsequence.Append(entity.Name);
            actualsequence.Append("_");
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field.IsPrimaryKey) primaryKeys.Add(column);
            }
            if (primaryKeys.Count == 0)
            {
                throw new OrigamException(ResourceUtils.GetString("NoPrimaryKey", entity.Name));
            }
            PrettyLine(sqlExpression);
            actualsequence.Append(((DataStructureColumn)primaryKeys[0]).Name);
            actualsequence.Append("_seq");
            sqlExpression.Append("; SELECT currval(" + actualsequence.ToString() + ")");
        }

        internal override string SelectSql(DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter,
               DataStructureSortSet sortSet, string scalarColumn, Hashtable replaceParameterTexts,
               Hashtable dynamicParameters, Hashtable selectParameterReferences, bool restrictScalarToTop1,
               bool paging, bool isInRecursion, bool forceDatabaseCalculation,
               string customWhereClause = null, string customOrderByClause = null, int? rowLimit = null)
        {
            if (!(entity.EntityDefinition is TableMappingItem))
            {
                throw new Exception("Only database mapped entities can be processed by the Data Service!");
            }

            if (paging)
            {
                if (PageNumberParameterReference.PersistenceProvider == null)
                {
                    PageNumberParameterReference.PersistenceProvider = ds.PersistenceProvider;
                    PageSizeParameterReference.PersistenceProvider = ds.PersistenceProvider;
                    _pageNumberParameterName = ParameterReferenceChar + PageNumberParameterReference.Parameter.Name;
                    _pageSizeParameterName = ParameterReferenceChar + PageSizeParameterReference.Parameter.Name;
                }

                selectParameterReferences.Add(ParameterDeclarationChar + PageNumberParameterReference.Parameter.Name, PageNumberParameterReference);
                selectParameterReferences.Add(ParameterDeclarationChar + PageSizeParameterReference.Parameter.Name, PageSizeParameterReference);
            }

            StringBuilder sqlExpression = new StringBuilder();
            StringBuilder orderByBuilder = new StringBuilder();
            StringBuilder groupByBuilder = new StringBuilder();

            // when processing lookup columns we process semicolon delimited list of columns
            // to be returned as a single concatted field
            // Example: FirstName;Name -> concat(FirstName, ', ', Name)
            bool concatScalarColumns = restrictScalarToTop1;
            // Select
            RenderSelectColumns(ds, sqlExpression, orderByBuilder,
                groupByBuilder, entity, scalarColumn, replaceParameterTexts, dynamicParameters,
                sortSet, selectParameterReferences, isInRecursion, concatScalarColumns,
                forceDatabaseCalculation);

            // paging column
            if (paging)
            {
                if (sortSet == null)
                {
                    sqlExpression.AppendFormat(", ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS {0}", RowNumColumnName);
                }
                else
                {
                    sqlExpression.AppendFormat(", ROW_NUMBER() OVER (ORDER BY {0}) AS {1}", orderByBuilder, RowNumColumnName);
                }
            }

            // From
            RenderSelectFromClause(sqlExpression, entity, entity, filter, replaceParameterTexts);

            bool whereExists = false;

            if (!entity.ParentItem.PrimaryKey.Equals(ds.PrimaryKey))
            {
                // render joins that we need for fields in this entity
                foreach (DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
                {
                    if (relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
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
                foreach (DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
                {
                    if (relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
                    {
                        RenderSelectRelation(sqlExpression, relation, relation, filter, replaceParameterTexts, true, true, 0, false, dynamicParameters, selectParameterReferences);
                    }
                    else if (relation.RelationType == RelationType.FilterParent || relation.RelationType == RelationType.NotExists)
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

                    sqlExpression.Append(joinedFilterBuilder);
                }
            }

            // Where filter - only on root entity, all other filters are on relations
            StringBuilder whereBuilder = new StringBuilder();
            RenderSelectWherePart(whereBuilder, entity, filter, replaceParameterTexts, dynamicParameters, selectParameterReferences);

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

                sqlExpression.Append(whereBuilder);
            }

            if (!string.IsNullOrEmpty(customWhereClause))
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
                sqlExpression.Append(customWhereClause);
            }

            // GROUP BY
            if (groupByBuilder.Length > 0)
            {
                PrettyLine(sqlExpression);
                sqlExpression.AppendFormat("GROUP BY {0}", groupByBuilder);
            }

            // ORDER BY
            if (!string.IsNullOrWhiteSpace(customOrderByClause))
            {
                PrettyLine(sqlExpression);
                sqlExpression.AppendFormat("ORDER BY {0}", customOrderByClause);
            }
            else
            {
                if ((!paging || paging && PagingCanIncludeOrderBy) && orderByBuilder.Length > 0)
                {
                    PrettyLine(sqlExpression);
                    sqlExpression.AppendFormat("ORDER BY {0}", orderByBuilder);
                }
            }

            string finalString = sqlExpression.ToString();

            // subqueries, etc. will have TOP 1, so it is sure that they select only 1 value
            if (scalarColumn != null && restrictScalarToTop1)
            {
                finalString = SelectClause(finalString, 1);
            }
            else if (rowLimit.HasValue)
            {
                finalString = SelectClause(finalString, rowLimit.Value);
            }
            else
            {
                finalString = SelectClause(finalString, 0);
            }

            if (paging)
            {
                finalString = string.Format(
                    "SELECT * FROM ({0}) _page WHERE _page.{1} BETWEEN (({2} - 1) * {3}) + 1 AND {3} * {2}",
                    finalString, RowNumColumnName, _pageNumberParameterName, _pageSizeParameterName);
            }

            return finalString;
        }
        internal override void RenderSelectWherePart(StringBuilder sqlExpression, DataStructureEntity entity, DataStructureFilterSet filterSet, Hashtable replaceParameterTexts, Hashtable parameters, Hashtable parameterReferences)
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
                RenderFilter(sqlExpression, filter, entity, replaceParameterTexts, parameters, parameterReferences);

                i++;
            }

            if (i > 0) sqlExpression.Append(")");
        }

        internal override void RenderFilter(StringBuilder sqlExpression, EntityFilter filter, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            int i = 0;
            foreach (AbstractSchemaItem filterItem in filter.ChildItems)
            {
                if (i > 0)
                    sqlExpression.Append(" AND ");
                else
                    sqlExpression.Append(" (");

                sqlExpression.Append(RenderExpression(filterItem, entity, replaceParameterTexts, dynamicParameters, parameterReferences));

                i++;
            }

            if (i > 0)
                sqlExpression.Append(")");
        }

        internal override string AggregationHelper(AggregatedColumn topLevelItem, DataStructureEntity topLevelEntity, AggregatedColumn item, Hashtable replaceParameterTexts, int level, StringBuilder joins, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            AggregatedColumn agg2 = item.Field as AggregatedColumn;

            DataStructureEntity aggregationVirtualEntity = new DataStructureEntity();
            aggregationVirtualEntity.PersistenceProvider = topLevelEntity.PersistenceProvider;
            aggregationVirtualEntity.ParentItem = topLevelEntity.RootItem;
            aggregationVirtualEntity.Name = "aggregation" + level;

            if (agg2 != null)
            {
                if (agg2.AggregationType != item.AggregationType)
                {
                    throw new ArgumentOutOfRangeException("AggregationType", agg2.AggregationType, "Nested aggregations must be of the same type. Path: " + agg2.Path);
                }

                // nested aggregated expression
                DataStructureEntity aggregationVirtualEntity2 = new DataStructureEntity();
                aggregationVirtualEntity2.PersistenceProvider = topLevelEntity.PersistenceProvider;
                aggregationVirtualEntity2.ParentItem = topLevelEntity.RootItem;
                aggregationVirtualEntity2.Name = "aggregation" + (level + 1);

                joins.AppendFormat(" INNER JOIN {0} AS {1} ON ",
                    RenderExpression(agg2.Relation.AssociatedEntity as ISchemaItem, null, replaceParameterTexts, dynamicParameters, parameterReferences),
                    NameLeftBracket + aggregationVirtualEntity2.Name + NameRightBracket
                    );

                int i = 0;
                foreach (AbstractSchemaItem relationItem in agg2.Relation.ChildItems)
                {
                    EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
                    EntityRelationFilter filter = relationItem as EntityRelationFilter;

                    if (i > 0) joins.Append(" AND ");

                    if (key != null)
                    {
                        RenderSelectRelationKey(joins,
                            key, aggregationVirtualEntity, aggregationVirtualEntity2,
                            replaceParameterTexts, dynamicParameters, parameterReferences);
                    }

                    if (filter != null)
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
                    joins
                    );

                int i = 0;
                foreach (AbstractSchemaItem relationItem in topLevelItem.Relation.ChildItems)
                {
                    EntityRelationColumnPairItem key = relationItem as EntityRelationColumnPairItem;
                    EntityRelationFilter filter = relationItem as EntityRelationFilter;

                    if (i > 0) result.Append(" AND ");

                    if (key != null)
                    {
                        RenderSelectRelationKey(result,
                            key, topLevelEntity, topLevelAggregationVirtualEntity,
                            replaceParameterTexts, dynamicParameters, parameterReferences);
                    }

                    if (filter != null)
                    {
                        RenderFilter(result, filter.Filter, topLevelAggregationVirtualEntity, parameterReferences);
                    }

                    i++;
                }

                result.Append(")");

                return result.ToString();
            }
        }

        internal override void RenderUpdateDeleteWherePart(StringBuilder sqlExpression, DataStructureEntity entity)
        {
            PrettyLine(sqlExpression);
            sqlExpression.Append("WHERE (");
            int i = 0;
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (ShouldUpdateColumn(column, entity)
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
        internal override string RenderSortDirection(DataStructureColumnSortDirection direction)
        {
            switch (direction)
            {
                case DataStructureColumnSortDirection.Ascending:
                    return "ASC";

                case DataStructureColumnSortDirection.Descending:
                    return "DESC";

                default:
                    throw new ArgumentOutOfRangeException("direction", direction, ResourceUtils.GetString("UnknownSortDirection"));
            }
        }

        internal override void RenderSelectRelation(StringBuilder sqlExpression, DataStructureEntity dsEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, bool skipStopAtEntity, bool includeFilter, int numberOfJoins, bool includeAllRelations, Hashtable dynamicParameters, Hashtable parameterReferences)
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
                    if (dsEntity.RelationType == RelationType.Normal
                        || dsEntity.RelationType == RelationType.LeftJoin)
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
            if (skipStopAtEntity && dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey)
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
            if (assoc.IsOR)
            {
                relationBuilder.Append("(");
            }

            foreach (AbstractSchemaItem item in assoc.ChildItems)
            {
                PrettyIndent(relationBuilder);
                if (i > 0)
                {
                    if (assoc.IsOR)
                    {
                        relationBuilder.Append(" OR");
                    }
                    else
                    {
                        relationBuilder.Append(" AND");
                    }
                }

                if (item is EntityRelationColumnPairItem)
                {
                    RenderSelectRelationKey(relationBuilder,
                        item as EntityRelationColumnPairItem, dsEntity.ParentItem as DataStructureEntity,
                        dsEntity, replaceParameterTexts, dynamicParameters, parameterReferences);
                }
                else if (item is EntityRelationFilter)
                {
                    RenderFilter(relationBuilder, (item as EntityRelationFilter).Filter, dsEntity, parameterReferences);
                }
                else
                    throw new NotSupportedException(ResourceUtils.GetString("TypeNotSupportedByPostgresSql", item.GetType().ToString()));

                i++;
            }

            if (assoc.IsOR)
            {
                relationBuilder.Append(")");
            }

            if (!(dsEntity.PrimaryKey.Equals(stopAtEntity.PrimaryKey) & skipStopAtEntity & includeFilter == false))
            {
                StringBuilder whereBuilder = new StringBuilder();
                RenderSelectWherePart(whereBuilder, dsEntity, filter, replaceParameterTexts, dynamicParameters, parameterReferences);

                if (whereBuilder.Length > 0)
                {
                    PrettyIndent(relationBuilder);
                    relationBuilder.AppendFormat(" AND {0}",
                        whereBuilder);
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
            foreach (DataStructureEntity relation in (dsEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
            {
                RenderSelectRelation(recursionBuilder, relation, stopAtEntity, filter, replaceParameterTexts, skipStopAtEntity, includeFilter, numberOfJoins, includeAllRelations, dynamicParameters, parameterReferences);
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
        public override IDbCommand UpdateFieldCommand(TableMappingItem entity, FieldMappingItem field)
        {
            IDbCommand cmd = GetCommand(
                "UPDATE "
                + RenderExpression(entity)
                + " SET "
                + RenderExpression(field, null)
                + " = " + ParameterReferenceChar + "newValue WHERE "
                + RenderExpression(field, null)
                + " = " + ParameterReferenceChar + "oldValue");
            cmd.CommandType = CommandType.Text;
            IDataParameter sqlParam = BuildParameter(
                ParameterDeclarationChar + "oldValue", null, field.DataType,
                field.MappedDataType, field.DataLength, field.AllowNulls);
            cmd.Parameters.Add(sqlParam);
            sqlParam = BuildParameter(ParameterDeclarationChar + "newValue",
                null, field.DataType, field.MappedDataType, field.DataLength,
                field.AllowNulls);
            cmd.Parameters.Add(sqlParam);
            return cmd;
        }

        public override string AddForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint)
        {
            StringBuilder ddl = new StringBuilder();

            ddl.AppendFormat("ALTER TABLE {0} ADD {1}",
                NameLeftBracket + table.MappedObjectName + NameRightBracket,
                ForeignKeyConstraintDdl(table, constraint));
            return ddl.ToString();
        }
        public override string ForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint)
        {
            StringBuilder ddl = new StringBuilder();

            if (constraint.ForeignEntity is TableMappingItem && constraint.Fields[0] is FieldMappingItem)
            {
                string pkTableName = (constraint.ForeignEntity as TableMappingItem).MappedObjectName;

                ddl.AppendFormat("CONSTRAINT {1}",
                    NameLeftBracket + table.MappedObjectName + NameRightBracket,
                    "FK_" + table.MappedObjectName + "_" + (constraint.Fields[0] as FieldMappingItem).MappedColumnName + "_" + pkTableName);

                ddl.Append(Environment.NewLine + "\tFOREIGN KEY (");
                int i = 0;
                foreach (FieldMappingItem field in constraint.Fields)
                {
                    if (i > 0) ddl.Append(", ");
                    ddl.Append(Environment.NewLine + "\t\t" + RenderExpression(field, null));

                    i++;
                }
                ddl.Append(Environment.NewLine + "\t)" + Environment.NewLine);

                ddl.AppendFormat(Environment.NewLine + "\tREFERENCES {0} (",
                    NameLeftBracket + pkTableName + NameRightBracket);

                i = 0;
                foreach (FieldMappingItem field in constraint.Fields)
                {
                    if (i > 0) ddl.Append(", ");
                    ddl.Append(Environment.NewLine + "\t\t" + RenderExpression(field.ForeignKeyField, null, null, null, null));

                    i++;
                }

                ddl.Append(Environment.NewLine + "\t);");
            }

            return ddl.ToString();
        }

        internal override void SelectParameterDeclarationsSetSql(StringBuilder result, Hashtable parameters)
        {
            foreach (string name in parameters.Keys)
            {
                result.AppendFormat("SET {0} = NULL{1}", name, Environment.NewLine);
            }
        }

        public override string SelectRowSql(DataStructureEntity entity, Hashtable selectParameterReferences,
            string columnName, bool forceDatabaseCalculation)
        {
            StringBuilder sqlExpression = new StringBuilder();

            ArrayList primaryKeys = new ArrayList();
            sqlExpression.Append("SELECT ");

            RenderSelectColumns(entity.RootItem as DataStructure, sqlExpression, new StringBuilder(),
                new StringBuilder(), entity, columnName, new Hashtable(), new Hashtable(), null,
                selectParameterReferences, forceDatabaseCalculation);

            int i = 0;
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field is FieldMappingItem && column.UseLookupValue == false && column.UseCopiedValue == false)
                {
                    if (column.Field.IsPrimaryKey) primaryKeys.Add(column);
                    i++;
                }
            }
            PrettyLine(sqlExpression);
            sqlExpression.AppendFormat("FROM {0} AS {1} ",
                RenderExpression(entity.EntityDefinition, null, null, null, null),
                NameLeftBracket + entity.Name + NameRightBracket
                );

            foreach (DataStructureEntity relation in (entity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
            {
                if (relation.RelationType == RelationType.LeftJoin || relation.RelationType == RelationType.InnerJoin)
                {
                    RenderSelectRelation(sqlExpression, relation, relation, null, null, true, true, 0, false, null, selectParameterReferences);
                }
            }
            PrettyLine(sqlExpression);
            sqlExpression.Append("WHERE (");

            i = 0;
            foreach (DataStructureColumn column in primaryKeys)
            {
                if (i > 0) sqlExpression.Append(" AND");
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
        public override string SelectUpdateFieldSql(TableMappingItem table, FieldMappingItem updatedField)
        {
            DataStructureEntity entity = new DataStructureEntity();
            entity.PersistenceProvider = table.PersistenceProvider;
            entity.Name = table.Name;
            entity.Entity = table;

            StringBuilder sqlExpression = new StringBuilder();

            ArrayList selectKeys = new ArrayList();
            sqlExpression.Append("SELECT ");

            int i = 0;
            foreach (DataStructureColumn column in entity.Columns)
            {
                // we only select the primary key and the changed field
                if (column.Field.IsPrimaryKey | column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
                {
                    if (i > 0) sqlExpression.Append(", ");

                    if (column.Field.PrimaryKey.Equals(updatedField.PrimaryKey)) selectKeys.Add(column);

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
            foreach (DataStructureColumn column in selectKeys)
            {
                if (i > 0) sqlExpression.Append(" AND ");

                sqlExpression.AppendFormat("{0} = {1}",
                    RenderExpression(column.Field, null, null, null, null),
                    NewValueParameterName(column, false)
                    );
                i++;
            }

            sqlExpression.Append(")");

            return sqlExpression.ToString();
        }
        public override string SelectReferenceCountSql(TableMappingItem table, FieldMappingItem updatedField)
        {
            DataStructureEntity entity = new DataStructureEntity();
            entity.PersistenceProvider = table.PersistenceProvider;
            entity.Name = table.Name;
            entity.Entity = table;

            StringBuilder sqlExpression = new StringBuilder();

            ArrayList selectKeys = new ArrayList();
            sqlExpression.Append("SELECT COUNT(*) ");

            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field.PrimaryKey.Equals(updatedField.PrimaryKey))
                {
                    selectKeys.Add(column);
                }
            }

            sqlExpression.AppendFormat(" FROM {0} AS {1} WHERE (",
                RenderExpression(entity.EntityDefinition, null, null, null, null),
                NameLeftBracket + entity.Name + NameRightBracket
                );

            int i = 0;
            foreach (DataStructureColumn column in selectKeys)
            {
                if (i > 0) sqlExpression.Append(" AND ");

                sqlExpression.AppendFormat("{0} = {1}",
                    RenderExpression(column.Field, null, null, null, null),
                    NewValueParameterName(column, false)
                    );
                i++;
            }

            sqlExpression.Append(")");

            return sqlExpression.ToString();
        }
        public override string DeleteSql(DataStructure ds, DataStructureEntity entity)
        {
            StringBuilder sqlExpression = new StringBuilder();

        sqlExpression.AppendFormat("DELETE FROM {0} ",
                RenderExpression(entity.EntityDefinition, null, null, null, null)
                );

            RenderUpdateDeleteWherePart(sqlExpression, entity);

            return sqlExpression.ToString();
        }
        public override string UpdateSql(DataStructure ds, DataStructureEntity entity)
        {
            StringBuilder sqlExpression = new StringBuilder();

            sqlExpression.AppendFormat("UPDATE {0} SET ",
                RenderExpression(entity.EntityDefinition, null, null, null, null));

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
                    if (column.Field.AutoIncrement) existAutoIncrement = true;
                }
            }
            RenderUpdateDeleteWherePart(sqlExpression, entity);

            // If there is any auto increment column, we include a SELECT statement after INSERT
            if (existAutoIncrement)
            {
                RenderSelectUpdatedData(sqlExpression, entity);
            }

            return sqlExpression.ToString();
        }
        internal override bool RenderSelectColumns(DataStructure ds, StringBuilder sqlExpression,
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
            if (concatScalarColumns && scalarColumnNames.Count > 1)
            {
                List<KeyValuePair<ISchemaItem, DataStructureEntity>> scalarColumns =
                    new List<KeyValuePair<ISchemaItem, DataStructureEntity>>();

                foreach (string scalar in scalarColumnNames)
                {
                    foreach (DataStructureColumn column in entity.Columns)
                    {
                        if (column.Name == scalar)
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
            i = 0;
            foreach (DataStructureColumn column in GetSortedColumns(entity, scalarColumnNames))
            {
                var expression = RenderDataStructureColumn(ds, entity,
                        replaceParameterTexts, dynamicParameters,
                        sortSet, selectParameterReferences, isInRecursion,
                        forceDatabaseCalculation, group, order, ref groupByNeeded, scalarColumnNames, column);
                if (expression != null)
                {
                    if (i > 0) sqlExpression.Append(",");
                    PrettyIndent(sqlExpression);
                    i++;
                    sqlExpression.Append(expression);
                }
            }

            if (order.Count > 0)
            {
                i = 0;
                foreach (DictionaryEntry entry in order)
                {
                    if (i > 0)
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
        internal override string RenderLookupColumnExpression(DataStructure ds, DataStructureEntity entity, IDataEntityColumn field,
            IDataLookup lookup, Hashtable replaceParameterTexts, Hashtable dynamicParameters,
            Hashtable parameterReferences, bool isInRecursion)
        {
            DataServiceDataLookup dataServiceLookup = lookup as DataServiceDataLookup;

            if (dataServiceLookup == null)
            {
                throw new ArgumentOutOfRangeException("DefaultLookup", lookup, ResourceUtils.GetString("LookupTypeUnsupportedException"));
            }

            if (dataServiceLookup.ValueMethod != null && !(dataServiceLookup.ValueMethod is DataStructureFilterSet))
            {
                throw new ArgumentOutOfRangeException("ListMethod", dataServiceLookup.ListMethod, ResourceUtils.GetString("LookupListMethodTypeUnsupportedException"));
            }

            DataStructureFilterSet valueFilterSet = dataServiceLookup.ValueMethod as DataStructureFilterSet;
            DataStructureEntity lookupEntity = dataServiceLookup.ValueDataStructure.Entities[0] as DataStructureEntity;

            // any lookups with same entity name as any of the entities in this datastructure must be renamed
            bool lookupRenamed = false;
            foreach (DataStructureEntity e in ds.Entities)
            {
                if (e.Name == lookupEntity.Name)
                {
                    lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
                    lookupEntity.Name = "lookup" + lookupEntity.Name;
                    lookupRenamed = true;
                    break;
                }
            }

            Hashtable replaceTexts = new Hashtable(1);

            AbstractSchemaItem renderField = field as AbstractSchemaItem;
            if (field is LookupField)
            {
                renderField = (field as LookupField).Field as AbstractSchemaItem;
            }
            string myColumn = RenderExpression(renderField, entity, replaceParameterTexts, dynamicParameters, parameterReferences);

            if (dataServiceLookup.ValueMethod == null)
            {
                throw new ArgumentOutOfRangeException("ValueFilterSet", null, ResourceUtils.GetString("NoValueFilterSetForLookup", dataServiceLookup.Path));
            }

            // replace lookup parameters with keys from the entity
            foreach (object key in dataServiceLookup.ValueMethod.ParameterReferences.Keys)
            {
                string finalKey = key as string;
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
            return builder.ToString();
        }

        internal override void RenderSelectFromClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts)
        {
            PrettyLine(sqlExpression);
            sqlExpression.Append("FROM");
            PrettyIndent(sqlExpression);
            sqlExpression.AppendFormat("{0} AS {1}",
                RenderExpression(baseEntity.EntityDefinition, null, null, null, null),
                NameLeftBracket + baseEntity.Name + NameRightBracket);
        }

        internal override void RenderSelectExistsClause(StringBuilder sqlExpression, DataStructureEntity baseEntity, DataStructureEntity stopAtEntity, DataStructureFilterSet filter, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            PrettyLine(sqlExpression);
            sqlExpression.AppendFormat("WHERE EXISTS (SELECT * FROM {0} AS {1}",
                RenderExpression(baseEntity.Entity, null, null, null, null),
                NameLeftBracket + baseEntity.Name + NameRightBracket);

            bool stopAtIncluded = false;
            bool notExistsIncluded = false;

            foreach (DataStructureEntity relation in (baseEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
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
                        RenderSelectRelation(sqlExpression, relation, stopAtEntity, filter, replaceParameterTexts, true, false, 0, true, dynamicParameters, parameterReferences);
                    }
                }
            }

            // finally we do current entity
            if (stopAtIncluded)
            {
                RenderSelectRelation(sqlExpression, stopAtEntity, stopAtEntity, filter, replaceParameterTexts, true, false, 0, false, dynamicParameters, parameterReferences);
            }

            if (notExistsIncluded)
            {
                int notExistsCount = (stopAtIncluded ? 1 : 0);
                foreach (DataStructureEntity relation in (baseEntity.ChildItemsByType(DataStructureEntity.ItemTypeConst)))
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

            if (whereBuilder.Length > 0)
            {
                sqlExpression.Append(" AND ");
                sqlExpression.Append(whereBuilder);
            }

            sqlExpression.Append(")");
        }
        internal override string RenderExpression(EntityFilterLookupReference lookupReference, DataStructureEntity entity, Hashtable replaceParameterTexts, Hashtable dynamicParameters, Hashtable parameterReferences)
        {
            DataServiceDataLookup lookup = lookupReference.Lookup as DataServiceDataLookup;

            if (lookup == null)
            {
                throw new ArgumentOutOfRangeException("lookup", lookupReference.Lookup, ResourceUtils.GetString("LookupTypeUnsupportedException"));
            }

            if (lookup.ListMethod != null && !(lookup.ListMethod is DataStructureFilterSet))
            {
                throw new ArgumentOutOfRangeException("ListMethod", lookup.ListMethod, ResourceUtils.GetString("LookupListMethodTypeUnsupportedException"));
            }

            DataStructureEntity lookupEntity = (lookup.ListDataStructure.Entities[0] as DataStructureEntity);
            lookupEntity = lookupEntity.Clone(true) as DataStructureEntity;
            lookupEntity.Name = "lookup" + lookupEntity.Name;

            Hashtable replaceTexts = new Hashtable();

            foreach (AbstractSchemaItem paramMapping in lookupReference.ChildItems)
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
        public override string DdlDataType(OrigamDataType columnType, int dataLenght,
            DatabaseDataType dbDataType)
        {
            switch (columnType)
            {
                case OrigamDataType.String:
                    return DdlDataType(columnType, dbDataType)
                        + "(" + dataLenght + ")";

                case OrigamDataType.Xml:
                    return DdlDataType(columnType, dbDataType) + "(2000)";

                case OrigamDataType.Float:
                    return DdlDataType(columnType, dbDataType) + "(28,10)";

                default:
                    return DdlDataType(columnType, dbDataType);
            }
        }

        public override object Clone()
        {
            PgSqlCommandGenerator gen = new PgSqlCommandGenerator();
            return gen;
        }
    }
}