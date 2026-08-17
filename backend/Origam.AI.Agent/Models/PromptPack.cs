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

namespace Origam.AI.Agent.Models;

public class PromptPack
{
    public const string SharedPackName = "Shared";

    private const string ResourcePrefix = "Origam.AI.Agent.Prompts.";

    private const string SectionMarker = "# ";

    private readonly string packName;
    private readonly IReadOnlyDictionary<string, string> packSections;
    private readonly IReadOnlyDictionary<string, string> sharedSections;

    public PromptPack(string packName)
    {
        this.packName = packName;
        packSections = ReadBundle(packName);
        sharedSections = packName == SharedPackName ? packSections : ReadBundle(SharedPackName);

        Replying = Load("Replying");
        CommunityWebSearch = Load("CommunityWebSearch");
        SessionSummaryHeader = Load("Context/SessionSummary");
        CustomInstructionsHeader = Load("Context/CustomInstructions");
        EmptyReply = Load("Messages/EmptyReply");
        StreamStalled = Load("Messages/StreamStalled");
        SessionSummarizerInstructions = Load("Messages/SessionSummarizer");
        UnknownAlias = Load("Messages/UnknownAlias");
    }

    public string Replying { get; }

    public string CommunityWebSearch { get; }

    public string SessionSummaryHeader { get; }

    public string CustomInstructionsHeader { get; }

    public string EmptyReply { get; }

    public string StreamStalled { get; }

    public string SessionSummarizerInstructions { get; }

    public string UnknownAlias { get; }

    protected string Load(string sectionName)
    {
        if (
            packSections.TryGetValue(sectionName, out var text)
            || sharedSections.TryGetValue(sectionName, out text)
        )
        {
            return text;
        }

        throw new InvalidOperationException(
            $"Prompt section '{sectionName}' is missing from bundle '{packName}.md' and from '{SharedPackName}.md'."
        );
    }

    private static IReadOnlyDictionary<string, string> ReadBundle(string pack)
    {
        var resourceName = ResourcePrefix + pack + ".md";
        using var stream = typeof(PromptPack).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Prompt bundle '{resourceName}' is missing.");
        }

        using var reader = new StreamReader(stream);
        return SplitIntoSections(reader.ReadToEnd().ReplaceLineEndings("\n"));
    }

    private static Dictionary<string, string> SplitIntoSections(string bundle)
    {
        var lines = bundle.Split('\n');
        var markerLines = Enumerable
            .Range(start: 0, lines.Length)
            .Where(index => lines[index].StartsWith(SectionMarker, StringComparison.Ordinal))
            .ToList();

        var sections = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var position = 0; position < markerLines.Count; position++)
        {
            var firstLine = markerLines[position] + 1;
            var lastLine =
                position + 1 < markerLines.Count ? markerLines[position + 1] : lines.Length;
            var sectionName = lines[markerLines[position]][SectionMarker.Length..].Trim();
            sections[sectionName] = string.Join(separator: '\n', lines[firstLine..lastLine])
                .TrimStart('\n')
                .TrimEnd();
        }

        return sections;
    }
}
