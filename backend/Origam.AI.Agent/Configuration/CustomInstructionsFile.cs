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

using Microsoft.Extensions.Options;

namespace Origam.AI.Agent.Configuration;

public sealed class CustomInstructionsFile
{
    private const string FileName = "Custom.md";
    private const string DefaultDirectoryName = "Prompts";

    private readonly string directory;

    public CustomInstructionsFile(IOptions<AiOptions> options)
    {
        var configuredPath = options.Value.PromptsPath;
        directory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, DefaultDirectoryName)
            : configuredPath;
    }

    public string Read()
    {
        var path = Path.Combine(directory, FileName);
        return File.Exists(path)
            ? File.ReadAllText(path).ReplaceLineEndings("\n").Trim()
            : string.Empty;
    }

    public void Write(string text)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, FileName),
            text.ReplaceLineEndings("\n").Trim() + "\n"
        );
    }
}
