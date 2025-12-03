### 1. Interface dispatch – the shocking detail everybody misses

**Why did he spend 10 minutes on this?** Because 99 % of C# programmers believe that hiding with new also hides interface calls → completely wrong!


```C#
interface I1 { void M1(); void M2(); }

class B : I1 {
    public void M1() { … }          // non-virtual
    public virtual void M2() { … }
}
class C : B { public new void M1() { … } }   // hiding
```

|Call|What actually executes when the real object is C?|
|---|---|
|((I1)c).M1()|Always B.M1() – the hiding in C is ignored!|
|((I1)c).M2()|C.M2() – normal virtual dispatch|

**Rule (he repeated twice)** Interface method table stores a pointer to the method descriptor that was chosen when the class said it implements the interface. → Non-virtual method → direct pointer to code → Virtual method → pointer to the v-table slot → Hiding with new in a derived class never touches the interface table → interface calls are completely unaffected!

**Consequences**

- You cannot “shadow” an interface implementation with new.
- Re-implementing the interface (class D : B, I1) rewrites the interface table → then hiding works.
### 2. Abstract classes vs Interfaces – final comparison table

(he drew this on the board at ~1:05)

|Feature|Abstract class + abstract/virtual methods|Interface|
|---|---|---|
|Can define a contract?|Yes|Yes|
|Contract visibility|Can be public, protected, internal…|Always public|
|Extensibility promise|You control it (virtual / sealed override)|Always extensible unless you make the impl virtual/abstract|
|Can contain instance fields / state?|Yes|No (only static / default interface methods in C# 8+)|
|Multiple inheritance|No (only one base class)|Yes|
|Diamond problem|Impossible (no multiple class inheritance)|Impossible (no data → no conflict)|
|Default implementation with state|Easy (protected fields + helper methods)|Impossible (no fields)|

**His rule of thumb (he said this is the default you should follow)** → Use interface whenever you are describing a contract (99 % of cases). → Switch to abstract class only when you need to give descendants shared state or complex default implementation.

### 3. Multiple interface inheritance & the diamond “problem” (1:12 – 1:17)

C#

```
interface IDisposable { void Dispose(); }
interface IReadable : IDisposable { … }
interface IWritable : IDisposable { … }
interface IReadWritable : IReadable, IWritable { }   // perfectly fine
```

- No problem at all → interfaces have no data → no diamond conflict.
- This is why .NET allows unlimited interface inheritance but forbids multiple class inheritance.

### 4. The killer argument: Why C# forces you to write virtual and override explicitly

(1:20 – end – the most important 15 minutes of the whole lecture)

**Scenario everybody has in real code**

C#

```
class Bender {
    void G() { /* long safety checks */ F(); }   // calls F
    void F() { KillAllHumans(); }                // dangerous code
}

class NiceRobot : Bender {
    void F() { ShowKitten(); }   // we just want to show a cute kitten
}
```

|Language|What happens by default?|Result when calling G() on NiceRobot|
|---|---|---|
|Java|Everything is virtual → accidental override|Kills all humans instead of showing kitten|
|C++|Extremely confusing rules (virtual is optional, nothing can mean override…) → one tiny change in base class silently turns hiding into overriding|Same bug, just harder to see|
|C#|virtual = new slot, no keyword = hiding|Always shows kitten – safe by default|

**C# fixes it forever**

C#

```
class Bender {
    public virtual void F() { KillAllHumans(); }
}
class NiceRobot : Bender {
    public override void F() { ShowKitten(); }   // you are forced to write override
}
class EvenNicerRobot : NiceRobot {
    public new void F() { ShowThreeKittens(); }  // no override → hiding → safe
}
```

- Adding virtual in base → never silently breaks derived classes.
- Forgetting override → compiler error (since C# 5 with [Override] warning turned to error in most projects).

### 5. Bonus: How to correctly call the base implementation without infinite recursion

C#

```
class C : B {
    public override void F() {
        base.F();        // forces non-virtual call to exactly B.F
        KillAllDwarves();
    }
}
```

base.F() → always generates call (not callvirt) → calls exactly the version in B, never the most-derived one → no recursion.

### Final slide he showed (memorise this!)

|Language|Default|Accidental override possible?|Safe by default?|
|---|---|---|---|
|Java|virtual|Yes – silent|No|
|C++|insane rules|Yes – extremely easy|No|
|C#|non-virtual|Impossible without explicit override|YES|

**His closing sentence (write it down exactly)** “C# is the only mainstream language that forces you to be explicit about creating a new virtual method or overriding an existing one. This is not bureaucracy – this is the reason real-world code bases in C# don’t explode when a library author adds a new virtual method or when you refactor. Java and C++ learned it the hard way. We got it right from the beginning.”

>介面的預設解析順序：
>1. 找 explicit
>2. 找 override（v-table）
>3. 找 public method（可當成實作的那個）
>=> 因此 `new` method 不能被被選到（因為不存在於v-table中）
