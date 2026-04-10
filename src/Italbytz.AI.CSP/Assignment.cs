using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.CSP;

public class Assignment<TVar, TVal> : IAssignment<TVar, TVal> where TVar : IVariable
{
    private Dictionary<TVar, TVal> _variableToValueMap = new();

    public IEnumerable<TVar> Variables => new List<TVar>(_variableToValueMap.Keys);

    public TVal? GetValue(TVar variable)
    {
        return _variableToValueMap.TryGetValue(variable, out var value) ? value : default;
    }

    public void Add(TVar variable, TVal value)
    {
        _variableToValueMap[variable] = value;
    }

    public void Remove(TVar variable)
    {
        _variableToValueMap.Remove(variable);
    }

    public bool Contains(TVar variable)
    {
        return _variableToValueMap.ContainsKey(variable);
    }

    public bool IsConsistent(IEnumerable<IConstraint<TVar, TVal>> constraints)
    {
        return constraints.All(constraint => constraint.IsSatisfiedWith(this));
    }

    public bool IsComplete(IEnumerable<TVar> variables)
    {
        return variables.All(Contains);
    }

    public bool IsSolution(ICSP<TVar, TVal> csp)
    {
        return IsConsistent(csp.Constraints) && IsComplete(csp.Variables);
    }

    public object Clone()
    {
        var result = (Assignment<TVar, TVal>)MemberwiseClone();
        result._variableToValueMap = new Dictionary<TVar, TVal>(_variableToValueMap);
        return result;
    }

    public override string ToString()
    {
        return string.Join("\n", _variableToValueMap.Select(entry => $"{entry.Key} = {entry.Value}"));
    }
}
