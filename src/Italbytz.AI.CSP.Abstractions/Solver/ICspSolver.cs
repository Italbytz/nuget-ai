namespace Italbytz.AI.CSP.Solver;

public interface ICspSolver<TVar, TVal> where TVar : IVariable
{
    IAssignment<TVar, TVal>? Solve(ICSP<TVar, TVal> csp);
}
