#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Server.Configuration;

public class ResourceOwnerPasswordAuthenticationProviderConfig
{
    public List<string> UrlsToBeAuthenticated { get; private set; } = new();
    public string AuthServerUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }

    public ResourceOwnerPasswordAuthenticationProviderConfig(IConfiguration configuration)
    {
        var section = configuration.GetSection("ResourceOwnerPasswordAuthenticationProviderConfig");
        if (!section.Exists())
        {
            return;
        }
        UrlsToBeAuthenticated = section
            .GetSectionOrThrow("UrlsToBeAuthenticated")
            .GetChildren()
            .Select(x => x.Value)
            .ToList();
        AuthServerUrl = section.GetStringOrThrow("AuthServerUrl");
        ClientId = section.GetStringOrThrow("ClientId");
        ClientSecret = section.GetStringOrThrow("ClientSecret");
        UserName = section.GetStringOrThrow("UserName");
        Password = section.GetStringOrThrow("Password");
    }
}
