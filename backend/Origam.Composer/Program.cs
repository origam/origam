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

using Microsoft.Extensions.DependencyInjection;
using Origam.Composer.BuilderTasks;
using Origam.Composer.Commands;
using Origam.Composer.DI;
using Origam.Composer.Interfaces.BuilderTasks;
using Origam.Composer.Interfaces.Services;
using Origam.Composer.Services;
using Spectre.Console.Cli;

namespace Origam.Composer;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IVisualService, VisualService>();
        services.AddSingleton<IProjectBuilderService, ProjectBuilderService>();
        services.AddSingleton<IPasswordGeneratorService, PasswordGeneratorService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();

        services.AddSingleton<IDownloadFileModelBuilderTask, DownloadFileModelBuilderTask>();
        services.AddSingleton<ICreateDatabaseBuilderTask, CreateDatabaseBuilderTask>();
        services.AddSingleton<
            IApplyDatabasePermissionsBuilderTask,
            ApplyDatabasePermissionsBuilderTask
        >();
        services.AddSingleton<IInitFileModelBuilderTask, InitFileModelBuilderTask>();
        services.AddSingleton<
            ICreateDatabaseStructureBuilderTask,
            CreateDatabaseStructureBuilderTask
        >();
        services.AddSingleton<ICreateNewPackageBuilderTask, CreateNewPackageBuilderTask>();
        services.AddSingleton<ICreateNewUserBuilderTask, CreateNewUserBuilderTask>();
        services.AddSingleton<IDockerBuilderTask, DockerBuilderTask>();
        services.AddSingleton<ICreateGitRepositoryBuilderTask, CreateGitRepositoryBuilderTask>();

        var registrar = new OrigamTypeRegistrar(services);

        var app = new CommandApp(registrar);
        app.Configure(config =>
        {
            config.AddCommand<CreateCommand>("create");
        });

        await app.RunAsync(args);
    }
}
