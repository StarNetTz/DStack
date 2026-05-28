# Copilot Context Management - Quick Reference

**For:** Working across DStack Framework, Templates, Documentation, and Book

---

## 🎯 The Strategy: Monorepo + Master Context

**Everything lives in one repository:**
- ✅ Framework code
- ✅ Project templates  
- ✅ Documentation
- ✅ Book manuscript
- ✅ Sample applications

**One context file rules them all:**
- 📍 `.github/copilot-instructions.md`

---

## 🚀 Quick Start

### When Starting Work

```bash
# 1. Pull latest
git pull origin net10

# 2. Open the solution
# Visual Studio: DStack.sln
# VS Code: Open workspace folder

# 3. Copilot automatically loads context from:
#    .github/copilot-instructions.md
```

**That's it!** Copilot now knows:
- Framework patterns
- Template structure
- Documentation style
- Code conventions
- Testing patterns

---

## 📂 Where Things Live

| Component | Location | Context File |
|-----------|----------|--------------|
| **Framework Core** | `src/Core/` | Master context |
| **Persistence** | `src/Infrastructure/` | Master context |
| **Templates** | `src/Templates/` | Master context + Template README |
| **Tutorials** | `docs/tutorials/` | Master context + Tutorial intro |
| **Book** | `docs/book/` | Master context + Book README |
| **Samples** | `samples/` | Master context + Sample README |
| **Tests** | `tests/` | Master context |

---

## 💡 Using Context Effectively

### Asking Copilot

**❌ Vague:**
> "Create an aggregate"

**✅ Specific:**
> "Following the DStack aggregate pattern in .github/copilot-instructions.md, create a PersonAggregate with RegisterPerson command"

**❌ Without context:**
> "Write a projection"

**✅ With context:**
> "Following the projection pattern, create a PersonProjection that subscribes to $ce-Persons and uses PersonHandler"

### Referencing Patterns

When asking Copilot for help, reference existing code:

```
"Like PersonAggregate, create an OrderAggregate..."
"Following the testing pattern in PersonTests..."
"Similar to the EventStore implementation, but for Kurrent..."
```

---

## 🔄 Cross-Component Workflow

### Scenario 1: Adding a New Framework Feature

```bash
# You're working in framework and want to update template + docs

# 1. Work in framework
cd src/Core/DStack.Aggregates/
# Copilot knows: framework patterns from master context

# 2. Update template
cd ../../Templates/CleanArchitecture/YourApp.WriteModel/
# Copilot knows: template structure from master context

# 3. Document it
cd ../../../docs/tutorials/01-todo-app/
# Copilot knows: tutorial style from master context

# 4. Atomic commit
git add .
git commit -m "feat: Add idempotency check with template and tutorial updates"
```

### Scenario 2: Writing a Tutorial

```bash
# You're writing a tutorial that uses framework + template

cd docs/tutorials/02-ecommerce/

# Ask Copilot:
"Create a tutorial following the structure in tutorials/01-todo-app,
using the CleanArchitecture template, showing how to build an Order
aggregate with OrderPlaced and OrderShipped events"

# Copilot generates:
# - Tutorial markdown (following tutorial template)
# - Code samples (following framework patterns)
# - Proper naming (from conventions)
# - Test examples (BDD style)
```

### Scenario 3: Writing Book Chapter

```bash
cd docs/book/manuscript/

# Ask Copilot:
"Write chapter 6 about aggregates, referencing the PersonAggregate
example from samples/01-TodoApp, explaining the pattern from
.github/copilot-instructions.md"

# Copilot generates:
# - Chapter text
# - Code samples from actual working code
# - Cross-references to other chapters
# - Proper terminology
```

---

## 📝 Context Layers

### Layer 1: Master Context (Always Active)
**File:** `.github/copilot-instructions.md`

**Contains:**
- Architecture patterns
- Naming conventions
- Code style rules
- Testing patterns
- Template structure

**Active:** Always (GitHub Copilot loads this automatically)

### Layer 2: Component README (Optional Enhancement)
**Files:** `src/Templates/README.md`, `docs/book/README.md`, etc.

**Contains:**
- Component-specific details
- References back to master context
- Local conventions

**Active:** When working in that directory

### Layer 3: File Comments (Inline Context)
**Example:**
```csharp
// Following the aggregate pattern from .github/copilot-instructions.md
public class OrderAggregate : Aggregate<OrderAggregateState>
{
    // Command handling: check idempotency, validate, apply
    internal void PlaceOrder(PlaceOrderCommand cmd)
    {
        if (ShouldHandleIdempotency && cmd.IsIdempotent(State))
            return;

        Apply(cmd.ToEvent());
    }
}
```

**Active:** When editing that file

---

## 🎯 Best Practices

### DO ✅

1. **Reference master context explicitly**
   ```
   "Following the pattern in .github/copilot-instructions.md..."
   ```

2. **Use specific examples**
   ```
   "Like PersonAggregate but for Order..."
   ```

3. **Mention layer**
   ```
   "In the Write Model, create..."
   "In the tutorial, explain..."
   ```

4. **Cross-reference**
   ```
   "Update both framework code in src/ and tutorial in docs/"
   ```

### DON'T ❌

1. **Be vague**
   ```
   ❌ "Create a thing"
   ✅ "Create an OrderAggregate following DStack patterns"
   ```

2. **Skip context references**
   ```
   ❌ "Write code for orders"
   ✅ "Following PersonAggregate pattern, create OrderAggregate"
   ```

3. **Work in isolation**
   ```
   ❌ Change framework without updating template
   ✅ Update framework + template + docs atomically
   ```

---

## 🔍 Context Verification

### Check What Copilot Knows

Ask Copilot:
```
"What are the DStack naming conventions for commands and events?"
```

Expected answer should reference:
- Commands: `{Verb}{Entity}`
- Events: `{Entity}{PastTense}`

If Copilot doesn't know, check:
1. Is `.github/copilot-instructions.md` committed?
2. Is it in the current workspace?
3. Try reloading VS Code/Visual Studio

---

## 📊 Context File Hierarchy

```
┌─────────────────────────────────────────┐
│  .github/copilot-instructions.md       │  ← MASTER CONTEXT
│  (Always loaded, single source of truth)│
└─────────────────────────────────────────┘
                    ↓
     ┌──────────────┼──────────────┐
     ↓              ↓               ↓
┌─────────┐  ┌─────────────┐  ┌──────────┐
│Framework│  │  Templates  │  │   Docs   │
│ READMEs │  │   READMEs   │  │ READMEs  │
└─────────┘  └─────────────┘  └──────────┘
     ↓              ↓               ↓
┌─────────┐  ┌─────────────┐  ┌──────────┐
│  Code   │  │Template Code│  │Tutorials │
│Comments │  │  Comments   │  │& Book    │
└─────────┘  └─────────────┘  └──────────┘
```

---

## 🚀 Workflow Shortcuts

### Create New Aggregate

```bash
# Ask Copilot in Chat:
"Following DStack patterns, create:
1. RegisterOrder command
2. OrderRegistered event  
3. OrderAggregateState
4. OrderAggregate
5. OrderInteractor
All following naming conventions and patterns from master context"
```

### Create New Tutorial

```bash
# In docs/tutorials/
"Create tutorial 03-inventory-system following the structure of
tutorial 01-todo-app, showing how to build InventoryItem aggregate
with Reserve and Release commands"
```

### Update Template

```bash
# In src/Templates/CleanArchitecture/
"Update YourApp.WriteModel to include the new idempotency helper
from DStack.Aggregates, following the pattern in PersonAggregate"
```

---

## 🎓 Teaching Copilot New Patterns

### When You Create a New Pattern

1. **Add to master context:**
   ```markdown
   ## New Pattern: Saga Orchestration

   [Pattern description and example]
   ```

2. **Commit to repository:**
   ```bash
   git add .github/copilot-instructions.md
   git commit -m "docs: Add saga orchestration pattern"
   ```

3. **Use it immediately:**
   ```
   "Following the saga orchestration pattern I just added,
   create an OrderSaga..."
   ```

---

## 🛠️ Troubleshooting

### Copilot Generates Wrong Pattern

**Problem:** Code doesn't follow DStack conventions

**Solutions:**
1. Check `.github/copilot-instructions.md` is committed
2. Explicitly reference context:
   ```
   "Using the aggregate pattern from .github/copilot-instructions.md..."
   ```
3. Reload window (VS Code: Ctrl+Shift+P → "Reload Window")

### Context Too Large

**Problem:** Master context file getting unwieldy

**Solution:** Split into sections with clear headings:
```markdown
## Architecture Patterns
[Collapse in editor]

## Naming Conventions  
[Collapse in editor]

## Code Generation
[Collapse in editor]
```

### Conflicting Patterns

**Problem:** Old code doesn't match new conventions

**Solution:** 
1. Update old code gradually
2. Add comment: `// TODO: Migrate to new pattern from v9.2`
3. Document migration in CHANGELOG

---

## 📚 Additional Resources

- **Master Context:** `.github/copilot-instructions.md`
- **Framework Review:** `DSTACK-FRAMEWORK-REVIEW.md`
- **Ecosystem Strategy:** `ECOSYSTEM-STRATEGY.md`
- **Architecture Docs:** `docs/architecture/`

---

## 🎯 Quick Commands

### Start Working
```bash
git pull origin net10
code .  # or open DStack.sln
```

### Add Framework Feature
```bash
# 1. Code it: src/Core/
# 2. Test it: tests/
# 3. Template it: src/Templates/
# 4. Document it: docs/
# 5. Commit atomically
```

### Write Tutorial
```bash
# 1. Create folder: docs/tutorials/XX-name/
# 2. Ask Copilot to follow tutorial template
# 3. Add code samples: samples/XX-Name/
# 4. Cross-link with book chapters
```

### Update Book
```bash
# 1. Edit: docs/book/manuscript/
# 2. Add code: docs/book/code-samples/
# 3. Reference framework: src/
# 4. Build preview
```

---

## ✅ Checklist: Am I Using Context Correctly?

Before asking Copilot:

- [ ] Opened correct solution (DStack.sln)
- [ ] Master context committed (`.github/copilot-instructions.md`)
- [ ] Referenced specific pattern ("Following PersonAggregate...")
- [ ] Mentioned target layer ("In WriteModel, create...")
- [ ] Will update related components (code + template + docs)

If all checked, **Copilot will generate consistent, pattern-following code!** ✨

---

## 🎉 Success!

**With this setup:**

✅ Work on framework, templates, docs, and book **in one workspace**  
✅ Copilot knows **all patterns and conventions**  
✅ Code is **consistent across all components**  
✅ Changes are **atomic and synchronized**  
✅ Documentation **always matches code**  

**Happy coding with full context! 🚀**

---

**Next Steps:**
1. Review `.github/copilot-instructions.md`
2. Try asking Copilot to generate something using patterns
3. Verify it follows conventions
4. Update master context if needed
5. Share this guide with collaborators
