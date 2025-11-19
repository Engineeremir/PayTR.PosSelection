using MediatR;

namespace PayTR.PosSelection.Shared.SeedWork.Entity;

public abstract class EntityBase<TId> : EntityBase, IEntityBase<TId>, ISoftDeletion
{
    public virtual TId Id { get; protected set; }
    public virtual DateTime CreatedOn { get; protected set; }
    public virtual DateTime? UpdatedOn { get; protected set; }
    public virtual DateTime? DeletedOn { get; protected set; }
}

public abstract class EntityBase : IEntityBase
{
    private List<INotification> _domainEvents;
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(eventItem);
    }

    public void ClearedDomainEvents()
    {
        _domainEvents?.Clear();
    }
}
