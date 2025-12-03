## Nullable Reference Example in C#

### Code Section
```csharp
namespace TraditionalCSharpNullable

{

	class A
	
	{
	
		public int x;
	
	}

  

	class Program
	
	{
	
		static void Main(string[] args)
		
		{
		
			A a1 = new A { x = 10 };
			
			Console.WriteLine($"a1 == {a1}");
			
			  
			
			a1 = null;
			
			Console.WriteLine($"a1 == {a1}");
		
		  
		
			/**/
			
			Console.WriteLine($"a1.x == {a1.x}"); // '.' -> check at runtime
			
			/*/
			
			string s = a1.ToString();
			
			Console.WriteLine("will print s:");
			
			Console.WriteLine(s);
			
			/**/
		
		}
		
	}
	
}
```

### What It Shows
This snippet demonstrates what happens when you access a **reference type** variable after setting it to `null`.
- `a1.x` and `a1.ToString()` both cause a **NullReferenceException** once `a1 = null;`        
### Key Idea
`class` variables hold **references**, not actual data.  
Setting them to `null` removes that reference — meaning there’s no object to access.  
Trying to use `null` like a real object crashes at **runtime**.

# Nullable Reference Types (C# 8+)

### Overview
Before C# 8, **all reference types** (like `string`, `class`, etc.) could freely be assigned `null`.  
C# 8 introduced **nullable reference types (NRT)** — a compile-time feature that helps detect potential null reference errors *before runtime*.

It doesn’t change how the CLR works; it only adds **compiler warnings**, not runtime restrictions.

---

### Example

```csharp
#nullable enable

string s = null;   // ⚠️ Warning: possible null assignment
string? s2 = null; // ✅ OK — explicitly marked as nullable

Console.WriteLine(s.Length);   // ⚠️ Warning: possible null dereference
Console.WriteLine(s2.Length);  // ⚠️ Warning: possible null dereference

```

- `string` → expected to be **non-nullable**
- `string?` → explicitly **nullable**
- The compiler warns when it detects a possible null value in a non-nullable variable.
### Behavior Summary

|Language version|Can assign `null` to reference type|Compiler behavior|Runtime behavior|
|---|---|---|---|
|C# 7 and earlier|✅ Yes|No warnings|Can cause `NullReferenceException`|
|C# 8+ (nullable disabled)|✅ Yes|No warnings|Same as before|
|C# 8+ (`#nullable enable`)|⚠️ Allowed but warned|Warning only|Same as before|
|Using `string?` / `A?`|✅ Yes|No warning|Safely nullable|

---

### Key Idea

- **Reference types** still can be `null` at runtime.
- **Nullable reference types** only change how the _compiler analyzes nulls_.
- It encourages safer code by forcing developers to think:
    - Should this value ever be `null`?
    - If yes → mark it as `Type?`
    - If no → compiler will warn if it might accidentally be null.

