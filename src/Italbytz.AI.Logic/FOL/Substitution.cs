using System.Collections.Generic;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// A substitution θ implemented as an immutable chain backed by a dictionary.
/// </summary>
public class Substitution : ISubstitution
{
    private readonly Dictionary<IVariable, ITerm> _bindings;

    public static readonly ISubstitution Empty = new Substitution(
        new Dictionary<IVariable, ITerm>());

    private Substitution(Dictionary<IVariable, ITerm> bindings)
    {
        _bindings = bindings;
    }

    public bool IsEmpty => _bindings.Count == 0;

    public bool Binds(IVariable variable) => _bindings.ContainsKey(variable);

    public ITerm GetBinding(IVariable variable) => _bindings[variable];

    public ISubstitution Extend(IVariable variable, ITerm term)
    {
        var newBindings = new Dictionary<IVariable, ITerm>(_bindings)
        {
            [variable] = term
        };
        return new Substitution(newBindings);
    }

    public override string ToString() =>
        "{" + string.Join(", ", System.Linq.Enumerable.Select(_bindings, kv =>
            $"{kv.Key.SymbolicName}/{kv.Value.SymbolicName}")) + "}";
}
