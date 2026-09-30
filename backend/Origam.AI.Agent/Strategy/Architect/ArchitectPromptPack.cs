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

using Origam.AI.Agent.Models;

namespace Origam.AI.Agent.Strategy.Architect;

public sealed class ArchitectPromptPack : PromptPack
{
    public const string PackName = "Architect";

    public ArchitectPromptPack()
        : base(PackName)
    {
        ToolUse = Load("ToolUse");
        Exploration = Load("Exploration");
        ModelItems = Load("ModelItems");
        ModelIndexHeader = Load("Context/ModelIndex");
        ModelIndexUpdatesHeader = Load("Context/ModelIndexUpdates");
        FocusHeader = Load("Context/Focus");
        ItemTypesHeader = Load("Context/ItemTypes");
        RequiredPropertiesHeader = Load("Context/RequiredProperties");
        SettablePropertiesHeader = Load("Context/SettableProperties");
        CreateNodeTypeRejected = Load("Messages/CreateNodeTypeRejected");
        CreateNodeEmptyRequired = Load("Messages/CreateNodeEmptyRequired");
        CreateNodeSuggestCommonValue = Load("Messages/CreateNodeSuggestCommonValue");
        CreateNodeSuggestAnyValue = Load("Messages/CreateNodeSuggestAnyValue");
    }

    public string ToolUse { get; }

    public string Exploration { get; }

    public string ModelItems { get; }

    public string ModelIndexHeader { get; }

    public string ModelIndexUpdatesHeader { get; }

    public string FocusHeader { get; }

    public string ItemTypesHeader { get; }

    public string RequiredPropertiesHeader { get; }

    public string SettablePropertiesHeader { get; }

    public string CreateNodeTypeRejected { get; }

    public string CreateNodeEmptyRequired { get; }

    public string CreateNodeSuggestCommonValue { get; }

    public string CreateNodeSuggestAnyValue { get; }
}
