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

using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class PanelControlFactory(SchemaService schemaService, TabService tabService)
{
    private const string PanelControlType = "Origam.Gui.Win.AsPanel";

    public void Create(PanelControlSet panelControlSet, Guid packageId)
    {
        ControlItem control = schemaService
            .GetProvider<UserControlSchemaItemProvider>()
            .NewItem<ControlItem>(packageId, group: null);
        control.Name = panelControlSet.Name;
        control.IsComplexType = true;
        control.ControlType = typeof(PanelControlSet).ToString();
        control.ControlNamespace = typeof(PanelControlSet).Namespace;
        control.PanelControlSet = panelControlSet;
        control.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
        var ancestor = new SchemaItemAncestor
        {
            SchemaItem = control,
            Ancestor = tabService.GetControlByType(PanelControlType),
            PersistenceProvider = control.PersistenceProvider,
        };
        control.ThrowEventOnPersist = false;
        control.Persist();
        ancestor.Persist();
        control.ThrowEventOnPersist = true;
    }
}
