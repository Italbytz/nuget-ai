using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Italbytz.AI.Tests;

[TestClass]
[DoNotParallelize]
public class ThreadSafeRandomNetCoreTests
{
    private static readonly int[] ExpectedWithSeed42 =
    [
        67, 14, 13, 52, 17, 26, 72, 51, 18, 76
    ];

    [TestMethod]
    public void FixedSeedProducesExpectedSequence()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        try
        {
            var random = ThreadSafeRandomNetCore.Shared;

            var numbers = new int[10];
            for (var i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(1, 100);
            }

            CollectionAssert.AreEqual(ExpectedWithSeed42, numbers);
        }
        finally
        {
            ThreadSafeRandomNetCore.Seed = null;
        }
    }

    [TestMethod]
    public void ResettingFixedSeedReproducesSameSequence()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        try
        {
            var firstRun = Enumerable.Range(0, 10)
                .Select(_ => ThreadSafeRandomNetCore.Shared.Next(1, 100))
                .ToArray();

            ThreadSafeRandomNetCore.Seed = 42;
            var secondRun = Enumerable.Range(0, 10)
                .Select(_ => ThreadSafeRandomNetCore.Shared.Next(1, 100))
                .ToArray();

            CollectionAssert.AreEqual(firstRun, secondRun);
        }
        finally
        {
            ThreadSafeRandomNetCore.Seed = null;
        }
    }

    [TestMethod]
    public void MultipleThreadsWithDefaultSeedProduceDistinctSequences()
    {
        ThreadSafeRandomNetCore.Seed = null;

        var results = new ConcurrentBag<string>();
        Parallel.For(0, 5, _ =>
        {
            var numbers = Enumerable.Range(0, 10)
                .Select(__ => ThreadSafeRandomNetCore.Shared.Next(1, 100))
                .ToArray();
            results.Add(string.Join(",", numbers));
        });

        Assert.AreEqual(5, results.Distinct().Count());
    }

    [TestMethod]
    public void SharedRandomRemainsUsableUnderParallelLoad()
    {
        ThreadSafeRandomNetCore.Seed = 42;
        try
        {
            var rng = ThreadSafeRandomNetCore.Shared;

            Parallel.For(0, 5, _ =>
            {
                var numbers = new int[10000];
                for (var i = 0; i < numbers.Length; i++)
                {
                    numbers[i] = rng.Next();
                }

                var zeroCount = numbers.Count(x => x == 0);
                Assert.AreEqual(0, zeroCount);
            });
        }
        finally
        {
            ThreadSafeRandomNetCore.Seed = null;
        }
    }
}
