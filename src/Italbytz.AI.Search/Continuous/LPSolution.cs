namespace Italbytz.AI.Search.Continuous;

public class LPSolution : ILPSolution
{
    public double Objective { get; set; }

    public double[] Solution { get; set; } = [];
}