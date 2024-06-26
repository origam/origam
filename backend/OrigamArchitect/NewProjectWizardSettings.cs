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
    [DefaultSettingValue(@".")]
    public string DatabaseServerName
    {
        get
        {
            return (string)this["DatabaseServerName"];
        }
        set
        {
            this["DatabaseServerName"] = value;
        }
    }
    [UserScopedSetting()]
	[DefaultSettingValue("c:\\OrigamProjects")]
    public string SourcesFolder
    {
        get
        {
            return (string)this["SourcesFolder"];
        }
        set
        {
            this["SourcesFolder"] = value;
        }
    }
    [UserScopedSetting()]
    [DefaultSettingValue(@"C:\inetpub")]
    public string BinFolder
    {
        get
        {
            return (string)this["BinFolder"];
        }
        set
        {
            this["BinFolder"] = value;
        }
    }
    [UserScopedSetting()]
    [DefaultSettingValue("Microsoft Sql Server")]
    public string DatabaseTypeText
    {
        get
        {
            return (string)this["DatabaseTypeText"];
        }
        set
        {
            this["DatabaseTypeText"] = value;
        }
    }
    [UserScopedSetting()]
    [DefaultSettingValue("http://localhost:2375")]
    public string DockerApiAdress
    {
        get
        {
            return (string)this["DockerApiAdress"];
        }
        set
        {
            this["DockerApiAdress"] = value;
        }
    }
    [UserScopedSetting()]
    [DefaultSettingValue("")]
    public string DockerSourceFolder
    {
        get
        {
            return (string)this["DockerSourceFolder"];
        }
        set
        {
            this["DockerSourceFolder"] = value;
        }
    }
}
