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

using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.Services.Xslt;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class XsltController(XsltService xsltService, ILogger<OrigamController> log)
    : OrigamController(log: log)
{
    [HttpPost(template: "Validate")]
    public IActionResult Validate([FromBody] XsltValidateModel model)
    {
        var input = new TransformationInput(
            SchemaItemId: model.SchemaItemId,
            SourceDataStructureId: model.SourceDataStructureId,
            TargetDataStructureId: model.TargetDataStructureId,
            RuleSetId: model.RuleSetId,
            InputXml: "<ROOT/>",
            Parameters: null
        );
        ValidationResult result = xsltService.Validate(input: input);
        return Ok(
            value: new ValidationResponse
            {
                Text = result.Text,
                Title = result.Title,
                Xml = result.Xml,
                Output = result.Output,
            }
        );
    }

    [HttpPost(template: "Transform")]
    public IActionResult Transform([FromBody] XsltTransformModel model)
    {
        var input = new TransformationInput(
            SchemaItemId: model.SchemaItemId,
            SourceDataStructureId: model.SourceDataStructureId,
            TargetDataStructureId: model.TargetDataStructureId,
            RuleSetId: model.RuleSetId,
            InputXml: model.InputXml,
            Parameters:
            [
                .. model.Parameters.Select(selector: x => new ParameterData(
                    name: x.Name,
                    type: x.Type,
                    textValue: x.Value
                )),
            ]
        );
        TransformationResult result = xsltService.Transform(input: input);
        return Ok(value: new TransformationResponse { Output = result.Output, Xml = result.Xml });
    }

    [HttpGet(template: "Parameters")]
    public IActionResult Parameters([FromQuery] Guid schemaItemId)
    {
        return Ok(value: xsltService.GetParameters(schemaItemId: schemaItemId));
    }

    [HttpGet(template: "Settings")]
    public IActionResult Settings()
    {
        return Ok(value: xsltService.GetSettings());
    }

    [HttpGet(template: "RuleSets")]
    public IActionResult RuleSets([FromQuery] Guid dataStructureId)
    {
        return Ok(value: xsltService.GetRuleSets(dataStructureId: dataStructureId));
    }
}
