using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.CSP.Solver;

public class CspHeuristics
{
    public static IVariableSelectionStrategy<TVar, TVal> Mrv<TVar, TVal>() where TVar : IVariable
    {
        return new MinimumRemainingValuesHeuristic<TVar, TVal>();
    }

    public static IValueOrderingStrategy<TVar, TVal> Lcv<TVar, TVal>() where TVar : IVariable
    {
        return new LeastConstrainingValueHeuristic<TVar, TVal>();
    }

    public static IVariableSelectionStrategy<TVar, TVal> Deg<TVar, TVal>() where TVar : IVariable
    {
        return new DegreeHeuristic<TVar, TVal>();
    }

    public static IVariableSelectionStrategy<TVar, TVal> MrvDeg<TVar, TVal>() where TVar : IVariable
    {
        return new MrvDegHeuristic<TVar, TVal>();
    }

    public interface IVariableSelectionStrategy<TVar, TVal> where TVar : IVariable
    {
        IList<TVar> Apply(ICSP<TVar, TVal> csp, IList<TVar> variables);
    }

    public interface IValueOrderingStrategy<TVar, TVal> where TVar : IVariable
    {
        IList<TVal> Apply(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> assignment, TVar variable);
    }

    public class MinimumRemainingValuesHeuristic<TVar, TVal> : IVariableSelectionStrategy<TVar, TVal> where TVar : IVariable
    {
        public IList<TVar> Apply(ICSP<TVar, TVal> csp, IList<TVar> variables)
        {
            var minDomain = int.MaxValue;
            var minDomainVariables = new List<TVar>();
            foreach (var variable in variables)
            {
                var domainSize = csp.GetDomain(variable).Count;
                if (domainSize < minDomain)
                {
                    minDomain = domainSize;
                    minDomainVariables.Clear();
                    minDomainVariables.Add(variable);
                }
                else if (domainSize == minDomain)
                {
                    minDomainVariables.Add(variable);
                }
            }

            return minDomainVariables;
        }
    }

    public class DegreeHeuristic<TVar, TVal> : IVariableSelectionStrategy<TVar, TVal> where TVar : IVariable
    {
        public IList<TVar> Apply(ICSP<TVar, TVal> csp, IList<TVar> variables)
        {
            var maxDegree = 0;
            var maxDegreeVariables = new List<TVar>();
            foreach (var variable in variables)
            {
                var degree = 0;
                foreach (var constraint in csp.GetConstraints(variable))
                {
                    degree += constraint.Scope.Count;
                }

                if (degree > maxDegree)
                {
                    maxDegree = degree;
                    maxDegreeVariables.Clear();
                    maxDegreeVariables.Add(variable);
                }
                else if (degree == maxDegree)
                {
                    maxDegreeVariables.Add(variable);
                }
            }

            return maxDegreeVariables;
        }
    }

    public class MrvDegHeuristic<TVar, TVal> : IVariableSelectionStrategy<TVar, TVal> where TVar : IVariable
    {
        public IList<TVar> Apply(ICSP<TVar, TVal> csp, IList<TVar> variables)
        {
            var mrvVariables = new MinimumRemainingValuesHeuristic<TVar, TVal>().Apply(csp, variables);
            return new DegreeHeuristic<TVar, TVal>().Apply(csp, mrvVariables);
        }
    }

    public class LeastConstrainingValueHeuristic<TVar, TVal> : IValueOrderingStrategy<TVar, TVal> where TVar : IVariable
    {
        public IList<TVal> Apply(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> assignment, TVar variable)
        {
            var values = csp.GetDomain(variable);
            var rankedValues = new List<(TVal Value, int Score)>();
            foreach (var value in values)
            {
                var score = 0;
                assignment.Add(variable, value);
                foreach (var constraint in csp.GetConstraints(variable))
                {
                    if (!constraint.Scope.Contains(variable))
                    {
                        continue;
                    }

                    foreach (var otherVariable in constraint.Scope)
                    {
                        if (otherVariable.Equals(variable) || assignment.Contains(otherVariable))
                        {
                            continue;
                        }

                        foreach (var otherValue in csp.GetDomain(otherVariable))
                        {
                            assignment.Add(otherVariable, otherValue);
                            if (constraint.IsSatisfiedWith(assignment))
                            {
                                score++;
                            }
                            assignment.Remove(otherVariable);
                        }
                    }
                }

                assignment.Remove(variable);
                rankedValues.Add((value, score));
            }

            return rankedValues.OrderByDescending(entry => entry.Score).Select(entry => entry.Value).ToList();
        }
    }
}
