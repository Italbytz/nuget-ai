namespace Italbytz.AI.CSP.Solver.Inference;

public interface IInferenceLog<TVar, TVal> where TVar : IVariable
{
    bool IsEmpty { get; }

    bool InconsistencyFound { get; }

    void Undo(ICSP<TVar, TVal> csp);
}
