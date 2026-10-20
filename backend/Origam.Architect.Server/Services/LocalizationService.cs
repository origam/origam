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

using Origam.DA.ObjectPersistence;
using Origam.OrigamEngine;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class LocalizationService(SchemaService schemaService)
{
    private readonly object _rebuildLock = new();

    public IReadOnlyList<string> Rebuild()
    {
        lock (_rebuildLock)
        {
            var package = schemaService.ActiveExtension;
            if (package == null)
            {
                throw new UserOrigamException(Strings.Localization_NoActivePackage);
            }

            var settings = ConfigurationManager.GetActiveConfiguration();
            var languages = (settings.TranslationBuilderLanguages ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();
            if (languages.Length == 0)
            {
                throw new UserOrigamException(Strings.Localization_NoLanguages);
            }

            string outputFolder = string.IsNullOrEmpty(settings.LocalizationFolder)
                ? Path.GetFullPath(Path.Combine(settings.ModelSourceControlLocation, "..", "l10n"))
                : settings.LocalizationFolder;
            Directory.CreateDirectory(outputFolder);

            var files = new List<string>();
            foreach (string language in languages)
            {
                string fileName = package.Name + "-" + language + ".xml";
                string outputPath = Path.Combine(outputFolder, fileName);
                using var currentTranslations = File.Exists(outputPath)
                    ? new LocalizationCache(outputPath)
                    : null;
                using var output = new MemoryStream();
                TranslationBuilder.Build(output, currentTranslations, language, package.Id);
                using var file = new FileStream(outputPath, FileMode.Create);
                output.Position = 0;
                output.CopyTo(file);
                files.Add(fileName);
            }
            return files;
        }
    }
}
