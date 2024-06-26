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
using System.Globalization;
using System.Security.Principal;
using Origam.DA.ObjectPersistence;

namespace Origam.DA;
/// <summary>
/// Summary description for IDataService.
/// </summary>
public interface IDataService : IDisposable
{
    string DbUser { get; set; }
    bool UserDefinedParameters{get; set;}
	string ConnectionString{get; set;}
    int BulkInsertThreshold { get; set; }
    int UpdateBatchSize { get; set; }
    string BuildConnectionString(string serverName, int port ,string databaseName, string userName, string password, bool integratedAuthentication, bool pooling);
	IStateMachineService StateMachine{get; set;}
    IAttachmentService AttachmentService { get; set; }
	int UpdateData(DataStructureQuery dataStructureQuery, IPrincipal userProfile, DataSet ds, string transactionId);
	int UpdateData(DataStructureQuery dataStructureQuery, IPrincipal userProfile, 
        DataSet ds, string transactionId, bool forceBulkInsert);
	
	DataSet LoadDataSet(DataStructureQuery dataStructureQuery, IPrincipal userProfile, string transactionId);
	DataSet LoadDataSet(DataStructureQuery dataStructureQuery, IPrincipal userProfile, DataSet dataSet, string transactionId);
	DataSet ExecuteProcedure(string name, string entityOrder, DataStructureQuery query, string transactionId);
	object GetScalarValue(DataStructureQuery query, ColumnsInfo columnsInfo, IPrincipal userProfile, string transactionId);
	string Xsd(Guid dataStructureId);
	DataSet GetEmptyDataSet(Guid dataStructureId);
	DataSet GetEmptyDataSet(Guid dataStructureId, CultureInfo culture);
	ArrayList CompareSchema(IPersistenceProvider provider);
	string DatabaseSchemaVersion();
	void UpdateDatabaseSchemaVersion(string version, string transactionId);
	string ExecuteUpdate(string command, string transactionId);
	int UpdateField(Guid entityId, Guid fieldId, object oldValue, object newValue, IPrincipal userProfile, string transactionId);
	int ReferenceCount(Guid entityId, Guid fieldId, object value, IPrincipal userProfile, string transactionId);
	string Info{get;}
    void CreateDatabase(string name);
    void DeleteDatabase(string name);
    string EntityDdl(Guid entityId);
    string[] FieldDdl(Guid fieldId);
    string[] DatabaseSpecificDatatypes();
    IDataReader ExecuteDataReader(
        DataStructureQuery dataStructureQuery, 
        IPrincipal userProfile, 
        string transactionId);
    IEnumerable<IEnumerable<object>> ExecuteDataReader
        (DataStructureQuery dataStructureQuery);
    IEnumerable<Dictionary<string, object>>
        ExecuteDataReaderReturnPairs(DataStructureQuery query);
}
