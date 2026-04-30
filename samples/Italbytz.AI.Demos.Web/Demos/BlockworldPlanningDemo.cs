namespace Italbytz.AI.Demos.Web.Demos;

internal enum BlockworldAlgorithm
{
    UniformCost,
    AStar
}

internal enum BlockworldHeuristic
{
    MisplacedBlocks,
    TowerPrefix
}

internal sealed record BlockworldScenario(
    string Key,
    string Name,
    string Summary,
    IReadOnlyList<string> Blocks,
    IReadOnlyDictionary<string, string> InitialOn,
    IReadOnlyDictionary<string, string> GoalOn,
    IReadOnlyList<string> GoalTower);

internal sealed record BlockworldState(
    IReadOnlyDictionary<string, string> On,
    string? Holding)
{
    public string Signature => BuildSignature(On, Holding);

    public bool IsHandEmpty => string.IsNullOrWhiteSpace(Holding);

    public HashSet<string> ClearBlocks
    {
        get
        {
            var occupiedSupports = new HashSet<string>(On.Values, StringComparer.Ordinal);
            var clear = new HashSet<string>(On.Keys, StringComparer.Ordinal);
            clear.ExceptWith(occupiedSupports);
            return clear;
        }
    }

    public static string BuildSignature(IReadOnlyDictionary<string, string> on, string? holding)
    {
        var orderedFacts = on
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}:{pair.Value}");
        var hand = string.IsNullOrWhiteSpace(holding) ? "HandEmpty" : $"Holding:{holding}";
        return string.Join("|", orderedFacts.Append(hand));
    }
}

internal sealed record BlockworldActionOption(
    string Action,
    bool IsApplicable,
    string Detail);

internal sealed record BlockworldSuccessorPreview(
    string Action,
    string StateSignature,
    int G,
    int H,
    int F);

internal sealed record BlockworldPlanningStep(
    int StepNumber,
    string CurrentStateSignature,
    IReadOnlyDictionary<string, string> On,
    string? Holding,
    IReadOnlyList<IReadOnlyList<string>> Stacks,
    IReadOnlyList<string> PlanSoFar,
    int G,
    int H,
    int F,
    int FrontierCount,
    int ExploredCount,
    bool IsGoal,
    string Summary,
    IReadOnlyList<BlockworldActionOption> ActionOptions,
    IReadOnlyList<BlockworldSuccessorPreview> Successors);

internal sealed record BlockworldPlanningRun(
    BlockworldScenario Scenario,
    BlockworldAlgorithm Algorithm,
    BlockworldHeuristic Heuristic,
    bool Solved,
    int ExpandedNodes,
    IReadOnlyList<string> Plan,
    IReadOnlyList<BlockworldPlanningStep> Steps);

internal static class BlockworldPlanningDemoFactory
{
    private const string Table = "Table";

    public static IReadOnlyList<BlockworldScenario> BuildScenarios()
    {
        var blocks = new[] { "A", "B", "C", "D" };

        return
        [
            new BlockworldScenario(
                "tower-rebuild",
                "Tower Rebuild",
                "Unstack and rebuild a clean A-B-C-D tower from a scrambled arrangement.",
                blocks,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["A"] = Table,
                    ["B"] = "D",
                    ["C"] = Table,
                    ["D"] = Table
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["A"] = Table,
                    ["B"] = "A",
                    ["C"] = "B",
                    ["D"] = "C"
                },
                new[] { "A", "B", "C", "D" }),
            new BlockworldScenario(
                "swap-support",
                "Swap Support",
                "Move B off C and create two stable mini towers while keeping the hand empty.",
                blocks,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["A"] = Table,
                    ["B"] = "C",
                    ["C"] = Table,
                    ["D"] = "A"
                },
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["A"] = Table,
                    ["B"] = "A",
                    ["C"] = Table,
                    ["D"] = "C"
                },
                new[] { "A", "B" }),
            BuildRandomizedHardScenario(seed: 20260429)
        ];
    }

    public static BlockworldPlanningRun BuildRun(
        BlockworldScenario scenario,
        BlockworldAlgorithm algorithm,
        BlockworldHeuristic heuristic,
        int maxExpandedNodes = 300)
    {
        var start = new BlockworldState(
            new Dictionary<string, string>(scenario.InitialOn, StringComparer.Ordinal),
            Holding: null);

        var frontier = new PriorityQueue<SearchNode, double>();
        var bestGBySignature = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [start.Signature] = 0
        };
        var explored = new HashSet<string>(StringComparer.Ordinal);
        var steps = new List<BlockworldPlanningStep>();

        var startNode = new SearchNode(start, Parent: null, ActionFromParent: null, G: 0);
        var startH = EvaluateHeuristic(start, scenario, heuristic);
        frontier.Enqueue(startNode, ComputePriority(startNode.G, startH, algorithm));

        SearchNode? solvedNode = null;
        var expanded = 0;

        while (frontier.Count > 0 && expanded < maxExpandedNodes)
        {
            var current = frontier.Dequeue();
            if (explored.Contains(current.State.Signature))
            {
                continue;
            }

            explored.Add(current.State.Signature);
            expanded++;

            var currentH = EvaluateHeuristic(current.State, scenario, heuristic);
            var applicableActions = EnumerateApplicableActions(current.State, scenario.Blocks).ToArray();
            var optionRows = EnumerateActionOptions(current.State, scenario.Blocks).ToArray();
            var successorPreviews = new List<BlockworldSuccessorPreview>();

            foreach (var (actionLabel, successorState) in applicableActions)
            {
                var successorG = current.G + 1;
                var successorSignature = successorState.Signature;
                if (bestGBySignature.TryGetValue(successorSignature, out var knownG) && successorG >= knownG)
                {
                    continue;
                }

                bestGBySignature[successorSignature] = successorG;
                var successorNode = new SearchNode(successorState, current, actionLabel, successorG);
                var successorH = EvaluateHeuristic(successorState, scenario, heuristic);
                var successorF = successorG + successorH;
                frontier.Enqueue(successorNode, ComputePriority(successorG, successorH, algorithm));
                successorPreviews.Add(new BlockworldSuccessorPreview(actionLabel, successorSignature, successorG, successorH, successorF));
            }

            var isGoal = IsGoalState(current.State, scenario);
            var planSoFar = BuildPlan(current).ToArray();
            var stacks = BuildStacks(current.State, scenario.Blocks).ToArray();
            var summary = isGoal
                ? "Goal reached. All goal relations are satisfied and the hand is empty."
                : $"Expanded {current.State.Signature}. Generated {successorPreviews.Count} successor states from {applicableActions.Length} applicable actions.";

            steps.Add(new BlockworldPlanningStep(
                StepNumber: steps.Count + 1,
                CurrentStateSignature: current.State.Signature,
                On: new Dictionary<string, string>(current.State.On, StringComparer.Ordinal),
                Holding: current.State.Holding,
                Stacks: stacks,
                PlanSoFar: planSoFar,
                G: current.G,
                H: currentH,
                F: current.G + currentH,
                FrontierCount: frontier.Count,
                ExploredCount: explored.Count,
                IsGoal: isGoal,
                Summary: summary,
                ActionOptions: optionRows,
                Successors: successorPreviews.OrderBy(s => s.F).ThenBy(s => s.Action, StringComparer.Ordinal).ToArray()));

            if (isGoal)
            {
                solvedNode = current;
                break;
            }
        }

        var finalPlan = solvedNode is null ? Array.Empty<string>() : BuildPlan(solvedNode).ToArray();

        return new BlockworldPlanningRun(
            scenario,
            algorithm,
            heuristic,
            solvedNode is not null,
            expanded,
            finalPlan,
            steps);
    }

    public static string AlgorithmLabel(BlockworldAlgorithm algorithm)
    {
        return algorithm switch
        {
            BlockworldAlgorithm.UniformCost => "Uniform-cost (uninformed)",
            BlockworldAlgorithm.AStar => "A* search",
            _ => algorithm.ToString()
        };
    }

    public static string HeuristicLabel(BlockworldHeuristic heuristic)
    {
        return heuristic switch
        {
            BlockworldHeuristic.MisplacedBlocks => "Misplaced blocks",
            BlockworldHeuristic.TowerPrefix => "Tower prefix",
            _ => heuristic.ToString()
        };
    }

    private static BlockworldScenario BuildRandomizedHardScenario(int seed)
    {
        var blocks = new[] { "A", "B", "C", "D", "E" };
        var goalOn = BuildGoalTowerMap(blocks);
        var goalState = new BlockworldState(goalOn, Holding: null);

        var random = new Random(seed);
        var current = goalState;

        for (var i = 0; i < 22; i++)
        {
            var actions = EnumerateApplicableActions(current, blocks).ToArray();
            if (actions.Length == 0)
            {
                break;
            }

            current = actions[random.Next(actions.Length)].Successor;
        }

        if (!current.IsHandEmpty && current.Holding is not null)
        {
            current = ApplyPutDown(current, current.Holding);
        }

        return new BlockworldScenario(
            "randomized-hard",
            "Randomized Hard",
            "Five-block scenario created via randomized reversible moves from the goal. It is guaranteed solvable and usually deeper than the fixed examples.",
            blocks,
            new Dictionary<string, string>(current.On, StringComparer.Ordinal),
            goalOn,
            blocks);
    }

    private static Dictionary<string, string> BuildGoalTowerMap(IReadOnlyList<string> blocks)
    {
        var goalOn = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [blocks[0]] = Table
        };

        for (var i = 1; i < blocks.Count; i++)
        {
            goalOn[blocks[i]] = blocks[i - 1];
        }

        return goalOn;
    }

    private static IEnumerable<BlockworldActionOption> EnumerateActionOptions(BlockworldState state, IReadOnlyList<string> blocks)
    {
        var clear = state.ClearBlocks;

        foreach (var block in blocks.OrderBy(b => b, StringComparer.Ordinal))
        {
            if (!state.On.ContainsKey(block))
            {
                continue;
            }
            var onSupport = state.On[block];
            yield return BuildActionOption(
                $"PickUp({block})",
                state.IsHandEmpty && onSupport == Table && clear.Contains(block),
                state.IsHandEmpty
                    ? onSupport == Table
                        ? clear.Contains(block) ? "applicable" : $"{block} is not clear"
                        : $"{block} is on {onSupport}, not on table"
                    : $"hand already holds {state.Holding}");

            yield return BuildActionOption(
                $"PutDown({block})",
                string.Equals(state.Holding, block, StringComparison.Ordinal),
                string.Equals(state.Holding, block, StringComparison.Ordinal)
                    ? "applicable"
                    : string.IsNullOrWhiteSpace(state.Holding)
                        ? "hand is empty"
                        : $"hand holds {state.Holding}, not {block}");

            foreach (var support in blocks.Where(candidate => !string.Equals(candidate, block, StringComparison.Ordinal)).OrderBy(b => b, StringComparer.Ordinal))
            {
                yield return BuildActionOption(
                    $"Unstack({block},{support})",
                    state.IsHandEmpty && string.Equals(onSupport, support, StringComparison.Ordinal) && clear.Contains(block),
                    state.IsHandEmpty
                        ? string.Equals(onSupport, support, StringComparison.Ordinal)
                            ? clear.Contains(block) ? "applicable" : $"{block} is not clear"
                            : $"{block} is not on {support}"
                        : $"hand already holds {state.Holding}");

                yield return BuildActionOption(
                    $"Stack({block},{support})",
                    string.Equals(state.Holding, block, StringComparison.Ordinal) && clear.Contains(support),
                    string.Equals(state.Holding, block, StringComparison.Ordinal)
                        ? clear.Contains(support) ? "applicable" : $"{support} is not clear"
                        : string.IsNullOrWhiteSpace(state.Holding)
                            ? "hand is empty"
                            : $"hand holds {state.Holding}, not {block}");
            }
        }
    }

    private static BlockworldActionOption BuildActionOption(string action, bool applicable, string detail)
    {
        return new BlockworldActionOption(action, applicable, detail);
    }

    private static IEnumerable<(string Action, BlockworldState Successor)> EnumerateApplicableActions(BlockworldState state, IReadOnlyList<string> blocks)
    {
        var clear = state.ClearBlocks;

        if (state.IsHandEmpty)
        {
            foreach (var block in blocks)
            {
                var support = state.On[block];
                if (!clear.Contains(block))
                {
                    continue;
                }

                if (support == Table)
                {
                    yield return ($"PickUp({block})", ApplyPickUp(state, block));
                }
                else
                {
                    yield return ($"Unstack({block},{support})", ApplyUnstack(state, block, support));
                }
            }

            yield break;
        }

        var held = state.Holding!;
        yield return ($"PutDown({held})", ApplyPutDown(state, held));

        foreach (var support in blocks.Where(candidate => !string.Equals(candidate, held, StringComparison.Ordinal)))
        {
            if (!clear.Contains(support))
            {
                continue;
            }

            yield return ($"Stack({held},{support})", ApplyStack(state, held, support));
        }
    }

    private static BlockworldState ApplyPickUp(BlockworldState state, string block)
    {
        var next = new Dictionary<string, string>(state.On, StringComparer.Ordinal);
        next.Remove(block);
        return new BlockworldState(next, block);
    }

    private static BlockworldState ApplyUnstack(BlockworldState state, string block, string support)
    {
        var next = new Dictionary<string, string>(state.On, StringComparer.Ordinal);
        next.Remove(block);
        return new BlockworldState(next, block);
    }

    private static BlockworldState ApplyPutDown(BlockworldState state, string block)
    {
        var next = new Dictionary<string, string>(state.On, StringComparer.Ordinal)
        {
            [block] = Table
        };
        return new BlockworldState(next, null);
    }

    private static BlockworldState ApplyStack(BlockworldState state, string block, string support)
    {
        var next = new Dictionary<string, string>(state.On, StringComparer.Ordinal)
        {
            [block] = support
        };
        return new BlockworldState(next, null);
    }

    private static bool IsGoalState(BlockworldState state, BlockworldScenario scenario)
    {
        if (!state.IsHandEmpty)
        {
            return false;
        }

        foreach (var (block, goalSupport) in scenario.GoalOn)
        {
            if (!state.On.TryGetValue(block, out var actualSupport) || !string.Equals(actualSupport, goalSupport, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static int EvaluateHeuristic(BlockworldState state, BlockworldScenario scenario, BlockworldHeuristic heuristic)
    {
        return heuristic switch
        {
            BlockworldHeuristic.MisplacedBlocks => EvaluateMisplacedBlocks(state, scenario),
            BlockworldHeuristic.TowerPrefix => EvaluateTowerPrefix(state, scenario),
            _ => 0
        };
    }

    private static int EvaluateMisplacedBlocks(BlockworldState state, BlockworldScenario scenario)
    {
        var misplaced = 0;
        foreach (var (block, support) in scenario.GoalOn)
        {
            if (!state.On.TryGetValue(block, out var actual) || !string.Equals(actual, support, StringComparison.Ordinal))
            {
                misplaced++;
            }
        }

        if (!state.IsHandEmpty)
        {
            misplaced++;
        }

        return misplaced;
    }

    private static int EvaluateTowerPrefix(BlockworldState state, BlockworldScenario scenario)
    {
        if (scenario.GoalTower.Count == 0)
        {
            return EvaluateMisplacedBlocks(state, scenario);
        }

        var prefix = 0;
        for (var i = 0; i < scenario.GoalTower.Count; i++)
        {
            var block = scenario.GoalTower[i];
            var expectedSupport = i == 0 ? Table : scenario.GoalTower[i - 1];
            if (!state.On.TryGetValue(block, out var support) || !string.Equals(support, expectedSupport, StringComparison.Ordinal))
            {
                break;
            }

            prefix++;
        }

        var missing = scenario.GoalTower.Count - prefix;
        return state.IsHandEmpty ? missing : missing + 1;
    }

    private static double ComputePriority(int g, int h, BlockworldAlgorithm algorithm)
    {
        return algorithm switch
        {
            BlockworldAlgorithm.UniformCost => g,
            BlockworldAlgorithm.AStar => g + h,
            _ => g + h
        };
    }

    private static IEnumerable<string> BuildPlan(SearchNode node)
    {
        var stack = new Stack<string>();
        var cursor = node;

        while (cursor.Parent is not null)
        {
            if (!string.IsNullOrWhiteSpace(cursor.ActionFromParent))
            {
                stack.Push(cursor.ActionFromParent);
            }

            cursor = cursor.Parent;
        }

        return stack;
    }

    private static IEnumerable<IReadOnlyList<string>> BuildStacks(BlockworldState state, IReadOnlyList<string> blocks)
    {
        var bySupport = state.On
            .GroupBy(pair => pair.Value, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(pair => pair.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        var roots = bySupport.TryGetValue(Table, out var tableBlocks)
            ? tableBlocks
            : blocks.Where(block => !state.On.Values.Contains(block, StringComparer.Ordinal)).OrderBy(block => block, StringComparer.Ordinal).ToArray();

        foreach (var root in roots)
        {
            var tower = new List<string> { root };
            var cursor = root;

            while (bySupport.TryGetValue(cursor, out var supportedBlocks) && supportedBlocks.Length > 0)
            {
                var next = supportedBlocks[0];
                tower.Add(next);
                cursor = next;
            }

            yield return tower;
        }
    }

    private sealed record SearchNode(
        BlockworldState State,
        SearchNode? Parent,
        string? ActionFromParent,
        int G);
}
