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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Origam.Schema;
using Origam.Schema.EntityModel;

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
			if(sqlParam.SqlDbType == SqlDbType.Decimal)
			{
				sqlParam.Precision = 28;
				sqlParam.Scale = 10;
			}
            // Workaround: in .net 2.0 if NText is not -1, 
            // sometimes the memo is truncated when storing to db
			if(sqlParam.SqlDbType == SqlDbType.NVarChar && dataLength == 0)
			{
				sqlParam.Size = -1;
			}
			if(sqlParam.SqlDbType == SqlDbType.NText 
                || sqlParam.SqlDbType == SqlDbType.Text)
			{
				sqlParam.Size = -1;
			}
			if(sqlParam.SqlDbType == SqlDbType.Structured)
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
			if(sqla == null) throw new ArgumentOutOfRangeException("adapter", adapter, ResourceUtils.GetString("InvalidAdapterType"));

			newa.AcceptChangesDuringFill = adapter.AcceptChangesDuringFill;
			newa.ContinueUpdateOnError = adapter.ContinueUpdateOnError;
			newa.DeleteCommand = (SqlCommand)CloneCommand(sqla.DeleteCommand);
			newa.InsertCommand = (SqlCommand)CloneCommand(sqla.InsertCommand);
			newa.MissingMappingAction = sqla.MissingMappingAction;
			newa.MissingSchemaAction = sqla.MissingSchemaAction;
			newa.SelectCommand = (SqlCommand)CloneCommand(sqla.SelectCommand);
			newa.UpdateCommand = (SqlCommand)CloneCommand(sqla.UpdateCommand);

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

			SqlCommand newc = GetCommand(command.CommandText) as SqlCommand;
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
			SqlParameter newp = GetParameter() as SqlParameter;
			newp.DbType = param.DbType;
			newp.Direction = param.Direction;
			newp.IsNullable = param.IsNullable;
			newp.ParameterName = param.ParameterName;
			newp.Precision = param.Precision;
			newp.Scale = param.Scale;
			// don't copy the size for blobs - they should always be 0 so the size is
			// automatically calculated on every update
			if(param.DbType != DbType.Binary) newp.Size = param.Size;
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
			switch(columnType)
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
			if(top == 0)
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
	}
}