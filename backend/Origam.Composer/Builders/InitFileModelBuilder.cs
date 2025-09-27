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

using Origam.Composer.DTOs;
using Origam.Composer.Services;
using Origam.Workbench.Services;
using Spectre.Console;

namespace Origam.Composer.Builders;

public class InitFileModelBuilder : AbstractBuilder
{
    public override string Name => "Initialize model from files";

    private SchemaService SchemaService;

    public override void Execute(Project project)
    {
        AnsiConsole.MarkupLine($"[orange1][bold]Executing:[/][/] {Name}");

        OrigamEngine.OrigamEngine.InitializeRuntimeServices();

        SchemaService = ServiceManager.Services.GetService<SchemaService>();

        var workbench = new Workbench(SchemaService); // TODO: Refactor this
        workbench.InitializeDefaultServices();

        SchemaService.LoadSchema(new Guid(project.BasePackageId), isInteractive: true);
    }

    public override void Rollback()
    {
        SchemaService.UnloadSchema();
        OrigamEngine.OrigamEngine.UnloadConnectedServices();
    }
}
