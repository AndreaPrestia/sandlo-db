using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandloDb.Core
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static void AddSandloDbContext(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.Configure<SandloDbConfiguration>(context.Configuration.GetSection(
                    SandloDbConfiguration.SandloDb));
                services.AddSingleton<SandloDbContext>();
                services.AddHostedService<MaintenanceService>();
                services.AddHostedService<MemoryManagerService>();
            });
        }
    }
}
