using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandloDb.Core.Services.Hosted;

namespace SandloDb.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddDbContext(this IHostBuilder builder, DbContext dbContext)
    {
        builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton(dbContext);
            services.AddHostedService<MaintenanceService>();
            services.AddHostedService<MemoryManagerService>();
        });
    }
}