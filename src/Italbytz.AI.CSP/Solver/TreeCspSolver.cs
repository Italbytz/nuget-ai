using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;

namespace Italbytz.AI.CSP.Solver;

public class TreeCspSolver<TVar, TVal> : ICspSolver<TVar, TVal> where TVar : IVariable
{
    private readonly bool _useRandom;

    public TreeCspSolver(bool useRandom)
    {
        _useRandom = useRandom;
    }

    public TreeCspSolver() : this(false)
    {
    }

    public IAssignment<TVar, TVal>? Solve(ICSP<TVar, TVal> csp)
    {
        var assignment = new Assignment<TVar, TVal>();
        var root = _useRandom
            ? csp.Variables[ThreadSafeRandomNetCore.Shared.Next(csp.Variables.Count)]
            : csp.Variables.First();

        var orderedVariables = new List<TVar>();
        var parentConstraints = new Dictionary<TVar, IConstraint<TVar, TVal>>();
        TopologicalSort(csp, root, orderedVariables, parentConstraints);

        if (csp.GetDomain(root).IsEmpty)
        {
            return null;
        }

        csp = csp.CopyDomains();
        for (var i = orderedVariables.Count - 1; i > 0; i--)
        {
            var variable = orderedVariables[i];
            var constraint = parentConstraints[variable];
            var parent = csp.GetNeighbor(variable, constraint);
            if (MakeArcConsistent(csp, parent, variable, constraint) && csp.GetDomain(variable).IsEmpty)
            {
                return null;
            }
        }

        foreach (var variable in orderedVariables)
        {
            foreach (var value in csp.GetDomain(variable))
            {
                assignment.Add(variable, value);
                if (assignment.IsConsistent(csp.GetConstraints(variable)))
                {
                    break;
                }
            }
        }

        return assignment;
    }

    private bool MakeArcConsistent(ICSP<TVar, TVal> csp, TVar parent, TVar variable, IConstraint<TVar, TVal> constraint)
    {
        var currentDomain = csp.GetDomain(parent);
        var newValues = new List<TVal>();
        var assignment = new Assignment<TVar, TVal>();
        foreach (var value in currentDomain)
        {
            assignment.Add(parent, value);
            foreach (var value2 in csp.GetDomain(variable))
            {
                assignment.Add(variable, value2);
                if (!constraint.IsSatisfiedWith(assignment))
                {
                    continue;
                }

                newValues.Add(value);
                break;
            }
            assignment.Remove(variable);
            assignment.Remove(parent);
        }

        if (newValues.Count >= currentDomain.Count)
        {
            return false;
        }

        csp.SetDomain(parent, new Domain<TVal>(newValues));
        return true;
    }

    private static void TopologicalSort(ICSP<TVar, TVal> csp, TVar root, List<TVar> orderedVariables, Dictionary<TVar, IConstraint<TVar, TVal>> parentConstraints)
    {
        orderedVariables.Add(root);
        parentConstraints[root] = null!;
        var currentParentIndex = -1;
        while (currentParentIndex < orderedVariables.Count - 1)
        {
            currentParentIndex++;
            var currentParent = orderedVariables[currentParentIndex];
            var arcsPointingUpwards = 0;
            foreach (var constraint in csp.GetConstraints(currentParent))
            {
                var neighbor = csp.GetNeighbor(currentParent, constraint);
                if (parentConstraints.ContainsKey(neighbor))
                {
                    arcsPointingUpwards++;
                    if (arcsPointingUpwards > 1)
                    {
                        throw new InvalidOperationException("CSP is not a tree");
                    }
                }
                else
                {
                    orderedVariables.Add(neighbor);
                    parentConstraints[neighbor] = constraint;
                }
            }
        }

        if (orderedVariables.Count != csp.Variables.Count)
        {
            throw new InvalidOperationException("CSP is not a tree");
        }
    }
}
