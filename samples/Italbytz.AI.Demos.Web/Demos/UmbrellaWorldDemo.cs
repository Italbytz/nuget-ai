using Italbytz.AI.Probability.HMM;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record UmbrellaWorldStep(
    int Day,
    bool UmbrellaObserved,
    double RainFiltered,
    double RainSmoothed,
    string Summary);

internal sealed record UmbrellaWorldRun(
    string Name,
    string Summary,
    IReadOnlyList<bool> Observations,
    IReadOnlyList<UmbrellaWorldStep> Steps);

internal static class UmbrellaWorldDemoFactory
{
    public static UmbrellaWorldRun BuildCanonicalRun()
    {
        return BuildRun(
            "Canonical sequence",
            "A compact observation sequence that makes smoothing visible: later non-umbrella evidence pulls down the earlier rain estimates.",
            [true, true, false, true, false]);
    }

    public static UmbrellaWorldRun BuildRandomRun(Random random, int days = 5)
    {
        var observations = Enumerable.Range(0, days)
            .Select(_ => random.Next(2) == 0)
            .ToArray();

        return BuildRun(
            "Randomized sequence",
            "A randomized umbrella observation sequence for inspecting how filtering and smoothing react to different evidence patterns.",
            observations);
    }

    private static UmbrellaWorldRun BuildRun(string name, string summary, IReadOnlyList<bool> observations)
    {
        var model = UmbrellaWorld.Build();
        var forwardBackward = new ForwardBackwardAlgorithm();
        var prior = model.Prior.ToArray();
        var filtered = new List<double[]>();
        var current = prior;

        foreach (var observation in observations)
        {
            current = forwardBackward.Forward(model, current, observation);
            filtered.Add(current);
        }

        var smoothed = forwardBackward.ForwardBackward(model, observations.Cast<object>().ToArray(), prior);
        var steps = observations
            .Select((observation, index) => new UmbrellaWorldStep(
                index + 1,
                observation,
                filtered[index][0],
                smoothed[index][0],
                CreateSummary(index + 1, observation, filtered[index][0], smoothed[index][0])))
            .ToArray();

        return new UmbrellaWorldRun(name, summary, observations, steps);
    }

    private static string CreateSummary(int day, bool observation, double filteredRain, double smoothedRain)
    {
        var evidence = observation ? "umbrella" : "no umbrella";
        var direction = smoothedRain >= filteredRain ? "raises" : "lowers";
        return $"Day {day}: observing {evidence} yields filtered P(Rain)= {FormatProbability(filteredRain)}; using future evidence {direction} that estimate to {FormatProbability(smoothedRain)}.";
    }

    public static string FormatObservation(bool observation)
    {
        return observation ? "umbrella" : "no umbrella";
    }

    public static string FormatProbability(double value)
    {
        return value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string FormatPercent(double value)
    {
        return value.ToString("P0", System.Globalization.CultureInfo.InvariantCulture);
    }
}