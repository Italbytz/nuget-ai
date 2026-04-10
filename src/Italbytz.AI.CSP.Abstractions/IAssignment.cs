using System;
using System.Collections.Generic;

namespace Italbytz.AI.CSP;

public interface IAssignment<TVar, TVal> : ICloneable where TVar : IVariable
{
    IEnumerable<TVar> Variables { get; }

    TVal? GetValue(TVar variable);

    void Add(TVar variable, TVal value);

    void Remove(TVar variable);

    bool Contains(TVar variable);

    bool IsConsistent(IEnumerable<IConstraint<TVar, TVal>> constraints);

    bool IsComplete(IEnumerable<TVar> variables);

    bool IsSolution(ICSP<TVar, TVal> csp);
}
