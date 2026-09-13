#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Text;
using System.Text.Json;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Strategy.Architect.Api;

namespace Origam.AI.Agent.Strategy.Architect.ModelIndex;

public class ModelIndexService
{
    private record IndexEntry(string Id, string Package, string Text);

    private record SchemaItemInfo(
        string Id,
        string Name,
        string Kind,
        string Package,
        List<string> Fields,
        List<RelatedItem> PrimaryKey,
        List<RelatedItem> UsedBy
    );

    private record RelatedItem(string Id, string Name, string Kind);

    private const int MaxUpdatesLength = 8000;

    private static readonly HashSet<string> AuditFields = new(StringComparer.Ordinal)
    {
        "_mockPk",
        "Selected",
        "RecordCreated",
        "RecordCreatedBy",
        "RecordCreatedServer",
        "RecordUpdated",
        "RecordUpdatedBy",
        "RecordUpdatedServer",
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ArchitectApiClient architectApi;
    private readonly AliasMappingService aliasMappingService;
    private readonly ArchitectPromptPack prompts;
    private readonly SemaphoreSlim snapshotLock = new(initialCount: 1, maxCount: 1);

    private string snapshotYaml = string.Empty;
    private Dictionary<string, string>? snapshotEntries;
    private string? lastError;

    public ModelIndexService(
        ArchitectApiClient architectApi,
        AliasMappingService aliasMappingService,
        ArchitectPromptPack prompts
    )
    {
        this.architectApi = architectApi;
        this.aliasMappingService = aliasMappingService;
        this.prompts = prompts;
    }

    public string? LastError => lastError;

    public async Task<ModelIndexContent> GetContentAsync(CancellationToken cancellationToken)
    {
        var schemaItems = await FetchSchemaItemInfosAsync(cancellationToken);
        if (schemaItems is null)
        {
            return new ModelIndexContent(snapshotYaml, string.Empty);
        }

        await snapshotLock.WaitAsync(cancellationToken);
        try
        {
            var entries = RenderEntries(schemaItems);

            if (snapshotEntries is null)
            {
                TakeSnapshot(entries);
                return new ModelIndexContent(snapshotYaml, string.Empty);
            }

            string updates = BuildUpdates(entries);
            if (updates.Length > MaxUpdatesLength)
            {
                TakeSnapshot(entries);
                return new ModelIndexContent(snapshotYaml, string.Empty);
            }

            return new ModelIndexContent(snapshotYaml, updates);
        }
        finally
        {
            snapshotLock.Release();
        }
    }

    private void TakeSnapshot(List<IndexEntry> entries)
    {
        snapshotYaml = Compose(entries);
        snapshotEntries = entries.ToDictionary(
            entry => entry.Id,
            entry => entry.Text,
            StringComparer.OrdinalIgnoreCase
        );
    }

    private string BuildUpdates(List<IndexEntry> entries)
    {
        var changed = entries
            .Where(entry =>
                !snapshotEntries!.TryGetValue(entry.Id, out string? previous)
                || !string.Equals(previous, entry.Text, StringComparison.Ordinal)
            )
            .ToList();

        var currentIds = new HashSet<string>(
            entries.Select(entry => entry.Id),
            StringComparer.OrdinalIgnoreCase
        );
        var removed = snapshotEntries!
            .Keys.Where(id => !currentIds.Contains(id))
            .Select(id => aliasMappingService.GetOrAddAlias(id))
            .ToList();

        if (changed.Count == 0 && removed.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine(prompts.ModelIndexUpdatesHeader);
        if (changed.Count > 0)
        {
            builder.Append(Compose(changed));
        }
        if (removed.Count > 0)
        {
            builder
                .Append("deleted:[")
                .Append(string.Join(separator: ",", removed))
                .AppendLine("]");
        }

        return builder.ToString();
    }

    private async Task<List<SchemaItemInfo>?> FetchSchemaItemInfosAsync(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await architectApi.GetSchemaItemInfosAsync(cancellationToken);
            if (!response.IsSuccess)
            {
                lastError = string.Format(
                    Strings.SchemaItemInfosRequestFailed,
                    (int)response.StatusCode
                );
                return null;
            }

            lastError = null;
            return JsonSerializer.Deserialize<List<SchemaItemInfo>>(response.Body, JsonOptions)
                ?? new();
        }
        catch (Exception ex)
        {
            lastError = $"{ex.GetType().Name}: {ex.Message}";
            return null;
        }
    }

    private List<IndexEntry> RenderEntries(List<SchemaItemInfo> schemaItems)
    {
        return schemaItems
            .OrderBy(
                schemaItem =>
                    string.IsNullOrWhiteSpace(schemaItem.Package)
                        ? "(no package)"
                        : schemaItem.Package,
                StringComparer.OrdinalIgnoreCase
            )
            .ThenBy(schemaItem => schemaItem.Name, StringComparer.OrdinalIgnoreCase)
            .Select(RenderEntry)
            .ToList();
    }

    private IndexEntry RenderEntry(SchemaItemInfo schemaItem)
    {
        var entityAlias = aliasMappingService.GetOrAddAlias(schemaItem.Id, prefix: "e");
        var kindCode = schemaItem.Kind.StartsWith(
            value: "Database",
            comparisonType: StringComparison.OrdinalIgnoreCase
        )
            ? "D"
            : "V";

        var builder = new StringBuilder();
        builder
            .Append(schemaItem.Name)
            .Append('(')
            .Append(entityAlias)
            .Append(',')
            .Append(kindCode)
            .AppendLine(")");

        AppendFields(builder, schemaItem);
        foreach (var usersOfKind in (schemaItem.UsedBy ?? []).GroupBy(item => item.Kind))
        {
            AppendRelated(
                builder,
                label: usersOfKind.Key,
                usersOfKind.ToList(),
                prefix: AliasPrefix(usersOfKind.Key)
            );
        }

        var package = string.IsNullOrWhiteSpace(schemaItem.Package)
            ? "(no package)"
            : schemaItem.Package;
        return new IndexEntry(schemaItem.Id, package, builder.ToString());
    }

    private static string AliasPrefix(string kind)
    {
        return string.Concat(
            kind.Split(separator: ' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToLowerInvariant(word[0]))
        );
    }

    private static string Compose(List<IndexEntry> entries)
    {
        var builder = new StringBuilder();
        string? currentPackage = null;
        foreach (var entry in entries)
        {
            if (!string.Equals(currentPackage, entry.Package, StringComparison.Ordinal))
            {
                builder.Append("# ").AppendLine(entry.Package);
                currentPackage = entry.Package;
            }

            builder.Append(entry.Text);
        }

        return builder.ToString();
    }

    private void AppendFields(StringBuilder builder, SchemaItemInfo schemaItem)
    {
        if (schemaItem.Fields == null || schemaItem.Fields.Count == 0)
        {
            return;
        }

        var primaryKeyIds = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var primaryKeyField in schemaItem.PrimaryKey ?? new List<RelatedItem>())
        {
            primaryKeyIds[primaryKeyField.Name] = primaryKeyField.Id;
        }

        var visibleFields = schemaItem
            .Fields.Where(field => !AuditFields.Contains(field))
            .Select(field =>
                primaryKeyIds.TryGetValue(field, out string? primaryKeyId)
                    ? $"{field}*={aliasMappingService.GetOrAddAlias(primaryKeyId)}"
                    : field
            )
            .ToList();

        if (visibleFields.Count == 0)
        {
            return;
        }

        builder.Append("f:[").Append(string.Join(separator: ",", visibleFields)).AppendLine("]");
    }

    private void AppendRelated(
        StringBuilder builder,
        string label,
        List<RelatedItem> items,
        string prefix
    )
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        builder.Append(label).Append(":[");
        var first = true;
        foreach (var item in items)
        {
            var alias = aliasMappingService.GetOrAddAlias(item.Id, prefix);
            if (!first)
            {
                builder.Append(',');
            }
            builder.Append(item.Name).Append('=').Append(alias);
            first = false;
        }
        builder.AppendLine("]");
    }
}
