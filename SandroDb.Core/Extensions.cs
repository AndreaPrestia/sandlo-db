using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SandloDb.Core
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static void AddSandloDb(this IServiceCollection services)
        {
            services.AddSingleton<SandloDbContext>();
        }
    }
}
