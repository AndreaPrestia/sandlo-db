using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SandloDb.Core.Services.Hosted;

[ExcludeFromCodeCoverage]
internal class MaintenanceService : BackgroundService
{
    private readonly ILogger<MaintenanceService> _logger;
    private readonly DbContext _dbContext;

    public MaintenanceService(ILogger<MaintenanceService> logger, DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbContext);
        
        _logger = logger;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var entityTtlMinutes = _dbContext.EntityTtlMinutes ?? 5;
        
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(entityTtlMinutes * 1.5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("MaintenanceService running.");

                var availableTypes = _dbContext.CurrentTypes;

                if (!availableTypes.Any())
                {
                    _logger.LogInformation("No types stored in DbContext. No maintenance will be provided.");
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