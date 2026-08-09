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

namespace Origam.AI.Agent;

public static class PromptLibrary
{
    private const string ResourcePrefix = "Origam.AI.Agent.Prompts.";

    public static string Identity { get; } = Load("Identity.md");

    public static string ToolUse { get; } = Load("ToolUse.md");

    public static string ModelItems { get; } = Load("ModelItems.md");

    public static string ModelIndexHeader { get; } = Load("Context/ModelIndex.md");

    public static string FocusHeader { get; } = Load("Context/Focus.md");

    public static string SessionSummaryHeader { get; } = Load("Context/SessionSummary.md");

    private static string Load(string relativePath)
    {
        var resourceName = ResourcePrefix + relativePath.Replace(oldChar: '/', newChar: '.');
        using var stream = typeof(PromptLibrary).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                "Prompt resource not found: "
                    + resourceName
                    + ". Check that Prompts/"
                    + relativePath
                    + " exists and is included as an EmbeddedResource."
            );
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().ReplaceLineEndings("\n").TrimEnd();
    }
}
