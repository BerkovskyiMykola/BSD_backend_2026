using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var appSettings = builder.Configuration.Get<AppSettings>()!;

var (bids, asks) = await ExchangeLoader.LoadFlatAsync(
    Path.Combine(AppContext.BaseDirectory, "order_books_data"),
    appSettings.Take,
    CancellationToken.None);

builder.Services.AddSingleton(new BuyExecutionPlanBuilder(asks));
builder.Services.AddSingleton(new SellExecutionPlanBuilder(bids));

var app = builder.Build();

app.MapPost("/execution-plan", (
    [FromBody] ExecutionPlanRequest request,
    [FromServices] BuyExecutionPlanBuilder buyBuilder,
    [FromServices] SellExecutionPlanBuilder sellBuilder,
    [FromServices] IConfiguration config) =>
{
    var settings = config.Get<AppSettings>()!;

    var result = request.OrderType switch
    {
        OrderType.Buy => buyBuilder.Build(settings.Balances, request.TargetAmount),
        OrderType.Sell => sellBuilder.Build(request.TargetAmount),
        _ => throw new UnreachableException()
    };

    return result switch
    {
        ExecutionPlanResult.Success success => Results.Ok(new
        {
            status = "success",
            executions = success.Executions
        }),

        ExecutionPlanResult.Failure failure => Results.BadRequest(new
        {
            status = "failure",
            reason = failure.Reason
        }),

        _ => Results.Problem()
    };
});

app.Run();

internal sealed record ExecutionPlanRequest(
    OrderType OrderType,
    decimal TargetAmount);

internal sealed record AppSettings(
    int Take,
    IReadOnlyDictionary<int, decimal> Balances);

internal enum OrderType
{
    Buy,
    Sell
}