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

using System.Net;
using System.Text.RegularExpressions;

namespace Origam.AI.Agent.Services.Community;

internal sealed class CommunityHtmlToText
{
    private const string EmojiPathFragment = "/images/emoji/";
    private const string DefaultImageCaption = "image";

    private static readonly Regex ImageTagPattern = new(
        pattern: "<img[^>]*>",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex SourceAttributePattern = new(
        pattern: "src\\s*=\\s*\"([^\"]+)\"",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex AltAttributePattern = new(
        pattern: "alt\\s*=\\s*\"([^\"]*)\"",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex AttachmentMetaPattern = new(
        pattern: "<div class=\"meta\">.*?</div>",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline
    );

    private static readonly Regex ListItemPattern = new(
        pattern: "<li[^>]*>",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex BlockEndPattern = new(
        pattern: "</(p|div|li|ul|ol|h[1-6]|blockquote|tr|pre)>|<br\\s*/?>",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex TagPattern = new(
        pattern: "<[^>]+>",
        options: RegexOptions.Compiled
    );

    private static readonly Regex RepeatedBlankLinePattern = new(
        pattern: "\\n{3,}",
        options: RegexOptions.Compiled
    );

    private readonly string baseUrl;

    public CommunityHtmlToText(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public string Convert(string html)
    {
        var withoutAttachmentMeta = AttachmentMetaPattern.Replace(html, string.Empty);
        var withImageLinks = ImageTagPattern.Replace(
            withoutAttachmentMeta,
            match => DescribeImage(match.Value)
        );
        var withListMarkers = ListItemPattern.Replace(withImageLinks, replacement: "\n- ");
        var withLineBreaks = BlockEndPattern.Replace(withListMarkers, replacement: "\n");
        var withoutTags = TagPattern.Replace(withLineBreaks, string.Empty);
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return RepeatedBlankLinePattern
            .Replace(decoded.ReplaceLineEndings(replacementText: "\n"), replacement: "\n\n")
            .Trim();
    }

    private string DescribeImage(string imageTag)
    {
        var source = SourceAttributePattern.Match(imageTag);
        if (
            !source.Success
            || source.Groups[1].Value.Contains(EmojiPathFragment, StringComparison.Ordinal)
        )
        {
            return string.Empty;
        }

        var alt = AltAttributePattern.Match(imageTag);
        var caption =
            alt.Success && alt.Groups[1].Value.Trim().Length > 0
                ? alt.Groups[1].Value.Trim()
                : DefaultImageCaption;
        return $"![{caption}]({ToAbsoluteUrl(source.Groups[1].Value)})";
    }

    private string ToAbsoluteUrl(string url)
    {
        return url.StartsWith(value: "/", StringComparison.Ordinal) ? baseUrl + url : url;
    }
}
