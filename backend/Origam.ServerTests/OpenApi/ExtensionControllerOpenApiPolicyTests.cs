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

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Origam.Server.Configuration;
using Origam.Server.Controller;
using Origam.Server.OpenApi;

namespace Origam.ServerTests.OpenApi;

public class ExtensionControllerOpenApiPolicyTests
{
    [Test]
    public void IncludesControllerFromConfiguredExtensionAssembly()
    {
        var policy = CreatePolicy(typeof(CustomExtensionController).Assembly.Location);

        bool isDocumented = policy.IsDocumented(
            CreateApiDescription(typeof(CustomExtensionController), httpMethod: "GET")
        );

        Assert.That(isDocumented, Is.True);
    }

    [Test]
    public void ExcludesControllerFromOrigamServerAssembly()
    {
        var policy = CreatePolicy(typeof(CustomExtensionController).Assembly.Location);

        bool isDocumented = policy.IsDocumented(
            CreateApiDescription(typeof(AboutController), httpMethod: "GET")
        );

        Assert.That(isDocumented, Is.False);
    }

    [Test]
    public void ExcludesActionWithoutHttpMethod()
    {
        var policy = CreatePolicy(typeof(CustomExtensionController).Assembly.Location);

        bool isDocumented = policy.IsDocumented(
            CreateApiDescription(typeof(CustomExtensionController), httpMethod: null)
        );

        Assert.That(isDocumented, Is.False);
    }

    [Test]
    public void UsesCustomApiControllerTag()
    {
        var policy = CreatePolicy(typeof(CustomExtensionController).Assembly.Location);

        string tagName = policy.GetTagName(
            CreateApiDescription(typeof(CustomExtensionController), httpMethod: "GET")
        );

        Assert.That(tagName, Is.EqualTo("Custom API — Custom Extension"));
    }

    private static ExtensionControllerOpenApiPolicy CreatePolicy(string extensionDll)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["ExtensionDlls:0"] = extensionDll }
            )
            .Build();
        return new ExtensionControllerOpenApiPolicy(new StartUpConfiguration(configuration));
    }

    private static ApiDescription CreateApiDescription(Type controllerType, string? httpMethod)
    {
        return new ApiDescription
        {
            HttpMethod = httpMethod,
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                RouteValues = new Dictionary<string, string?>
                {
                    ["controller"] = controllerType.Name.Replace(
                        oldValue: "Controller",
                        newValue: string.Empty
                    ),
                },
            },
        };
    }

    private sealed class CustomExtensionController { }
}
