Constructors’ real names in .NET + How to correctly express “constants” in C#
### 1. Why you see .ctor and .cctor in stack traces (very important!)
- C# syntax MyClass() or static MyClass() is **only sugar for the C# compiler**
- In real .NET metadata / IL, every constructor is stored as:
    - Instance constructor → method named **.ctor**
    - Static/class constructor → method named **.cctor**
- The CLR and the whole .NET runtime **have no idea** which language the code was originally written in → the only name it knows is .ctor or .cctor
- Therefore every exception stack trace, debugger, Reflector/ILSpy, etc. will show exactly these names

```C#
try { var x = new Foo(); }
catch (Exception ex) { Console.WriteLine(ex); }
```
Output you will really see:
```
System.Exception: blah blah
   at Foo..ctor()   ← yes, two dots, this is your normal constructor!
or at Foo..cctor()  ← if the static constructor threw
```
→ Never be scared when you see ..ctor or ..cctor in a crash — it is just your constructor!

**Warning from the lecturer** Never throw normal/recoverable exceptions from a static constructor (.cctor):
- It can fire at any random place (the first time the type is touched)
- You would have to wrap literally every possible first-use point in try/catch → impossible in practice
- Only throw from .cctor if something is completely broken and the process should just crash.