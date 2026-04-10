using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;

namespace Italbytz.AI.CSP.Solver;

public class MinConflictsSolver<TVar, TVal> : AbstractCspSolver<TVar, TVal> where TVar : IVariable
{
    private readonly int _maxSteps;

    public MinConflictsSolver(int maxSteps)
    {
        _maxSteps = maxSteps;
    }

    public override IAssignment<TVar, TVal>? Solve(ICSP<TVar, TVal> csp)
    {
        var current = GenerateRandomAssignment(csp);
        for (var i = 0; i < _maxSteps; i++)
        {
            if (current.IsSolution(csp))
            {
                return current;
            }

            var variables = GetConflictedVariables(csp, current).ToList();
            var variable = variables[ThreadSafeRandomNetCore.Shared.Next(variables.Count)];
            var value = GetMinConflictValue(csp, variable, current);
            current.Add(variable, value);
        }

        return null;
    }

    private TVal GetMinConflictValue(ICSP<TVar, TVal> csp, TVar variable, IAssignment<TVar, TVal> assignment)
    {
        var constraints = csp.GetConstraints(variable);
        var testAssignment = (IAssignment<TVar, TVal>)assignment.Clone();
        var minConflicts = int.MaxValue;
        var resultCandidates = new List<TVal>();
        foreach (var value in csp.GetDomain(variable))
        {
            testAssignment.Add(variable, value);
            var conflicts = constraints.Count(constraint => !constraint.IsSatisfiedWith(testAssignment));
            if (conflicts < minConflicts)
            {
                minConflicts = conflicts;
                resultCandidates.Clear();
                resultCandidates.Add(value);
            }
            else if (conflicts == minConflicts)
            {
                resultCandidates.Add(value);
            }

            testAssignment.Remove(variable);
        }

        return resultCandidates[ThreadSafeRandomNetCore.Shared.Next(resultCandidates.Count)];
    }

    private ISet<TVar> GetConflictedVariables(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> current)
    {
        return new HashSet<TVar>(csp.Variables.Where(variable => csp.GetConstraints(variable).Any(constraint => !constraint.IsSatisfiedWith(current))));
    }

    private IAssignment<TVar, TVal> GenerateRandomAssignment(ICSP<TVar, TVal> csp)
    {
        var assignment = new Assignment<TVar, TVal>();
        foreach (var variable in csp.Variables)
        {
            var domain = csp.GetDomain(variable);
            assignment.Add(variable, domain[ThreadSafeRandomNetCore.Shared.Next(domain.Count)]);
        }

        return assignment;
    }
}
