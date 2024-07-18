using Origam.Architect.Server.Wrappers;

namespace Origam.Architect.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<ConfigManager>();
            var workbench = new Workbench();
            builder.Services.AddSingleton(workbench);
            var app = builder.Build();
            workbench.InitializeDefaultServices();
            workbench.Connect("Demo");
            
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
