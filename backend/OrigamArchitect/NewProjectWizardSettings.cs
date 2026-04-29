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

using System.Configuration;

namespace OrigamArchitect;

public class NewProjectWizardSettings : ApplicationSettingsBase
{
    [UserScopedSetting()]
    [DefaultSettingValue(value: @"localhost")]
    public string DatabaseServerName
    {
        get { return (string)this[propertyName: "DatabaseServerName"]; }
        set { this[propertyName: "DatabaseServerName"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue(value: "c:\\OrigamProjects")]
    public string SourcesFolder
    {
        get { return (string)this[propertyName: "SourcesFolder"]; }
        set { this[propertyName: "SourcesFolder"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue(value: @"C:\inetpub")]
    public string BinFolder
    {
        get { return (string)this[propertyName: "BinFolder"]; }
        set { this[propertyName: "BinFolder"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue(value: "Microsoft Sql Server")]
    public string DatabaseTypeText
    {
        get { return (string)this[propertyName: "DatabaseTypeText"]; }
        set { this[propertyName: "DatabaseTypeText"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue(value: "http://localhost:2375")]
    public string DockerApiAdress
    {
        get { return (string)this[propertyName: "DockerApiAdress"]; }
        set { this[propertyName: "DockerApiAdress"] = value; }
    }

    [UserScopedSetting()]
    [DefaultSettingValue(value: "")]
    public string DockerSourceFolder
    {
        get { return (string)this[propertyName: "DockerSourceFolder"]; }
        set { this[propertyName: "DockerSourceFolder"] = value; }
    }
}
