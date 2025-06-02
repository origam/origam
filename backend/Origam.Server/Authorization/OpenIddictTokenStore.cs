#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

/// <summary>
/// OpenIddict based token store.
/// </summary>
public class OpenIddictTokenStore : IOpenIddictTokenStore<OpenIddictToken>
{
    private static readonly Guid OrigamIdentityGrantDataStructureId = new Guid("ee21a554-9cd7-49bd-b989-4596d918af63");
    private static readonly Guid GetGrantByKeyFilterId = new Guid("12cffef2-6d1b-40d0-9e45-0e8b095c7248");
    private static readonly Guid GetGrantBySubjectIdFilterId = new Guid("32682bdb-191b-448a-8414-388c9b4a695b");
    private static readonly Guid GetBySubjectIdClientIdFilterId = new Guid("4a66c4da-f309-4aa5-93f1-9724e628fe12");
    private static readonly Guid GetBySubjectIdClientIdTypeFilterId = new Guid("d2ea1683-a049-4a26-bed2-1abcb7218ddf");
    private static readonly string GrantTableName = "OrigamIdentityGrant";
    private static readonly string KeyParameterName = "OrigamIdentityGrant_parKey";
    private static readonly string SubjectIdParameterName = "OrigamIdentityGrant_parSubjectId";
    private static readonly string ClientIdParameterName = "OrigamIdentityGrant_parClientId";
    private static readonly string TypeParameterName = "OrigamIdentityGrant_parType";
    private static readonly string SessionIdParameterName = "OrigamIdentityGrant_sessionId";

    private readonly DataStructureQuery dataStructureQuery = new()
    {
        DataSourceId = OrigamIdentityGrantDataStructureId,
        SynchronizeAttachmentsOnDelete = false,
        FireStateMachineEvents = false,
        LoadActualValuesAfterUpdate = false,
    };

    public Task CreateAsync(OpenIddictToken token, CancellationToken cancellationToken)
    {
        IPersistenceProvider schemaProvider = ServiceManager.Services
            .GetService<IPersistenceService>()
            .SchemaProvider;
        DatasetGenerator dataSetGenerator = new(true);
        DataStructure dataStructure = (DataStructure)schemaProvider.RetrieveInstance(typeof(ISchemaItem),
            new ModelElementKey(OrigamIdentityGrantDataStructureId));

        DataSet dataSet = dataSetGenerator.CreateDataSet(dataStructure);
        DataRow newRow = dataSet.Tables[GrantTableName].NewRow();
        newRow.SetField("Key", token.Id);
        newRow.SetField("Type", token.Type);
        newRow.SetField("SubjectId", token.Subject);
        newRow.SetField("ClientId", token.ClientId);
        newRow.SetField("CreationTime", token.CreationDate.UtcDateTime);
        newRow.SetField("Expiration", token.ExpirationDate?.UtcDateTime);
        newRow.SetField("Data", token.Payload);
        newRow.SetField("SessionId", token.ReferenceId);

        dataSet.Tables[GrantTableName].Rows.Add(newRow);
        DataService.Instance.StoreData(dataStructureQuery, dataSet, null);

        return Task.CompletedTask;
    }

    public Task<OpenIddictToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrantByKeyFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            KeyParameterName,
            identifier);

        return Task.FromResult(GrantsFromDataSet(dataSet).FirstOrDefault());
    }

    public async Task DeleteAsync(OpenIddictToken token, CancellationToken cancellationToken)
    {
        await RemoveAsync(token.Id, cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrantByKeyFilterId,
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

    public IAsyncEnumerable<OpenIddictToken> FindAsync(string subject, string client, CancellationToken cancellationToken)
    {
        var filter = new OpenIddictTokenFilter
        {
            SubjectId = subject,
            ClientId = client
        };
        return Task.FromResult(GetAllAsync(filter)).Result.ToAsyncEnumerable();
    }

    public Task<IEnumerable<OpenIddictToken>> GetAllAsync(OpenIddictTokenFilter filter)
    {
        QueryParameterCollection parameters = FilterToParameterCollection(filter);
        DataSet dataSet = DataService.Instance.LoadData(
            OrigamIdentityGrantDataStructureId,
            GetGrantBySubjectIdFilterId,
            Guid.Empty,
            Guid.Empty,
            null,
            parameters
        );
        return Task.FromResult(GrantsFromDataSet(dataSet));
    }

    private static QueryParameterCollection FilterToParameterCollection(OpenIddictTokenFilter filter)
    {
        QueryParameterCollection parameters = new();
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

    private static IEnumerable<OpenIddictToken> GrantsFromDataSet(DataSet dataSet)
    {
        return dataSet.Tables[GrantTableName].Rows
            .Cast<DataRow>()
            .Select(
                row =>
                    new OpenIddictToken
                    {
                        Id = row["Key"] as string ?? string.Empty,
                        Type = row["Type"] as string ?? string.Empty,
                        Subject = row["SubjectId"] as string ?? string.Empty,
                        ClientId = row["ClientId"] as string ?? string.Empty,
                        CreationDate = ((DateTime)row["CreationTime"]).ToUniversalTime(),
                        ExpirationDate = row["Expiration"] as DateTime?,
                        Payload = row["Data"] as string ?? string.Empty,
                        ReferenceId = row["SessionId"] as string ?? string.Empty,
                    }
            );
    }

    #region NotImplementedMembers
    public IQueryable<OpenIddictToken> Tokens => throw new NotImplementedException();

    public ValueTask<long> CountAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<OpenIddictToken>, IQueryable<TResult>> query, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<OpenIddictToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken) => throw new NotImplementedException();

    public IAsyncEnumerable<OpenIddictToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken) => throw new NotImplementedException();

    public IAsyncEnumerable<OpenIddictToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<string> GetApplicationIdAsync(OpenIddictToken token, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<string> GetAuthorizationIdAsync(OpenIddictToken token, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<DateTimeOffset?> GetCreationDateAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.CreationDate);

    public ValueTask<DateTimeOffset?> GetExpirationDateAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.ExpirationDate);

    public ValueTask<string> GetIdAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.Id);

    public ValueTask<string> GetPayloadAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.Payload);

    public ValueTask<string> GetReferenceIdAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.ReferenceId);

    public ValueTask<string> GetStatusAsync(OpenIddictToken token, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask<string> GetSubjectAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.Subject);

    public ValueTask<string> GetTypeAsync(OpenIddictToken token, CancellationToken cancellationToken) => new(token.Type);

    public ValueTask SetApplicationIdAsync(OpenIddictToken token, string identifier, CancellationToken cancellationToken)
    {
        token.ClientId = identifier;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetAuthorizationIdAsync(OpenIddictToken token, string identifier, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask SetCreationDateAsync(OpenIddictToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        token.CreationDate = date ?? DateTimeOffset.UtcNow;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetExpirationDateAsync(OpenIddictToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        token.ExpirationDate = date;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetPayloadAsync(OpenIddictToken token, string payload, CancellationToken cancellationToken)
    {
        token.Payload = payload;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetReferenceIdAsync(OpenIddictToken token, string identifier, CancellationToken cancellationToken)
    {
        token.ReferenceId = identifier;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetStatusAsync(OpenIddictToken token, string status, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask SetSubjectAsync(OpenIddictToken token, string subject, CancellationToken cancellationToken)
    {
        token.Subject = subject;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetTypeAsync(OpenIddictToken token, string type, CancellationToken cancellationToken)
    {
        token.Type = type;
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateAsync(OpenIddictToken token, CancellationToken cancellationToken) => throw new NotImplementedException();

    public ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken) => throw new NotImplementedException();
    #endregion
}

internal class OpenIddictToken
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset CreationDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
}

internal class OpenIddictTokenFilter
{
    public string? ClientId { get; set; }
    public string? SubjectId { get; set; }
    public string? Type { get; set; }
    public string? SessionId { get; set; }
}
