using PayTR.PosSelection.Domain.Models;

namespace PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate.Services;

public class PosCostCalculator : IPosCostCalculator
{
    public decimal CalculatePrice(decimal amount, GetPosRatiosDto ratio)
    {
        decimal rawCost = ratio.Currency switch
        {
            "USD" => amount * ratio.CommissionRate * 1.01m,
            _ => amount * ratio.CommissionRate
        };

        var cost = Math.Max(rawCost, ratio.MinFee);

        return Math.Round(cost, 2, MidpointRounding.AwayFromZero);
    }
}
