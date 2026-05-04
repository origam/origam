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
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Configuration;
using Origam.Architect.Server.ControlAdapter;
using Origam.Architect.Server.Interfaces.Services;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Architect.Server.Services.Xslt;
using Origam.Extensions;
using Origam.Workbench.Services;

namespace Origam.Architect.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var schema = new SchemaService();
        var workbench = new Workbench(schema: schema);
        workbench.InitializeDefaultServices();
        workbench.Connect();

        var deploymentService = ServiceManager.Services.GetService<IDeploymentService>();
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var documentation = ServiceManager.Services.GetService<IDocumentationService>();
        var businessServicesService =
            ServiceManager.Services.GetService<IBusinessServicesService>();

        var builder = WebApplication.CreateBuilder(args: args);
        builder.Services.AddSingleton(implementationInstance: deploymentService);
        builder.Services.AddSingleton<TreeNodeFactory>();
        builder.Services.AddSingleton<EditorPropertyFactory>();
        builder.Services.AddSingleton<PropertyParser>();
        builder.Services.AddSingleton<EditorService>();
        builder.Services.AddTransient<XsltService>();
        builder.Services.AddTransient<SearchService>();
        builder.Services.AddSingleton<PropertyEditorService>();
        builder.Services.AddSingleton<DesignerEditorService>();
        builder.Services.AddSingleton<DeploymentVersionCurrentService>();
        builder.Services.AddSingleton<DeploymentScriptRunnerService>();
        builder.Services.AddSingleton<ControlAdapterFactory>();
        builder.Services.AddSingleton(implementationInstance: schema);
        builder.Services.AddSingleton(implementationInstance: workbench);
        builder.Services.AddSingleton(implementationInstance: persistence);
        builder.Services.AddSingleton(implementationInstance: documentation);
        builder.Services.AddSingleton(implementationInstance: businessServicesService);
        builder.Services.AddSingleton<DocumentationHelperService>();
        builder.Services.AddSingleton<IAddToDeploymentService, AddToDeploymentService>();
        builder.Services.AddSingleton<IAddToModelService, AddToModelService>();
        builder.Services.AddSingleton<IPlatformResolveService, PlatformResolveService>();
        builder.Services.AddSingleton<
            ISchemaDbCompareResultsService,
            SchemaDbCompareResultsService
        >();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder
            .Services.AddControllers()
            .AddJsonOptions(configure: options =>
            {
                options.JsonSerializerOptions.Converters.Add(item: new JsonStringEnumConverter());
            });
        builder.Services.AddLogging(configure: logging =>
        {
            logging.AddLog4Net();
        });

        var spaConfig = builder.Configuration.GetSectionOrThrow(key: "SpaConfig").Get<SpaConfig>();
        builder.Services.AddSpaStaticFiles(configuration: configuration =>
        {
            configuration.RootPath = spaConfig.PathToClientApplication;
        });

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<OrigamErrorHandlingMiddleware>();
        app.UseMiddleware<ServerIdentityMiddleware>();
        app.UseStaticFiles();
        string assemblyPath = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
        app.UseStaticFiles(
            options: new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    root: Path.Combine(path1: assemblyPath, path2: "Assets", path3: "Icons")
                ),
                RequestPath = "/Icons",
            }
        );
        app.UseSpaStaticFiles();
        app.MapControllers();
        app.UseSpa(configuration: spa =>
        {
            spa.Options.SourcePath = spaConfig.PathToClientApplication;
        });
        app.Run();
        SecurityManager.SetDIServiceProvider(
            diServiceProvider: ((IApplicationBuilder)app).ApplicationServices
        );
    }
}
