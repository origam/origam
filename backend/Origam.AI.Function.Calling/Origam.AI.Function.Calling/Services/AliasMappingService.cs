using System.Collections.Concurrent;

namespace Origam.AI.Function.Calling.Services;

public class AliasMappingService
{
    private const string DefaultPrefix = "n";
    private readonly ConcurrentDictionary<string, string> _aliasToUuid = new(
        StringComparer.OrdinalIgnoreCase
    );

    public string GetOrAddAlias(string uuid, string prefix = DefaultPrefix)
    {
        if (string.IsNullOrWhiteSpace(uuid))
        {
            return string.Empty;
        }

        if (!Guid.TryParse(uuid, out var parsedGuid))
        {
            return uuid;
        }

        var normalizedUuid = parsedGuid.ToString("D");
        var alias = BuildAlias(normalizedUuid, prefix);

        _aliasToUuid.AddOrUpdate(
            alias,
            addValue: normalizedUuid,
            updateValueFactory: (_, existing) => existing
        );

        return alias;
    }

    public string ResolveUuid(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Alias cannot be null or empty.");
        }

        if (Guid.TryParse(alias, out var parsedGuid))
        {
            return parsedGuid.ToString("D");
        }

        if (_aliasToUuid.TryGetValue(alias, out var uuid))
        {
            return uuid;
        }

        throw new InvalidOperationException(
            $"Alias '{alias}' is not known. It may reference a schema element that no longer exists. "
                + "Call SearchSchemaAsync or fetch the model index again to get valid aliases."
        );
    }

    public void Register(string uuid, string prefix = DefaultPrefix)
    {
        GetOrAddAlias(uuid, prefix);
    }

    private static string BuildAlias(string normalizedUuid, string prefix)
    {
        var shortPart = normalizedUuid.Substring(startIndex: 0, length: 8);
        return $"{prefix}_{shortPart}";
    }
}
