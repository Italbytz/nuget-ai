using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Italbytz.AI;

/// <summary>
/// Provides thread-safe random number generation with deterministic support for fixed seeds.
/// </summary>
public class ThreadSafeRandomNetCore : Random
{
    private static int? _seed;
    private static int _seedGeneration;
    private static int _seedCounter = Environment.TickCount;

    [ThreadStatic]
    private static int _localGeneration;

    [ThreadStatic]
    private static Random? _threadRandom;

    /// <summary>
    /// Gets or sets the seed for random number generation.
    /// </summary>
    public static int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _seedCounter = value ?? Environment.TickCount;
            Interlocked.Increment(ref _seedGeneration);
            _threadRandom = null;
        }
    }

    public new static Random Shared { get; } = new ThreadSafeRandomNetCore();

    private static Random LocalRandom
    {
        get
        {
            if (_threadRandom is null || _localGeneration != _seedGeneration)
            {
                _localGeneration = _seedGeneration;
                _threadRandom = Create();
            }

            return _threadRandom;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Random Create()
    {
        var seed = _seed ?? Interlocked.Increment(ref _seedCounter);
        return new Random(seed);
    }

    public override int Next()
    {
        var result = LocalRandom.Next();
        AssertInRange(result, 0, int.MaxValue);
        return result;
    }

    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        var result = LocalRandom.Next(maxValue);
        AssertInRange(result, 0, maxValue);
        return result;
    }

    public override int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal to maxValue");
        }

        var result = LocalRandom.Next(minValue, maxValue);
        AssertInRange(result, minValue, maxValue);
        return result;
    }

    public override long NextInt64()
    {
        var result = LocalRandom.NextInt64();
        AssertInRange(result, 0, long.MaxValue);
        return result;
    }

    public override long NextInt64(long maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        var result = LocalRandom.NextInt64(maxValue);
        AssertInRange(result, 0, maxValue);
        return result;
    }

    public override long NextInt64(long minValue, long maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal to maxValue");
        }

        var result = LocalRandom.NextInt64(minValue, maxValue);
        AssertInRange(result, minValue, maxValue);
        return result;
    }

    public override float NextSingle()
    {
        var result = LocalRandom.NextSingle();
        Debug.Assert(result >= 0.0f && result < 1.0f, $"Expected 0.0 <= {result} < 1.0");
        return result;
    }

    public override double NextDouble()
    {
        var result = LocalRandom.NextDouble();
        Debug.Assert(result >= 0.0 && result < 1.0, $"Expected 0.0 <= {result} < 1.0");
        return result;
    }

    public override void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        LocalRandom.NextBytes(buffer);
    }

    public override void NextBytes(Span<byte> buffer)
    {
        LocalRandom.NextBytes(buffer);
    }

    protected override double Sample()
    {
        return LocalRandom.NextDouble();
    }

    private static void AssertInRange(long result, long minInclusive, long maxExclusive)
    {
        if (maxExclusive > minInclusive)
        {
            Debug.Assert(result >= minInclusive && result < maxExclusive,
                $"Expected {minInclusive} <= {result} < {maxExclusive}");
        }
        else
        {
            Debug.Assert(result == minInclusive, $"Expected {minInclusive} == {result}");
        }
    }
}
