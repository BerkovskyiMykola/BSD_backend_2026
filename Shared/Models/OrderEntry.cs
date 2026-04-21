namespace Shared.Models;

public sealed record OrderEntry(
    int ExchangeId,
    decimal Price,
    decimal Amount);
