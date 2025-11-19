using Ardalis.Specification;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Domain.Models;

namespace PayTR.PosSelection.Application.Specifications.PosRatios;

public class GetPosRatiosForSelectionSpecification : Specification<PosRatio, GetPosRatiosDto>
{
    public GetPosRatiosForSelectionSpecification(
        int installment,
        string currency,
        string? cardType,
        string? cardBrand,
        bool withTracking = false)
    {
        Query.Where(x => x.Installment == installment &&
                         x.Currency == currency);

        if (!string.IsNullOrWhiteSpace(cardType))
        {
            Query.Where(x => x.CardType == cardType);
        }

        if (!string.IsNullOrWhiteSpace(cardBrand))
        {
            Query.Where(x => x.CardBrand == cardBrand);
        }

        Query.Select(x => new GetPosRatiosDto
        {
            Installment = x.Installment,
            MinFee = x.MinFee,
            Currency = x.Currency,
            CardType = x.CardType,
            CardBrand = x.CardBrand,
            CommissionRate = x.CommissionRate,
            PosName = x.PosName,
            Priority = x.Priority
        });

        if (!withTracking)
        {
            Query.AsNoTracking();
        }
    }
}