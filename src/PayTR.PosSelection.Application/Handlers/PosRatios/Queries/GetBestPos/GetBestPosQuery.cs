using MediatR;
using PayTR.PosSelection.Shared.Models;

namespace PayTR.PosSelection.Application.Handlers.PosRatios.Queries.GetBestPos;

public class GetBestPosQuery : IRequest<ApiResult<GetBestPosQueryDto>>
{
    public decimal Amount { get; init; }
    public int Installment { get; init; }
    public string Currency { get; init; } = default!;
    public string? CardType { get; init; }
    public string? CardBrand { get; init; }
}