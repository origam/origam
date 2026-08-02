using System.Text;
using System.Text.Json;

namespace Origam.AI.Function.Calling.Services;

public class ModelIndexService
{
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

    public async Task<string> GetYamlAsync(CancellationToken cancellationToken)
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
                return string.Empty;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var cards = JsonSerializer.Deserialize<List<EntityCard>>(json, JsonOptions) ?? new();

            lastError = null;
            return SerializeToYaml(cards);
        }
        catch (Exception ex)
        {
            lastError = $"{ex.GetType().Name}: {ex.Message}";
            return string.Empty;
        }
    }

    private string SerializeToYaml(List<EntityCard> cards)
    {
        if (cards.Count == 0)
        {
            return string.Empty;
        }

        var groupedByPackage = cards
            .GroupBy(card =>
                string.IsNullOrWhiteSpace(card.Package) ? "(no package)" : card.Package
            )
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        var builder = new StringBuilder();
        foreach (var group in groupedByPackage)
        {
            builder.Append("# ").AppendLine(group.Key);
            foreach (var card in group.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
            {
                aliasMappingService.Register(card.Id);
                var entityAlias = aliasMappingService.GetOrAddAlias(card.Id);
                var kindCode = card.Kind.StartsWith(
                    value: "Database",
                    comparisonType: StringComparison.OrdinalIgnoreCase
                )
                    ? "D"
                    : "V";

                builder
                    .Append(card.Name)
                    .Append('(')
                    .Append(entityAlias)
                    .Append(',')
                    .Append(kindCode)
                    .AppendLine(")");

                AppendFields(builder, card);
                AppendRelated(builder, label: "s", card.Structures);
                AppendRelated(builder, label: "c", card.Screens);
                AppendRelated(builder, label: "p", card.Panels);
                AppendRelated(builder, label: "l", card.Lookups);
                AppendRelated(builder, label: "q", card.WorkQueues);
            }
        }

        return builder.ToString();
    }

    private static void AppendFields(StringBuilder builder, EntityCard card)
    {
        if (card.Fields == null || card.Fields.Count == 0)
        {
            return;
        }

        var primaryKeys = new HashSet<string>(card.PrimaryKey, StringComparer.Ordinal);
        var visibleFields = card
            .Fields.Where(field => !AuditFields.Contains(field))
            .Select(field => primaryKeys.Contains(field) ? $"{field}*" : field)
            .ToList();

        if (visibleFields.Count == 0)
        {
            return;
        }

        builder.Append("f:[").Append(string.Join(separator: ",", visibleFields)).AppendLine("]");
    }

    private void AppendRelated(StringBuilder builder, string label, List<RelatedItem> items)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        builder.Append(label).Append(":[");
        var first = true;
        foreach (var item in items)
        {
            aliasMappingService.Register(item.Id);
            var alias = aliasMappingService.GetOrAddAlias(item.Id);
            if (!first)
            {
                builder.Append(',');
            }
            builder.Append(item.Name).Append('=').Append(alias);
            first = false;
        }
        builder.AppendLine("]");
    }

    private record EntityCard(
        string Id,
        string Name,
        string Kind,
        string Package,
        List<string> Fields,
        List<string> PrimaryKey,
        List<RelatedItem> Structures,
        List<RelatedItem> Screens,
        List<RelatedItem> Panels,
        List<RelatedItem> Lookups,
        List<RelatedItem> WorkQueues
    );

    private record RelatedItem(string Id, string Name);
}
