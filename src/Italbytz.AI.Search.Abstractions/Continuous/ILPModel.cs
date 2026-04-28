using System.Collections.Generic;

namespace Italbytz.AI.Search.Continuous;

public interface ILPModel
{
    bool Maximization { get; set; }

    double[] ObjectiveFunction { get; set; }

    (double Lower, double Upper)[] Bounds { get; set; }

    List<ILPConstraint> Constraints { get; set; }

    bool[] IntegerVariables { get; set; }
}