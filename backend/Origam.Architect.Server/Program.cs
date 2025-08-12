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

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Configuration;
using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Architect.Server.Wrappers;
using Origam.Extensions;
using Origam.Workbench.Services;

namespace Origam.Architect.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var schema = new SchemaService();
        var workbench = new Workbench(schema);
        workbench.InitializeDefaultServices();
        workbench.Connect();

        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var documentation = ServiceManager.Services.GetService<IDocumentationService>();
        var builder = WebApplication.CreateBuilder(args);
        builder
            .Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<ConfigManager>();
        builder.Services.AddSingleton<TreeNodeFactory>();
        builder.Services.AddSingleton<EditorPropertyFactory>();
        builder.Services.AddSingleton<PropertyParser>();
        builder.Services.AddSingleton<EditorService>();
        builder.Services.AddSingleton<PropertyEditorService>();
        builder.Services.AddSingleton<DesignerEditorService>();
        builder.Services.AddSingleton<ControlAdapterFactory>();
        builder.Services.AddSingleton(schema);
        builder.Services.AddSingleton(workbench);
        builder.Services.AddSingleton(persistence);
        builder.Services.AddSingleton(documentation);
        builder.Services.AddSingleton<DocumentationHelperService>();
        builder.Services.AddLogging(logging =>
        {
            logging.AddLog4Net();
        });

        var spaConfig = builder.Configuration.GetSectionOrThrow("SpaConfig").Get<SpaConfig>();
        builder.Services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = spaConfig.PathToClientApplication;
        });

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.UseStaticFiles();
        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(assemblyPath, "Assets", "Icons")
                ),
                RequestPath = "/Icons",
            }
        );
        app.UseSpaStaticFiles();
        app.MapControllers();
        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = spaConfig.PathToClientApplication;
        });
        app.Run();
    }
}
