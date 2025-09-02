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
using System.Collections;
using System.IO;
using System.Reflection;

namespace Origam;

public class ConfigurationManager
{
    public static string UserProfileOrigamSettings =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ORIGAM",
            GetVersion(),
            "OrigamSettings.config"
        );
    private static OrigamSettings _activeConfiguration;
    private static string _pathToOrigamSettings;

    public static string GetPathOrigamSettings()
    {
        return _pathToOrigamSettings;
    }

    public static void SetPathOrigamSettings(string pathToOrigamSettings)
    {
        _pathToOrigamSettings = pathToOrigamSettings;
    }

    public static void SetActiveConfiguration(OrigamSettings configuration)
    {
        _activeConfiguration = configuration;
    }

    public static OrigamSettings GetActiveConfiguration()
    {
        return _activeConfiguration;
    }

    public static OrigamSettingsCollection GetAllUserHomeConfigurations(
        string origamSettingsPath = null
    )
    {
        CreateUserProfileConfigFile(origamSettingsPath);
        return new OrigamSettingsReader(_pathToOrigamSettings).GetAll();
    }

    public static OrigamSettingsCollection GetAllConfigurations()
    {
        return new OrigamSettingsReader(_pathToOrigamSettings).GetAll();
    }

    public static void WriteConfiguration(OrigamSettingsCollection configuration)
    {
        // do some sanity check
        SortedList list = new SortedList();
        foreach (OrigamSettings setting in configuration)
        {
            if (!list.Contains(setting.Name))
            {
                list.Add(setting.Name, setting);
            }
            else
            {
                throw new Exception(ResourceUtils.GetString("CantSaveConfigDuplicateName"));
            }
        }
        new OrigamSettingsReader(_pathToOrigamSettings).Write(configuration);
    }

    private static void CreateUserProfileConfigFile(string origamSettingsPath = null)
    {
        string origamSettings = origamSettingsPath ?? UserProfileOrigamSettings;

        if (!File.Exists(origamSettings))
        {
            FileInfo file = new(origamSettings);
            file.Directory.Create();
            OrigamSettingsReader origamSetting = new(origamSettings);
            if (File.Exists(origamSetting.GetDefaultPathToOrigamSettings()))
            {
                File.Copy(origamSetting.GetDefaultPathToOrigamSettings(), origamSettings);
            }
            else
            {
                new OrigamSettingsReader(origamSettings).Write(new OrigamSettingsCollection());
            }
        }
        SetPathOrigamSettings(origamSettings);
    }

    private static string GetVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly();
        return assembly.GetName().Version.Major + "." + assembly.GetName().Version.Minor;
    }
}

public class OrigamSettingsException : Exception
{
    private static string MakeMessage(string message) =>
        Strings.OrigamSettingsExceptionPrefix + message;

    public OrigamSettingsException(string message, Exception innerException)
        : base(MakeMessage(message), innerException) { }

    public OrigamSettingsException(string message)
        : base(MakeMessage(message)) { }
}
