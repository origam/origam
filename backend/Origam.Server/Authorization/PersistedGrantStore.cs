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
using System.Collections.Immutable;
using System.Text.Json;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Authorization;

#nullable enable

/// <summary>
/// Data model used by <see cref="PersistedGrantStore"/> to store OpenIddict tokens.
/// </summary>
public class OrigamToken
{
    public string Key { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public DateTime? Expiration { get; set; }
    public string Data { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// Filter used when deleting tokens.
/// </summary>
public class PersistedGrantFilter
{
    public string? ClientId { get; set; }
    public string? SubjectId { get; set; }
    public string? Type { get; set; }
    public string? SessionId { get; set; }
}

public class PersistedGrantStore : IOpenIddictTokenStore<OrigamToken>
{
    private static readonly Guid OrigamIdentityGrantDataStructureId = new Guid("ee21a554-9cd7-49bd-b989-4596d918af63");
    private static readonly Guid GetGrandByKeyFilterId = new Guid("12cffef2-6d1b-40d0-9e45-0e8b095c7248");
    private static readonly Guid GetGrandBySubjectIdFilterId = new Guid("32682bdb-191b-448a-8414-388c9b4a695b");
    private static readonly Guid GetBySubjectIdClientIdFilterId = new Guid("4a66c4da-f309-4aa5-93f1-9724e628fe12");
    private static readonly Guid GetBySubjectIdClientIdTypeFilterId = new Guid("d2ea1683-a049-4a26-bed2-1abcb7218ddf");
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
    public Task StoreAsync(OrigamToken grant)
    {
        IPersistenceProvider schemaProvider = ServiceManager.Services
            .GetService<IPersistenceService>()
            .SchemaProvider;
        DatasetGenerator dataSetGenerator = new DatasetGenerator(true);
        DataStructure dataStructure = (DataStructure)schemaProvider.RetrieveInstance(typeof(ISchemaItem), 
            new ModelElementKey(OrigamIdentityGrantDataStructureId));
        
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
    public Task<OrigamToken> GetAsync(string key)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId, 
            GetGrandByKeyFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            KeyParameterName,
            key);
        return Task.FromResult(GrantsFromDataSet(dataSet).FirstOrDefault());
    }
    public Task<IEnumerable<OrigamToken>> GetAllAsync(PersistedGrantFilter filter)
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
    public Task<IEnumerable<OrigamToken>> GetAllAsync(string subjectId)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId, 
            GetGrandBySubjectIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            SubjectIdParameterName,
            subjectId);
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
            key);
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
            parameters);
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
            parameters);
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
            parameters);
        foreach (DataRow row in dataSet.Tables[GrantTableName].Rows)
        {
            row.Delete();
        }
        
        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);
        
        return Task.CompletedTask;
    }
    private static IEnumerable<OrigamToken> GrantsFromDataSet(DataSet dataSet)
    {
        return dataSet.Tables[GrantTableName].Rows
            .Cast<DataRow>()
            .Select(
                row =>
                    new OrigamToken
                    {
                        Key = row["Key"] as string,
                        Type = row["Type"] as string,
                        SubjectId = row["SubjectId"] as string,
                        ClientId = row["ClientId"] as string,
                        CreationTime = (DateTime) row["CreationTime"],
                        Expiration = row["Expiration"] as DateTime?,
                        Data = row["Data"] as string,
                        SessionId = row["SessionId"] as string,
                    }
            );
    }

    #region IOpenIddictTokenStore implementation

    public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        var all = await GetAllAsync(new PersistedGrantFilter());
        return all.LongCount();
    }

    public ValueTask CreateAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask(StoreAsync(token));
    }

    public ValueTask DeleteAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask(RemoveAsync(token.Key));
    }

    public ValueTask DeleteAsync(string identifier, CancellationToken cancellationToken)
    {
        return new ValueTask(RemoveAsync(identifier));
    }

    public ValueTask<OrigamToken?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        return new ValueTask<OrigamToken?>(GetAsync(identifier));
    }

    public async IAsyncEnumerable<OrigamToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(subject);
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public ValueTask<string?> GetIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.Key);
    }

    public ValueTask<string?> GetApplicationIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.ClientId);
    }

    public ValueTask<string?> GetSubjectAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.SubjectId);
    }

    public ValueTask<string?> GetTypeAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.Type);
    }

    public ValueTask<DateTimeOffset?> GetExpirationDateAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<DateTimeOffset?>(token.Expiration);
    }

    public ValueTask<DateTimeOffset?> GetCreationDateAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<DateTimeOffset?>(token.CreationTime);
    }

    public ValueTask<string?> GetPayloadAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.Data);
    }

    public ValueTask<string?> GetReferenceIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.Key);
    }

    public ValueTask<string?> GetAuthorizationIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>((string?)null);
    }

    public ValueTask<DateTimeOffset?> GetRedemptionDateAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<DateTimeOffset?>((DateTimeOffset?)null);
    }

    public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary<string, JsonElement>.Empty);
    }

    public ValueTask<string?> GetStatusAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(null);
    }

    public ValueTask<string?> GetSessionIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(token.SessionId);
    }

    public ValueTask<bool> HasReferenceIdAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        return new ValueTask<bool>(!string.IsNullOrEmpty(token.Key));
    }

    public ValueTask<OrigamToken> InstantiateAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<OrigamToken>(new OrigamToken());
    }

    public async IAsyncEnumerable<OrigamToken> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter());
        foreach (var item in list.Skip(offset ?? 0).Take(count ?? int.MaxValue))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
        Func<IQueryable<OrigamToken>, TState, IQueryable<TResult>> query,
        TState state,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter());
        foreach (var item in query(list.AsQueryable(), state))
        {
            yield return item;
        }
    }

    public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<OrigamToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter());
        return query(list.AsQueryable()).LongCount();
    }

    public async ValueTask<TResult?> GetAsync<TResult>(Func<IQueryable<OrigamToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter());
        return query(list.AsQueryable()).FirstOrDefault();
    }

    public async ValueTask<TResult?> GetAsync<TState, TResult>(
        Func<IQueryable<OrigamToken>, TState, IQueryable<TResult>> query,
        TState state,
        CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter());
        return query(list.AsQueryable(), state).FirstOrDefault();
    }

    public ValueTask<OrigamToken?> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
    {
        return new ValueTask<OrigamToken?>(GetAsync(identifier));
    }

    public async IAsyncEnumerable<OrigamToken> FindByApplicationIdAsync(string applicationId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter { ClientId = applicationId });
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public IAsyncEnumerable<OrigamToken> FindByAuthorizationIdAsync(string authorizationId, CancellationToken cancellationToken)
    {
        return AsyncEnumerable.Empty<OrigamToken>();
    }

    public async IAsyncEnumerable<OrigamToken> FindAsync(string subject, string client, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter { SubjectId = subject, ClientId = client });
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<OrigamToken> FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter { SubjectId = subject, ClientId = client });
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<OrigamToken> FindAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
    {
        var list = await GetAllAsync(new PersistedGrantFilter { SubjectId = subject, ClientId = client, Type = type });
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public ValueTask SetApplicationIdAsync(OrigamToken token, string? applicationId, CancellationToken cancellationToken)
    {
        token.ClientId = applicationId ?? string.Empty;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetAuthorizationIdAsync(OrigamToken token, string? authorizationId, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SetCreationDateAsync(OrigamToken token, DateTimeOffset? creationDate, CancellationToken cancellationToken)
    {
        token.CreationTime = creationDate?.UtcDateTime ?? DateTime.UtcNow;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetExpirationDateAsync(OrigamToken token, DateTimeOffset? expirationDate, CancellationToken cancellationToken)
    {
        token.Expiration = expirationDate?.UtcDateTime;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetPayloadAsync(OrigamToken token, string? payload, CancellationToken cancellationToken)
    {
        token.Data = payload ?? string.Empty;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetReferenceIdAsync(OrigamToken token, string? identifier, CancellationToken cancellationToken)
    {
        token.Key = identifier ?? string.Empty;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetRedemptionDateAsync(OrigamToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SetPropertiesAsync(OrigamToken token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SetStatusAsync(OrigamToken token, string? status, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SetSubjectAsync(OrigamToken token, string? subject, CancellationToken cancellationToken)
    {
        token.SubjectId = subject ?? string.Empty;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetTypeAsync(OrigamToken token, string? type, CancellationToken cancellationToken)
    {
        token.Type = type ?? string.Empty;
        return ValueTask.CompletedTask;
    }

    public async ValueTask UpdateAsync(OrigamToken token, CancellationToken cancellationToken)
    {
        await RemoveAsync(token.Key);
        await StoreAsync(token);
    }

    public ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        // Tokens are removed when expired
        return new ValueTask();
    }

    #endregion
}
