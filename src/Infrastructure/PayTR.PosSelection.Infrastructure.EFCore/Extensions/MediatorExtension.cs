using MediatR;
using PayTR.PosSelection.Infrastructure.EFCore.Contexts;
using PayTR.PosSelection.Shared.SeedWork.Entity;

namespace PayTR.PosSelection.Infrastructure.EFCore.Extensions;

public static class MediatorExtension
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, PosSelectionDbContext context)
    {
        var domainEntites = context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Count != 0);

        var domainEvents = domainEntites
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntites.ToList()
            .ForEach(entity => entity.Entity.ClearedDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
