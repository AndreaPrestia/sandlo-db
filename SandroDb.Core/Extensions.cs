using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandloDb.Core
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static void AddSandloDbContext(this IHostBuilder builder, SandloDbOptions? sandloDbOptions = null)
        {
            builder.ConfigureServices((_, services) =>
            {
                SandloDbConfiguration.SandloDbOptions = sandloDbOptions;
                services.AddSingleton<SandloDbContext>();
                services.AddHostedService<MaintenanceService>();
                services.AddHostedService<MemoryManagerService>();
            });
        }
    }
}
