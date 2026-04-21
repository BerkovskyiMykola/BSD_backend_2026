using Shared.Models;

namespace Shared;

public sealed class BuyExecutionPlanBuilder(IReadOnlyList<OrderEntry> asks)
{
    private readonly IReadOnlyList<OrderEntry> _asks = asks
        .OrderBy(x => x.Price)
        .ToList();

    public ExecutionPlanResult Build(
        IReadOnlyDictionary<int, decimal> exchangeIdBalances,
        decimal targetAmount)
    {
        var result = new List<Execution>();
        var remaining = targetAmount;

        var amountLeft = exchangeIdBalances.ToDictionary(x => x.Key, x => x.Value);

        foreach (var ask in _asks)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (!amountLeft.TryGetValue(ask.ExchangeId, out var availableBalance))
            {
                continue;
            }

            var maxAffordable = availableBalance / ask.Price;
            var available = Math.Min(ask.Amount, maxAffordable);
            var take = Math.Min(available, remaining);

            if (take <= 0)
            {
                continue;
            }

            result.Add(new Execution(ask.ExchangeId, ask.Price, take));

            amountLeft[ask.ExchangeId] -= take * ask.Price;
            remaining -= take;
        }

        if (remaining > 0)
        {
            return new ExecutionPlanResult.Failure("Insufficient liquidity or balance");
        }

        return new ExecutionPlanResult.Success(result);
    }
}
