using System.Collections.Generic;

namespace Italbytz.AI.Search.Continuous;

public class LPModel : ILPModel
{
    public bool Maximization { get; set; }

    public double[] ObjectiveFunction { get; set; } = [];

    public (double Lower, double Upper)[] Bounds { get; set; } = [];

    public List<ILPConstraint> Constraints { get; set; } = [];

    public bool[] IntegerVariables { get; set; } = [];
}