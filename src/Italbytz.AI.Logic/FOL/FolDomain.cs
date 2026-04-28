using System.Collections.Generic;

namespace Italbytz.AI.Logic.Fol;

public class FolDomain
{
    public ISet<string> Constants { get; } = new HashSet<string>();

    public ISet<string> Functions { get; } = new HashSet<string>();

    public ISet<string> Predicates { get; } = new HashSet<string>();

    public FolDomain AddConstant(params string[] constants)
    {
        foreach (var constant in constants)
        {
            Constants.Add(constant);
        }

        return this;
    }

    public FolDomain AddFunction(params string[] functions)
    {
        foreach (var function in functions)
        {
            Functions.Add(function);
        }

        return this;
    }

    public FolDomain AddPredicate(params string[] predicates)
    {
        foreach (var predicate in predicates)
        {
            Predicates.Add(predicate);
        }

        return this;
    }
}