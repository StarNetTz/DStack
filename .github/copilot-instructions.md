- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.

---

# DStack Ecosystem - Master Context

> **Framework Version:** 9.1.2  
> **Target:** .NET 10  
> **Last Updated:** January 2025

## 🏛️ Repository Structure

```
DStack/
├── src/Core/          - Framework abstractions (zero dependencies)
├── src/Infrastructure/ - EventStore, Kurrent, RavenDB implementations
├── src/Testing/       - Testing frameworks
├── src/Templates/     - Project templates (planned)
├── docs/             - Documentation & book (planned)
├── samples/          - Sample applications (planned)
└── tests/            - Unit & integration tests
```

## 🎯 Architecture Patterns

### Aggregate Pattern
```csharp
public class {Entity}Aggregate : Aggregate<{Entity}AggregateState>
{
    internal void {Command}({Command}Command cmd)
    {
        if (ShouldHandleIdempotency && cmd.IsIdempotent(State))
            return;
        Apply(cmd.ToEvent());
    }
}
```

### Projection Pattern
```csharp
[SubscribesToStream("$ce-{Entity}")]
public class {Entity}Projection : Projection, IHandledBy<{Entity}Handler>
```

## 📛 Naming Conventions

- **Commands:** `{Verb}{Entity}` (RegisterPerson, CreateOrder)
- **Events:** `{Entity}{PastTense}` (PersonRegistered, OrderCreated)
- **Aggregates:** `{Entity}Aggregate`
- **States:** `{Entity}AggregateState`
- **Projections:** `{Entity}Projection`
- **Handlers:** `{Entity}Handler`

## 💾 Database Support

- **EventStore** (Legacy) - Port 2113
- **Kurrent** (LTS) - Port 2114
- **RavenDB** (Read Models)

## 🧪 Testing Style

```csharp
// BDD-style aggregate testing
Given(/* prior events */);
When(new SomeCommand());
await Expect(new SomeEvent());
```

## 🎨 Code Style

- ✅ Use `ConfigureAwait(false)` in library code
- ✅ Throw `DomainError.Named()` for business rules
- ✅ Apply events via `Apply()` method
- ✅ Check `ShouldHandleIdempotency` for updates

## 📚 Template Structure (Planned)

```
Template.Api/         - Controllers, DI setup
Template.WriteModel/  - Commands, Aggregates, Events
Template.ReadModel/   - Projections, Handlers, DTOs
```

---

**For detailed patterns and examples, see full framework review in repo documentation.**

