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
using System.IO;
using static Origam.NewProjectEnums;

namespace Origam.ProjectAutomation;
public class SettingsBuilder : AbstractBuilder
{
    private int _settingsIndex;
    OrigamSettingsCollection _settings;
    OrigamSettings _setting;
    public OrigamSettings Setting
    {
        get
        {
            return _setting;
        }
    }
    public override string Name
    {
        get
        {
            return "Add Settings";
        }
    }
    public override void Execute(Project project)
    {
        _settings = GetSettings();
        _setting = new OrigamSettings();
        _setting.Name = project.Name;
        _setting.TitleText = project.Name;
        _setting.DataConnectionString = project.BuilderDataConnectionString;
        _setting.ModelSourceControlLocation = GetModelSourceLocation(project);
        _setting.ServerUrl = project.BaseUrl;
        _setting.DataDataService = project.GetDataDataService;
        _setting.SchemaDataService = project.GetDataDataService;
        _setting.ModelSourceControlLocation = project.ModelSourceFolder;
        _settingsIndex = _settings.Add(_setting);
        project.ActiveConfigurationIndex = _settingsIndex;
        ConfigurationManager.SetActiveConfiguration(_setting);
        try
        {
            SaveSettings(_settings);
        }
        catch
        {
            Rollback();
            throw;
        }
    }
    private string GetModelSourceLocation(Project project)
    {
        switch (project.TypeTemplate)
        {
            case TypeTemplate.Default:
                return project.ModelSourceFolder;
            case TypeTemplate.Open:
            case TypeTemplate.Template:
                return project.SourcesFolder;
            default:
                throw new Exception("Bad TypeTemplate " + project.TypeTemplate.ToString());
        }
        
    }
    public override void Rollback()
    {
        _settings.RemoveAt(_settingsIndex);
        SaveSettings(_settings);
        ConfigurationManager.SetActiveConfiguration(null);
    }
    public static OrigamSettingsCollection GetSettings() => 
        ConfigurationManager.GetAllUserHomeConfigurations();
    public static void SaveSettings(OrigamSettingsCollection settings)
    {
        ConfigurationManager.WriteConfiguration(settings);
    }
}
