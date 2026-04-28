using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Search.Environment.Map;

public class ExtendableMap
{
    private readonly Dictionary<string, Dictionary<string, double>> _links = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MapPosition> _positions = new(StringComparer.Ordinal);

    public void AddBidirectionalLink(string from, string to, double distance)
    {
        AddUnidirectionalLink(from, to, distance);
        AddUnidirectionalLink(to, from, distance);
    }

    public void AddUnidirectionalLink(string from, string to, double distance)
    {
        if (!_links.TryGetValue(from, out var destinations))
        {
            destinations = new Dictionary<string, double>(StringComparer.Ordinal);
            _links[from] = destinations;
        }

        destinations[to] = distance;
        _links.TryAdd(to, new Dictionary<string, double>(StringComparer.Ordinal));
    }

    public void SetPosition(string location, double x, double y)
    {
        _positions[location] = new MapPosition(x, y);
        _links.TryAdd(location, new Dictionary<string, double>(StringComparer.Ordinal));
    }

    public MapPosition? GetPosition(string location)
    {
        return _positions.TryGetValue(location, out var position) ? position : null;
    }

    public List<string> GetPossibleNextLocations(string from)
    {
        return _links.TryGetValue(from, out var destinations)
            ? destinations.Keys.ToList()
            : new List<string>();
    }

    public List<string> GetPossiblePreviousLocations(string to)
    {
        return _links
            .Where(link => link.Value.ContainsKey(to))
            .Select(link => link.Key)
            .ToList();
    }

    public double? GetDistance(string from, string to)
    {
        return _links.TryGetValue(from, out var destinations) && destinations.TryGetValue(to, out var distance)
            ? distance
            : null;
    }

    public string RandomlyGenerateDestination()
    {
        var locations = _links.Keys.ToList();
        return locations.Count == 0 ? string.Empty : locations[Random.Shared.Next(locations.Count)];
    }
}