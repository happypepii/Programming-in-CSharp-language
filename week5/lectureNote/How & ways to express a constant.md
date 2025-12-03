
### Lecture 5 – Final Part: English Notes (Clean & Complete)

#### How to correctly represent “concepts / magic values” in C#

The lecturer compared **6 different ways** to give a name to a repeated value (e.g., `-1` = end of input, `80` = line width, `'!'` = special marker).

| # | Technique                          | Example                                      | Scope              | Truly immutable? | Can be in interface? | Memory cost                     | Best for                                                                 |
|---|------------------------------------|----------------------------------------------|--------------------|------------------|-----------------------|---------------------------------|-----------------------------------------------------------------------------|
| 1 | Normal field / variable            | `public int Width = 80;`                     | per instance       | No               | No                    | One slot per object             | NEVER for constants (allows changes → lies about intent)                  |
| 2 | `readonly` field                   | `public readonly int Width = 80;`<br>`private readonly int Width = 80;` | per instance       | Yes (only ctor)  | No                    | One slot per object             | Configurable per instance (different objects can have different values)   |
| 3 | Getter-only property (init or =>)  | `public int Width { get; } = 80;`<br>`public int Width => 80;` | per instance (or per type if static) | Yes (if => constant) | YES              | Zero if => constant (inlined)   | When the constant must be part of an interface/contract                   |
| 4 | `static readonly` field            | `public static readonly int Limit = 80;`     | per type / AppDomain | Yes (only in static ctor) | No             | One slot per AppDomain          | Global constant that never changes between runs (but cannot depend on command-line args) |
| 5 | `static` getter-only property      | `public static int Limit => 80;`             | per type           | Yes              | Yes (C# 8+ static interface members) | Zero (inlined) | **Current best practice for global constants**                             |
| 6 | `const`                            | `public const int EndOfInput = -1;`<br>`private const char Marker = '!';` | compile-time       | Yes              | No                    | Zero (value baked into IL everywhere) | Simple values known at compile time (int, string, char, bool, null, etc.) |

#### Decision Cheat-Sheet (the exact logic the lecturer wants you to use)

1. Is the value different for each instance?  
   → Yes → use `readonly` field or init/getter-only property  
   → No → continue

2. Does the value need to be part of an interface/contract?  
   → Yes → must be a property (instance or static)  
   → No → continue

3. Is the value known at compile time (number, string, etc.)?  
   → Yes → `const` (fastest, enables constant folding)  
   → No → continue

4. Global for the whole program and never changes?  
   → `public static int X => 42;` (modern best)  
   → or `public static readonly int X = 42;` (old but still ok)

#### What’s the real difference between **static** versions and **non-static** versions?

| Aspect                           | Non-static (instance) versions (2 & 3)                | Static versions (4, 5, 6)                                      |
|----------------------------------|--------------------------------------------------------|-----------------------------------------------------------------|
| How many copies exist?           | One copy **per object** you create                     | Only **one copy** for the whole program (or baked into IL)     |
| Memory usage                     | Every `new MyClass()` wastes 4-8 bytes for the value   | Almost zero (especially `=>` and `const`)                       |
| Can different objects have different values? | Yes (perfect for configurable objects)            | No – same for the entire run                                    |
| Can be used in interfaces?       | Yes (properties only)                                  | Only static properties in C# 8+ (advanced)                      |
| When does the value get decided? | At instance construction time                          | Compile time (`const`) or very early at program start (static)  |
| Typical real-world example       | `LineJustifier justifier = new(width: 120);`           | `Console.OutputEncoding = Encoding.UTF8;` (global setting)     |

#### Quick modern recommendation (2025 best practice)

```csharp
// 1. Simple magic number → const
public const int EndOfFile = -1;

// 2. Global setting that never changes → static getter
public static int DefaultLineWidth => 80;

// 3. Per-instance configurable → init property (or readonly field)
public int MaxWidth { get; }

// 4. Need interface contract
public interface IHasLimit { int Limit { get; } }
```


## Const and Enum

The lecturer used these slides as a **final warning** after teaching the 6 ways to define constants.  
The goal: **prove that using a bunch of `const int` to simulate enums is extremely dangerous**, and even real `enum` is not 100% safe if you misuse it.

#### The Code on the Slides (fully restored & runnable)

```csharp
using System;

class Program
{
    static void Main()
    {
        DayOfWeek day = DayOfWeek.Saturday;

        // =============== Part 1: enum is still just an int underneath ===============
        // day = Month.May;       // Compile error → good!
        // day = 5;               // Compile error → good!

        day++;                    // Allowed! (dangerous)
        Console.WriteLine(day);  // Sunday

        day++;                    // Allowed again!
        Console.WriteLine(day);  // Monday (value is now 8 → out of range!)

        Console.WriteLine();

        // =============== Part 2: for-loop on enum looks safe... ===============
        Console.WriteLine("+++ Not teaching:");
        for (Month month = Month.May; month <= Month.September; month++)
        {
            Console.WriteLine(month);
        }
        // Output: May June July August September → looks perfect

        Console.WriteLine();

        // =============== Part 3: explicit cast destroys type safety ===============
        Console.WriteLine("After explicit cast:");
        day = (DayOfWeek)123;     // Compiles and runs!
        Console.WriteLine(day);  // 123 (not a real day)

        Console.WriteLine();

        // =============== Part 4: random months with cast → chaos ===============
        Console.WriteLine("+++ Few random months:");
        var r = new Random();
        for (int i = 0; i < 10; i++)
        {
            int value = r.Next(0, 13);  // 0~12 (sometimes 12 = December + 1)
            Month randomMonth = (Month)value;
            Console.WriteLine(randomMonth);  // sometimes prints garbage like "13"
        }

        Console.ReadLine();
    }
}

enum DayOfWeek { Sunday = 0, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday }
enum Month { January = 0, February, March, April, May, June, July, August, September, October, November, December }
```

#### What the lecturer wanted to scare you with

| Part | What happens                               | Why it’s terrifying                              |                                                                                          |
| ---- | ------------------------------------------ | ------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| 1    | `day++` twice                              | Saturday → Sunday → Monday (value 8)             | `enum` is just a fancy `int`. Arithmetic operations are allowed → easy out-of-range bugs |
| 2    | `for (Month m = May; m <= September; m++)` | Works perfectly                                  | Looks safe, but it’s only safe because we didn’t break it yet                            |
| 3    | `(DayOfWeek)123`                           | Compiles and prints "123"                        | Explicit cast completely destroys type safety                                            |
| 4    | Random cast to `Month`                     | Sometimes prints valid months, sometimes garbage | Real programs can silently get invalid values                                            |

#### Final Takeaway from the Lecturer (the punchline)

| Approach                              | Type safety | Range checking | Lecturer’s verdict |
|---------------------------------------|-------------|----------------|---------------------|
| Bunch of `const int Monday=0, Tuesday=1, …` | None        | None           | Extremely dangerous – avoid forever |
| Real `enum`                           | Names only  | Very weak (allows `++`, casts) | Much better, but still not bulletproof |
| `enum` + never do `++` / casts        | Good        | Good           | Current best practice |
| Future (C# 10+ with source generators / analyzers) | Perfect | Perfect     | 100% safe (advanced) |

#### Golden Rule (remember this forever)

> **Never, ever simulate an enum with a bunch of `const int`.**  
> **Even with real enums: never do `++`, `--`, or unchecked casts.**  
> **If you need a closed set of named values → use `enum`. For everything else → the 6 constant techniques from the previous slides.**
