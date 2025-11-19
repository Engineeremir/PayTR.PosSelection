using Ardalis.Specification.EntityFrameworkCore;
using PayTR.PosSelection.Infrastructure.EFCore.Contexts;
using PayTR.PosSelection.Shared.SeedWork.Repository;

namespace PayTR.PosSelection.Infrastructure.EFCore.Repositories;

public class EfRepository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    protected readonly PosSelectionDbContext _context;
    public IUnitOfWork UnitOfWork => _context;

    public EfRepository(PosSelectionDbContext context) : base(context)
    {
        _context = context;
    }
}