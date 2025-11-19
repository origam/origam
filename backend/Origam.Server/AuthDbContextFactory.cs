using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Origam.Server
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs =
                config.GetConnectionString("AuthDb")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=OrigamAuth;Trusted_Connection=True;Encrypt=False";

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer(
                cs,
                b =>
                {
                    b.MigrationsAssembly(typeof(AuthDbContextFactory).Assembly.FullName);
                }
            );

            optionsBuilder.UseOpenIddict();

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
