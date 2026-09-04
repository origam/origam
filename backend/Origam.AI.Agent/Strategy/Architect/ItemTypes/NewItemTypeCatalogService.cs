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
using Origam.AI.Agent.Models.Requests;
using Origam.AI.Agent.Strategy.Architect.Api;

namespace Origam.AI.Agent.Strategy.Architect.ItemTypes;

public class NewItemTypeCatalogService
{
    private static readonly string[] AlwaysInScopeCaptions = ["Database Entity"];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ArchitectApiClient architectApi;
    private readonly ArchitectPromptPack prompts;
    private readonly SemaphoreSlim buildLock = new(initialCount: 1, maxCount: 1);

    private ItemTypeCatalog? cachedCatalog;
    private string? lastError;

    public NewItemTypeCatalogService(ArchitectApiClient architectApi, ArchitectPromptPack prompts)
    {
        this.architectApi = architectApi;
        this.prompts = prompts;
    }

    public string? LastError => lastError;

    public ItemTypeCatalog? CachedCatalog => cachedCatalog;

    public async Task<ItemTypePromptSections> GetPromptSectionsAsync(
        ChatFocus? focus,
        CancellationToken cancellationToken
    )
    {
        ItemTypeCatalog? catalog = await GetCatalogAsync(cancellationToken);
        return catalog is null
            ? ItemTypePromptSections.Empty
            : new ItemTypePromptSections(
                Types: RenderTypes(catalog),
                Properties: RenderProperties(catalog, focus)
            );
    }

    private async Task<ItemTypeCatalog?> GetCatalogAsync(CancellationToken cancellationToken)
    {
        await buildLock.WaitAsync(cancellationToken);
        try
        {
            cachedCatalog ??= await FetchCatalogAsync(cancellationToken);
            return cachedCatalog;
        }
        finally
        {
            buildLock.Release();
        }
    }

    private async Task<ItemTypeCatalog?> FetchCatalogAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await architectApi.GetItemTypeCatalogAsync(cancellationToken);
            if (!response.IsSuccess)
            {
                lastError = $"Architect returned {(int)response.StatusCode} for ItemTypeCatalog.";
                return null;
            }

            var catalog = JsonSerializer.Deserialize<ItemTypeCatalog>(response.Body, JsonOptions);
            if (catalog is null || catalog.Types.Count == 0)
            {
                lastError = "Architect returned an empty item type catalog.";
                return null;
            }

            lastError = null;
            return catalog;
        }
        catch (Exception exception)
        {
            lastError = $"{exception.GetType().Name}: {exception.Message}";
            return null;
        }
    }

    private string RenderTypes(ItemTypeCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine(prompts.ItemTypesHeader);

        var existingCounts = catalog
            .Types.GroupBy(type => type.Caption, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Max(type => type.ExistingCount),
                StringComparer.OrdinalIgnoreCase
            );

        foreach (var provider in catalog.Providers.Where(provider => provider.Children.Count > 0))
        {
            builder.AppendLine(
                $"Under {provider.Name} (nodeId: {provider.Id}): "
                    + DescribeChildren(provider.Children, existingCounts)
            );
        }

        foreach (var type in catalog.Types.Where(type => type.Children.Count > 0))
        {
            builder.AppendLine(
                $"Inside {type.Caption}: {DescribeChildren(type.Children, existingCounts)}"
            );
        }

        AppendRequiredProperties(builder, catalog);
        return builder.ToString();
    }

    private static string DescribeChildren(
        IReadOnlyList<string> children,
        IReadOnlyDictionary<string, int> existingCounts
    )
    {
        return string.Join(
            separator: ", ",
            children.Select(caption =>
                existingCounts.TryGetValue(caption, out int count) && count > 0
                    ? $"{caption} ({count})"
                    : caption
            )
        );
    }

    private void AppendRequiredProperties(StringBuilder builder, ItemTypeCatalog catalog)
    {
        var typesWithRequired = catalog
            .Types.Select(type =>
                (type.Caption, Required: type.Properties.Where(property => property.Required))
            )
            .Where(entry => entry.Required.Any())
            .OrderBy(entry => entry.Caption, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (typesWithRequired.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine(prompts.RequiredPropertiesHeader);

        foreach (var (caption, required) in typesWithRequired)
        {
            builder.AppendLine(
                $"{caption}: {string.Join(separator: ", ", required.Select(DescribeRequired))}"
            );
        }
    }

    private static string DescribeRequired(ItemTypeProperty property)
    {
        return string.IsNullOrWhiteSpace(property.CommonValue)
            ? Describe(property)
            : $"{Describe(property)} usually \"{property.CommonValue}\"";
    }

    private string RenderProperties(ItemTypeCatalog catalog, ChatFocus? focus)
    {
        var typesInScope = GetTypesInScope(catalog, focus);
        if (typesInScope.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine(prompts.SettablePropertiesHeader);

        foreach (var type in typesInScope)
        {
            builder.AppendLine(
                $"{type.Caption}: {string.Join(separator: ", ", type.Properties.Select(Describe))}"
            );
        }

        return builder.ToString();
    }

    private static IReadOnlyList<ItemType> GetTypesInScope(
        ItemTypeCatalog catalog,
        ChatFocus? focus
    )
    {
        var typesByCaption = catalog
            .Types.GroupBy(type => type.Caption, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase
            );

        var captionsInScope = new HashSet<string>(
            AlwaysInScopeCaptions.Concat(GetFocusedCaptions(focus)),
            StringComparer.OrdinalIgnoreCase
        );
        foreach (string caption in captionsInScope.ToList())
        {
            if (typesByCaption.TryGetValue(caption, out ItemType? scopedType))
            {
                captionsInScope.UnionWith(scopedType.Children);
            }
        }

        return captionsInScope
            .Select(caption =>
                typesByCaption.TryGetValue(caption, out ItemType? type) ? type : null
            )
            .Where(type => type is not null && type.Properties.Count > 0)
            .Select(type => type!)
            .OrderBy(type => type.Caption, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> GetFocusedCaptions(ChatFocus? focus)
    {
        if (focus is null)
        {
            return [];
        }

        var captions = new List<string?> { focus.ActiveEditor?.ItemTypeName };
        if (focus.OpenTabs is not null)
        {
            captions.AddRange(focus.OpenTabs.Select(tab => tab.ItemTypeName));
        }

        return captions
            .Where(caption => !string.IsNullOrWhiteSpace(caption))
            .Select(caption => caption!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string Describe(ItemTypeProperty property)
    {
        return property.Values.Count > 0
            ? $"{property.Name}[{string.Join(separator: "|", property.Values)}]"
            : $"{property.Name}({property.Type})";
    }
}
