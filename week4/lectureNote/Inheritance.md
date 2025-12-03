
## Abstract Class Cannot Be Instantiated

- **Explanation**: An abstract class is a blueprint with abstract methods (no implementation); it cannot be directly instantiated with new. Forces subclasses to inherit and implement abstracts.
- **Example**:
    ```C#
    abstract class Shape { public abstract double Area(); }  // No new Shape()
    class Circle : Shape { public override double Area() => Math.PI * 2; }  // Subclass implements
    Shape s = new Circle();  // OK: Upcasting
    ```
- **Why**: Ensures type completeness, prevents incomplete objects.

## Everything Inherits from System.Object (CLI Type System)

- **Explanation**: All classes implicitly inherit System.Object (root type), providing common methods like ToString(), Equals(), GetHashCode(). Structs don't inherit but have similar behaviors.
- **CLI Link**: Common Language Infrastructure defines .NET types; Object is base for all reference types. Interfaces implement via :, not inherit.
- **Example**:
    ```C#
    class MyClass { }  // Implicit: MyClass : Object
    MyClass obj = new();  // obj.ToString() available
    ```
    
- **Why**: Unified API for polymorphism and reflection.

## Constructors Are Not Inherited

- **Explanation**: Inheritance covers methods/properties/fields, but not constructors. Subclass B : A must explicitly call base constructor with base(); defaults to parameterless base if omitted.
- **Example** (from code):
    ```C#
    class A { public A() { Console.WriteLine("A ctor"); } }
    class B : A { public B() : base() { Console.WriteLine("B ctor"); } }
    new B();  // Output: A ctor → B ctor (chaining)
    ```
    
- **Why**: Constructors initialize specific state; subclass controls base init. No base() implicitly calls it—if base lacks parameterless, compile error.

## is vs. typeof()

- **Explanation**:
    - is: **Runtime** checks if object _is_ a type (or subclass/implements interface); returns bool, supports polymorphism. 
    - typeof(): **Compile-time** gets Type object for reflection (e.g., method info); checks types, not instances. 
- **Example**:
    ```C#
    A a = new B();  // Upcasting
    Console.WriteLine(a is B);     // true (runtime: actually B)
    Console.WriteLine(typeof(A) == typeof(B));  // false (compile-time: A != B)
    ```
- **Difference**: is dynamic (instance-based); typeof static (type-based). Use is for compatibility, typeof for metadata.

# Code example
```C#
using System;

class Program {
    static int AddTwoNumbers(string number1AsString, int number2AsInt) {
        int number1AsInt = int.Parse(number1AsString);
        return number1AsInt + number2AsInt;
    }
    static int CoolVariable = 5;

    static void Main(string[] args) {
        Console.WriteLine("Hello, your result is: " + AddTwoNumbers("42", CoolVariable));

        Console.WriteLine("Creating A instance:");
        var a = new A();

        Console.WriteLine("Creating B instance:");
        A a = new B();  // 向上轉型：B 物件存為 A 參考
    }
}

class A {
    public A() { Console.WriteLine("A.A() constructor"); }
}

class B : A {  // B 繼承 A
    public B() { Console.WriteLine("B.B() constructor"); }
}
```

**Creating Instance of B (with Upcasting)**:
- Console.WriteLine("Creating B instance:");
- A a = new B(); // Upcasting: B object stored in A reference
    - Allocates memory for a **B object** (which includes A's members due to inheritance).
    - **Constructor Chaining**: In C#, when creating a derived class instance:
        - First, the **base class (A) constructor** is called automatically (even if not explicit).
        - Then, the **derived class (B) constructor** runs.
    - Prints: "A.A() constructor" (from A) → "B.B() constructor" (from B).
- The variable a is of type A, so you can only access A's members via a (e.g., no direct access to B-specific methods without casting).

#### **Key Concepts Demonstrated**
- **Inheritance (B : A)**:
    - B "is-a" A: B inherits A's constructor implicitly (via chaining), fields, and methods.
    - Upcasting (A a = new B();): Safe (B derives from A), but limits access to A's interface. To access B-specific stuff: ((B)a).SomeBMethod(); or use is/as.
- **Constructor Chaining**:
    - Default: Derived constructor implicitly calls base() (A's constructor).
    - Why A first? Base must be fully initialized before derived adds its own state (OOP principle).
    - If A had parameters, B's constructor would need base(arg) explicitly.
- **Static vs. Instance Members**:
    - AddTwoNumbers and CoolVariable: Static—tied to class, not object. Called without new.
    - Constructors: Instance—run on new.
- **Type Conversion**:
    - int.Parse("42"): Runtime conversion (string → int). Throws if invalid (e.g., "abc").
- **No Errors Here**: Everything is concrete (no abstracts). If B didn't call base implicitly, it'd fail compilation.
## Verification
```C#
class A { public int Id; // 基類必要狀態 public A() { Id = 1; Console.WriteLine("A init"); } // 必須先跑 }

class B : A { public B() { // 依賴 A.Id Console.WriteLine($"B uses A.Id: {Id}"); // 若無 A init，這裡 Id=0 錯 } }

A a = new B(); // 先 A() → B()，輸出：A init → B uses A.Id: 1
```