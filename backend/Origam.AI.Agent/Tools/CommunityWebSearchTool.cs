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

using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Origam.AI.Agent.Configuration;
using Origam.AI.Agent.Extensions;
using Origam.AI.Agent.Invocation;
using Origam.AI.Agent.Models.Responses;
using Origam.AI.Agent.Services.Community;

namespace Origam.AI.Agent.Tools;

public class CommunityWebSearchTool
{
    public const string SectionName = "CommunityWebSearch";
    public const string HttpClientName = "community";

    private const string SearchFunctionName = "SearchCommunity";
    private const string ReadTopicFunctionName = "ReadCommunityTopic";
    private const int MaxSearchResults = 8;
    private const int MaxBlurbLength = 240;
    private const int MaxTopicCharacters = 10000;

    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly CommunityHtmlToText htmlToText;

    private CommunityWebSearchTool(IHttpClientFactory httpClientFactory, string baseUrl)
    {
        httpClient = httpClientFactory.CreateClient(HttpClientName);
        this.baseUrl = baseUrl;
        htmlToText = new CommunityHtmlToText(baseUrl);
    }

    public static IReadOnlyList<AITool> CreateTools(
        IHttpClientFactory httpClientFactory,
        CommunityOptions options
    )
    {
        var baseUrl = options.ResolveBaseUrl();
        if (baseUrl is null)
        {
            return Array.Empty<AITool>();
        }

        var tool = new CommunityWebSearchTool(httpClientFactory, baseUrl);
        return
        [
            AIFunctionFactory.Create(tool.SearchCommunityAsync, CreateOptions(SearchFunctionName)),
            AIFunctionFactory.Create(
                tool.ReadCommunityTopicAsync,
                CreateOptions(ReadTopicFunctionName)
            ),
        ];
    }

    public static SectionInfo? GetSectionInfo(CommunityOptions options, IReadOnlyList<string> tags)
    {
        return options.ResolveBaseUrl() is null
            ? null
            : new SectionInfo(
                SectionName,
                Description: "Searches and reads the public ORIGAM community forum for product concepts and how-to guidance.",
                Tags: tags,
                FunctionCount: 2,
                [
                    new SectionFunctionInfo(
                        SearchFunctionName,
                        Method: null,
                        Path: null,
                        ReadFunctionDescription(nameof(SearchCommunityAsync))
                    ),
                    new SectionFunctionInfo(
                        ReadTopicFunctionName,
                        Method: null,
                        Path: null,
                        ReadFunctionDescription(nameof(ReadCommunityTopicAsync))
                    ),
                ],
                EnabledByDefault: true
            );
    }

    private static string ReadFunctionDescription(string methodName)
    {
        return typeof(CommunityWebSearchTool)
                .GetMethod(methodName)
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description ?? string.Empty;
    }

    [Description(
        "Searches the public ORIGAM community forum for guides and discussions about how ORIGAM works."
    )]
    public async Task<string> SearchCommunityAsync(
        [Description("Search terms, for example 'create a simple API'.")] string query,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Error: query must not be empty.";
        }

        try
        {
            using var document = await FetchAsync(
                $"/search.json?q={Uri.EscapeDataString(query)}",
                cancellationToken
            );
            return FormatSearchResults(document.RootElement);
        }
        catch (Exception exception)
        {
            return $"Error: {exception.Message}";
        }
    }

    [Description(
        "Reads one topic of the ORIGAM community forum in full, as plain text. "
            + "Call it with a topic id returned by SearchCommunity, and only for the topics that look relevant. "
            + "Images inside posts are kept as markdown links, they are not loaded."
    )]
    public async Task<string> ReadCommunityTopicAsync(
        [Description("The topic id, for example 4292.")] int topicId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var document = await FetchAsync($"/t/{topicId}.json", cancellationToken);
            return FormatTopic(document.RootElement, topicId);
        }
        catch (Exception exception)
        {
            return $"Error: {exception.Message}";
        }
    }

    private static AIFunctionFactoryOptions CreateOptions(string name)
    {
        return new AIFunctionFactoryOptions
        {
            Name = name,
            AdditionalProperties = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [AliasArgumentResolver.ResolveAliasesProperty] = false,
            },
        };
    }

    private async Task<JsonDocument> FetchAsync(
        string relativeUrl,
        CancellationToken cancellationToken
    )
    {
        var response = await httpClient.GetAsync(baseUrl + relativeUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"{baseUrl} returned {(int)response.StatusCode}.");
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private string FormatSearchResults(JsonElement root)
    {
        var topicTitles = new Dictionary<int, string>();
        var topicSlugs = new Dictionary<int, string>();
        if (root.TryGetProperty(propertyName: "topics", out var topics))
        {
            foreach (var topic in topics.EnumerateArray())
            {
                var topicId = topic.GetInt32OrNull(propertyName: "id");
                if (topicId is null)
                {
                    continue;
                }

                topicTitles[topicId.Value] = topic.GetStringOrNull(propertyName: "title") ?? "";
                topicSlugs[topicId.Value] = topic.GetStringOrNull(propertyName: "slug") ?? "";
            }
        }

        var lines = new List<string>();
        var seenTopics = new HashSet<int>();
        if (root.TryGetProperty(propertyName: "posts", out var posts))
        {
            foreach (var post in posts.EnumerateArray())
            {
                var topicId = post.GetInt32OrNull(propertyName: "topic_id");
                if (topicId is null || !seenTopics.Add(topicId.Value))
                {
                    continue;
                }

                lines.Add(
                    string.Join(
                        separator: " | ",
                        topicId.Value.ToString(),
                        topicTitles.GetValueOrDefault(topicId.Value, defaultValue: ""),
                        Shorten(post.GetStringOrNull(propertyName: "blurb") ?? "", MaxBlurbLength),
                        BuildTopicUrl(
                            topicId.Value,
                            topicSlugs.GetValueOrDefault(topicId.Value, defaultValue: "")
                        )
                    )
                );

                if (lines.Count == MaxSearchResults)
                {
                    break;
                }
            }
        }

        return lines.Count == 0 ? "No matches found." : string.Join(separator: "\n", lines);
    }

    private string FormatTopic(JsonElement root, int topicId)
    {
        var title = root.GetStringOrNull(propertyName: "title") ?? "";
        var slug = root.GetStringOrNull(propertyName: "slug") ?? "";

        var builder = new StringBuilder();
        builder.AppendLine(title);
        builder.AppendLine(BuildTopicUrl(topicId, slug));

        if (
            !root.TryGetProperty(propertyName: "post_stream", out var postStream)
            || !postStream.TryGetProperty(propertyName: "posts", out var posts)
        )
        {
            return builder.ToString().TrimEnd();
        }

        var truncated = false;
        foreach (var post in posts.EnumerateArray())
        {
            var author = post.GetStringOrNull(propertyName: "username") ?? "";
            var postNumber = post.GetInt32OrNull(propertyName: "post_number") ?? 0;
            var text = htmlToText.Convert(post.GetStringOrNull(propertyName: "cooked") ?? "");
            if (text.Length == 0)
            {
                continue;
            }

            var remaining = MaxTopicCharacters - builder.Length;
            if (remaining <= 0)
            {
                truncated = true;
                break;
            }

            if (text.Length > remaining)
            {
                text = text.Substring(startIndex: 0, length: remaining);
                truncated = true;
            }

            builder.AppendLine();
            builder.AppendLine($"#{postNumber} {author}:");
            builder.AppendLine(text);

            if (truncated)
            {
                break;
            }
        }

        if (truncated)
        {
            builder.AppendLine();
            builder.AppendLine(
                $"... topic truncated, read the rest at {BuildTopicUrl(topicId, slug)}"
            );
        }

        return builder.ToString().TrimEnd();
    }

    private string BuildTopicUrl(int topicId, string slug)
    {
        return slug.Length == 0 ? $"{baseUrl}/t/{topicId}" : $"{baseUrl}/t/{slug}/{topicId}";
    }

    private static string Shorten(string text, int maxLength)
    {
        var singleLine = text.ReplaceLineEndings(replacementText: " ").Trim();
        return singleLine.Length <= maxLength
            ? singleLine
            : singleLine.Substring(startIndex: 0, length: maxLength) + "...";
    }
}
