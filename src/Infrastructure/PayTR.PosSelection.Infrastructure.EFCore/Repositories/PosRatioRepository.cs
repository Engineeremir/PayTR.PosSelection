using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Infrastructure.EFCore.Contexts;

namespace PayTR.PosSelection.Infrastructure.EFCore.Repositories;

public class PosRatioRepository : EfRepository<PosRatio>, IPosRatioRepository
{
    public PosRatioRepository(PosSelectionDbContext context) : base(context)
    {
    }
}