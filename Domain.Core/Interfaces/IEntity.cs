namespace Domain.Core.Interfaces;
public interface IEntity<TId>
{
    TId Id { get; }
}
