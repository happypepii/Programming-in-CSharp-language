# Dictionary Increment Benchmark Report

## Goal
Compare the performance of three different dictionary increment methods:  
`IncrementV1`, `IncrementV2`, and `IncrementV3`.

---

## Methods

### **1. IncrementV1 — Direct indexer (`dict[key]`)**
- Implements `dict[word] = dict[word] + 1`
- If the key does **not** exist, the indexer throws an exception  
- In this benchmark, the key **always exists**, so no exception is thrown
- Performs **one dictionary lookup**

---

### **2. IncrementV2 — `ContainsKey` + indexer**
- First checks `dict.ContainsKey(word)`
- If true → reads `dict[word]` again
- This results in **two dictionary lookups**

---

### **3. IncrementV3 — `TryGetValue`**
- Calls `dict.TryGetValue(word, out value)`  
- If key doesn’t exist, `value` defaults to 0
- Only **one dictionary lookup**
- No exceptions and no need for branching logic

---

## Benchmark Environment
```

BenchmarkDotNet v0.15.6, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a


```


| Method      | Mean     | Error    | StdDev   |
|------------ |---------:|---------:|---------:|
| IncrementV1 | 15.05 ns | 0.225 ns | 0.199 ns |
| IncrementV2 | 20.96 ns | 0.276 ns | 0.230 ns |
| IncrementV3 | 14.10 ns | 0.033 ns | 0.026 ns |

---

## Results Summary

### **IncrementV3 is the fastest (~14 ns)**
Because:
- Only **one** dictionary lookup  
- No exceptions  
- Short and predictable code path  
- `TryGetValue` is highly optimized in .NET

### **IncrementV2 is the slowest (~21 ns)**
Because:
- `ContainsKey` = **1 lookup**  
- `dict[key]` = **2 lookup**  
- Twice the hashing and dictionary search work

### **IncrementV1 is slightly slower than V3**
Because:
- Also only one lookup
- But still slightly slower than `TryGetValue` due to internal optimizations
- No exceptions were triggered in this benchmark

---

## Conclusion
`TryGetValue` (IncrementV3) is the fastest approach for incrementing dictionary values because it uses a **single lookup**, avoids exceptions, and has a straightforward execution path. `IncrementV2` is the slowest due to performing **two separate lookups**, while `IncrementV1` performs similarly to V3 as long as no exceptions occur.



 
