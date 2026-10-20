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
            if (
                string.IsNullOrWhiteSpace(settings.TranslationBuilderLanguages)
                || settings
                    .TranslationBuilderLanguages.Split(TranslationBuilder.LanguageSeparator)
                    .All(string.IsNullOrWhiteSpace)
            )
            {
                throw new UserOrigamException(Strings.Localization_NoLanguages);
            }

            return TranslationBuilder.BuildFiles(settings, package.Name, package.Id);
        }
    }
}
