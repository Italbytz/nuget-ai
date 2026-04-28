# Tic-Tac-Toe demo

The Tic-Tac-Toe demo is a public adversarial-search showcase in the `nuget-ai` demo host.

It compares pure minimax and alpha-beta pruning on the same board positions.

## What the demo shows

- side-by-side move recommendations from minimax and alpha-beta
- expanded-node metrics for both algorithms on the same state
- scenario presets that expose different branching structures
- interactive continuation by clicking empty board cells

## Relevant package entry points

- `Italbytz.AI.Search`
- `Italbytz.AI.Search.Adversarial.MinimaxSearch`
- `Italbytz.AI.Search.Adversarial.AlphaBetaSearch`
- `Italbytz.AI.Search.Adversarial.IGame`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/tic-tac-toe`

## Why this demo is useful

Tic-Tac-Toe is a compact game model where users can directly verify two central properties:

- minimax and alpha-beta should choose equivalent optimal moves
- alpha-beta can reduce search effort substantially with favorable move ordering
