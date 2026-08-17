using System.ComponentModel;
using Origam.AI.Agent.Services;
using Origam.AI.Agent.Strategy.Architect;
using Origam.AI.Agent.Strategy.Architect.Api;

namespace Origam.AI.Agent.Tools;

public class SchemaExplorationTool(
    ArchitectApiClient architectApi,
    AliasMappingService aliasMappingService,
    YamlSchemaSerializer yamlSerializer
)
{
    private const int ExploreDepth = 3;

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
            uuid = aliasMappingService.ResolveUuid(alias);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        var response = await architectApi.GetSchemaNodeDetailsAsync(
            uuid,
            ExploreDepth,
            cancellationToken
        );
        if (!response.IsSuccess)
        {
            return $"Error: Backend returned {response.StatusCode}";
        }

        var yaml = yamlSerializer.SerializeFromJson(response.Body);

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
        var response = await architectApi.SearchSchemaAsync(query, cancellationToken);
        if (!response.IsSuccess)
        {
            return $"Error: Backend returned {response.StatusCode}";
        }

        var yaml = yamlSerializer.SerializeFromJson(response.Body);

        return string.IsNullOrWhiteSpace(yaml) ? "No matches found." : yaml;
    }
}
