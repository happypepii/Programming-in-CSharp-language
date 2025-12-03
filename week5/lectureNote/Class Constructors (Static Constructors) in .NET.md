## Class constructor vs. Instance constructor
| Feature                 | Class constructor (static constructor)                                                                                   | Instance constructor (normal constructor)                                 |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------- |
| Syntax in C#            | `static MyClass() { … }`                                                                                                 | `public MyClass() { … }` (or with parameters)                             |
| What it initializes     | The **type itself** (static fields, type-level data)                                                                     | One **specific object/instance** (instance fields)                        |
| When it runs            | Exactly **once** per AppDomain, lazily, just before the **first use of the type** (any static or instance member access) | **Every time** you create a new object with `new MyClass()`               |
| Who calls it            | CLR calls it automatically (you never call it directly)                                                                  | You (or the runtime) call it explicitly with `new`                        |
| Can have parameters?    | No — always parameterless                                                                                                | Yes — can have parameters                                                 |
| Can be more than one?   | No — at most one per type                                                                                                | Yes — you can have many overloaded constructors                           |
| Access modifier?        | None (not even private)                                                                                                  | public, private, protected, etc.                                          |
| Appears in IL as        | `.cctor` (type constructor)                                                                                              | `.ctor` (instance constructor)                                            |
| Thread-safety guarantee | CLR guarantees it runs **exactly once**, even with multiple threads                                                      | No special guarantee — you have to make it thread-safe yourself if needed |
### Simple example
``` C#
class Person
{
    // This is the class constructor (static constructor)
    static Person()
    {
        Console.WriteLine("Class constructor runs ONCE for the whole type Person");
        RegisteredCount = 0;
    }

    // These are instance constructors (normal constructors)
    public Person()                  // parameterless instance ctor
    {
        Console.WriteLine("Instance constructor runs for EACH new object");
        RegisteredCount++;
    }

    public Person(string name) : this()   // another instance ctor
    {
        Name = name;
    }

    public static int RegisteredCount;  // static field → initialized in .cctor
    public string Name;                  // instance field
}
```

``` C#
Console.WriteLine("=== Start ===");
var p1 = new Person("Alice");   // → class ctor runs here (first use), then instance ctor
var p2 = new Person("Bob");     // → only instance ctor runs again
```
output:
```
=== Start ===
Class constructor runs ONCE for the whole type Person
Instance constructor runs for EACH new object
Instance constructor runs for EACH new object
```
## Context & Motivation
We previously saw that initializers for **instance fields** are automatically moved into every instance constructor (including the hidden parameterless one).  
Now the question is: what happens when the field is **static**? Those initializers can’t run for every object because static fields belong to the type itself, not to individual instances. .NET solves this with a special mechanism called the **class constructor** (also known as static constructor or type initializer).

## Core Rules (the things you must remember)

1. **Declaration in C#**
   ```csharp
   class MyClass
   {
       static MyClass()   // ← this is the class constructor
       {
           // runs exactly once
       }
   }
   ```
   - No access modifier (not even private)
   - Always `static`, always parameterless
   - At most one per type

2. **When does it run?**
   - Exactly **once per AppDomain**
   - **Lazily** – only just before the **first real use** of the type in that particular program execution
   - The CLR guarantees thread-safety: even with many threads racing, it will never run more than once

3. **What counts as “first use” that triggers it?**
   - Reading or writing any static field
   - Reading or writing any instance field
   - Calling any static method
   - Calling any instance method (including the instance constructor during `new MyClass()`)

4. **What does NOT trigger it?**
   - `typeof(MyClass)`
   - Declaring a variable `MyClass x;`
   - The type existing in metadata only

5. **Static field initializers are automatically moved into the class constructor**
   ```csharp
   static int Count = 42;        // ← this line is moved into .cctor
   static List<string> Log = new List<string> { "init" };
   ```
   Even if you write your own explicit static constructor, these initializers still execute **first**, before your code.

6. **IL & JIT level – what really happens**
   - C# compiler emits a special method named `.cctor` (type constructor)
   - The IL itself contains no call to `.cctor`
   - When the JIT compiler compiles a method that touches the type for the first time, it checks its internal tables:
     - If the type is not yet initialized → JIT inserts a conditional call to `.cctor`
     - Subsequent methods that use the same type → no check, direct access (faster)

   This is why the very first method (in execution order) that uses a type is slightly slower – it carries the initialization check.

## Practical Consequences (very important!)

- Static constructors are **not guaranteed to run at program startup**.  
  If the type is never used in a particular run, its class constructor never executes.
- You **cannot** reliably use static constructors for “register this type at startup” like you can with C++ static objects or module initializers.
- The exact location where the static constructor runs can differ between runs (depends on which code path touches the type first).

## Quick Reference Table

| Aspect                     | Class Constructor (static ctor)                | Instance Constructor (normal ctor)             |
|--------------------------------|------------------------------------------------|------------------------------------------------|
| C# syntax                      | `static MyClass() { … }`                       | `public MyClass() { … }`                       |
| IL name                        | `.cctor`                                       | `.ctor`                                        |
| Runs                           | Once per AppDomain, lazily                     | Every `new`                                    |
| Triggered by                   | First use of any member (static or instance)  | Explicit `new`                                 |
| Parameters / overloads         | Never                                          | Yes                                            |
| Access modifier                | None                                           | public / private / etc.                        |
| Who calls it                   | CLR automatically                              | Your code (`new`)                              |
| Static field initializers go here | Yes (automatically)                          | No                                             |

