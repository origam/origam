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

using System.Text.Json;

namespace Origam.AI.Agent.Tests.Infrastructure;

public sealed record ChatFocusNode(
    string? Label,
    string? ItemTypeName,
    string? OrigamId,
    string Path
);

public sealed record ChatFocusPayload(IReadOnlyList<ChatFocusNode> VisibleNodes)
{
    public string Describe()
    {
        return $"{VisibleNodes.Count} visible nodes: "
            + string.Join(separator: ", ", VisibleNodes.Select(node => node.Label));
    }
}

public static class ChatFocusFactory
{
    private const int MaxVisibleNodes = 40;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<ChatFocusPayload> FromExpandedPathAsync(
        HttpClient architect,
        IReadOnlyList<string> expandedPath,
        CancellationToken cancellationToken
    )
    {
        var visibleNodes = new List<ChatFocusNode>();
        var topNodes = await ReadNodesAsync(
            architect,
            requestUri: "/Model/GetTopNodes",
            cancellationToken
        );

        await CollectAsync(
            architect,
            topNodes,
            expandedPath,
            depth: 0,
            path: string.Empty,
            visibleNodes,
            cancellationToken
        );

        return new ChatFocusPayload(visibleNodes);
    }

    private static async Task CollectAsync(
        HttpClient architect,
        IReadOnlyList<TreeNode> nodes,
        IReadOnlyList<string> expandedPath,
        int depth,
        string path,
        List<ChatFocusNode> visibleNodes,
        CancellationToken cancellationToken
    )
    {
        foreach (var node in nodes)
        {
            if (visibleNodes.Count >= MaxVisibleNodes)
            {
                return;
            }

            visibleNodes.Add(
                new ChatFocusNode(
                    node.NodeText,
                    node.ItemTypeName ?? node.NodeLevelType,
                    node.OrigamId,
                    path.Length == 0 ? "root" : path
                )
            );

            if (depth >= expandedPath.Count || node.NodeText != expandedPath[depth])
            {
                continue;
            }

            var children = await ReadChildrenAsync(architect, node, cancellationToken);
            if (children.Count == 0)
            {
                continue;
            }

            await CollectAsync(
                architect,
                children,
                expandedPath,
                depth + 1,
                path.Length == 0 ? node.NodeText : $"{path} / {node.NodeText}",
                visibleNodes,
                cancellationToken
            );
        }
    }

    private static async Task<IReadOnlyList<TreeNode>> ReadChildrenAsync(
        HttpClient architect,
        TreeNode node,
        CancellationToken cancellationToken
    )
    {
        if (node.Children is { Count: > 0 } inlineChildren)
        {
            return inlineChildren;
        }

        var requestUri =
            $"/Model/GetChildren?id={Uri.EscapeDataString(node.OrigamId ?? string.Empty)}"
            + $"&nodeText={Uri.EscapeDataString(node.NodeText ?? string.Empty)}"
            + $"&isNonPersistentItem={node.IsNonPersistentItem.ToString().ToLowerInvariant()}";

        return await ReadNodesAsync(architect, requestUri, cancellationToken);
    }

    private static async Task<IReadOnlyList<TreeNode>> ReadNodesAsync(
        HttpClient architect,
        string requestUri,
        CancellationToken cancellationToken
    )
    {
        var body = await architect.GetStringAsync(requestUri, cancellationToken);
        return JsonSerializer.Deserialize<List<TreeNode>>(body, JsonOptions) ?? [];
    }

    private sealed record TreeNode(
        string? OrigamId,
        string? NodeText,
        string? ItemTypeName,
        string? NodeLevelType,
        bool IsNonPersistentItem,
        IReadOnlyList<TreeNode>? Children
    );
}
