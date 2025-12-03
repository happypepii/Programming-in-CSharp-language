A copy-ready Markdown note that explains, step-by-step, **how the runtime chooses which method implementation to call** in C#. Includes simple rules, an example walkthrough, and a quick checklist you can use when reasoning about calls.

---

## Quick summary (the golden rules)
1. **The compile-time type of the variable decides _which slot_ is referenced.**
2. **If that slot is virtual, the runtime object (the object’s actual type) decides _which implementation_ sits in the slot and will be executed.**

Put another way:
- _Compile-time type → which slot_
- _Runtime (object) type → which implementation in that slot_
---
## Terminology (short)

- **Slot**: an index in the v-table (virtual-method table) for a type; each virtual method has a slot.
- **v-table**: one per concrete type at runtime; array of pointers to method implementations.
- **`call`**: IL instruction for non-virtual direct calls.
- **`callvirt`**: IL instruction for virtual calls (looks up the slot in the object’s v-table).

---

## Step-by-step decision procedure (how to reason about any call)

1. **Look at the variable declaration** — what is the _compile-time type_ of the variable used to call the method?
    - Example: `Animal a = ...;` → compile-time type is `Animal`.
2. **In that compile-time type, find the method name → which slot?**
    - If the method is virtual (or inherited virtual), the compiler emits a virtual call that references the slot number for _that_ compile-time type.
    - If the method is non-virtual, the compiler emits a direct `call` (no slot lookup).
3. **If the call references a virtual slot, at runtime:**
    - Inspect the _actual object type_ (the runtime type, e.g., `Beagle`).
    - Look up the object type’s v-table at the slot index chosen by the compiler.
    - The function pointer stored there is the implementation that runs.
4. **If the call is non-virtual:**
    - There is no v-table lookup — the compiler has already fixed the target method.
    - The direct (fastest) call to the method’s address runs, regardless of the object’s runtime type.

---

## Example: `Animal` / `Mammal` / `Dog` / `Beagle`

```csharp
class Animal { public virtual void WhoAreYou() => Console.WriteLine("Animal"); }
class Mammal : Animal { public override void WhoAreYou() => Console.WriteLine("Mammal"); }
class Dog : Mammal { public new void WhoAreYou() => Console.WriteLine("Dog"); } // new -> new slot
class Beagle : Dog { public override void WhoAreYou() => Console.WriteLine("Beagle"); } // overrides Dog's slot

```

### v-table evolution (visual)

- `Animal` v-table
    ```csharp
    [0] → Animal.WhoAreYou`
    ```
- `Mammal` (overrides slot 0)
    ```csharp
    [0] → Mammal.WhoAreYou   // replaced Animal's implementation at slot 0
    ```
- `Dog` (uses `new`, so adds a new slot 1)
 ```csharp
 [0] → Mammal.WhoAreYou   // inherited slot 0 unchanged
 [1] → Dog.WhoAreYou      // new slot added
 ```
- `Beagle` (overrides Dog’s slot 1)
```csharp
[0] → Mammal.WhoAreYou
[1] → Beagle.WhoAreYou
```
### Two calls to compare
```csharp
Animal a = new Beagle();
Dog d = new Beagle();

a.WhoAreYou(); // ?
d.WhoAreYou(); // ?
```

**Reasoning for `a.WhoAreYou()`**
1. Compile-time type = `Animal` → compiler picks **slot 0** (because `Animal.WhoAreYou` is the method in `Animal`).
2. At runtime the object is `Beagle`. Look at `Beagle` v-table slot 0 → it points to `Mammal.WhoAreYou`.
3. **Executes `Mammal.WhoAreYou`**.

**Reasoning for `d.WhoAreYou()`**
4. Compile-time type = `Dog` → compiler picks **slot 1** (because `Dog` introduced a `new` `WhoAreYou`, placed in slot 1).
5. At runtime the object is `Beagle`. Look at `Beagle` v-table slot 1 → it points to `Beagle.WhoAreYou` (Beagle overrode Dog slot).
6. **Executes `Beagle.WhoAreYou`.**

---

## Short examples of keywords and effects

| Keyword/Action     | Effect on dispatch / v-table                                              |
| ------------------ | ------------------------------------------------------------------------- |
| `virtual` (base)   | Creates a slot for the method (slot N)                                    |
| `override`         | Replaces the existing slot N implementation for the derived type          |
| `new`              | Hides base method and **adds a new slot** for the derived type (slot N+1) |
| non-virtual method | No slot; compile-time direct `call` (fastest)                             |