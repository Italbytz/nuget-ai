namespace Italbytz.AI.CSP.Solver.Inference;

public interface IInferenceStrategy<TVar, TVal> where TVar : IVariable
{
    IInferenceLog<TVar, TVal> Apply(ICSP<TVar, TVal> csp);

    IInferenceLog<TVar, TVal> Apply(ICSP<TVar, TVal> csp, IAssignment<TVar, TVal> assignment, TVar variable);
}
