## Core Idea
**Attributes** are "sticky note" attached to the code (classes, methods, parameters, and so on)
- They carry **extra information** that normal C# syntax cannot express
- They're stored in the assembly's **metadata** (not in CIL code)
- By themselves, attributes do nothing -> someone has to read them

## How Custom Attributes work?
1. Create a normal class that inherits from 'System.Attribute'
	```csharp
	public class MyInfoAttribute: Attribute {...}
	// Convention: name ends with "Attribute"
	```
2. You apply it with square brackets
	```csharp
	[MyInfo]
	[Obsolete("Don't use this")]
	[Flags]
	public enum Day {...}
	```
3. At compile time:
	- No object is created automatically
	- The compiler only writes the attribute + its parameters into **metadata**

4. At runtime or compile time, **someone reads the metadata** using **reflection**
	- C# compiler -> `[Obsolete]`, `[AttributeUsage]`
	- Visual Studio/ Rider -> syntax highlighting, strikethrough
	- .Net libraries -> `[Flag]` -> `ToString()` prints "Monday, Tuesday"
	- Framework -> `[Test]`, `[HttpGet]`, and so on

## Summary
| Attribute                 | Who reads it?                 | Effect                                       |
| ------------------------- | ----------------------------- | -------------------------------------------- |
| `[Obsolete]`              | compiler + ide                | Warning + StrikeThrough                      |
| `[Flag]`                  | Enum.ToString(), Enum.Parse() | "Red, Blue" instead of 3                     |
| `[AttributeUsage]`        | C# compiler                   | Restricts where your attribute can be placed |
| `[Test]`,`[Fact]`         | Test runners (XUnit)          | Marks a method as a test                     |
| `[HttpGet]`, `[Required]` | ASP.NET, EF Core              | Drives framework behavior                    |

> Custom attributes are the standard, extensible way C# and .Net let you add metadata to code so that the compiler, IDE, runtime, or your own tools can change their behavior without any built-in language magic

