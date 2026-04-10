namespace Italbytz.AI.CSP.Solver.Inference;

public class EmptyInferenceLog<TVar, TVal> : IInferenceLog<TVar, TVal> where TVar : IVariable
{
    public bool IsEmpty { get; } = true;

    public bool InconsistencyFound { get; } = false;

    public void Undo(ICSP<TVar, TVal> csp)
    {
    }
}
