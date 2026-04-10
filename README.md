# nuget-ai

`nuget-ai` is the target repository for the refactored `Italbytz.AI.*` package family.

## Current migration status

The repository now contains the first three concrete Phase 2 waves:

- `Italbytz.AI.Abstractions`
- `Italbytz.AI`
- `Italbytz.AI.Agent.Abstractions`
- `Italbytz.AI.Agent`
- `Italbytz.AI.Search.Abstractions`
- `Italbytz.AI.Search`
- `Italbytz.AI.CSP.Abstractions`
- `Italbytz.AI.CSP`

Upcoming migration slices will add:

- `Italbytz.AI.Planning(.Abstractions)`
- `Italbytz.AI.Learning(.Abstractions)`
- `Italbytz.AI.Evolutionary(.Abstractions)`
- `Italbytz.AI.ML*`

## Which package should I use?

- Use `Italbytz.AI.Abstractions` for general solver and metrics contracts.
- Use `Italbytz.AI` for shared runtime helpers such as metrics and thread-safe randomness.
- Use `Italbytz.AI.Agent.Abstractions` when you only need agent-facing contracts.
- Use `Italbytz.AI.Agent` for basic agent and environment helper implementations.
- Use `Italbytz.AI.Search.Abstractions` for search/problem contracts.
- Use `Italbytz.AI.Search` for concrete search helpers such as `SearchAgent`, `GeneralProblem`, breadth-first search, and uniform-cost search.
- Use `Italbytz.AI.CSP.Abstractions` for constraint-satisfaction contracts such as `ICSP`, `IAssignment`, `IDomain`, and `IConstraint`.
- Use `Italbytz.AI.CSP` for concrete CSP building blocks and solvers such as `CSP`, `MapCSP`, `FlexibleBacktrackingSolver`, `TreeCspSolver`, and `MinConflictsSolver`.

## Migration notice

Older repositories and articles may still refer to names such as:

- `Italbytz.Ports.Algorithms`
- `Italbytz.Adapters.Algorithms`
- `Italbytz.Ports.Algorithms.AI`
- `Italbytz.Adapters.Algorithms.AI`
- `Italbytz.Ports.Algorithms.AIMA`
- `Italbytz.Adapters.Algorithms.AIMA`
- `Italbytz.Ports.Algorithms.EA`
- `Italbytz.Adapters.Algorithms.EA`
- `Italbytz.ML`

For all new development, please use the new `Italbytz.AI.*` package family.

## Documentation

API documentation is generated with `docfx` and can be published via GitHub Pages:

- `https://italbytz.github.io/nuget-ai/`

If the URL still returns `404`, wait until the `CI` workflow on `main` has completed the first Pages publish run.

## Quality checks

This repository includes:

- a `GitHub Actions` workflow in `.github/workflows/ci.yml`
- automated `restore`, `build`, `test`, `pack`, and docs generation
- a `docfx` setup under `docfx/`

## Local validation

```bash
dotnet restore nuget-ai.sln
dotnet test nuget-ai.sln -v minimal
dotnet pack nuget-ai.sln -c Release -v minimal
dotnet tool restore
dotnet tool run docfx docfx/docfx.json
```