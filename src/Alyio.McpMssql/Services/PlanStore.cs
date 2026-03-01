// MIT License

using System.Collections.Concurrent;

namespace Alyio.McpMssql.Services;

internal sealed class PlanStore : IPlanStore
{
    private static readonly TimeSpan s_ttl = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<string, (string Xml, DateTime Expiry)> _store = new();

    public string Save(string xml)
    {
        Evict();

        var id = Guid.NewGuid().ToString("N")[..8];
        _store[id] = (xml, DateTime.UtcNow + s_ttl);
        return id;
    }

    public string? TryGet(string id)
    {
        if (_store.TryGetValue(id, out var entry))
        {
            if (DateTime.UtcNow <= entry.Expiry)
            {
                return entry.Xml;
            }

            _store.TryRemove(id, out _);
        }

        return null;
    }

    private void Evict()
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _store)
        {
            if (now > kvp.Value.Expiry)
            {
                _store.TryRemove(kvp.Key, out _);
            }
        }
    }
}
