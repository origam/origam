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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Origam.Server.Configuration;

namespace Origam.Server.OpenApi;

public class ExtensionControllerOpenApiPolicy
{
    private const string TagPrefix = "Custom API — ";
    private readonly HashSet<string> extensionAssemblyPaths;

    public ExtensionControllerOpenApiPolicy(StartUpConfiguration startUpConfiguration)
    {
        extensionAssemblyPaths = new HashSet<string>(GetPathComparer());
        foreach (string extensionDll in startUpConfiguration.ExtensionDlls)
        {
            extensionAssemblyPaths.Add(Path.GetFullPath(extensionDll));
        }
    }

    public bool IsDocumented(ApiDescription apiDescription)
    {
        return apiDescription.ActionDescriptor is ControllerActionDescriptor action
            && !string.IsNullOrWhiteSpace(apiDescription.HttpMethod)
            && extensionAssemblyPaths.Contains(
                Path.GetFullPath(action.ControllerTypeInfo.Assembly.Location)
            );
    }

    public string GetTagName(ApiDescription apiDescription)
    {
        string controllerName = apiDescription.ActionDescriptor.RouteValues.TryGetValue(
            key: "controller",
            value: out string name
        )
            ? name
            : "Controller";
        string displayName = Regex.Replace(
            input: controllerName,
            pattern: "(?<=[a-z0-9])(?=[A-Z])",
            replacement: " "
        );
        return TagPrefix + displayName;
    }

    private static StringComparer GetPathComparer()
    {
        return OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
    }
}
