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
        string dockerImage,
        string adminName,
        string adminEmail
    )
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Project", name);
        table.AddRow("Project folder", folder);
        table.AddRow("Docker image", dockerImage);
        table.AddRow("Admin user", adminName);
        table.AddRow("Admin email", adminEmail);

        var panel = new Panel(table)
            .Header("[green]Project Configuration[/]")
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintDatabaseValues(string host, int port, string name, string username)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Database Host", host);
        table.AddRow("Database Port", port.ToString());
        table.AddRow("Database Name", name);
        table.AddRow("Database Username", username);
        table.AddRow("Database Password", "[dim]-- masked --[/]");

        var panel = new Panel(table)
            .Header("[blue]Database Configuration[/]")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintArchitectValues(string dockerImage, int port)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Purple)
            .AddColumn("[bold]Parameter[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Architect image", dockerImage);
        table.AddRow("Architect port", port.ToString());

        var panel = new Panel(table)
            .Header("[purple]Architect Configuration[/]")
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

        table.AddRow("Git", isEnabled ? "[green]enabled[/]" : "[red]disabled[/]");
        table.AddRow("Git user", user);
        table.AddRow("Git email", email);

        var panel = new Panel(table).Header("[cyan1]Git Configuration[/]").BorderColor(Color.Cyan1);

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
            .AddColumn("[bold]Actions to be performed[/]");

        foreach (var action in actionsList)
        {
            table.AddRow(action);
        }

        var panel = new Panel(table)
            .Header("[orange1]Project Creation Tasks[/]")
            .BorderColor(Color.Orange1);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public void PrintBye()
    {
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[orange1]All steps completed. Bye![/]");
    }
}
