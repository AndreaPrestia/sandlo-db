namespace SandloDb.Core
{
    public interface IEntity
    {
        Guid Id { get; set; }
        long Created { get; set; }
        long Updated { get; set; }
    }
}
