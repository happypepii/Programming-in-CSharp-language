## 1. Real-world use of `[Flag]` enum
Common pattern in games / inventors
```csharp
[Flag]
public enum SlotType
{
	None = 0,
	LeftHand = 1 << 0,
	RightHand = 1 << 1,
	Head = 1 << 2,
	Neck = 1 << 3,
	//...
	Everything = ~0
}

public SlotType AllowedSlots; // on items
public SlotType AcceptedSlots; // on equipment slots
```
-> Check compatibility with one line:
```csharp
if((item.AllowedSlots & slot.AcceptedSlots) != 0) -> can equip
```
Tools (Unity Editor etc.) automatically show checkboxes/multi-select dropdowns instead of a plain number -> bc the `[Flags]` attribute via reflection.

## 2. Attributes are used by tools, IDEs, and frameworks
- Unity -> checkboxes for `[Flag]` enums
- xUnit/ NUnit -> finds `[Fact]`, `[Test]` -> run them
- ASP.NET Core, EF Core, Json serializers -> all are driven by attributes

## 3. NuGet Packages = the normal way to get libraries / frameworks
- One `.nupkg` = zip file containing one or more DLLs + metadata
- Added via \<PackageReference Include="SkiaSharp" version="1.4.3" /> in `.csproj`
- nuget.org = central public server
- only install packages with trusted authors

## 4. Some NuGet Packages contains native code 
- Native code - codes can directly compile to real machines code, such as C, C++, Rust...
- SkiaSharp, SQLite, etc. -> The packages come in two parts
	- Managed wrapper: pure C#
	- native DLLs for Windows/Linux/MacOS
	- different folder for win-x64, linux-x64, osx-arm64...
- If the correct native asses is missing -> progra crashes with "DLL not found"

## 5. Micro-benchmarking is bad
Naive way with `StopWatch` -> completely wrong results because of:
- Debug vs Release
- First-call JIT cost
- GC, CPU frequency changes
- Tiered compilation
- Background process
- ...
-> Never do this!!!!!!!
-> Instead
### Use the standard library: BenchmarkDotNet
- Automatically does warm-up, many iterations, removes JIT testing effects, statistical analysis

