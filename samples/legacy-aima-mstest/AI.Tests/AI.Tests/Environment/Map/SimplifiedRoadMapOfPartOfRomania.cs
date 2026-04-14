// The original version of this file is part of <see href="https://github.com/aimacode/aima-csharp"/> which is released under 
// MIT License
// Copyright (c) 2018 aimacode

namespace Italbytz.Adapters.Algorithms.Tests.Environment.Map;

/**
 * Represents a simplified road map of Romania. The initialization method is
 * declared static. So it can also be used to initialize other specialized
 * subclasses of {@link ExtendableMap} with road map data from Romania. Location
 * names, road distances and directions have been extracted from Artificial
 * Intelligence A Modern Approach (2nd Edition), Figure 3.2, page 63. The
 * straight-line distances to Bucharest have been taken from Artificial
 * Intelligence A Modern Approach (2nd Edition), Figure 4.1, page 95.
 *
 * @author Ruediger Lunde
 */
public class SimplifiedRoadMapOfPartOfRomania : ExtendableMap
{
    // The different locations in the simplified map of part of Romania
    public const string ORADEA = "Oradea";
    public const string ZERIND = "Zerind";
    public const string ARAD = "Arad";
    public const string TIMISOARA = "Timisoara";
    public const string LUGOJ = "Lugoj";
    public const string MEHADIA = "Mehadia";
    public const string DOBRETA = "Dobreta";
    public const string SIBIU = "Sibiu";
    public const string RIMNICU_VILCEA = "RimnicuVilcea";
    public const string CRAIOVA = "Craiova";
    public const string FAGARAS = "Fagaras";
    public const string PITESTI = "Pitesti";
    public const string GIURGIU = "Giurgiu";
    public const string BUCHAREST = "Bucharest";
    public const string NEAMT = "Neamt";
    public const string URZICENI = "Urziceni";
    public const string IASI = "Iasi";
    public const string VASLUI = "Vaslui";
    public const string HIRSOVA = "Hirsova";
    public const string EFORIE = "Eforie";

    public SimplifiedRoadMapOfPartOfRomania()
    {
        initMap(this);
    }

    /**
         * Initializes a map with a simplified road map of Romania.
         */
    public static void initMap(ExtendableMap map)
    {
        // mapOfRomania
        map.Clear();
        map.AddBidirectionalLink(ORADEA, ZERIND, 71.0);
        map.AddBidirectionalLink(ORADEA, SIBIU, 151.0);
        map.AddBidirectionalLink(ZERIND, ARAD, 75.0);
        map.AddBidirectionalLink(ARAD, TIMISOARA, 118.0);
        map.AddBidirectionalLink(ARAD, SIBIU, 140.0);
        map.AddBidirectionalLink(TIMISOARA, LUGOJ, 111.0);
        map.AddBidirectionalLink(LUGOJ, MEHADIA, 70.0);
        map.AddBidirectionalLink(MEHADIA, DOBRETA, 75.0);
        map.AddBidirectionalLink(DOBRETA, CRAIOVA, 120.0);
        map.AddBidirectionalLink(SIBIU, FAGARAS, 99.0);
        map.AddBidirectionalLink(SIBIU, RIMNICU_VILCEA, 80.0);
        map.AddBidirectionalLink(RIMNICU_VILCEA, PITESTI, 97.0);
        map.AddBidirectionalLink(RIMNICU_VILCEA, CRAIOVA, 146.0);
        map.AddBidirectionalLink(CRAIOVA, PITESTI, 138.0);
        map.AddBidirectionalLink(FAGARAS, BUCHAREST, 211.0);
        map.AddBidirectionalLink(PITESTI, BUCHAREST, 101.0);
        map.AddBidirectionalLink(GIURGIU, BUCHAREST, 90.0);
        map.AddBidirectionalLink(BUCHAREST, URZICENI, 85.0);
        map.AddBidirectionalLink(NEAMT, IASI, 87.0);
        map.AddBidirectionalLink(URZICENI, VASLUI, 142.0);
        map.AddBidirectionalLink(URZICENI, HIRSOVA, 98.0);
        map.AddBidirectionalLink(IASI, VASLUI, 92.0);
        // addBidirectionalLink(VASLUI - already all linked
        map.AddBidirectionalLink(HIRSOVA, EFORIE, 86.0);
        // addBidirectionalLink(EFORIE - already all linked

        // distances and directions
        // reference location: Bucharest
        map.SetDistAndDirToRefLocation(ARAD, 366, 117);
        map.SetDistAndDirToRefLocation(BUCHAREST, 0, 360);
        map.SetDistAndDirToRefLocation(CRAIOVA, 160, 74);
        map.SetDistAndDirToRefLocation(DOBRETA, 242, 82);
        map.SetDistAndDirToRefLocation(EFORIE, 161, 282);
        map.SetDistAndDirToRefLocation(FAGARAS, 176, 142);
        map.SetDistAndDirToRefLocation(GIURGIU, 77, 25);
        map.SetDistAndDirToRefLocation(HIRSOVA, 151, 260);
        map.SetDistAndDirToRefLocation(IASI, 226, 202);
        map.SetDistAndDirToRefLocation(LUGOJ, 244, 102);
        map.SetDistAndDirToRefLocation(MEHADIA, 241, 92);
        map.SetDistAndDirToRefLocation(NEAMT, 234, 181);
        map.SetDistAndDirToRefLocation(ORADEA, 380, 131);
        map.SetDistAndDirToRefLocation(PITESTI, 100, 116);
        map.SetDistAndDirToRefLocation(RIMNICU_VILCEA, 193, 115);
        map.SetDistAndDirToRefLocation(SIBIU, 253, 123);
        map.SetDistAndDirToRefLocation(TIMISOARA, 329, 105);
        map.SetDistAndDirToRefLocation(URZICENI, 80, 247);
        map.SetDistAndDirToRefLocation(VASLUI, 199, 222);
        map.SetDistAndDirToRefLocation(ZERIND, 374, 125);
    }
}