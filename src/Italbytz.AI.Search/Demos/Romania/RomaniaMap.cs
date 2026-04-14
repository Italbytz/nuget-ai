using Italbytz.AI.Search.Framework.Problem;

namespace Italbytz.AI.Search.Demos.Romania;

public static class RomaniaMap
{
    public const string Arad = "Arad";
    public const string Zerind = "Zerind";
    public const string Oradea = "Oradea";
    public const string Sibiu = "Sibiu";
    public const string Timisoara = "Timisoara";
    public const string Lugoj = "Lugoj";
    public const string Mehadia = "Mehadia";
    public const string Dobreta = "Dobreta";
    public const string Craiova = "Craiova";
    public const string RimnicuVilcea = "RimnicuVilcea";
    public const string Fagaras = "Fagaras";
    public const string Pitesti = "Pitesti";
    public const string Bucharest = "Bucharest";

    private static readonly IReadOnlyList<string> _cities =
    [
        Arad,
        Sibiu,
        Zerind,
        Timisoara,
        Oradea,
        Lugoj,
        Mehadia,
        Dobreta,
        Craiova,
        RimnicuVilcea,
        Fagaras,
        Pitesti,
        Bucharest
    ];

    private static readonly IReadOnlyList<RomaniaRoad> _roads =
    [
        new(Arad, Zerind, 75),
        new(Arad, Sibiu, 140),
        new(Arad, Timisoara, 118),
        new(Zerind, Oradea, 71),
        new(Oradea, Sibiu, 151),
        new(Sibiu, Fagaras, 99),
        new(Sibiu, RimnicuVilcea, 80),
        new(Timisoara, Lugoj, 111),
        new(Lugoj, Mehadia, 70),
        new(Mehadia, Dobreta, 75),
        new(Dobreta, Craiova, 120),
        new(Craiova, RimnicuVilcea, 146),
        new(Craiova, Pitesti, 138),
        new(RimnicuVilcea, Pitesti, 97),
        new(Fagaras, Bucharest, 211),
        new(Pitesti, Bucharest, 101)
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<RomaniaRoad>> _adjacency = CreateAdjacency();

    private static readonly IReadOnlyDictionary<string, double> _straightLineDistanceToBucharest = new Dictionary<string, double>
    {
        [Arad] = 366,
        [Zerind] = 374,
        [Oradea] = 380,
        [Sibiu] = 253,
        [Timisoara] = 329,
        [Lugoj] = 244,
        [Mehadia] = 241,
        [Dobreta] = 242,
        [Craiova] = 160,
        [RimnicuVilcea] = 193,
        [Fagaras] = 176,
        [Pitesti] = 100,
        [Bucharest] = 0
    };

    public static IReadOnlyList<string> Cities => _cities;

    public static IReadOnlyList<RomaniaRoad> Roads => _roads;

    public static GeneralProblem<string, RomaniaMoveToAction> CreateProblem(string initialState, string goalState = Bucharest, bool useDistanceCosts = true)
    {
        return useDistanceCosts
            ? new GeneralProblem<string, RomaniaMoveToAction>(
                initialState,
                state => _adjacency[state].Select(road => new RomaniaMoveToAction(road.To)).ToList(),
                (_, action) => action.ToLocation,
                state => state == goalState,
                (state, action, _) => _adjacency[state].First(road => road.To == action.ToLocation).Cost)
            : new GeneralProblem<string, RomaniaMoveToAction>(
                initialState,
                state => _adjacency[state].Select(road => new RomaniaMoveToAction(road.To)).ToList(),
                (_, action) => action.ToLocation,
                state => state == goalState);
    }

    public static double HeuristicToBucharest(string city)
    {
        return _straightLineDistanceToBucharest[city];
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<RomaniaRoad>> CreateAdjacency()
    {
        var lookup = _cities.ToDictionary(city => city, _ => new List<RomaniaRoad>());

        foreach (var road in _roads)
        {
            lookup[road.From].Add(road with { To = road.To });
            lookup[road.To].Add(new RomaniaRoad(road.To, road.From, road.Cost));
        }

        return lookup.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<RomaniaRoad>)pair.Value);
    }
}