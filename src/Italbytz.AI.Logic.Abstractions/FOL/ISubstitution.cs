using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// A substitution θ binding variables to terms.
/// θ = {x/t1, y/t2, ...}
/// </summary>
public interface ISubstitution
{
    bool IsEmpty { get; }
    bool Binds(IVariable variable);
    ITerm GetBinding(IVariable variable);
    ISubstitution Extend(IVariable variable, ITerm term);
}
