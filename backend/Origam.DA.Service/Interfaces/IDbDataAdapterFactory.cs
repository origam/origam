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
using System.Data;
using System.Data.Common;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service;
/// <summary>
/// Summary description for DbDataAdapterFactory.
/// </summary>
public interface IDbDataAdapterFactory : ICloneable, IDisposable
{
	bool UserDefinedParameters{get; set;}
	bool ResolveAllFilters{get; set;}
    bool PrettyFormat { get; set; }
	DbDataAdapter CreateDataAdapter(SelectParameters adParameters,
        bool forceDatabaseCalculation);
	DbDataAdapter CreateDataAdapter(string procedureName, ArrayList entitiesOrdered, 
        IDbConnection connection, IDbTransaction transaction);
	DbDataAdapter CreateSelectRowDataAdapter(DataStructureEntity entity, 
		DataStructureFilterSet filterSet, ColumnsInfo columnsInfo,
        bool forceDatabaseCalculation);
	IDbCommand ScalarValueCommand(DataStructure ds,  DataStructureFilterSet filter, 
        DataStructureSortSet sortSet, ColumnsInfo columnsInfo, Hashtable parameters);
	IDbCommand UpdateFieldCommand(TableMappingItem entity,  FieldMappingItem field);
	IDbCommand GetCommand(string cmdText, IDbConnection connection);
	IDbCommand GetCommand(string cmdText, IDbConnection connection, IDbTransaction transaction);
	IDbCommand GetCommand(string cmdText);
	IDbDataParameter GetParameter();
	IDbDataParameter GetParameter(string name, Type type);
	DbDataAdapter CreateUpdateFieldDataAdapter(TableMappingItem table, FieldMappingItem field);
	IDbCommand SelectReferenceCountCommand(TableMappingItem table, FieldMappingItem field);
	DbDataAdapter GetAdapter();
	DbDataAdapter GetAdapter(IDbCommand command);
	DbDataAdapter CloneAdapter(DbDataAdapter adapter);
	IDbCommand CloneCommand(IDbCommand command);
	ArrayList Parameters(DataStructure ds, DataStructureEntity entity, 
        DataStructureFilterSet filter, DataStructureSortSet sort, bool paging,
        string columnName);
    string TableDefinitionDdl(TableMappingItem table);
    string AddColumnDdl(FieldMappingItem field);
    string AlterColumnDdl(FieldMappingItem field);
    string AddForeignKeyConstraintDdl(TableMappingItem table, DataEntityConstraint constraint);
	string ParameterDeclarationChar { get; }
}
