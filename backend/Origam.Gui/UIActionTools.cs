#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Generic;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui;

public class UIActionTools
{
    public static bool GetValidActions(
        Guid formId,
        Guid panelId,
        bool disableActionButtons,
        Guid entityId,
        List<EntityUIAction> validActions
    )
    {
        bool hasMultipleSelection = false;
        if (entityId != Guid.Empty)
        {
            IPersistenceService ps =
                ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                as IPersistenceService;
            AbstractDataEntity entity = (AbstractDataEntity)
                ps.SchemaProvider.RetrieveInstance(
                    type: typeof(AbstractDataEntity),
                    primaryKey: new ModelElementKey(id: entityId)
                );
            var actionsSorted = entity.ChildItemsByTypeRecursive(
                itemType: EntityUIAction.CategoryConst
            );
            actionsSorted.Sort(comparer: new EntityUIActionOrderComparer());
            foreach (EntityUIAction action in actionsSorted)
            {
                if (
                    RenderTools.ShouldRenderAction(action: action, formId: formId, panelId: panelId)
                )
                {
                    if (
                        (action.Mode != PanelActionMode.ActiveRecord)
                        && (action.Mode != PanelActionMode.Always)
                    )
                    {
                        hasMultipleSelection = true;
                    }
                    if (disableActionButtons == false)
                    {
                        validActions.Add(item: action);
                    }
                }
            }
        }
        return hasMultipleSelection;
    }

    public static List<string> GetOriginalParameters(EntityUIAction action)
    {
        var originalDataParameters = new List<string>();
        foreach (
            var mapping in action.ChildItemsByType<EntityUIActionParameterMapping>(
                itemType: EntityUIActionParameterMapping.CategoryConst
            )
        )
        {
            if (mapping.Type == EntityUIActionParameterMappingType.Original)
            {
                originalDataParameters.Add(item: mapping.Name);
            }
        }
        return originalDataParameters;
    }

    public static EntityUIAction GetAction(string action)
    {
        return !Guid.TryParse(input: action, result: out Guid actionId)
            ? null
            : ServiceManager
                .Services.GetService<IPersistenceService>()
                .SchemaProvider.RetrieveInstance<EntityUIAction>(instanceId: actionId);
    }
}
