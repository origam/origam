using System;
using System.Data;
using Origam.DA;

namespace Origam.Workbench.Services.CoreServices;

public interface ICoreDataService
{
    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId);

    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId, string paramName1,
        object paramValue1);

    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId, string paramName1,
        object paramValue1, string paramName2, object paramValue2);

    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId,
        QueryParameterCollection parameters);

    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId,
        QueryParameterCollection parameters, DataSet currentData);

    DataSet LoadData(Guid dataStructureId, Guid methodId, Guid defaultSetId,
        Guid sortSetId, string transactionId,
        QueryParameterCollection parameters, DataSet currentData,
        string entity, string columnName);

    DataSet LoadRow(Guid dataStructureEntityId,
        Guid filterSetId, QueryParameterCollection parameters,
        DataSet currentData, string transactionId);

    DataSet StoreData(Guid dataStructureId, DataSet data,
        bool loadActualValuesAfterUpdate, string transactionId);

    DataSet StoreData(DataStructureQuery dataStructureQuery, DataSet data,
        string transactionId);

    DataSet ExecuteProcedure(string procedureName,
        QueryParameterCollection parameters, string transactionId);

    long ReferenceCount(Guid entityId, object value, string transactionId);
    string EntityDdl(Guid entityId);
    string[] FieldDdl(Guid fieldId);
    string ExecuteSql(string command);
}