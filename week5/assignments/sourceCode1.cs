using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace WordFrequencyBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<WordIncrementBenchmarks>();
        }
    }

    public class WordIncrementBenchmarks
    {
        private Dictionary<string, int> _dict;
        private string _word;

        [GlobalSetup]
        public void Setup()
        {
            _dict = new Dictionary<string, int>();
            _word = "hello";
        }

        // ===== Version 1: try/catch =====
        private static void IncrementWordCount_V1(IDictionary<string, int> dict, string word)
        {
            try
            {
                dict[word]++;
            }
            catch (KeyNotFoundException)
            {
                dict[word] = 1;
            }
        }

        // ===== Version 2: ContainsKey =====
        private static void IncrementWordCount_V2(IDictionary<string, int> dict, string word)
        {
            if (dict.ContainsKey(word))
            {
                dict[word]++;
            }
            else
            {
                dict[word] = 1;
            }
        }

        // ===== Version 3: TryGetValue =====
        private static void IncrementWordCount_V3(IDictionary<string, int> dict, string word)
        {
            _ = dict.TryGetValue(word, out int value);
            dict[word] = value + 1;
        }

        // ===== Benchmarks =====
        [Benchmark]
        public void IncrementV1() => IncrementWordCount_V1(_dict, _word);

        [Benchmark]
        public void IncrementV2() => IncrementWordCount_V2(_dict, _word);

        [Benchmark]
        public void IncrementV3() => IncrementWordCount_V3(_dict, _word);
    }
}