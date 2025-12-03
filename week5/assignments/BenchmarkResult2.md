# Dictionary Increment Benchmark Report

## Goal
Compare the performance of three different data structures:  
`Dictionary<string, int>` then sort, `SortedList<string, int>`, and `SortedDictionary<string, int>`.

---

## 2. Benchmarking Code

The benchmark generates 100,000 random words with many duplicates.
Each data structure is tested by inserting all words and then (if needed) performing a final alphabetical sort.

SortedList maintains sorted order on every insertion.

SortedDictionary maintains sorted order using a red–black tree.

Dictionary + Sort inserts in O(1) average time and sorts once at the end.

---
## 3. Benchmark Results
```

BenchmarkDotNet v0.15.6, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 9.0.9 (9.0.9, 9.0.925.41916), Arm64 RyuJIT armv8.0-a


```
| Method                | Mean      | Error    | StdDev   | Rank | Gen0     | Gen1     | Gen2     | Allocated  |
|---------------------- |----------:|---------:|---------:|-----:|---------:|---------:|---------:|-----------:|
| UseDictionaryThenSort |  15.36 ms | 0.036 ms | 0.034 ms |    1 | 703.1250 | 656.2500 | 656.2500 | 2769.04 KB |
| UseSortedList         | 127.63 ms | 1.208 ms | 1.071 ms |    2 |        - |        - |        - |  768.67 KB |
| UseSortedDictionary   | 127.91 ms | 1.204 ms | 1.068 ms |    2 |        - |        - |        - | 1087.35 KB |

---

## 4. Interpretation of Results
### 4.1 Dictionary + Sort (Fastest)

Result: ~15 ms (Rank 1)

This is the fastest method by a large margin.

Reasons:

Insertions into Dictionary are O(1) average cost.

Sorting happens once using a highly optimized O(n log n) sort.

**This avoids repeated reordering during insertion.**

Drawbacks:

High number of GC collections (Gen0/Gen1/Gen2), because LINQ's
OrderBy(...).ToList() creates many temporary objects.

Still significantly faster despite GC activity.

Conclusion: Best performance due to minimal insertion cost and a single final sort.

### 4.2 SortedList (Slow)

Result: ~127 ms (Rank 2)

SortedList stores elements in a contiguous array, so:

Finding the insertion point is O(log n)

Shifting the array to make space is O(n)

This results in near O(n^2) behavior with large input sizes.

No GC collections were triggered during the benchmark, because total allocations are small and stable.

Conclusion: Consistently slow due to expensive insertion operations.

### 4.3 SortedDictionary (Slow)

Result: ~127 ms (Rank 2)

A SortedDictionary is a red–black tree, so:

Insertions are O(log n)

No array shifting

Every insertion still involves tree traversal and rebalancing

Although algorithmically better than SortedList, in practice:

Pointer-heavy tree operations are slower than array operations

It performs similarly to SortedList for this workload

Also triggers no GC collections because allocations are modest and incremental.

Conclusion: More predictable than SortedList but still significantly slower than Dictionary + Sort.


## 6. Overall Conclusion
Best Choice: Dictionary<string, int> + Sort Afterwards

It is:

faster than SortedList or SortedDictionary

Algorithmically optimal for this specific problem:

Fast insertions

Single final sort
