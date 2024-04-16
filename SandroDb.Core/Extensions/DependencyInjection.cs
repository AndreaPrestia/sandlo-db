using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandloDb.Core.Builders;
using SandloDb.Core.Services.Hosted;

namespace SandloDb.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddInMemoryDbContext(this IHostBuilder builder, DbContextBuilder dbContextBuilder)
    {
        builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton(_ =>
            {
                var dbContext = dbContextBuilder.Build();
                return dbContext;
            });
            
            services.AddHostedService<MaintenanceService>();
        });
    }
}