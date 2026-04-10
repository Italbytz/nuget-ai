using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.CSP.Solver.Inference;

namespace Italbytz.AI.CSP.Solver;

public class FlexibleBacktrackingSolver<TVar, TVal> : AbstractBacktrackingSolver<TVar, TVal> where TVar : IVariable
{
    public IInferenceStrategy<TVar, TVal>? InferenceStrategy { get; set; }

    public CspHeuristics.IVariableSelectionStrategy<TVar, TVal>? VariableSelectionStrategy { get; set; }

    public CspHeuristics.IValueOrderingStrategy<TVar, TVal>? ValueOrderingStrategy { get; set; }

    public override IAssignment<TVar, TVal>? Solve(ICSP<TVar, TVal> csp)
    {
        if (InferenceStrategy is null)
        {
            return base.Solve(csp);
        }

        csp = csp.CopyDomains();
        var log = InferenceStrategy.Apply(csp);
        return log is { IsEmpty: false, InconsistencyFound: true }
            ? null
            : base.Solve(csp);
    }

    protected override IInferenceLog<TVar, TVal> Infer(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> assignment, TVar variable)
    {
        return InferenceStrategy is not null
            ? InferenceStrategy.Apply(csp, assignment, variable)
            : new EmptyInferenceLog<TVar, TVal>();
    }

    protected override IEnumerable<TVal> OrderDomainValues(ICSP<TVar, TVal> csp, TVar variable, IAssignment<TVar, TVal> assignment)
    {
        return ValueOrderingStrategy is not null
            ? ValueOrderingStrategy.Apply(csp, assignment, variable)
            : csp.GetDomain(variable);
    }

    protected override TVar SelectUnassignedVariable(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> assignment)
    {
        var unassigned = csp.Variables.Where(variable => !assignment.Contains(variable)).ToList();
        return VariableSelectionStrategy is not null
            ? VariableSelectionStrategy.Apply(csp, unassigned).First()
            : unassigned.First();
    }
}
