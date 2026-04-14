# Legacy sample note

This folder is the in-repo replacement for the removed standalone consumer `csharp-mstest-ai`.

It is kept as a legacy sample snapshot inside `nuget-ai` because parts of its old AIMA-style example worlds remain useful as didactic material. The actively maintained regression coverage has been moved into the modern `nuget-ai/tests/Italbytz.AI.Tests/` suites.

The former source repository for `Italbytz.Adapters.Algorithms.AI` now lives in the workspace's deprecated package area and should be treated as historical reference material, not as an actively developed package source.

# Introduction

The [Hexagonal Architecture](https://web.archive.org/web/20180822100852/http://alistair.cockburn.us/Hexagonal+architecture), also known as the Ports and Adapters pattern, is a design approach that emphasizes separation of concerns by isolating the core application logic from external systems like databases, user interfaces, or APIs. This is achieved through the use of "ports" (interfaces) and "adapters" (implementations), enabling easier testing, maintainability, and flexibility in swapping external dependencies without affecting the core logic.

This repository provides unit tests for Artificial Intelligence projects planning to use the following NuGet packages:

- [Italbytz.Ports.Algorithms.AI](https://www.nuget.org/packages/Italbytz.Ports.Algorithms.AI) (Source: [nuget-ports-algorithms-ai](https://github.com/Italbytz/nuget-ports-algorithms-ai))
- [Italbytz.Adapters.Algorithms.AI](https://www.nuget.org/packages/Italbytz.Adapters.Algorithms.AI) (Legacy source: [nuget-adapters-algorithms-ai](https://github.com/Italbytz/nuget-adapters-algorithms-ai))

 