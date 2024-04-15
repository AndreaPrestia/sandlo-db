namespace SandloDb.Core.Configurations;

public sealed class SandloDbOptions
{
    public SandloDbOptions(int entityTtlMinutes)
    {
        EntityTtlMinutes = entityTtlMinutes;
    }

    public int EntityTtlMinutes { get; }
}