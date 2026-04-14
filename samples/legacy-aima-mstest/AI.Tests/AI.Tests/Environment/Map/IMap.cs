// The original version of this file is part of <see href="https://github.com/aimacode/aima-csharp"/> which is released under 
// MIT License
// Copyright (c) 2018 aimacode

using Italbytz.Adapters.Algorithms.AI.Util;

namespace Italbytz.Adapters.Algorithms.Tests.Environment.Map;

/**
 * Provides a general interface for maps.
 *
 * @author Ruediger Lunde
 */
public interface IMap
{
	/**
     * Returns a list of all locations.
     */
	List<string> GetLocations();

    /**
     * Answers to the question: Where can I get, following one of the
     * connections starting at the specified location?
     */
    List<string> GetPossibleNextLocations(string location);

    /**
     * Answers to the question: From where can I reach a specified location,
     * following one of the map connections?
     */
    List<string> GetPossiblePrevLocations(string location);

    /**
     * Returns the travel distance between the two specified locations if they
     * are linked by a connection and null otherwise.
     */
    double GetDistance(string fromLocation, string toLocation);

    /**
     * Returns the position of the specified location. The position is
     * represented by two coordinates, e.g. latitude and longitude values.
     */
    Point2D GetPosition(string loc);

    /**
     * Returns a location which is selected by random.
     */
    string RandomlyGenerateDestination();
}