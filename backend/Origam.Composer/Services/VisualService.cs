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
        var headerPanel = new Panel($"[orange1 bold]ORIGAM Composer[/] [orange1]| {title}[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Orange1)
            .Padding(1, 0);

        AnsiConsole.Write(headerPanel);
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
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Name", name);
        table.AddRow("Folder", folder);
        table.AddRow("Admin username", adminName);
        table.AddRow("Admin email", adminEmail);
        table.AddRow("Admin password", "[dim]-- masked --[/]");
        table.AddRow("Docker image (linux)", dockerImageLinux);
        table.AddRow("Docker image (win)", dockerImageWindows);

        var panel = new Panel(table)
            .Header(Strings.Project_Configuration_Header)
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);
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
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Type", type);
        table.AddRow("Host", host);
        table.AddRow("Port", port.ToString());
        table.AddRow("Name", name);
        table.AddRow("Username", username);
        table.AddRow("Password", "[dim]-- masked --[/]");

        var panel = new Panel(table)
            .Header(Strings.Database_Configuration_Header)
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintArchitectValues(string dockerImageLinux, string dockerImageWindows, int port)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Purple)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Docker image (linux)", dockerImageLinux);
        table.AddRow("Docker image (win)", dockerImageWindows);
        table.AddRow("Docker port", port.ToString());

        var panel = new Panel(table)
            .Header(Strings.Architect_Configuration_Header)
            .BorderColor(Color.Purple);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintGitValues(bool isEnabled, string user, string email)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Git", isEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]");
        table.AddRow("Git user", user);
        table.AddRow("Git email", email);

        var panel = new Panel(table)
            .Header(Strings.Git_Configuration_Header)
            .BorderColor(Color.Cyan1);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintProjectCreateTasks(List<IBuilderTask> tasks)
    {
        var actionsList = new List<string>();
        foreach (IBuilderTask projectBuilderTask in tasks)
        {
            actionsList.Add(projectBuilderTask.Name);
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Orange1)
            .AddColumn(Strings.Actions_to_be_performed);

        foreach (var action in actionsList)
        {
            table.AddRow(action);
        }

        var panel = new Panel(table)
            .Header(Strings.Project_Creation_Tasks_Header)
            .BorderColor(Color.Orange1);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintBye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(Strings.All_steps_completed);
    }
}
