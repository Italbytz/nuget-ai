namespace Italbytz.AI.Search.Continuous;

public class LPConstraint : ILPConstraint
{
    public double[] Coefficients { get; set; } = [];

    public ConstraintType ConstraintType { get; set; }

    public double RHS { get; set; }
}