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
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Authorization;

public class PersistedGrantStore : IPersistedGrantStore
{
    private static readonly Guid OrigamIdentityGrantDataStructureId = new Guid(
        "ee21a554-9cd7-49bd-b989-4596d918af63"
    );
    private static readonly Guid GetGrandByKeyFilterId = new Guid(
        "12cffef2-6d1b-40d0-9e45-0e8b095c7248"
    );
    private static readonly Guid GetGrandBySubjectIdFilterId = new Guid(
        "32682bdb-191b-448a-8414-388c9b4a695b"
    );
    private static readonly Guid GetBySubjectIdClientIdFilterId = new Guid(
        "4a66c4da-f309-4aa5-93f1-9724e628fe12"
    );
    private static readonly Guid GetBySubjectIdClientIdTypeFilterId = new Guid(
        "d2ea1683-a049-4a26-bed2-1abcb7218ddf"
    );
    private static readonly string GrantTableName = "OrigamIdentityGrant";
    private static readonly string KeyParameterName = "OrigamIdentityGrant_parKey";
    private static readonly string SubjectIdParameterName = "OrigamIdentityGrant_parSubjectId";
    private static readonly string ClientIdParameterName = "OrigamIdentityGrant_parClientId";
    private static readonly string TypeParameterName = "OrigamIdentityGrant_parType";
    private static readonly string SessionIdParameterName = "OrigamIdentityGrant_sessionId";
    private readonly DataStructureQuery dataStructureQuery = new DataStructureQuery
    {
        DataSourceId = OrigamIdentityGrantDataStructureId,
        SynchronizeAttachmentsOnDelete = false,
        FireStateMachineEvents = false,
        LoadActualValuesAfterUpdate = false,
    };

    public Task StoreAsync(PersistedGrant grant)
    {
        IPersistenceProvider schemaProvider = ServiceManager
            .Services.GetService<IPersistenceService>()
            .SchemaProvider;
        DatasetGenerator dataSetGenerator = new DatasetGenerator(true);
        DataStructure dataStructure = (DataStructure)
            schemaProvider.RetrieveInstance(
                typeof(ISchemaItem),
                new ModelElementKey(OrigamIdentityGrantDataStructureId)
            );

        DataSet dataSet = dataSetGenerator.CreateDataSet(dataStructure);
        DataRow newRow = dataSet.Tables[GrantTableName].NewRow();
        newRow.SetField("Key", grant.Key);
        newRow.SetField("Type", grant.Type);
        newRow.SetField("SubjectId", grant.SubjectId);
        newRow.SetField("ClientId", grant.ClientId);
        newRow.SetField("CreationTime", grant.CreationTime);
        newRow.SetField("Expiration", grant.Expiration);
        newRow.SetField("Data", grant.Data);
        newRow.SetField("SessionId", grant.SessionId);

        dataSet.Tables[GrantTableName].Rows.Add(newRow);
        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);

        return Task.CompletedTask;
    }

    public Task<PersistedGrant> GetAsync(string key)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrandByKeyFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            KeyParameterName,
            key
        );
        return Task.FromResult(GrantsFromDataSet(dataSet).FirstOrDefault());
    }

    public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        QueryParameterCollection parameters = FilterToParameterCollection(filter);
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrandBySubjectIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return Task.FromResult(GrantsFromDataSet(dataSet));
    }

    private static QueryParameterCollection FilterToParameterCollection(PersistedGrantFilter filter)
    {
        QueryParameterCollection parameters = new QueryParameterCollection();
        if (!string.IsNullOrEmpty(filter.ClientId))
        {
            parameters.Add(new QueryParameter(ClientIdParameterName, filter.ClientId));
        }
        if (!string.IsNullOrEmpty(filter.SubjectId))
        {
            parameters.Add(new QueryParameter(SubjectIdParameterName, filter.SubjectId));
        }
        if (!string.IsNullOrEmpty(filter.Type))
        {
            parameters.Add(new QueryParameter(TypeParameterName, filter.Type));
        }
        if (!string.IsNullOrEmpty(filter.SessionId))
        {
            parameters.Add(new QueryParameter(SessionIdParameterName, filter.SessionId));
        }
        return parameters;
    }

    public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrandBySubjectIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            SubjectIdParameterName,
            subjectId
        );
        return Task.FromResult(GrantsFromDataSet(dataSet));
    }

    public Task RemoveAsync(string key)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrandByKeyFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            KeyParameterName,
            key
        );
        if (dataSet.Tables[GrantTableName].Rows.Count == 0)
        {
            throw new ArgumentException($"Grant {key} not found.");
        }
        dataSet.Tables[GrantTableName].Rows[0].Delete();
        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);
        return Task.CompletedTask;
    }

    public Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        QueryParameterCollection parameters = FilterToParameterCollection(filter);
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetBySubjectIdClientIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        foreach (DataRow row in dataSet.Tables[GrantTableName].Rows)
        {
            row.Delete();
        }
        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);
        return Task.CompletedTask;
    }

    public Task RemoveAllAsync(string subjectId, string clientId)
    {
        var parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter(SubjectIdParameterName, subjectId));
        parameters.Add(new QueryParameter(ClientIdParameterName, clientId));
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetBySubjectIdClientIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        foreach (DataRow row in dataSet.Tables[GrantTableName].Rows)
        {
            row.Delete();
        }

        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);

        return Task.CompletedTask;
    }

    public Task RemoveAllAsync(string subjectId, string clientId, string type)
    {
        var parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter(SubjectIdParameterName, subjectId));
        parameters.Add(new QueryParameter(ClientIdParameterName, clientId));
        parameters.Add(new QueryParameter(TypeParameterName, type));
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetBySubjectIdClientIdTypeFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        foreach (DataRow row in dataSet.Tables[GrantTableName].Rows)
        {
            row.Delete();
        }

        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);

        return Task.CompletedTask;
    }

    private static IEnumerable<PersistedGrant> GrantsFromDataSet(DataSet dataSet)
    {
        return dataSet
            .Tables[GrantTableName]
            .Rows.Cast<DataRow>()
            .Select(row => new PersistedGrant
            {
                Key = row["Key"] as string,
                Type = row["Type"] as string,
                SubjectId = row["SubjectId"] as string,
                ClientId = row["ClientId"] as string,
                CreationTime = (DateTime)row["CreationTime"],
                Expiration = (DateTime)row["Expiration"],
                Data = row["Data"] as string,
                SessionId = row["SessionId"] as string,
            });
    }
}
