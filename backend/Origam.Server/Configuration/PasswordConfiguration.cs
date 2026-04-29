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

using Microsoft.Extensions.Configuration;

namespace Origam.Server.Configuration;

public class PasswordConfiguration
{
    public bool RequireDigit { get; }
    public int RequiredLength { get; }
    public bool RequireNonAlphanumeric { get; }
    public bool RequireUppercase { get; }
    public bool RequireLowercase { get; }

    public PasswordConfiguration(IConfiguration configuration)
    {
        IConfigurationSection passwordSection = configuration.GetSection(key: "PasswordConfig");

        RequireDigit = passwordSection.GetValue(key: "RequireDigit", defaultValue: true);
        RequiredLength = passwordSection.GetValue(key: "RequiredLength", defaultValue: 10);
        RequireNonAlphanumeric = passwordSection.GetValue(
            key: "RequireNonAlphanumeric",
            defaultValue: true
        );
        RequireUppercase = passwordSection.GetValue(key: "RequireUppercase", defaultValue: true);
        RequireLowercase = passwordSection.GetValue(key: "RequireLowercase", defaultValue: true);
    }
}
