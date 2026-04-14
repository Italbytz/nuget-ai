using Italbytz.Adapters.Algorithms.AI.Search.Agent;
using Italbytz.Adapters.Algorithms.AI.Search.Framework.Problem;
using Italbytz.Adapters.Algorithms.AI.Search.Local;
using Italbytz.Adapters.Algorithms.AI.Util.Datastructure;
using Italbytz.Adapters.Algorithms.Tests.Environment.NQueens;
using Italbytz.Ports.Algorithms.AI.Agent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AI.Tests.Unit.Search.Local;

[TestClass]
public class NQueensSimulatedAnnealingTests
{
    private const bool ConsoleLogging = true;

    private static ILoggerFactory _loggerFactory =
        NullLoggerFactory.Instance;
    
    [TestInitialize]
    public void TestInitialize()
    {
        if (ConsoleLogging)
            _loggerFactory =
                LoggerFactory.Create(builder => builder.AddConsole());
    }
    
    [TestCleanup]
    public void TestCleanup()
    {
        _loggerFactory.Dispose();
    }
    
    [TestMethod]
    public void TestNQueensBoard1()
    {
        var board = new NQueensBoard(8);
        for (var i = 0; i < board.Size; i++)
            board.AddQueenAt(new XYLocation(i, 0));
        var agent = TestNQueens(board);
        var env = new NQueensEnvironment(board) { Agent = agent };
        while (!agent.Done) env.Step();
        // Optimal solution is not guaranteed
        Assert.IsTrue(env.Board.GetNumberOfAttackingPairs()==0 || env.Board.GetNumberOfAttackingPairs()==28);
    }

    [TestMethod]
    public void TestNQueensBoard2()
    {
        var board = new NQueensBoard(8);
        board.AddQueenAt(new XYLocation(0, 4));
        board.AddQueenAt(new XYLocation(1, 5));
        board.AddQueenAt(new XYLocation(2, 6));
        board.AddQueenAt(new XYLocation(3, 3));
        board.AddQueenAt(new XYLocation(4, 4));
        board.AddQueenAt(new XYLocation(5, 5));
        board.AddQueenAt(new XYLocation(6, 6));
        board.AddQueenAt(new XYLocation(7, 5));
        var agent = TestNQueens(board);
        var env = new NQueensEnvironment(board) { Agent = agent };
        while (!agent.Done) env.Step();
        // Optimal solution is not guaranteed
        Assert.IsTrue(env.Board.GetNumberOfAttackingPairs()==0 || env.Board.GetNumberOfAttackingPairs()==17);
    }

    [TestMethod]
    public void TestNQueensBoard3()
    {
        var board = new NQueensBoard(8);
        board.AddQueenAt(new XYLocation(0, 5));
        board.AddQueenAt(new XYLocation(1, 7));
        board.AddQueenAt(new XYLocation(2, 0));
        board.AddQueenAt(new XYLocation(3, 1));
        board.AddQueenAt(new XYLocation(4, 1));
        board.AddQueenAt(new XYLocation(5, 7));
        board.AddQueenAt(new XYLocation(6, 7));
        board.AddQueenAt(new XYLocation(7, 2));
        var agent = TestNQueens(board);
        var env = new NQueensEnvironment(board) { Agent = agent };
        while (!agent.Done) env.Step();
        // Optimal solution is not guaranteed
        Assert.IsTrue(env.Board.GetNumberOfAttackingPairs()==0 || env.Board.GetNumberOfAttackingPairs()==6);
    }

    private SearchAgent<IPercept, NQueensBoard, QueenAction> TestNQueens(
        NQueensBoard board)
    {
        var problem = new GeneralProblem<NQueensBoard, QueenAction>(board,
            NQueensFunctions.GetCSFActions, NQueensFunctions.GetResult,
            NQueensFunctions.TestGoal);
        var search = new SimulatedAnnealingSearch<NQueensBoard, QueenAction>(
            NQueensFunctions.GetNumberOfAttackingPairs,
            new Scheduler(20, 0.045, 1000));
        var agent = new SearchAgent<IPercept, NQueensBoard, QueenAction>(
            problem, search, _loggerFactory);
        return agent;
    }

}