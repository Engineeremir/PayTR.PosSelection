using Ardalis.Specification;

namespace PayTR.PosSelection.Shared.SeedWork.Repository;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{
    IUnitOfWork UnitOfWork { get; }
}