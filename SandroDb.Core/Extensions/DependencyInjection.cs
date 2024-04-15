using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandloDb.Core.Configurations;
using SandloDb.Core.Services.Hosted;

namespace SandloDb.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
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