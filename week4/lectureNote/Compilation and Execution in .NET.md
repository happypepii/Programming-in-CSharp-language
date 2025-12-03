## 1. Compilation in C++: The Traditional Approach

The lecture starts by reviewing C++ compilation for multi-file projects, as a baseline for comparison.

### Process Overview

For a project with multiple .cpp source files:

1. **Compile Each Source File**: Run the C++ compiler (e.g., g++ or cl.exe) on each .cpp file individually.
    - Output: Object files (.obj on Windows, .o on Linux/macOS).
    - These contain **machine code** specific to the target platform (e.g., x86 for 32-bit desktops).
2. **Link Object Files**: Use a **linker** to combine .obj files into a final executable (e.g., .exe on Windows).
    - Result: Platform-specific binary with native machine code.

### Portability Challenges

- **Source Code Portability**: The same .cpp files can be recompiled for different platforms (e.g., x86 → x64).
- **Binary Portability Issue**: End-users must download the correct executable for their hardware/OS. Recompiling for every possible target (e.g., 32-bit vs. 64-bit) is burdensome.
- **User Friction**: Non-technical users struggle to select the right binary.

|Step|Input|Tool|Output|Platform Dependency|
|---|---|---|---|---|
|Compile|.cpp files|C++ Compiler|.obj/.o files|Yes (machine code)|
|Link|.obj/.o files|Linker|.exe|Yes (final binary)|

**Whiteboard Visualization** (from image): Left side shows C++ flow: Source (.cpp) → C++ Compiler → .obj → Linker → Executable (.exe).

---

## 2. Apple's Solution: Fat/Universal Binaries

To address binary portability for non-experts, the lecture discusses Apple's historical approach (relevant to C++-like ecosystems).

### Historical Context

Apple has switched CPU architectures multiple times for optimal performance:

- 1980s: Motorola 68000 (8-bit precursor).
- 1990s: PowerPC (IBM collaboration).
- 2000s: Intel x86/x64 (aligned with Windows desktops).
- 2020s: Apple Silicon (M1/M2/M3 – ARM64-based).

Each switch requires recompilation due to incompatible instruction sets.

### The "Fat Binary" Trick

- **Fat Binary**: A single executable embedding machine code for **two** architectures (e.g., Motorola 68000 + PowerPC).
    - OS loads only the relevant half at runtime.
- **Universal Binary**: Extension for **multiple** architectures (e.g., x86 + ARM64).
    - Compile source code 4x (once per target) and bundle into one file.
    - OS selects the matching slice for execution.

### Limitations

- **Forward Compatibility Only**: Works for new apps on old hardware (backwards-compatible builds).
- **No Reverse**: Old apps can't run on new hardware without recompilation.
- **Still Relies on Source Portability**: Developers must recompile and bundle variants.

**Whiteboard Visualization**: Top-right shows Apple's flow: (6502) → Motorola 68000 → PowerPC → Intel x86 → ARM (M-series) with "Universal Binary" branching to multiple targets.

**Key Insight**: This is a "clever trick" for user-friendliness but doesn't fully solve general portability.

---

## 3. Compilation in C#/.NET: Managed Code and Binary Portability

C#/.NET flips the model: Focus on generating **platform-agnostic** code, deferring machine code generation to runtime.

### Process Overview

For a C# project (.csproj file listing .cs sources):

1. **Single Compilation Step**: .NET build system (e.g., MSBuild or dotnet build) feeds **all** source files to the C# compiler (csc.exe).
    - No per-file compilation – treats the project holistically.
2. **Output**: An **assembly** containing:
    - **Common Intermediate Language (CIL/IL)**: Platform-independent bytecode (like assembly but abstract).
    - **Metadata**: Descriptions of types, methods, signatures (more on this later).

- Assemblies end in .dll (libraries) or .exe (executables, though modern .NET often uses .dll + loader).

### Execution: The CLR Virtual Machine

- **No Native Executable**: IL isn't directly runnable.
- **Common Language Runtime (CLR)**: .NET's virtual machine.
    - Loads the assembly.
    - **Just-In-Time (JIT) Compiler**: Converts IL to machine code **at runtime**, per-method (not whole-program).
        - Compiles Main() first, then lazily compiles called methods.
        - Stretches startup time across execution.
- **Binary Portability**: Same assembly runs anywhere with CLR (Windows, Linux, macOS; x86, ARM, etc.).
    - JIT generates target-specific machine code in memory.

|Step|Input|Tool|Output|Platform Dependency|
|---|---|---|---|---|
|Compile|.cs files + .csproj|C# Compiler (csc)|Assembly (.dll/.exe with IL + Metadata)|No (IL is abstract)|
|Runtime|Assembly|CLR + JIT|In-Memory Machine Code|JIT handles this|

**Whiteboard Visualization**: Middle shows C# flow: Source (.cs) → C# Compiler (csc) → Executable Assembly (.dll) → CLR (JIT) → Machine Code.

### Comparison to Other Languages

- **Java**: Similar – compiles to **bytecode**, runs on **JVM** (Java Virtual Machine).
- **Visual Basic (Pre-.NET)**: Compiled to IL-like intermediate code, ran on VB runtime.
- **C#/.NET**: Builds on these; CLR is the "virtual machine" enabling **managed code** (requires VM) vs. **unmanaged/native code** (direct machine code, like C++).

**Whiteboard Visualization**: Branches to Java (Source → Java Compiler → JVM → Bytecode → JIT → Machine Code) and VB.NET (.NET CIL).

---

## 4. Assemblies: Modularity and References

To address recompilation bottlenecks (e.g., changing one file requires full rebuild):

- Split projects into multiple assemblies (e.g., one executable + libraries).

### Executable vs. Library Assemblies

- **Executable Assembly**: Has an **entry point** (e.g., Main() method) – runnable.
- **Library Assembly**: No entry point – reusable code (always .dll).
- **Building**: dotnet build on separate .csproj files → separate assemblies.

### Cross-Assembly References

- **Problem**: How does one assembly use types/methods from another?
- **Solution**:
    - Compile with **references** (e.g., /reference:Other.dll or `<`Reference`>` in .csproj).
    - **Metadata** in each assembly describes public types/methods (signatures, visibility, params/returns).
    - Compiler checks metadata for validity (e.g., type compatibility).
    - At runtime, CLR loads referenced assemblies via metadata links.
- **C++ Analogy**: Header files (.hpp) declare interfaces; linker resolves. .NET's metadata automates this (no manual headers).

**Metadata Details**:

- **Types**: Class/struct names, visibility (public/private).
- **Methods**: Name, static/instance, params (types/names), return type.
- **IL Boundaries**: Explicit (unlike opaque machine code).
- Enables: Compile-time checks, runtime reflection, named arguments (e.g., M(y: 2, x: 1)).

**Example**:
```C#
// In Assembly A
B.M(1, 2.0f);  // Compiler checks B.M in Assembly B's metadata: public static bool M(int x, float y)
```

- IL: `call [B.dll] B::M(int32, float32)` + loads (e.g., `ldc.i4.1` for  int 1, `ldc.r4 2.0` for float 2.0f).

**Whiteboard Visualization**: Bottom shows multi-project flow: `Project 1 (.csproj)` → `C# Compiler` → `Executable Assembly (.dll)` (with entry point); `Project 2` → `Library Assembly (.dll)`; references via `/r:Library.dll`.

---

## 5. Optimizations and Trade-offs

### JIT Advantages

- **Lazy Compilation**: Per-method, on-demand → Faster perceived startup.
- **Platform-Specific**: JIT knows hardware (e.g., x64 vs. ARM).
- **Tiered Compilation** (.NET 6+): Quick "cold" JIT → Background "hot" re-JIT for frequent methods → Near-C++ performance.
- **Runtime Benefits**: Existing apps auto-upgrade with new .NET (better JIT = faster runs without rebuild).

### Drawbacks

- **Startup Time**: JIT overhead (seconds for large apps). Mitigated by lazy + tiered.
- **No Whole-Program Optimization**: JIT can't see full app at compile-time.
- **Solution: Ahead-of-Time (AOT) Compilation** (.NET 7+):
    - Explicit flag: Compile IL to machine code **at build time**.
    - Pros: Zero runtime JIT, aggressive opts.
    - Cons: Loses runtime features (e.g., dynamic code gen); platform-specific binary.

### Multi-Language Interop

- All languages (C#, F#, VB.NET) → IL + Metadata.
- CLR ignores original language → Seamless calls.
- Compilers focus on language features; JIT handles opts.

**Whiteboard Visualization**: Notes on "Optimizations" branch from JIT, with AOT alternative.

---

## 6. Tools for Inspection and Debugging

### Building and Running

- **.NET CLI** (dotnet tool):
    - dotnet build: Compiles project.
    - dotnet run: Builds + executes.
    - dotnet test: Runs tests (e.g., xUnit).
    - dotnet <assembly.dll>: Loads CLR + runs (cross-platform).
- **Output on Windows**: .dll (IL) + .exe loader (native stub to start CLR).
- **Cross-Platform**: .dll is portable; use dotnet on Linux/macOS.

### Inspecting Assemblies

- **ILDASM** (.NET SDK tool): Disassembles IL + dumps metadata.
    - Example: ildasm MyApp.dll → Shows types, methods, IL bytecode (e.g., ldarg.0, call, ret).
    - Reveals: Signatures (e.g., int32 Main(string, int32)), calls (e.g., call [mscorlib] [System.Console]::WriteLine).
- **ILSpy** (Open-source Decompiler): Reverses IL to C#-like code.
    - Uses metadata + **PDB** (debug info file) for variable names/line mappings.
    - Example: Decompiles int num = x + y; back to similar C# (but reorders fields; loses comments).
    - PDB: Generated by default; enables stepping in Visual Studio.

**Demo Insights**:

- Strings like "Hello" appear in UTF-16.
- IL is compact (e.g., call = 5 bytes referencing metadata tables).
- Decompilation works well due to minimal C# compiler opts (preserves patterns for JIT).

**Whiteboard Visualization**: Right side shows decompilation flow: Assembly (.dll) → ILSpy → Decompiled C#.

---

## 7. Key Advantages of .NET Approach

|Aspect|C++|C#/.NET|
|---|---|---|
|**Portability**|Source-only; rebuild per platform|Binary (IL + CLR)|
|**Build Speed**|Incremental (per-file)|Full rebuild; mitigate with multi-assemblies|
|**Optimizations**|Compile-time, aggressive|Runtime JIT; auto-upgrades; tiered for speed|
|**Interop**|Manual headers/linker|Metadata-driven; multi-language|
|**Debugging**|Hard (optimized binaries)|Easy (unoptimized IL + PDB)|
|**Startup**|Instant (native)|Delayed (JIT); AOT fixes|

- **User Wins**: No platform guessing; apps improve over time.
- **Developer Wins**: Simpler compilers; focus on features.

## Conclusion and Teasers

This model enables .NET's strengths: Portability without recompiles, managed execution for safety, and ecosystem growth. Future lectures will explore JIT opts, advanced features relying on runtime code gen, and summer semester topics (e.g., .NET libraries).

**Lecture Runtime**: ~1 hour. References: Principles of Computer Systems (for IL/machine code basics). Practice: Use dotnet new console and inspect with ILSpy.