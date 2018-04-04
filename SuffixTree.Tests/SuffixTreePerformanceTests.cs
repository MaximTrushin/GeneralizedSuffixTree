using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneralizedSuffixTree.Tests.TestCaseGeneration;
using GeneralizedSuffixTree.Tests.Ukkonen;
using Gma.DataStructures.StringSearch;
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
            var ut = new UkkonenTrie<string>();

            Mesure(trie, ut, randomText, lookupWords, out var buildUp, out var avgLookUp);
            Console.WriteLine("Build-up time: {0}", buildUp);
            Console.WriteLine("Avg. look-up time: {0}", avgLookUp);
        }

        private void Mesure(GeneralizedSuffixTree<string> tree, UkkonenTrie<string> ukkonenTrie,
            IEnumerable<string> randomText, IEnumerable<string> lookupWords,
            out TimeSpan buildUp, out TimeSpan avgLookUp)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            foreach (string word in randomText)
            {
                tree.AddWord(word, word);
                ukkonenTrie.Add(word, word);
            }
            stopwatch.Stop();
            int internalNodes;
            int leaves;
            int edges;
            int sLinks;
            TreeHelper.CountNodes(tree, out leaves, out internalNodes, out edges, out sLinks);
            int internalNodes2;
            int leaves2;
            int edges2;
            int sLinks2;
            UTreeHelper.CountNodes(ukkonenTrie, out leaves2, out internalNodes2, out edges2, out sLinks2);



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
