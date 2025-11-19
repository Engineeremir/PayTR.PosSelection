using PayTR.PosSelection.Shared.SeedWork;
using PayTR.PosSelection.Shared.SeedWork.Entity;

namespace PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;

public class PosRatio : Entity
{
    public string PosName { get; private set; }
    public string CardType { get; private set; }
    public string CardBrand { get; private set; }
    public int Installment { get; private set; }
    public string Currency { get; private set; }
    public decimal CommissionRate { get; private set; }
    public decimal MinFee { get; private set; }
    public int Priority { get; private set; }

    protected PosRatio() { }

    public PosRatio(
        string posName,
        string cardType,
        string cardBrand,
        int installment,
        string currency,
        decimal commissionRate,
        decimal minFee,
        int priority)
    {
        Id = Guid.NewGuid();

        Validate(posName, installment, currency, commissionRate, minFee);

        PosName = posName;
        CardType = cardType;
        CardBrand = cardBrand;
        Installment = installment;
        Currency = currency;
        CommissionRate = commissionRate;
        MinFee = minFee;
        Priority = priority;

        CreatedOn = DateTime.UtcNow;
    }

    public void Update(
        string posName,
        string cardType,
        string cardBrand,
        int installment,
        string currency,
        decimal commissionRate,
        decimal minFee,
        int priority)
    {
        Validate(posName, installment, currency, commissionRate, minFee);

        PosName = posName;
        CardType = cardType;
        CardBrand = cardBrand;
        Installment = installment;
        Currency = currency;
        CommissionRate = commissionRate;
        MinFee = minFee;
        Priority = priority;

        UpdatedOn = DateTime.UtcNow;
    }

    private static void Validate(
        string posName,
        int installment,
        string currency,
        decimal commissionRate,
        decimal minFee)
    {
        var errors = new Dictionary<string, object>();

        if (string.IsNullOrWhiteSpace(posName))
            errors.Add(nameof(posName), "PosName cannot be empty.");

        if (installment <= 0)
            errors.Add(nameof(installment), "Installment must be greater than zero.");

        if (string.IsNullOrWhiteSpace(currency))
            errors.Add(nameof(currency), "Currency cannot be empty.");

        if (commissionRate < 0)
            errors.Add(nameof(commissionRate), "CommissionRate cannot be negative.");

        if (minFee < 0)
            errors.Add(nameof(minFee), "MinFee cannot be negative.");

        if (errors.Count > 0)
            throw new DomainException("PosRatio validation failed.", errors);
    }
}
