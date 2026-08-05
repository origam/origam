using System.ComponentModel;
using Origam.AI.Function.Calling.Services;

namespace Origam.AI.Function.Calling.Plugins;

public class SchemaExplorationPlugin
{
    private readonly HttpClient _httpClient;
    private readonly AliasMappingService _aliasMappingService;
    private readonly YamlSchemaSerializer _yamlSerializer;
    private readonly string _baseUrl;

    public SchemaExplorationPlugin(
        IHttpClientFactory httpClientFactory,
        AliasMappingService aliasMappingService,
        YamlSchemaSerializer yamlSerializer,
        IConfiguration configuration
    )
    {
        _httpClient = httpClientFactory.CreateClient("architect");
        _aliasMappingService = aliasMappingService;
        _yamlSerializer = yamlSerializer;
        _baseUrl = configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
    }

    [Description(
        "Gets the detailed YAML schema of a specific node/entity to explore its fields, filters, and relations. Use this before modifying an entity to understand its structure. Pass the entity's alias (e.g. e_g7f2)."
    )]
    public async Task<string> ExploreNodeAsync(
        [Description("The short alias of the node to explore.")] string alias,
        CancellationToken cancellationToken
    )
    {
        string uuid;
        try
        {
            uuid = _aliasMappingService.ResolveUuid(alias);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        var response = await _httpClient.GetAsync(
            $"{_baseUrl}/Model/GetSchemaNodeDetails?id={uuid}&depth=3",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return $"Error: Backend returned {response.StatusCode}";
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var yaml = _yamlSerializer.SerializeFromJson(json);

        return string.IsNullOrWhiteSpace(yaml) ? "Node has no details or is empty." : yaml;
    }

    [Description(
        "Searches the entire Origam schema horizontally for elements matching a specific query string. Returns a compact list of aliases and paths."
    )]
    public async Task<string> SearchSchemaAsync(
        [Description("The search query or pattern to find (e.g. 'Translation', 'Active').")]
            string query,
        CancellationToken cancellationToken
    )
    {
        var response = await _httpClient.GetAsync(
            $"{_baseUrl}/Model/SearchSchema?query={Uri.EscapeDataString(query)}",
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            return $"Error: Backend returned {response.StatusCode}";
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var yaml = _yamlSerializer.SerializeFromJson(json);

        return string.IsNullOrWhiteSpace(yaml) ? "No matches found." : yaml;
    }
}
