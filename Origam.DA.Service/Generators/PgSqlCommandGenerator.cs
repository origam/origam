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
using Npgsql;
using NpgsqlTypes;

using Origam.Schema;
using Origam.Schema.EntityModel;
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

        internal override string getSql(ConvertSql v)
        {
                switch (v)
                {
                    case ConvertSql.FREETEXT:
                        throw new NotImplementedException();
                    case ConvertSql.ISNULL:
                        return "coalesce";
                    case ConvertSql.DBO:
                        return "";
                    case ConvertSql.NVARCHAR_MAX:
                        return "AS TEXT";
                    case ConvertSql.NVARCHAR:
                        return "AS VARCHAR";
                    case ConvertSql.INT:
                        return "AS INTEGER";
                }
            
            throw new NotImplementedException();
        }
    }
}