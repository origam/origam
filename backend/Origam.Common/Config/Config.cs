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
    private static readonly Lazy<string> userSecretsId = new(valueFactory: ResolveUserSecretsId);

    private static string ResolveUserSecretsId()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                foreach (var attr in asm.GetCustomAttributes(inherit: false))
                {
                    var type = attr.GetType();
                    if (
                        type.FullName
                        == "Microsoft.Extensions.Configuration.UserSecrets.UserSecretsIdAttribute"
                    )
                    {
                        var prop = type.GetProperty(
                            name: "UserSecretsId",
                            bindingAttr: BindingFlags.Public | BindingFlags.Instance
                        );
                        return prop?.GetValue(obj: attr) as string;
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

    private static bool TryGetJsonValue(string filePath, string[] path, out string value)
    {
        value = null;
        try
        {
            if (!File.Exists(path: filePath))
            {
                return false;
            }

            var jsonText = File.ReadAllText(path: filePath);
            var token = JToken.Parse(json: jsonText);
            foreach (var segment in path)
            {
                if (token is JObject obj)
                {
                    token = obj[propertyName: segment];
                    if (token == null)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (token.Type == JTokenType.String)
            {
                value = token.Value<string>();
                return !string.IsNullOrWhiteSpace(value: value);
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
        string environment =
            Environment.GetEnvironmentVariable(variable: "ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable(variable: "DOTNET_ENVIRONMENT")
            ?? "Production";

        string value;

        var appsettingsPath = Path.Combine(path1: basePath, path2: "appsettings.json");
        if (
            TryGetJsonValue(filePath: appsettingsPath, path: appSettingsPath, value: out value)
            && !string.IsNullOrWhiteSpace(value: value)
        )
        {
            if (long.TryParse(s: value, result: out long parsed) && parsed > 0)
            {
                return parsed;
            }
        }

        var envAppsettingsPath = Path.Combine(
            path1: basePath,
            path2: $"appsettings.{environment}.json"
        );
        if (
            TryGetJsonValue(filePath: envAppsettingsPath, path: appSettingsPath, value: out value)
            && !string.IsNullOrWhiteSpace(value: value)
        )
        {
            if (long.TryParse(s: value, result: out long parsed) && parsed > 0)
            {
                return parsed;
            }
        }

        var secretsId = userSecretsId.Value;
        if (!string.IsNullOrWhiteSpace(value: secretsId))
        {
            try
            {
                var userProfile = Environment.GetFolderPath(
                    folder: Environment.SpecialFolder.ApplicationData
                );
                var secretsPath = Path.Combine(
                    paths: new[]
                    {
                        userProfile,
                        "Microsoft",
                        "UserSecrets",
                        secretsId,
                        "secrets.json",
                    }
                );
                if (
                    TryGetJsonValue(filePath: secretsPath, path: appSettingsPath, value: out value)
                    && !string.IsNullOrWhiteSpace(value: value)
                )
                {
                    if (long.TryParse(s: value, result: out long parsed) && parsed > 0)
                    {
                        return parsed;
                    }
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
            }
        }

        string keyColon = string.Join(separator: ":", value: appSettingsPath);
        value = Environment.GetEnvironmentVariable(variable: keyColon);
        if (string.IsNullOrWhiteSpace(value: value))
        {
            string keySingle = string.Join(separator: "_", value: appSettingsPath);
            value = Environment.GetEnvironmentVariable(variable: keySingle);
        }

        if (string.IsNullOrWhiteSpace(value: value))
        {
            string keyDouble = string.Join(separator: "__", value: appSettingsPath);
            value = Environment.GetEnvironmentVariable(variable: keyDouble);
        }

        if (
            !string.IsNullOrWhiteSpace(value: value)
            && long.TryParse(s: value, result: out long parsedEnv)
            && parsedEnv > 0
        )
        {
            return parsedEnv;
        }

        return null;
    }
}
#endif
