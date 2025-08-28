#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using Origam.DA;

namespace Origam.Workbench.Services.CoreServices;

public interface ICoreDataService
{
    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId
    );

    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        string paramName1,
        object paramValue1
    );

    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        string paramName1,
        object paramValue1,
        string paramName2,
        object paramValue2
    );

    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        QueryParameterCollection parameters
    );

    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        QueryParameterCollection parameters,
        DataSet currentData
    );

    DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid defaultSetId,
        Guid sortSetId,
        string transactionId,
        QueryParameterCollection parameters,
        DataSet currentData,
        string entity,
        string columnName
    );

    DataSet LoadRow(
        Guid dataStructureEntityId,
        Guid filterSetId,
        QueryParameterCollection parameters,
        DataSet currentData,
        string transactionId
    );

    DataSet StoreData(
        Guid dataStructureId,
        DataSet data,
        bool loadActualValuesAfterUpdate,
        string transactionId
    );

    DataSet StoreData(DataStructureQuery dataStructureQuery, DataSet data, string transactionId);

    DataSet ExecuteProcedure(
        string procedureName,
        QueryParameterCollection parameters,
        string transactionId
    );

    long ReferenceCount(Guid entityId, object value, string transactionId);
    string EntityDdl(Guid entityId);
    string[] FieldDdl(Guid fieldId);
    string ExecuteSql(string command);
}
