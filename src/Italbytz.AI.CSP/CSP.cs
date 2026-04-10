using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.CSP;

public class CSP<TVar, TVal> : ICSP<TVar, TVal> where TVar : IVariable
{
    private readonly Dictionary<TVar, IList<IConstraint<TVar, TVal>>> _constraintNetwork = new();

    public CSP(IList<TVar> variables, IList<IDomain<TVal>> domains, IList<IConstraint<TVar, TVal>> constraints) : this(variables)
    {
        if (variables.Count != domains.Count)
        {
            throw new ArgumentException("Each variable must have a matching domain.", nameof(domains));
        }

        for (var i = 0; i < variables.Count; i++)
        {
            Domains[i] = domains[i];
        }

        foreach (var constraint in constraints)
        {
            AddConstraint(constraint);
        }
    }

    public CSP(IList<TVar> variables) : this()
    {
        foreach (var variable in variables)
        {
            AddVariable(variable);
        }
    }

    protected CSP()
    {
        Constraints = new List<IConstraint<TVar, TVal>>();
        Variables = new List<TVar>();
        Domains = new List<IDomain<TVal>>();
    }

    public IList<IConstraint<TVar, TVal>> Constraints { get; }

    public IList<TVar> Variables { get; }

    public IList<IDomain<TVal>> Domains { get; set; }

    public void AddVariable(TVar variable)
    {
        if (_constraintNetwork.ContainsKey(variable))
        {
            throw new ArgumentException("Variable already exists in CSP", nameof(variable));
        }

        Variables.Add(variable);
        Domains.Add(new Domain<TVal>([]));
        _constraintNetwork.Add(variable, new List<IConstraint<TVar, TVal>>());
    }

    public void AddConstraint(IConstraint<TVar, TVal> constraint)
    {
        Constraints.Add(constraint);
        foreach (var variable in constraint.Scope)
        {
            _constraintNetwork[variable].Add(constraint);
        }
    }

    public bool RemoveConstraint(IConstraint<TVar, TVal> constraint)
    {
        if (!Constraints.Remove(constraint))
        {
            return false;
        }

        foreach (var variable in constraint.Scope)
        {
            _constraintNetwork[variable].Remove(constraint);
        }

        return true;
    }

    public void SetDomain(TVar variable, IDomain<TVal> domain)
    {
        Domains[Variables.IndexOf(variable)] = domain;
    }

    public IList<IConstraint<TVar, TVal>> GetConstraints(TVar variable)
    {
        return _constraintNetwork[variable];
    }

    public TVar GetNeighbor(TVar variable, IConstraint<TVar, TVal> constraint)
    {
        if (constraint.Scope.Count != 2)
        {
            throw new ArgumentException("Constraint must involve exactly two variables", nameof(constraint));
        }

        return constraint.Scope.First(candidate => !candidate.Equals(variable));
    }

    public IDomain<TVal> GetDomain(TVar variable)
    {
        return Domains[Variables.IndexOf(variable)];
    }

    public ICSP<TVar, TVal> CopyDomains()
    {
        var copy = (CSP<TVar, TVal>)MemberwiseClone();
        copy.Domains = new List<IDomain<TVal>>(Domains);
        return copy;
    }

    public bool RemoveValueFromDomain(TVar variable, TVal value)
    {
        var currentDomain = GetDomain(variable);
        var values = currentDomain.Where(domainValue => !Equals(domainValue, value)).ToList();
        if (values.Count == currentDomain.Count)
        {
            return false;
        }

        SetDomain(variable, new Domain<TVal>(values));
        return true;
    }
}
