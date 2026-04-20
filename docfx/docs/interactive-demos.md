# Interactive demos

`nuget-ai` combines written package documentation with interactive browser-based demonstrations for selected AIMA-style algorithms.

Use the guides and API reference to understand package selection and implementation details.
Use the interactive demos to inspect algorithm state transitions step by step.

## What the demos are for

The demo area is intended to help package consumers:

- see how a specific algorithm evolves over time
- connect abstract package APIs to visible runtime behavior
- compare algorithm states such as frontier, explored nodes, actions, or assignments
- explore canonical AI examples without cloning the repository first

## Planned demo categories

- Search
- Agents
- CSP
- Planning
- Probability
- Learning

## Current category map

- Search: `Romania search` shows how `Italbytz.AI.Search` drives frontier-based state expansion and path evaluation, including randomized start cities.
- Search: `Weighted Graph` uses `WeightedGraphSearchSimulator` to reveal Dijkstra relaxations, distance updates, and predecessor-tree growth step by step.
- Agents: `Vacuum World` uses `Italbytz.AI.Agent` base types such as `SimpleAgent` and `AbstractEnvironment` for a randomized visible action loop.
- CSP: `Map Coloring` uses `Italbytz.AI.CSP` to compare constructive backtracking and min-conflicts repair on the Australia map.
- CSP: `N-Queens` uses `Italbytz.AI.CSP` for constraint-aware board evaluation and a package-backed `MinConflictsSolver` reference solution, with randomized starts and progressively revealed traces.
- Probability: `Burglary Network` uses `Italbytz.AI.Probability` to compare exact and approximate Bayesian inference on the same evidence assignment.

## Initial showcase set

The first public demo wave is expected to focus on:

- Romania search
- Vacuum World
- N-Queens

Each demo should be paired with a short guide that explains the scenario, the relevant `Italbytz.AI.*` packages, and the key observations to make while interacting with the visualization.

The currently available public demos are:

- `Burglary Network`: `https://italbytz.github.io/nuget-ai/demos/burglary-network`
- `Map Coloring`: `https://italbytz.github.io/nuget-ai/demos/map-coloring`
- `Romania search`: `https://italbytz.github.io/nuget-ai/demos/romania-search`
- `Vacuum World`: `https://italbytz.github.io/nuget-ai/demos/vacuum-world`
- `N-Queens`: `https://italbytz.github.io/nuget-ai/demos/n-queens`
- `Weighted Graph`: `https://italbytz.github.io/nuget-ai/demos/weighted-graph`

## What to look for in each demo

- `Burglary Network`: compare exact and approximate posteriors for `P(Burglary | evidence)` while switching evidence on intermediate nodes and observed calls.
- `Map Coloring`: compare backtracking, heuristic variable ordering, and min-conflicts repair on the same CSP while randomizing tie orders between runs.
- `Romania search`: compare how the queue discipline changes between breadth-first search, uniform-cost search, and A* while trying different randomized start cities.
- `Vacuum World`: compare reactive behavior versus model-based stopping in the same two-room environment while revealing the action trace one step at a time.
- `N-Queens`: compare local and evolutionary search traces while checking conflicts against a CSP-backed baseline and revealing each run progressively.
- `Weighted Graph`: inspect accepted versus rejected relaxations, changing frontier priorities, and the shortest-path tree that emerges from the weighted graph.

## Site model

The documentation site keeps a clear separation of concerns:

- `docfx` provides the written guides and API reference
- the demo host provides interactive visualizations under a dedicated `/demos/` path

This keeps the package documentation concise while still offering public, linkable algorithm showcases.