namespace SandloDb.Core;

public record SandloDbConfiguration(int EntityTtlMinutes)
{
    public const string SandloDb  = nameof(SandloDb);
};