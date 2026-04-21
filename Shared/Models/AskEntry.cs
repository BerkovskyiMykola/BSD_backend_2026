namespace Shared.Models;

public sealed record AskEntry(
    int ExchangeId,
    decimal Price,
    decimal Amount);
