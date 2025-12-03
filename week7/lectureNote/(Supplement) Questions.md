Q: 
```text
if B inheritates from A

and B override A.F()

can C override B.F() from class B?

since only "virtual" methods can be overrided (compare to new keyword new method can't be overrided right?)
```

| How the method is declared in the base class | Can a derived class override it? | What keyword must the derived class use? | What happens if you forget the keyword?              |
| -------------------------------------------- | -------------------------------- | ---------------------------------------- | ---------------------------------------------------- |
| `public virtual void F()`                    | YES                              | `override`                               | Compiler error: “does not override inherited member” |
| `public void F()` (normal / non-virtual)     | NO                               | You can only hide with `new`             | No override possible – only hiding                   |
| `public override void F()`                   | YES (the promise continues)      | `override`                               | Compiler error if you use `new` or nothing           |
| `public sealed override void F()`            | NO                               | Cannot override anymore                  | Compiler error if you try                            |

### Concrete example (exactly the one from the lecture)

C#

```csharp
class A
{
    public virtual void F() { Console.WriteLine("A.F"); }
}

class B : A
{
    public override void F() { Console.WriteLine("B.F"); }   // valid override
}

class C : B
{
    public override void F() { Console.WriteLine("C.F"); }   // perfectly legal!
}
```

All of these calls do what you expect:


```csharp
A x = new C();
x.F();        // → C.F   (polymorphic – goes through v-table)
```

### Comparison with new (hiding) – this one CANNOT be overridden
```csharp
class A
{
    public void F() { Console.WriteLine("A.F"); }   // non-virtual
}

class B : A
{
    public new void F() { Console.WriteLine("B.F"); }   // hiding, not overriding
}

class C : B
{
    // public override void F() { }   ← COMPILER ERROR!
    // You are NOT allowed to write override here
    public new void F() { Console.WriteLine("C.F"); }   // only hiding again
}
```

Result:

```csharp
A x = new C();
x.F();        // → A.F   (static type is A → calls A’s version)
B y = new C();
y.F();        // → C.F   (only because variable type is B or C)
```

### One-sentence rule you must memorise for the exam

> **Only methods that entered the v-table as virtual (i.e., were declared virtual or override somewhere up the chain) can be overridden further down. Methods introduced with new (hiding) or ordinary non-virtual methods are forever non-overrideable.**

So yes – once a method becomes virtual in A, every single descendant (B, C, D, …) can keep overriding it until someone writes sealed override.