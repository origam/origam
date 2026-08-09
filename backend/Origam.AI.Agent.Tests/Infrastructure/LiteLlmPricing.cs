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

using System.Globalization;
using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure;

public sealed record ModelPrice(
    string ModelId,
    string Provider,
    decimal InputPerToken,
    decimal OutputPerToken,
    decimal CachedInputPerToken
)
{
    public decimal InputPerMillion => InputPerToken * 1_000_000m;

    public decimal OutputPerMillion => OutputPerToken * 1_000_000m;

    public decimal CachedInputPerMillion => CachedInputPerToken * 1_000_000m;

    public string Describe()
    {
        return $"{ModelId} ({Provider}) per 1M tokens: input ${Format(InputPerMillion)}, "
            + $"cached ${Format(CachedInputPerMillion)}, output ${Format(OutputPerMillion)}";
    }

    private static string Format(decimal ratePerMillion)
    {
        return ratePerMillion.ToString(format: "0.######", CultureInfo.InvariantCulture);
    }
}

public static class LiteLlmPricing
{
    public const string CatalogueUrl =
        "https://raw.githubusercontent.com/BerriAI/litellm/main/model_prices_and_context_window.json";

    private const string SampleSpecKey = "sample_spec";

    private static readonly SemaphoreSlim CatalogueGate = new(initialCount: 1, maxCount: 1);

    private static IReadOnlyDictionary<string, ModelPrice>? cachedCatalogue;

    public static async Task<IReadOnlyDictionary<string, ModelPrice>> GetCatalogueAsync(
        CancellationToken cancellationToken
    )
    {
        if (cachedCatalogue is not null)
        {
            return cachedCatalogue;
        }

        await CatalogueGate.WaitAsync(cancellationToken);
        try
        {
            cachedCatalogue ??= await DownloadCatalogueAsync(cancellationToken);
            return cachedCatalogue;
        }
        finally
        {
            CatalogueGate.Release();
        }
    }

    public static async Task<ModelPrice?> TryGetPriceAsync(
        string model,
        CancellationToken cancellationToken
    )
    {
        var catalogue = await GetCatalogueAsync(cancellationToken);
        foreach (var candidateId in CandidateModelIds(model))
        {
            if (catalogue.TryGetValue(candidateId, out var price))
            {
                return price;
            }
        }
        return null;
    }

    public static decimal EstimateCost(ModelPrice price, RunUsage usage)
    {
        var cachedTokens = Math.Clamp(value: usage.CachedTokens, min: 0, max: usage.PromptTokens);
        var freshInputTokens = usage.PromptTokens - cachedTokens;
        return (freshInputTokens * price.InputPerToken)
            + (cachedTokens * price.CachedInputPerToken)
            + (usage.CompletionTokens * price.OutputPerToken);
    }

    public static IEnumerable<string> CandidateModelIds(string model)
    {
        yield return model;

        var lastSeparator = model.LastIndexOf('/');
        if (lastSeparator >= 0 && lastSeparator < model.Length - 1)
        {
            yield return model[(lastSeparator + 1)..];
        }
    }

    private static async Task<IReadOnlyDictionary<string, ModelPrice>> DownloadCatalogueAsync(
        CancellationToken cancellationToken
    )
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        var body = await httpClient.GetStringAsync(CatalogueUrl, cancellationToken);

        using var document = JsonDocument.Parse(body);
        var prices = new Dictionary<string, ModelPrice>(StringComparer.Ordinal);
        foreach (var entry in document.RootElement.EnumerateObject())
        {
            if (entry.Name == SampleSpecKey || entry.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var inputPerToken = ReadRate(entry.Value, propertyName: "input_cost_per_token");
            if (inputPerToken <= 0m)
            {
                continue;
            }

            prices[entry.Name] = new ModelPrice(
                entry.Name,
                ReadProvider(entry.Value),
                inputPerToken,
                ReadRate(entry.Value, propertyName: "output_cost_per_token"),
                ReadRate(entry.Value, propertyName: "cache_read_input_token_cost")
            );
        }
        return prices;
    }

    private static string ReadProvider(JsonElement entry)
    {
        return entry.TryGetProperty(propertyName: "litellm_provider", out var provider)
            ? provider.GetString() ?? ""
            : "";
    }

    private static decimal ReadRate(JsonElement entry, string propertyName)
    {
        if (
            !entry.TryGetProperty(propertyName, out var value)
            || value.ValueKind != JsonValueKind.Number
        )
        {
            return 0m;
        }

        return value.TryGetDecimal(out var rate) ? rate : (decimal)value.GetDouble();
    }
}
