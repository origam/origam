using Origam.Architect.Server.Wrappers;
using Origam.Workbench.Services;

namespace Origam.Architect.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var schema = new SchemaService();
            var workbench = new Workbench(schema);
            workbench.InitializeDefaultServices();
            workbench.Connect("Demo");
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<ConfigManager>();
            builder.Services.AddSingleton(schema);
            builder.Services.AddSingleton(workbench);
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
}
