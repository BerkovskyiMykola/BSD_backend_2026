using Shared.Models;

namespace Shared;

public sealed class SellExecutionPlanBuilder(IReadOnlyList<OrderEntry> bids)
{
    private readonly IReadOnlyList<OrderEntry> _bids = bids
        .OrderByDescending(x => x.Price)
        .ToList();

    public ExecutionPlanResult Build(
        decimal targetAmount)
    {
        var result = new List<Execution>();
        var remaining = targetAmount;

        foreach (var bids in _bids)
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(bids.Amount, remaining);

            if (take <= 0)
            {
                continue;
            }

            result.Add(new Execution(bids.ExchangeId, bids.Price, take));

            remaining -= take;
        }

        if (remaining > 0)
        {
            return new ExecutionPlanResult.Failure("Insufficient liquidity");
        }

        return new ExecutionPlanResult.Success(result);
    }
}
