using Origam.Architect.Server.ArchitectLogic;
using Origam.Architect.Server.Controllers;
using Origam.Architect.Server.Wrappers;
using Origam.Workbench.Services;

namespace Origam.Architect.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var schema = new SchemaService();
        var workbench = new Workbench(schema);
        workbench.InitializeDefaultServices();
        workbench.Connect("Demo");
            
        IPersistenceService persistence = ServiceManager.Services
            .GetService<IPersistenceService>();
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<ConfigManager>();
        builder.Services.AddSingleton<TreeNodeFactory>();
        builder.Services.AddSingleton<EditorPropertyFactory>();
        builder.Services.AddSingleton<PropertyParser>();
        builder.Services.AddSingleton(schema);
        builder.Services.AddSingleton(workbench);
        builder.Services.AddSingleton(persistence);
        builder.Services.AddLogging(logging =>
        {
            logging.AddLog4Net();
        });
        var app = builder.Build();
            
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}