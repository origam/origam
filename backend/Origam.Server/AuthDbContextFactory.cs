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
            // Resolve content root (project folder when run from CLI)
            var basePath = Directory.GetCurrentDirectory();

            // Build configuration like the app would, but minimal (no DI)
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Prefer "AuthDb", fall back to "DefaultConnection"
            var cs =
                config.GetConnectionString("AuthDb")
                ?? config.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=OrigamAuth;Trusted_Connection=True;Encrypt=False";

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer(
                cs,
                b =>
                {
                    // Make sure migrations end up in this assembly
                    b.MigrationsAssembly(typeof(AuthDbContextFactory).Assembly.FullName);
                }
            );

            // IMPORTANT for OpenIddict schema
            optionsBuilder.UseOpenIddict();

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
