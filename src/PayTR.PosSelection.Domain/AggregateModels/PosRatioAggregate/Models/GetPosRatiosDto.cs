namespace PayTR.PosSelection.Domain.Models;

public class GetPosRatiosDto
{
    public string PosName { get; set; }
    public string CardType { get; set; }
    public string CardBrand { get; set; }
    public int Installment { get; set; }
    public string Currency { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal MinFee { get; set; }
    public int Priority { get; set; }
}