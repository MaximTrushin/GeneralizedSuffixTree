using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
               // ukkonenTrie.Add(word, word);
            }

            stopwatch.Stop();
            TreeHelper.CountNodes(tree, out var leaves, out var internalNodes, out var edges, out var sLinks);

            //UTreeHelper.CountNodes(ukkonenTrie, out var leaves2, out var internalNodes2, out var edges2,
            //    out var sLinks2);

            buildUp = stopwatch.Elapsed;

            int lookupCount = 0;
            stopwatch.Reset();

            foreach (string lookupWord in lookupWords)
            {
                lookupCount++;
                stopwatch.Start();
                var r1 = tree.Retrieve(lookupWord);
                //var r2 = ukkonenTrie.Retrieve(lookupWord);
                stopwatch.Stop();
                //var c1 = r1.Count();
                //var c2 = r2.Count();
                //Assert.AreEqual(c1, c2);
                //if (c1 != c2)
                //{
                //    var sb = new StringBuilder();
                //    foreach (string word in randomText.ToArray())
                //    {
                //        sb.AppendLine(word);
                //    }

                //    var sb2 = new StringBuilder();
                //    foreach (string l in lookupWords)
                //    {
                //        sb2.AppendLine(l);
                //    }
                //}
            }

            avgLookUp = new TimeSpan(stopwatch.ElapsedTicks / lookupCount);
        }



    }
}
