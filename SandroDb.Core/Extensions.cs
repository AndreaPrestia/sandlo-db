using Microsoft.Extensions.DependencyInjection;

namespace SandloDb.Core
{
    public static class Extensions
    {
        public static void AddSandloDb(this IServiceCollection services)
        {
            services.AddSingleton<SandloDbContext>();
        }
    }
}
