using Italbytz.AI.Agent;

namespace Italbytz.AI.Search.Environment.Map;

public class MoveToAction : DynamicAction
{
    public MoveToAction(string toLocation) : base("moveTo")
    {
        ToLocation = toLocation;
    }

    public string ToLocation { get; }

    public override string ToString()
    {
        return $"MoveToAction[name={Name}, location={ToLocation}]";
    }
}