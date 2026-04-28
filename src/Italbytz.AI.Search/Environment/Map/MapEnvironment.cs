using Italbytz.AI.Agent;

namespace Italbytz.AI.Search.Environment.Map;

public class MapEnvironment : IEnvironment<DynamicPercept, MoveToAction>
{
    private readonly Dictionary<IAgent<DynamicPercept, MoveToAction>, string> _agentLocations = new();

    public MapEnvironment(ExtendableMap map)
    {
        Map = map;
    }

    public ExtendableMap Map { get; }

    public void AddAgent(IAgent<DynamicPercept, MoveToAction> agent, string location)
    {
        _agentLocations[agent] = location;
    }

    public string? GetAgentLocation(IAgent<DynamicPercept, MoveToAction> agent)
    {
        return _agentLocations.TryGetValue(agent, out var location) ? location : null;
    }

    public void Execute(IAgent<DynamicPercept, MoveToAction> agent, MoveToAction action)
    {
        if (_agentLocations.TryGetValue(agent, out var currentLocation)
            && Map.GetDistance(currentLocation, action.ToLocation) is not null)
        {
            _agentLocations[agent] = action.ToLocation;
        }
    }

    public DynamicPercept? GetPerceptSeenBy(IAgent<DynamicPercept, MoveToAction> agent)
    {
        return _agentLocations.TryGetValue(agent, out var location)
            ? new DynamicPercept().SetAttribute(AttNames.PerceptIn, location)
            : null;
    }
}