using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SandloDb.Core.Configurations;

namespace SandloDb.Core.Services.Hosted;

internal sealed class MemoryManagerService : BackgroundService
{
    private readonly ILogger<MemoryManagerService> _logger;
    private readonly SandloDbContext _dbContext;

    public MemoryManagerService(ILogger<MemoryManagerService> logger, SandloDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var entityTtlMinutes = SandloDbConfiguration.SandloDbOptions != null ? SandloDbConfiguration.SandloDbOptions.EntityTtlMinutes : 5;

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("MemoryManagerService running.");

                var availableTypes = _dbContext.CurrentTypes;

                if (!availableTypes.Any())
                {
                    _logger.LogInformation("No types stored in SandloDbContext. No maintenance will be provided.");
                }
                else
                {
                    foreach (var type in availableTypes)
                    {
                        var entitiesToDelete = _dbContext.GetBy(x => x.Created <= new DateTimeOffset(DateTime.UtcNow).AddMinutes(-entityTtlMinutes).ToUnixTimeMilliseconds(), type);

                        if (!entitiesToDelete.Any())
                        {
                            _logger.LogInformation($"No entities for type {type.Name} found.");
                            continue;
                        }
                        
                        var deleteResult = _dbContext.RemoveMany(entitiesToDelete, type);
                            
                        _logger.LogInformation($"Deleted for type {type.Name} - {deleteResult} entities.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}