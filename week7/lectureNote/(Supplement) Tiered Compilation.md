| Phase  | Name          | When it happens                                                                            | What the JIT actually does                                                                                                                                                | Goal                                                      |
| ------ | ------------- | ------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| Tier 0 | Quick & Dirty | The very first time a method is executed                                                   | Generates code very fast No heavy optimizations Inserts tiny counters/profiling code everywhere                                                                           | Get the program running as fast as possible               |
| Tier 1 | Optimizing    | After the method has been called many times (usually ~30–100 times, depends on the method) | Re-compiles the same method from scratch Now does aggressive optimizations (inlining, devirtualization, loop unrolling, etc.) Uses the profiling data collected in Tier 0 | Produce the absolute fastest machine code for hot methods |
## Visual Timeline (exactly what he drew on the board)
```text
Method first called
       │
       ▼
   Tier 0 code generated (fast, instrumented)
       │
       ▼
   Runs many times → counters increase
       │
       ▼
   Counter exceeds threshold → JIT queue
       │
       ▼
   Tier 1 code generated (slow to compile, but extremely fast to run)
       │
       ▼
   From now on, every call uses the Tier 1 version
```

## What data does Tier 0 collect? (this is the key part)
During Tier 0 execution, the JIT inserts tiny invisible instrumentation that records:
1. How many times this exact method was called
2. What concrete types appeared in receiver parameters (this) and other parameters → this is the Dynamic PGO data he talked about today
3. Which branches were taken most often
4. Which call sites were monomorphic (always the same target type) vs polymorphic

## Real Example from Today’s Lecture
```csharp
A obj = new C();       // sometimes new A(), sometimes new B(), sometimes new C()
for (int i = 0; i < 1_000_000_000; i++)
    obj.V();           // virtual method
```

What happens:
1. First ~50 calls → Tier 0 code (slowish, full v-table dispatch every time)
2. JIT sees: “99.9 % of the time obj is actually a C”
3. Triggers Tier 1 recompilation
4. Tier 1 generates roughly this machine code:
	```csharp
	; Hot path (99.9 % of executions)
    cmp [obj], C_TypeHandle          ; fast type check
    jne ColdPath
    call C.V() directly              ; devirtualized!
    ; + the body of C.V() is inlined here
    ret

  ColdPath:
    ; normal slow v-table lookup (rarely executed)
	```
	