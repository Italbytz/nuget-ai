// The original version of this file is part of <see href="https://github.com/aimacode/aima-csharp"/> which is released under 
// MIT License
// Copyright (c) 2018 aimacode

using Italbytz.Adapters.Algorithms.AI.Util;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.Map;

/**
 * Implements a map with locations, distance labeled links between the
 * locations, straight line distances, and 2d-placement positions of locations.
 * Locations are represented by strings and travel distances by double values.
 * Locations and links can be added dynamically and removed after creation. This
 * enables to read maps from file or to modify them with respect to newly
 * obtained knowledge.
 *
 * @author Ruediger Lunde
 */
public class ExtendableMap : IMap
{
    /**
     * Stores map data. Locations are represented as vertices and connections
     * (links) as directed edges labeled with corresponding travel distances.
     */
    private readonly LabeledGraph<string, double> _links;

    /**
     * Stores xy-coordinates for each location.
     */
    private readonly Dictionary<string, Point2D> _locationPositions;

    /**
     * Creates an empty map.
     */
    public ExtendableMap()
    {
        _links = new LabeledGraph<string, double>();
        _locationPositions = new Dictionary<string, Point2D>();
    }

    /**
     * Returns a list of all locations.
     */
    public List<string> GetLocations() => _links.getVertexLabels();

    /**
     * Answers to the question: Where can I get, following one of the
     * connections starting at the specified location?
     */
    public List<string> GetPossibleNextLocations(string location)
    {
        var result = _links.getSuccessors(location);
        result.Sort();
        return result;
    }

    /**
     * Answers to the question: From where can I reach a specified location,
     * following one of the map connections? This implementation just calls
     * {@link #getPossibleNextLocations(String)} as the underlying graph structure
     * cannot be traversed efficiently in reverse order.
     */
    public List<string> GetPossiblePrevLocations(string location) =>
        GetPossibleNextLocations(location);

    /**
     * Returns the travel distance between the two specified locations if they
     * are linked by a connection and null otherwise.
     */
    public double GetDistance(string fromLocation, string toLocation) =>
        _links.get(fromLocation, toLocation);

    /**
     * Returns a location which is selected by random.
     */
    public string RandomlyGenerateDestination() =>
        Util.SelectRandomlyFromList(GetLocations());

    /**
     * Returns the position of the specified location as with respect to an
     * orthogonal coordinate system.
     */
    public Point2D GetPosition(string loc) => _locationPositions[loc];

    /**
     * Removes everything.
     */
    public void Clear()
    {
        _links.clear();
        _locationPositions.Clear();
    }

    /**
     * Clears all connections but keeps location position informations.
     */
    public void ClearLinks()
    {
        _links.clear();
    }

    /**
     * Checks whether the given string is the name of a location.
     */
    public bool IsLocation(string str) => _links.isVertexLabel(str);

    /**
     * Adds a one-way connection to the map.
     */
    public void AddUnidirectionalLink(string fromLocation, string toLocation,
        double distance)
    {
        _links.set(fromLocation, toLocation, distance);
    }

    /**
     * Adds a connection which can be traveled in both direction. Internally,
     * such a connection is represented as two one-way connections.
     */
    public void AddBidirectionalLink(string fromLocation, string toLocation,
        double distance)
    {
        _links.set(fromLocation, toLocation, distance);
        _links.set(toLocation, fromLocation, distance);
    }

    /**
     * Removes a one-way connection.
     */
    public void RemoveUnidirectionalLink(string fromLocation, string toLocation)
    {
        _links.remove(fromLocation, toLocation);
    }

    /**
     * Removes the two corresponding one-way connections.
     */
    public void RemoveBidirectionalLink(string fromLocation, string toLocation)
    {
        _links.remove(fromLocation, toLocation);
        _links.remove(toLocation, fromLocation);
    }

    /**
     * Defines the position of a location as with respect to an orthogonal
     * coordinate system.
     */
    public void SetPosition(string loc, double x, double y)
    {
        _locationPositions.Add(loc, new Point2D(x, y));
    }

    /**
     * Defines the position of a location within the map.Using this method, one
     * location should be selected as reference position (
     * <code>dist= 0 </code>
     * and
     * < code> dir = 0 </code>
     * ) and all the other location should be placed
     * relative to it.
     *
     * @param loc
     * location name
     * @param dist
     * distance to a reference position
     * @param dir
     * bearing (compass direction) in which the location is seen from
     * the reference position
     */
    public void SetDistAndDirToRefLocation(string loc, double dist, int dir)
    {
        var coords = new Point2D(-Math.Sin(dir * Math.PI / 180.0) * dist,
            Math.Cos(dir * Math.PI / 180.0) * dist);
        _links.addVertex(loc);
        _locationPositions.Add(loc, coords);
    }
}