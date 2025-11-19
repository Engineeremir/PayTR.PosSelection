using Ardalis.Specification;
using MediatR;
using PayTR.PosSelection.Application.Specifications.PosRatios;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate.Services;
using PayTR.PosSelection.Domain.Models;
using PayTR.PosSelection.Shared.DistributedCache;
using PayTR.PosSelection.Shared.DistributedCache.Redis;
using PayTR.PosSelection.Shared.Models;

namespace PayTR.PosSelection.Application.Handlers.PosRatios.Queries.GetBestPos;

public class GetBestPosQueryHandler(
    IPosRatioRepository posRatioRepository,
    IPosCostCalculator posCostCalculator,
    IRedisDistributedCacheService cacheService
    ) : IRequestHandler<GetBestPosQuery, ApiResult<GetBestPosQueryDto>>
{
    private const string PosRatiosCacheKey = "pos-ratios-cache-key";
    public async Task<ApiResult<GetBestPosQueryDto>> Handle(GetBestPosQuery request, CancellationToken cancellationToken)
    {
        var ratios = await cacheService.GetAsync(new CacheRequestModel<List<GetPosRatiosDto>>
        {
            Db = RedisDb.DB0,
            Key = PosRatiosCacheKey,
        });

        if (ratios is null)
        {
            var spec = new GetPosRatiosForSelectionSpecification(
            request.Installment,
            request.Currency,
            request.CardType,
            request.CardBrand);

            ratios = await posRatioRepository.ListAsync(spec, cancellationToken);

            await cacheService.SetAsync(new CacheRequestModel<List<GetPosRatiosDto>>
            {
                Key = PosRatiosCacheKey,
                Value = ratios,
                Options = new CacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddDays(1),
                    SlidingExpiration = TimeSpan.FromHours(1),
                },
                Db = RedisDb.DB0
            });
        }

        var candidates = ratios.Select(r =>
        {
            var price = posCostCalculator.CalculatePrice(request.Amount, r);

            var payableTotal = Math.Round(
                request.Amount + price,
                2,
                MidpointRounding.AwayFromZero);

            return new
            {
                Ratio = r,
                Price = price,
                PayableTotal = payableTotal
            };
        });

        var best = candidates
            .OrderBy(x => x.Price)                  
            .ThenByDescending(x => x.Ratio.Priority)
            .ThenBy(x => x.Ratio.CommissionRate)
            .ThenBy(x => x.Ratio.PosName)
            .First();

        var response = new GetBestPosQueryDto
        {
            Filters = new GetBestPosQueryFiltersDto
            {
                Amount = request.Amount,
                Installment = request.Installment,
                Currency = request.Currency,
                CardType = request.CardType,
                CardBrand = request.CardBrand
            },
            OverallMin = new GetBestPosQueryOverallMinDto
            {
                PosName = best.Ratio.PosName,
                CardType = best.Ratio.CardType,
                CardBrand = best.Ratio.CardBrand,
                Installment = best.Ratio.Installment,
                Currency = best.Ratio.Currency,
                CommissionRate = best.Ratio.CommissionRate,
                Price = best.Price,
                PayableTotal = best.PayableTotal
            }
        };

        return new ApiResult<GetBestPosQueryDto>().ResponseOk(response);
    }
}
