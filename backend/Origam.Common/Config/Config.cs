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

#if NETSTANDARD
using System;

namespace Origam.Config;

using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

public class Config : IConfig
{
    private static readonly Lazy<string> userSecretsId = new (ResolveUserSecretsId);

    private static string? ResolveUserSecretsId()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                foreach (var attr in asm.GetCustomAttributes(false))
                {
                    var type = attr.GetType();
                    if (type.FullName == "Microsoft.Extensions.Configuration.UserSecrets.UserSecretsIdAttribute")
                    {
                        var prop = type.GetProperty("UserSecretsId", BindingFlags.Public | BindingFlags.Instance);
                        return prop?.GetValue(attr) as string;
                    }
                }
            }
        }
        catch
        {
#if DEBUG
            throw;
#endif
        }

        return null;
    }

    private static bool TryGetJsonValue(string filePath, string[] path, out string? value)
    {
        value = null;
        try
        {
            if (!File.Exists(filePath)) return false;
            var jsonText = File.ReadAllText(filePath);
            var token = JToken.Parse(jsonText);
            foreach (var segment in path)
            {
                if (token is JObject obj)
                {
                    token = obj[segment];
                    if (token == null) return false;
                }
                else
                {
                    return false;
                }
            }

            if (token.Type == JTokenType.String)
            {
                value = token.Value<string>();
                return !string.IsNullOrWhiteSpace(value);
            }

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                value = token.ToString();
                return true;
            }

            if (token.Type == JTokenType.Boolean)
            {
                value = token.Value<bool>() ? "true" : "false";
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public long? GetValue(string[] appSettingsPath)
    {
        if (appSettingsPath == null)
        {
            return null;
        }
        
        string basePath = AppContext.BaseDirectory;
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                             ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                             ?? "Production";

        string value;
        
        var appsettingsPath = Path.Combine(basePath, "appsettings.json");
        if (TryGetJsonValue(appsettingsPath, appSettingsPath, out value) && !string.IsNullOrWhiteSpace(value))
        {
            if (long.TryParse(value, out long parsed) && parsed > 0) return parsed;
        }
        
        var envAppsettingsPath = Path.Combine(basePath, $"appsettings.{environment}.json");
        if (TryGetJsonValue(envAppsettingsPath, appSettingsPath, out value) && !string.IsNullOrWhiteSpace(value))
        {
            if (long.TryParse(value, out long parsed) && parsed > 0) return parsed;
        }

        var secretsId = userSecretsId.Value;
        if (!string.IsNullOrWhiteSpace(secretsId))
        {
            try
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var secretsPath = Path.Combine(userProfile, "Microsoft", "UserSecrets", secretsId, "secrets.json");
                if (TryGetJsonValue(secretsPath, appSettingsPath, out value) && !string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out long parsed) && parsed > 0) return parsed;
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
            }
        }

        string keyColon = string.Join(":", appSettingsPath);
        value = Environment.GetEnvironmentVariable(keyColon);
        if (string.IsNullOrWhiteSpace(value))
        {
            string keySingle = string.Join("_", appSettingsPath);
            value = Environment.GetEnvironmentVariable(keySingle);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            string keyDouble = string.Join("__", appSettingsPath);
            value = Environment.GetEnvironmentVariable(keyDouble);
        }

        if (!string.IsNullOrWhiteSpace(value) && long.TryParse(value, out long parsedEnv) && parsedEnv > 0)
        {
            return parsedEnv;
        }

        return null;
    }
}
#endif