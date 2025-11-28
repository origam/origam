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
[Route("[controller]")]
public class XsltController(
    XsltService xsltService,
    IWebHostEnvironment environment,
    ILogger<OrigamController> log
) : OrigamController(log, environment)
{
    [HttpPost("Validate")]
    public IActionResult Validate([FromBody] XsltValidateModel model)
    {
        return RunWithErrorHandler(() =>
        {
            var input = new TransformationInput(
                SchemaItemId: model.SchemaItemId,
                SourceDataStructureId: model.SourceDataStructureId,
                TargetDataStructureId: model.TargetDataStructureId,
                RuleSetId: model.RuleSetId,
                InputXml: null,
                Parameters: null
            );
            ValidationResult result = xsltService.Validate(input);
            return Ok(result);
        });
    }

    [HttpPost("Transform")]
    public IActionResult Transform([FromBody] XsltTransformModel model)
    {
        return RunWithErrorHandler(() =>
        {
            var input = new TransformationInput(
                SchemaItemId: model.SchemaItemId,
                SourceDataStructureId: model.SourceDataStructureId,
                TargetDataStructureId: model.TargetDataStructureId,
                RuleSetId: model.RuleSetId,
                InputXml: model.InputXml,
                Parameters:
                [
                    .. model.Parameters.Select(x => new ParameterData(
                        name: x.Name,
                        type: x.Type,
                        textValue: x.Value
                    )),
                ]
            );
            TransformationResult result = xsltService.Transform(input);
            return Ok(result);
        });
    }

    [HttpGet("Parameters")]
    public IActionResult Parameters([FromQuery] Guid schemaItemId)
    {
        return RunWithErrorHandler(() => Ok(xsltService.GetParameters(schemaItemId)));
    }

    [HttpGet("Settings")]
    public IActionResult Settings()
    {
        return RunWithErrorHandler(() => Ok(xsltService.GetSettings()));
    }

    [HttpGet("RuleSets")]
    public IActionResult RuleSets([FromQuery] Guid dataStructureId)
    {
        return RunWithErrorHandler(() => Ok(xsltService.GetRuleSets(dataStructureId)));
    }
}
