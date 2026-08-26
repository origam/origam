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

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Origam.AI.Agent.Models;

namespace Origam.AI.Agent.Services;

public class AliasMappingService(PromptPack prompts)
{
    private const string DefaultPrefix = "n";
    private const int UuidDigitsUsed = 5;
    private const string Base36Digits = "0123456789abcdefghijklmnopqrstuvwxyz";

    private readonly ConcurrentDictionary<string, string> aliasToUuid = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly ConcurrentDictionary<string, string> uuidToAlias = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly object registrationLock = new();

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
        if (uuidToAlias.TryGetValue(normalizedUuid, out var existingAlias))
        {
            return existingAlias;
        }

        lock (registrationLock)
        {
            if (uuidToAlias.TryGetValue(normalizedUuid, out existingAlias))
            {
                return existingAlias;
            }

            var alias = BuildAlias(parsedGuid, prefix);
            while (
                aliasToUuid.TryGetValue(alias, out var takenBy)
                && !string.Equals(takenBy, normalizedUuid, StringComparison.OrdinalIgnoreCase)
            )
            {
                alias += "x";
            }

            aliasToUuid[alias] = normalizedUuid;
            uuidToAlias[normalizedUuid] = alias;
            return alias;
        }
    }

    public string ResolveUuid(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException(Strings.AliasEmpty);
        }

        if (Guid.TryParse(alias, out var parsedGuid))
        {
            return parsedGuid.ToString("D");
        }

        if (aliasToUuid.TryGetValue(alias, out var uuid))
        {
            return uuid;
        }

        throw new InvalidOperationException(string.Format(prompts.UnknownAlias, alias));
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
