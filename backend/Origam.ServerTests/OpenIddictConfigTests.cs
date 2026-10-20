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

using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Origam.Server.Configuration;

namespace Origam.ServerTests;

public class OpenIddictConfigTests
{
    [TestCase(null, 60)]
    [TestCase("15", 15)]
    [TestCase("120", 120)]
    public void AccessTokenExpirationDefaultsToOneHourAndCanBeConfigured(
        string configuredMinutes,
        int expectedMinutes
    )
    {
        var config = new OpenIddictConfig(CreateConfiguration(configuredMinutes));

        Assert.That(config.AccessTokenExpirationMinutes, Is.EqualTo(expectedMinutes));
    }

    [TestCase("0")]
    [TestCase("-1")]
    public void AccessTokenExpirationMustBePositive(string configuredMinutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpenIddictConfig(CreateConfiguration(configuredMinutes))
        );
    }

    private static IConfiguration CreateConfiguration(string configuredMinutes)
    {
        var settings = new Dictionary<string, string>
        {
            ["OpenIddictConfig:ClientApplicationTemplates:Configured"] = "true",
        };
        if (configuredMinutes != null)
        {
            settings["OpenIddictConfig:AccessTokenExpirationMinutes"] = configuredMinutes;
        }
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
