using Italbytz.Adapters.Algorithms.AI.Search.Local;
using Italbytz.Adapters.Algorithms.AI.Util.Datastructure;
using Italbytz.Adapters.Algorithms.Tests.Environment.NQueens;
using Italbytz.Ports.Algorithms.AI.Search.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AI.Tests.Unit.Search.Local;

[TestClass]
public class NQueensGeneticAlgorithmTests
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
    public void TestNQueens()
    {
        var alphabet = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7
        };
        var algo = new GeneticAlgorithm<int>(8, alphabet, 0.3);
        var initPopulation = new List<IIndividual<int>>();
        var random = new Random();
        for (var i = 0; i < 100; i++)
        {
            var randomRepresentation = new List<int>(8);
            for (var j = 0; j < 8; j++)
                randomRepresentation.Add(random.Next(8));
            initPopulation.Add(new Individual<int>(randomRepresentation));
        }

        var result = algo.Execute(initPopulation, FitnessFn, 100);
        var finalFitness = FitnessFn(result);

        Assert.IsTrue(Math.Abs(finalFitness - 1.0) < 0.01);
        
        return;

        double FitnessFn(IIndividual<int> individual)
        {
            var board = new NQueensBoard(8);
            var x = 0;
            foreach (var y in individual.Representation)
            {
                board.AddQueenAt(new XYLocation(x, y));
                x++;
            }

            return 1.0 / (1.0 + board.GetNumberOfAttackingPairs());
        }
    }
}