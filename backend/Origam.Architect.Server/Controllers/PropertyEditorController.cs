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

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PropertyEditorController(
    PropertyEditorService propertyService,
    EditorService editorService
) : ControllerBase
{
    [HttpPost("Update")]
    public UpdatePropertiesResult Update([FromBody] ChangesModel changes)
    {
        EditorData editor = editorService.ChangesToEditorData(changes);
        PropertyInfo[] properties = editor.Item.GetType().GetProperties();
        IEnumerable<PropertyUpdate> propertyUpdates = propertyService
            .GetEditorProperties(editor.Item)
            .Select(editorProperty =>
            {
                PropertyInfo property = properties.FirstOrDefault(x =>
                    x.Name == editorProperty.Name
                );
                return new PropertyUpdate
                {
                    PropertyName = editorProperty.Name,
                    Errors = propertyService.GetRuleErrors(property, editor.Item),
                    DropDownValues = editorProperty.DropDownValues ?? Array.Empty<DropDownValue>(),
                };
            });
        return new UpdatePropertiesResult
        {
            PropertyUpdates = propertyUpdates,
            IsDirty = editor.IsDirty,
        };
    }
}
