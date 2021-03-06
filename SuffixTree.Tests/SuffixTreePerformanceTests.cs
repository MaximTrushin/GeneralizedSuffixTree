﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneralizedSuffixTree.Tests.TestCaseGeneration;
using NUnit.Framework;
using SuffixTree;

namespace GeneralizedSuffixTree.Tests
{
    [TestFixture]
    [Explicit]
    public class SuffixTreePerformanceTests
    {
        private string[] _Vocabualry;

        [OneTimeSetUp]
        public void SetUp()
        {
            _Vocabualry = NonsenseGeneration.GetVocabulary();
        }


        [TestCase(10000, 1000)]
        [TestCase(100000, 1000)]
        [TestCase(1000000, 1000)]
        [TestCase(10000000, 1000)]
        public void TestX(int wordCount, int lookupCount)
        {
            string[] randomText = NonsenseGeneration.GetRandomWords(_Vocabualry, wordCount).ToArray();
            string[] lookupWords = NonsenseGeneration.GetRandomWords(_Vocabualry, lookupCount).ToArray();
            var trie = new GeneralizedSuffixTree<string>(1);

            Mesure(trie, randomText, lookupWords, out var buildUp, out var avgLookUp);
            Console.WriteLine("Build-up time: {0}", buildUp);
            Console.WriteLine("Avg. look-up time: {0}", avgLookUp);

        }

        private void Mesure(GeneralizedSuffixTree<string> tree, IEnumerable<string> randomText, IEnumerable<string> lookupWords,
            out TimeSpan buildUp, out TimeSpan avgLookUp)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            foreach (string word in randomText)
            {
                tree.AddWord(word, word);
            }
            stopwatch.Stop();

            buildUp = stopwatch.Elapsed;

            int lookupCount = 0;
            stopwatch.Reset();

            foreach (string lookupWord in lookupWords)
            {
                lookupCount++;
                stopwatch.Start();
                tree.Retrieve(lookupWord);
                stopwatch.Stop();
            }
            avgLookUp = new TimeSpan(stopwatch.ElapsedTicks / lookupCount);
        }
    }
}
