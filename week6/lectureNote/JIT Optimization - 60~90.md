## 1. JIT Intrinsics: Extremely Fast Replacements

A **JIT intrinsic** is a special optimization where the .NET JIT recognizes certain method calls and replaces them with highly optimized machine code instead of executing the method normally.

### Example: `Enum.HasFlag()`

```csharp
[Flags] 
enum Permissions { Read = 1, Write = 2 }  
Permissions p = Permissions.Read | Permissions.Write;  
bool has = p.HasFlag(Permissions.Write);
```
### Old .NET Framework
`HasFlag()` was slow because it:
- used reflection
- boxed the enum
- inspected metadata

### Modern .NET (6+)

`HasFlag()` is a **JIT intrinsic**:
- The JIT recognizes the method call
```csharp
	p.HasFlag(x)
```
- Replaces it with `(p & flag) == flag`
```csharp
	(p & x) == x
```
- Emits just a couple of CPU instructions
```assembly
	AND    r1, r2 CMP    r1, r2`
```
This makes it **as fast as manually checking the bits**.
```csharp
bool has = (p & Permissions.Write) != 0; // same speed as HasFlag()
```
### Caveat
The intrinsic applies only when the JIT can analyze the call (simple IL); overly complex patterns may disable the optimization.


----


## 2. Method Inlining

### 1. What is Method Inlining?

**Method inlining** is a JIT optimization where the JIT **copies the body of a small method directly into the caller**, instead of emitting a real method call.

**Goal:**  
Remove _call overhead_ and unlock deeper optimizations.

---
### 2. Why Inline Methods?
A normal method call has overhead:
- Preparing arguments
- Creating a stack frame
- Jumping to the function
- Returning back
- Restoring registers/state
Inlining **eliminates all of this**.

After inlining, the method behaves as if you manually wrote its body inside the caller

---
### 3. Example
```csharp
int Add(int a, int b) => a + b;

int Test() => Add(1, 2);
```

This method is extremely small — just one arithmetic instruction — making it a perfect candidate for JIT inlining.

---

### **Without inlining (conceptually):**

```assembly
call Add
Add: load a, load b
     add
     ret
```

- The CPU must perform a real method call
- Creates a stack frame
- Jumps to the method
- Returns back
- Restores state
---
### **With inlining (JIT-optimized):**

```assembly
load 1
load 2
add
```
✔️ **No method call**  
✔️ **No stack frame**  
✔️ **No return**  
✔️ **Only the actual computation remains**

---
### **Result**
BenchmarkDotNet shows that calling:
```csharp
var x = Add(1, 2);`
```
and writing:
```csharp
var x = 1 + 2;`
```
run at **the exact same speed**, because:
➡️ `Add(a, b)` is fully inlined  
➡️ The JIT replaces the call with the literal `a + b` machine instructions

---
### 4. When Inlining Happens

The JIT will inline **small, simple** methods:
- Auto-property getters/setters
- Methods with a single return and few instructions
- Methods without loops, try/catch, or complex branching
- Methods that aren’t recursive
- Methods that aren’t too large (default size limit ≈ 32 IL bytes)

These are considered _aggressively inlineable_.

----
### 5. When Inlining Does _Not_ Happen

Inlining is skipped if the method:

- Is too large
- Contains complex logic
- Contains `try/catch/finally`
- Is recursive
- Uses `async`/`await`
- Has complicated generics
- Is virtual/interface and cannot be devirtualized
- Requires too much JIT analysis time

When the JIT does not inline → **actual method call overhead** occurs, which is visibly slower in microbenchmarks.




## 3. How virtual methods REALLY work in .NET (the most important part!)
Virtual methods are the mechanism that allows .NET to choose **at runtime** which method implementation to call. This is how polymorphism works under the hood.

To understand this, you have to understand **call instructions**, **v-tables**, and the rules for **override** vs **new**.

### 1.  `call` vs `callvirt` — the core difference

|Concept|Non-virtual method|Virtual method|
|---|---|---|
|**Call decision time**|Compile time (static dispatch)|Compile time chooses slot → runtime chooses implementation|
|**IL instruction**|`call`|`callvirt`|
|**Lookup**|Direct jump to method address|Lookup in v-table (1 extra pointer)|
|**Speed**|Fastest|Slightly slower (but still very fast)|
|**Who decides what runs?**|_Variable’s compile-time type_|_Object’s runtime type_|
### Key idea:
- `call` means **“jump to exactly this method”**
- `callvirt` means  
    **“find the correct method implementation in the v-table of the runtime object”**

---
### 2. The Virtual Method Table (v-table)
Every class with virtual methods has a hidden array created by the CLR:
- One v-table per class
- Each entry = “slot” pointing to a virtual method implementation
- Derived classes **inherit** the parent’s table and **override entries** when necessary

Think of it as:
```css
Class → v-table → array of function pointers
```

Here’s an example using `WhoAreYou()`:
```csharp
class Animal
{
    public virtual void WhoAreYou() { ... }
}

class Mammal : Animal
{
    public override void WhoAreYou() { ... }
}

class Dog : Mammal
{
    public new void WhoAreYou() { ... }
}

class Beagle : Dog
{
    public override void WhoAreYou() { ... }
}

```

| Animal v-table         | Mammal v-table         | Dog v-table            | Beagle v-table         |
| ---------------------- | ---------------------- | ---------------------- | ---------------------- |
| [0] → Animal.WhoAreYou | [0] → Mammal.WhoAreYou | [0] → Mammal.WhoAreYou | [0] → Mammal.WhoAreYou |
|                        |                        | [1] → Dog.WhoAreYou    | [1] → Beagle.WhoAreYou |
[[(Supplement) How v-table constructs]]
#### Important:
- **Override** replaces an existing slot
- **new** creates a brand new slot
----
### 3. `new` keyword vs `override`
| Code snippet                  | Meaning                                          | Result in v-table                     |
| ----------------------------- | ------------------------------------------------ | ------------------------------------- |
| `public new void WhoAreYou()` | Method hiding → creates a _new_ unrelated method | Allocates **new slot** (e.g., slot 1) |
| `public override WhoAreYou()` | Overrides a base virtual method                  | **Replaces existing slot**            |
### Visual:
#### Using **`override`**
```csharp
Base slot 0: Base.Who() Derived slot 0: Derived.Who()   ← replaces it
```
#### Using **`new`**
```csharp
Base slot 0: Base.Who() 
Derived slot 0: Base.Who()      ← unchanged   
Derived slot 1: Derived.Who()   ← new slot!`
```
---
### 4. The golden rule (memorize this!)

> **The compile-time type of the variable decides WHICH method name/slot is called.**  
> **If that slot is virtual, the runtime type of the object decides which implementation runs.**

---
### 5. Examples that prove the rule
| Animal v-table         | Mammal v-table         | Dog v-table            | Beagle v-table         |
| ---------------------- | ---------------------- | ---------------------- | ---------------------- |
| [0] → Animal.WhoAreYou | [0] → Mammal.WhoAreYou | [0] → Mammal.WhoAreYou | [0] → Mammal.WhoAreYou |
|                        |                        | [1] → Dog.WhoAreYou    | [1] → Beagle.WhoAreYou |
```csharp

Animal a = new Beagle();

a.WhoAreYou();        // Compile-time type = Animal → slot 0 → Mammal version!

  

Dog d = new Beagle();

d.WhoAreYou();        // Compile-time type = Dog    → slot 1 → Beagle version!

```
[[(Supplement) Which Function to call?]]
### One-sentence summary of the whole ending

> “Non-virtual = fastest call, decided at compile time.  

> Virtual = tiny overhead, uses a per-type v-table, compile-time chooses slot, runtime chooses implementation.  

> `new` = new slot (hiding), `override` = replace slot.”
