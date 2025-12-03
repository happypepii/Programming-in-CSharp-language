# More Keywords

| Keyword           | Meaning for future descendants                                        | Example                               |
| ----------------- | --------------------------------------------------------------------- | ------------------------------------- |
| `virtual`         | “I start a promise – everyone down the chain may override”            | `public virtual void G()`             |
| `override`        | “I fulfil the promise with a better version – promise stays open”     | `public override void G()`            |
| `sealed override` | “I fulfil the promise, but I close it – no one below me may override” | `public sealed override void G()`     |
| `sealed class`    | “This whole class is a leaf – no one may inherit from it”             | `public sealed class MyStringBuilder` |
**Side benefit**: sealed override → JIT can devirtualise the call even when the variable is of the base type, because it knows no further override is possible.
[[(Supplement) Questions]]
# `virtual`vs.`abstract` 

| Question you must be able to answer →         | `virtual` method                                              | `abstract` method                                           |
| --------------------------------------------- | ------------------------------------------------------------- | ----------------------------------------------------------- |
| Can the class be instantiated?                | Yes – completely normal class                                 | No – the class itself must be marked `abstract`             |
| Does the method have a body / implementation? | Yes – it has a real, working implementation (the default one) | No – no body at all, only signature                         |
| Can you call it without overriding?           | Yes – if nobody overrides, the base implementation runs       | No – compiler error if a derived class does not override it |
| What does it put in the v-table?              | New slot + fills the slot with the base implementation        | New slot + the slot stays empty (null)                      |
| What promise does it make?                    | “You MAY override me if you want, but you don’t have to”      | “You MUST override me – I am deliberately incomplete”       |
| Real-life example                             | `ToString()`, `Equals()` in most classes                      | `Draw()` in a `Shape` base class                            |
| Syntax                                        | `public virtual void Draw() { …default… }`                    | `public abstract void Draw();`                              |
### One-sentence summary he repeats in every lecture

- **virtual** = “Here is a sensible default, feel free to replace it.”
- **abstract** = “There is no sensible default in the base class – you are forced to write your own.”

### Visual of the v-table (what he always draws)

text

```
class Animal          class Animal
{                     {                       abstract class Animal
    public virtual void Speak()          public abstract void Speak();
    { Console.WriteLine("???"); }        // no body!
}                     }                       }

Animal a = new Animal();   → OK         new Animal(); → compile error!
Dog d = new Dog();                    Dog d = new Dog();
d.Speak();  → prints "???" (default)   d.Speak(); → must print "Woof" (forced override)
```

### Decision table he wants you to memorise

| Situation                                                               | Use virtual | Use abstract |
| ----------------------------------------------------------------------- | ----------- | ------------ |
| You can provide a reasonable default implementation                     | Yes         | No           |
| You want derived classes to be able to keep the default                 | Yes         | No           |
| The base class concept makes no sense without a concrete implementation | No          | Yes          |
| You want to force every single derived class to implement it            | No          | Yes          |

### His exact words at the end of the lecture

“Never make something abstract just because you are lazy. Only make it abstract when the base class literally cannot know what the correct behaviour is. Everything else gets virtual with a good default or stays non-virtual.”
# What virtual / override / abstract actually do to the v-table

|Declaration in code|What happens in the v-table|
|---|---|
|`virtual void F()`|New slot created (e.g. slot 0), implementation filled in|
|`abstract void F()`|New slot created, but the entry is null → class becomes abstract|
|`override void F()`|Existing slot is overwritten with the new implementation|
|normal method (no keyword)|No slot at all → direct call, not in v-table|
|`new void M1()` (hiding)|No new slot → just a new non-virtual method with the same name|

## Interfaces vs. Normal virtual methods
```csharp
interface I1 { void M1(); void M2(); }

class B : I1
{
    public void M1() { Console.WriteLine("B.M1"); }  // non-virtual!!
    public virtual void M2() { Console.WriteLine("B.M2"); }
}

class C : B
{
    public new void M1() { Console.WriteLine("C.M1"); } // hiding!
    public override void M2() { Console.WriteLine("C.M2"); }
}
```
Now run this code:
```csharp
I1 x = new C();
x.M1();    // → ???????
x.M2();    // → C.M2  (expected)
```
**Result: prints “B.M1”** 
Even though the real object is C and C has its own M1 → the interface call completely ignores the new hiding!
### Why? → Two Completely Different Tables

| Table name                     | Normal inheritance (virtual methods) | Interfaces                              |
| ------------------------------ | ------------------------------------ | --------------------------------------- |
| Virtual method table (v-table) | One per class                        | Not used for interface dispatch         |
| Interface method table         | Does not exist for normal classes    | One per implemented interface per class |
## Step-by-Step: What Actually Happens Inside .NET
#### 1. When B implements I1

The CLR creates an **interface method table** specifically for the pair (B, I1):

| Slot in I1 interface table | What the CLR writes into the slot for type B                      |
| -------------------------- | ----------------------------------------------------------------- |
| 0 – I1.M1                  | Pointer to the method descriptor of B.M1 (the non-virtual method) |
| 1 – I1.M2                  | Pointer to slot 1 in B’s normal v-table (the virtual M2)          |
Important details:

- For **non-virtual** implementing methods → stores a pointer to the method itself (not the final code address yet).
- For **virtual** implementing methods → stores a pointer into the normal v-table.
#### 2. When C inherits from B

C automatically implements I1 too (inheritance rule: you can’t drop interfaces).

The CLR simply copies B’s interface table → C gets exactly the same table!

|Slot in I1 interface table for type C|Content (copied from B)|
|---|---|
|0 – I1.M1|Still points to B.M1 method descriptor|
|1 – I1.M2|Still points to v-table slot → now correctly points to C.M2 because C overrode it|

The new void M1() in C is completely invisible to the interface table → it never gets a chance to be inserted.

#### 3. What happens at runtime when you call x.M1()

C#

```
I1 x = new C();
x.M1();          // compiled to callvirt I1.M1
```

Machine code generated by the JIT:

asm

```
mov  rcx, [x]          ; load the object reference
call System.RuntimeType.GetInterfaceMap   ; find the interface table for I1
mov  rax, [interface_table + 0*8]         ; slot 0 → points to B.M1 method descriptor
call rax                                   ; finally jumps to B.M1 code
```

→ Always B.M1, forever. The hiding in C is ignored.

For M2 it goes through the normal v-table → correctly gets C.M2.

#### Super Simple Analogy (the one the professor uses in the next semester)

Normal virtual methods = family photo album → every child gets their own copy and can replace photos (override).

Interfaces = a framed contract hanging on the wall → when B signed the contract, it wrote “I will do M1 this way”. → C inherits the house with the same framed contract still on the wall. → C can paint over photos in its own album, but it can’t change what’s written in the frame.

#### The Rule You Must Tattoo on Your Forehead

|How the method is implemented in the base class|Does new hiding in a derived class affect interface calls?|
|---|---|
|public void M() (non-virtual)|NO – interface keeps calling the base version forever|
|public virtual void M()|YES – normal override rules apply|

#### Why Did Microsoft Design It This Way?

So that interface contracts are rock-solid. If hiding could break interface calls, every library author would live in fear that some random derived class silently broke the contract.

#### Final Table – Memorise This

| Call style     | Variable type | Real object | M1 is non-virtual in B | M1 is virtual in B   |
| -------------- | ------------- | ----------- | ---------------------- | -------------------- |
| ((B)obj).M1()  | B             | C           | B.M1 (normal hiding)   | B.M1                 |
| ((C)obj).M1()  | C             | C           | C.M1 (hiding wins)     | C.M1                 |
| ((I1)obj).M1() | I1            | C           | B.M1 (interface wins)  | C.M1 (override wins) |

| Interface slot | What B puts in it             | What C inherits                  | What actually gets called via I1 |
| -------------- | ----------------------------- | -------------------------------- | -------------------------------- |
| I1.M1 (slot 0) | Pointer to the method B.M1    | Still points to B.M1 (not C.M1!) | Always B.M1 implementation       |
| I1.M2 (slot 1) | Pointer to v-table slot of M2 | Points to the overridden version | Correct polymorphic behaviour    |
**The crucial rule** When a class implements an interface method with a non-virtual method → the interface table stores a pointer to the method descriptor, not directly to the code. → Hiding with new in a derived class does NOT affect the interface dispatch! → Interface calls are always virtual, even if the implementing method is non-virtual.

**His exact words** “Interface method table stores the method, not the implementation. That’s why new hiding never affects interface calls – only real overrides do.”
# Interfaces vs. abstract classes
– Exactly How the Professor Wants You to Know It for the Exam and Real Life

|Feature|Interface (interface I…)|Abstract class (abstract class A…)|
|---|---|---|
|Can have instance fields?|NO – never (not even private ones)|YES – normal fields, properties, events|
|Can have constructors?|NO|YES (including private/protected ones)|
|Can have non-virtual methods with code?|C# 8+ allows default interface methods (but still no fields)|YES – normal methods, virtual, non-virtual, anything|
|Multiple inheritance allowed?|YES – a class can implement 100 interfaces|NO – only one base class|
|Access modifiers on members?|Before C# 8: all members implicitly public|Normal access modifiers (private, protected, internal, etc.)|
|Versioning – can you safely add a new member later?|NO – adding a new member to an existing interface breaks all existing implementers (they won’t compile)|YES – you can add a new virtual/abstract method and give it a default implementation → existing derived classes still compile|
|Speed (virtual dispatch)|Slightly slower – goes through the interface method table|Slightly faster – normal v-table (and sealed override devirtualises)|
|Hiding with new affects calls?|NO – interface calls completely ignore new hiding (see previous page)|YES – normal C# hiding rules apply|
|Designed for…|“This thing can DO something” (behaviour contract)|“This thing IS something” (is-a relationship)|
|Real-world example|IComparable<T>, IEnumerable<T>, IDisposable|Stream, Control, ComponentModel.Component|

### The One-Sentence Rule the Professor Repeats Every Semester

> “Use an interface when you care about WHAT something can do. Use an abstract class when you want to share code or state and you have a clear is-a hierarchy.”

### Classic Real Examples from .NET Itself

|Use interface|Use abstract class|
|---|---|
|IEnumerable<T> – anything that can be enumerated|Stream – all streams share position, length, Close() logic|
|IComparable<T> – anything that can be sorted|Control – all UI controls share Handle, Parent, etc.|
|IDisposable – anything that needs cleanup|Component – shares ISite, events, container|

### Versioning – The Killer Reason Abstract Classes Win in Libraries

C#

```
// BAD – interface in a library
public interface IParser
{
    void Parse(string s);
    // v2 of your library wants to add this:
    bool TryParse(string s, out Result r);   // breaks every existing implementer!
}

// GOOD – abstract class in a library
public abstract class Parser
{
    public abstract void Parse(string s);
    // v2 – you can safely add this:
    public virtual bool TryParse(string s, out Result r) => false; // old code still compiles
}
```

### Summary Table He Shows Before Every Exam

|Question you ask yourself|Answer → choose…|
|---|---|
|Do I need to share fields / protected state?|Abstract class|
|Do I need multiple inheritance?|Interface|
|Will this API be in a public NuGet package?|Abstract class (safer versioning)|
|Is it purely a capability / role?|Interface|
|Do I need default implementation that uses state?|Abstract class|
|Do I want to force implementers to write everything themselves?|Interface|

### His Final Words Every Time

> “In real Microsoft code you will see 10 abstract base classes for every 1 interface that actually has more than one method. Interfaces are beautiful in theory, but abstract classes are what make large, long-lived libraries possible.”