using Italbytz.AI.Search.Continuous;

namespace Italbytz.AI.Tests;

[TestClass]
public class LinearProgrammingRegressionTests
{
    private const string LegacyIntegerLp = """
                                     // Objective function
                                     max: + 6*x0 + 5*x1;
                                     // constraints
                                      + 1*x0 + 1*x1 <= 5;
                                      + 3*x0 + 2*x1 <= 12;
                                     """;

    private const string LegacyFractionalLp = """
                                        // Objective function
                                        max: + 5*x0 + 6*x1;
                                        // constraints
                                         + 1*x0 + 1*x1 <= 5;
                                         + 4*x0 + 7*x1 <= 28;
                                        """;

    private const string LegacyIntegerIlp = """
                                      // Objective function
                                      max: + 5*x0 + 6*x1;
                                      // constraints
                                       + 1*x0 + 1*x1 <= 5;
                                       + 4*x0 + 7*x1 <= 28;
                                      // declaration
                                      int x0, x1;
                                      """;

    private const string LegacyIntegerMps = """
                                       NAME
                                       ROWS
                                        N  R0
                                        L  R1
                                        L  R2
                                       COLUMNS
                                           C1        R0        -6.000000000   R1        1.0000000000
                                           C1        R2        3.0000000000
                                           C2        R0        -5.000000000   R1        1.0000000000
                                           C2        R2        2.0000000000
                                       RHS
                                           RHS       R1        5.0000000000   R2        12.000000000
                                       ENDATA
                                       """;

    private const string LegacyFractionalMps = """
                                          NAME
                                          ROWS
                                           N  R0
                                           L  R1
                                           L  R2
                                          COLUMNS
                                              C1        R0        -5.000000000   R1        1.0000000000
                                              C1        R2        4.0000000000
                                              C2        R0        -6.000000000   R1        1.0000000000
                                              C2        R2        7.0000000000
                                          RHS
                                              RHS       R1        5.0000000000   R2        28.000000000
                                          ENDATA
                                          """;

    private const string LegacyIntegerMarkedMps = """
                                              NAME
                                              ROWS
                                               N  R0
                                               L  R1
                                               L  R2
                                              COLUMNS
                                                  MARK0000  'MARKER'                 'INTORG'
                                                  C1        R0        -5.000000000   R1        1.0000000000
                                                  C1        R2        4.0000000000
                                                  C2        R0        -6.000000000   R1        1.0000000000
                                                  C2        R2        7.0000000000
                                                  MARK0001  'MARKER'                 'INTEND'
                                              RHS
                                                  RHS       R1        5.0000000000   R2        28.000000000
                                              BOUNDS
                                               LO BND       C1        0.0000000000
                                               LO BND       C2        0.0000000000
                                              ENDATA
                                              """;

    [TestMethod]
    public void LpSolverSolvesLegacyTwoVariableMaximizationModel()
    {
        var solver = new LPSolver();
        var model = new LPModel
        {
            Maximization = true,
            ObjectiveFunction = [6.0, 5.0],
            Bounds = [(0.0, double.PositiveInfinity), (0.0, double.PositiveInfinity)],
            IntegerVariables = [false, false],
            Constraints =
            [
                new LPConstraint { Coefficients = [1.0, 1.0], ConstraintType = ConstraintType.LE, RHS = 5.0 },
                new LPConstraint { Coefficients = [3.0, 2.0], ConstraintType = ConstraintType.LE, RHS = 12.0 }
            ]
        };

        var solution = solver.Solve(model);

        Assert.AreEqual(27.0, solution.Objective, 0.0001);
        Assert.AreEqual(2.0, solution.Solution[0], 0.0001);
        Assert.AreEqual(3.0, solution.Solution[1], 0.0001);
    }

    [TestMethod]
    public void LpSolverSolvesLegacyFractionalLpModel()
    {
        var solver = new LPSolver();
        var model = new LPModel
        {
            Maximization = true,
            ObjectiveFunction = [5.0, 6.0],
            Bounds = [(0.0, double.PositiveInfinity), (0.0, double.PositiveInfinity)],
            IntegerVariables = [false, false],
            Constraints =
            [
                new LPConstraint { Coefficients = [1.0, 1.0], ConstraintType = ConstraintType.LE, RHS = 5.0 },
                new LPConstraint { Coefficients = [4.0, 7.0], ConstraintType = ConstraintType.LE, RHS = 28.0 }
            ]
        };

        var solution = solver.Solve(model);

        Assert.AreEqual(27.66, solution.Objective, 0.01);
        Assert.AreEqual(2.33, solution.Solution[0], 0.01);
        Assert.AreEqual(2.66, solution.Solution[1], 0.01);
    }

    [TestMethod]
    public void LpSolverSolvesLegacyIntegerIlpModelWithBranchAndBound()
    {
        var solver = new LPSolver();
        var model = new LPModel
        {
            Maximization = true,
            ObjectiveFunction = [5.0, 6.0],
            Bounds = [(0.0, double.PositiveInfinity), (0.0, double.PositiveInfinity)],
            IntegerVariables = [true, true],
            Constraints =
            [
                new LPConstraint { Coefficients = [1.0, 1.0], ConstraintType = ConstraintType.LE, RHS = 5.0 },
                new LPConstraint { Coefficients = [4.0, 7.0], ConstraintType = ConstraintType.LE, RHS = 28.0 }
            ]
        };

        var solution = solver.Solve(model);

        Assert.AreEqual(27.0, solution.Objective, 0.0001);
        Assert.AreEqual(3.0, solution.Solution[0], 0.0001);
        Assert.AreEqual(2.0, solution.Solution[1], 0.0001);
    }

    [TestMethod]
    public void LpSolverParsesLegacyLpSolveTextForContinuousCase()
    {
        var solver = new LPSolver();

        var solution = solver.Solve(LegacyFractionalLp, LPFileFormat.lp_solve);

        Assert.AreEqual(27.66, solution.Objective, 0.01);
        Assert.AreEqual(2.33, solution.Solution[0], 0.01);
        Assert.AreEqual(2.66, solution.Solution[1], 0.01);
    }

    [TestMethod]
    public void LpSolverParsesLegacyLpSolveTextForIntegerCase()
    {
        var solver = new LPSolver();

        var solution = solver.Solve(LegacyIntegerIlp, LPFileFormat.lp_solve);

        Assert.AreEqual(27.0, solution.Objective, 0.0001);
        Assert.AreEqual(3.0, solution.Solution[0], 0.0001);
        Assert.AreEqual(2.0, solution.Solution[1], 0.0001);
    }

    [TestMethod]
    public void LpSolverParsesLegacyMpsFixedForContinuousCase()
    {
        var solver = new LPSolver();

        var solution = solver.Solve(LegacyFractionalMps, LPFileFormat.MPS_FIXED);

        Assert.AreEqual(-27.66, solution.Objective, 0.01);
        Assert.AreEqual(2.33, solution.Solution[0], 0.01);
        Assert.AreEqual(2.66, solution.Solution[1], 0.01);
    }

    [TestMethod]
    public void LpSolverParsesLegacyMpsFixedForIntegerMarkedCase()
    {
        var solver = new LPSolver();

        var solution = solver.Solve(LegacyIntegerMarkedMps, LPFileFormat.MPS_FIXED);

        Assert.AreEqual(-27.0, solution.Objective, 0.0001);
        Assert.AreEqual(3.0, solution.Solution[0], 0.0001);
        Assert.AreEqual(2.0, solution.Solution[1], 0.0001);
    }

    [TestMethod]
    public void LpSolverReadsLegacyLpSolveAndMpsModelsFromFiles()
    {
        var solver = new LPSolver();
        var lpFile = Path.GetTempFileName();
        var mpsFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(lpFile, LegacyIntegerLp);
            File.WriteAllText(mpsFile, LegacyIntegerMps);

            var lpSolution = solver.SolveFile(lpFile, LPFileFormat.lp_solve);
            var mpsSolution = solver.SolveFile(mpsFile, LPFileFormat.MPS_FIXED);

            Assert.AreEqual(27.0, lpSolution.Objective, 0.0001);
            Assert.AreEqual(2.0, lpSolution.Solution[0], 0.0001);
            Assert.AreEqual(3.0, lpSolution.Solution[1], 0.0001);

            Assert.AreEqual(-27.0, mpsSolution.Objective, 0.0001);
            Assert.AreEqual(2.0, mpsSolution.Solution[0], 0.0001);
            Assert.AreEqual(3.0, mpsSolution.Solution[1], 0.0001);
        }
        finally
        {
            File.Delete(lpFile);
            File.Delete(mpsFile);
        }
    }

    [TestMethod]
    public void LpSolverRejectsUnsupportedMpsVariants()
    {
        var solver = new LPSolver();

        AssertUnsupportedFormat(() => solver.Solve(LegacyIntegerMps, LPFileFormat.MPS_FREE));
        AssertUnsupportedFormat(() => solver.Solve(LegacyIntegerMps, LPFileFormat.MPS_IBM));
    }

    private static void AssertUnsupportedFormat(Action action)
    {
        try
        {
            action();
            Assert.Fail("Expected a NotSupportedException for unsupported formats.");
        }
        catch (NotSupportedException)
        {
        }
    }
}