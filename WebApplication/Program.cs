using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Shared;
using Shared.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Execution Plan API", Version = "v1" });
});

var appSettings = builder.Configuration.Get<AppSettings>()!;

var (bids, asks) = await ExchangeLoader.LoadFlatAsync(
    Path.Combine(AppContext.BaseDirectory, "order_books_data"),
    appSettings.Take,
    CancellationToken.None);

builder.Services.AddSingleton(new BuyExecutionPlanBuilder(asks));
builder.Services.AddSingleton(new SellExecutionPlanBuilder(bids));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Execution Plan API v1");
    options.RoutePrefix = string.Empty;
});

app.MapPost("/execution-plan", (
    [FromBody] ExecutionPlanRequest request,
    [FromServices] BuyExecutionPlanBuilder buyBuilder,
    [FromServices] SellExecutionPlanBuilder sellBuilder,
    [FromServices] IConfiguration config) =>
{
    var settings = config.Get<AppSettings>()!;

    var result = request.OrderType switch
    {
        OrderType.Buy => buyBuilder.Build(settings.EurBalances, request.TargetAmount),
        OrderType.Sell => sellBuilder.Build(settings.BtcBalances, request.TargetAmount),
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
    IReadOnlyDictionary<int, decimal> EurBalances,
    IReadOnlyDictionary<int, decimal> BtcBalances);

internal enum OrderType
{
    Buy,
    Sell
}