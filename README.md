# nuget-ai

`nuget-ai` is the target repository for the refactored `Italbytz.AI.*` package family.

## Current migration status

The repository now contains the **base AI layer** for Phase 2:

- `Italbytz.AI.Abstractions`
- `Italbytz.AI`

Upcoming migration slices will add the split packages for:

- `Italbytz.AI.Agent(.Abstractions)`
- `Italbytz.AI.Search(.Abstractions)`
- `Italbytz.AI.CSP(.Abstractions)`
- `Italbytz.AI.Planning(.Abstractions)`
- `Italbytz.AI.Learning(.Abstractions)`
- `Italbytz.AI.Evolutionary(.Abstractions)`
- `Italbytz.AI.ML*`

## Which package should I use?

- Use `Italbytz.AI.Abstractions` for general solver and metrics contracts.
- Use `Italbytz.AI` for the first shared runtime helpers and baseline implementations.

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