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
                "/Model/GetEntityIndex",
            },
            SectionDescriptions = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["DeploymentScripts"] =
                    "Makes a deployment version current and runs its deployment scripts against the database.",
                ["DeploymentScriptsGenerator"] =
                    "Compares the model with the database and adds the differences to a deployment version or back into the model.",
                ["Documentation"] =
                    "Opens and edits the documentation attached to a model element.",
                ["ItemTypeCatalog"] =
                    "Lists the item types that can be created under a node and the properties each of them has.",
                ["Model"] =
                    "Browses the model tree, reads node details, searches the schema and deletes model elements.",
                ["Package"] = "Lists the packages of the model and switches the active one.",
                ["PropertyEditor"] = "Writes property values on the element.",
                ["ScreenEditor"] =
                    "Edits a screen opened in the designer: creates, updates and deletes the items on it.",
                ["Search"] =
                    "Finds model elements by text and shows what references them and what they depend on.",
                ["SectionEditor"] =
                    "Edits a screen section opened in the designer: creates, updates and deletes the items on it.",
                ["Tab"] =
                    "Opens, closes and saves editor tabs, and creates new model nodes inside them.",
                ["Wizard"] =
                    "Creates screens, lookups, menu items, work queue classes and filters through the Architect wizards.",
                ["Xslt"] =
                    "Validates and runs XSLT transformations and reads their parameters, settings and rule sets.",
            },
        };
    }
}
