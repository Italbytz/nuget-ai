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

- Search: `Romania search` shows how `Italbytz.AI.Search` drives frontier-based state expansion and path evaluation.
- Agents: `Vacuum World` uses `Italbytz.AI.Agent` base types such as `SimpleAgent` and `AbstractEnvironment` for the visible action loop.
- CSP: `N-Queens` uses `Italbytz.AI.CSP` for constraint-aware board evaluation and a package-backed `MinConflictsSolver` reference solution.

## Initial showcase set

The first public demo wave is expected to focus on:

- Romania search
- Vacuum World
- N-Queens

Each demo should be paired with a short guide that explains the scenario, the relevant `Italbytz.AI.*` packages, and the key observations to make while interacting with the visualization.

The currently available public demos are:

- `Romania search`: `https://italbytz.github.io/nuget-ai/demos/romania-search`
- `Vacuum World`: `https://italbytz.github.io/nuget-ai/demos/vacuum-world`
- `N-Queens`: `https://italbytz.github.io/nuget-ai/demos/n-queens`

## What to look for in each demo

- `Romania search`: compare how the queue discipline changes between breadth-first search, uniform-cost search, and A*.
- `Vacuum World`: compare reactive behavior versus model-based stopping in the same two-room environment.
- `N-Queens`: compare local and evolutionary search traces while checking conflicts against a CSP-backed baseline.

## Site model

The documentation site keeps a clear separation of concerns:

- `docfx` provides the written guides and API reference
- the demo host provides interactive visualizations under a dedicated `/demos/` path

This keeps the package documentation concise while still offering public, linkable algorithm showcases.