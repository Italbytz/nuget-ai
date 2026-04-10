using System.Collections.Generic;

namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Helper functions for working with search results.
/// </summary>
public static class SearchUtils
{
    public static IReadOnlyList<TAction> ToActions<TState, TAction>(INode<TState, TAction>? node)
    {
        if (node is null)
        {
            return [];
        }

        var actions = new Stack<TAction>();
        var current = node;
        while (current is not null && !current.IsRootNode())
        {
            if (current.Action is not null)
            {
                actions.Push(current.Action);
            }

            current = current.Parent;
        }

        return actions.ToArray();
    }
}
