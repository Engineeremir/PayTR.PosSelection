namespace PayTR.PosSelection.Shared.SeedWork.Entity;

public interface IEntityBase<out TId>
{
    TId Id { get; }
}
public interface IEntityBase
{
}