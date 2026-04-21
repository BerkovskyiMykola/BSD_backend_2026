using Shared.Models;
using System.Text.Json;

namespace Shared;

public static class ExchangeLoader
{
    public static async Task<(IReadOnlyList<OrderEntry> Bids, IReadOnlyList<OrderEntry> Asks)> LoadFlatAsync(
        string path,
        int take,
        CancellationToken cancellationToken = default)
    {
        var bids = new List<OrderEntry>();
        var asks = new List<OrderEntry>();
        var id = 1;

        await foreach (var line in File.ReadLinesAsync(path, cancellationToken))
        {
            if (id > take)
            {
                break;
            }

            var parts = line.Split('\t');

            if (parts.Length < 2)
            {
                continue;
            }

            var root = JsonSerializer.Deserialize<Root>(parts[1]);

            if (root is null)
            {
                continue;
            }

            foreach (var b in root.Bids)
            {
                bids.Add(new OrderEntry(id, b.Order.Price, b.Order.Amount));
            }

            foreach (var a in root.Asks)
            {
                asks.Add(new OrderEntry(id, a.Order.Price, a.Order.Amount));
            }

            id++;
        }

        return (bids, asks);
    }

    private sealed record Root(
        IReadOnlyList<OrderWrapper> Bids,
        IReadOnlyList<OrderWrapper> Asks);

    private sealed record OrderWrapper(Order Order);

    private sealed record Order(
        decimal Price,
        decimal Amount);
}
