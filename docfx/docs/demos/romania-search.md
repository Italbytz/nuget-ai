# Romania search demo

The Romania search demo is the first public interactive showcase in the `nuget-ai` demo host.

It illustrates the `Italbytz.AI.Search` package family with the standard AIMA route-finding example from Arad to Bucharest.

## What the demo shows

- breadth-first search
- uniform-cost search
- A* search
- current path progression
- frontier ordering
- explored states and generated successors

## Relevant package entry points

- `Italbytz.AI.Search`
- `RomaniaMap`
- `RomaniaSearchSimulator`

## Public demo

Open the live demo here:

- `https://italbytz.github.io/nuget-ai/demos/romania-search`

## Why this demo is the first migration candidate

Romania search is a strong public showcase because it is:

- directly tied to the search package family
- widely recognizable as an AIMA example
- compact enough to explain in a package-oriented public site
- useful both for teaching and for external package consumers who want to inspect runtime behavior