using Italbytz.AI.Probability;
using Italbytz.AI.Probability.Demo;
using Italbytz.AI.Probability.MDP;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record GridWorldTransitionOutcome(
    string NextState,
    double Probability);

internal sealed record GridWorldActionEvaluation(
    string Action,
    double ExpectedUtility,
    IReadOnlyList<GridWorldTransitionOutcome> Outcomes);

internal sealed record GridWorldInspectionStep(
    int Number,
    string State,
    double Utility,
    string? RecommendedAction,
    string Summary,
    IReadOnlyList<GridWorldActionEvaluation> ActionEvaluations);

internal sealed record GridWorldAlgorithmDemo(
    string Key,
    string Name,
    string Summary,
    int Iterations,
    IReadOnlyDictionary<string, double> Utilities,
    IReadOnlyDictionary<string, string> Policy,
    IReadOnlyList<GridWorldInspectionStep> Steps);

internal static class GridWorldDemoFactory
{
    private static readonly string[] StateOrder =
    [
        "1,1", "1,2", "1,3", "1,4",
        "2,1", "2,3", "2,4",
        "3,1", "3,2", "3,3", "3,4"
    ];

    public static IReadOnlyList<GridWorldAlgorithmDemo> BuildAll()
    {
        var mdp = new GridWorldMdp();

        return
        [
            BuildDemo(
                "ValueIteration",
                "Value iteration",
                "Bellman updates repeatedly refine utility estimates until the whole grid stabilizes under the optimality equation.",
                new ValueIteration<string, string>(),
                mdp),
            BuildDemo(
                "PolicyIteration",
                "Policy iteration",
                "Alternates between evaluating the current policy and greedily improving it, often converging in fewer outer rounds.",
                new PolicyIteration<string, string>(),
                mdp)
        ];
    }

    public static string FormatUtility(double value)
    {
        return value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string ActionToArrow(string? action)
    {
        return action switch
        {
            "Up" => "↑",
            "Down" => "↓",
            "Left" => "←",
            "Right" => "→",
            _ => "■"
        };
    }

    private static GridWorldAlgorithmDemo BuildDemo(
        string key,
        string name,
        string summary,
        IMdpSolver<string, string> solver,
        GridWorldMdp mdp)
    {
        var result = solver.Solve(mdp);
        var policy = mdp.States.ToDictionary(
            state => state,
            state => result.Policy.Action(state) ?? string.Empty,
            StringComparer.Ordinal);
        var steps = StateOrder
            .Select((state, index) => BuildStep(index + 1, state, result.Utilities, policy, mdp))
            .ToArray();

        return new GridWorldAlgorithmDemo(
            key,
            name,
            summary,
            solver.Metrics.GetInt("iterations"),
            new Dictionary<string, double>(result.Utilities, StringComparer.Ordinal),
            policy,
            steps);
    }

    private static GridWorldInspectionStep BuildStep(
        int number,
        string state,
        IReadOnlyDictionary<string, double> utilities,
        IReadOnlyDictionary<string, string> policy,
        GridWorldMdp mdp)
    {
        var actions = mdp.Actions(state);
        var evaluations = actions
            .Select(action => new GridWorldActionEvaluation(
                action,
                mdp.States.Sum(nextState => mdp.Transition(state, action, nextState) * utilities[nextState]),
                mdp.States
                    .Select(nextState => new GridWorldTransitionOutcome(nextState, mdp.Transition(state, action, nextState)))
                    .Where(outcome => outcome.Probability > 0)
                    .OrderByDescending(outcome => outcome.Probability)
                    .ToArray()))
            .OrderByDescending(evaluation => evaluation.ExpectedUtility)
            .ToArray();

        var recommendedAction = policy[state];
        var summary = actions.Count == 0
            ? $"State {state} is terminal with fixed reward {FormatUtility(mdp.Reward(state))}."
            : $"State {state} prefers {recommendedAction} because it maximizes expected successor utility under the stochastic transition model.";

        return new GridWorldInspectionStep(
            number,
            state,
            utilities[state],
            string.IsNullOrWhiteSpace(recommendedAction) ? null : recommendedAction,
            summary,
            evaluations);
    }
}