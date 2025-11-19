namespace PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate.Models;

public class SelectedPosResult
{
    public string PosName { get; set; }
    public string CardType { get; set; }
    public string CardBrand { get; set; }
    public int Installment { get; set; }
    public string Currency { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Price { get; set; }
    public decimal PayableTotal { get; set; }
}