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
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Origam.Extensions;

namespace Origam.Server.Configuration;

public class StartUpConfiguration
{
    private readonly IConfiguration configuration;

    public StartUpConfiguration(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public IEnumerable<string> UserApiPublicRoutes =>
        configuration
            .GetSection(key: "UserApiOptions")
            .GetSection(key: "PublicRoutes")
            .GetChildren()
            .Select(selector: c => c.Value);
    public IEnumerable<string> UserApiRestrictedRoutes =>
        configuration
            .GetSection(key: "UserApiOptions")
            .GetSection(key: "RestrictedRoutes")
            .GetChildren()
            .Select(selector: c => c.Value);
    public bool EnableSoapInterface =>
        configuration.GetSection(key: "SoapAPI").GetValue<bool>(key: "Enabled");

    public bool SoapInterfaceRequiresAuthentication =>
        configuration
            .GetSection(key: "SoapAPI")
            .GetValue(key: "RequiresAuthentication", defaultValue: true);
    public bool ExpectAndReturnOldDotNetAssemblyReferences =>
        configuration
            .GetSection(key: "SoapAPI")
            .GetValue(key: "ExpectAndReturnOldDotNetAssemblyReferences", defaultValue: true);
    public string PathToCustomAssetsFolder =>
        configuration.GetSection(key: "CustomAssetsConfig")[key: "PathToCustomAssetsFolder"];
    public string RouteToCustomAssetsFolder =>
        configuration.GetSection(key: "CustomAssetsConfig")[key: "RouteToCustomAssetsFolder"];
    public bool HasCustomAssets =>
        !string.IsNullOrWhiteSpace(value: PathToCustomAssetsFolder)
        && !string.IsNullOrWhiteSpace(value: RouteToCustomAssetsFolder);
    public string PathToClientApp
    {
        get
        {
            string pathToClientApp = configuration[key: "PathToClientApp"];
            if (!Path.IsPathRooted(path: pathToClientApp))
            {
                throw new Exception(
                    message: $"The PathToClientApp \"{pathToClientApp}\" must be an absolute path"
                );
            }
            return pathToClientApp;
        }
    }

    public string[] ExtensionDlls
    {
        get
        {
            var subSection = configuration.GetSection(key: "ExtensionDlls");
            if (!subSection.Exists())
            {
                return Array.Empty<string>();
            }
            return subSection.GetStringArrayOrEmpty();
        }
    }

    public bool ReloadModelWhenFilesChangesDetected =>
        configuration.GetValue<bool>(key: "ReloadModelWhenFilesChangesDetected");

    public bool EnableMiniProfiler =>
        configuration.GetSection(key: "MiniProfiler").GetValue(key: "Enabled", defaultValue: false);

    public int MultipartBodyLengthLimit =>
        configuration
            .GetSection(key: "HttpFormSettings")
            .GetValue(key: "MultipartBodyLengthLimit", defaultValue: 134_217_728);

    public int MultipartHeadersLengthLimit =>
        configuration
            .GetSection(key: "HttpFormSettings")
            .GetValue(key: "MultipartHeadersLengthLimit", defaultValue: 16_384);

    public int ValueLengthLimit =>
        configuration
            .GetSection(key: "HttpFormSettings")
            .GetValue(key: "ValueLengthLimit", defaultValue: 4_194_304);

    public SecurityProtocolType SecurityProtocol
    {
        get
        {
            var protocols = configuration
                .GetSection(key: "SupportedSecurityProtocols")
                .GetChildren()
                .ToArray();
            if (protocols.Length == 0)
            {
                return SecurityProtocolType.SystemDefault;
            }
            return protocols
                .Select(selector: ParseSecurityProtocolType)
                .Aggregate(func: (current, next) => current | next);
        }
    }

    private SecurityProtocolType ParseSecurityProtocolType(IConfigurationSection section)
    {
        if (Enum.TryParse(value: section.Value, result: out SecurityProtocolType protocolType))
        {
            return protocolType;
        }

        throw new ArgumentException(
            message: $"Cannot parse \"{section.Value}\" to a valid {nameof(SecurityProtocol)} when parsing values of SupportedSecurityProtocols"
        );
    }
}
