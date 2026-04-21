namespace Shared.Models;

public sealed record Execution(
    int ExchangeId, 
    decimal Price, 
    decimal Amount);
