using Microsoft.Extensions.Configuration;
using Shared;
using Shared.Models;
using System.Diagnostics;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var appSettings = config.Get<AppSettings>()!;

var (bids, asks) = await ExchangeLoader.LoadFlatAsync(
    Path.Combine(AppContext.BaseDirectory, "order_books_data"),
    appSettings.Take, 
    CancellationToken.None);

ExecutionPlanResult executionPlanResult = appSettings.OrderType switch
{
    OrderType.Buy => new BuyExecutionPlanBuilder(asks)
        .Build(appSettings.EurBalances, appSettings.TargetAmount),

    OrderType.Sell => new SellExecutionPlanBuilder(bids)
        .Build(appSettings.BtcBalances, appSettings.TargetAmount),

    _ => throw new UnreachableException("Unsupported order type")
};

switch (executionPlanResult)
{
    case ExecutionPlanResult.Success success:
        {
            Console.WriteLine("Execution plan:");

            foreach (var execution in success.Executions)
            {
                Console.WriteLine(execution);
            }

            break;
        }

    case ExecutionPlanResult.Failure failure:
        {
            Console.WriteLine($"Failed: {failure.Reason}");
            break;
        }
}

internal sealed record AppSettings(
    int Take,
    OrderType OrderType,
    decimal TargetAmount,
    IReadOnlyDictionary<int, decimal> EurBalances,
    IReadOnlyDictionary<int, decimal> BtcBalances);

internal enum OrderType
{
    Buy,
    Sell
}