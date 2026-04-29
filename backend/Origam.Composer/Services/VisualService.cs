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

using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Spectre.Console;

namespace Origam.Composer.Services;

public class VisualService : IVisualService
{
    public void PrintHeader(string title)
    {
        var headerPanel = new Panel(text: $"[orange1 bold]ORIGAM Composer[/] [orange1]| {title}[/]")
            .Border(border: BoxBorder.Double)
            .BorderColor(color: Color.Orange1)
            .Padding(horizontal: 1, vertical: 0);

        AnsiConsole.Write(renderable: headerPanel);
        AnsiConsole.WriteLine();
    }

    public void PrintProjectValues(
        string name,
        string folder,
        string dockerImageLinux,
        string dockerImageWindows,
        string adminName,
        string adminEmail
    )
    {
        var table = new Table()
            .Border(border: TableBorder.Rounded)
            .BorderColor(color: Color.Green)
            .AddColumn(column: "[bold]Parameter[/]")
            .AddColumn(column: "[bold]Value[/]");

        table.AddRow(columns: ["Name", name]);
        table.AddRow(columns: ["Folder", folder]);
        table.AddRow(columns: ["Admin username", adminName]);
        table.AddRow(columns: ["Admin email", adminEmail]);
        table.AddRow(columns: ["Admin password", "[dim]-- masked --[/]"]);
        table.AddRow(columns: ["Docker image (linux)", dockerImageLinux]);
        table.AddRow(columns: ["Docker image (win)", dockerImageWindows]);

        var panel = new Panel(content: table)
            .Header(text: Strings.Project_Configuration_Header)
            .BorderColor(color: Color.Green);

        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }

    public void PrintDatabaseValues(
        string type,
        string host,
        int port,
        string name,
        string username
    )
    {
        var table = new Table()
            .Border(border: TableBorder.Rounded)
            .BorderColor(color: Color.Blue)
            .AddColumn(column: "[bold]Parameter[/]")
            .AddColumn(column: "[bold]Value[/]");

        table.AddRow(columns: ["Type", type]);
        table.AddRow(columns: ["Host", host]);
        table.AddRow(columns: ["Port", port.ToString()]);
        table.AddRow(columns: ["Name", name]);
        table.AddRow(columns: ["Username", username]);
        table.AddRow(columns: ["Password", "[dim]-- masked --[/]"]);

        var panel = new Panel(content: table)
            .Header(text: Strings.Database_Configuration_Header)
            .BorderColor(color: Color.Blue);

        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }

    public void PrintArchitectValues(string dockerImageLinux, string dockerImageWindows, int port)
    {
        var table = new Table()
            .Border(border: TableBorder.Rounded)
            .BorderColor(color: Color.Purple)
            .AddColumn(column: "[bold]Parameter[/]")
            .AddColumn(column: "[bold]Value[/]");

        table.AddRow(columns: ["Docker image (linux)", dockerImageLinux]);
        table.AddRow(columns: ["Docker image (win)", dockerImageWindows]);
        table.AddRow(columns: ["Docker port", port.ToString()]);

        var panel = new Panel(content: table)
            .Header(text: Strings.Architect_Configuration_Header)
            .BorderColor(color: Color.Purple);

        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }

    public void PrintGitValues(bool isEnabled, string user, string email)
    {
        var table = new Table()
            .Border(border: TableBorder.Rounded)
            .BorderColor(color: Color.Cyan1)
            .AddColumn(column: "[bold]Parameter[/]")
            .AddColumn(column: "[bold]Value[/]");

        table.AddRow(columns: ["Git", isEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]"]);
        table.AddRow(columns: ["Git user", user]);
        table.AddRow(columns: ["Git email", email]);

        var panel = new Panel(content: table)
            .Header(text: Strings.Git_Configuration_Header)
            .BorderColor(color: Color.Cyan1);

        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }

    public void PrintProjectCreateTasks(List<IBuilderTask> tasks)
    {
        var actionsList = new List<string>();
        foreach (IBuilderTask projectBuilderTask in tasks)
        {
            actionsList.Add(item: projectBuilderTask.Name);
        }

        var table = new Table()
            .Border(border: TableBorder.Rounded)
            .BorderColor(color: Color.Orange1)
            .AddColumn(column: Strings.Actions_to_be_performed);

        foreach (var action in actionsList)
        {
            table.AddRow(columns: action);
        }

        var panel = new Panel(content: table)
            .Header(text: Strings.Project_Creation_Tasks_Header)
            .BorderColor(color: Color.Orange1);

        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }

    public void PrintBye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(value: Strings.All_steps_completed);
    }

    public void PrintProjectAlreadyExists(string folder)
    {
        var panel = new Panel(
            content: new Markup(
                text: $"[bold]{folder.EscapeMarkup()}[/]\n\nNothing to do, Composer will exit."
            )
        )
        {
            Header = new PanelHeader(text: "[yellow] Model already exists [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Yellow),
            Padding = new Padding(left: 1, top: 0, right: 1, bottom: 0),
        };
        AnsiConsole.WriteLine();
        AnsiConsole.Write(renderable: panel);
        AnsiConsole.WriteLine();
    }
}
