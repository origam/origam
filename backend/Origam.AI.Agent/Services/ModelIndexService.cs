using System.Text;
using System.Text.Json;

namespace Origam.AI.Agent.Services;

public record ModelIndexContent(string Index, string Updates);

public class ModelIndexService
{
    private const int MaxUpdatesLength = 8000;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

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

    private readonly IHttpClientFactory httpClientFactory;
    private readonly AliasMappingService aliasMappingService;
    private readonly string architectBaseUrl;
    private readonly SemaphoreSlim snapshotLock = new(initialCount: 1, maxCount: 1);

    private string snapshotYaml = string.Empty;
    private Dictionary<string, string>? snapshotEntries;
    private string? lastError;

    public ModelIndexService(
        IHttpClientFactory httpClientFactory,
        AliasMappingService aliasMappingService,
        IConfiguration configuration
    )
    {
        this.httpClientFactory = httpClientFactory;
        this.aliasMappingService = aliasMappingService;
        architectBaseUrl =
            configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
    }

    public string? LastError => lastError;

    public async Task<ModelIndexContent> GetContentAsync(CancellationToken cancellationToken)
    {
        var cards = await FetchCardsAsync(cancellationToken);
        if (cards is null)
        {
            return new ModelIndexContent(snapshotYaml, string.Empty);
        }

        await snapshotLock.WaitAsync(cancellationToken);
        try
        {
            var entries = RenderEntries(cards);

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
        builder.AppendLine("## MODEL INDEX UPDATES");
        builder.AppendLine(
            "The MODEL INDEX above is a snapshot. The entries below were added or changed after "
                + "it was taken and take precedence over it."
        );
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

    private async Task<List<EntityCard>?> FetchCardsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("architect");
            var response = await httpClient.GetAsync(
                $"{architectBaseUrl}/Model/GetEntityIndex",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                lastError =
                    $"Architect returned {(int)response.StatusCode} when fetching entity index.";
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            lastError = null;
            return JsonSerializer.Deserialize<List<EntityCard>>(json, JsonOptions) ?? new();
        }
        catch (Exception ex)
        {
            lastError = $"{ex.GetType().Name}: {ex.Message}";
            return null;
        }
    }

    private List<IndexEntry> RenderEntries(List<EntityCard> cards)
    {
        return cards
            .OrderBy(
                card => string.IsNullOrWhiteSpace(card.Package) ? "(no package)" : card.Package,
                StringComparer.OrdinalIgnoreCase
            )
            .ThenBy(card => card.Name, StringComparer.OrdinalIgnoreCase)
            .Select(RenderEntry)
            .ToList();
    }

    private IndexEntry RenderEntry(EntityCard card)
    {
        var entityAlias = aliasMappingService.GetOrAddAlias(card.Id, prefix: "e");
        var kindCode = card.Kind.StartsWith(
            value: "Database",
            comparisonType: StringComparison.OrdinalIgnoreCase
        )
            ? "D"
            : "V";

        var builder = new StringBuilder();
        builder
            .Append(card.Name)
            .Append('(')
            .Append(entityAlias)
            .Append(',')
            .Append(kindCode)
            .AppendLine(")");

        AppendFields(builder, card);
        AppendRelated(builder, label: "s", card.Structures, prefix: "s");
        AppendRelated(builder, label: "c", card.Screens, prefix: "c");
        AppendRelated(builder, label: "p", card.Panels, prefix: "p");
        AppendRelated(builder, label: "l", card.Lookups, prefix: "l");
        AppendRelated(builder, label: "q", card.WorkQueues, prefix: "q");

        var package = string.IsNullOrWhiteSpace(card.Package) ? "(no package)" : card.Package;
        return new IndexEntry(card.Id, package, builder.ToString());
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

    private void AppendFields(StringBuilder builder, EntityCard card)
    {
        if (card.Fields == null || card.Fields.Count == 0)
        {
            return;
        }

        var primaryKeyIds = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var primaryKeyField in card.PrimaryKey ?? new List<RelatedItem>())
        {
            primaryKeyIds[primaryKeyField.Name] = primaryKeyField.Id;
        }

        var visibleFields = card
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

    private record IndexEntry(string Id, string Package, string Text);

    private record EntityCard(
        string Id,
        string Name,
        string Kind,
        string Package,
        List<string> Fields,
        List<RelatedItem> PrimaryKey,
        List<RelatedItem> Structures,
        List<RelatedItem> Screens,
        List<RelatedItem> Panels,
        List<RelatedItem> Lookups,
        List<RelatedItem> WorkQueues
    );

    private record RelatedItem(string Id, string Name);
}
