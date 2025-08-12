#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controls;
using Origam.Architect.Server.Services;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.ControlAdapter;

public class ControlAdapterFactory(
    EditorPropertyFactory propertyFactory,
    SchemaService schemaService,
    IPersistenceService persistenceService,
    PropertyParser propertyParser
)
{
    public ControlAdapter Create(ControlSetItem controlSetItem)
    {
        string oldFullClassName = controlSetItem.ControlItem.ControlType;
        try
        {
            string className = oldFullClassName.Split(".").LastOrDefault();
            if (className == "PanelControlSet")
            {
                className = "AsPanel";
            }
            string newFullClassName = "Origam.Architect.Server.Controls." + className;
            Type controlType = Type.GetType(newFullClassName);
            if (controlType == null)
            {
                throw new Exception("Cannot find type: " + newFullClassName);
            }

            IControl control = Activator.CreateInstance(controlType) as IControl;
            return new ControlAdapter(
                controlSetItem,
                control,
                propertyFactory,
                schemaService,
                persistenceService,
                propertyParser
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Cannot find a form class for " + oldFullClassName, ex);
        }
    }
}
