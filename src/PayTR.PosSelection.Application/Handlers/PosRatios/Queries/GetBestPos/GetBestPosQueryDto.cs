namespace PayTR.PosSelection.Application.Handlers.PosRatios.Queries.GetBestPos;

public class GetBestPosQueryDto
{
    public GetBestPosQueryFiltersDto Filters { get; set; } = default!;
    public GetBestPosQueryOverallMinDto OverallMin { get; set; } = default!;
}

public class GetBestPosQueryFiltersDto
{
    public decimal Amount { get; set; }
    public int Installment { get; set; }
    public string Currency { get; set; } = default!;
    public string? CardType { get; set; }
    public string? CardBrand { get; set; }
}

public class GetBestPosQueryOverallMinDto
{
    public string PosName { get; set; } = default!;
    public string CardType { get; set; } = default!;
    public string CardBrand { get; set; } = default!;
    public int Installment { get; set; }
    public string Currency { get; set; } = default!;
    public decimal CommissionRate { get; set; }
    public decimal Price { get; set; }
    public decimal PayableTotal { get; set; }
}
