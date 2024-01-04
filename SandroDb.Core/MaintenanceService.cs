using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SandloDb.Core;

[ExcludeFromCodeCoverage]
internal class MaintenanceService : BackgroundService
{
    private readonly ILogger<MaintenanceService> _logger;
    private readonly SandloDbContext _dbContext;
    private readonly SandloDbConfiguration _configuration;

    public MaintenanceService(ILogger<MaintenanceService> logger, SandloDbContext dbContext, IOptionsMonitor<SandloDbConfiguration> optionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        
        _logger = logger;
        _dbContext = dbContext;
        _configuration = optionsMonitor.CurrentValue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(_configuration.EntityTtlMinutes * 1.5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("MaintenanceService running.");

                var availableTypes = _dbContext.CurrentTypes;

                if (!availableTypes.Any())
                {
                    _logger.LogInformation("No types stored in SandloDbContext. No maintenance will be provided.");
                }
                else
                {
                    foreach (var type in availableTypes)
                    {
                        var entitiesToDelete = _dbContext.GetBy(x => x.Created <= new DateTimeOffset(DateTime.UtcNow).AddMinutes(-_configuration.EntityTtlMinutes).ToUnixTimeMilliseconds(), type);

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