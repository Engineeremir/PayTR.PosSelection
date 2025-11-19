using MediatR;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Domain.Models;
using PayTR.PosSelection.Shared.DistributedCache;
using PayTR.PosSelection.Shared.DistributedCache.Redis;
using PayTR.PosSelection.Shared.SeedWork.HttpClient;

namespace PayTR.PosSelection.Application.Handlers.PosRatios.Commands.SyncPosRatiosViaScheduler;

public class SyncPosRatiosViaSchedulerCommandHandler(
    IBaseHttpClientService baseHttpClientService,
    IPosRatioRepository posRatioRepository,
    IRedisDistributedCacheService cacheService
    ) : IRequestHandler<SyncPosRatiosViaSchedulerCommand, bool>
{
    public async Task<bool> Handle(SyncPosRatiosViaSchedulerCommand request, CancellationToken cancellationToken)
    {
        var apiResponse = await baseHttpClientService.GetAsync<List<SyncPosRatiosViaMockApi>>(new HttpRequestModel
        {
            Url = "https://6899a45bfed141b96ba02e4f.mockapi.io/paytr/ratios",
        });

        var dbRatios = await posRatioRepository.ListAsync(cancellationToken);

        var dbLookup = dbRatios.ToDictionary(
                x => new { x.PosName, x.CardType, x.CardBrand, x.Installment, x.Currency },
                x => x);

        foreach (var item in apiResponse)
        {
            var key = new
            {
                item.PosName,
                item.CardType,
                item.CardBrand,
                item.Installment,
                item.Currency
            };

            if (dbLookup.TryGetValue(key, out var existing))
            {
                existing.Update(item.PosName, item.CardType, item.CardBrand, item.Installment, item.Currency, item.CommissionRate, item.MinFee, item.Priority);
            }
            else
            {
                var newRecord = new PosRatio(item.PosName, item.CardType, item.CardBrand, item.Installment, item.Currency, item.CommissionRate, item.MinFee, item.Priority);

                await posRatioRepository.AddAsync(newRecord, cancellationToken);
            }
        }

        await posRatioRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.SetAsync(new CacheRequestModel<List<GetPosRatiosDto>>
        {
            Key = "pos-ratios-cache-key",
            Value = dbRatios.Select(x => new GetPosRatiosDto
            {
                PosName = x.PosName,
                CardType = x.CardType,
                CardBrand = x.CardBrand,
                CommissionRate = x.CommissionRate,
                Currency = x.Currency,
                Installment = x.Installment,
                MinFee = x.MinFee,
                Priority = x.Priority
            }).ToList(),
            Options = new CacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddDays(1),
                SlidingExpiration = TimeSpan.FromHours(1),
            },
            Db = RedisDb.DB0
        });

        return true;
    }
}
