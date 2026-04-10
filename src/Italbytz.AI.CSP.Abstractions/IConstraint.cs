using System.Collections.Generic;

namespace Italbytz.AI.CSP;

public interface IConstraint<TVar, TVal> where TVar : IVariable
{
    IList<TVar> Scope { get; }

    bool IsSatisfiedWith(IAssignment<TVar, TVal> assignment);
}
