namespace PayTR.PosSelection.Shared.SeedWork.Entity;

public interface ISoftDeletion
{
    public DateTime CreatedOn { get; }
    public DateTime? UpdatedOn { get; }
    public DateTime? DeletedOn { get; }
}