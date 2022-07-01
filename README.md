# DStack

## Event Sourcing and CQRS building blocks for .NET Core

### Introduction

DStack tries to provide a clean and testable environment for designing and implementing published language, aggregates and projections in **Domain-driven design** solutions.

It abstracts aggregates (write model) and projections (read model), and provides a TDD-friendly development environment.

It also provides storage implementation for:

- **EventStoreDB** for aggregates persisted as a stream of events after succesfully executed commands
- **RavenDB** as a NoSQL read-model storage

## Flow

- **Command** is issued to an aggregate.
- Aggregate executes command and produces an **event**.
- **Projection** is using a subscription to pick up the event from event stream, and use it to create read model.
