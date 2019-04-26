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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.DA.Service
{
    /// <summary>
    /// Summary description for MsSqlCommandGenerator.
    /// </summary>
    public class MsSqlCommandGenerator : AbstractSqlCommandGenerator
    {
        public MsSqlCommandGenerator() : base()
        {
        }

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

        public override IDbCommand GetCommand(string cmdText, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand(cmdText, connection as SqlConnection, transaction as SqlTransaction);
        }

        public override IDbDataParameter BuildParameter(string paramName,
            string sourceColumn, OrigamDataType dataType, DatabaseDataType dbDataType,
            int dataLength, bool allowNulls)
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
            if (sqlParam.SqlDbType == SqlDbType.NText
                || sqlParam.SqlDbType == SqlDbType.Text)
            {
                sqlParam.Size = -1;
            }
            if (sqlParam.SqlDbType == SqlDbType.Structured)
            {
                sqlParam.TypeName = "OrigamListValue";
            }
            return sqlParam;
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
            if (sqla == null) throw new ArgumentOutOfRangeException("adapter", adapter, ResourceUtils.GetString("InvalidAdapterType"));

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
            if (command == null) return null;

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
            if (param.DbType != DbType.Binary) newp.Size = param.Size;
            newp.SourceColumn = param.SourceColumn;
            newp.SourceVersion = param.SourceVersion;
            newp.SqlDbType = ((SqlParameter)param).SqlDbType;
            newp.Offset = ((SqlParameter)param).Offset;
            newp.TypeName = ((SqlParameter)param).TypeName;
            return newp;
        }
        #endregion

        public override string DefaultDdlDataType(OrigamDataType columnType)
        {
            switch (columnType)
            {
                case OrigamDataType.Geography:
                    return "geography";
                case OrigamDataType.Memo:
                    return "nvarchar(max)";
                case OrigamDataType.Blob:
                    return "varbinary(max)";
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


        public SqlDbType ConvertDataType(OrigamDataType columnType,
            DatabaseDataType dbDataType)
        {
            if (dbDataType != null)
            {
                return (SqlDbType)Enum.Parse(typeof(SqlDbType),
                    dbDataType.MappedDatabaseTypeName);
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
                    return SqlDbType.NText;
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

        public override string NameLeftBracket
        {
            get
            {
                return "[";
            }
        }

        public override string NameRightBracket
        {
            get
            {
                return "]";
            }
        }

        public override string GetIndexName(IDataEntity entity, DataEntityIndex index)
        {
            return index.Name;
        }

        public override string ParameterDeclarationChar
        {
            get
            {
                return "@";
            }
        }

        public override string ParameterReferenceChar
        {
            get
            {
                return "@";
            }
        }

        public override string StringConcatenationChar
        {
            get
            {
                return "+";
            }
        }


        public override string SelectClause(string finalQuery, int top)
        {
            if (top == 0)
            {
                return "SELECT" + finalQuery;
            }
            else
            {
                return "SELECT TOP " + top.ToString() + finalQuery;
            }
        }

        public override string True
        {
            get
            {
                return "1";
            }
        }

        public override string False
        {
            get
            {
                return "0";
            }
        }

        public override bool PagingCanIncludeOrderBy
        {
            get
            {
                return false;
            }
        }

        public override string ConvertGeoFromTextClause(string argument)
        {
            return "geography::STGeomFromText(" + argument + ", 4326)";
        }

        public override string ConvertGeoToTextClause(string argument)
        {
            return argument + ".STAsText()";
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
                    throw new NotImplementedException(ResourceUtils.GetString("TypeNotImplementedByMS", constant.DataType.ToString()));
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
                ddl.Append(" PRIMARY KEY NONCLUSTERED");
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

            ddl.Append(")");

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
            ddl.AppendFormat("CREATE {0}INDEX {1} ON {2} (",
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
            ddl.Append(")");

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
                if (dsEntity != null)
                {
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
        public override string FunctionDefinitionDdl(Function function)
        {
            if (function.FunctionType == OrigamFunctionType.Database)
            {
                StringBuilder builder = new StringBuilder("CREATE FUNCTION dbo.");
                builder.Append(function.Name + "(");
                int i = 0;
                foreach (FunctionParameter parameter in function.ChildItems)
                {
                    if (i > 0) builder.Append(", ");
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

        internal override void SelectParameterDeclarationsSql(StringBuilder result, Hashtable ht, DataStructure ds, DataStructureEntity entity, DataStructureFilterSet filter, DataStructureSortSet sort, bool paging, string columnName)
        {
            IDbCommand cmd = SelectCommand(ds, entity, filter, sort, paging, columnName);
            ArrayList list = Parameters(cmd);
            foreach (string paramName in list)
            {
                IDataParameter param = cmd.Parameters[ParameterDeclarationChar + paramName] as IDataParameter;

                if (!ht.Contains(param.ParameterName))
                {
                    result.AppendFormat("DECLARE {0} AS {1}{2}",
                        param.ParameterName,
                        SqlDataType(param as System.Data.SqlClient.SqlParameter),
                        Environment.NewLine);

                    ht.Add(param.ParameterName, null);
                }
            }
        }

        internal override string SqlDataType(IDataParameter Iparam)
        {
            SqlParameter param = Iparam as SqlParameter;
            string result = param.SqlDbType.ToString();

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
                    return " CAST (" + expression + " AS NVARCHAR (36))";
                case OrigamDataType.Boolean:
                    return " CAST (" + expression + "  AS INT)";
                default:
                    return expression;
            }
        }

        internal override string FixSumAggregation(AggregationType aggregationType, string expression)
        {
            if (aggregationType == AggregationType.Sum)
            {
                return " ISNULL (" + expression + ", 0)";
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
                    if (item.Function.Name == "FullText") result = "FREETEXT (";

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
                    result += ")";
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
                    result = "dbo." + item.Function.Name + " (";

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
                        + "NVARCHAR(" + item.DataLength + "), "
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

                    result =  "ISNULL (" + RenderExpression(expressionArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ", " + RenderExpression(replacementArg, entity, replaceParameterTexts, dynamicParameters, parameterReferences) + ")";

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
                concatBuilder.Append("AS NVARCHAR(MAX)");
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
                    return "COUNT_BIG";

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
                            if (keys > 0)
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

            if (keys == 0)
            {
                throw new Exception("Cannot build an UPSERT command, no UPSERT keys specified in the entity.");
            }

            sqlExpression.AppendFormat("MERGE INTO {0} USING (SELECT {1}) AS src ON {2} WHEN MATCHED THEN UPDATE SET {3} WHEN NOT MATCHED THEN INSERT ({4}) VALUES ({5});",
                tableName,
                keysBuilder,
                searchPredicatesBuilder,
                updateBuilder,
                insertColumnsBuilder,
                insertValuesBuilder
                );

            return sqlExpression.ToString();
        }

        internal override string InsertSql(DataStructure ds, DataStructureEntity entity)
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
            foreach (DataStructureColumn column in entity.Columns)
            {
                if (column.Field.IsPrimaryKey) primaryKeys.Add(column);
            }
            if (primaryKeys.Count == 0)
            {
                throw new OrigamException(ResourceUtils.GetString("NoPrimaryKey", entity.Name));
            }
            PrettyLine(sqlExpression);
            sqlExpression.Append("; SELECT @@IDENTITY AS " + ((DataStructureColumn)primaryKeys[0]).Name);
        }
    }
}