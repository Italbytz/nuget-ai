namespace Italbytz.AI.Search.Continuous;

public interface ILPSolver
{
    ILPSolution Solve(ILPModel model);

    ILPSolution Solve(string model, LPFileFormat format);

    ILPSolution SolveFile(string filename, LPFileFormat format);
}