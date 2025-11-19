using PayTR.PosSelection.Domain.Models;

namespace PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate.Services;

public interface IPosCostCalculator
{
    decimal CalculatePrice(decimal amount, GetPosRatiosDto ratio);
}