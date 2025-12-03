# Step-by-step: How v-table construction works

### 1. `Animal` declares a virtual method:

```csharp
class Animal {  public virtual void WhoAreYou() { ... } }
````

This creates:
```markdown
Animal v-table
---------------
[0] → Animal.WhoAreYou

```

---

### 2. `Mammal` derives from `Animal` and **overrides** the method:

```csharp
class Mammal : Animal
{
    public override void WhoAreYou() { ... }
}
```

What happens now?

- `WhoAreYou` **already exists** as a virtual method slot (slot 0)
- `Mammal` **does not get a new slot**
- Instead, it **replaces** slot 0 with its own implementation

So Mammal’s v-table becomes:
```markdown
Mammal v-table
---------------
[0] → Mammal.WhoAreYou    (replaces Animal version)

```

✔️ Mammal _inherits_ the slot  
✔️ Mammal _overrides_ the method  
✔️ Therefore, Mammal _replaces_ the implementation

---
#  3. Why Dog adds a second slot

Dog’s version uses **new**, not override:

```csharp
class Dog : Mammal
{
    public new void WhoAreYou() { ... }
}
```

Now the rule is:

> **`new` hides the base method and creates a new, independent slot.**

So Dog’s v-table:

```markdown
Dog v-table
---------------
[0] → Mammal.WhoAreYou    (inherited, unchanged)
[1] → Dog.WhoAreYou       (new slot!)

```

---
# 4. Finally, Beagle overrides Dog’s version

```csharp
class Beagle : Dog
{
    public override void WhoAreYou() { ... }
}
```

This replaces **slot 1**, not slot 0:
```markdown
Beagle v-table
-----------------------
[0] → Mammal.WhoAreYou
[1] → Beagle.WhoAreYou    (overrides Dog version)

```

---
#  Summary (super important!)

|Method keyword|V-table effect|Result|
|---|---|---|
|**override**|Replaces existing slot|No new slot added|
|**new**|Adds a new slot|Base slot stays untouched|

So:
### 🟢 Mammal overrides → replaces Animal’s implementation
### 🔵 Dog uses new → adds a new slot
### 🟣 Beagle overrides Dog’s new slot → replaces slot 1

---