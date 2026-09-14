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

using Origam.AI.Agent.Services.OpenApi;

namespace Origam.AI.Agent.Strategy.Architect;

public static class ArchitectTargetOptions
{
    public static TargetOptions Create(Func<string> baseUrl)
    {
        return new TargetOptions
        {
            Name = ArchitectTargetStrategy.TargetName,
            BaseUrl = baseUrl,
            DefaultSections =
            [
                "Wizard",
                "Search",
                "Tab",
                "Model",
                "PropertyEditor",
                "CommunityWebSearch",
                "ItemTypeCatalog",
            ],
            SectionsOutOfBeta = new HashSet<string>(StringComparer.Ordinal) { "Model", "Wizard" },
            AdditionalSectionTags = new Dictionary<string, IReadOnlyList<string>>(
                StringComparer.Ordinal
            )
            {
                ["DeploymentScripts"] = new[] { OpenApiSectionProvider.UnstableTag },
                ["DeploymentScriptsGenerator"] = new[] { OpenApiSectionProvider.UnstableTag },
            },
            SectionsNeverExposedAsTools = new HashSet<string>(StringComparer.Ordinal)
            {
                OpenApiSectionProvider.AgentApiSectionName,
                "Test",
            },
            PathsNeverExposedAsTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/Model/GetSchemaItemInfos",
            },
            SectionDescriptions = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["DeploymentScripts"] = Strings.SectionDeploymentScriptsDescription,
                ["DeploymentScriptsGenerator"] =
                    Strings.SectionDeploymentScriptsGeneratorDescription,
                ["Documentation"] = Strings.SectionDocumentationDescription,
                ["ItemTypeCatalog"] = Strings.SectionItemTypeCatalogDescription,
                ["Model"] = Strings.SectionModelDescription,
                ["Package"] = Strings.SectionPackageDescription,
                ["PropertyEditor"] = Strings.SectionPropertyEditorDescription,
                ["ScreenEditor"] = Strings.SectionScreenEditorDescription,
                ["Search"] = Strings.SectionSearchDescription,
                ["SectionEditor"] = Strings.SectionSectionEditorDescription,
                ["Tab"] = Strings.SectionTabDescription,
                ["Wizard"] = Strings.SectionWizardDescription,
                ["Xslt"] = Strings.SectionXsltDescription,
            },
        };
    }
}
