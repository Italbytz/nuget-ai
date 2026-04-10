using System.Collections.Generic;

namespace Italbytz.AI.CSP;

public interface ICSP<TVar, TVal> where TVar : IVariable
{
    IList<IConstraint<TVar, TVal>> Constraints { get; }

    IList<TVar> Variables { get; }

    IList<IDomain<TVal>> Domains { get; set; }

    void AddConstraint(IConstraint<TVar, TVal> constraint);

    void AddVariable(TVar variable);

    IDomain<TVal> GetDomain(TVar variable);

    IList<IConstraint<TVar, TVal>> GetConstraints(TVar variable);

    TVar GetNeighbor(TVar variable, IConstraint<TVar, TVal> constraint);

    bool RemoveConstraint(IConstraint<TVar, TVal> constraint);

    void SetDomain(TVar variable, IDomain<TVal> domain);

    ICSP<TVar, TVal> CopyDomains();

    bool RemoveValueFromDomain(TVar variable, TVal value);
}
