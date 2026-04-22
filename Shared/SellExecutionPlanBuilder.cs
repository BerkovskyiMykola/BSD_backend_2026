using Shared.Models;

namespace Shared;

public sealed class SellExecutionPlanBuilder(IReadOnlyList<OrderEntry> bids)
{
    private readonly IReadOnlyList<OrderEntry> _bids = bids
        .OrderByDescending(x => x.Price)
        .ToList();

    public ExecutionPlanResult Build(
        IReadOnlyDictionary<int, decimal> exchangeIdBalances,
        decimal targetAmount)
    {
        var result = new List<Execution>();
        var remaining = targetAmount;
        var amountLeft = exchangeIdBalances.ToDictionary(x => x.Key, x => x.Value);

        foreach (var bid in _bids)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (!amountLeft.TryGetValue(bid.ExchangeId, out var availableBalance))
            {
                continue;
            }

            var available = Math.Min(bid.Amount, availableBalance);
            var take = Math.Min(available, remaining);

            if (take <= 0)
            {
                continue;
            }

            result.Add(new Execution(bid.ExchangeId, bid.Price, take));

            amountLeft[bid.ExchangeId] -= take;
            remaining -= take;
        }

        if (remaining > 0)
        {
            return new ExecutionPlanResult.Failure("Insufficient liquidity or balance");
        }

        return new ExecutionPlanResult.Success(result);
    }
}
