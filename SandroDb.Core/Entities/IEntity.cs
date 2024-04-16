namespace SandloDb.Core.Entities;

public interface IEntity
{
    Guid Id { get; internal set; }
    long Created { get; internal set; }
    long Updated { get; internal set; }
}