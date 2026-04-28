namespace Italbytz.AI.Search.Continuous;

public interface ILPConstraint
{
    double[] Coefficients { get; set; }

    ConstraintType ConstraintType { get; set; }

    double RHS { get; set; }
}