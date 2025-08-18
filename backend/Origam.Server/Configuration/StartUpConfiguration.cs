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
            .GetSection("UserApiOptions")
            .GetSection("PublicRoutes")
            .GetChildren()
            .Select(c => c.Value);
    public IEnumerable<string> UserApiRestrictedRoutes =>
        configuration   
            .GetSection("UserApiOptions")
            .GetSection("RestrictedRoutes")
            .GetChildren()
            .Select(c => c.Value);
    public bool EnableSoapInterface => configuration
        .GetSection("SoapAPI")
        .GetValue<bool>("Enabled");       
    
    public bool SoapInterfaceRequiresAuthentication => configuration
        .GetSection("SoapAPI")
        .GetValue("RequiresAuthentication", true);       
    public bool ExpectAndReturnOldDotNetAssemblyReferences => configuration
        .GetSection("SoapAPI")
        .GetValue("ExpectAndReturnOldDotNetAssemblyReferences", true);
    public string PathToCustomAssetsFolder => 
        configuration.GetSection("CustomAssetsConfig")["PathToCustomAssetsFolder"];    
    public string RouteToCustomAssetsFolder => 
        configuration.GetSection("CustomAssetsConfig")["RouteToCustomAssetsFolder"];    
    public string IdentityGuiLogoUrl => 
        configuration.GetSection("CustomAssetsConfig")["IdentityGuiLogoUrl"];    
    public string Html5ClientLogoUrl => 
        configuration.GetSection("CustomAssetsConfig")["Html5ClientLogoUrl"];
    public bool HasCustomAssets => !string.IsNullOrWhiteSpace(PathToCustomAssetsFolder) &&
                                   !string.IsNullOrWhiteSpace(RouteToCustomAssetsFolder);
    public string PathToClientApp
    {
        get
        {
            string pathToClientApp = configuration["PathToClientApp"];
            if(!Path.IsPathRooted(pathToClientApp))
            {
                throw new Exception($"The PathToClientApp \"{pathToClientApp}\" must be an absolute path");
            }
            return pathToClientApp;
        }
    } 
    
    public string[] ExtensionDlls
    {
        get
        {
            var subSection = configuration.GetSection("ExtensionDlls");
            if (!subSection.Exists())
            {
                return Array.Empty<string>();
            }
            return subSection.GetStringArrayOrEmpty();
        }
    }   
    
    public bool ReloadModelWhenFilesChangesDetected =>
        configuration.GetValue<bool>("ReloadModelWhenFilesChangesDetected");
    
    public bool EnableMiniProfiler =>
        configuration
            .GetSection("MiniProfiler")
            .GetValue("Enabled", false);
    
    public int MultipartBodyLengthLimit =>
        configuration
            .GetSection("HttpFormSettings")
            .GetValue("MultipartBodyLengthLimit", 134_217_728);    
        
    public int MultipartHeadersLengthLimit =>
        configuration
            .GetSection("HttpFormSettings")
            .GetValue("MultipartHeadersLengthLimit", 16_384);    
        
    public int ValueLengthLimit =>
        configuration
            .GetSection("HttpFormSettings")
            .GetValue("ValueLengthLimit", 4_194_304);
    
    public SecurityProtocolType SecurityProtocol
    {
        get
        {
            var protocols = configuration
                .GetSection("SupportedSecurityProtocols")
                .GetChildren()
                .ToArray();
            if (protocols.Length == 0)
            {
                return SecurityProtocolType.SystemDefault;
            }
            return protocols
                .Select(ParseSecurityProtocolType)
                .Aggregate((current, next) => current | next);
        }
    }
    private SecurityProtocolType ParseSecurityProtocolType(IConfigurationSection section)
    {
        if (Enum.TryParse(section.Value, out SecurityProtocolType protocolType))
        {
            return protocolType;
        }
        else
        {
            throw new ArgumentException($"Cannot parse \"{section.Value}\" to a valid {nameof(SecurityProtocol)} when parsing values of SupportedSecurityProtocols");
        }
    }
}
