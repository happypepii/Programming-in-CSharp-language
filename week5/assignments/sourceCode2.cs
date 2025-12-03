using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace WordFrequencyBenchmarks
{

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<AlphabeticallySortedBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class AlphabeticallySortedBenchmarks
    {
        private List<string> _words;
        private const int WordCount = 100000;

        [GlobalSetup]
        public void Setup()
        {
            var rnd = new Random(42);

            _words = new List<string>(WordCount);

            // Generate random words
            for (int i = 0; i < WordCount; i++)
            {
                string w = "word" + rnd.Next(0, 20000); // generating the duplicates
                _words.Add(w);
            }
        }

        // =============== OPTION 1: SortedList ===================
        [Benchmark]
        public SortedList<string, int> UseSortedList()
        {
            var list = new SortedList<string, int>();

            foreach (string w in _words)
            {
                if (list.TryGetValue(w, out int value))
                    list[w] = value + 1;
                else
                    list[w] = 1;   // sorted insert = expensive
            }

            return list;
        }

        // =============== OPTION 2: SortedDictionary =============
        [Benchmark]
        public SortedDictionary<string, int> UseSortedDictionary()
        {
            var dict = new SortedDictionary<string, int>();

            foreach (string w in _words)
            {
                if (dict.TryGetValue(w, out int value))
                    dict[w] = value + 1;
                else
                    dict[w] = 1;   // tree insertion = log(n)
            }

            return dict;
        }

        // =============== OPTION 3: Dictionary + sort afterwards =========
        [Benchmark]
        public List<KeyValuePair<string, int>> UseDictionaryThenSort()
        {
            var dict = new Dictionary<string, int>();

            foreach (string w in _words)
            {
                if (dict.TryGetValue(w, out int value))
                    dict[w] = value + 1;
                else
                    dict[w] = 1;   // O(1)
            }

            // Sorting happens once at the end
            return dict.OrderBy(kvp => kvp.Key).ToList();
        }
    }
}