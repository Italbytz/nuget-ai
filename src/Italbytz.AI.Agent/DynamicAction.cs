namespace Italbytz.AI.Agent;

/// <summary>
/// Simple action implementation identified by a human-readable name.
/// </summary>
public class DynamicAction : ObjectWithDynamicAttributes, IAction
{
    private const string AttributeName = "name";

    public DynamicAction(string name)
    {
        Attributes[AttributeName] = name;
    }

    public string Name => (string)(Attributes[AttributeName] ?? string.Empty);
}
