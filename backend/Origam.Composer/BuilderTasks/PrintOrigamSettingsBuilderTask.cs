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

using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Origam.Composer.DTOs;
using Origam.Composer.Enums;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Spectre.Console;

namespace Origam.Composer.BuilderTasks;

public class PrintOrigamSettingsBuilderTask(IConnectionStringService connectionStringService)
    : IPrintOrigamSettingsBuilderTask
{
    public string Name => Strings.BuilderTask_Print_Settings_object;
    public BuilderTaskState State { get; set; } = BuilderTaskState.Prepared;

    public void Execute(Project project)
    {
        var _setting = new OrigamSettings
        {
            Name = project.Name,
            TitleText = project.Name,
            DataConnectionString = connectionStringService.GetConnectionString(project),
            DataDataService = project.GetDataDataService,
            SchemaDataService = project.GetDataDataService,
            ModelSourceControlLocation = project.ModelFolder,
        };

        var origamSettingsCollection = new OrigamSettingsCollection { _setting };
        PrintToConsole(_setting);
    }

    private void PrintToConsole(OrigamSettings settings)
    {
        var xmlDocument = new XmlDocument();
        XPathNavigator nav = xmlDocument.CreateNavigator();
        using (XmlWriter writer = nav.AppendChild())
        {
            var xmlSerializer = new XmlSerializer(typeof(OrigamSettings));
            xmlSerializer.Serialize(writer, settings);
        }

        AnsiConsole.WriteLine("----------");
        AnsiConsole.WriteLine(xmlDocument.InnerXml);
        AnsiConsole.WriteLine("----------");
    }

    public void Rollback(Project project) { }
}
