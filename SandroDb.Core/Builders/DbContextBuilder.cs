namespace SandloDb.Core.Builders;

/// <summary>
/// Builder to initialize correctly the DbContext with options
/// </summary>
public class DbContextBuilder
{
    private readonly DbContext _dbContext;

    private DbContextBuilder()
    {
        _dbContext = new DbContext();
    }

    // Static initializer to create the builder instance
    public static DbContextBuilder Initialize()
    {
        return new DbContextBuilder();
    }

    /// <summary>
    /// Adds the EntityTtlMinutes to the dbContext
    /// </summary>
    /// <param name="entityTtlMinutes"></param>
    /// <returns></returns>
    public DbContextBuilder WithEntityTtlMinutes(int entityTtlMinutes = 5)
    {
        switch (entityTtlMinutes)
        {
            case <= 0:
                throw new ArgumentException($"{nameof(entityTtlMinutes)} cannot be equal or less than 0 minutes.");
            case > 30:
                throw new ArgumentException($"{nameof(entityTtlMinutes)} cannot be more than 30 minutes.");
            default:
                _dbContext.EntityTtlMinutes = entityTtlMinutes;
                return this;
        }
    }
    
    /// <summary>
    /// Adds the MaxMemoryAllocationBytes to the dbContext
    /// </summary>
    /// <param name="maxMemoryAllocationInBytes"></param>
    /// <returns></returns>
    public DbContextBuilder WithMaxMemoryAllocationInBytes(double maxMemoryAllocationInBytes = 1e+7)
    {
        switch (maxMemoryAllocationInBytes)
        {
            case <= 0:
                throw new ArgumentException($"{nameof(maxMemoryAllocationInBytes)} cannot be equal or less than 0 bytes.");
            case > 2e+8:
                throw new ArgumentException($"{nameof(maxMemoryAllocationInBytes)} cannot be more than 200 megabytes.");
            default:
                _dbContext.MaxMemoryAllocationInBytes = maxMemoryAllocationInBytes;
                return this;
        }
    }
    
    public DbContext Build()
    {
        return _dbContext;
    }
}