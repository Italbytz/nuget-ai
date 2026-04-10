# Architecture and package layout

This guide carries forward the core rationale from the former `nuget-ports-algorithms-ai`, `nuget-ports-algorithms-aima`, and `nuget-adapters-algorithms-aima` repositories.

## Why the split exists

The older AI packages explicitly followed the **Hexagonal Architecture** (also known as **Ports and Adapters**):

- `*.Abstractions` packages contain stable contracts and interfaces.
- concrete packages contain implementations, helpers, and reusable defaults.
- higher-level applications can depend on the contracts without being forced to pull in every implementation detail.

That same idea is preserved in the consolidated `Italbytz.AI.*` family.

## How the historical names map to the current structure

| Historical package or repo | Current home | Purpose |
| --- | --- | --- |
| `Italbytz.Ports.Algorithms` | `Italbytz.AI.Abstractions` | shared solver and metrics contracts |
| `Italbytz.Adapters.Algorithms` | `Italbytz.AI` | shared runtime helpers and base implementations |
| `Italbytz.Ports.Algorithms.AI` | `Italbytz.AI.Abstractions` | AI-specific contracts |
| `Italbytz.Adapters.Algorithms.AI` | `Italbytz.AI` | AI helper implementations |
| `Italbytz.Ports.Algorithms.AIMA` | `Italbytz.AI.Agent/Search/CSP/Planning/Learning.Abstractions` | textbook-oriented interfaces |
| `Italbytz.Adapters.Algorithms.AIMA` | `Italbytz.AI.Agent/Search/CSP/Planning/Learning` | textbook-oriented implementations |
| `Italbytz.Ports.Algorithms.EA` | `Italbytz.AI.Evolutionary.Abstractions` | evolutionary-algorithm contracts |
| `Italbytz.Adapters.Algorithms.EA` | `Italbytz.AI.Evolutionary` | evolutionary implementations |
| `Italbytz.ML` / `Italbytz.Adapters.Algorithms.ML` | `Italbytz.AI.ML.Core` and `Italbytz.AI.ML` | ML.NET helpers, trainers, and inspection tools |

## Choosing the right package

Use the following rule of thumb:

- depend on `*.Abstractions` when you only need interfaces, DTOs, or contracts
- depend on the concrete package when you want a ready-to-use implementation
- keep app- or UI-specific orchestration outside these packages

## Example package pairs

| Contract package | Implementation package | Typical use |
| --- | --- | --- |
| `Italbytz.AI.Search.Abstractions` | `Italbytz.AI.Search` | search problems and concrete search helpers |
| `Italbytz.AI.CSP.Abstractions` | `Italbytz.AI.CSP` | constraint-satisfaction contracts and solvers |
| `Italbytz.AI.Planning.Abstractions` | `Italbytz.AI.Planning` | planning models and planning implementations |
| `Italbytz.AI.Learning.Abstractions` | `Italbytz.AI.Learning` | learning contracts and reusable learning helpers |
| `Italbytz.AI.Evolutionary.Abstractions` | `Italbytz.AI.Evolutionary` | EA contracts and search-space implementations |

## Background

The consolidated structure keeps the original architectural intent, but it makes the package names more domain-oriented and easier to discover for new consumers.