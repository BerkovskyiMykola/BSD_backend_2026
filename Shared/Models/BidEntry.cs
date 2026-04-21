namespace Shared.Models;

public sealed record BidEntry(
    int ExchangeId,
    decimal Price,
    decimal Amount);
