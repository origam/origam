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

namespace Origam.AI.Agent.Services;

public record ItemTypeProperty(
    string Name,
    string Type,
    IReadOnlyList<string> Values,
    bool Required,
    int SetOnExisting,
    string? CommonValue
);

public record ItemType(
    string Caption,
    string TypeName,
    string? FolderName,
    IReadOnlyList<string> Children,
    IReadOnlyList<ItemTypeProperty> Properties,
    int ExistingCount
);

public record ItemTypeProvider(string Id, string Name, IReadOnlyList<string> Children);

public record ItemTypeCatalog(
    IReadOnlyList<ItemTypeProvider> Providers,
    IReadOnlyList<ItemType> Types
);

public record ItemTypePromptSections(string Types, string Properties)
{
    public static readonly ItemTypePromptSections Empty = new(
        Types: string.Empty,
        Properties: string.Empty
    );
}

public class NewItemTypeCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] AlwaysInScopeCaptions = ["Database Entity"];

    private readonly IHttpClientFactory httpClientFactory;
    private readonly string architectBaseUrl;
    private readonly SemaphoreSlim buildLock = new(initialCount: 1, maxCount: 1);

    private ItemTypeCatalog? cachedCatalog;
    private string? lastError;

    public NewItemTypeCatalogService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        this.httpClientFactory = httpClientFactory;
        architectBaseUrl =
            configuration.GetSection("Architect")["BaseUrl"] ?? "https://localhost:7099";
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
        if (cachedCatalog is not null)
        {
            return cachedCatalog;
        }

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
            var httpClient = httpClientFactory.CreateClient("architect");
            var response = await httpClient.GetAsync(
                $"{architectBaseUrl}/ItemTypeCatalog/Get",
                cancellationToken
            );
            if (!response.IsSuccessStatusCode)
            {
                lastError = $"Architect returned {(int)response.StatusCode} for ItemTypeCatalog.";
                return null;
            }

            var catalog = await response.Content.ReadFromJsonAsync<ItemTypeCatalog>(
                JsonOptions,
                cancellationToken
            );
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

    private static string RenderTypes(ItemTypeCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("## ITEM TYPES YOU CAN CREATE");
        builder.AppendLine(
            "The complete list of what can be created under what. Pass the caption from this "
                + "list to CreateNode as newTypeName (for example 'Database Field'); the server "
                + "resolves it against the parent you named, so you do NOT need a GetMenuItems "
                + "call first. 'Under X' is a top-level model folder and the nodeId printed "
                + "beside it is what you pass to CreateNode when you create straight into that "
                + "folder - copy it character for character, it is a type name rather than a "
                + "GUID. Do not take that id out of GetTopNodes or GetChildren: their 'id' field "
                + "is a display key that CreateNode rejects, and only the value printed here "
                + "works. When the user names a sub-folder instead ('in the Dimensions folder'), "
                + "that folder is an ordinary item with a GUID - take its id from FOCUS when it "
                + "is listed there and otherwise look it up with SearchSchemaAsync, then pass "
                + "that GUID as nodeId. 'Inside X' is an item of that type, and there nodeId is "
                + "the id of the concrete item you are adding to. The Architect tree also shows "
                + "grouping folders (Fields, Filters, "
                + "Relationships and the like) - you do not need them, create directly under the "
                + "owning item and the new item lands in the right folder by itself. If a type is "
                + "not listed for the parent you have, it cannot be created there. The number "
                + "after a caption is how many items of that type this model already contains. "
                + "When several types could serve the same purpose, take the one this model "
                + "actually uses - a type with a handful of items is a specialised variant that "
                + "usually drags extra required objects along with it, and picking it when the "
                + "common one would do leaves you building things nobody asked for."
        );

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

    private static void AppendRequiredProperties(StringBuilder builder, ItemTypeCatalog catalog)
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
        builder.AppendLine("## PROPERTIES THAT MUST BE SET WHEN CREATING");
        builder.AppendLine(
            "Pass every property listed for the type in the same CreateNode call, each with a "
                + "real value - an empty string counts as not set and gets the item rejected just "
                + "as leaving it out does. When that happens the server throws the item away and "
                + "answers with discarded true, costing you the whole attempt. 'usually X' is the "
                + "value the existing items of that type in this model actually carry, so use it "
                + "unless the user asked for something else. A type that is not listed here has "
                + "no mandatory properties. This is the minimum, not the whole set - the create "
                + "response comes back with every property the item has, each tagged with its "
                + "kind, so read that instead of guessing what else to fill in. (reference) means "
                + "the id of an existing item, never a name or a literal like 'GET'."
        );

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

    private static string RenderProperties(ItemTypeCatalog catalog, ChatFocus? focus)
    {
        var typesInScope = GetTypesInScope(catalog, focus);
        if (typesInScope.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine("## PROPERTIES YOU CAN SET ON THESE ITEM TYPES");
        builder.AppendLine(
            "The properties you may set on these types, as name(kind) or name[allowed|values] "
                + "for enums. Pass them to CreateNode as changes: [{name, value}] and to the "
                + "update tool the same way; enums take the value NAME, not a number, and "
                + "(reference) properties take the id of an existing model item - look it up, "
                + "never invent it. Properties not listed here are not settable. For a type that "
                + "is not listed, create the item with Name only and read the 'props' the create "
                + "response returns - they carry the same information for that type."
        );

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

        var captions = new List<string?>
        {
            focus.ActiveNode?.ItemTypeName,
            focus.ActiveEditor?.ItemTypeName,
        };
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
