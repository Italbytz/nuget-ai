# Weighted graph demo

The weighted graph demo is a public search showcase in the `nuget-ai` demo host.

It uses the weighted-graph simulator from `Italbytz.AI.Search.Demos` to reveal a Dijkstra-style run step by step.

## What the demo shows

- node expansion order under uniform-cost search
- accepted and rejected edge relaxations
- frontier contents after each step
- evolving distance table and predecessor tree
- multiple prepared scenarios plus randomized selection

## Relevant package entry points

- `Italbytz.AI.Search`
- `Italbytz.AI.Search.Demos.WeightedGraph`
- `WeightedGraphSearchSimulator`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/weighted-graph`

## Why this demo is useful

The weighted graph page complements the Romania route demo by focusing on one algorithmic idea:

- shortest-path relaxation
- queue reordering under path costs
- the relationship between frontier state and the final predecessor tree
- a graph that is small enough to explain on slides or in documentation