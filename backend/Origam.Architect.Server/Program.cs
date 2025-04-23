using System.Text.Json;
using System.Text.Json.Serialization;
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
            
        IPersistenceService persistence = ServiceManager.Services
            .GetService<IPersistenceService>();
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers()    
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
        builder.Services.AddLogging(logging =>
        {
            logging.AddLog4Net();
        });
        
        var spaConfig = builder.Configuration
            .GetSectionOrThrow("SpaConfig")
            .Get<SpaConfig>();
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
        app.UseSpaStaticFiles();
        app.MapControllers();
        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = spaConfig.PathToClientApplication;
        });
        app.Run();
    }
}