namespace Italbytz.AI.Search.Continuous;

public interface ILPSolution
{
    double Objective { get; set; }

    double[] Solution { get; set; }
}