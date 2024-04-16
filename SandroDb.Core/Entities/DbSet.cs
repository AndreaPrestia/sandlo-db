using System.Diagnostics.CodeAnalysis;

namespace SandloDb.Core.Entities;

[ExcludeFromCodeCoverage]
internal sealed class DbSet<T> where T : IEntity
{
    public T? Content { get; internal set; }
    public int CurrentSizeInBytes { get; internal set; }
    public long LastUpdateTime { get; internal set; }
}