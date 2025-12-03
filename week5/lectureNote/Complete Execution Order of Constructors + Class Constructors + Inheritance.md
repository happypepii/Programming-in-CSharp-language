## Example
### Program
```C#
using System;

class P
{
    public static int M(string msg)
    {
        Console.WriteLine(msg);
        return 0;   // value doesn't matter, we only care about the side-effect
    }
}

class A
{
    int x = P.M("1. Initializing A.x (instance field initializer)");

    public A(int arg)
    {
        Console.WriteLine("6. Inside A(int) constructor body");
        PrintX();
    }

    public virtual void PrintX()
    {
        Console.WriteLine("7. Inside A.PrintX() -> x = " + x);
    }

    static A()
    {
        Console.WriteLine("4. Class constructor of A (static ctor)");
    }
}

class B : A
{
    int y = P.M("3. Initializing B.y (instance field initializer)");

    public B() : this(P.M("2. Preparing argument for B(int) from parameterless B()"))
    {
        Console.WriteLine("8. Inside parameterless B() body");
    }

    public B(int dummy) : base(P.M("5. Preparing argument for base A(int)"))
    {
        Console.WriteLine("9. Inside B(int) constructor body");
        PrintX();   // still calls the overridden version if you override it
    }

    // (optional override to prove the virtual call danger)
    // public override void PrintX()
    // {
    //     Console.WriteLine("Would see y = " + y);   // ← would be 0 if init ran after base!
    // }

    static B()
    {
        Console.WriteLine("0. Class constructor of B (static ctor)");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("-1. Main starts");
        B b = new B();                  // ← this single line triggers everything
        Console.WriteLine("10. Main ends");
        Console.ReadLine();
    }
}

```

### Output (with numbers added for explanation)
```
-1. Main starts
0. Class constructor of B (static ctor)               ← first use of type B
1. Initializing A.x (instance field initializer)       ← B's field initializers run first!
2. Initializing B.y (instance field initializer)
3. Preparing argument for B(int) from parameterless B()
4. Class constructor of A (static ctor)                ← first use of type A (happens now)
5. Preparing argument for base A(int)
6. Inside A(int) constructor body
7. Inside A.PrintX() -> x = ...                       ← x is already properly initialized
8. Inside B(int) constructor body
   Inside A.PrintX() again (or overridden version)
9. Inside parameterless B() body
10. Main ends
```

### Step-by-Step Explanation (exactly the order the lecturer described)
| Step | What happens                                                                                                     | Why this order? (the key rules)                                                                             |
| ---- | ---------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| -1   | `Main` starts, prints "-1. Main starts"                                                                          | Normal method execution                                                                                     |
| 0    | **B's class constructor** runs                                                                                   | `new B()` is the first use of type **B** at all → CLR must run B's `.cctor` first                           |
| 1+3  | **All instance field initializers of B** (including those inherited from A!) run **before any constructor body** | Rule: **Instance field initializers of the most-derived class run first**, in declaration order (A.x → B.y) |
| 2    | Preparation of argument for `: this(...)`                                                                        | Code you write before `: this(...)` or `: base(...)`                                                        |
| 4    | **A's class constructor** runs                                                                                   | Calling the base constructor is the first use of type **A** → CLR runs A's `.cctor` now                     |
| 5    | Preparation of argument for `: base(...)`                                                                        | Again, argument preparation code runs                                                                       |
| 6    | Body of `A(int)` runs → calls `PrintX()`                                                                         | At this moment **x is already initialized** (step 1), safe even if `PrintX` is virtual                      |
| 9    | Body of `B(int)` runs                                                                                            | After returning from base constructor                                                                       |
| 8    | Body of parameterless `B()` runs                                                                                 | After returning from `: this(...)`                                                                          |
| 10   | Back to `Main`                                                                                                   | Object is fully constructed                                                                                 |
### The Most Important Rules You Must Remember Forever

1. **Class constructors (.cctor)**
    - Run **once**, **lazily**, just before the **first** use of the type (static or instance)
    - In inheritance: **derived .cctor → base .cctor** (derived first!)
2. **Instance field initializers**
    - Belong to the **most-derived class**
    - Always run **before** any instance constructor body (even the base one!)
    - Run in textual order (base fields first, then derived fields)
3. **Constructor chaining order** (what the C# compiler actually generates)    
    ```
    new B() 
      → B's .cctor
      → B's field initializers (A.x then B.y)
      → code before :this(...) 
      → B(int) constructor
          → code before :base(...)
          → A's .cctor
          → A's field initializers (already done in step above!)
          → A(int) constructor body
          → B(int) body
      → parameterless B() body
    ```
    
4. **Why field initializers run before the base constructor** → Because virtual method calls inside the base constructor must already see correctly initialized derived fields! → In .NET (unlike C++ or Java in some cases), virtual calls are always fully polymorphic even during construction.
5. **Why :this(...) skips its own field initializers** → Because the chained constructor will do them → avoids double execution and double side-effects.