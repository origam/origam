using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Origam.AI.Agent.Services;

public class AliasMappingService
{
    private const string DefaultPrefix = "n";
    private const int UuidDigitsUsed = 5;
    private const string Base36Digits = "0123456789abcdefghijklmnopqrstuvwxyz";

    private readonly ConcurrentDictionary<string, string> _aliasToUuid = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly ConcurrentDictionary<string, string> _uuidToAlias = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly object _registrationLock = new();

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
        if (_uuidToAlias.TryGetValue(normalizedUuid, out var existingAlias))
        {
            return existingAlias;
        }

        lock (_registrationLock)
        {
            if (_uuidToAlias.TryGetValue(normalizedUuid, out existingAlias))
            {
                return existingAlias;
            }

            var alias = BuildAlias(parsedGuid, prefix);
            while (
                _aliasToUuid.TryGetValue(alias, out var takenBy)
                && !string.Equals(takenBy, normalizedUuid, StringComparison.OrdinalIgnoreCase)
            )
            {
                alias += "x";
            }

            _aliasToUuid[alias] = normalizedUuid;
            _uuidToAlias[normalizedUuid] = alias;
            return alias;
        }
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

    private static string BuildAlias(Guid uuid, string prefix)
    {
        var digits = uuid.ToString("N").Substring(startIndex: 0, length: UuidDigitsUsed);
        var value = long.Parse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return $"{prefix}_{ToBase36(value)}";
    }

    private static string ToBase36(long value)
    {
        if (value == 0)
        {
            return "0";
        }

        var builder = new StringBuilder();
        while (value > 0)
        {
            builder.Insert(index: 0, value: Base36Digits[(int)(value % 36)]);
            value /= 36;
        }

        return builder.ToString();
    }
}
