using Italbytz.AI.Agent;

namespace Italbytz.AI.Demos.Web.Demos;

internal enum VacuumDemoAgentKind
{
    ReflexAgent,
    ModelBasedReflexAgent
}

internal enum VacuumLocationState
{
    Clean,
    Dirty
}

internal sealed record VacuumPercept(string AgentLocation, VacuumLocationState CurrentState);

internal sealed record VacuumWorldAction(string Name) : IAction
{
    public static readonly VacuumWorldAction Left = new("Move left");
    public static readonly VacuumWorldAction Right = new("Move right");
    public static readonly VacuumWorldAction Suck = new("Suck");
    public static readonly VacuumWorldAction NoOp = new("No-Op");
}

internal sealed record VacuumWorldStep(
    int Number,
    string AgentLocation,
    string LocationAState,
    string LocationBState,
    string ActionLabel,
    int PerformanceMeasure,
    string Summary,
    string Description);

internal sealed record VacuumWorldAgentDemo(
    string Key,
    string Name,
    string Summary,
    string InitialLocation,
    string InitialLocationAState,
    string InitialLocationBState,
    IReadOnlyList<VacuumWorldStep> Steps);

internal sealed record VacuumWorldScenario(
    string InitialLocation,
    VacuumLocationState InitialLocationAState,
    VacuumLocationState InitialLocationBState);

internal static class VacuumWorldDemoFactory
{
    private const string LocationA = "A";
    private const string LocationB = "B";
    private const int MaxSteps = 10;

    public static IReadOnlyList<VacuumWorldAgentDemo> BuildAll()
    {
        return BuildAll(CreateDefaultScenario());
    }

    public static IReadOnlyList<VacuumWorldAgentDemo> BuildAll(VacuumWorldScenario scenario)
    {
        return
        [
            Simulate(VacuumDemoAgentKind.ReflexAgent, scenario.InitialLocation, scenario.InitialLocationAState, scenario.InitialLocationBState),
            Simulate(VacuumDemoAgentKind.ModelBasedReflexAgent, scenario.InitialLocation, scenario.InitialLocationAState, scenario.InitialLocationBState)
        ];
    }

    public static VacuumWorldScenario CreateRandomScenario(Random random)
    {
        var initialLocation = random.Next(2) == 0 ? LocationA : LocationB;
        var initialA = random.Next(2) == 0 ? VacuumLocationState.Clean : VacuumLocationState.Dirty;
        var initialB = random.Next(2) == 0 ? VacuumLocationState.Clean : VacuumLocationState.Dirty;

        if (initialA == VacuumLocationState.Clean && initialB == VacuumLocationState.Clean)
        {
            if (initialLocation == LocationA)
            {
                initialA = VacuumLocationState.Dirty;
            }
            else
            {
                initialB = VacuumLocationState.Dirty;
            }
        }

        return new VacuumWorldScenario(initialLocation, initialA, initialB);
    }

    private static VacuumWorldScenario CreateDefaultScenario()
    {
        return new VacuumWorldScenario(LocationA, VacuumLocationState.Dirty, VacuumLocationState.Dirty);
    }

    private static VacuumWorldAgentDemo Simulate(
        VacuumDemoAgentKind kind,
        string initialLocation,
        VacuumLocationState initialA,
        VacuumLocationState initialB)
    {
        var agent = CreateAgent(kind);
        var environment = new VacuumWorldEnvironment(initialLocation, initialA, initialB, agent);
        var steps = new List<VacuumWorldStep>();

        for (var step = 1; step <= MaxSteps; step++)
        {
            var percept = environment.PeekPercept();
            environment.Step();

            var action = agent.LastAction ?? VacuumWorldAction.NoOp;
            var description = DescribeAction(kind, percept, action);

            steps.Add(BuildStep(step, environment.AgentLocation, environment.LocationAState, environment.LocationBState, action, environment.PerformanceMeasure, description));

            if (action == VacuumWorldAction.NoOp)
            {
                break;
            }
        }

        return new VacuumWorldAgentDemo(
            kind.ToString(),
            kind == VacuumDemoAgentKind.ReflexAgent ? "Reflex vacuum agent" : "Model-based reflex agent",
            kind == VacuumDemoAgentKind.ReflexAgent
                ? "Runs on Italbytz.AI.Agent.SimpleAgent with simple condition-action rules: if dirty then suck, otherwise alternate between A and B."
                : "Runs on Italbytz.AI.Agent.SimpleAgent and uses an internal state model to stop as soon as both rooms are known to be clean.",
            initialLocation,
            FormatState(initialA),
            FormatState(initialB),
            steps);
    }

    private static VacuumWorldStep BuildStep(
        int stepNumber,
        string agentLocation,
        VacuumLocationState locationA,
        VacuumLocationState locationB,
        VacuumWorldAction action,
        int performance,
        string description)
    {
        return new VacuumWorldStep(
            stepNumber,
            agentLocation,
            FormatState(locationA),
            FormatState(locationB),
            action.Name,
            performance,
            $"{action.Name} @ {agentLocation}",
            description);
    }

    private static string FormatState(VacuumLocationState state) => state == VacuumLocationState.Clean ? "Clean" : "Dirty";

    private static VacuumWorldAgentBase CreateAgent(VacuumDemoAgentKind kind)
    {
        return kind == VacuumDemoAgentKind.ReflexAgent
            ? new ReflexVacuumAgent()
            : new ModelBasedReflexAgent();
    }

    private static string DescribeAction(VacuumDemoAgentKind kind, VacuumPercept percept, VacuumWorldAction action)
    {
        if (action == VacuumWorldAction.NoOp)
        {
            return kind == VacuumDemoAgentKind.ModelBasedReflexAgent
                ? "The internal model marks both rooms as clean, so the agent stops acting."
                : "The agent has no useful move left and therefore does nothing."
                ;
        }

        if (action == VacuumWorldAction.Suck)
        {
            return $"Room {percept.AgentLocation} is dirty, so the agent sucks and gains 10 performance points.";
        }

        if (action == VacuumWorldAction.Right)
        {
            return "The current room is clean, so the agent moves right to inspect room B.";
        }

        return "The current room is clean, so the agent moves left to inspect room A.";
    }

    private abstract class VacuumWorldAgentBase : SimpleAgent<VacuumPercept, VacuumWorldAction>
    {
        public VacuumWorldAction? LastAction { get; private set; }

        protected VacuumWorldAction Remember(VacuumWorldAction action)
        {
            LastAction = action;
            return action;
        }
    }

    private sealed class ReflexVacuumAgent : VacuumWorldAgentBase
    {
        public override VacuumWorldAction? Act(VacuumPercept? percept)
        {
            if (percept is null)
            {
                return Remember(VacuumWorldAction.NoOp);
            }

            if (percept.CurrentState == VacuumLocationState.Dirty)
            {
                return Remember(VacuumWorldAction.Suck);
            }

            return Remember(percept.AgentLocation == LocationA ? VacuumWorldAction.Right : VacuumWorldAction.Left);
        }
    }

    private sealed class ModelBasedReflexAgent : VacuumWorldAgentBase
    {
        private VacuumLocationState? _knownA;
        private VacuumLocationState? _knownB;

        public override VacuumWorldAction? Act(VacuumPercept? percept)
        {
            if (percept is null)
            {
                return Remember(VacuumWorldAction.NoOp);
            }

            if (percept.AgentLocation == LocationA)
            {
                _knownA = percept.CurrentState;
            }
            else
            {
                _knownB = percept.CurrentState;
            }

            if (_knownA == VacuumLocationState.Clean && _knownB == VacuumLocationState.Clean)
            {
                return Remember(VacuumWorldAction.NoOp);
            }

            if (percept.CurrentState == VacuumLocationState.Dirty)
            {
                return Remember(VacuumWorldAction.Suck);
            }

            return Remember(percept.AgentLocation == LocationA ? VacuumWorldAction.Right : VacuumWorldAction.Left);
        }
    }

    private sealed class VacuumWorldEnvironment : AbstractEnvironment<VacuumPercept, VacuumWorldAction>
    {
        public VacuumWorldEnvironment(string initialLocation, VacuumLocationState locationAState, VacuumLocationState locationBState, VacuumWorldAgentBase agent)
        {
            AgentLocation = initialLocation;
            LocationAState = locationAState;
            LocationBState = locationBState;
            Agent = agent;
        }

        public string AgentLocation { get; private set; }

        public VacuumLocationState LocationAState { get; private set; }

        public VacuumLocationState LocationBState { get; private set; }

        public int PerformanceMeasure { get; private set; }

        public VacuumPercept PeekPercept() => GetPerceptSeenBy(Agent!);

        protected override void Execute(IAgent<VacuumPercept, VacuumWorldAction> agent, VacuumWorldAction action)
        {
            if (action == VacuumWorldAction.Suck)
            {
                if (AgentLocation == LocationA)
                {
                    LocationAState = VacuumLocationState.Clean;
                }
                else
                {
                    LocationBState = VacuumLocationState.Clean;
                }

                PerformanceMeasure += 10;
                return;
            }

            if (action == VacuumWorldAction.Right)
            {
                AgentLocation = LocationB;
                PerformanceMeasure -= 1;
                return;
            }

            if (action == VacuumWorldAction.Left)
            {
                AgentLocation = LocationA;
                PerformanceMeasure -= 1;
            }
        }

        protected override VacuumPercept GetPerceptSeenBy(IAgent<VacuumPercept, VacuumWorldAction> agent)
        {
            var currentState = AgentLocation == LocationA ? LocationAState : LocationBState;
            return new VacuumPercept(AgentLocation, currentState);
        }
    }
}