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

public sealed record CustomInstructionsUpdate(string Text);

public static class CustomInstructionsFile
{
    private const string FileName = "Custom.md";

    private const string DefaultDirectoryName = "Prompts";

    public static string Read(IConfiguration configuration)
    {
        var path = Path.Combine(ResolveDirectory(configuration), FileName);
        return File.Exists(path)
            ? File.ReadAllText(path).ReplaceLineEndings("\n").Trim()
            : string.Empty;
    }

    public static void Write(IConfiguration configuration, string text)
    {
        var directory = ResolveDirectory(configuration);
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, FileName),
            text.ReplaceLineEndings("\n").Trim() + "\n"
        );
    }

    private static string ResolveDirectory(IConfiguration configuration)
    {
        var configuredPath = configuration.GetSection("Ai")["PromptsPath"];
        return string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, DefaultDirectoryName)
            : configuredPath;
    }
}
