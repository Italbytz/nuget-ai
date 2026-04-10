using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public class ActionSchema : IActionSchema
{
    public ActionSchema(string name, List<ITerm>? variables, string precondition, string effect)
        : this(name, variables, Utils.Parse(precondition), Utils.Parse(effect))
    {
    }

    protected ActionSchema(string name, List<ITerm>? variables, IList<ILiteral> precondition, IList<ILiteral> effect)
    {
        Variables = variables ?? new List<ITerm>();
        Name = name;
        Precondition = precondition;
        Effect = effect;
    }

    public string Name { get; }

    public List<ITerm> Variables { get; }

    public IList<ILiteral> Precondition { get; }

    public IList<ILiteral> Effect { get; }

    public bool Equals(IActionSchema? other)
    {
        return other is not null
            && string.Equals(Name, other.Name, StringComparison.Ordinal)
            && Variables.SequenceEqual(other.Variables)
            && Precondition.SequenceEqual(other.Precondition)
            && Effect.SequenceEqual(other.Effect);
    }

    public override bool Equals(object? obj)
    {
        return obj is IActionSchema actionSchema && Equals(actionSchema);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        foreach (var variable in Variables)
        {
            hash.Add(variable);
        }
        foreach (var literal in Precondition)
        {
            hash.Add(literal);
        }
        foreach (var literal in Effect)
        {
            hash.Add(literal);
        }
        return hash.ToHashCode();
    }

    public IActionSchema GetActionBySubstitution(IEnumerable<IConstant> constants)
    {
        var substitutions = constants.ToList();
        var newPrecondition = SubstituteLiterals(Precondition, substitutions);
        var newEffect = SubstituteLiterals(Effect, substitutions);
        return new ActionSchema(Name, substitutions.Cast<ITerm>().ToList(), newPrecondition, newEffect);
    }

    public IList<IConstant> GetConstants()
    {
        var result = new List<IConstant>();
        foreach (var literal in Precondition.Concat(Effect))
        {
            foreach (var term in literal.Atom.Args)
            {
                if (term is IConstant constant && !result.Contains(constant))
                {
                    result.Add(constant);
                }
            }
        }

        return result;
    }

    private IList<ILiteral> SubstituteLiterals(IEnumerable<ILiteral> source, IReadOnlyList<IConstant> constants)
    {
        var result = new List<ILiteral>();
        foreach (var literal in source)
        {
            var newTerms = new List<ITerm>();
            foreach (var term in literal.Atom.Args)
            {
                if (term is Variable variable)
                {
                    var index = Variables.FindLastIndex(candidate => Equals(candidate, variable));
                    newTerms.Add(index >= 0 ? constants[index] : variable);
                }
                else
                {
                    newTerms.Add(term);
                }
            }

            result.Add(new Literal(new Predicate(literal.Atom.SymbolicName, newTerms), literal.NegativeLiteral));
        }

        return result;
    }
}
